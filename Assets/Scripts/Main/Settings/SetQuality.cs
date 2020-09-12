using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SetQuality : MonoBehaviour, ISettingsSetter
{
    const string RES_KEY = "Resolution", Q_KEY = "Quality";

    public TMP_Dropdown resxDropdown;
    public TMP_Dropdown qualityDropdown;
    Resolution[] resolutions;

    private int currentResolution = 0;

    public void Prepare()
    {
        resolutions = Screen.resolutions;
        for (int i = 0; i < resolutions.Length; i++)
        {
            resxDropdown.options.Add(new TMP_Dropdown.OptionData(ResToString(resolutions[i])));
            // Find what index currentResolution is
            if (Screen.currentResolution.Equals(resolutions[i]))
            {
                currentResolution = i;
            }
        }
        resxDropdown.onValueChanged.AddListener(SetResolution);

        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            qualityDropdown.options.Add(new TMP_Dropdown.OptionData(QualitySettings.names[i]));
        }
        qualityDropdown.onValueChanged.AddListener(SetQualityLevel);
    }

    public void Load()
    {
        int q = PlayerPrefs.GetInt(Q_KEY, QualitySettings.GetQualityLevel());
        int res = PlayerPrefs.GetInt(RES_KEY, currentResolution);
        resxDropdown.value = res;
        qualityDropdown.value = q;
    }


    private void SetQualityLevel(int i)
    {
        PlayerPrefs.SetInt(Q_KEY, i);
        QualitySettings.SetQualityLevel(i);
    }

    private void SetResolution(int i)
    {
        PlayerPrefs.SetInt(RES_KEY, i);
        Screen.SetResolution(resolutions[i].width, resolutions[i].height, true);
    }

    string ResToString(Resolution res)
    {
        return res.width + " x " + res.height;
    }
}

