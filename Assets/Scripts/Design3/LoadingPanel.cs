using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingPanel : MonoBehaviour
{
    public GameObject loadingIcon;

    // Update is called once per frame
    void Update()
    {
        loadingIcon.transform.Rotate(new Vector3(0, 0, -1));
    }
}
