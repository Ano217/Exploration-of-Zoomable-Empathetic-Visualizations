using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

public class HumanVizManager : MonoBehaviour
{
    private Dictionary<string, GameObject> bars;
    private GameObject floor;
    private bool growFloor = false;
    private float targetHeight;

 
    public void createVisualization()
    {
        // Create the floor
        Debug.Log("Creating the floor");
        floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        Vector3 userPos = FindAnyObjectByType<PlayerMove>().transform.position;
        floor.transform.position = new Vector3(userPos.x, -3f, userPos.z);
        floor.transform.parent = this.transform;
        floor.transform.localScale = new Vector3(0.0001f, 0.0001f, 0.0001f);
        Renderer floorRenderer = floor.GetComponent<Renderer>();
        floorRenderer.material = Resources.Load("GreenGrass") as Material;
        growFloor = true;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (growFloor) floor.transform.localScale = floor.transform.localScale * 1.005f;
    }


    public void startMakingHumans(SortDescrAndData sorting)
    {
        Dictionary<string, GameObject[]> shapesDict = new Dictionary<string, GameObject[]>();
        foreach (ShapeAssociation s in sorting.sortDesr.shapes)
        {
            shapesDict.Add(s.categoryName, s.prefab);
        }

        AnimatorController animCtrl = Resources.Load("idleAnim") as AnimatorController;
        foreach (string k in sorting.data.Keys)
        {
            foreach (DataPoint p in sorting.data[k])
            {
                if (shapesDict.ContainsKey(k))
                {
                    int shapeNum = (int)Random.Range(0, shapesDict[k].Length);
                    GameObject human = Instantiate(shapesDict[k][shapeNum]);
                    Vector3 pos = p.getVisu().transform.position;
                    Vector3 scale = p.getVisu().transform.localScale;
                    minNmax dim = GeometryAnalyzer.getMinNmax(human);
                    targetHeight = dim.max.y - dim.min.y;

                    human.transform.position = new Vector3(pos.x, pos.y, pos.z);

                    
                    
                    Vector3 size = human.transform.localScale;
                    human.transform.localScale = human.transform.localScale * 0.1f;
                    //human.transform.localRotation = Quaternion.Euler(90, 180, 0);
                    human.transform.parent = this.transform;
                    human.transform.localRotation = Quaternion.Euler(90, 180, 0);
                    //human.transform.localRotation.eulerAngles.Set(90, 180, 0);



                    p.replaceVisu(human);

                    // Add animation to the avatar
                    Animator animator = human.GetComponent<Animator>();
                    if (animator == null)
                    {
                        human.AddComponent<Animator>();
                    }
                    animator.runtimeAnimatorController = animCtrl as RuntimeAnimatorController;
                    AvatarController avatarCtrl = human.AddComponent<AvatarController>();
                    //avatarCtrl.startGrowing(size);
                    //avatarCtrl.startBeingVisible();
                }
            }
        }
    }


    public  void startMakingHumans(Dictionary<string, List<DataPoint>> data, SortDescriptionD3 sortDescr, Dictionary<string, BarScript> oldBars)
    {
        growFloor = false;
        // Turn ShapeAssociation into a dictionary
        Dictionary<string, GameObject[]> shapesDict = new Dictionary<string, GameObject[]>();
        foreach (ShapeAssociation s in sortDescr.shapes)
        {
            shapesDict.Add(s.categoryName, s.prefab);
        }

        // Create Humans
        int colID = sortDescr.columnID;
        foreach (string key in data.Keys)
        {
            minNmax dim = GeometryAnalyzer.getMinNmax(oldBars[key].gameObject);
            Vector3 start = dim.min;
            float stopZ = dim.max.z;

            float x = start.x;
            float y = floor.transform.position.y;
            float z = start.z;

            AnimatorController animCtrl = Resources.Load("idleAnim") as AnimatorController;
            

            foreach (DataPoint p in data[key])
            {
                string value = p.getColumnValue(colID);
                if (shapesDict.ContainsKey(value))
                {
                    int shapeNum = (int)Random.Range(0, shapesDict[value].Length);
                    GameObject human = Instantiate(shapesDict[value][shapeNum]);
                    p.replaceVisu(human);

                    human.transform.position = new Vector3(x, y, z);
                    z += 1.5f;
                    if (z > stopZ)
                    {
                        z = start.z;
                        x += 1.5f;
                    }
                    human.transform.parent = this.transform;
                    human.transform.Rotate(new Vector3(0f, 180f, 0f));
                    Vector3 size = human.transform.localScale;
                    //human.transform.localScale = human.transform.localScale * 0.01f;


                    // Add animation to the avatar
                    Animator animator = human.GetComponent<Animator>();
                    if (animator == null)
                    {
                        human.AddComponent<Animator>();
                    }
                    animator.runtimeAnimatorController = animCtrl as RuntimeAnimatorController;
                    AvatarController avatarCtrl = human.AddComponent<AvatarController>();
                    //avatarCtrl.startGrowing(size);
                    //avatarCtrl.startBeingVisible();
                }
            }
        }
    }
}
