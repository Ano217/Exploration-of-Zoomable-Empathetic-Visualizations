using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

public class AvatarMaker : MonoBehaviour
{
    public GameObject avatar;
    public AnimatorController animCtrl;

    // Start is called before the first frame update
    void Start()
    {
        GameObject instantiated = Instantiate(avatar);
        Animator animator = instantiated.GetComponent<Animator>();
        if (animator == null)
        {
            instantiated.AddComponent<Animator>();
        }
        //animator.runtimeAnimatorController = Resources.Load("path_to_your_controller") as RuntimeAnimatorController;
        if (animator != null)
        {
            if (animCtrl != null)
            {
                animator.runtimeAnimatorController = animCtrl as RuntimeAnimatorController;
                instantiated.AddComponent<AvatarController>();
            }
            else Debug.Log("Animation ctrl is null");
        }
        else
        {
            Debug.Log("animator is null");
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
