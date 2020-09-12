using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Calls MainSingleton functions
/// </summary>
public class SingletonCaller : MonoBehaviour
{
    public void StartGame()
    {
        MainSingleton.Instance.StartGame();
    }

    public void PlayButtonSoundEffect()
    {
        MainSingleton.Instance.PlayButtonSoundEffect();
    }


    public void ViewAbout()
    {
        MainSingleton.Instance.ViewAbout();
    }

    public void ViewSettings()
    {
        MainSingleton.Instance.ViewSettings();
    }


    public void ViewQuit()
    {
        MainSingleton.Instance.ViewQuit();
    }

}
