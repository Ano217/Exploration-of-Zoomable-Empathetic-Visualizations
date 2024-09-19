using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using TMPro;
using Unity.Burst;
using Unity.VisualScripting;
using UnityEngine;
using Valve.VR;
using static UnityEngine.GraphicsBuffer;

public class BarChartManager : MonoBehaviour
{
    [HideInInspector] public delegate void DelegateLoadingEnds();
    [HideInInspector] public DelegateLoadingEnds loadingends;

    const float maxHeight = 2.0f;
    const float width = 0.3f;
    const float depth = 0.05f;
    const float gap = 0.15f;
    const float rotationSpeed = 15f;
    const float xStartPosition = -1.5f;
    public Vector3 down = new Vector3(0f, -0.001f, 0f);


    //private bool pivot = false;
    private bool disapearing = false;
    private int maxNbInd = 0;
    
    private Dictionary<string, BarScript> _barDict;
    private Dictionary<string, List<DataPoint>> _dataSource;
    private Dictionary<string, GameObject> _labels;
    private List<GameObject> _miniLabels;
    private GameObject _barChartTitle;
    private SortSelector _sortSelector;

    private bool unitCreated;
    private bool humansCreated = false;
    public GameObject defaultHumans;
    public GameObject avatarPanel;

    private bool fadeBars = false;
    private bool fadeUnits = false;
    private Vector3 _targetPointScale;
    private Vector3 _targetCapsuleScale = new Vector3(0.1f, 0.1f, 0.31f);

    // waiting variables
    private bool _morphersAttributed = false;
    private bool _waitForUnitVisu = false;
    private bool _waitForAvatars = false;

    // Cube => Sphere => Avatar
    private bool _morphCubeToSphere = false;
    private bool _morphSphereToCapsule = false;
    private bool _morphCapsuletoAvatar = false;

    // Avatar => Sphere => Cube
    private bool _morphAvatarToCapsule = false;
    private bool _morphCapsuleToSphere = false;
    private bool _morphSphereToCube = false;

    public void createBarChart(Dictionary<string,List<DataPoint>> data, SortType sortType,  string title, string[] sortButtons = null)
    {
        _dataSource = data;
        /*foreach(string k in data.Keys)
        {
            if (data[k].Count > maxNbInd) maxNbInd = data[k].Count;
        }*/
        float coef = getBarChartCoef(data);//maxHeight / maxNbInd;

        //Create bars
        GameObject textLabel = Resources.Load("UI/TextLabel") as GameObject;
        GameObject titlePrefab = Resources.Load("UI/TitleCanvas") as GameObject;
        GameObject sortSelectorPrefab = Resources.Load("UI/SortSelector") as GameObject;


        // Title
        _barChartTitle = GameObject.Instantiate<GameObject>(titlePrefab);
        _barChartTitle.transform.SetParent(this.transform);
        _barChartTitle.transform.localPosition = new Vector3(0, maxHeight + 0.5f, -0.01f);
        var textMesh = _barChartTitle.GetComponentInChildren<TextMeshProUGUI>();
        if(textMesh != null) { textMesh.text = title; }


        _barDict = new Dictionary<string, BarScript>();
        _labels = new Dictionary<string, GameObject>();

        var nbBars = data.Count;
        float x = xStartPosition; //getBarXPosition(nbBars);

        // Sort Selector
        if(sortButtons!= null)
        {
            var sortSelector = GameObject.Instantiate<GameObject>(sortSelectorPrefab);
            sortSelector.transform.SetParent(this.transform);
            sortSelector.transform.localPosition = new Vector3(xStartPosition - 1, 0.5f, -0.01f);
            sortSelector.tag = "Interactive";

            _sortSelector = sortSelector.GetComponent<SortSelector>();
            _sortSelector.createButtons(sortButtons);
            //_sortSelector.selectedButton(0);
        }
        


        /*if (nbBars % 2 == 0)
        {
            x = -width * nbBars / 2 + 0.5f * width - gap * nbBars / 2 + 0.5f * gap;
        }
        else
        {
            x = -(width + gap) * (nbBars / 2);
        }*/

        string previousValue = "0";
        foreach(string k in data.Keys)
        {
            GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var barScript = bar.AddComponent<BarScript>();
            barScript.value = data[k].Count;

            Material barMat;
            var foundBarMat = bar.TryGetComponent<Material>(out barMat);
            if(!foundBarMat)
            {
                var barRenderer = bar.GetComponent<Renderer>();
                barRenderer.material = Resources.Load<Material>("Materials_Textures/BarMaterial");
            }

            bar.tag = "Interactive";
            float h = coef * data[k].Count;

            bar.transform.localScale = new Vector3(width, h, depth);
            //bar.transform.position = new Vector3(x, 0.3f+(h/2), 0f);
            bar.transform.parent = this.transform;
            bar.transform.localPosition = new Vector3(x, (h / 2), 0f);

            // Add label
            GameObject barLabel = Instantiate(textLabel);
            _labels.Add(k, barLabel);
            barLabel.tag = "Interactive";
            TextLabelManager textManager = barLabel.GetComponent<TextLabelManager>();
            textManager.resize(0.2f, width);

            var textLabelContent = k;
            if (sortType == SortType.NumericalRange && k != "unknown")
            {
                textLabelContent = previousValue + "-" + k;
                previousValue = k;
            }
            //else Debug.Log("cond1=" + (sortType == SortType.NumericalRange) + "  cond2=" + k != "unknown");
            textManager.changeText(textLabelContent);
            textManager.setPivotX(width/2);
            //barLabel.transform.position = new Vector3(x, 0.05f, -0.01f);
            barLabel.transform.SetParent(this.transform);
            //barLabel.transform.localPosition = new Vector3(x, -0.2f, -0.01f);
            barLabel.transform.localPosition = new Vector3(x-width/2, -0.3f, -0.005f);
            barLabel.transform.localRotation = Quaternion.Euler(0, 0, 45f);



            x += width + gap;
            
            bar.name = k;
            _barDict.Add(k, barScript);

            Renderer r = bar.GetComponent<Renderer>();
            StructTools.ToFadeMode(r.material);
        }
        unitCreated = false;
        humansCreated = false;

        StartCoroutine(createGranularity());
    }

