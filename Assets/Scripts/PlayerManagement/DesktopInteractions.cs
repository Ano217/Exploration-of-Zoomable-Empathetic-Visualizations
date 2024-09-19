using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;
using Valve.VR;
using Valve.VR.Extras;
using Valve.VR.InteractionSystem;
using static UnityEngine.GraphicsBuffer;
using static Valve.VR.SteamVR_Events;

public class Constants
{
    public const int RIGHTHAND = 0;
    public const int LEFTHAND = 1;
    public const int RIGHTPOINTER = 2;
    public const int LEFTPOINTER = 3;

}


public class DesktopInteractions : MonoBehaviour
{
    [HideInInspector] public delegate void DelegateZoom(float z);
    [HideInInspector] public DelegateZoom zoom;

    [HideInInspector] public delegate void DelegateKeyPressed(KeyCode k);
    [HideInInspector] public DelegateKeyPressed keyPressed;

    [HideInInspector] public delegate void DelegateGrab(GameObject grabbed, int attachConst);
    [HideInInspector] public DelegateGrab grab;

    [HideInInspector] public delegate void DelegatePan(GameObject target, Transform hitPointer, int panningLaser, bool postStretch = false);
    [HideInInspector] public DelegatePan pan;

    [HideInInspector] public delegate void DelegateStretch(GameObject grabbed1, GameObject grabbed2, Transform ref1, Transform ref2);
    [HideInInspector] public DelegateStretch stretch;

    [HideInInspector] public delegate void DelegateRelease(int holder, GameObject target = null); //0:righthand ; 1:leftHand; 2:rigthPointer; 3:leftPointer
    [HideInInspector] public DelegateRelease release;

    [HideInInspector] public delegate void DelegateBascule();
    [HideInInspector] public DelegateBascule bascule;

    [HideInInspector] public delegate void DelegateExit(bool is360);
    [HideInInspector] public DelegateExit exitView;

    [HideInInspector] public delegate void DelegateReorganize(int sortNb);
    [HideInInspector] public DelegateReorganize reorganize;

    [HideInInspector] public delegate void DelegateGrabHead(int hand);
    [HideInInspector] public DelegateGrabHead grabHead;


    public Camera cam;
    //public MySceneManager manager;

    private List<GameObject> _leftGrab;
    private List<GameObject> _rightGrab;

    private GameObject leftPointerIn;
    private GameObject rightPointerIn;
    private bool leftPointerGrab;
    private bool rightPointerGrab;

    private SteamVR_Action_Boolean _leftGrapGrip;
    private SteamVR_Action_Boolean _rightGrapGrip;
    private SteamVR_Action_Vector2 _joystickValue;
    private SteamVR_Action_Boolean _leftGrabPinch;
    private SteamVR_Action_Boolean _rightGrabPinch;
    private SteamVR_Action_Boolean _basculeInput;
    private SteamVR_Action_Boolean _exitInput;
    private bool _leftAirGrab;
    private bool _rightAirGrab;

    public Valve.VR.InteractionSystem.Hand leftHand;
    public Valve.VR.InteractionSystem.Hand rightHand;
    private HandCollisionHandler _leftHandCollider;
    private HandCollisionHandler _rightHandCollider;

    // LaserPointer
    private SteamVR_LaserPointer _leftHandLaserPointer;
    private SteamVR_LaserPointer _rightHandLaserPointer;
    private LaserPointerTipHandler _leftLaserTip;
    private LaserPointerTipHandler _rightLaserTip;
    private bool _rightLaserPointerEnabled = true;
    private bool _leftLaserPointerEnabled = true;

    public List<GameObject> leftGrab => _leftGrab;
    public List<GameObject> rightGrab => _rightGrab;

    private bool _firstBasculeHint = true;
    private bool _firstExitHint = true;
    private bool _isGrabbingHeadRight = false;
    private bool _isCollidingHeadRight = false;
    private bool _isGrabbingHeadLeft = false;
    private bool _isCollidingHeadLeft = false;

