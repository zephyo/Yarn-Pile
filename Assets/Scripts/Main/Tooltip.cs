using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputBinding;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    [SerializeField]
    CanvasGroup canvasGroup;

    [SerializeField]
    TextMeshProUGUI tooltipText;

    private void OnValidate()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        tooltipText = GetComponentInChildren<TextMeshProUGUI>();
    }
    private void Start()
    {
        HideTooltip();
        TooltipEvents.OnShowTooltip += ShowTooltip;
        TooltipEvents.OnHideTooltip += HideTooltip;
    }

    private void OnDestroy()
    {
        TooltipEvents.OnShowTooltip -= ShowTooltip;
        TooltipEvents.OnHideTooltip -= HideTooltip;
    }

    private void ShowTooltip(TooltipData data)
    {
        // Try get action
        InputAction action = MainSingleton.Instance.input.actions.FindAction(data.title);
        string shortcut = null;
        if (action != null)
        {
            shortcut = action.GetBindingDisplayString(DisplayStringOptions.DontIncludeInteractions, MainSingleton.Instance.input.currentControlScheme);
        }
        // Format 
        tooltipText.text =
        $"<b>{data.title}</b>{(shortcut != null ? $"<align=\"flush\"> {shortcut.ToUpper()}</align>" : "")}" +
        $"{(!string.IsNullOrEmpty(data.description) ? $"\n<size=80%>{data.description}" : "")}" +
        $"{(!string.IsNullOrEmpty(data.footnote) ? $"\n<size=80%><i>{data.footnote}" : "")}";

        canvasGroup.alpha = 1;
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }

    private void HideTooltip()
    {
        canvasGroup.alpha = 0;
    }
}