    // Update is called once per frame
    void Update()
    {
        /*if (pivot)
        {
            layDownChart();
            if (transform.rotation.eulerAngles.x == 90)
            {
                pivot = false;
                FindAnyObjectByType<MySceneManager>().endTransition(0);
            }
        }*/

        if(_waitForUnitVisu && unitCreated)
        {
            granuralize();
            _waitForUnitVisu = false;
        }
        if (_waitForAvatars && _morphersAttributed && humansCreated)
        {
            _waitForAvatars = false;
            turnToHumans();
        }

        if (fadeBars)
        {
            var targetAchieved = rescalePoints(0.02f, _targetPointScale.z, _targetPointScale.z, _targetPointScale.z);
            if (targetAchieved)
            {
                fadeBars = false;
            }
        }
        if (fadeUnits)
        {
            var targetAchieved = rescalePoints(0.02f, _targetPointScale.x, _targetPointScale.y, _targetPointScale.z);
            if (targetAchieved)
            {
                if (_sortSelector != null) _sortSelector.gameObject.SetActive(true);
                fadeUnits = false;
                enableBars(true);
                enableUnitPoints(false);
            }
            
        }

        // CUBE => CAPSULE => SPHERE => AVATAR
        
        if (_morphCubeToSphere)
        {
            var isDone = true;
            //var isDone = false;
            //isDone = setPointsMorphSliders(0.1f);
            if (isDone)
            {
                _morphCubeToSphere = false;
                //enableUnitMorphers(false);
                _morphSphereToCapsule = true;
            }
        }
        if (_morphSphereToCapsule)
        {
            var targetAchieved = true;
            //var targetAchieved = rescalePoints(0.1f, _targetCapsuleScale.x, _targetCapsuleScale.y, _targetCapsuleScale.z);
            //movePointsToAvatarCenter(0.11f);
            if (targetAchieved)
            {
                _morphSphereToCapsule = false;
                _morphCapsuletoAvatar = true;
                enableHumans(true);
            }
        }
        if (_morphCapsuletoAvatar)
        {
            var targetAchieved = rescalePoints(0.1f, 0.05f, 0.05f, 0.05f);
            if (targetAchieved)
            {
                _morphCapsuletoAvatar = false;
                enableUnitPoints(false);
            }
        }

        // AVATAR => CAPSULE => SPHERE => CUBE
        if (_morphAvatarToCapsule)
        {
            var targetAchieved = rescalePoints(0.1f, _targetCapsuleScale.x, _targetCapsuleScale.y, _targetCapsuleScale.z);
            if (targetAchieved)
            {
                _morphAvatarToCapsule = false;
                _morphCapsuleToSphere = true;
                enableHumans(false);
            }
        }
        if (_morphCapsuleToSphere)
        {
            var targetAchieved = true;
            //var targetAchieved = rescalePoints(0.1f, _targetPointScale.z, _targetPointScale.z, _targetPointScale.z);
            //movePointsToAvatarCenter(0f);
            if (targetAchieved)
            {
                _morphCapsuleToSphere = false;
                _morphSphereToCube = true;
                //enableUnitMorphers(true);
            }
        }
        if (_morphSphereToCube)
        {
            var isDone = true;
            //var isDone = false;
            //isDone = setPointsMorphSliders(-0.1f);
            if (isDone)
            {
                _morphSphereToCube = false;
                //enableUnitMorphers(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            switchBarsTransparency(true);
            setBarsTransparency(-0.01f);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            setBarsTransparency(-0.01f);
        }

    }

    public bool isInTransition()
    {
        var transitionCubeAvatar = _morphCubeToSphere || _morphSphereToCapsule || _morphCapsuletoAvatar;
        var transitionAvatarCube = _morphAvatarToCapsule || _morphCapsuleToSphere || _morphSphereToCube;
        return fadeBars || fadeUnits || transitionCubeAvatar || transitionAvatarCube;
    }

    /*public void startLayingDown()
    {
        pivot = true;
        float newMaxHeight = (maxNbInd * 1.25f) / maxHeight;
    }*/

    public void layDownChart()
    {
        Vector3 axis = new Vector3(1f, 0f, 0f);
        this.transform.RotateAround(this.transform.position, axis, rotationSpeed*Time.deltaTime);
        
        this.transform.localScale = this.transform.localScale * 1.001f;
        this.transform.Translate(down);
    }

    public void turnToBars()
    {
        Debug.Log("Turn to bars");
        fadeUnits = true;
        fadeBars = false;
    }

    [BurstCompile]
    public void granuralize()
    {
        if (unitCreated)
        {
            enableUnitPoints(true);
        }
        else
        {
            _waitForUnitVisu = true;
        }
        enableBars(false);
        fadeBars = true;
        fadeUnits = false;
        if(_sortSelector!=null) _sortSelector.gameObject.SetActive(false);
    }

    public void enableSortSelector(bool enable)
    {
        if (_sortSelector != null) _sortSelector.gameObject.SetActive(enable);
    }

    public void turnToHumans()
    {
        if (humansCreated && _morphersAttributed)
        {
            //enableUnitMorphers(true);
            //_morphCubeToSphere = true;
            enableUnitPoints(false);
            enableHumans(true);
        }
        else
        {
            Debug.Log("Humans not created yet");
            _waitForAvatars = true;
        }
    }

    public void turnHumansToUnit()
    {
        //Debug.Log("turnHumansToUnit");
        //_morphAvatarToCapsule = true;
        enableUnitPoints(true);
        enableHumans(false);
    }

    public void enableHumans(bool enable)
    {
        foreach (var k in _dataSource.Keys)
        {
            foreach (var p in _dataSource[k])
            {
                p.enableHumanVisu(enable);
            }
        }
    }

    [BurstCompile]
    public void enableUnitPoints(bool enable)
    {
        foreach (var k in _dataSource.Keys)
        {
            foreach (var p in _dataSource[k])
            {
                p.enableUnitVisu(enable);
            }
        }
    }

    [BurstCompile]
    public void enableBars(bool enable)
    {
        foreach(var k in _dataSource.Keys)
        {
            _barDict[k].gameObject.SetActive(enable);
        }
    }

    IEnumerator createGranularity()
    {
        yield return null;
        var visuMaker = GameObject.FindAnyObjectByType<VisuMakerD3>();
        // Find the number of columns and rows
        float h;
        float l;
        int n;
        (h, l, n) = getMaxBar();

        int nbCol = 1;

        var diff = getDiff(nbCol, h, l, n);
        while (getDiff(nbCol + 1, h, l, n) < diff)
        {
            nbCol++;
            diff = getDiff(nbCol, h, l, n);
        }
        int nbRows = getRow(nbCol, n);

        float pointHeight = 0f;
        float pointWidth = 0f;
        float x; // Start x position for points
        float y = 0f; // Start y position for points
        float z = 0f; // z position for points
        pointHeight = (h * this.transform.localScale.y * this.transform.parent.localScale.y) / nbRows;
        pointWidth = l / nbCol;
        float w = Mathf.Min(pointHeight, pointWidth) * 0.7f;

        /*if (nbCol > 1) visuMaker.calculateMaxZoomSize(pointWidth);
        else visuMaker.calculateMaxZoomSize(-1);*/


        _targetPointScale = new Vector3(pointWidth, pointHeight, w);

        y = y + w / 2;

        _miniLabels = new List<GameObject>();
        foreach (string k in _dataSource.Keys)
        {
            var label = _labels[k];
            TextLabelManager labelManager = label.GetComponent<TextLabelManager>();
            var labelSize = labelManager.getSize();
            var labelPivot = labelManager.getPivot();
            var labelText = labelManager.getText();

            int cptX = 0;
            int cptY = 0;

            // Find the first x position for a point
            Vector3 barPos = _barDict[k].transform.position;
            var barSize = _barDict[k].transform.localScale.y * this.transform.localScale.y * this.transform.parent.localScale.y;
            x = barPos.x - l / 2;//barDict[k].transform.localScale.x * this.transform.localScale.x / 2 + w / 2;
            y = barPos.y - barSize / 2;
            z = barPos.z;
            // Create a parent to agregate all points of a bar
            GameObject bar = new GameObject();
            bar.name = k;
            bar.transform.parent = this.transform;

            // Create points
            foreach(DataPoint p in _dataSource[k])
            {
                GameObject blenderCube = Resources.Load<GameObject>("Objects/UnitCube");
                GameObject cube = GameObject.Instantiate(blenderCube);//GameObject.CreatePrimitive(PrimitiveType.Cube);
                // Uniform Scale
                //cube.transform.localScale = new Vector3(w, w, w);
                // Non uniform scale
                cube.transform.localScale = new Vector3(pointWidth, pointHeight, w);
                cube.transform.position = new Vector3(x + (cptX + 0.5f) * pointWidth, y + (cptY + 0.5f) * pointHeight, z / 2);
                cube.transform.parent = bar.transform;
                

                // Add Material to the point
                var renderer = cube.GetComponent<Renderer>();
                renderer.material = Resources.Load<Material>("Materials_Textures/BarMaterial");

                //MaterialExtensions.ToFadeMode(mat);
                //mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 0);*/
                p.setColumn(cptX, nbCol);
                p.setUnitVisu(cube);
                cptX++;
                if (cptX >= nbCol)
                {
                    cptX = 0;
                    cptY++;
                    if (cptY % 2 > 0)
                    {
                        // Create a miniLabel
                        GameObject miniLabel = Instantiate(label);
                        _miniLabels.Add(miniLabel);
                        miniLabel.tag = "Interactive";
                        TextLabelManager textManager = miniLabel.GetComponent<TextLabelManager>();
                        
                        textManager.resize(labelSize.x / 5, labelSize.y / 5);

                        var textLabelContent = labelManager.getText();
                        textManager.changeText(textLabelContent);
                        textManager.setPivotX(labelPivot.x);

                        miniLabel.transform.position = new Vector3(x + (nbCol+1) * pointWidth, y + (cptY + 0.5f) * pointHeight, -0.001f);
                        miniLabel.transform.rotation = Quaternion.Euler(0, 0, 0);
                        miniLabel.transform.SetParent(this.transform);
                    }
                }
                p.enableUnitVisu(false);
                yield return null;
            }
            //var coroutine = WaitForMorphers();
            //StartCoroutine(coroutine);
            enableMiniLabels(false);
            unitCreated = true;
        }
        StartCoroutine(WaitForMorphers());
    }

    IEnumerator WaitForMorphers()
    {
        //code éventuel
        Debug.Log("WaitForMorphers  t="+Time.time);
        var t = Time.time;
        yield return new WaitForSeconds(1);
        foreach(var lp in _dataSource.Values)
        {
            foreach(var p in lp)
            {
                p.setMorpher();
                yield return null;
            }
        }
        _morphersAttributed = true;
        
        StartCoroutine(createHumans());
    }

    IEnumerator createHumans()
    {
        yield return null;
        Debug.Log("Create humans t=" + Time.time);
        var sceneManager = GameObject.FindAnyObjectByType<MySceneManager>();
        var visuMaker = GameObject.FindAnyObjectByType<VisuMakerD3>();
        //var maxZoomSize = visuMaker.getMaxZoomSize();

        Vector2 scaleRange = new Vector2(88f, 100f);
        
        foreach (var k in _dataSource.Keys)
        {
            foreach (var p in _dataSource[k])
            {
                //Debug.Log("Create " + p.data[0]);
                var avatarName = p.data.Length >= 3 ? p.data[1] + "_" + p.data[2] : "avatar_" + p.data[0];
                var prefabID = p.getPrefabId();
                var prefab = sceneManager.getPrefab(prefabID);
                var humanVis = GameObject.Instantiate(prefab);//GameObject.Instantiate(defaultHumans);
                humanVis.name = avatarName;
                var rx = -90f;
                var ry = 180f;
                var rz = 0f;
                
                //if (humanVis.transform.rotation.eulerAngles.y != 0) ry = 180 - humanVis.transform.rotation.eulerAngles.y;
                //if (humanVis.transform.rotation.eulerAngles.z != 0) ry = -humanVis.transform.rotation.eulerAngles.z;
                //Debug.Log(humanVis.name + " rotation " + humanVis.transform.rotation.eulerAngles);
                if(humanVis.transform.rotation.eulerAngles.x == 0 && humanVis.transform.rotation.eulerAngles.y == 180 && humanVis.transform.rotation.eulerAngles.z == 180)
                {
                    rx += 180;
                    rz -= 180;
                }
                else if (humanVis.transform.rotation.eulerAngles.x == 0 && humanVis.transform.rotation.eulerAngles.y == 0 && humanVis.transform.rotation.eulerAngles.z == 0)
                {
                    rz -= 180;
                }

                var unitVis = p.getUnitVisu();
                humanVis.transform.position = unitVis.transform.position;
                humanVis.transform.parent = unitVis.transform.parent;

                // Add rendomness to the avatars' scale
                var randomScale = UnityEngine.Random.Range(scaleRange.x, scaleRange.y);
                var scaleCoef = randomScale / 100;
                var revizedScale = (unitVis.transform.localScale.y * scaleCoef);// / maxZoomSize;
                
                //humanVis.transform.localScale = new Vector3(unitVis.transform.localScale.y, unitVis.transform.localScale.y, unitVis.transform.localScale.y) * scaleCoef;
                humanVis.transform.localScale = Vector3.one * revizedScale;

                humanVis.transform.Rotate(90, 180, 0);
                var avatarHandler = humanVis.AddComponent<AvatarHandler>();
                avatarHandler.setRandomScaleCoef(scaleCoef);

                // Create panel
                var panel = GameObject.Instantiate(avatarPanel);
                
                panel.transform.rotation = Quaternion.Euler(rx, ry, rz);
                //panel.transform.parent = avatar.transform;
                panel.transform.SetParent(humanVis.transform);

                panel.transform.localPosition = new Vector3(-0.5f, 1.4f, 0f);
                //panel.transform.Rotate(-90, 180, 0);
                
                panel.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
                avatarHandler.setPanel(panel);
                avatarHandler.setColumn(p.getCol(), p.getNbColumns());

                // Create immersive sphere

                var immersivePrefab = Resources.Load<GameObject>("Objects/360Sphere");
                var immersiveSphere = GameObject.Instantiate(immersivePrefab);
                immersiveSphere.transform.SetParent(humanVis.transform);
                immersiveSphere.transform.localPosition = new Vector3(0f, 1.2f, 0.4f);
                avatarHandler.setImmersiveSphere(immersiveSphere);

                p.setHumanVisu(humanVis, panel);
                p.setImmersiveSphere(immersiveSphere.GetComponent<ImmersiveSphere>());
                p.enableHumanVisu(false);
                yield return null;
            }
        }
        humansCreated = true;
        Debug.Log("Humans created t=" + Time.time);
        loadingends?.Invoke();
    }

    public void startDisapear() { disapearing = true; }
    public Dictionary<string, BarScript> getBarDictionary() { return _barDict; }

    public void switchBarsTransparency(bool transparent)
    {
        foreach (var b in _barDict.Values)
        {
            Material mat;
            var foundBarmat = b.TryGetComponent<Material>(out mat);
            if(mat == null)
            {
                var barRenderer = b.GetComponent<Renderer>();
                mat = barRenderer.material;
            }
            if(transparent) MaterialExtensions.ToFadeMode(mat);
            else MaterialExtensions.ToOpaqueMode(mat);
            Debug.Log("Turn bars to fade mode : " + transparent);
        }
    } 

    public void switchUnitPointsTransparency(bool transparent)
    {
        foreach(var pList in _dataSource.Values)
        {
            foreach(var p in pList)
            {
                p.switchTransparency(transparent);
            }
        }
    }

    public float setBarsTransparency(float delta, bool isNewValue=false)
    {
        float a = 0f;
        foreach(var b in _barDict.Values)
        {
            var barRenderer = b.GetComponent<Renderer>();
            Material mat = barRenderer.material;
            a = Mathf.Max(0, Mathf.Min(1, mat.color.a + delta));
            mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, a);
        }
        return a;
    }

