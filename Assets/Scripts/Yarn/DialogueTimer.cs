using System;
using UnityEngine;
using Yarn.Unity;

public class DialogueTimer : MonoBehaviour
{
    public const float DEFAULT_TIMER_SECONDS = 5f;

    public event Action<float> OnTimerChanged;

    private void Start()
    {
        DialogueRunner runner = MainSingleton.Instance.dialogueRunner;
        runner.AddCommandHandler<float>("SetTimer", SetTimer);
        SetTimer(DEFAULT_TIMER_SECONDS);
    }

    public void SetTimer(float seconds)
    {
        OnTimerChanged?.Invoke(seconds);
    }
}
