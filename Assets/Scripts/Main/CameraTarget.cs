using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public struct TargetParameters
{
    public float minimize, minX, maxX, minY, maxY;
}

/// <summary>
/// Rotate on mouse move. -X is down. X is up. -Y is right; Y is left
/// </summary>
public class CameraTarget : MoveRotateMouse
{

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }

    public void SetBasePosition(Vector2 p)
    {
        basePosition = p;
        MoveMouse(Mouse.current.position.ReadValue());
    }

    #region SCENE LISTENERS

    /// <summary>
    /// On scene load, change the parameters
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        Dictionary<string, TargetParameters> dict = new Dictionary<string, TargetParameters>();
        dict["Start Menu"] = new TargetParameters
        {
            minimize = 0.01f,
            minX = -2,
            maxX = 2,
            minY = -2,
            maxY = 2
        };
        TargetParameters p;
        if (dict.TryGetValue(scene.name, out p))
        {
            minimize = p.minimize;
            minX = p.minX;
            maxX = p.maxX;
            minY = p.minY;
            maxY = p.maxY;
        }
    }
    #endregion
}
