using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingManager : MonoBehaviour
{
    public GameObject trainingScene;
    public TrainingPlane trainingPlane;

    // Find in Resources
    //private GameObject _handBasculeButton;
    //private GameObject _handExit360ViewButton;
    private Material _skybox;
    private Texture _defaultSky;
    private Texture _textureView360;
    private Training_Sphere _currentImmersiveSphere;

    // Find in scene
    public DesktopInteractions desktopInteraction;

    private Transform _player;
    private Transform _rightHand;
    private Transform _leftHand;
    private GameObject _rightGrab;
    private GameObject _leftGrab;
    private GameObject _rightPointerGrab;
    private GameObject _leftPointerGrab;

    // Teleportation
    private Transform _teleportationTransform;
    private float _startPanTime;
    private Vector3 _startPanPosition;
    private bool _postStretch;

    private bool _firstUpdate = true;


    void Start()
    {
        _player = GameObject.Find("Player").transform;
        _rightHand = GameObject.Find("RightHand").transform;
        _leftHand = GameObject.Find("LeftHand").transform;

        // Get in Resources
        _skybox = Resources.Load<Material>("Materials_Textures/DefaultSky");
        _defaultSky = Resources.Load<Texture>("Materials_Textures/sky2Background");
        _textureView360 = Resources.Load<Texture>("Materials_Textures/Nice");


        // Get elements in the scene
        //_trainingPlane = GameObject.FindFirstObjectByType<TrainingPlane>();
        
        //_trainingPlane.enableBasculeButton += enableBasculeButton;
    }

    public void Update()
    {
        if (_firstUpdate)
        {
            enableInteractions();
            _firstUpdate = false;
        }
    }

    public void enableExitButton(bool enable)
    {
        desktopInteraction.enableExitButton(enable);
    }

    public void enableBasculeButton(bool enable)
    {
        desktopInteraction.enableBasculeHint(enable);
    }
    public void enableInteractions()
    {
        Debug.Log("Associate desktop interactions");
        //if (desktopInteraction == null) desktopInteraction = GameObject.FindFirstObjectByType<DesktopInteractions>();
        desktopInteraction.keyPressed += onKeyPressed;
        desktopInteraction.stretch += startStretching;
        desktopInteraction.release += release;
        desktopInteraction.bascule += layDown;
        desktopInteraction.grab += grabObject;
        desktopInteraction.exitView += switch360View;
        desktopInteraction.pan += startPan;
        desktopInteraction.grabHead += grabHead;
    }

    private void OnDisable()
    {
        /*desktopInteraction.keyPressed -= onKeyPressed;
        desktopInteraction.stretch -= startStretching;
        desktopInteraction.release -= release;
        desktopInteraction.bascule -= layDown;
        desktopInteraction.exitView -= switch360View;
        desktopInteraction.pan -= startPan;*/

        changeSkybox(true);
    }

    public void onKeyPressed(KeyCode k)
    {
        /*switch (k)
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
        }*/

    }


    public void startStretching(GameObject target1, GameObject target2, Transform ref1, Transform ref2)
    {
        //_teleportationMode = false;
        trainingPlane.stopPan(Constants.LEFTPOINTER);
        trainingPlane.stopPan(Constants.RIGHTPOINTER);
        var cond1 = target1 == null && target2 == null;

        if (cond1 || (target1.transform.IsChildOf(trainingPlane.visualisation.transform) && target2.transform.IsChildOf(trainingPlane.visualisation.transform)))
        {
            trainingPlane.setStretch(true);
            trainingPlane.setStretchReferences(ref1, ref2);
        }
    }

    public void startPan(GameObject target, Transform hitPointer, int panningLaser, bool postStretch = false)
    {
        if (target.transform.IsChildOf(trainingPlane.gameObject.transform))
        {
            _teleportationTransform = hitPointer;
            trainingPlane.startPan(hitPointer, panningLaser);
            _startPanTime = Time.time;
            _startPanPosition = hitPointer.position;
            _postStretch = postStretch;
            /*_teleportationTransform = hitPointer;
            Debug.Log("6");
            _trainingPlane.startPan(hitPointer, panningLaser);*/
        }
    }

    public void release(int holder, GameObject target = null)
    {
        if (holder == Constants.RIGHTPOINTER || holder == Constants.LEFTPOINTER)
        {
            trainingPlane.stopPan(holder);
            trainingPlane.setStretch(false);

            var grabTime = Time.time - _startPanTime;
            var pointerPosition = _teleportationTransform.position;
            var grabDist = Vector3.Distance(_startPanPosition, pointerPosition);
            if (grabTime < 0.2f && grabDist < 0.3f && !_postStretch)
            {
                if (target != null)
                {
                    TrainingButton btn;
                    var foundBtn = target.TryGetComponent<TrainingButton>(out btn);
                    if (foundBtn)
                    {
                        btn.OnClick();
                    }
                    else trainingPlane.teleport(pointerPosition);
                }
            }
        }
        releaseGrabObjects(holder);
    }


    public void layDown()
    {
        if (trainingPlane.isActiveAndEnabled)
        {
            release(Constants.RIGHTPOINTER);
            release(Constants.LEFTPOINTER);
            trainingPlane.bascule();
        }
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
                        target.transform.SetParent(desktopInteraction.getPointerTip(attachConst).transform);
                        desktopInteraction.getPointerTip(attachConst).transform.SetParent(_leftHand);
                        _leftPointerGrab = target;
                        break;
                    case Constants.RIGHTPOINTER:
                        target.transform.SetParent(desktopInteraction.getPointerTip(attachConst).transform);
                        desktopInteraction.getPointerTip(attachConst).transform.SetParent(_rightHand);
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
    }

    public void releaseGrabObjects(int attactConst)
    {
        Training_Sphere immersiveSphere = null;
        bool isSphere = false;
        GameObject gameObject = null;
        ref GameObject grab = ref gameObject;
        if (attactConst == Constants.RIGHTHAND && _rightGrab != null)
        {
            grab = _rightGrab;
        }
        else if (attactConst == Constants.LEFTHAND && _leftGrab != null)
        {
            grab = _leftGrab;
        }
        else if (attactConst == Constants.RIGHTPOINTER && _rightPointerGrab != null)
        {
            // Get right pointer tip
            grab = _rightPointerGrab;
        }
        else if (attactConst == Constants.LEFTPOINTER && _leftPointerGrab != null)
        {
            // Get let pointer tip
            grab = _leftPointerGrab;
        }
        if (grab != null)
        {
            grab.transform.SetParent(null);
            isSphere = grab.TryGetComponent<Training_Sphere>(out immersiveSphere);
            grab = null;
            if (isSphere) immersiveSphere.resetPosition();
            desktopInteraction.setLaserTip(attactConst);
            if (_currentImmersiveSphere == null) desktopInteraction.setLaserTip(attactConst);
        }
        else if (_currentImmersiveSphere != null)
        {
            _currentImmersiveSphere.resetPosition();
        }
    }

    public void switch360View(bool is360, Training_Sphere sphere, Texture texture = null)
    {
        _currentImmersiveSphere = sphere;
        releaseGrabObjects(Constants.LEFTHAND);
        releaseGrabObjects(Constants.RIGHTHAND);
        releaseGrabObjects(Constants.RIGHTPOINTER);
        releaseGrabObjects(Constants.LEFTPOINTER);
        trainingPlane.gameObject.SetActive(!is360);
        desktopInteraction.resetHandCollisions();
        desktopInteraction.enableLaserTips(false);
        changeSkybox(texture);
        StartCoroutine(ButtonCoroutine(is360));
    }

    public void switch360View(bool is360)
    {
        _currentImmersiveSphere = null;
        desktopInteraction.enableLaserTips(!is360);
        trainingPlane.gameObject.SetActive(!is360);
        changeSkybox(!is360);
        StartCoroutine(ButtonCoroutine(is360));
    }
    public Training_Sphere getCurrentImmersiveSphere() { return _currentImmersiveSphere; }

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


    public int getVisuStatus()
    {
        return trainingPlane.getVisuStatus();
    }

    public Vector3 getCenterOfAttention()
    {
        var lp = desktopInteraction.getPointerPosition(Constants.LEFTPOINTER);
        var rp = desktopInteraction.getPointerPosition(Constants.RIGHTPOINTER);

        return new Vector3((lp.x + rp.x) / 2, (lp.y + rp.y) / 2, (lp.z + rp.z) / 2);
    }

    public void grabHead(int hand)
    {
        if (_currentImmersiveSphere != null)
        {
            var vrCamera = GameObject.Find("VRCamera");
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

    }
}
