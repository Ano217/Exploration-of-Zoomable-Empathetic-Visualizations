using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Valve.VR;
using Valve.VR.InteractionSystem;
using static UnityEngine.GraphicsBuffer;

public class VisualizationLevels
{
    public const float BARS = 1.5f;//1.2f;
    public const float HUMANS = 3f;//1.8f;
    public const float BASCULE = 3.3f;//2f;
    public const float BASCULE2 = 5f;//3f;

    public const float MAXSIZE = 7.5f;
    public const float MINSIZE = 0.01f;
    public const float MAXHUMANSIZE = 1.7f;
}

public class VisuMakerD3 : MonoBehaviour
{
    const float rotationSpeed = 15f;

    [HideInInspector] public delegate void DelegateLoadingEnds();
    [HideInInspector] public DelegateLoadingEnds loadingends;

    [HideInInspector] public delegate SortDescrAndData DelegateGetStep(int sortLevel);
    [HideInInspector] public DelegateGetStep getStep;

    [HideInInspector] public delegate void DelegateBasculeButton(bool enable);
    [HideInInspector] public DelegateBasculeButton enableBasculeButton;


    public float maxSizeAvatar;

    [Range(0, 1)]
    public float asymptoteZoom = 0.1f; // zoom asymptote on veut a in [0,1]
    [Range(0,20)]
    public float steepnessZoom = 2.5f; // zoom steepness
    [Range(0, 1)]
    public float panSteepness = 0.65f; // pan steepness
    [Range(0, 1)]
    public float maxSizeTeleport = 1f;

    private Vector3 _visualizationPosition = new Vector3(0, 0.5f, 0);

    private MySceneManager _sceneManager;
    private GameObject _visualization;
    private BarChartManager barChartManager;
    private HumanVizManager humanVizManager;
    //private float zoomLevel = 1f;
    private int sortLevel = 0;
    private bool verticalVisu = true;
    private GameObject floor;
    private Dictionary<string, List<DataPoint>> dataSource;

    //private bool layingDown = false;
    private bool standUp = false;

    // Stretch
    private Transform _stretchRef1;
    private Transform _stretchRef2;
    private float _stretchDistance;
    private bool _stretch;
    private Vector2 _middleToCenter;
    //private float _maxZoomSize;

    // Pan
    private bool _pan;
    private int _panningLaser;
    private Vector2 _panDist;

    // Teleport
    private bool _teleporting;
    private float _initialDistance;
    private float _scaleRange;


    // anchor
    private GameObject _anchor;
    private GameObject _subAnchor;

    private int _visuStatus;
    private Vector3 _rotationPoint;
    //private GameObject _sphere360;

    private GameObject _player;

    private bool _rotationOk;
    private bool _positionOk;
    private bool _scaleOk;
    private float _initialZoom;

    public GameObject visualisation => _visualization;

    private void Start()
    {
        if (maxSizeAvatar == 0) maxSizeAvatar = 6.5f;
        _sceneManager = GameObject.FindAnyObjectByType<MySceneManager>();
        /*_visualization = new GameObject();
        _visualization.name = "Visualization";
        _visualization.transform.position = new Vector3(0, 1.2f, 0);
        floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        //floor.transform.position = new Vector3(0, 1.2f, 0);
        floor.transform.parent = _visualization.transform;
        floor.gameObject.tag = "Interactive";
        _visuStatus = 0;

        _player = GameObject.Find("Player");

        _anchor = new GameObject();
        _anchor.name = "VisualizationAnchor";
        _anchor.transform.position = new Vector3(_visualization.transform.position.x, 0f, _visualization.transform.position.z);*/
    }

    public void initialization()
    {
        _visualization = new GameObject("Visualization");
        //_visualization.name = "Visualization";
        _visualization.transform.position = _visualizationPosition;
        _visualization.transform.localScale = Vector3.one;
        floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        //floor.transform.position = new Vector3(0, 1.2f, 0);
        floor.transform.parent = _visualization.transform;
        floor.gameObject.tag = "Interactive";
        _visuStatus = 0;

        _player = GameObject.Find("Player");

        _anchor = new GameObject("VisualizationAnchor");
        _anchor.transform.position = new Vector3(_visualization.transform.position.x, 0f, _visualization.transform.position.z);

        _subAnchor = new GameObject("SubAnchor");

        /*var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        sphere.transform.position = _anchor.transform.position;
        sphere.transform.parent = _anchor.transform;*/

    }

