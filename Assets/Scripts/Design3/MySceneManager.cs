using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.WSA;
using Valve.VR;
using Valve.VR.InteractionSystem;
using static UnityEngine.GraphicsBuffer;


public enum Type360Visu { Image360, UnityScene}
public struct SortedData
{
    public Dictionary<string, List<DataPoint>> categories;
    public string[] categoriesOrder;
}

[System.Serializable]
public struct SortDescriptionD3
{
    public string title;
    public string columnName;
    [HideInInspector] public int columnID;
    public SortType sortType;
    public float[] numericalRangesOptional;
    public TimeRange timeRangeOptional;
    public ShapeAssociation[] shapes;
}

public struct SortDescrAndData
{
    public Dictionary<string, List<DataPoint>> data;
    public SortDescriptionD3 sortDesr;
}

public class MySceneManager : MonoBehaviour
{
    [HideInInspector] public string dataPath;
    [HideInInspector] public static MySceneManager Instance;

    // Barchart
    [HideInInspector] public SortDescriptionD3 barChartOrganization;
    [HideInInspector] public List<SortDescriptionD3> barChartDescriptions;
    private int _selectedBarchartDescription = 0;

    // Unit visu
    [HideInInspector] public float tileCovering;
    [HideInInspector] public GameObject visuPrefab;

    // HumanVisu
    [HideInInspector] public SortDescriptionD3 humanVisualization;
    [SerializeField, HideInInspector] public List<PrefabHolder> humanVisShapeAssociation;
    public GameObject defaultHumanViz;

    // 360 Visu
    [HideInInspector] public Type360Visu immersiveVisu;
    [HideInInspector] public int immersiveLocationField;
    [HideInInspector] public string immesiveSceneName;
    [HideInInspector] public bool sceneIsUnique;
    [HideInInspector] public ImageSource typeImmersiveImage;
    [HideInInspector] public int coordinatesField;

    // PANEL
    [SerializeField, HideInInspector] public List<int> title;
    [HideInInspector] public bool displayImage;
    [HideInInspector] public ImageSource imageSource;
    [HideInInspector] public int imageID;
    [SerializeField, HideInInspector] public List<PanelInfoField> infoFields;
    //[SerializeField, HideInInspector] public PanelInspector panelInspector;

    //public GameObject view360Sphere;

    [HideInInspector] public CSVData csvData;
    private VisuMakerD3 visuMaker;
    private int level = 0;
    private Dictionary<string, List<DataPoint>> _myData;

    // Find in Resources
    private GameObject _handBasculeButton;
    private GameObject _handExit360ViewButton;
    private Material _skybox;
    private Texture _defaultSky;
    private Texture _textureView360;
    private GameObject _avatarPanel;
    private ImmersiveSphere _currentImmersiveSphere;

    // Find in scene
    private DesktopInteractions _desktopInteraction;
    private GameObject _loadingPanel;

    [HideInInspector] public Transform player;
    private Transform _rightHand;
    private Transform _leftHand;
    private GameObject _rightGrab;
    private GameObject _leftGrab;
    private GameObject _rightPointerGrab;
    private GameObject _leftPointerGrab;

    // Teleportation
    //private bool _teleportationMode = false;
    private Transform _teleportationTransform;
    private float _startPanTime;
    private Vector3 _startPanPosition;
    private bool _postStretch;

    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        player = GameObject.Find("Player").transform;
        _rightHand = GameObject.Find("RightHand").transform;
        _leftHand = GameObject.Find("LeftHand").transform;
        if(_desktopInteraction==null) _desktopInteraction = GameObject.FindFirstObjectByType<DesktopInteractions>();

        // Get in Resources
        _skybox = Resources.Load<Material>("Materials_Textures/DefaultSky");
        _defaultSky = Resources.Load<Texture>("Materials_Textures/sky2Background");
        _textureView360 = Resources.Load<Texture>("Materials_Textures/Nice");
        _avatarPanel = Resources.Load<GameObject>("UI/AvatarPanel");


        // Get elements in the scene
        visuMaker = GameObject.FindFirstObjectByType<VisuMakerD3>();
        if (csvData == null)
        {
            Debug.Log("Make data");
            makeData();
        }
        // myData = calculateData(csvData.getAllData(), barChartOrganization);
        var currentDescription = getCurrentBarchartDescription();
        _myData = calculateData(csvData.getAllData(), currentDescription);

        makePrefabAssociation();
        // visuMaker.startBarChartVisu(myData, barChartOrganization.sortType, defaultHumanViz, _avatarPanel, barChartOrganization.title);

