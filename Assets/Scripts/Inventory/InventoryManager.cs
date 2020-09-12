using System;
using System.Collections.Generic;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;
using Yarn.Unity;
public class InventoryManager : MonoBehaviour
{
    const int MAX_ITEMS = 6;
    // Public events
    public delegate void ItemDataHandler(ItemData data);
    public event ItemDataHandler OnAddItem;
    public event ItemDataHandler OnRemoveItem;

    // Inventory name to inventory scriptable object
    [System.Serializable]
    class ItemDictionary : SerializableDictionaryBase<ItemName, ItemData> { }

    [SerializeField]
    private ItemDictionary itemDictionary = new ItemDictionary();

    // Item to quantity
    private Dictionary<ItemName, int> inventory = new Dictionary<ItemName, int>();

    public void SetInventory(Dictionary<ItemName, int> value)
    {
        foreach (KeyValuePair<ItemName, int> pair in value)
        {
            Take(pair.Key, pair.Value, true);
        }
    }

    private void Awake()
    {
#if UNITY_EDITOR
        foreach (KeyValuePair<ItemName, ItemData> entry in itemDictionary)
        {
            if (entry.Key != entry.Value.item)
            {
                Debug.LogError($"{entry.Key} does not match the value!");
                return;
            }
        }
#endif
    }

    private void Start()
    {
        DialogueRunner runner = MainSingleton.Instance.dialogueRunner;
        runner.AddCommandHandler<string, int>("Take", Take);
        runner.AddCommandHandler<string, int>("Drop", Drop);
        runner.AddFunction("has", delegate (string s)
       {
           ItemName item = Parse(s);
           return inventory.ContainsKey(item) && inventory[item] > 0;
       });

    }

    /// <summary>
    /// Parse given string into ItemName enum  type.
    /// Will throw ArgumentException if given invalid string
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    ItemName Parse(string item) => (ItemName)Enum.Parse(typeof(ItemName), item);

    void Take(string strItem, int quantity)
    {
        ItemName item = Parse(strItem);
        Take(item, quantity);
    }

    private void Take(ItemName item, int quantity, bool silent = false)
    {
        if (quantity < 1)
        {
            Debug.LogWarning($"{quantity} is less than 1");
            return;
        }
        if (inventory.Count >= MAX_ITEMS)
        {
            MainSingleton.Instance.notification.Notify("Inventory full");
            return;
        }
        ItemData data = itemDictionary[item];
        if (inventory.ContainsKey(item))
        {
            inventory[item] += quantity;
        }
        else
        {
            inventory.Add(item, quantity);
        }
        OnAddItem?.Invoke(data);
        if (!silent)
        { MainSingleton.Instance.notification.Notify($"Added {item.ToString()} to inventory"); }
    }

    void Drop(string strItem, int quantity)
    {
        ItemName item = Parse(strItem);

        if (quantity < 1)
        {
            Debug.LogWarning($"{quantity} is less than 1");
            return;
        }
        if (!inventory.ContainsKey(item))
        {
            Debug.LogWarning($"I don't have {item.ToString()}");
            return;
        }
        ItemData data = itemDictionary[item];
        inventory[item]--;
        OnRemoveItem?.Invoke(data);
        MainSingleton.Instance.notification.Notify($"Dropped {item.ToString()} from inventory");
    }
}
