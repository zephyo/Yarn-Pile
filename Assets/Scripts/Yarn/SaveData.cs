using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    #region CURRENT POINT
    /// <summary>
    /// currentScene indicates the current scene name,
    /// as well as the Yarn node that will handle everything
    /// to set up this scene.
    /// Example: Office
    /// </summary>
    public string currentScene;
    public DateTime globalTime;
    public Dictionary<ItemName, int> inventory = new Dictionary<ItemName, int>();
    #endregion


    /// <summary>
    /// Save dictionary of key (string) to values (string)
    /// On loading the save data, replace the defaultVariables' values with the saved values
    /// Use above logic as a workaround around the fact that Yarn.Value isn't serializable
    /// </summary>
    /// <typeparam name="string"></typeparam>
    /// <typeparam name="string"></typeparam>
    /// <returns></returns>
    [SerializeField]
    public Dictionary<string, string> variables = new Dictionary<string, string>();

    /// <summary>
    /// Because we can't serialize hashsets, serialize visited nodes as a list,
    /// then load it into customstorage's visited nodes upon load
    /// </summary>
    /// <typeparam name="string"></typeparam>
    /// <returns></returns>
    public List<string> visitedNodesList = new List<string>();

}