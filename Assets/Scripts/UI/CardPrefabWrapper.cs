using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class CardPrefabWrapper : MonoBehaviour
{
    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        var parent = transform.parent as RectTransform;
        if (parent == null) return;

        var grid = parent.GetComponent<GridLayoutGroup>();
        if (grid == null) return;

        float baseWidth = 400f;
        float baseHeight = 600f;

        float scaleX = grid.cellSize.x / baseWidth;
        float scaleY = grid.cellSize.y / baseHeight;

        float finalScale = Mathf.Min(scaleX, scaleY);
        rectTransform.localScale = new Vector3(finalScale, finalScale, 1f);
    }
}
