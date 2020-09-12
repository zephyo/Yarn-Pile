using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Defines dialogue UI.
/// Handles all related UI logic like animations.
/// 
/// Extended by DialogueOptionsGroup
/// </summary>

public class DialogueGroup : MonoBehaviour
{

    public TweenText tweenText;

    [SerializeField]
    protected Animator animator;

    protected readonly int openHash = Animator.StringToHash("Open");

    protected CustomDialogueUI dialogueUI;

    protected virtual void OnValidate()
    {
        animator = GetComponent<Animator>();
        tweenText = transform.Find("DialogPanel").Find("Text").GetComponent<TweenText>();
    }

    protected bool IsOn() => animator.GetBool(openHash) == true;

    public virtual void TurnOn(CustomDialogueUI ui)
    {
        // Ensure gameobject is on, and we set dialogueUI field
        gameObject.SetActive(true);

        if (IsOn()) return;

        MainSingleton.Instance.input.onActionTriggered += TriggerNextLine;
        ui.onLineStart = OnLineStart;
        ui.onTextFinishDisplaying += tweenText.KillShow;
        ui.onLineParse += tweenText.Clear;
        this.dialogueUI = ui;
    }

    public virtual void TurnOff()
    {
        MainSingleton.Instance.input.onActionTriggered -= TriggerNextLine;
        if (this.dialogueUI)
        {
            dialogueUI.onLineStart = OnLineStart;
            dialogueUI.onTextFinishDisplaying -= tweenText.KillShow;
            dialogueUI.onLineParse -= tweenText.Clear;
            this.dialogueUI = null;
        }
        Hide();
    }

    public void Hide()
    {
        animator.SetBool(openHash, false);
    }

    // Listener to DialogueUI's onLineStart
    protected virtual Sequence OnLineStart(string text, float speed)
    {
        return OnLineStart(text, speed, 0);
    }

    // helper function for DialogueOptionsGroup - same functionality as OnLineStart, but pass in a delay
    protected Sequence OnLineStart(string text, float speed, float delay = 0)
    {
        animator.SetBool(openHash, true);
        return tweenText.Show(text, speed, delay);
    }

    private void TriggerNextLine(InputAction.CallbackContext context)
    {
        // Check if cancelled
        if (context.canceled) return;

        // Check if over UI that isn't the dialogue UI
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Mouse.current.position.ReadValue();

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);

        foreach (var go in raycastResults)
        {
            if (go.gameObject.CompareTag("UI"))
            {
                return;
            }
        }

        // else, mark line as complete
        if (context.action.name == "Submit" && context.performed)
        {
            dialogueUI.MarkLineComplete();
        }
    }
}