    public float setUnitPointsTransparency(float delta)
    {
        float a = 0f;
        foreach (var pList in _dataSource.Values)
        {
            foreach (var p in pList)
            {
                a = p.setTransparency(delta);
            }
        }
        return a;
    }

    public (float h, float l, int n) getMaxBar()
    {
        var tempoMax = 0;
        float height = 0f;
        float width = 0f;
        foreach(var b in _barDict.Values)
        {
            if(b.value>tempoMax)
            {
                tempoMax = b.value;
                height = b.getHeight();
                width = b.getWidth();
            }
        }
        return (height, width, tempoMax);
    }

    public int getRow(int c, int n)
    {
        var reste = n % c;
        return reste == 0 ? (int)n / c : ((n - reste) / c) + 1;
    }

    public float getSx(int c, float l) { return l / c; }
    public float getSy(float h, int r) { return h / r; }

    public float getDiff(int c, float h, float l, int n)
    {
        var r = getRow(c, n);
        var sX = getSx(c, l);
        var sY = getSy(c, r);
        return Mathf.Abs(sX - sY);
    }

    [BurstCompile]
    public bool rescalePoints(float coef, float targetX, float targetY , float targetZ)
    {
        var result = false;
        foreach(var lp in _dataSource.Values)
        {
            foreach(var p in lp)
            {
                result = p.rescaleVisu(coef, targetX, targetY, targetZ);
            }
        }
        return result;
    }

