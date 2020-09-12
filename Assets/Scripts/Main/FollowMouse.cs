using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

/// <summary>
/// Class for keyboard and mouse platforms only.
/// Hides cursor
/// Attach image component to this
/// </summary>
[RequireComponent(typeof(Graphic))]
public class FollowMouse : MonoBehaviour
{
    public Canvas canvas;
    public float smoothTime = 0.3f;
    public float growSpeed = 10;
    public float hoverSize = 4;
    private Vector2 originalSize;
    private float targetSizeFactor = 1;
    private RectTransform rectTransform;
    private Vector3 velocity = Vector3.zero;

    void Awake()
    {
#if !UNITY_STANDALONE && !UNITY_WEBGL 
        Destroy(gameObject);
        return;
#else
        rectTransform = ((RectTransform)transform);
        originalSize = rectTransform.sizeDelta;
#endif

    }

#if UNITY_STANDALONE || UNITY_WEBGL 
    void Update()
    {
        Vector3 target;

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            target = Mouse.current.position.ReadValue();
        }
        // render mode is camera -- only tested for screen space - camera
        else
        {
            Vector3 screenPoint = Mouse.current.position.ReadValue();
            screenPoint.z = canvas.planeDistance; //distance of the plane from the camera
            target = canvas.worldCamera.ScreenToWorldPoint(screenPoint);
        }

        transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, smoothTime);

        // detect if mouse is hovering over clickable element.
        // If so, enlargen cursor
        targetSizeFactor = EventSystem.current.IsPointerOverGameObject() ? hoverSize : 1;
        rectTransform.sizeDelta = Vector2.Lerp(rectTransform.sizeDelta, originalSize * targetSizeFactor, Time.deltaTime * growSpeed);
    }
#endif
}
