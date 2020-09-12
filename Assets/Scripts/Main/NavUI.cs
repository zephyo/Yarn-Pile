using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Class to handle nav UI
/// If any ClosableUI is triggered to open, close the rest
/// Assumed to live for lifetime. 
/// If doesn't, make sure to remove listeners on destroy!
/// </summary>
public class NavUI : MonoBehaviour
{
    [SerializeField]
    private ClosableUI[] Uis;

    private ClosableUI current;

    private void OnValidate()
    {
        Uis = GetComponentsInChildren<ClosableUI>();
    }

    private void Awake()
    {
        foreach (ClosableUI ui in Uis)
        {
            ui.button.onClick.AddListener(() => TryCloseCurrent(ui));
        }
    }

    private void TryCloseCurrent(ClosableUI trigger)
    {
        if (current == trigger)
        {
            return;
        }
        current?.Close();
        current = trigger;
    }

}
