using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class SortSelector : MonoBehaviour
{
    private string[] _sortCategories;
    private GameObject[] _buttons;

    public void createButtons(string[] sortNames)
    {
        _sortCategories = sortNames;
        _buttons = new GameObject[sortNames.Length];
        var y = 20;
        for(int i =0;  i<sortNames.Length; i++)
        {
            // Create Button
            GameObject btn = new GameObject();
            btn.tag = "Interactive";
            btn.name = sortNames[i];
            btn.transform.parent = this.transform;
            var button = btn.AddComponent<UnityEngine.UI.Button>();
            var img = btn.AddComponent<Image>();
            var btnTransform = btn.GetComponent<RectTransform>();

            if (btnTransform != null)
            {
                btnTransform.localPosition = new Vector3(0, y - 50 * i, 0);
                btnTransform.rotation = new Quaternion(0, 0, 0, 0);
                btnTransform.localScale = Vector3.one;
                btnTransform.sizeDelta = new Vector2(160, 30);
            }
            else Debug.Log("No rectTransform found");

            // Btn Text
            var infContener = new GameObject();
            var inf = infContener.AddComponent<TextMeshProUGUI>();
            infContener.transform.SetParent(btn.transform);
            inf.rectTransform.localPosition = new Vector3(0, 0, 0);
            inf.rectTransform.rotation = new Quaternion(0, 0, 0, 0);
            inf.rectTransform.localScale = Vector3.one;
            inf.rectTransform.sizeDelta = new Vector2(160, 30);

            inf.text = _sortCategories[i];
            inf.color = Color.black;
            inf.fontSize = 24;
            inf.horizontalAlignment = HorizontalAlignmentOptions.Center;
            inf.verticalAlignment = VerticalAlignmentOptions.Middle;

            // Btn script
            var btnScript = btn.AddComponent<SortButton>();
            btnScript.setSortName(_sortCategories[i]);
            btnScript.setSortNb(i);

            // Collider & mesh renderer
            var collider = btn.AddComponent<BoxCollider>();
            collider.size = new Vector3(160, 30, 10);
            btn.AddComponent<MeshRenderer>();

            _buttons[i] = btn;
            // Add script to button
        }
    }

    public void selectedButton(int numBtn)
    {
        for(int i=0; i<_buttons.Length; i++)
        {
            var sortBtn = _buttons[i].GetComponent<SortButton>();
            sortBtn.selectBtn(numBtn);
        }
    }
}
