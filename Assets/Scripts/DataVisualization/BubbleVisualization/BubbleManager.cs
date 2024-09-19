using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SortType { Nominal, TimeRange, NumericalRange, IndividualAssociation }
public enum SortModif { Bubbles, Color, Shape}
public enum TimeRange { Day, Month, Year }

public enum TriggerType { Collision, Distance, Touch}

[System.Serializable]
public struct SortDescription
{
    public string columnName;
    [HideInInspector] public int columnID;
    public SortType sortType;
    public SortModif sortModif;
    public float[] numericalRangesOptional;
    public TimeRange timeRangeOptional;
    public ShapeAssociation[] shapes;
    public TriggerType triggerType;
    
}

// serializable association category-shape
[System.Serializable]
public struct ShapeAssociation
{
    public string categoryName;
    public GameObject[] prefab;
}

public class BubbleManager : MonoBehaviour
{
    // Data
    public string dataPath;
    private CSVData allData;

    // Bubbles
    private Bubble masterBubble;
    public BubbleMaker bubbleMaker;

    // Sortings
    public SortDescription[] sortings;
    private int level = 0;



    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        // Read the data on the csv file
        //CSVReader reader = new CSVReader();
        CSVData allData = CSVReader.readCVSFile(dataPath);
        
        for(int i=0; i<sortings.Length; i++)
        {
            sortings[i].columnID = allData.getColumnID(sortings[i].columnName);
        }

        // Create bubbles
        if (sortings.Length > 0)
        {
            masterBubble = new Bubble(bubbleMaker, "master", allData.getAllData(), 0, sortings);
            bubbleMaker.createBubbleVisu(masterBubble);
            masterBubble.setTrigger(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void zoomIn(List<DataPoint> datas)
    {
        level++;
        Debug.Log("Zoom in");
        SceneManager.LoadScene("ContextScene");
    }
}
