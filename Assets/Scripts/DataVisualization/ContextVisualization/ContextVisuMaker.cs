using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlaneArea{
    Vector3[] limits;
}

public class ContextVisuMaker : MonoBehaviour
{
    public GameObject contextVisu;


    public void Start()
    {
        VisualizationManager visuManager = FindAnyObjectByType<VisualizationManager>();
        if (visuManager != null) visuManager.setContextVisuMaker(this);
    }
    public void createContext(Dictionary<string,List<DataPoint>> data, minNmax zone, Dictionary<string,GameObject> shapes)
    {
        float x = zone.min.x;
        float z = zone.min.z;

        int nbPoints = 0;
        foreach (string k in data.Keys) nbPoints += data[k].Count;
        float deltaX = (zone.max.x - x)/Mathf.Sqrt(nbPoints);
        float deltaZ = (zone.max.z - z) / Mathf.Sqrt(nbPoints);


        foreach (string key in data.Keys)
        {
            foreach(DataPoint point in data[key])
            {
                GameObject visu = GameObject.Instantiate<GameObject>(contextVisu);
                GameObject ssVisu = GameObject.Instantiate<GameObject>(shapes[key]);
                ssVisu.transform.parent = visu.transform;
                visu.transform.position = new Vector3(x, 0.0f, z);
                z += deltaZ;
                if (z > zone.max.z)
                {
                    z = zone.min.z;
                    x += deltaX;
                }
            }
        }
    }
}
