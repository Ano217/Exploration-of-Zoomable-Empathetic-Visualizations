using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class VisualizationManager : MonoBehaviour
{
    public Transform player;
    public string path;

    public AbstractVisuMaker visuMaker;
    public ContextVisuMaker contextVisuMaker;
    public string xAxis;
    public string rangeSort;
    public float[] ranges;
    public string thirdAxis;
    public string[] shapesThirdAxisName;
    public GameObject[] shapesThirdAxisNameObjects;
    SceneData contextData;





    //private CSVReader reader;
    private Dictionary<string, List<DataPoint>> categories;
    protected CSVData myData;
    private int mode;
    private Dictionary<string, GameObject> shapes;

    // Start is called before the first frame update
    void Start()
    {
        //reader = new CSVReader();
        myData = CSVReader.readCVSFile(path);
        categories = myData.sortByNominal(xAxis);
        visuMaker.createBarGraph(categories);
        //categories = myData.sortByTimeRange(xAxis, TimeRange.Month);
        //visuMaker.createBarGraph(categories, true);
        mode = 0;

        shapes = new Dictionary<string, GameObject>();
        if (shapesThirdAxisName.Length>0 && shapesThirdAxisNameObjects.Length>=shapesThirdAxisName.Length)
        {
            for(int i=0; i<shapesThirdAxisName.Length; i++)
            {
                shapes.Add(shapesThirdAxisName[i], shapesThirdAxisNameObjects[i]);
            }
        }
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        updateAbstractVisu();
    }

    private void updateAbstractVisu()
    {
        if (mode < 1 && Vector3.Distance(player.position, visuMaker.transform.position) < 3f)
        {
            mode = 1;
            Dictionary<string, List<DataPoint>> secondCategories = myData.sortByRange(rangeSort, ranges);
            visuMaker.colorDataPoints(secondCategories);
        }
        if (mode < 2 && Vector3.Distance(player.position, visuMaker.transform.position) < 2f)
        {
            mode = 2;
            Debug.Log("thrid level");
            Dictionary<string, List<DataPoint>> thirdCategories = myData.sortByNominal(thirdAxis);
            visuMaker.changeShape(thirdCategories, shapes);
        }
        if (mode < 3 && Mathf.Abs(player.position.z - visuMaker.transform.position.z) < 0.01f)
        {
            mode = 3;
            Debug.Log("Switch scene");
            SceneManager.LoadScene("ContextScene");
            //LoadScene("ContextScene");
        }
    }

    private void startContextVisualization()
    {
        if (contextData != null)
        {
            player.position = contextData.cameraPosition.position;
            player.rotation = contextData.cameraPosition.rotation;
        }
        else Debug.Log("SceneData component not found");
        
        if (contextVisuMaker == null) Debug.Log("Context visu maker not found");

        Dictionary<string, List<DataPoint>> nomimalData = myData.sortByNominal(contextData.variable);
        
        contextVisuMaker.createContext(nomimalData, contextData.getMinNMax(), contextData.getShapes());
    }

    public void setContextVisuMaker(ContextVisuMaker ctxtVisMk)
    {
        Debug.Log("Add context visu maker");
        contextVisuMaker = ctxtVisMk;
        if (contextData != null)
        {
            startContextVisualization();
        }
    }

    public void setSceneData(SceneData sceneDatas)
    {
        Debug.Log("Add sceneData");
        contextData = sceneDatas;
        if (contextData != null)
        {
            startContextVisualization();
        }
    }

    private IEnumerator LoadScene(string sceneName)
    {
        // code from: https://stackoverflow.com/questions/52722160/in-unity-after-loadscene-is-there-common-way-to-wait-all-monobehaviourstart-t
        // Start loading the scene
        AsyncOperation asyncLoadLevel = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        // Wait until the level finish loading
        while (!asyncLoadLevel.isDone)
            yield return null;
        // Wait a frame so every Awake and Start method is called
        yield return new WaitForEndOfFrame();
    }
}
