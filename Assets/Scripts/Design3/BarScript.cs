using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarScript : MonoBehaviour
{
    public int value;

    public float getHeight() { return transform.localScale.y; }
    public float getWidth() { return transform.localScale.x; }
}