    public void resetUnitPointsScale()
    {
        foreach (var lp in _dataSource.Values)
        {
            foreach (var p in lp)
            {
                p.setScale(_targetPointScale);
            }
        }
    }
    

    public bool setPointsMorphSliders(float delta)
    {
        bool result = false;
        foreach(var lp in _dataSource.Values)
        {
            foreach (var p in lp)
            {
                result = p.setMorpherSlider(delta);
            }
        }
        return result;
    }

    public void enableUnitMorphers(bool enable)
    {
        foreach (var lp in _dataSource.Values)
        {
            foreach (var p in lp)
            {
                p.enableMorpher(enable);
            }
        }
    }

    public void movePointsToAvatarCenter(float target)
    {
        foreach (var lp in _dataSource.Values)
        {
            foreach (var p in lp)
            {
                p.moveUnitToAvatarCenter(target);
            }
        }
    }

    public void enableMiniLabels(bool enable)
    {
        foreach(var label in _miniLabels) label.SetActive(enable);
    }


    public void modifyBarChart(Dictionary<string, List<DataPoint>> data, SortType sortType, string title, int sortNum)
    {
        // Update Title
        var textMesh = _barChartTitle.GetComponentInChildren<TextMeshProUGUI>();
        if (textMesh != null)
        {
            textMesh.text = title;
        }

        _dataSource = data;
        var coef = getBarChartCoef(data);

        var newBarDict = new Dictionary<string, BarScript>();
        var newlabels = new Dictionary<string, GameObject>();

        // Calculate initial x position
        var nbBars = data.Count;
        float x = xStartPosition;//getBarXPosition(nbBars);

        if (_sortSelector != null)
        {
            _sortSelector.selectedButton(sortNum);
            _sortSelector.transform.localPosition = new Vector3(x - 1, 0.5f, -0.01f);
        }

        // Erase old bars and labels
        foreach ((var name, var bar) in _barDict) Destroy(bar.gameObject);
        foreach ((var name, var Label) in _labels) Destroy(Label.gameObject);
        // Reset _barDict and labels
        _barDict = new Dictionary<string, BarScript>();
        _labels = new Dictionary<string, GameObject>();

        string previousValue = "0";
        // Upload necessary resources
        Material barMaterial = Resources.Load<Material>("Materials_Textures/BarMaterial");
        GameObject textLabel = Resources.Load("UI/TextLabel") as GameObject;
        foreach (string k in data.Keys)
        {
            GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var barScript = bar.AddComponent<BarScript>();
            barScript.value = data[k].Count;

            Material barMat;
            var foundBarMat = bar.TryGetComponent<Material>(out barMat);
            if(!foundBarMat)
            {
                var barRenderer = bar.GetComponent<Renderer>();
                barRenderer.material = barMaterial;
            }

            bar.tag = "Interactive";
            float h = coef * data[k].Count;
            bar.transform.localScale = new Vector3(width, h, depth);
            bar.transform.parent = this.transform;
            bar.transform.localPosition = new Vector3(x, (h / 2), 0f);
            Debug.Log("Bar" + k + " position on creation :" + bar.transform.position);

            // Add label
            GameObject barLabel = Instantiate(textLabel);
            _labels.Add(k, barLabel);
            barLabel.tag = "Interactive";
            TextLabelManager textManager = barLabel.GetComponent<TextLabelManager>();
            textManager.resize(0.2f, width);

            var textLabelContent = k;
            if (sortType == SortType.NumericalRange && k != "unknown")
            {
                textLabelContent = previousValue + "-" + k;
                previousValue = k;
            }

            textManager.changeText(textLabelContent);
            textManager.setPivotX(width/2);
            barLabel.transform.SetParent(this.transform);
            //barLabel.transform.localPosition = new Vector3(x, -0.2f, -0.01f);
            barLabel.transform.localPosition = new Vector3(x - width / 2, -0.3f, -0.005f);
            barLabel.transform.localRotation = Quaternion.Euler(0, 0, 45f);



            x += width + gap;
            
            bar.name = k;
            _barDict.Add(k, barScript);

            Renderer r = bar.GetComponent<Renderer>();
            StructTools.ToFadeMode(r.material);
        }
        // Update unit visulization
        reorganizeUnitVis();
    }

