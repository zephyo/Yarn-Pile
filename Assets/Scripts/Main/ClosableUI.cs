using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// If click button, open or close panel
/// If click outside this rectTransform, close panel
/// </summary>
public class ClosableUI : MonoBehaviour
{
    public string shortcutActionName;
    public Button button;
    const string OPEN_BOOL = "Open";
    private Animator animator;
    private Transform panel;
    // which camera is rendering this?
    [SerializeField]
    private Camera canvasCamera;

    private void OnValidate()
    {
        button = transform.Find("Button")?.GetComponent<Button>();
        canvasCamera = transform.root.GetComponent<Canvas>().worldCamera;
        animator = GetComponent<Animator>();
        panel = transform.Find("Panel");
    }

    protected virtual void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            panel = transform.Find("Panel");
        }
        MainSingleton.Instance.input.onActionTriggered += TestOpenShortcutHack;
        Close();
    }

    public bool IsOpen() => animator.GetBool(OPEN_BOOL);

    protected virtual void Open()
    {
        //  set button to close
        button.onClick.RemoveListener(Open);
        button.onClick.AddListener(Close);
        // Enable inventory
        animator.SetBool(OPEN_BOOL, true);
        MainSingleton.Instance.input.onActionTriggered += TestClickOutside;
    }

    public virtual void Close()
    {
        //  set button to close
        button.onClick.RemoveListener(Close);
        button.onClick.AddListener(Open);
        // close animation, then set inactive
        animator.SetBool(OPEN_BOOL, false);
        MainSingleton.Instance.input.onActionTriggered -= TestClickOutside;

    }

    // Called twice for some reason if UI is open
    // if shortcut key was pressed, open
    private void TestOpenShortcutHack(InputAction.CallbackContext context)
    {
        // Check if cancelled
        if (context.canceled) return;

        if (context.action.name == shortcutActionName && context.performed)
        {
            if (!IsOpen() && panel.localScale.x < 0.5f)
            {
                button.onClick.Invoke();
            }
            // hack to check if it's actually open
            else if (IsOpen() && panel.localScale.x == 1)
            {
                button.onClick.Invoke();
            }
        }
    }

    /// <summary>
    /// Close if click outside panel
    /// </summary>
    /// <param name="context"></param>
    private void TestClickOutside(InputAction.CallbackContext context)
    {
        // Check if cancelled
        if (context.canceled) return;

        if (context.action.name == "Submit" && context.performed)
        {
            bool outside = !RectTransformUtility.RectangleContainsScreenPoint(
                     GetComponent<RectTransform>(),
                        Mouse.current.position.ReadValue(),
                     canvasCamera);
            if (IsOpen() && outside)
            {
                button.onClick.Invoke();
            }
        }
    }
}
