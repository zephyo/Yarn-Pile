using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

/// <summary>
/// Control adding and unloading of additive scenes
/// </summary>
[RequireComponent(typeof(MainSingleton))]
public class SceneControl : MonoBehaviour
{
    [SerializeField]
    string startMenuScene = "Start Menu";
    [HideInInspector]
    public string currentScene = null;

    private void Start()
    {
        DialogueRunner runner = MainSingleton.Instance.dialogueRunner;
        runner.AddCommandHandler<string>("Load", (str) => StartCoroutine(LoadSceneAdditiveWait(str)));
        SceneManager.sceneLoaded += OnSceneLoad;
        // Load start menu
        LoadSceneAdditive(startMenuScene);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        currentScene = scene.name;
    }

    public void LoadSceneAdditive(string name)
    {
        if (name != currentScene)
        {
            if (!string.IsNullOrEmpty(currentScene))
            {
                SceneManager.UnloadSceneAsync(currentScene);
            }
            SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
        }
    }

    public IEnumerator LoadSceneAdditiveWait(string name)
    {
        LoadSceneAdditive(name);

        yield return new WaitUntil(() => currentScene == name);
    }

}
