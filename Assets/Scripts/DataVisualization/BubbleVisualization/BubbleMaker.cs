using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleMaker : MonoBehaviour
{
    public Transform player;
    public GameObject defaultShape;

    private float r = 7.0f;
    private float r2 = 1.5f;
    private float speed = 10f;
    
    public void createBubbleVisu(Bubble bubble)
    {
        switch (bubble.getSortDescription().sortModif)
        {
            case SortModif.Bubbles:
                createBubbles(bubble);
                break;
            case SortModif.Color:
                modifyColor(bubble);
                break;
            case SortModif.Shape:
                modifyShape(bubble);
                break;
        }
    }

    private void createBubbles(Bubble bubble)
    {
        SortDescription sort = bubble.getSortDescription();
        Dictionary<string, List<DataPoint>> datas = bubble.getData();

        int nbBubles = datas.Count;

        float theta;
        if (nbBubles > 3) theta = (2 * Mathf.PI) / (nbBubles - 1);
        else theta = Mathf.PI / (nbBubles - 1);

        bool createShape = true;
        GameObject shape;
        if (sort.shapes.Length > 0) shape = sort.shapes[0].prefab[0];
        else shape = defaultShape;
        createShape = !bubble.getHasParent();
        float angle = 0;
        foreach (string key in datas.Keys)
        {
            // Create BubbleSphere
            Vector3 position = new Vector3(r * Mathf.Cos(angle) + player.position.x, player.position.y, r * Mathf.Sin(angle) + player.position.z);
            angle += theta;
            GameObject bubbleSphere = new GameObject();
            bubbleSphere.transform.position = position;
            bubbleSphere.gameObject.name = key;

            // Collider & rigidbody
            CapsuleCollider collider = bubbleSphere.AddComponent<CapsuleCollider>();
            collider.transform.localScale = new Vector3(r2, r2, r2);
            collider.isTrigger = true;
            Rigidbody rigidBody = bubbleSphere.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;
            rigidBody.useGravity = false;


            BubbleVisu bblVisu = bubbleSphere.AddComponent<BubbleVisu>();
            bubbleSphere.transform.parent = this.transform;
            bblVisu.setBubbles(bubble, bubble.getChild(key));
            bubble.getChild(key).setVisualization(bblVisu);

            // For pointPosition
            foreach (DataPoint point in datas[key])
            {
                float rdX = Random.Range(-r2, r2);
                float maxZ = Mathf.Sqrt(r2 * r2 - rdX * rdX);
                float rdZ = Random.Range(-maxZ, maxZ);
                float d = Mathf.Sqrt(rdX * rdX + rdZ * rdZ);
                float maxY = Mathf.Sqrt(r2 * r2 - d * d);
                float rdY = Random.Range(-maxY, maxY);
                GameObject visu;
                Vector3 visuPos = new Vector3(position.x + rdX, position.y + rdY, position.z + rdZ);
                if (createShape)
                {
                    visu = GameObject.Instantiate(shape);
                    visu.AddComponent<DataPointHandler>();
                    point.replaceVisu(visu);
                    visu.transform.position = visuPos;
                }
                else
                {
                    point.moveVisuTo(visuPos);
                    visu = point.getVisu();
                }
                visu.transform.parent = bubbleSphere.transform;
                bblVisu.addPoint(visu);
            }
        }
    }


    private void modifyColor(Bubble bubble)
    {
        Dictionary<string, List<DataPoint>> datas = bubble.getData();
        int nbColors = datas.Count;
        float gap = 1.0f / nbColors;
        float h = 0f;
        foreach (string key in datas.Keys)
        {
            Color c = Color.HSVToRGB(h, 1f, 1f);
            foreach (DataPoint point in datas[key])
            {
                GameObject vis = point.getVisu();
                point.setColor(c);
            }
            h += gap;
        }
    }

    private void modifyShape(Bubble bubble)
    {
        Dictionary<string, List<DataPoint>> datas = bubble.getData();
        SortDescription description = bubble.getSortDescription();
        ShapeAssociation[] shapes = description.shapes;
        foreach(string key in datas.Keys)
        {
            GameObject shape = StructTools.findShape(shapes, key);
            if (shape != null)
            {
                foreach(DataPoint p in datas[key])
                {
                    Transform oldVisu = p.getVisu().transform;
                    GameObject instantiated = Instantiate(shape);
                    instantiated.transform.position = oldVisu.position;

                    // Scale
                    minNmax oldVisuProportions = GeometryAnalyzer.getMinNmax(p.getVisu());
                    minNmax shapeProportions = GeometryAnalyzer.getMinNmax(instantiated);
                    float newScale = GeometryAnalyzer.getScaleAdjustment(oldVisuProportions, shapeProportions);
                    instantiated.transform.localScale = instantiated.transform.localScale * newScale;

                    instantiated.transform.parent = oldVisu.parent;
                    p.replaceVisu(instantiated);
                }
            }
            
        }
    }

    


}
