using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// On load this scene, attach camera to canvas
/// </summary>

[RequireComponent(typeof(Canvas))]
public class SceneSetup : MonoBehaviour
{
    public const string BG_CAM_TAG = "BgCamera", MG_CAM_TAG = "MgCamera", FG_CAM_TAG = "FgCamera";

    private void Awake()
    {
        SetCanvasCamera();
    }

    private void SetCanvasCamera()
    {
        string layer = LayerMask.LayerToName(gameObject.layer);
        // Get camera corresponding to the layer 
        Camera camera;
        switch (layer)
        {
            case "Background":
                camera = GameObject.FindGameObjectWithTag(BG_CAM_TAG).GetComponent<Camera>();
                break;
            case "Middleground":
            case "Middleground2":
            case "Middleground3":
                camera = GameObject.FindGameObjectWithTag(MG_CAM_TAG).GetComponent<Camera>();
                break;
            // everything else
            default:
                camera = Camera.main;
                break;

        }
        GetComponent<Canvas>().worldCamera = camera;
    }
}