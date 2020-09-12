using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VectorGraphics;

/// <summary>
/// Displays an icon, percentage number, and percentage bar
/// Listens for change in var value; if there's a change, update UI accordingly
/// 
/// Assumes yarn variable keyed by varName is a number.
/// </summary>
public class VarUI : MonoBehaviour
{
    public string varName;
    [Header("UI Components")]
    public Graphic icon;
    public TextMeshProUGUI percentText;
    public Slider progressBar;

    private DefaultVariable data;

    private CustomStorage storage;

    private void Start()
    {
        if (varName[0] == '$')
        {
            Debug.LogError($"{varName} is not a valid variable name for accessing defaultVariables." +
            "Please give a variable name, unformatted for Yarn.");
        }
        storage = (CustomStorage)MainSingleton.Instance.dialogueRunner.variableStorage;
        /* Access defaultVariables with unformatted varName, e.g. 'name'
        This is because yarn variables are stored as '$name' but defaultVariables, which
        are a custom extension of Yarn, are stored as 'name' to be compatible with enums */
        data = storage.defaultVariables[varName];
        // set var name to be format '$name'
        varName = varName.YarnFormat();

        // set UI elements
        if (icon is Image)
        {
            ((Image)icon).sprite = data.icon;
        }
        else if (icon is SVGImage)
        {
            ((SVGImage)icon).sprite = data.icon;
        }

        Yarn.Value value = storage.GetValue(varName);
        SetPercentUI(value.AsNumber);

        storage.OnSetValue += OnYarnValueChanged;
    }

    private void SetPercentUI(float value)
    {
        value = Mathf.Clamp(value, data.min, data.max);
        // Calculate percent
        float percent = (value - data.min) / (data.max - data.min);
        // set percent UI
#if UNITY_EDITOR
        Debug.Log($"VarUI {varName}: Set percent from ${percent} to ${Mathf.Floor(percent * 100)}!");
#endif
        percentText.text = $"{Mathf.Floor(percent * 100)}%";
        progressBar.minValue = data.min;
        progressBar.maxValue = data.max;
        progressBar.value = value;
    }

    private void OnDestroy()
    {
        storage.OnSetValue -= OnYarnValueChanged;
    }

    private void OnYarnValueChanged(string name, Yarn.Value value)
    {
        if (name == varName)
        {
            SetPercentUI(value.AsNumber);
        }
    }
}
