using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    public void PrepareAndLoadAll()
    {
        List<ISettingsSetter> setters = new List<ISettingsSetter>();
        setters.Add(gameObject.GetComponent<SetVolume>());
        setters.Add(gameObject.GetComponent<SetPostProcessing>());
        setters.Add(gameObject.GetComponent<SetQuality>());
        setters.Add(gameObject.GetComponent<SetTextSpeed>());
        foreach (ISettingsSetter setter in setters)
        {
            setter.Prepare();
            setter.Load();
        }
    }
}