    private void makeFloor()
    {
        /*minNmax dimBarChart = GeometryAnalyzer.getMinNmax(barChartManager.gameObject);
        float width = dimBarChart.max.x - dimBarChart.min.x;
        float heigth = dimBarChart.max.y - dimBarChart.min.y;*/


        floor.transform.position = new Vector3(0, 0.5f, 0);//new Vector3(dimBarChart.min.x + width / 2, dimBarChart.min.y + heigth / 2, 0f);
        floor.transform.Rotate(new Vector3(-90, 0, 0));
        //floor.transform.localScale = new Vector3(width / 10 + 0.02f, 1f, heigth / 10 + 0.05f);
        floor.transform.localScale = new Vector3(50, 50, 50);
        Renderer r = floor.GetComponent<Renderer>();
        r.material = Resources.Load("Materials_Textures/FloorMat") as Material;
        
    }

    public void startBarChartVisu(Dictionary<string, List<DataPoint>> data, SortType sortType, GameObject defaultHuman, GameObject avatarpanel, string title, string[] sortButtons=null)
    {
        initialization();
        // Save data 
        dataSource = data;

        // Create parent
        GameObject barChart = new GameObject("BarChart");
        //barChart.name = "BarChart";
        barChart.transform.position = new Vector3(0, 0.5f, 0);
        barChartManager = barChart.AddComponent<BarChartManager>();
        barChartManager.createBarChart(data, sortType, title, sortButtons);
        barChart.transform.parent = _visualization.transform;
        // Create Floor
        makeFloor();
        barChartManager.defaultHumans = defaultHuman;
        barChartManager.avatarPanel = avatarpanel;
        barChartManager.loadingends += onLoadingEnd;
        //_sphere360 = GameObject.Instantiate(sphere);
        //_sphere360.SetActive(false);
    }

    public void OnDisable()
    {
        barChartManager.loadingends += onLoadingEnd;
    }

    public void startHumanVisu()
    {
        // start human viz
        GameObject humanViz = new GameObject();
        humanViz.name = "humanViz";
        humanVizManager = humanViz.AddComponent<HumanVizManager>();
        SortDescrAndData sorting = getStep.Invoke(1);
        humanViz.transform.parent = _visualization.transform;
        humanViz.transform.localEulerAngles = Vector3.zero;
        humanVizManager.startMakingHumans(sorting);
    }

    public void anchorVisualization(Vector3 position)
    {
        _anchor.transform.rotation = new Quaternion(0, 0, 0, 0);
        _anchor.transform.position = position;
        _visualization.transform.parent = _anchor.transform;
        //_subAnchor.transform.localPosition = position;
        //_subAnchor.transform.parent = _visualization.transform;
    }

    public void resetAnchor()
    {
        _visualization.transform.parent = null;
        _anchor.transform.position = new Vector3(_visualization.transform.position.x, 0f, _visualization.transform.position.z);
        _anchor.transform.rotation = new Quaternion(0, 0, 0, 0);
        _anchor.transform.localScale = Vector3.one;
        //_subAnchor.transform.parent = null;
    }

    public void updateSubAnchor()
    {
        _subAnchor.transform.parent = null;
        _visualization.transform.parent = _subAnchor.transform;
        _subAnchor.transform.position = _anchor.transform.position;
        _visualization.transform.parent = _anchor.transform;
        _subAnchor.transform.parent = _visualization.transform;
    }