    // Start is called before the first frame update
    void Start()
    {
        GameObject laserTipPrefab = Resources.Load<GameObject>("Objects/LaserPointerTip");

        _leftHandLaserPointer = leftHand.gameObject.GetComponent<SteamVR_LaserPointer>();
        _leftHandLaserPointer.PointerIn += OnPointerIn;
        _leftHandLaserPointer.PointerOut += OnPointerOut;
        _leftHandLaserPointer.PointerClick += OnPointerClick;

        var leftTip = GameObject.Instantiate(laserTipPrefab) as GameObject;
        _leftLaserTip = leftTip.GetComponent<LaserPointerTipHandler>();
        setLaserTip(Constants.LEFTPOINTER);

        _rightHandLaserPointer = rightHand.gameObject.GetComponent<SteamVR_LaserPointer>();
        _rightHandLaserPointer.PointerIn += OnPointerIn;
        _rightHandLaserPointer.PointerOut += OnPointerOut;
        _rightHandLaserPointer.PointerClick += OnPointerClick;
        var rightTip = GameObject.Instantiate(laserTipPrefab) as GameObject;
        _rightLaserTip = rightTip.GetComponent<LaserPointerTipHandler>();
        setLaserTip(Constants.RIGHTPOINTER);

        _leftGrapGrip = SteamVR_Input.GetBooleanAction("LeftGrabGrip");
        _rightGrapGrip = SteamVR_Input.GetBooleanAction("RightGrabGrip");
        _joystickValue = SteamVR_Input.GetVector2Action("JoystickPosition");
        _leftGrabPinch = SteamVR_Input.GetBooleanAction("LeftGrabPinch");
        _rightGrabPinch = SteamVR_Input.GetBooleanAction("RightGrabPinch");
        _basculeInput = SteamVR_Input.GetBooleanAction("Bascule");
        _exitInput = SteamVR_Input.GetBooleanAction("Exit");

        var leftHandColObj = GameObject.Find("HandColliderLeft(Clone)");
        if(leftHandColObj != null)
        {
            _leftHandCollider = leftHandColObj.AddComponent<HandCollisionHandler>();
            _leftHandCollider.collisionEnter += OnLeftHandCollisionEnter;
            _leftHandCollider.collisionExit += OnLeftHandCollisionExit;
            _leftHandCollider.collisionHead += OnLeftCollideHead;

        }
        var rightHandColObj = GameObject.Find("HandColliderRight(Clone)");
        if(rightHandColObj != null)
        {
            _rightHandCollider = rightHandColObj.AddComponent<HandCollisionHandler>();
            _rightHandCollider.collisionEnter += OnRightHandCollisionEnter;
            _rightHandCollider.collisionExit += OnRightHandCollisionExit;
            _rightHandCollider.collisionHead += OnRightCollideHead;
        }


        _leftGrab = new List<GameObject>();
        _rightGrab = new List<GameObject>();
        leftPointerGrab = false;
        rightPointerGrab = false;
        _leftAirGrab = false;
        _rightAirGrab = false;

        rightHand.ShowController(true);
        leftHand.ShowController(true);
    }

    public void OnDisable()
    {
        _rightHandCollider.collisionEnter -= OnRightHandCollisionEnter;
    }

