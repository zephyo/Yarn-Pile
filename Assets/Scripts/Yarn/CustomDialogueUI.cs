/*

The MIT License (MIT)

Copyright (c) 2015-2017 Secret Lab Pty. Ltd. and Yarn Spinner contributors.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/


using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Yarn.Unity;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections.Generic;
using DG.Tweening;

public enum DialogueType
{
    Player,
    Think,
    NPC,
}

public class CustomDialogueUI : DialogueViewBase
{
    // Dialogue type to dialogue object

    [System.Serializable]
    class DialogueDictionary : SerializableDictionaryBase<DialogueType, DialogueGroup> { }

    [SerializeField] private DialogueDictionary dialogueDictionary = new DialogueDictionary();

    // current dialogue
    [HideInInspector]
    public DialogueGroup currentDialogue;

    /// <summary>
    /// How quickly to show the text, in seconds per character
    /// </summary>
    [Tooltip("How quickly to show the text, in seconds per character")]
    public float textSpeed = SetTextSpeed.speedDefault;

    /// <summary>
    /// When true, the Runner has signaled to finish the current line
    /// asap.
    /// </summary>
    private bool finishCurrentLine = false;

    // The method that we should call when the user has chosen an
    // option. Externally provided by the DialogueRunner.
    private System.Action<int> currentOptionSelectionHandler;

    // When true, the DialogueRunner is waiting for the user to press
    // one of the option buttons.
    private bool waitingForOptionSelection = false;

    private bool timedOptions = true;


    #region EVENTS
    public event Action onDialogueStart;

    public event Action onDialogueEnd;


    /// <summary>
    /// Called before a <see cref="Line"/> is going to be parsed for markup.
    /// </summary>
    public event Action onLineParse;

    /// <summary>
    /// Called when a <see cref="Line"/> has been delivered. 
    /// Use this event to prepare the scene to deliver a line.
    /// Takes in line value and text speed.
    /// Returns the total time it'll take to show the line.
    /// </summary>
    public Func<string, float, Sequence> onLineStart;

    /// <summary>
    /// Called when the line has finished being displayed by this view.
    /// This method is called after <see cref="onLineUpdate"/>. Use
    /// this method to display UI elements like a "continue" button. 
    /// </summary>
    public event Action onTextFinishDisplaying;

    /// <summary>
    /// Called when a line has finished being delivered on all views. 
    /// Use this method to display UI elements like a "continue" button
    /// in sync with other <see cref="DialogueViewBase"/> objects, like
    /// voice over playback.
    /// </summary>   
    public event Action onLineFinishDisplaying;

    /// <summary>
    /// Called when the visible part of the line's localised text changes.
    /// Gives length of string to be revealed, at a rate of <see
    /// cref="textSpeed"/> seconds per character. 
    /// If the line's Status becomes <see
    /// cref="LineStatus.Interrupted"/>, which indicates
    /// that the user has requested that the Dialogue UI skip to the
    /// end of the line,
    /// <see cref="onLineUpdate"/> will be called once more, to display
    /// the entire text.
    /// If <see cref="textSpeed"/> is `0`, <see cref="onLineUpdate"/>
    /// will be called just once, to display the entire text all at
    /// once.
    /// </summary>
    public event Action<int> onLineUpdate;

    /// <summary>
    /// Called when a line has finished displaying, and should be removed from
    /// the screen.
    /// This method is called after the line's <see
    /// cref="LocalizedLine.Status"/>
    /// has changed to <see cref="LineStatus.Ended"/>.
    /// Use this method to dismiss the line's UI elements.
    ///
    /// After this method is called, the next piece of dialogue content
    /// will be presented, or the dialogue will end.
    /// </summary>
    public event Action onLineEnd;

    /// <summary>
    /// Called when an <see cref="OptionSet"/> has been displayed to the user.
    /// Before this method is called, the <see cref="Button"/>s in <see
    /// cref="optionButtons"/> are enabled or disabled (depending on
    /// how many options there are), and the <see cref="Text"/> or <see
    /// cref="TMPro.TextMeshProUGUI"/> is updated with the correct
    /// text.
    ///
    /// Use this method to ensure that the active <see
    /// cref="optionButtons"/>s are visible, such as by enabling the
    /// object that they're contained in.
    /// 
    /// The bool parameter indicates whether or not the options are timed.
    /// </summary>
    public event Action<bool> onOptionsStart;

    /// <summary>
    /// Called when an option has been selected, and the <see
    /// cref="optionButtons"/> should be hidden.
    /// This method is called after one of the <see
    /// cref="optionButtons"/> has been clicked, or the <see
    /// cref="SelectOption(int)"/> method has been called.
    ///
    /// Use this method to hide all of the <see cref="optionButtons"/>,
    /// such as by disabling the object they're contained in. (The
    /// DialogueUI won't hide them for you individually.)
    /// </summary>
    public event Action onOptionsEnd;
    #endregion

    private void Awake()
    {
        FindObjectOfType<SetTextSpeed>().OnSetTextSpeed += (f) => textSpeed = f;
        MainSingleton.Instance.dialogueRunner.AddCommandHandler<bool>("SetTimed", SetTimed);
    }

    private void OnDestroy()
    {
        DialogueRunner runner = MainSingleton.Instance?.dialogueRunner;
        if (runner)
        { runner.RemoveCommandHandler("SetTimed"); }
    }

    private void SetTimed(bool t)
    {
        timedOptions = t;
    }

    /// <inheritdoc/>
    public override void RunLine(LocalizedLine dialogueLine, System.Action onDialogueLineFinished)
    {
        StartCoroutine(DoRunLine(dialogueLine, onDialogueLineFinished));
    }

    /// <summary>
    /// Shows a line of dialogue, gradually.
    /// </summary>
    /// <param name="dialogueLine">The line to deliver.</param>
    /// <param name="onDialogueLineFinished">A callback to invoke when
    /// the text has finished appearing.</param>
    /// <returns></returns>
    protected IEnumerator DoRunLine(LocalizedLine dialogueLine, System.Action onDialogueLineFinished)
    {
        finishCurrentLine = false;

        // First, potentially change DialogueGroups
        Yarn.Markup.MarkupParseResult markupResult = TryParseCharacter(dialogueLine);

        // Then, prepare dialogue group for tweening, markup, etc
        onLineParse?.Invoke();

        // The final text we'll be showing for this line.
        string text = TryParseMarkup(markupResult);

        Sequence showAnimation = onLineStart.Invoke(text, textSpeed);
        var frame = new WaitForEndOfFrame();

        while (showAnimation != null && showAnimation.IsActive() && !showAnimation.IsComplete())
        {
            if (finishCurrentLine)
            {
                break;
            }
            yield return frame;
        }

        // Indicate to the rest of the game that the text has finished
        // being delivered
        onTextFinishDisplaying?.Invoke();

        // Indicate to the dialogue runner that we're done delivering
        // the line here
        onDialogueLineFinished();
    }

    private Yarn.Markup.MarkupParseResult TryParseCharacter(LocalizedLine dialogueLine)
    {
        Yarn.Markup.MarkupParseResult markupResult = dialogueLine.Text;
        if (markupResult.TryGetAttributeWithName("character", out var attribute))
        {
            TextAttributeHandler.HandleAttribute(attribute, this);
            return markupResult.DeleteRange(attribute);
        }
        return markupResult;
    }

    private string TryParseMarkup(Yarn.Markup.MarkupParseResult markupResult)
    {
        // Delete attributes
        foreach (KeyValuePair<string, bool> pair in TextAttributeHandler.attributes)
        {
            // If attribute is present, remove its text
            if (markupResult.TryGetAttributeWithName(pair.Key, out var attribute))
            {
                if (pair.Value == true)
                {
                    markupResult = markupResult.DeleteRange(attribute);
                }
                TextAttributeHandler.HandleAttribute(attribute, this);
            }

        }
        return markupResult.Text;
    }

    public void SetCurrentDialogue(DialogueType type, string name = null)
    {
        DialogueGroup dialogueGroup = dialogueDictionary[type];
        if (dialogueGroup && dialogueGroup != currentDialogue)
        {
            currentDialogue?.TurnOff();
            dialogueGroup.TurnOn(this);
            currentDialogue = dialogueGroup;
        }
        // If dialogue is said by NPC, and we explicitly said who the NPC is, position above the NPC
        if (type == DialogueType.NPC && !string.IsNullOrEmpty(name))
        {
            MainSingleton.Instance.CharacterManager.PositionDialogue(dialogueGroup, name);
        }
    }

    public override void OnLineStatusChanged(LocalizedLine dialogueLine)
    {

        switch (dialogueLine.Status)
        {
            case LineStatus.Running:
                // No-op; this line is running
                break;
            case LineStatus.Interrupted:
                // The line is now interrupted, and we need to hurry up
                // in our delivery
                finishCurrentLine = true;
                break;
            case LineStatus.Delivered:
                // The line has now finished its delivery across all
                // views, so we can signal call our UnityEvent for it
                onLineFinishDisplaying?.Invoke();
                break;
            case LineStatus.Ended:
                // The line has now Ended. DismissLine will be called
                // shortly.
                onLineEnd?.Invoke();
                break;
        }
    }

    public override void DismissLine(System.Action onDismissalComplete)
    {
        // This view doesn't need any extra time to dismiss its view,
        // so it can just call onDismissalComplete immediately.
        onDismissalComplete();
    }

    /// Runs a set of options.
    /// <inheritdoc/>
    public override void RunOptions(DialogueOption[] dialogueOptions, System.Action<int> onOptionSelected)
    {
        StartCoroutine(DoRunOptions(dialogueOptions, onOptionSelected));
    }

    /// Show a list of options, and wait for the player to make a
    /// selection.
    private IEnumerator DoRunOptions(DialogueOption[] dialogueOptions, System.Action<int> selectOption)
    {

        // Display each option in a button, and make it visible
        int i = 0;

        waitingForOptionSelection = true;

        currentOptionSelectionHandler = selectOption;

        /* Only Think dialogue shows options; if want other dialogues to 
        show options, then create a new DialogueOptionsGroup and change below logic */
        SetCurrentDialogue(DialogueType.Think);
        DialogueOptionsGroup optionsGroup = (DialogueOptionsGroup)currentDialogue;

        foreach (var dialogueOption in dialogueOptions)
        {
            optionsGroup.SetOption(i, () => SelectOption(dialogueOption.DialogueOptionID), dialogueOption);
            i++;
        }


        onOptionsStart?.Invoke(timedOptions);

        // Wait until the chooser has been used and then removed 
        while (waitingForOptionSelection)
        {
            yield return null;
        }
        onOptionsEnd?.Invoke();
    }

    /// Called when the dialogue system has started running.
    /// <inheritdoc/>
    public override void DialogueStarted()
    {
        onDialogueStart?.Invoke();
    }

    /// Called when the dialogue system has finished running.
    /// <inheritdoc/>
    public override void DialogueComplete()
    {
        onDialogueEnd?.Invoke();

        // Hide the dialogue interface.
        if (currentDialogue)
            currentDialogue.Hide();
    }

    /// <summary>
    /// Signals that the user has selected an option.
    /// </summary>
    /// <remarks>
    /// This method is called by the <see cref="Button"/>s in the <see
    /// cref="optionButtons"/> list when clicked.
    ///
    /// If you prefer, you can also call this method directly.
    /// </remarks>
    /// <param name="optionID">The <see cref="OptionSet.Option.ID"/> of
    /// the <see cref="OptionSet.Option"/> that was selected.</param>
    public void SelectOption(int optionID)
    {
        if (!waitingForOptionSelection)
        {
            Debug.LogWarning("An option was selected, but the dialogue UI was not expecting it.");
            return;
        }
        waitingForOptionSelection = false;
        currentOptionSelectionHandler?.Invoke(optionID);
    }


}
