using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Yarn.Unity;
using TMPro;
using DG.Tweening;
using UnityEngine.InputSystem;

public class DialogueOptionsGroup : DialogueGroup
{

    [SerializeField]
    private Button[] optionButtons;

    [SerializeField]
    private float OPTION_SELECT_ANIMATION_LENGTH;

    private float maxTime;

    private readonly int timerHash = Animator.StringToHash("Timed"), optionsHash = Animator.StringToHash("Options");
    private const float KEYBOARD_DELAY = 0.15f;

    private bool finishOptions = false;

    /// <summary>
    /// Did we show options just now?
    /// If true, and we're showing a dialogue line right after
    /// in this same dialogue group, then delay char tweener.
    /// Hack around the fact that if we show options, then
    /// show a dialogue line immediately after in same dialogue
    ///  group, then char tweener won't play the Show animation.
    /// </summary>
    private bool wasOptionsShown;

    protected override void OnValidate()
    {
        base.OnValidate();
        GameObject optionsPanel = transform.Find("OptionsPanel")?.gameObject;
        if (optionsPanel)
        {
            optionButtons = optionsPanel.transform.Find("Options").GetComponentsInChildren<Button>();
            RuntimeAnimatorController ac = optionButtons[0].GetComponent<Animator>().runtimeAnimatorController;
            for (int i = 0; i < ac.animationClips.Length; i++)
            {
                if (ac.animationClips[i].name == "Pressed")
                {
                    OPTION_SELECT_ANIMATION_LENGTH = ac.animationClips[i].length;
                    // Add transition time
                    OPTION_SELECT_ANIMATION_LENGTH += 1;
                    break;
                }
            }
        }
    }

    protected void Awake()
    {
        HideAllOptions();
        FindObjectOfType<DialogueTimer>().OnTimerChanged += SetMaxTime;
    }

    public override void TurnOn(CustomDialogueUI ui)
    {
        // The first thing we do is call parent class's turn on
        // will set dialogueUI field
        base.TurnOn(ui);
        ui.onOptionsStart += SetupOptions;
    }

    public override void TurnOff()
    {
        if (this.dialogueUI)
        {
            dialogueUI.onOptionsStart -= SetupOptions;
        }

        // reset wasOptionsShown
        wasOptionsShown = false;

        // The last thing we do is call parent class's turn off
        // Will nullify dialogueUI
        base.TurnOff();
    }

    /// <summary>
    /// If we just showed options, make sure the dialogue box is open.
    /// </summary>
    protected override Sequence OnLineStart(string text, float speed)
    {
        float delay = 0;
        if (wasOptionsShown)
        {
            delay = 1f;
            wasOptionsShown = false;
        }
        return base.OnLineStart(text, speed, delay);
    }

    private void SetupOptions(bool timed)
    {
        HighlightFirstChoice();
        wasOptionsShown = true;
        finishOptions = false;

        Hide();

        DoListenForKeyboard();

        if (timed)
        {
            RunTimer();
        }
        else
        {
            animator.SetBool(optionsHash, true);
        }
    }

    private void CleanupOptions()
    {
        MainSingleton.Instance.input.onActionTriggered -= ListenForKeyboard;
        finishOptions = true;
        animator.SetBool(optionsHash, false);
    }

    /// <summary>
    /// If the submit action is performed via anything other than mouse, 
    /// click the current selected button
    /// </summary>
    /// <param name="context"></param>
    private void ListenForKeyboard(InputAction.CallbackContext context)
    {
        // Check if cancelled
        if (context.canceled) return;

        // Don't allow early keyboard clicks in case we're still in the middle of showing options
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1 < KEYBOARD_DELAY * animator.speed)
        {
            return;
        }

        // else, click if current selected is an options button
        if (context.action.name == "Submit" && context.performed && context.control != Mouse.current.leftButton)
        {
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
            if (currentSelected?.CompareTag("Options") == true)
            {
                ClickButton(currentSelected);
            }
        }
    }

    private void SetMaxTime(float seconds)
    {
        maxTime = seconds;
    }

    public void DoListenForKeyboard()
    {
        StartCoroutine(RunListenForKeyboard());
    }

    // Delay before listening for keyboard so that animator can transition to next state
    IEnumerator RunListenForKeyboard()
    {
        yield return null;
        MainSingleton.Instance.input.onActionTriggered += ListenForKeyboard;
    }

    void RunTimer()
    {
        StartCoroutine(DoRunTimer());
    }

    IEnumerator DoRunTimer()
    {
        animator.speed = 1 / maxTime;
        animator.SetBool(timerHash, true);

        // Wait a frame to allow animator to transition
        yield return null;

        float i = 0;
        float len = animator.GetCurrentAnimatorStateInfo(0).length;
        var frame = new WaitForEndOfFrame();

        while (i < len)
        {
            i += Time.deltaTime;
            if (finishOptions == true)
            {
                break;
            }
            yield return frame;
        }

        // if timer ran out, select either the currently highlighted or first
        if (!finishOptions)
        {
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
            if (currentSelected?.CompareTag("Options") == true)
            {
                ClickButton(currentSelected);
            }
            else
            {
                ClickButton(optionButtons[0].gameObject);
            }
        }

        // clean up animator
        animator.speed = 1;
        animator.SetBool(timerHash, false);
    }

    private void ClickButton(GameObject click)
    {
        BaseEventData data = new BaseEventData(EventSystem.current);
        ExecuteEvents.Execute(click, data, ExecuteEvents.submitHandler);
    }

    public void HighlightFirstChoice()
    {
        optionButtons[0].Select();
    }

    public void SetOption(int i, UnityAction onClick, DialogueOption dialogueOption)
    {

        // Check
        if (i >= optionButtons.Length)
        {
            Debug.LogWarning("There are more options to present than there are" +
                             "buttons to present them in. Only the first " +
                             $"{optionButtons.Length} options will be shown.");
            return;
        }

        Button b = optionButtons[i];
        b.gameObject.SetActive(true);

        // When the button is selected, tell the dialogue about it
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(() =>
        {
            CleanupOptions();
            StartCoroutine(FadeOptions(b));
            onClick();
        });

        var optionText = dialogueOption.Line.Text.Text;

        if (optionText == null)
        {
            Debug.LogWarning($"Option {dialogueOption.TextID} doesn't have any localised text");
            optionText = dialogueOption.TextID;
        }

        var textMeshProText = b.GetComponentInChildren<TextMeshProUGUI>();
        if (textMeshProText != null)
        {
            textMeshProText.text = optionText;
        }
    }

    public IEnumerator FadeOptions(Button clicked)
    {
        foreach (var button in optionButtons)
        {
            if (button == clicked) continue;
            button.gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(OPTION_SELECT_ANIMATION_LENGTH);
        clicked.gameObject.SetActive(false);
    }

    public void HideAllOptions()
    {
        foreach (var button in optionButtons)
        {
            button.gameObject.SetActive(false);
        }
    }
}
