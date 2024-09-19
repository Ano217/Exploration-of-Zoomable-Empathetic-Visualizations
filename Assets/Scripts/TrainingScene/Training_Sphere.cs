using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Training_Sphere : MonoBehaviour
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
        var sceneManager = GameObject.FindAnyObjectByType<TrainingManager>();
        if (collision.gameObject.name == "VRCamera" && transform.parent.name != _parent.name && sceneManager.getCurrentImmersiveSphere() == null)
        {
            //Debug.Log("Theoretical parent: " + _parent.name + "   actual parent:" + transform.parent.name);

            sceneManager.switch360View(true, this, texture);
        }
    }

    public void OnCollisionExit(Collision collision)
    {
        var sceneManager = GameObject.FindAnyObjectByType<TrainingManager>();
        if (collision.gameObject.name == "VRCamera" && transform.parent.name != _parent.name)
        {
            sceneManager.switch360View(false);
        }
    }

    public void AttachToHand(GameObject hand, Vector3 position)
    {
        this.transform.position = position;
        this.transform.parent = hand.transform;
    }
}
