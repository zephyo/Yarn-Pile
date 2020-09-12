using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipEvents : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static event Action<TooltipData> OnShowTooltip;
    public static event Action OnHideTooltip;

    [SerializeField]
    TooltipData data;

    //Detect if the Cursor starts to pass over the GameObject
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        OnShowTooltip?.Invoke(data);
    }

    //Detect when Cursor leaves the GameObject
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        OnHideTooltip?.Invoke();
    }
}