    public float getBarChartCoef(Dictionary<string, List<DataPoint>> data)
    {
        maxNbInd = 0;
        foreach (string k in data.Keys)
        {
            if (data[k].Count > maxNbInd) maxNbInd = data[k].Count;
        }
        float coef = maxHeight / maxNbInd;
        return coef;
    }

    public float getBarXPosition(int nbBars)
    {
        float x = 0f;
        if (nbBars % 2 == 0)
        {
            x = -width * nbBars / 2 + 0.5f * width - gap * nbBars / 2 + 0.5f * gap;
        }
        else
        {
            x = -(width + gap) * (nbBars / 2);
        }
        return x;
    }

    public void reorganizeUnitVis()
    {
        var visuMaker = GameObject.FindAnyObjectByType<VisuMakerD3>();
        //var maxZoomSize = visuMaker.getMaxZoomSize();
        // Find the number of columns and rows
        float h;
        float l;
        int n;
        (h, l, n) = getMaxBar();

        int nbCol = 1;

        var diff = getDiff(nbCol, h, l, n);
        while (getDiff(nbCol + 1, h, l, n) < diff)
        {
            nbCol++;
            diff = getDiff(nbCol, h, l, n);
        }
        int nbRows = getRow(nbCol, n);

        // Calculate positions and sizes
        float pointHeight = 0f;
        float pointWidth = 0f;
        float x; // Start x position for points
        float y = 0f; // Start y position for points
        float z = 0f; // z position for points
        pointHeight = (h * this.transform.localScale.y * this.transform.parent.localScale.y) / nbRows;
        pointWidth = l / nbCol;
        float w = Mathf.Min(pointHeight, pointWidth) * 0.7f;

        /*if (nbCol > 1) visuMaker.calculateMaxZoomSize(pointWidth);
        else visuMaker.calculateMaxZoomSize(-1);*/

        _targetPointScale = new Vector3(pointWidth, pointHeight, w);

        y = y + w / 2;

        // Erase old mini labels
        foreach (var lab in _miniLabels) Destroy(lab);
        _miniLabels = new List<GameObject>();
        
        foreach (string k in _dataSource.Keys)
        {
            // Prepare elements for mini labels
            var label = _labels[k];
            TextLabelManager labelManager = label.GetComponent<TextLabelManager>();
            var labelSize = labelManager.getSize();
            var labelPivot = labelManager.getPivot();
            var labelText = labelManager.getText();

            int cptX = 0;
            int cptY = 0;

            // Find the first x position for a point
            Vector3 barPos = _barDict[k].gameObject.transform.position;
            var barSize = _barDict[k].transform.localScale.y * this.transform.localScale.y * this.transform.parent.localScale.y;
            x = barPos.x - l / 2;//barDict[k].transform.localScale.x * this.transform.localScale.x / 2 + w / 2;
            y = barPos.y - barSize / 2;
            z = barPos.z;
            Debug.Log("k="+k+"  Bar "+ _barDict[k].gameObject.name + " pos z =" + z);
            // Create a parent to agregate all points of a bar
            GameObject bar = new GameObject();
            bar.name = k;
            bar.transform.parent = this.transform;


            // Create points
            foreach (DataPoint p in _dataSource[k])
            {
                //GameObject blenderCube = Resources.Load<GameObject>("Objects/UnitCube");
                GameObject cube = p.getUnitVisu();//GameObject.Instantiate(blenderCube);
                // Non uniform scale
                cube.transform.localScale = new Vector3(pointWidth, pointHeight, w);
                cube.transform.position = new Vector3(x + (cptX + 0.5f) * pointWidth, y + (cptY + 0.5f) * pointHeight, z);
                cube.transform.parent = bar.transform;
                //cube.transform.localPosition = new Vector3(cube.transform.localPosition.x, cube.transform.localPosition.y,)
                p.setColumn(cptX, nbCol);
                // Add Material to the point
                var renderer = cube.GetComponent<Renderer>();
                renderer.material = Resources.Load<Material>("Materials_Textures/BarMaterial");

                // Position and size of human visu
                var humanVisu = p.getHumanVisu();
                AvatarHandler avatarHandler = humanVisu.GetComponent<AvatarHandler>();
                humanVisu.transform.parent = cube.transform.parent;
                humanVisu.transform.position = cube.transform.position;

                //var revizedScale = humanVisu.transform.localScale.x / maxZoomSize;
                var revizedScale = (cube.transform.localScale.y * avatarHandler.getRandomScaleCoef());// / maxZoomSize;
                humanVisu.transform.localScale = Vector3.one * revizedScale;

                
                avatarHandler.setPosition(humanVisu.transform.localPosition);
                avatarHandler.setColumn(p.getCol(), p.getNbColumns());


                //humanVis.transform.parent = unitVis.transform.parent; => should not be necessary


                cptX++;
                if (cptX >= nbCol)
                {
                    cptX = 0;
                    cptY++;
                    if (cptY % 2 > 0)
                    {
                        // Create a miniLabel
                        GameObject miniLabel = Instantiate(label);
                        _miniLabels.Add(miniLabel);
                        miniLabel.tag = "Interactive";
                        TextLabelManager textManager = miniLabel.GetComponent<TextLabelManager>();

                        textManager.resize(labelSize.x / 5, labelSize.y / 5);

                        var textLabelContent = labelManager.getText();
                        textManager.changeText(textLabelContent);
                        textManager.setPivotX(labelPivot.x);

                        miniLabel.transform.position = new Vector3(x + (nbCol + 1) * pointWidth, y + (cptY + 0.5f) * pointHeight, -0.001f);
                        miniLabel.transform.rotation = Quaternion.Euler(0, 0, 0);
                        miniLabel.transform.SetParent(this.transform);
                    }
                }
                
            }
            //var coroutine = WaitForMorphers();
            //StartCoroutine(coroutine);
            enableMiniLabels(false);
        }
    }

    public (bool, float) allAvatarsReachedMaxSize()
    {
        var reached = true;
        var coef = 1f;
        foreach(var cat in _dataSource.Values)
        {
            foreach(var p in cat)
            {
                var human = p.getHumanVisu();
                var avatarHandler = human.GetComponent<AvatarHandler>();
                var isMax = avatarHandler.reachedMaxSize();
                reached = reached && isMax;
                if (!isMax) coef = Mathf.Max(avatarHandler.getMaxRemainingZoom(), coef);
            }
        }
        return (reached, coef);
    }
}