    // Update is called once per frame
    void Update()
    {

        KeyCode[] keysToListen = { KeyCode.Keypad0, KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.R };
        foreach(KeyCode k in keysToListen)
        {
            if (Input.GetKeyDown(k))
            {
                keyPressed?.Invoke(k);
            }
        }

        // VR Interactions
        if (_leftHandCollider.collisions.Count > 0 && _leftLaserPointerEnabled)
        {
            var target = _leftHandCollider.collisions[0];
            TagsManager tagManager;
            var foundTagManager = target.TryGetComponent<TagsManager>(out tagManager);
            if (foundTagManager)
            {
                if (tagManager.hasTag(CustomTags.Grabbable))
                {
                    enableLaserPointer(false, Constants.LEFTPOINTER);
                }
            }
        }
        else if (_leftHandCollider.collisions.Count == 0 && !_leftLaserPointerEnabled)
        {
            enableLaserPointer(true, Constants.LEFTPOINTER);
        }

        if (_rightHandCollider.collisions.Count > 0 && _rightLaserPointerEnabled)
        {
            var target = _rightHandCollider.collisions[0];
            TagsManager tagManager;
            var foundTagManager = target.TryGetComponent<TagsManager>(out tagManager);
            if (foundTagManager)
            {
                if (tagManager.hasTag(CustomTags.Grabbable))
                {
                    enableLaserPointer(false, Constants.RIGHTPOINTER);
                }
            }
        }
        else if (_rightHandCollider.collisions.Count == 0 && !_rightLaserPointerEnabled)
        {
            enableLaserPointer(true, Constants.RIGHTPOINTER);
        }


        /*if (_leftGrapGrip.lastStateDown)
        {
            // Grabbing
            if (_leftHandCollider.collisions.Count > 0)
            {
                foreach (GameObject o in _leftHandCollider.collisions)
                {
                    _leftGrab.Add(o);
                    grab.Invoke(o, false );
                }
            }
        }
        if(_leftGrapGrip.lastStateUp)
        {
            // Release
            _leftGrab = new List<GameObject>();
            _leftAirGrab = false;
            release.Invoke(Constants.LEFTHAND);
        }

        if (_rightGrapGrip.lastStateDown)
        {
            // Grabbing
            if (_rightHandCollider.collisions.Count > 0)
            {
                foreach (GameObject o in _rightHandCollider.collisions)
                {
                    _rightGrab.Add(o);
                    grab.Invoke(o, true);
                }
            }
        }
        if (_rightGrapGrip.lastStateUp)
        {
            // Release
            _rightGrab = new List<GameObject>();
            release.Invoke(Constants.RIGHTHAND);
            _rightAirGrab=false;
        }*/

        if (_basculeInput.lastStateDown)
        {
            bascule.Invoke();
            if (_firstBasculeHint)
            {
                enableBasculeHint(false);
                _firstBasculeHint = false;
                //enableBasculeHint(true);
            }
        }

        if (_exitInput.lastStateDown)
        {
            exitView.Invoke(false);
            if (_firstExitHint) _firstBasculeHint=false;
            enableExitButton(false);
        }

        checkPointers();

    }

    private void enableLaserPointer(bool enable, int pointer)
    {
        switch (pointer)
        {
            case Constants.LEFTPOINTER:
                _leftLaserPointerEnabled = enable;
                _leftLaserTip.gameObject.SetActive(enable);
                break;
            case Constants.RIGHTPOINTER:
                _rightLaserPointerEnabled = enable;
                _rightLaserTip.gameObject.SetActive(enable);
                break;
        }
    }


    private void checkPointers()
    {
        if (_leftGrabPinch.lastStateDown )
        {
            if(_leftLaserPointerEnabled && leftPointerIn != null)
            { // Left pointer grab
                _leftLaserTip.grab(true);
                if (isGrabbable(leftPointerIn))
                {
                    // Grab object
                    grab.Invoke(leftPointerIn, Constants.LEFTPOINTER);
                }
                else
                {
                    // Stretch or pan
                    leftPointerGrab = true;
                    checkPointerStretch();
                    if (!rightPointerGrab && leftPointerIn != null) pan.Invoke(leftPointerIn, _leftHandLaserPointer.getHitTransform().transform, Constants.LEFTPOINTER);
                }
            }
            else if (!_leftLaserPointerEnabled)
            { // Left hand grab
                foreach (GameObject o in _leftHandCollider.collisions)
                {
                    _leftGrab.Add(o);
                    grab.Invoke(o, Constants.LEFTHAND);
                }
            }
            else if (_isCollidingHeadLeft)
            {
                _isGrabbingHeadLeft = true;
                grabHead.Invoke(Constants.LEFTHAND);
            }
        }
        else if (_leftGrabPinch.lastStateUp)
        {

            // Left pointer release
            _leftLaserTip.grab(false);
            leftPointerGrab = false;
            release.Invoke(Constants.LEFTPOINTER, leftPointerIn);
            if(rightPointerGrab) pan.Invoke(rightPointerIn, _rightHandLaserPointer.getHitTransform().transform, Constants.RIGHTPOINTER, true);

            // Left hand release
            _leftGrab = new List<GameObject>();
            release.Invoke(Constants.LEFTHAND);
            release.Invoke(Constants.LEFTPOINTER);

            _isGrabbingHeadLeft = false;
        }
        if(_rightGrabPinch.lastStateDown)
        {
            if (_rightLaserPointerEnabled && rightPointerIn != null)
            { // Right pointer grab
                _rightLaserTip.grab(true);
                if (isGrabbable(rightPointerIn))
                {
                    // Grab object
                    grab.Invoke(rightPointerIn, Constants.RIGHTPOINTER);
                }
                else
                {
                    rightPointerGrab = true;
                    checkPointerStretch();
                    if (!leftPointerGrab && rightPointerIn != null && _rightHandLaserPointer != null) pan.Invoke(rightPointerIn, _rightHandLaserPointer.getHitTransform().transform, Constants.RIGHTPOINTER);
                }
            }
            else if (!_rightLaserPointerEnabled)
            { // Right hand grab
                foreach (GameObject o in _rightHandCollider.collisions)
                {
                    _rightGrab.Add(o);
                    grab.Invoke(o, Constants.RIGHTHAND);
                }
            }
            else if (_isCollidingHeadRight)
            {
                Debug.Log("Right grab head ");
                _isGrabbingHeadRight = true;
                grabHead.Invoke(Constants.RIGHTHAND);
            }
            
        }
        else if (_rightGrabPinch.lastStateUp)
        {
            // Right laser release
            _rightLaserTip.grab(false);
            rightPointerGrab= false;
            release.Invoke(Constants.RIGHTPOINTER, rightPointerIn);
            if(leftPointerGrab) pan.Invoke(leftPointerIn, _leftHandLaserPointer.getHitTransform().transform, Constants.LEFTPOINTER, true);

            // Right hand release
            _rightGrab = new List<GameObject>();
            release.Invoke(Constants.RIGHTHAND);
            release.Invoke(Constants.RIGHTPOINTER);

            _isGrabbingHeadRight = false;
        }
    }

