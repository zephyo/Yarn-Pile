using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if !UNITY_EDITOR && UNITY_WEBGL
    using System.Runtime.InteropServices;
#endif

/// <summary>
/// LaunchURL:
/// If WebGL, use WebURL.jslib to open in new tab
/// Else, use Application.OpenURL
/// </summary>
public class About : MonoBehaviour
{
#if !UNITY_EDITOR && UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void OpenNewTab(string URL);
#endif

    public void LaunchURL(string URL)
    {
#if !UNITY_EDITOR && UNITY_WEBGL
             OpenNewTab(URL);
#else
        Application.OpenURL(URL);
#endif
    }
}
