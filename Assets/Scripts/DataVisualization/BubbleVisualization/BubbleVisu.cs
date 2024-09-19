using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleVisu : MonoBehaviour
{
    private List<GameObject> pointsVisu;
    private Bubble parent;
    private Bubble subBubble;
    private PlayerMove player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void setBubbles(Bubble parentBubble, Bubble thisBubble)
    {
        parent = parentBubble;
        subBubble = thisBubble;
    }

    public void addPoint(GameObject p)
    {
        if(pointsVisu==null) pointsVisu = new List<GameObject>();
        pointsVisu.Add(p);
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) player = FindAnyObjectByType<PlayerMove>();
        else
        {
            if(Vector3.Distance(player.transform.position, this.transform.position)<3.0f)
            {
                // Trigger bubble
                subBubble.isGettingCloser(3.0f);
            }
        }
    }

    public void destroyAllVisu()
    {
        foreach(GameObject obj in pointsVisu)
        {
            Destroy(obj);
        }
        pointsVisu = new List<GameObject>();
    }


    private void OnTriggerEnter(Collider other)
    {
        PlayerMove isPlayer = other.gameObject.GetComponent<PlayerMove>();
        if (isPlayer != null )//&& !subBubble.getTrigger())
        {
            parent.childIsEntered(subBubble.getKey());
            subBubble.isEntered();
        }
    }

    public void isTouched()
    {
        subBubble.onTouch();
    }

    public void destroyVisu()
    {
        Destroy(this.gameObject);
    }
}
