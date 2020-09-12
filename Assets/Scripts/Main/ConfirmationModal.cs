using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class ConfirmationModal : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI text;

    [SerializeField]
    Button yesButton;

    [SerializeField]
    Button noButton;

    private System.Action onConfirm, onEither;


    private void OnValidate()
    {
        text = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        yesButton = transform.Find("Yes").GetComponent<Button>();
        noButton = transform.Find("No").GetComponent<Button>();
    }

    public void Init(string message, System.Action onConfirm, System.Action onEither)
    {
        text.text = message;
        this.onConfirm = onConfirm;
        this.onEither = onEither;
    }

    public void OnConfirm()
    {
        onConfirm?.Invoke();
        onEither?.Invoke();
    }

    public void OnReject()
    {
        onEither?.Invoke();
    }
}
