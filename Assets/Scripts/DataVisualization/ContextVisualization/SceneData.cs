using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneData : MonoBehaviour
{
    public Transform cameraPosition;
    public Transform minPos;
    public Transform maxPos;
    public string[] keys;
    public GameObject[] shapes;

    public string variable;

    public void Start()
    {
        VisualizationManager visuManager = FindAnyObjectByType<VisualizationManager>();
        if (visuManager != null) visuManager.setSceneData(this);
        else Debug.Log("SceneData could not find visu manager");
    }

    public minNmax getMinNMax()
    {
        minNmax mnm = new minNmax();
        //mnm.max = minPos.position;
        //mnm.min = maxPos.position;
        mnm.max = new Vector3(Mathf.Max(minPos.position.x, maxPos.position.x), Mathf.Max(minPos.position.y, maxPos.position.y), Mathf.Max(minPos.position.z, maxPos.position.z));
        mnm.min = new Vector3(Mathf.Min(minPos.position.x, maxPos.position.x), Mathf.Min(minPos.position.y, maxPos.position.y), Mathf.Min(minPos.position.z, maxPos.position.z));
        return mnm;
    }

    public Dictionary<string,GameObject> getShapes()
    {
        Dictionary<string, GameObject> shapesDic = new Dictionary<string, GameObject>();
        for (int i = 0; i < Mathf.Min(keys.Length, shapes.Length); i++)
        {
            shapesDic.Add(keys[i], shapes[i]);
        }
        return shapesDic;
    }
}
