using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public DesktopInteractions desktopInteraction;

    float speed = 0.005f;
    float rotationSpeed = 0.2f;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        
    }

    private void OnEnable()
    {
        desktopInteraction.keyPressed += onKeyPressed;
    }

    private void OnDisable()
    {
        desktopInteraction.keyPressed -= onKeyPressed;
    }

    public void onKeyPressed(KeyCode k)
    {
        Vector3 r = new Vector3();
        Vector3 v = new Vector3();
        switch (k)
        {
            case KeyCode.UpArrow:
                r.x -= rotationSpeed;
                break;
            case KeyCode.DownArrow:
                r.x += rotationSpeed;
                break;
            case KeyCode.LeftArrow:
                r.y -= rotationSpeed;
                break;
            case KeyCode.RightArrow:
                r.y += rotationSpeed;
                break;
            case KeyCode.B:
                v.y -= speed;
                break;
            case KeyCode.D:
                v.x += speed;
                break;
            case KeyCode.H:
                v.y += speed;
                break;
            case KeyCode.Q:
                v.x -= speed;
                break;
            case KeyCode.S:
                v.z -= speed;
                break;
            case KeyCode.Z:
                v.z += speed;
                break;
        }
        transform.Rotate(r);
        transform.Translate(v);
    }


}
