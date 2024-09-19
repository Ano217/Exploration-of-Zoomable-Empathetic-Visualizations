using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractVisuMaker : MonoBehaviour
{
    public GameObject basicVisu;

    public void createBarGraph(Dictionary<string, List<DataPoint>> sortedData, bool ordered=false)
    {
        float x = 0.0f;
        float y = 0.0f;
        float z = 0.0f;

        // Create a tab with the keys in order to potentially order them behind
        string[] keys = new string[sortedData.Count];
        int k = 0;
        foreach (string key in sortedData.Keys)
        {
            keys[k] = key;
            k++;
        }


        if (ordered) StructTools.sortTab(keys);


        foreach(string key in sortedData.Keys)
        {
            int c = 0;
            int l = 0;
            foreach(DataPoint point in sortedData[key])
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.localScale= new Vector3(0.05f, 0.05f, 0.05f);
                cube.transform.position= new Vector3(x + c*0.07f, y + l*0.07f, z);
                cube.transform.SetParent(gameObject.transform);

                point.replaceVisu(cube);
                c++;
                if (c % 4 == 0)
                {
                    c = 0;
                    l++;
                }
            }
            x += 0.4f;

        }
    }

    public void colorDataPoints(Dictionary<string, List<DataPoint>> sortedData)
    {
        foreach (string key in sortedData.Keys)
        {
            Color c = new Color(Random.value, Random.value, Random.value);
            foreach (DataPoint point in sortedData[key])
            {
                GameObject vis = point.getVisu();
                point.setColor(c);
            }
        }
    }

    public void changeShape(Dictionary<string,List<DataPoint>> sortedData, Dictionary<string, GameObject> shapes)
    {
        foreach(string key in sortedData.Keys)
        {
            GameObject shape = shapes[key];
            
            foreach(DataPoint point in sortedData[key])
            {
                Transform oldVisu = point.getVisu().transform;
                GameObject visu = GameObject.Instantiate(shape);
                visu.transform.position = oldVisu.position;

                minNmax oldVisuProportions = GeometryAnalyzer.getMinNmax(point.getVisu());
                minNmax shapeProportions = GeometryAnalyzer.getMinNmax(visu);
                float newScale = GeometryAnalyzer.getScaleAdjustment(oldVisuProportions, shapeProportions);
                visu.transform.localScale = visu.transform.localScale * newScale;//new Vector3(visu.transform.localScale.x * newScale.x, visu.transform.localScale.y * newScale.y, visu.transform.localScale.z * newScale.z);
                visu.transform.SetParent(gameObject.transform);
                point.replaceVisu(visu);
            }
        }
    }

}
