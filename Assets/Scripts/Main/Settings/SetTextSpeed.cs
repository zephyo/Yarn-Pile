using UnityEngine.UI;
using UnityEngine;
using System;

public class SetTextSpeed : MonoBehaviour, ISettingsSetter
{
    const string KEY = "TextSpeed";
    public const float speedDefault = 0.02f;
    public Slider slider;
    public event Action<float> OnSetTextSpeed;

    public void Prepare()
    {
        // set min and max
        slider.minValue = 20;
        slider.maxValue = 100;

        // set listener
        slider.onValueChanged.AddListener(textSpeed);
    }

    public void Load()
    {
        float speed = PlayerPrefs.GetFloat(KEY, speedDefault);
        // set value
        slider.value = 1 / speed;
    }

    private void textSpeed(float value)
    {
        float speed = 1 / value;
        PlayerPrefs.SetFloat(KEY, speed);
        OnSetTextSpeed?.Invoke(speed);
    }
}