    public void teleport ( Vector3 position)
    {
        if (canInteract())
        {
            Debug.Log("Teleport to position " + position);
            if (_visuStatus == 2)
            {
                // Stop pan and zoom
                stopPan(Constants.LEFTPOINTER);
                stopPan(Constants.RIGHTPOINTER);
                setStretch(false);

                // Prepare teleport
                position = checkPosition(position);
                _anchor.transform.localScale = _visualization.transform.localScale;
                anchorVisualization(position);
                _teleporting = true;
                _scaleOk = false;
                _rotationOk = false;
                _positionOk = false;
                _initialDistance = Vector3.Distance(_anchor.transform.position, _player.transform.position);
                _scaleRange = maxSizeAvatar - (_visualization.transform.localScale.x * _anchor.transform.localScale.x);
                //_scaleRange = VisualizationLevels.MAXSIZE - _visualization.transform.localScale.x;
                var human = FindFirstObjectByType<AvatarHandler>();
                _initialZoom = human.getMaxRemainingZoom();
            }
            else if (_visuStatus == 3)
            {
                _player.transform.position = position;
            }
        }
    }

    public Vector3 getVisualizationScale() { return _visualization.transform.localScale; }


    public void bascule()
    {
        if(canInteract() && !barChartManager.isInTransition())
        {
            if (_visuStatus == 2)
            {
                // Teleport
                teleport(barChartManager.transform.position);
            }
            else if(_visuStatus == 3)
            {
                //stand up
                prepareStand();
            }
        }
    }


    public bool getTeleporting() { return _teleporting; }
    public bool getStandUp() { return standUp; }

    public void checkZoomLevel()
    {
        if(_visualization.transform.localScale.x > maxSizeAvatar)
        {
            Debug.Log("Update zoom to " + maxSizeAvatar);
            _visualization.transform.localScale = new Vector3(maxSizeAvatar, maxSizeAvatar, maxSizeAvatar);
        }
    }

