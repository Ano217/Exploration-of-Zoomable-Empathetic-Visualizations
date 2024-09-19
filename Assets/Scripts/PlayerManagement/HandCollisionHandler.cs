using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCollisionHandler : MonoBehaviour
{
    private List<GameObject> _collisions;
    [HideInInspector] public delegate void DelegateCollisionEnter(GameObject collided);
    [HideInInspector] public DelegateCollisionEnter collisionEnter;
    [HideInInspector] public delegate void DelegateColisionExit(GameObject collided);
    [HideInInspector] public DelegateColisionExit collisionExit;
    [HideInInspector] public delegate void DelegateColisionHead(bool collided);
    [HideInInspector] public DelegateColisionHead collisionHead;

    public List<GameObject> collisions => _collisions;

    public void Start()
    {
        _collisions = new List<GameObject>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Interactive")
        {
            _collisions.Add(collision.gameObject);
            collisionEnter.Invoke(collision.gameObject);
        }
        if (collision.gameObject.tag == "VirtualButton")
        {
            if (collisionEnter != null && collision.gameObject != null) collisionEnter.Invoke(collision.gameObject);
        }
        if(collision.gameObject.name == "VRCamera")
        {
            collisionHead.Invoke(true);
        }

    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Interactive")
        {
            _collisions.Remove(collision.gameObject);
            collisionExit.Invoke(collision.gameObject);
        }
        if (collision.gameObject.name == "VRCamera")
        {
            collisionHead.Invoke(false);
        }
    }

    public bool isColliding(GameObject other)
    {
        return _collisions.Contains(other);
    }

    public void resetCollisions()
    {
        _collisions.Clear();
    }
}
