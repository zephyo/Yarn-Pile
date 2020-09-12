using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Yarn.Unity;
using Yarn;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using UnityEngine.SceneManagement;
#if UNITY_WEBGL
using System.Runtime.InteropServices;
#endif


/// <summary>
/// Extended implementation of InMemoryVariableStorage.
/// Saves to Application.persistentDataPath; loads on Awake.
/// In script execution manager, set this to execute early, 
/// so that data is loaded before anything else.
/// 
/// All declared yarn variables in .yarn should be in
/// defaultVariables here, so other information like min/max
/// can be declared.
/// </summary>
public class CustomStorage : InMemoryVariableStorage
{

    [HideInInspector]
    public HashSet<string> visitedNodes = new HashSet<string>();

    public delegate void YarnValueHandler(string name, Value value);
    public event YarnValueHandler OnSetValue;

    const string SAVE_PATH = "/gamesave.dat";
    private SaveData saveData;
    protected override void Awake()
    {
        SaveData data;
        // Load data, then set the loaded data
        // Assume on start, there's an empty inventory
        if (TryLoadData(out data))
        {
            UseSaveData(data);
            saveData = data;
        }
        else
        {
            saveData = new SaveData();
        }
        base.Awake();
    }

    void Start()
    {
        // Set listeners after populating store with either saved data or default data
        SetListeners();
#if UNITY_EDITOR
        foreach (CharacterName n in Enum.GetValues(typeof(CharacterName)))
        {
            if (!IsValidVariable(n.ToString()))
            {
                Debug.LogError($"ERROR! {n.ToString()} is a CharacterName but is not a yarn variable field." +
                $"Set {n.ToString()} in defaultVariables.");
            }
        }
#endif
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void SetListeners()
    {
        // Set inventory listeners
        // On add, boost stats
        InventoryManager inventoryManager = MainSingleton.Instance.inventoryManager;

        inventoryManager.OnAddItem += (ItemData data) =>
        {
            foreach (KeyValuePair<string, int> pair in data.yarnValues)
            {
                SetValue(pair.Key.YarnFormat(), new Value(pair.Value));
            }
        };
        // On remove, decrement stats
        inventoryManager.OnRemoveItem += (ItemData data) =>
        {
            foreach (KeyValuePair<string, int> pair in data.yarnValues)
            {
                SetValue(pair.Key.YarnFormat(), new Value(-pair.Value));
            }
        };

        // Save data on every node
        // TODO: If too performance heavy, then refactor
        DialogueRunner runner = MainSingleton.Instance.dialogueRunner;
        runner.onDialogueComplete.AddListener(SaveData);

        SaveDataListeners();
    }

#if UNITY_EDITOR
    /// <summary>
    /// Check if valid variable name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool IsValidVariable(string name)
    {
        foreach (KeyValuePair<string, DefaultVariable> pair in defaultVariables)
        {
            if (pair.Key == name)
            {
                return true;
            }

        }
        return false;
    }
#endif

    #region SAVE/LOAD/DELETE
    private void UseSaveData(SaveData data)
    {
        MainSingleton.Instance.dialogueRunner.startNode = data.currentScene;

        visitedNodes = new HashSet<string>(data.visitedNodesList);
        // Load variables into defaultVariables so the latest save is used to set the default
        foreach (KeyValuePair<string, string> pair in data.variables)
        {
            defaultVariables[pair.Key].value = pair.Value;
        }
    }

    /// <summary>
    /// Call this right before dialogueRunner starts dialogue for the first time
    /// Not necessarily needed to be called on Awake, e.g. if there's a menu screen
    /// </summary>
    public void UseSaveDataOnPlay()
    {
        if (this.saveData.inventory.Count > 0)
        {  // Load saved inventory
            FindObjectOfType<InventoryManager>().SetInventory(this.saveData.inventory);
        }

        if (this.saveData.globalTime != null)
        {
            // Load saved time
            FindObjectOfType<GlobalTime>().SaveTime(this.saveData.globalTime);
        }

        SaveDataListenersOnPlay();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // if not the ui screen scenes
        if (scene.name != "Main" && scene.name != "Start Menu")
        {
            saveData.currentScene = scene.name;
        }
    }

    // Set listeners so that if relevant fields elsewhere update, our saveData updates as well
    private void SaveDataListeners()
    {
        OnSetValue += (str, value) => saveData.variables[str.Substring(1)] = value.AsString;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// Initialize these listeners after calling UseSaveDataOnPlay
    /// Or else InventoryManager throws InvalidOperationException 
    /// because saveData.inventory will be modified during SetInventory
    /// so enumeration operation  cannot execute
    /// </summary>
    private void SaveDataListenersOnPlay()
    {
        FindObjectOfType<GlobalTime>().OnSetTime += (time) => saveData.globalTime = time;

        // On add, set save data accordingly
        InventoryManager inventoryManager = MainSingleton.Instance.inventoryManager;
        inventoryManager.OnAddItem += (i) =>
          {
              ItemName item = i.item;
              if (!saveData.inventory.ContainsKey(item))
              {
                  saveData.inventory.Add(item, 1);
              }
              else
              {
                  saveData.inventory[item]++;
              }
          };
        // On remove, set save data accordingly
        inventoryManager.OnRemoveItem += (i) =>
        {
            ItemName item = i.item;
            if (saveData.inventory.ContainsKey(item))
            {
                saveData.inventory[item]--;
            }
        };
    }

    private SaveData PrepareSaveData()
    {
        saveData.visitedNodesList = new List<string>(visitedNodes);
        return saveData;
    }

    public bool TryLoadData(out SaveData data)
    {
        if (File.Exists(Application.persistentDataPath + SAVE_PATH))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + SAVE_PATH, FileMode.Open);
            data = (SaveData)bf.Deserialize(file);
            file.Close();
#if UNITY_EDITOR
            DebugData("TryLoadData", data);
#endif
            return true;
        }
        data = null;
        return false;
    }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void SyncFiles();
#endif

    public void SaveData()
    {
        SaveData data = PrepareSaveData();

#if UNITY_EDITOR
        DebugData("SaveData", data);
#endif

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + SAVE_PATH);
        bf.Serialize(file, data);
        file.Close();

#if UNITY_WEBGL
        SyncFiles();
#endif
    }

    public void DeleteSaveData()
    {
        if (File.Exists(Application.persistentDataPath + SAVE_PATH))
        {
            File.Delete(Application.persistentDataPath + SAVE_PATH);
        }
    }

#if UNITY_EDITOR
    private void DebugData(string method, SaveData data)
    {
        // Debugging information
        Debug.Log($"<color=cyan>{method}: Printing SaveData</color> at {Application.persistentDataPath + SAVE_PATH}");
        foreach (string node in visitedNodes)
        {
            Debug.Log($"Visited node: {node}");
        }
        foreach (KeyValuePair<string, string> entry in data.variables)
        {
            Debug.Log($"Yarn key value: {entry.Key}: {entry.Value}");
        }
        foreach (KeyValuePair<ItemName, int> entry in data.inventory)
        {
            Debug.Log($"Yarn inventory: {entry.Key}: {entry.Value}");
        }
        Debug.Log($"globalTime: {data.globalTime}" +
        $"\ncurrentScene: {data.currentScene}");

        Debug.Log($"<color=cyan>{method}: Done printing</color>");
    }
#endif
    #endregion

    public override void SetValue(string variableName, Value value)
    {
#if UNITY_EDITOR
        Debug.Log($"Set {variableName} as {value.ToString()}");
#endif
        base.SetValue(variableName, value);
        OnSetValue?.Invoke(variableName, value);
    }
}
