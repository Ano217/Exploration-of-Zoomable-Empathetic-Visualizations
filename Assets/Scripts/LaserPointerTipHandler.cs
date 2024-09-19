using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaserPointerTipHandler : MonoBehaviour
{
    public Material fullyTransparent;
    public Material transparentMat;
    public Material filledMaterial;
    private Renderer _renderer;
    private Outline _outline;
    private Transform _hitTransform;

    // Start is called before the first frame update
    void Start()
    {
        _outline = GetComponent<Outline>();
        if (_outline != null) _outline.enabled = true;
        _renderer = GetComponent<Renderer>();
    }

    public void setHitTransform(Transform hit)
    {
        if (hit != null)
        {
            this.gameObject.transform.parent = hit;
            _hitTransform = hit;
            this.gameObject.transform.position = hit.position;
        }
        else
        {
            Debug.Log("detach from parent");
            this.gameObject.transform.parent = null;
            this.gameObject.transform.position= Vector3.zero;
        }
    }

    // Update is called once per frame
    void Update()
    {
        /*if(_hitTransform != null)
        {
            transform.position = _hitTransform.position;
        }*/
    }

    public void makeInvisible(bool visible)
    {
        Debug.Log("Make visible " + visible);
        if (_renderer == null)
        {
            Debug.Log("Renderer is null");
            _renderer = this.GetComponent<Renderer>();
        }
        if (_renderer != null)
        {
            _renderer.material = visible ? transparentMat : fullyTransparent;
        }
        else Debug.Log("Renderer is still null");

        this.gameObject.SetActive(visible);
        Debug.Log("End makeInvisible");
    }

    public void grab(bool grabbing)
    {
        //Debug.Log("Pointer tip grab: " + grabbing);
        if (_renderer != null)
        {
            if (filledMaterial == null) Debug.Log("Filledmaterial is null");
            if (transparentMat == null) Debug.Log("Transoarent material is null");
            if (grabbing)
            {
                _renderer.material = filledMaterial;
            }
            else
            {
                _renderer.material = transparentMat;
            }
        }
        else
        {
            Debug.Log("Renderer is null");
            _renderer = GetComponent<Renderer>();
        }
    }
}
