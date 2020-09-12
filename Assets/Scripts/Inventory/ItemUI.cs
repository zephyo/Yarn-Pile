using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class ItemUI : MonoBehaviour
{
    public Image icon;

    public void Init(ItemData data, UnityAction action)
    {
        icon.sprite = data.icon;
        GetComponent<Button>().onClick.AddListener(action);
    }
}