    private void Update()
    {
        //if (!_teleporting && !standUp) checkZoomLevel();
        if (_teleporting)
        {
            //updateSubAnchor();
            //var rotationOK = false;
            //var positionOK = false;
            //var scaleOK = false;

            // Rotation
            var seuilDbtRot = 20;
            var seuilFinRot = 80;

            var currentRotation = _anchor.transform.localEulerAngles.x % 360;
            var rotationSpeed = Mathf.Min(20 * Time.deltaTime, 20);
            //Debug.Log("Rotation speed =" + rotationSpeed);
            if (currentRotation > seuilFinRot)
            {
                rotationSpeed = rotationSpeed * Mathf.Max(0.2f, ((90 - currentRotation) / (90 - seuilFinRot)));
            }
            if (currentRotation < seuilDbtRot)
            {
                var coef = 0.7f + (currentRotation / seuilDbtRot) * 0.3f;
                rotationSpeed = rotationSpeed * coef;
            }

            if (!_rotationOk)
            {
                if ((Mathf.Abs(_anchor.transform.localEulerAngles.x - 90) % 360) < 0.1f || _anchor.transform.localEulerAngles.x % 360 > 90f)
                {
                    //Debug.Log("Rotation  is ok");
                    _anchor.transform.eulerAngles.Set(90f, 0, 0);
                    _rotationOk = true;
                }
                else
                {
                    Vector3 axis = new Vector3(1f, 0f, 0f);
                    _anchor.transform.RotateAround(_anchor.transform.position, axis, rotationSpeed);
                }
            }
            

            var ratio = rotationSpeed / 90f;
            // Translation
            if (!_positionOk)
            {
                var targetPosition = new Vector3(_player.transform.position.x, _player.transform.position.y - 0.01f, _player.transform.position.z);
                if (Vector3.Distance(_anchor.transform.position, targetPosition) < 0.01f)
                {
                    //Debug.Log("Position is ok");
                    _anchor.transform.position = targetPosition;
                    _positionOk = true;
                }
                else
                {
                    _anchor.transform.position = Vector3.MoveTowards(_anchor.transform.position, targetPosition, ratio * _initialDistance);
                }
            }


            // Scale
            if (!_scaleOk)
            {
                var human = FindFirstObjectByType<AvatarHandler>();
                var remainingZoom = human.getMaxRemainingZoom();
                var reachedMaxSize = human.reachedMaxSize();

                var scale = _anchor.transform.localScale.x * _visualization.transform.localScale.x;
                //Debug.Log("Scale=" + scale);
                
                if (reachedMaxSize)//VisualizationLevels.MAXSIZE)//_anchor.transform.localScale.x >= VisualizationLevels.MAXSIZE * _maxZoomSize)
                {
                    _scaleOk = true;
                    //Debug.Log("Max size is reached => Remaining zoom =" + remainingZoom);
                }
                else
                {
                    ratio = Mathf.Min(ratio, 1);
                    var addScale = 1 + Mathf.Abs((ratio * _initialZoom));
                    
                    addScale = Mathf.Max(addScale, 1);
                    addScale = Mathf.Min(addScale, remainingZoom);
                    //Debug.Log("addScale = " + addScale);
                    var newScale = _anchor.transform.localScale.x * addScale;

                    //newScale = Mathf.Max(newScale, 3f);
                    //newScale = Mathf.Min(newScale, remainingZoom);//VisualizationLevels.MAXSIZE);
                    if (_rotationOk)
                    {
                        Debug.Log("Finish zooming in once");
                        newScale = _anchor.transform.localScale.x * remainingZoom;
                    }
                    _anchor.transform.localScale = new Vector3(newScale, newScale, newScale);
                }
            }
            

            // Final check
            if (_rotationOk && _positionOk && _scaleOk)
            {
                resetAnchor();
                _teleporting = false;
                barChartManager.enableMiniLabels(true);
                _visuStatus = 3;
            }
        }
        if (standUp)
        {
            //updateSubAnchor();
            var rotationOK = false;
            var positionOK = false;
            var scaleOK = false;

            // Rotation
            var seuilDbtRot = 70;
            var seuilFinRot = 20;

            var currentRotation = _anchor.transform.localEulerAngles.x % 360;
            var rotationSpeed = 15 * Time.deltaTime;
            if (currentRotation > seuilDbtRot)
            {
                rotationSpeed = rotationSpeed * Mathf.Max(0.1f, ((90 - currentRotation) / (90 - seuilDbtRot)));
            }
            if (currentRotation > seuilFinRot)
            {
                //var coef = 0.7f + (currentRotation / seuilFinRot) * 0.3f;
                //rotationSpeed = rotationSpeed * coef;
                rotationSpeed = rotationSpeed * Mathf.Max(0.1f, (currentRotation / seuilFinRot));
            }

            if (Mathf.Abs(_anchor.transform.localEulerAngles.x - 270) < 0.5f)
            {
                rotationOK = true;
                //Debug.Log("cond1<0.1:" + Mathf.Abs(_anchor.transform.localEulerAngles.x - 270) + "   cond2<270:" + _anchor.transform.localEulerAngles.x % 360);
            }
            else
            {
                Vector3 axis = new Vector3(1f, 0f, 0f);
                _anchor.transform.RotateAround(_anchor.transform.position, axis, -rotationSpeed);
            }

            var ratio = rotationSpeed / 90f;
            // Translation
            var targetPosition = new Vector3(_player.transform.position.x, _player.transform.position.y, _player.transform.position.z+5f);
            if (Vector3.Distance(_anchor.transform.position, targetPosition) < 0.01f)
            {
                positionOK = true;
            }
            else
            {
                _anchor.transform.position = Vector3.MoveTowards(_anchor.transform.position, targetPosition, ratio * _initialDistance);
            }

            // Scale
            //if (_anchor.transform.localScale.x < VisualizationLevels.BASCULE2 * _maxZoomSize)
            if (_anchor.transform.localScale.x < VisualizationLevels.BASCULE2)
            {
                scaleOK = true;
            }
            else
            {
                var addScale = _scaleRange * ratio;
                var newScale = _anchor.transform.localScale.x - Mathf.Abs(addScale);
                _anchor.transform.localScale = new Vector3(newScale, newScale, newScale);
            }

            // Final check
            if (rotationOK && positionOK && scaleOK)
            {
                resetAnchor();
                standUp = false;
                barChartManager.enableMiniLabels(false);
                _visuStatus = 2;
            }
        }
        if (_stretch && canInteract())
        {
            updateStretch();
        }
        if (_pan && canInteract())
        {
            Vector3 laserPos = _stretchRef1.position;
            Vector3 newPos = _visualization.transform.position;
            var visPos = _visualization.transform.position;

            var distToPlayer = Vector3.Distance(laserPos, _player.transform.position);

            if (_visuStatus < 3)
            {
                newPos = new Vector3(laserPos.x + _panDist.x, laserPos.y + _panDist.y, _visualization.transform.position.z);
            }
            else if (_visuStatus == 3)
            {
                newPos = new Vector3(laserPos.x + _panDist.x, _visualization.transform.position.y, laserPos.z + _panDist.y);
            }

            var dist = Vector3.Distance(newPos, _visualization.transform.position);
            var revisedDist = getRevisedDistance(dist, distToPlayer);
            var ratio = revisedDist / dist;
            var deltaX = newPos.x - visPos.x;
            if (_visuStatus < 3)
            {
                var deltaY = newPos.y - visPos.y;
                newPos = visPos + new Vector3(deltaX * ratio, deltaY * ratio, 0);
            }
            else if (_visuStatus == 3)
            {
                var deltaZ = newPos.z - visPos.z;
                newPos = visPos + new Vector3(deltaX * ratio, 0, deltaZ * ratio);
            }
            if (!float.IsNaN(newPos.x) && !float.IsNaN(newPos.y) && !float.IsNaN(newPos.z))
            {
                _visualization.transform.position = newPos;
            }
            else
            {
                stopPan(Constants.LEFTPOINTER);
                stopPan(Constants.RIGHTPOINTER);
                //Debug.Log("Pan: mex position is Nan");
            }
            //Debug.Log("Dist pan=" + dist + "    Distance to Player = " + distToPlayer);
            //if ((distToPlayer > 6f && dist < 0.5f) || (distToPlayer < 6f && dist < 0.8f)) _visualization.transform.position = newPos;
            //else Debug.Log("No Pan");

        }
        if(!barChartManager.isInTransition() && !_teleporting && !standUp)
        {
            var scale = visualisation.transform.localScale.x;
            if (visualisation.transform.parent != null) scale = scale * visualisation.transform.parent.localScale.x;
            //if (canInteract() && scale > VisualizationLevels.BARS && scale < VisualizationLevels.HUMANS * _maxZoomSize && _visuStatus == 0)
            if (canInteract() && scale > VisualizationLevels.BARS && scale < VisualizationLevels.HUMANS && _visuStatus == 0)
            {
                //Debug.Log("Bars => Unit");
                _visuStatus = 1;
                barChartManager.granuralize();
            }
            else if (canInteract() && scale < VisualizationLevels.BARS && _visuStatus == 1)
            {
                //Debug.Log("Unit => Bars");
                barChartManager.turnToBars();
                _visuStatus = 0;
            }
            else if (canInteract() && scale > VisualizationLevels.HUMANS && _visuStatus == 1)
            {
                _visuStatus = 2;
                barChartManager.turnToHumans();
                enableBasculeButton.Invoke(true);
            }
            else if (canInteract() && scale < VisualizationLevels.HUMANS && _visuStatus == 2)
            {
                _visuStatus = 1;
                //barChartManager.switchPointVisu(true);
                barChartManager.turnHumansToUnit();
                enableBasculeButton.Invoke(false);
            }
            else if(canInteract() && scale < VisualizationLevels.BASCULE && _visuStatus == 3)
            {
                // Zoom out
                prepareStand();
            }
            else if(canInteract() && scale > VisualizationLevels.BASCULE2 && _visuStatus == 2)
            {
                // Zoom in
                //bascule();
                teleport(_sceneManager.getCenterOfAttention());
            }
        }
    }

