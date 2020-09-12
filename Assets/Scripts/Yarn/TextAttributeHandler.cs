using UnityEngine;
using System.Collections.Generic;
public class TextAttributeHandler
{
    public const string Character = "character";
    public const string Wave = "wave";
    public const string Shake = "shake";
    public const string Screenshake = "screenshake";

    // string: Attribute name
    // bool: Should we delete attribute range?
    // These are parsed after CustomDialogueUI.onLineParse is raised
    public static readonly Dictionary<string, bool> attributes = new Dictionary<string, bool>(){
        {Wave, false},
        {Shake, false},
        {Screenshake, false},
    };

    public static void HandleAttribute(Yarn.Markup.MarkupAttribute attribute, CustomDialogueUI ui)
    {
        switch (attribute.Name)
        {
            case Character:
                SetDialogueGivenLine(attribute, ui);
                break;
            case Wave:
                float speed = attribute.Properties.ContainsKey("s") ? attribute.Properties["s"].FloatValue : 0.5f;
                ui.currentDialogue.tweenText.Wave(
                attribute.Position,
                attribute.Position + attribute.Length,
                speed);
                break;
            case Shake:
                float strength = attribute.Properties.ContainsKey("s") ? attribute.Properties["s"].IntegerValue : 4;
                ui.currentDialogue.tweenText.Shake(
                attribute.Position,
                attribute.Position + attribute.Length,
                strength);
                break;
            case Screenshake:
                float amplitude = attribute.Properties.ContainsKey("a") ? attribute.Properties["a"].FloatValue : 1;
                MainSingleton.Instance.ImpulseSource.m_ImpulseDefinition.m_AmplitudeGain = amplitude;
                MainSingleton.Instance.ImpulseSource.GenerateImpulse();
                break;
        }
    }

    /// <summary>
    /// Set current DialogueGroup based on line attributes: character name
    /// </summary>
    /// <param name="dialogueLine"></param>
    private static void SetDialogueGivenLine(Yarn.Markup.MarkupAttribute attribute, CustomDialogueUI ui)
    {
        string name = attribute.Properties["name"].StringValue;
        DialogueType type;
        // Edit below to add more types
        switch (name)
        {
            case "Player":
                type = DialogueType.Player;
                break;
            case "Think":
                type = DialogueType.Think;
                break;
            default:
                type = DialogueType.NPC;
                break;
        }
        ui.SetCurrentDialogue(type, name);
    }
}