using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Pause : MonoBehaviour
{
    // Register callback to player input - if escape, then show pause
    void Awake()
    {
        MainSingleton.Instance.input.onActionTriggered += TriggerPauseMenu;
    }

    private void TriggerPauseMenu(InputAction.CallbackContext context)
    {
        if (context.canceled) return;
        if (context.action.name == "Cancel" && context.performed)
        {
            MainSingleton.Instance.ViewPause();
        }
    }
}