    public float getRevisedDistance(float d, float dp) // dp = distance to player
    {
        var asymptote = (1 / (1+Mathf.Exp(-1/(1+dp)))) + 0.1f;
        var revisedDist = (1 - Mathf.Pow(panSteepness, d)) * asymptote;
        return revisedDist;
    }

    public void prepareStand()
    {
        if (canInteract())
        {
            // Stop pan and zoom
            stopPan(Constants.LEFTPOINTER);
            stopPan(Constants.RIGHTPOINTER);
            setStretch(false);

            // Prepare to stand up
            _anchor.transform.localScale = _visualization.transform.localScale;
            anchorVisualization(_player.transform.position);
            standUp = true;
        }
    }

    /*public void startTransition(int oldLevel)
    {
        if (oldLevel == 0)
        {
            barChartManager.startLayingDown();

            // start human viz
            GameObject humanViz = new GameObject();
            humanViz.name = "humanViz";
            humanVizManager = humanViz.AddComponent<HumanVizManager>();
            humanVizManager.createVisualization();
        }
    }*/




    public void updateStretch()
    {
        var d = Vector3.Distance(_stretchRef1.position, _stretchRef2.position);
        var ratio = 1f;
        if (_stretchDistance != 0) ratio = d / _stretchDistance;
        //else Debug.Log("_stretchDistance==0");

        //Debug.Log("Ratio=" + ratio);
        _stretchDistance = d;
        
        // Zoom 
        ratio = zoom(Mathf.Abs(ratio));

        // Pan
        if (ratio > 0)
        {
            if (_visuStatus < 3)
            {
                var middleX = (_stretchRef1.transform.position.x + _stretchRef2.transform.position.x) / 2;
                var middleY = (_stretchRef1.transform.position.y + _stretchRef2.transform.position.y) / 2;

                var centerX = middleX - _middleToCenter.x * ratio;
                var centerY = middleY - _middleToCenter.y * ratio;
                _visualization.transform.position = new Vector3(centerX, centerY, _visualization.transform.position.z);
            }
            else if (_visuStatus == 3)
            {
                var middleX = (_stretchRef1.transform.position.x + _stretchRef2.transform.position.x) / 2;
                var middleZ = (_stretchRef1.transform.position.z + _stretchRef2.transform.position.z) / 2;

                var centerX = middleX - _middleToCenter.x * ratio;
                var centerZ = middleZ - _middleToCenter.y * ratio;
                _visualization.transform.position = new Vector3(centerX, _visualization.transform.position.y, centerZ);
            }
            updateMiddleToCenter();
        }
        
    }

