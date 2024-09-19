using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainingButton : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    private float _lastClick;
    private bool _clicked = false;
    private float _resetTime = 3f;
    private Image _image;
    private Color _defaultColor = new Color(0.8f, 0.7f, 1);
    private Color _collideColor = new Color(0.8f, 0.6f, 1);
    private Color _clickedColor = new Color(0.8f, 0.4f, 1);

    public void Start()
    {
        _image = GetComponent<Image>();
        _image.color = _defaultColor;
        textMeshPro.text = "TEST";
    }

    public void Update()
    {
        if (_clicked && Time.time - _lastClick > _resetTime)
        {
            _lastClick = 0f; 
            _clicked = false;
            _image.color = _defaultColor;
            textMeshPro.text = "TEST";
        }
    }

    public void OnTipEnter()
    {
        if(!_clicked)
        {
            _image.color = _collideColor;
        }
        
    }

    public void OnTipExit()
    {
        if (!_clicked) _image.color = _defaultColor;
    }

    public void OnClick()
    {
        textMeshPro.text = "Click!";
        _lastClick = Time.time;
        _clicked = true;
        _image.color = _clickedColor;
    }
}
