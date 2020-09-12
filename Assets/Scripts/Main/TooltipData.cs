using System.Collections.Generic;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

[CreateAssetMenu(fileName = "TooltipData", menuName = "ScriptableObjects/TooltipData", order = 1)]
public class TooltipData : ScriptableObject
{
    public string title;
    [TextArea]
    public string description;
    [TextArea]
    public string footnote;

}