    public float getRevisedRatio(float ratio)
    {
        var zoom = 1 - (((2 * asymptoteZoom) / (1 + Mathf.Exp(-steepnessZoom * (1 - ratio)))) - asymptoteZoom);
        return zoom;
    }

    public float zoom(float zoomValue)
    {
        var playerPos = _player.transform.position;
        //var visPos = _visualization.transform.position;
        //var vectorPlayerVis = new Vector2(playerPos.x - visPos.x, playerPos.z - visPos.z);   // player - vis



        var reachedMaxSize = false;
        var maxZoom = maxSizeAvatar / _visualization.transform.localScale.x; //VisualizationLevels.MAXSIZE;// * _maxZoomSize;
        if (_visuStatus == 3)
        {
            var human = FindFirstObjectByType<AvatarHandler>();
            reachedMaxSize = human.reachedMaxSize();
            maxZoom = human.getMaxRemainingZoom();
        }
        //if ((zoomValue > 1f && (scale < VisualizationLevels.MAXSIZE * _maxZoomSize || _visualization.transform.localScale.x < VisualizationLevels.MAXSIZE * _maxZoomSize)) || (zoomValue < 1f && (scale > VisualizationLevels.MINSIZE || _visualization.transform.localScale.x > VisualizationLevels.MINSIZE)))
        if ((zoomValue > 1f && !reachedMaxSize) || (zoomValue < 1f && (_visualization.transform.localScale.x > VisualizationLevels.MINSIZE)))
        {
            var revizedZoomValue = zoomValue;
            //zoomLevel = zoomLevel * zoomValue;
            if (_visuStatus == 3 && zoomValue > 1f) revizedZoomValue = Mathf.Min(zoomValue, maxZoom);
            revizedZoomValue = Mathf.Abs(getRevisedRatio(zoomValue));
            var scale = _visualization.transform.localScale.x * revizedZoomValue;
            //scale = Mathf.Min(scale, VisualizationLevels.MAXSIZE * _maxZoomSize);
            if (scale < VisualizationLevels.MINSIZE)
            {
                var xCoef = VisualizationLevels.MINSIZE / scale;
                scale = VisualizationLevels.MINSIZE;
                revizedZoomValue = revizedZoomValue * xCoef;
            }
            
            _visualization.transform.localScale = new Vector3(scale, scale, scale);
            //Debug.Log("  revizedZoom=" + revizedZoomValue + "  scale=" + _visualization.transform.localScale);
            return revizedZoomValue;
        }
        else return -1;
    }


