using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingImmersiveSphere : MonoBehaviour
{
    public Texture texture;
    private Transform _parent;
    private Vector3 _position;

    public void Start()
    {
        Outline outliner = GetComponent<Outline>();
        if (outliner != null)
        {
            outliner.enabled = false;
        }
        _parent = transform.parent;
        _position = transform.localPosition;
        var renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = texture;
    }

    public void resetPosition()
    {
        transform.parent = _parent;
        transform.localPosition = _position;
    }

    public Texture getTexture() { return texture; }



    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "VRCamera" && transform.parent.name != _parent.name)
        {
            var sceneManager = GameObject.FindAnyObjectByType<TrainingManager>();
            //sceneManager.switch360View(true, this, texture);
        }
    }
}
