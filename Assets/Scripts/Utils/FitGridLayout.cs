using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FitGridLayout : MonoBehaviour
{
    public int maxColumns;
    public int maxRows;

    // Use this for initialization
    IEnumerator Start()
    {
        /* 
        Must wait one frame 
        because some platform builds don't 
        have RectTransform size set yet
        */
        yield return null;
        RectTransform r = (RectTransform)transform;
        float width = r.rect.width;
        float height = r.rect.height;
        Vector2 newSize = new Vector2(width / maxColumns, height / maxRows);
        gameObject.GetComponent<GridLayoutGroup>().cellSize = newSize;

    }
}