    private void checkPointerStretch()
    {
        if(leftPointerGrab && rightPointerGrab && leftPointerIn != null && rightPointerIn != null)
        {
            stretch.Invoke(leftPointerIn, rightPointerIn, _leftHandLaserPointer.hitTransform.transform, _rightHandLaserPointer.hitTransform.transform);
        }
    }

    private void OnPointerIn(object sender, PointerEventArgs e)
    {
        if (e.target.tag == "Interactive")
        {
            if (sender.ToString().Contains("RightHand")) rightPointerIn = e.target.gameObject;
            else if (sender.ToString().Contains("LeftHand")) leftPointerIn = e.target.gameObject;
            if (isGrabbable(e.target.gameObject)) enableOutline(true, e.target.gameObject);

            SortButton btn;
            var foundBtn = e.target.gameObject.TryGetComponent<SortButton>(out btn);
            if (foundBtn) btn.OnTipEnter();

            TrainingButton btnTraining;
            var foundBtnTraining = e.target.gameObject.TryGetComponent<TrainingButton>(out btnTraining);
            if (foundBtnTraining) btnTraining.OnTipEnter();
        }

    }

    private void OnPointerOut(object sender, PointerEventArgs e)
    {
        if (sender.ToString().Contains("RightHand"))
        {
            if (rightPointerIn != null) enableOutline(false, rightPointerIn);
            rightPointerIn = null;
        }
        else if (sender.ToString().Contains("LeftHand"))
        {
            if (leftPointerIn != null) enableOutline(false, leftPointerIn);
            leftPointerIn = null;
        }

        if (e.target.tag == "Interactive")
        {
            SortButton btn;
            var foundBtn = e.target.gameObject.TryGetComponent<SortButton>(out btn);
            if (foundBtn) btn.OnTipExit();

            TrainingButton btnTraining;
            var foundBtnTraining = e.target.gameObject.TryGetComponent<TrainingButton>(out btnTraining);
            if (foundBtnTraining) btnTraining.OnTipExit();
        }
    }

    private void OnPointerClick(object sender, PointerEventArgs e){}

    private void OnRightHandCollisionEnter(GameObject collided)
    {
        TagsManager tagManager;
        var hasTagManager = collided.TryGetComponent<TagsManager>(out tagManager);
        if (hasTagManager)
        {
            if (tagManager.hasTag(CustomTags.BasculeButton)) bascule.Invoke();
            if (tagManager.hasTag(CustomTags.Exit360Button)) exitView.Invoke(false);
            if (tagManager.hasTag(CustomTags.Grabbable)) enableOutline(true, collided);
        }
    }

