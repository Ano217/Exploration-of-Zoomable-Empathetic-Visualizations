using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SortButton : MonoBehaviour
{
    private int _sortNb;
    private string _sortName;
    private bool _selected = false;

    private Image _image;
    private Color _defaultColor = new Color(0.8f, 0.7f, 1);
    private Color _collideColor = new Color(0.8f, 0.6f, 1);
    private Color _clickedColor = new Color(0.8f, 0.4f, 1);

    public void Start()
    {
        _image = GetComponent<Image>();
        _image.color = _sortNb == 0 ? _clickedColor : _defaultColor;
    }

    public void setSortNb(int sortNb) { _sortNb = sortNb; }
    public int getSortNb() { return  _sortNb; }
    public void setSortName(string name) { _sortName = name; }
    public string getSortName() { return _sortName; }

    public void OnTipEnter()
    {
        _image.color = _collideColor;
    }

    public void OnTipExit()
    {
        if (!_selected) _image.color = _defaultColor;
    }

    public void selectBtn(int numSelected)
    {
        _selected = numSelected == _sortNb ? true : false;
        if (_selected)
        {
            _image.color = _clickedColor;
        }
        else _image.color = _defaultColor;
        
    }
}
