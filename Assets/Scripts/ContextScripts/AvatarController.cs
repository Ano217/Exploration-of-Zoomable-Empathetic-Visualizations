using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

public class AvatarController : MonoBehaviour
{
    // sources: https://www.youtube.com/watch?v=53Yx8C5s05c
    const string idle = "Idle_";
    
    private Animator animator;
    private bool growing = false;
    private Vector3 targetSize;
    private bool makeVisible = false;
    private Transform player;
    private bool isAnimated = true;

    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();

        float r = Random.Range(1f, 12.99f);
        int rInt = (int)r;
        string animName = "";
        if (rInt < 10) animName = idle + "0" + rInt.ToString();
        else animName = idle + rInt.ToString();
        ChangeAnimationState(animName);

        player = FindAnyObjectByType<PlayerMove>().transform;
    }

    private void enableAnimator(bool enable) 
    { 
        animator.enabled = enable;
        isAnimated = enable;
    }

    private void ChangeAnimationState(string newState)
    {
        animator.Play(newState);
    }

    public void startGrowing(Vector3 targetSize)
    {
        this.targetSize = targetSize;
        growing = true;
    }

    public void startBeingVisible()
    {
        for(int i =0; i<transform.childCount; i++)
        {
            Renderer r = transform.GetChild(i).GetComponent<Renderer>();
            if (r != null)
            {
                StructTools.ToFadeMode(r.material);
                Color c = r.material.color;
                r.material.color = new Color(c.r, c.g, c.b, 0);
            }
        }
        makeVisible = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (growing)
        {
            this.transform.localScale = this.transform.localScale * 1.05f;
            if (this.transform.localScale.y >= targetSize.y) growing = false; 
        }
        if (makeVisible)
        {
            float a = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                Renderer r = transform.GetChild(i).GetComponent<Renderer>();
                if (r != null)
                {
                    Color c = r.material.color;
                    r.material.color = new Color(c.r, c.g, c.b, c.a + 0.05f);
                    if (a < r.material.color.a) a = r.material.color.a;
                }
            }
            if (a >= 1)
            {
                makeVisible = false;
                for (int i = 0; i < transform.childCount; i++)
                {
                    Renderer r = transform.GetChild(i).GetComponent<Renderer>();
                    if (r != null)
                    {
                        StructTools.ToOpaqueMode(r.material);
                    }
                }
            }
        }
        if(!isAnimated && Vector3.Distance(player.position, transform.position) < 10f)
        {
            enableAnimator(true);
        }
        else if(isAnimated && Vector3.Distance(player.position, transform.position) > 10f)
        {
            enableAnimator(false);
        }
    }
}
