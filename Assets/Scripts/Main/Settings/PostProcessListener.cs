using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(UniversalAdditionalCameraData))]
public class PostProcessListener : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FindObjectOfType<SetPostProcessing>().OnSetPostProcess += SetPostProcess;
    }

    void SetPostProcess(bool on)
    {
        GetComponent<UniversalAdditionalCameraData>().renderPostProcessing = on;
    }
}