    public void startHumanoidVisualization(SortDescriptionD3 sort, Dictionary<string, List<DataPoint>> data)
    {
        humanVizManager.startMakingHumans(data, sort, barChartManager.getBarDictionary());
        barChartManager.startDisapear();
    }

    public void setStretch(bool stretching)
    {
        //if (!stretching) Debug.Log("Stop zooming");
        if (!stretching || (stretching && _visuStatus <= 3 && !_teleporting && !standUp)) _stretch = stretching;
    }

    public void setStretchReferences(Transform ref1, Transform ref2)
    {
        _stretchRef1 = ref1;
        _stretchRef2 = ref2;
        _stretchDistance = Vector3.Distance(ref1.position, ref2.position);
        if (_stretchDistance == 0) _stretchDistance = 0.01f;
        
        updateMiddleToCenter();
    }

    public void updateMiddleToCenter()
    {
        float middleX = (_stretchRef1.position.x + _stretchRef2.position.x) / 2;
        if (_visuStatus < 3)
        {
            float middleY = (_stretchRef1.position.y + _stretchRef2.position.y) / 2;
            _middleToCenter = new Vector2(middleX - _visualization.transform.position.x, middleY - _visualization.transform.position.y);
        }
        else if (_visuStatus == 3)
        {
            float middleZ = (_stretchRef1.position.z + _stretchRef2.position.z) / 2;
            _middleToCenter = new Vector2(middleX - _visualization.transform.position.x, middleZ - _visualization.transform.position.z);
        }
    }

    public void startPan(Transform hitPointer, int panningLaser)
    {
        if (_visuStatus <= 3 && canInteract())
        {
            _pan = true;
            if (_visuStatus < 3)
            {
                _panDist = new Vector2(_visualization.transform.position.x - hitPointer.position.x, _visualization.transform.position.y - hitPointer.position.y);
            }
            else if(_visuStatus == 3)
            {
                _panDist = new Vector2(_visualization.transform.position.x - hitPointer.position.x, _visualization.transform.position.z - hitPointer.position.z);
            }
            _panningLaser = panningLaser;
            _stretchRef1 = hitPointer;
        }
        
    }

    public void stopPan(int panningLaser)
    {
        if (panningLaser == _panningLaser)
        {
            _pan = false;
            _panningLaser = -1;
            _stretchRef1 = null;
        }
        else
        {
            var param = panningLaser == Constants.LEFTPOINTER ? "left" : panningLaser == Constants.RIGHTPOINTER ? "right" : "other";
            var record = _panningLaser == Constants.LEFTPOINTER ? "left" : _panningLaser == Constants.RIGHTPOINTER ? "right" : "other";
        }
    }

    public void launch360View(bool enable)
    {
        //_sphere360.transform.position = player.position;
        //_sphere360.transform.parent = player;
        _visualization.SetActive(!enable);
        if (enable) _visuStatus = 4;
        else _visuStatus = 3;
    }

    public void onLoadingEnd()
    {
        /*barChartManager.enableBars(false);
        barChartManager.enableBars(true);
        barChartManager.enableUnitPoints(true);
        barChartManager.enableUnitPoints(false);
        barChartManager.rescalePoints(0, 1, 1, 1);*/
        barChartManager.granuralize();
        barChartManager.turnToBars();
        loadingends.Invoke();
    }

    public void moveTo(Vector3 pos)
    {
        _visualization.transform.position = pos;
    }

    public int getVisuStatus()
    {
        return _visuStatus;
    }

