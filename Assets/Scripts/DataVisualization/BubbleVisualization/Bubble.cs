using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble
{
    //private BubbleManager manager;
    private string keyWord;
    private BubbleMaker visuMaker;
    private SortDescription sortDescription;

    private Dictionary<string, Bubble> children;
    private Dictionary<string, List<DataPoint>> datas;
    private List<DataPoint> allData;

    private BubbleVisu visualization;

    private bool hasParent;
    private bool isTrigerred;


    public Bubble(BubbleMaker maker, string bblKey, List<DataPoint> dataPoints, int sortID, SortDescription[] sortDescriptions)
    {
        allData = dataPoints;
        keyWord = bblKey;
        visuMaker = maker;
        if (sortID < sortDescriptions.Length)
        {
            sortDescription = sortDescriptions[sortID];
            calculateData(dataPoints);
            createChildren(sortDescriptions, dataPoints, sortID);
            
        }
        else
        {
            //datas = new Dictionary<string, List<DataPoint>>();
            //datas.Add(keyWord, dataPoints);
            sortDescription = new SortDescription();
            sortDescription.triggerType = TriggerType.Collision;
        }
        hasParent = (sortID > 0);
        isTrigerred = false;
    }

    public void setVisualization(BubbleVisu vis)
    {
        visualization = vis;
    }

    public Bubble getChild(string key)
    {
        return children[key];
    }

    private void calculateData(List<DataPoint> dataPoints)
    {
        switch (sortDescription.sortType)
        {
            case SortType.Nominal:
                datas = DataSorter.sortByNominal(dataPoints, sortDescription.columnID);
                break;
            case SortType.NumericalRange:
                datas = DataSorter.sortByRange(dataPoints, sortDescription.columnID, sortDescription.numericalRangesOptional);
                break;
            case SortType.TimeRange:
                datas = DataSorter.sortByTimeRange(dataPoints, sortDescription.columnID, sortDescription.timeRangeOptional);
                break;
        }
    }

    private void createChildren(SortDescription[] childrenDescriptions, List<DataPoint> dataPoints, int sortID)
    {
        int nbChildren = datas.Count;
        children = new Dictionary<string, Bubble>(); //new Bubble[nbChildren];
        if (sortDescription.sortModif == SortModif.Bubbles && childrenDescriptions.Length >= (sortID + 1) )
        {
            foreach (string key in datas.Keys)
            {
                Bubble bbl = new Bubble(visuMaker, key, datas[key], sortID + 1, childrenDescriptions);
                children.Add(key, bbl);
            }
        }
        else if (sortDescription.sortModif != SortModif.Bubbles && childrenDescriptions.Length>(sortID+1))
        {
            Bubble bbl = new Bubble(visuMaker, keyWord, dataPoints, sortID + 1, childrenDescriptions);
            children.Add(keyWord, bbl);
        }
    }

    public void isEntered()
    {
        // call manager to create visu
        if(sortDescription.triggerType == TriggerType.Collision && children!=null)
        {
            setTrigger(true);
        }
        else if (children == null || children.Count==0)
        {
            Debug.Log("Switch to contextualized scene");
            BubbleManager manager = GameObject.FindAnyObjectByType<BubbleManager>();
            if (manager != null)
            {
                manager.zoomIn(allData);
            }
        }
    }

    public void isNotEntered()
    {
        if(sortDescription.sortModif == SortModif.Bubbles && visualization!=null)
        {
            visualization.destroyAllVisu();
            visualization.enabled = false;
            visualization.destroyVisu();
        }
        
    }

    public void childIsEntered(string childKey)
    {
        if (sortDescription.triggerType == TriggerType.Collision)
        {
            foreach (string key in children.Keys)
            {
                if (key != childKey)
                {
                    children[key].isNotEntered();
                }
            }
        }
    }

    public void isGettingCloser(float dist)
    {
        if (sortDescription.triggerType == TriggerType.Distance)
        {
            setTrigger(true);
        }
    }

    public void onTouch()
    {
        if(sortDescription.triggerType == TriggerType.Touch)
        {
            setTrigger(true);
        }
    }

    public void setTrigger(bool triggered) 
    { 
        isTrigerred = triggered;
        if (isTrigerred && keyWord!="master")
        {
            visuMaker.createBubbleVisu(this);
            if (sortDescription.sortModif == SortModif.Bubbles)
            {
                visualization.destroyVisu();
            }
            else
            {
                if (children != null && children.Count == 1)
                {
                    Debug.Log("Change visu bubbles from " + sortDescription.sortModif + " to " + children[keyWord].getSortDescription().sortModif);
                    visualization.setBubbles(this, children[keyWord]);
                    children[keyWord].setVisualization(visualization);
                }
            }
            
        }
        
    }

    public SortDescription getSortDescription() { return sortDescription; }
    public Dictionary<string, List<DataPoint>> getData() { return datas; }
    public bool getHasParent() { return hasParent; }
    public string getKey() { return keyWord; }
    public bool getTrigger() { return isTrigerred; }
}