        if (barChartDescriptions.Count > 1)
        {
            var sortButtons = new string[barChartDescriptions.Count];
            for(int i=0; i<barChartDescriptions.Count; i++)
            {
                Debug.Log(barChartDescriptions[i].columnName);
                sortButtons[i] = barChartDescriptions[i].columnName;
            }
            visuMaker.startBarChartVisu(_myData, currentDescription.sortType, defaultHumanViz, _avatarPanel, currentDescription.title, sortButtons);
        }
        else visuMaker.startBarChartVisu(_myData, currentDescription.sortType, defaultHumanViz, _avatarPanel, currentDescription.title);

        _loadingPanel = GameObject.Find("LoadingPanel");

        visuMaker.getStep += getSortLevel;
        visuMaker.enableBasculeButton += enableBasculeButton;
        visuMaker.loadingends += OnLoadingEnds;

        //makeButtons();

        
    }

    public SortDescriptionD3 getCurrentBarchartDescription()
    {
        return barChartDescriptions[_selectedBarchartDescription];
    }

    public void makeData(){ csvData = CSVReader.readCVSFile(dataPath); }
    
    public void enableExitButton(bool enable)
    {
        //_handExit360ViewButton.SetActive(enable);
        _desktopInteraction.enableExitButton(enable);
    }

    public void enableBasculeButton(bool enable)
    {
        //_handBasculeButton.SetActive(enable);
        _desktopInteraction.enableBasculeHint(enable);
    }

    public SortDescrAndData getSortLevel(int level)
    {
        SortDescrAndData res = new SortDescrAndData();
        switch (level)
        {
            case 0:
                res.data = calculateData(csvData.getAllData(), getCurrentBarchartDescription());//myData;
                //res.sortDesr = barChartOrganization;
                res.sortDesr = getCurrentBarchartDescription();
                break;
            case 1:
                res.data = calculateData(csvData.getAllData(), humanVisualization);
                res.sortDesr = humanVisualization;
                break;
        }
        return res;
    }



    private void enableInteractions()
    {
        if (_desktopInteraction == null) _desktopInteraction = GameObject.FindFirstObjectByType<DesktopInteractions>();
        _desktopInteraction.zoom += OnZoom;
        _desktopInteraction.keyPressed += onKeyPressed;
        _desktopInteraction.stretch += startStretching;
        _desktopInteraction.release += release;
        _desktopInteraction.bascule += layDown;
        _desktopInteraction.grab += grabObject;
        _desktopInteraction.exitView += switch360View;
        _desktopInteraction.pan += startPan;
        _desktopInteraction.grabHead += grabHead;
    }

    private void OnDisable()
    {
        _desktopInteraction.zoom -= OnZoom;
        _desktopInteraction.keyPressed -= onKeyPressed;
        _desktopInteraction.stretch -= startStretching;
        _desktopInteraction.release -= release;
        _desktopInteraction.bascule -= layDown;
        _desktopInteraction.exitView -= switch360View;
        _desktopInteraction.pan -= startPan;
        visuMaker.getStep -= getSortLevel;
        //visuMaker.enableBasculeButton -= enableBasculeButton;
        visuMaker.loadingends -= OnLoadingEnds;
        _desktopInteraction.grabHead -= grabHead;

        changeSkybox(true);
    }

    public Dictionary<string, List<DataPoint>> calculateData(List<DataPoint> dataPoints, SortDescriptionD3 sort)
    {
        switch (sort.sortType)
        {
            case SortType.Nominal:
                return DataSorter.sortByNominal(dataPoints, sort.columnID);
            case SortType.NumericalRange:
                return DataSorter.sortByRange(dataPoints, sort.columnID, sort.numericalRangesOptional);
            case SortType.TimeRange:
                return DataSorter.sortByTimeRange(dataPoints, sort.columnID, sort.timeRangeOptional);
            case SortType.IndividualAssociation:
                Debug.Log("Calculate data for individual association:");
                var res = new Dictionary<string, List<DataPoint>>();
                res.Add("sort", dataPoints);
                return res;
            default:
                return null;
        }
    }

    public void onKeyPressed(KeyCode k)
    {
        switch (k)
        {
            case KeyCode.Keypad0:
                // default
                changeBarchartSort(0);
                break;
            case KeyCode.Keypad1:
                changeBarchartSort(1);
                break;
            case KeyCode.Keypad2:
                changeBarchartSort(2);
                break;
            case KeyCode.R:
                visuMaker.resetVisualization();
                break;
        }

    }

    public void endTransition(int level)
    {
        humanVisualization.columnID = csvData.getColumnID(humanVisualization.columnName);
        if (level == 0) visuMaker.startHumanoidVisualization(humanVisualization, _myData);
    }

    public void OnZoom(float zoom)
    {
        //visuMaker.zoom(zoom/10);
    }

    public void startStretching(GameObject target1, GameObject target2, Transform ref1, Transform ref2)
    {
        //_teleportationMode = false;
        visuMaker.stopPan(Constants.LEFTPOINTER);
        visuMaker.stopPan(Constants.RIGHTPOINTER);
        var cond1 = target1 == null && target2 == null;

        if (cond1 || (target1.transform.IsChildOf(visuMaker.visualisation.transform) && target2.transform.IsChildOf(visuMaker.visualisation.transform)))
        {
            visuMaker.setStretch(true);
            visuMaker.setStretchReferences(ref1, ref2);
        }
    }

    public void startPan(GameObject target, Transform hitPointer, int panningLaser, bool postStretch = false)
    {
        if (target.transform.IsChildOf(visuMaker.visualisation.transform))
        {
            _teleportationTransform = hitPointer;
            visuMaker.startPan(hitPointer, panningLaser);
            _startPanTime = Time.time;
            _startPanPosition = hitPointer.position;
            _postStretch = postStretch;

            _teleportationTransform = hitPointer;
            visuMaker.startPan(hitPointer, panningLaser);
        }
        else
        {
            Debug.Log("Not clicking in visualisation");
        }
    }

    public void release(int holder, GameObject target = null)
    {
        if(holder==Constants.RIGHTPOINTER || holder == Constants.LEFTPOINTER)
        {
            visuMaker.stopPan(holder);
            visuMaker.setStretch(false);

            var grabTime = Time.time - _startPanTime;
            var pointerPosition = _teleportationTransform.position;
            var grabDist = Vector3.Distance(_startPanPosition, pointerPosition);
            if (grabTime < 0.4f && grabDist < 0.3f && !_postStretch)
            {
                if (target != null)
                {
                    //Debug.Log("Teleport => scale = " + visuMaker.getVisualizationScale());
                    SortButton btn;
                    var foundBtn = target.TryGetComponent<SortButton>(out btn);
                    if (foundBtn)
                    {
                        var sortNb = btn.getSortNb();
                        Debug.Log("click btn n°" + sortNb);
                        changeBarchartSort(sortNb);
                    }
                    else visuMaker.teleport(pointerPosition);
                }
                // else do nothing

            }
        }
        releaseGrabObjects(holder);
    }


    public void layDown()
    {
        if (visuMaker.getVisuStatus() < 4)
        {
            release(Constants.RIGHTPOINTER);
            release(Constants.LEFTPOINTER);
            visuMaker.bascule();
        }
        //visuMaker.layDown();
    }

    public void grabObject(GameObject target, int attachConst)
    {
        // Check if it is a sphere
        TagsManager tagManager;
        var foundTagManager = target.TryGetComponent<TagsManager>(out tagManager);
        if (foundTagManager)
        {
            if (tagManager.hasTag(CustomTags.Grabbable))
            {
                releaseGrabObjects(attachConst);
                switch (attachConst)
                {
                    case Constants.LEFTPOINTER:
                        target.transform.SetParent(_desktopInteraction.getPointerTip(attachConst).transform);
                        _desktopInteraction.getPointerTip(attachConst).transform.SetParent(_leftHand);
                        _leftPointerGrab = target;
                        break;
                    case Constants.RIGHTPOINTER:
                        target.transform.SetParent(_desktopInteraction.getPointerTip(attachConst).transform);
                        _desktopInteraction.getPointerTip(attachConst).transform.SetParent(_rightHand);
                        _rightPointerGrab = target;
                        break;
                    case Constants.RIGHTHAND:
                        target.transform.SetParent(_rightHand);
                        _rightGrab = target;
                        break;
                    case Constants.LEFTHAND:
                        target.transform.SetParent(_leftHand);
                        _leftGrab = target;
                        break;
                }
            }
        }

        // Check if it is an avatar
        /*AvatarHandler avatarScript;
        var foundAvatarScript = target.TryGetComponent<AvatarHandler>(out avatarScript);
        if (foundAvatarScript) switch360View(true);*/
    }

    public void releaseGrabObjects(int attactConst)
    {
        ImmersiveSphere immersiveSphere = null;
        bool isSphere = false;
        GameObject gameObject = null;
        ref GameObject grab = ref gameObject;
        if (attactConst == Constants.RIGHTHAND && _rightGrab != null)
        {
            grab = _rightGrab;
        }
        else if(attactConst == Constants.LEFTHAND && _leftGrab != null)
        {
            grab = _leftGrab;
        }
        else if(attactConst == Constants.RIGHTPOINTER && _rightPointerGrab != null)
        {
            // Get right pointer tip
            grab = _rightPointerGrab;
        }
        else if(attactConst == Constants.LEFTPOINTER && _leftPointerGrab != null)
        {
            // Get let pointer tip
            grab = _leftPointerGrab;
        }
        if (grab != null)
        {
            grab.transform.SetParent(null);
            isSphere = grab.TryGetComponent<ImmersiveSphere>(out immersiveSphere);
            grab = null;
            if (isSphere) immersiveSphere.resetPosition();
            if (_currentImmersiveSphere == null)  _desktopInteraction.setLaserTip(attactConst);
        }
        else if(_currentImmersiveSphere != null)
        {
            _currentImmersiveSphere.resetPosition();
        }
    }

    public void switch360View(bool is360, ImmersiveSphere sphere, Texture texture = null)
    {
        
        _currentImmersiveSphere = sphere;
        releaseGrabObjects(Constants.LEFTHAND);
        releaseGrabObjects(Constants.RIGHTHAND);
        releaseGrabObjects(Constants.RIGHTPOINTER);
        releaseGrabObjects(Constants.LEFTPOINTER);
        visuMaker.launch360View(is360);
        _desktopInteraction.resetHandCollisions();
        changeSkybox(texture);
        _desktopInteraction.enableLaserTips(false);
        StartCoroutine(ButtonCoroutine(is360));
    }

    public void switch360View(bool is360)
    {
        _currentImmersiveSphere = null;
        _desktopInteraction.enableLaserTips(!is360);
        visuMaker.launch360View(is360);
        changeSkybox(!is360);
        StartCoroutine(ButtonCoroutine(is360));
    }

    public ImmersiveSphere getCurrentImmersiveSphere() { return _currentImmersiveSphere; }

    public void changeSkybox(bool isDefault)
    {
        
        _skybox.mainTexture = isDefault ? _defaultSky : _textureView360;
    }

    public void changeSkybox(Texture texture)
    {
        _skybox.mainTexture = texture;
    }

    IEnumerator ButtonCoroutine(bool is360)
    {
        if (is360)
        {
            //enableBasculeButton(!is360);
            yield return new WaitForSeconds(3);
            enableExitButton(is360);
        }
        else
        {
            enableExitButton(is360);
            yield return new WaitForSeconds(5);
            //enableBasculeButton(!is360);
        }
    }

    public void OnLoadingEnds()
    {
        enableInteractions();
        _loadingPanel.SetActive(false);
    }

    public int getRandomPrefab(string categoryName)
    {
        var cpt = 0;
        foreach(var prefHolder in humanVisShapeAssociation)
        {
            //Debug.Log("Prefab holder name=" + prefHolder.associatedName);
            var nMax = prefHolder.prefabs.Count;
            if (prefHolder.associatedName == categoryName)
            {
                var id = Random.Range(0, nMax);
                //Debug.Log("Id = " + id+"   " + prefHolder.prefabs[id].name);
                return cpt + id; // For ECS, all prefabs will be held in a single tab
            }
            else
            {
                cpt += nMax;
            }
        }
        //Debug.Log("Not found with category name " + categoryName);
        return -1;
    }

    public GameObject getPrefab(int prefabID)
    {
        var cpt = 0;
        foreach (var prefHolder in humanVisShapeAssociation)
        {
            //Debug.Log("Prefabholder " + prefHolder.associatedName);
            //Debug.Log("PrefHolder " + prefHolder.associatedName + " has " + prefHolder.prefabs.Count + " elements");
            var nMax = prefHolder.prefabs.Count;
            var idx = prefabID - cpt;
            if (idx >= nMax)
            {
                cpt += nMax;
            }
            else
            {
                if (prefHolder.prefabs[idx] != null)
                {
                    var pref = prefHolder.prefabs[idx];
                    //Debug.Log("Id=" + prefabID + "   Name=" + pref.name+ "  idx = "+idx);
                    return pref;
                }
                //else Debug.Log("prefab[" + idx + "] is null");
            }
        }
        return defaultHumanViz;
    }

    public void makePrefabAssociation()
    {
        var sortAssociation = getSortLevel(1);

        if (sortAssociation.sortDesr.sortType != SortType.IndividualAssociation)
        {
            foreach (var (cat, data) in sortAssociation.data)
            {
                //Debug.Log("Catégorie:" + cat+ "  length="+data.Count);
                foreach (var point in data)
                {
                    var id = getRandomPrefab(cat);
                    point.setPrefabID(id);
                }
            }
        }
        else
        {
            Debug.Log("Make individual associations");
            humanVisShapeAssociation = new List<PrefabHolder>();
            var columnID = sortAssociation.sortDesr.columnID;
            var prefabs = new List<GameObject>();
            Debug.Log("ColumnID=" + columnID);
            var cpt = 0;
            Dictionary<string, int> prefabsNamesAssociation = new Dictionary<string, int>();
            foreach (var (cat, data) in sortAssociation.data)
            {
                Debug.Log("Cat " + cat);
                //Debug.Log("Catégorie:" + cat+ "  length="+data.Count);
                foreach (var point in data)
                {
                    //var id = getRandomPrefab(cat);
                    var prefabName = point.data[columnID];
                    
                    if (!prefabsNamesAssociation.ContainsKey(prefabName))
                    {
                        //var idPref = prefabs.Count;
                        prefabsNamesAssociation.Add(prefabName, cpt);
                        var prefab = (GameObject)Resources.Load<GameObject>(prefabName);
                        if (prefab == null) Debug.Log("Error: " + prefabName + " not found");
                        prefabs.Add(prefab);
                        cpt++;
                    }
                    var id = prefabsNamesAssociation[prefabName];
                    //Debug.Log(id + " : " + prefabName);
                    
                    point.setPrefabID(id);
                }
            }
            PrefabHolder prefabHolder = new PrefabHolder
            {
                associatedName = "IndividualAssociations",
                prefabs = prefabs,
            };
            humanVisShapeAssociation.Add(prefabHolder);
        }
        
    }

    public int getNbPoints()
    {
        return csvData.getSize();
    }

    public int getPointPrefabID(int pointID)
    {
        var p = csvData.GetPoint(pointID);
        return p.getPrefabId();
    }

    public Vector3 getPointPosition(int pointID)
    {
        var p = csvData.GetPoint(pointID);
        return p.getPointPosition();    
    }

    public int getNbPrefabs()
    {
        var cpt = 0;
        foreach (var prefHolder in humanVisShapeAssociation)
        {
            cpt += prefHolder.prefabs.Count;
        }
        return cpt;
    }

    public int getVisuStatus()
    {
        return visuMaker.getVisuStatus();
    }

    public Vector3 getCenterOfAttention()
    {
        var lp = _desktopInteraction.getPointerPosition(Constants.LEFTPOINTER);
        var rp = _desktopInteraction.getPointerPosition(Constants.RIGHTPOINTER);

        return new Vector3((lp.x + rp.x) / 2, (lp.y + rp.y) / 2, (lp.z + rp.z) / 2);
    }

    public void changeBarchartSort(int sortNb)
    {
        if (sortNb < barChartDescriptions.Count && sortNb != _selectedBarchartDescription)
        {
            var oldSelected = _selectedBarchartDescription;
            _selectedBarchartDescription = sortNb;
            SortDescrAndData sortDescrAndData = getSortLevel(0);

            // Call modification function in VisuMakerD3
            var canReorganize = visuMaker.reorganizeBarchart(sortDescrAndData.data, sortDescrAndData.sortDesr.sortType, sortDescrAndData.sortDesr.title, sortNb);

            // if modifications can be done
            if (canReorganize)
            {
                _myData = sortDescrAndData.data;
            }
            else
            {
                _selectedBarchartDescription = oldSelected;
            }
        }
    }

    public void grabHead(int hand)
    {
        if (_currentImmersiveSphere != null)
        {
            var vrCamera = GameObject.Find("VRCamera");
            Debug.Log("SM: Grab head");
            if (hand == Constants.RIGHTHAND)
            {
                _currentImmersiveSphere.AttachToHand(_rightHand.gameObject, vrCamera.transform.position);
                _rightGrab = _currentImmersiveSphere.gameObject;
            }
            else if (hand == Constants.LEFTHAND)
            {
                _currentImmersiveSphere.AttachToHand(_leftHand.gameObject, vrCamera.transform.position);
                _leftGrab = _currentImmersiveSphere.gameObject;
            }
        }
        else Debug.Log("No current immersive scene");
        
    }

}