    private Vector3 checkPosition(Vector3 position)
    {
        var avatars = GameObject.FindObjectsOfType<AvatarHandler>();
        // 1: check all avatars and find the closest
        GameObject closestAvatar = null;
        float minDist = 0.7f;

        var scale = _visualization.transform.localScale.x;
        var coef = maxSizeAvatar / scale;
        //var coef = VisualizationLevels.MAXSIZE / scale;
        foreach (var avatar in avatars)
        {
            var dist = Vector3.Distance(position, avatar.transform.position);
            //Debug.Log("dist=" + dist + "   dist*coef=" + dist * coef);
            dist = dist * coef;
            if (dist < minDist)
            {
                minDist = dist;
                closestAvatar = avatar.gameObject;
            }
        }
        
        if (closestAvatar == null)
        {
            // 2: if far enough => return position
            return position;
        }
        else
        {
            // 3: else: return the bottom right position (diagonal)
            var avatarPos = closestAvatar.transform.position;
            var periX = 0.6f / coef;
            var periY = 0.5f / coef;
            Vector3 diag = new Vector3(avatarPos.x + periX, avatarPos.y - periY, avatarPos.z);
            return diag;
            // 3: else: check right, left, up and down positions around and find the one with the biggest minimum distance to another avatar
            /*Debug.Log("Not far enough with minDist=" + minDist);
            var peri = 0.7f / coef; 
            var rightPos = new Vector3(position.x - peri, position.y, position.z);
            var leftPos = new Vector3(position.x + peri, position.y, position.z);
            var upPos = new Vector3(position.x, position.y + peri, position.z);
            var downPos = new Vector3(position.x, position.y - peri, position.z);
            Vector3[] positions = new Vector3[4];
            positions[0] = rightPos;
            positions[1] = leftPos;
            positions[2] = upPos;
            positions[3] = downPos;

            int bestPosition = 0;
            float bestDistance = 0;
            
            for(int i = 0; i < 4; i++)
            {
                var miniDist = 1f;
                foreach(var avatar in avatars)
                {
                    var dist = Vector3.Distance(positions[i], avatar.transform.position);
                    if (dist < miniDist && Vector3.Distance(avatar.transform.position, position) > 0f)
                    {
                        miniDist = dist;
                    }
                }
                if (miniDist > bestDistance)
                {
                    bestDistance = miniDist;
                    bestPosition = i;
                }
            }

            return positions[bestPosition];*/
        }
    }

    public bool reorganizeBarchart(Dictionary<string, List<DataPoint>> data, SortType sortType, string title, int sortNum)
    {
        if (_visuStatus == 0)
        {
            _visualization.transform.localScale = new Vector3(1, 1, 1);
            _visualization.transform.position = _visualizationPosition;
            _player.transform.position = new Vector3(0, 0, -5);
            barChartManager.modifyBarChart(data, sortType, title, sortNum);
            return true;
        }
        return false;
    }

    /*public void calculateMaxZoomSize(float actualDistance)
    {
        float minDistance = 1.2f;
        if (actualDistance >= 0)
        {
            _maxZoomSize = (_visualization.transform.localScale.x * minDistance) / (actualDistance * VisualizationLevels.MAXSIZE);
        }
        else _maxZoomSize = 1f;
        Debug.Log("MaxZoomSize = " + _maxZoomSize);
    }*/

    //public float getMaxZoomSize() { return _maxZoomSize; }

    public bool canInteract() { return !_teleporting && !standUp; }

    public void resetVisualization()
    {
        if (_visuStatus == 4) launch360View(false);

        barChartManager.resetUnitPointsScale();
        barChartManager.enableMiniLabels(false);
        barChartManager.enableHumans(false);
        barChartManager.enableUnitPoints(false);
        barChartManager.enableBars(true);
        barChartManager.enableSortSelector(true);

        // visu status, zoom && position
        _visualization.transform.rotation = new Quaternion(0, 0, 0, 0);
        _visualization.transform.localScale = new Vector3(1, 1, 1);
        _visualization.transform.position = _visualizationPosition;
        _player.transform.position = new Vector3(0, 0, -5);
        _visuStatus = 0;

        // reset all bool variables
        _stretch = false;
        _pan = false;
        _panningLaser = -1;
        _stretchRef1 = null;
        _teleporting = false;
        standUp = false;
        _rotationOk = false;
        _positionOk = false;
        _scaleOk = false;
    }

}
