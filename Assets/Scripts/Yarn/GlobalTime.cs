using System;
using UnityEngine;
using Yarn.Unity;
using TMPro;
using System.Globalization;

[RequireComponent(typeof(MainSingleton))]
public class GlobalTime : MonoBehaviour
{
    const string format = "h:mmtt";
    [SerializeField]
    TextMeshProUGUI timeText;
    [HideInInspector]
    public DateTime time;

    public event Action<DateTime> OnSetTime;

    private void OnValidate()
    {
        timeText = GameObject.Find("GlobalTimeText").GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        DialogueRunner runner = MainSingleton.Instance.dialogueRunner;
        runner.AddCommandHandler<string>("SetTime", SetTime);
        runner.AddCommandHandler<int>("AddMinutes", AddMinutes);
    }

    /// <summary>
    /// Parse given string into datetime
    /// Expect format: 9:42 PM
    /// </summary>
    /// <param name="dateStr"></param>
    private void SetTime(string dateStr)
    {
#if UNITY_EDITOR
        Debug.Log($"SetTime: {dateStr}");
#endif
        SaveTime(DateTime.ParseExact(dateStr, format, CultureInfo.InvariantCulture));

    }

    private void AddMinutes(int minutes)
    {
        SaveTime(time.AddMinutes(minutes));
    }

    /// <summary>
    /// Display current internalTime to UI
    /// </summary>
    public void SaveTime(DateTime time)
    {
        this.time = time;
        timeText.text = time.ToString(format);
        OnSetTime?.Invoke(time);
    }
}