    private void OnRightHandCollisionExit(GameObject collided)
    {
        if (isGrabbable(collided)) enableOutline(false, collided);
    }

    private void OnLeftHandCollisionEnter(GameObject collided)
    {
        if (isGrabbable(collided)) enableOutline(true, collided);
    }

    private void OnLeftHandCollisionExit(GameObject collided)
    {
        if (isGrabbable(collided)) enableOutline(false, collided);
    }

    public void resetHandCollisions()
    {
        _rightHandCollider.resetCollisions();
        _leftHandCollider.resetCollisions();
    }

    public void enableLaserTips(bool enable)
    {
        Debug.Log("enableLaserTips: " + enable);
        _leftLaserTip.makeInvisible(enable);
        _rightLaserTip.makeInvisible(enable);
        //untieLaserTips(!enable);

        _leftHandLaserPointer.thickness = enable ? 0.001f : 0f;
        _rightHandLaserPointer.thickness = enable ? 0.001f : 0f;

    }

    public void enableBasculeHint(bool enable)
    {
        string message = "Appuyer pour basculer";
        if (enable)
        {
            ControllerButtonHints.ShowTextHint(rightHand, _basculeInput, message, false);
        }
        else ControllerButtonHints.HideTextHint(rightHand, _basculeInput);
    }

    public void enableExitButton(bool enable)
    {
        string message = "Appuyer pour quitter";
        if(enable)
        {
            if (_firstExitHint) ControllerButtonHints.ShowTextHint(leftHand, _exitInput, message, true);
            else ControllerButtonHints.ShowTextHint(leftHand, _exitInput, message, false);
        }
        else
        {
            ControllerButtonHints.HideTextHint(leftHand, _exitInput);
        }
    }

    private bool isGrabbable(GameObject target)
    {
        TagsManager tagManager;
        var hasTagManager = target.TryGetComponent<TagsManager>(out tagManager);
        if (hasTagManager)
        {
            if (tagManager.hasTag(CustomTags.Grabbable))
            {
                return true;
            }
        }
        return false;
    }

    private void enableOutline(bool enable, GameObject target)
    {
        Outline outliner;
        var foundOutline = target.TryGetComponent<Outline>(out outliner);
        if (foundOutline) { outliner.enabled = enable; }
    }

    public GameObject getPointerTip(int pointerIndex)
    {
        switch (pointerIndex)
        {
            case Constants.LEFTPOINTER:
                return _leftLaserTip.gameObject;    
            case Constants.RIGHTPOINTER:
                return _rightLaserTip.gameObject;
            default: return null;
        }
    }

    public Vector3 getPointerPosition(int pointerIndex)
    {
        switch(pointerIndex)
        {
            case Constants.LEFTPOINTER:
                return _leftLaserTip.transform.position;
                break;
            case Constants.RIGHTPOINTER:
                return _rightLaserTip.transform.position;
                break;
            default: return Vector3.zero;
        }
    }

    public void untieLaserTips(bool untie)
    {
        if (untie)
        {
            Debug.Log("Untie laser tip");
            _leftLaserTip.setHitTransform(null);
            _rightLaserTip.setHitTransform(null);
        }
        else
        {
            setLaserTip(Constants.LEFTPOINTER);
            setLaserTip(Constants.RIGHTPOINTER);
        }
    }

    public void setLaserTip(int tipIndex)
    {
        switch(tipIndex)
        {
            case Constants.LEFTPOINTER:
                _leftLaserTip.setHitTransform(_leftHandLaserPointer.getHitTransform().transform);
                break;
            case Constants.RIGHTPOINTER:
                _rightLaserTip.setHitTransform(_rightHandLaserPointer.getHitTransform().transform);
                break;
        }
    }

    public void OnRightCollideHead(bool collideHead)
    {
        _isCollidingHeadRight = collideHead;
    }

    public void OnLeftCollideHead(bool collideHead)
    {
        _isCollidingHeadLeft = collideHead;
    }

    /*public void grabImmersiveSphere(ImmersiveSphere sphere, int hand)
    {
        if (hand == Constants.LEFTHAND) sphere.transform.parent = leftHand.transform;
        else if(hand == Constants.RIGHTHAND) sphere.transform.parent = rightHand.transform;
        sphere.transform.localPosition = Vector3.zero;
    }*/

}
