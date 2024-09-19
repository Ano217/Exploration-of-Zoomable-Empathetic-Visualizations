using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingPlane : MonoBehaviour
{
    [HideInInspector] public delegate void DelegateBasculeButton(bool enable);
    [HideInInspector] public DelegateBasculeButton enableBasculeButton;
    
    const float rotationSpeed = 15f;

    private GameObject _visualization;
    public GameObject trainingButtons;
    public DisplayZoomLevel displayZoomLevel;
    private TrainingManager _sceneManager;

    private int sortLevel = 0;
    private bool verticalVisu = true;
    private GameObject floor;

    //private bool layingDown = false;
    private bool standUp = false;

    // Stretch
    private Transform _stretchRef1;
    private Transform _stretchRef2;
    private float _stretchDistance;
    private bool _stretch;
    private Vector2 _middleToCenter;

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

    private GameObject _player;

    public GameObject visualisation => _visualization;

    private void Start()
    {
        _sceneManager = GameObject.FindAnyObjectByType<TrainingManager>();
        _visualization = this.gameObject;
        _visuStatus = 0;

        _player = GameObject.Find("Player");

        _anchor = new GameObject("VisualizationAnchor");
        _anchor.transform.position = new Vector3(_visualization.transform.position.x, 0f, _visualization.transform.position.z);
        _subAnchor = new GameObject("SubAnchor");

    }


    public void anchorVisualization(Vector3 position)
    {
        _anchor.transform.rotation = new Quaternion(0, 0, 0, 0);
        _anchor.transform.position = position;
        _visualization.transform.parent = _anchor.transform;
    }

    public void resetAnchor()
    {
        _visualization.transform.parent = null;
        _anchor.transform.position = new Vector3(_visualization.transform.position.x, 0f, _visualization.transform.position.z);
        _anchor.transform.rotation = new Quaternion(0, 0, 0, 0);
        _anchor.transform.localScale = Vector3.one;
    }

    public void updateSubAnchor()
    {
        _subAnchor.transform.parent = null;
        _visualization.transform.parent = _subAnchor.transform;
        _subAnchor.transform.position = _anchor.transform.position;
        _visualization.transform.parent = _anchor.transform;
        _subAnchor.transform.parent = _visualization.transform;
    }

    public void teleport(Vector3 position)
    {
        if (_visuStatus == 0)
        {
            _anchor.transform.localScale = _visualization.transform.localScale;
            anchorVisualization(position);
            _teleporting = true;
            _initialDistance = Vector3.Distance(_anchor.transform.position, _player.transform.position);
            _scaleRange = VisualizationLevels.MAXSIZE - _visualization.transform.localScale.x;
            trainingButtons.SetActive(false);
        }
        else if (_visuStatus == 1)
        {
            _player.transform.position = position;
        }

    }

    public Vector3 getVisualizationScale() { return _visualization.transform.localScale; }


    public void bascule()
    {
        if (!_teleporting && !standUp)
        {
            if (_visuStatus == 0)
            {
                // Teleport
                teleport(_visualization.transform.position);
                trainingButtons.SetActive(false);
            }
            else if (_visuStatus == 1)
            {
                //stand up
                prepareStand();
                trainingButtons.SetActive(true);
            }
        }
    }


    public bool getTeleporting() { return _teleporting; }
    public bool getStandUp() { return standUp; }

    private void Update()
    {
        var visScale = _visualization.transform.localScale.x;
        if(_visualization.transform.parent != null) visScale = visScale * _visualization.transform.parent.localScale.x;
        displayZoomLevel.updateBar(visScale);
        if (_teleporting)
        {
            //updateSubAnchor();
            var rotationOK = false;
            var positionOK = false;
            var scaleOK = false;

            // Rotation
            var seuilDbtRot = 20;
            var seuilFinRot = 80;

            var currentRotation = _anchor.transform.localEulerAngles.x % 360;
            var rotationSpeed = 20 * Time.deltaTime;
            if (currentRotation > seuilFinRot)
            {
                rotationSpeed = rotationSpeed * Mathf.Max(0.2f, ((90 - currentRotation) / (90 - seuilFinRot)));
            }
            if (currentRotation < seuilDbtRot)
            {
                var coef = 0.7f + (currentRotation / seuilDbtRot) * 0.3f;
                rotationSpeed = rotationSpeed * coef;
            }

            if ((Mathf.Abs(_anchor.transform.localEulerAngles.x - 90) % 360) < 0.1f || _anchor.transform.localEulerAngles.x % 360 > 90f) rotationOK = true;
            else
            {
                Vector3 axis = new Vector3(1f, 0f, 0f);
                _anchor.transform.RotateAround(_anchor.transform.position, axis, rotationSpeed);
            }

            var ratio = rotationSpeed / 90f;
            // Translation
            var targetPosition = new Vector3(_player.transform.position.x, _player.transform.position.y - 0.01f, _player.transform.position.z);
            if (Vector3.Distance(_anchor.transform.position, targetPosition) < 0.01f)
            {
                positionOK = true;
            }
            else
            {
                _anchor.transform.position = Vector3.MoveTowards(_anchor.transform.position, targetPosition, ratio * _initialDistance);
            }

            // Scale
            if (_anchor.transform.localScale.x >= VisualizationLevels.MAXSIZE)
            {
                scaleOK = true;
            }
            else
            {
                var addScale = _scaleRange * ratio;
                var newScale = _anchor.transform.localScale.x + addScale;
                _anchor.transform.localScale = new Vector3(newScale, newScale, newScale);
            }

            // Final check
            if (rotationOK && positionOK && scaleOK)
            {
                resetAnchor();
                _teleporting = false;
                _visuStatus = 1;
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
            }
            else
            {
                Vector3 axis = new Vector3(1f, 0f, 0f);
                _anchor.transform.RotateAround(_anchor.transform.position, axis, -rotationSpeed);
            }

            var ratio = rotationSpeed / 90f;
            // Translation
            var targetPosition = new Vector3(_player.transform.position.x, _player.transform.position.y, _player.transform.position.z + 5f);
            if (Vector3.Distance(_anchor.transform.position, targetPosition) < 0.01f)
            {
                positionOK = true;
            }
            else
            {
                _anchor.transform.position = Vector3.MoveTowards(_anchor.transform.position, targetPosition, ratio * _initialDistance);
            }

            // Scale
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
                _visuStatus = 0;
            }
        }

        if (_stretch)
        {
            updateStretch();
        }

        if (_pan)
        {
            Vector3 laserPos = _stretchRef1.position;
            Vector3 newPos = _visualization.transform.position;

            var distToPlayer = Vector3.Distance(laserPos, _player.transform.position);

            if (_visuStatus == 0)
            {
                newPos = new Vector3(laserPos.x + _panDist.x, laserPos.y + _panDist.y, _visualization.transform.position.z);
            }
            else if (_visuStatus == 1)
            {
                newPos = new Vector3(laserPos.x + _panDist.x, _visualization.transform.position.y, laserPos.z + _panDist.y);
            }

            var dist = Vector3.Distance(newPos, _visualization.transform.position);
            if ((distToPlayer > 6f && dist < 0.5f) || (distToPlayer < 6f && dist < 0.8f)) _visualization.transform.position = newPos;
        }
        if (!_teleporting && !standUp)
        {
            var scale = visualisation.transform.localScale.x;
            if (visualisation.transform.parent != null) scale = scale * visualisation.transform.parent.localScale.x;

            if (scale < VisualizationLevels.BASCULE  && _visuStatus == 1)
            {
                // Zoom out
                prepareStand();
            }
            else if (scale > VisualizationLevels.BASCULE2 && _visuStatus == 0)
            {
                // Zoom in
                teleport(_sceneManager.getCenterOfAttention());
            }
        }
    }

    public void prepareStand()
    {
        _anchor.transform.localScale = _visualization.transform.localScale;
        anchorVisualization(_player.transform.position);
        standUp = true;
        _stretch = false;
        _pan = false;
        trainingButtons.SetActive(true);
    }

    public void updateStretch()
    {
        var d = Vector3.Distance(_stretchRef1.position, _stretchRef2.position);
        var ratio = d / _stretchDistance;
        _stretchDistance = d;

        // Zoom 
        zoom(ratio);

        // Pan
        if (_visuStatus == 0)
        {
            var middleX = (_stretchRef1.transform.position.x + _stretchRef2.transform.position.x) / 2;
            var middleY = (_stretchRef1.transform.position.y + _stretchRef2.transform.position.y) / 2;

            var centerX = middleX - _middleToCenter.x * ratio;
            var centerY = middleY - _middleToCenter.y * ratio;
            _visualization.transform.position = new Vector3(centerX, centerY, _visualization.transform.position.z);
        }
        else if (_visuStatus == 1)
        {
            var middleX = (_stretchRef1.transform.position.x + _stretchRef2.transform.position.x) / 2;
            var middleZ = (_stretchRef1.transform.position.z + _stretchRef2.transform.position.z) / 2;

            var centerX = middleX - _middleToCenter.x * ratio;
            var centerZ = middleZ - _middleToCenter.y * ratio;
            _visualization.transform.position = new Vector3(centerX, _visualization.transform.position.y, centerZ);
        }
        updateMiddleToCenter();
    }



    public void zoom(float zoomValue)
    {
        var playerPos = _player.transform.position;

        var scale = _visualization.transform.localScale.x * zoomValue;
        if ((zoomValue > 1f && (scale < VisualizationLevels.MAXSIZE || _visualization.transform.localScale.x < VisualizationLevels.MAXSIZE)) || (zoomValue < 1f && (scale > VisualizationLevels.MINSIZE || _visualization.transform.localScale.x > VisualizationLevels.MINSIZE)))
        {
            scale = Mathf.Min(scale, VisualizationLevels.MAXSIZE);
            scale = Mathf.Max(scale, VisualizationLevels.MINSIZE);
            _visualization.transform.localScale = new Vector3(scale, scale, scale);
        }
    }



    public void setStretch(bool stretching)
    {
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
        if (_visuStatus <= 1 && !_teleporting && !standUp)
        {
            _pan = true;
            if (_visuStatus == 0)
            {
                _panDist = new Vector2(_visualization.transform.position.x - hitPointer.position.x, _visualization.transform.position.y - hitPointer.position.y);
            }
            else if (_visuStatus == 1)
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

    public void moveTo(Vector3 pos)
    {
        _visualization.transform.position = pos;
    }

    public int getVisuStatus()
    {
        return _visuStatus;
    }

}
