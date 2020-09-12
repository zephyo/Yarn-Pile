using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Canvas))]
public class GameUI : MonoBehaviour
{
    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvas.enabled = false;

        SceneManager.sceneLoaded += CheckForEnable;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= CheckForEnable;
    }

    private void CheckForEnable(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Main" && scene.name != "Start Menu")
        {
            canvas.enabled = true;
        }
    }
}
