using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextLabelManager : MonoBehaviour
{
    public Canvas canvas;
    public RectTransform canvasTransform;
    public Image panel;
    public TMP_Text text;


    public void resize(float height, float width) // height and width are in meters
    {
        panel.rectTransform.localScale = new Vector3(width, height, panel.rectTransform.localScale.z);
    }

    public Vector3 getSize()
    {
        return panel.rectTransform.localScale;
    }

    public void changeText(string newText) { text.text = newText; }

    public string getText() { return text.text; }

    public void changePanelBackgroung(Color c) { panel.material.color = c; }

    public void setPivotX(float x)
    {
        canvasTransform.pivot = new Vector2(x, canvasTransform.pivot.y);
    }

    public Vector2 getPivot()
    {
        return canvasTransform.pivot;
    }
}
