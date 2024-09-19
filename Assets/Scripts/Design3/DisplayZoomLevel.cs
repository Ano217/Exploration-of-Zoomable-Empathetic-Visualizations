using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayZoomLevel : MonoBehaviour
{
    public RectTransform loadingBar;
    public RectTransform backGroundBar;

    public void updateBar(float zoomLevel)
    {
        var prop = (zoomLevel-VisualizationLevels.MINSIZE)/VisualizationLevels.MAXSIZE;
        var backWidth = backGroundBar.rect.width;
        loadingBar.sizeDelta = new Vector2(backWidth * prop, backGroundBar.sizeDelta.y);
    }
}
