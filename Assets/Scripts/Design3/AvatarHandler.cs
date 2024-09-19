using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum ColumnPlace { Left, Middle, Right }

public class AvatarHandler : MonoBehaviour
{
    public string animationName;

    private Transform _player;
    private VisuMakerD3 _visuMakerD3;
    //private GameObject avatarPanel;
    private AvatarPanel _avatarPanel;
    private Renderer _avatarRenderer;
    private GameObject _immersiveSphere;
    private Animator _animator;
    private bool _enabledAnimator = false;
    private bool _canAnimate = false;
    
    // Random position/rotation
    private Vector3 _position;
    private Vector3 _targetRandomPosition;
    private float _targetRandomRotation;
    private bool _teleportationStarted = false;
    private bool _randomRotationDone = false;

    private float _randomScaleCoef;
    private int _numCol;
    private int _maxCol;

 
    //private Camera _camera;
    


    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.Find("Player").transform;
        _visuMakerD3 = GameObject.FindAnyObjectByType<VisuMakerD3>();
        //_camera = _player.GetComponentInChildren<Camera>();
        if(_avatarPanel != null ) _avatarPanel.gameObject.SetActive(false);
        if (_immersiveSphere != null) _immersiveSphere.SetActive(false);

        _animator = GetComponent<Animator>();
        
        _position = transform.localPosition;

        StartCoroutine("recalculateBounds");
    }

    IEnumerator recalculateBounds()
    {
        yield return null;
        var lodGroup = GetComponent<LODGroup>();
        if (lodGroup != null) lodGroup.RecalculateBounds();
        else Debug.Log("Did not find lodGorup");
        StartCoroutine("disableAnimator");
    }

    IEnumerator disableAnimator()
    {
        yield return null;
        if (_animator != null)
        {
            _animator.enabled = false;
            setRandomAnimation();
            enableAnimator(false);
        }
    }

    public void setImmersiveSphere(GameObject sphere) { _immersiveSphere = sphere;}

    public void setPanel(GameObject panel)
    {
        //Debug.Log("Set panel");
        _avatarPanel = panel.GetComponent<AvatarPanel>();
        var foundRenderer = gameObject.TryGetComponent<Renderer>(out _avatarRenderer);
        if(!foundRenderer) _avatarRenderer = gameObject.GetComponentInChildren<Renderer>();
        //_avatarRenderer = avatarPanel.GetComponent<Renderer>();
        //if (_avatarRenderer == null) avatarPanel.GetComponentInChildren<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        var dist = Vector3.Distance(_player.position, transform.position);
        var isRendered =  _avatarRenderer.isVisible;
        if (dist < 2f && !_avatarPanel.gameObject.activeSelf)
        {
            _avatarPanel.gameObject.SetActive(true);
            _avatarPanel.displayPanel();
            _immersiveSphere.SetActive(true);
        }
        else if (dist > 2f & _avatarPanel.gameObject.activeSelf)
        {
            _avatarPanel.gameObject.SetActive(false);
            _immersiveSphere.SetActive(false);
        }
        //else Debug.Log("dist=" + dist + "  avatarpanel=" + avatarPanel.activeSelf + "    isrendered=" + isRendered);
        if (_animator != null && _canAnimate)
        {
            if (dist < 6f && !_enabledAnimator)
            {
                enableAnimator(true);
            }
            else if (dist > 6f && _enabledAnimator)
            {
                enableAnimator(false);
            }
        }
        var visuStatus = _visuMakerD3.getVisuStatus();
        if (visuStatus == 3 && !_canAnimate)
        {
            _canAnimate = true;
            // Random positions and rotations
        }
        else if(visuStatus == 2 && _canAnimate)
        {
            _canAnimate = false;
            enableAnimator(false);
        }

        var speed = 2f * Time.deltaTime;
        if (_visuMakerD3.getStandUp())
        {
            if (Vector3.Distance(_position, transform.localPosition) > 0.01f) transform.localPosition = Vector3.MoveTowards(transform.localPosition, _position, speed);
        }
        else if(_visuMakerD3.getVisuStatus()==2 && transform.localRotation.eulerAngles.x != 90 && !_visuMakerD3.getTeleporting())
        {
            transform.localEulerAngles = new Vector3(90f, transform.localRotation.eulerAngles.y , transform.localRotation.eulerAngles.z);
        }
        if (_visuMakerD3.getTeleporting())
        {
            if (_teleportationStarted)
            {
                if (Vector3.Distance(transform.localPosition, _targetRandomPosition) > 0.01f) transform.localPosition = Vector3.MoveTowards(transform.localPosition, _targetRandomPosition, speed);
            }
            else
            {
                _teleportationStarted = true;
                // Random position
                var deltaX = 0f;//UnityEngine.Random.Range(-0.02f, 0.02f);
                if (_maxCol == 1) deltaX = UnityEngine.Random.Range(-0.02f, 0.02f);
                else if (_numCol == 0) deltaX = UnityEngine.Random.Range(-0.025f, -0.01f);
                else if (_numCol == _maxCol - 1) deltaX = UnityEngine.Random.Range(0.01f, 0.025f);
                //Debug.Log("col " + _numCol + "/" + _maxCol + "   dx=" + deltaX);
                var deltay = UnityEngine.Random.Range(-0.02f, 0.02f);
                _targetRandomPosition = new Vector3(_position.x + deltaX, _position.y+deltay, _position.z);

                // Add randomness in the avatars' rotations
                Vector2 rotationRange = new Vector2(0, 20);
                var randomRotation = UnityEngine.Random.Range(rotationRange.x, rotationRange.y) - rotationRange.y / 2;
                transform.Rotate(0, randomRotation, 0);
            }
        }
        else
        {
            _teleportationStarted = false;
        }
    }

    public void enableAnimator(bool enabled)
    {
        _enabledAnimator = enabled;
        _animator.enabled = enabled;
    }

    public void setRandomAnimation()
    {
        int r = UnityEngine.Random.Range(0, 16);
        string animName = "";
        animName ="Idle_" + r.ToString();

        try
        {
            animationName = animName;
            _animator.Play(animName);
        }
        catch (ArgumentException e)
        {
            Debug.Log("Could not launch the state " + animName);
        }
    }

    public void setPosition(Vector3 position) { _position = position; }
    public void setRandomScaleCoef(float scale) {  _randomScaleCoef = scale; }
    public float getRandomScaleCoef() { return _randomScaleCoef; }

    public minNmax getMinNMaxAvatar()
    {
        //GeometryAnalyzer.getObjectSize(this.gameObject);
        var res = GeometryAnalyzer.getMinNmax(this.gameObject);
        return res;
    }

    public float getAvatarSize()
    {
        return GeometryAnalyzer.getObjectSize(this.gameObject);
    }

    public bool reachedMaxSize(float coef = 1f)
    {
        var currentSize = getWorldSize();
        var res = currentSize >= 1f;
        return res;
    }

    public float getWorldSize()
    {
        var size = this.gameObject.transform.localScale.x;
        var parent = this.gameObject.transform.parent;
        while(parent != null)
        {
            size = size * parent.localScale.x;
            parent = parent.parent;
        }
        return size;
    }

    public float getMaxRemainingZoom(float coef = 1f)
    {
        var currentSize = getWorldSize();

        var remainingZoom = 1f / currentSize;
        //Debug.Log("Size=" + currentSize + "  remainingZoom=" + remainingZoom );
        return remainingZoom;
    }

    public void setColumn(int colNb, int totalColumns)
    {
        _numCol = colNb;
        _maxCol = totalColumns;
    }
}
