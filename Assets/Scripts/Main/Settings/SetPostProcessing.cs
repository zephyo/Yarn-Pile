using UnityEngine.UI;
using UnityEngine;
using System;

public class SetPostProcessing : MonoBehaviour, ISettingsSetter
{
    const string KEY = "PostProcessing";
    const int ppDefault = 1;
    public Toggle toggle;
    public event Action<bool> OnSetPostProcess;

    public void Prepare()
    {
        // set listener
        toggle.onValueChanged.AddListener(PostProcessing);
    }

    public void Load()
    {
        float pp = PlayerPrefs.GetInt(KEY, ppDefault);
        // set value
        toggle.isOn = pp == 1;
    }

    private void PostProcessing(bool on)
    {
        PlayerPrefs.SetInt(KEY, on ? 1 : 0);
        OnSetPostProcess?.Invoke(on);
    }
}
