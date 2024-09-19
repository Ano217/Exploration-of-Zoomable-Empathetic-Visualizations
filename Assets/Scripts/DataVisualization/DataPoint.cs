using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Burst;
using Unity.VisualScripting;
using UnityEngine;
using Valve.VR;
using static UnityEngine.GraphicsBuffer;

public class DataPoint
{
    public string[] data;
    private GameObject unitVisu;
    private GameObject humanVisu;
    private GameObject avatarPanel;
    private AvatarPanel _panelScript;
    private bool activeVisu; //true: unit   ; false: humanvisu
    private DataPointHandler pointHandler;
    private Color coloration = Color.white;
    private Material unitMaterial;
    private Morpher _unitMorpher;
    private int _prefabID;

    private int _numCol;
    private int _maxCol;

    private ImmersiveSphere _360Sphere;

    public void setColumn(int col, int totalCol)
    {
        _numCol = col;
        _maxCol = totalCol;
    }

    public int getCol() { return _numCol; }
    public int getNbColumns() { return _maxCol; }

    public void setPrefabID(int prefabID) { _prefabID = prefabID; }
    public int getPrefabId() { return _prefabID; }

    public void setData(string[] dataString)
    {
        data = dataString;
    }

    public string[] getData()
    {
        return data;
    }

    public string getColumnValue(int colID)
    {
        return data[colID];
    }

    public GameObject getVisu()
    {
        if(activeVisu) return unitVisu;
        else return humanVisu;
        //return visu;
    }

    public GameObject getUnitVisu()
    {
        return unitVisu;
    }

    public GameObject getHumanVisu()
    {
        return humanVisu;
    }

    public void setColor(Color c)
    {
        coloration = c;
        Renderer r = unitVisu.GetComponent<Renderer>();
        if (r != null)
        {
            r.material.color = coloration;
        }
        else
        {
            foreach(Renderer rend in unitVisu.GetComponentsInChildren<Renderer>())
            {
                rend.material.color = coloration;
            }
        }
    }

    public void setUnitVisu(GameObject visu)
    {
        activeVisu = true;
        unitVisu = visu;
        var foundUnitMat = unitVisu.TryGetComponent<Material>(out  unitMaterial);
        if(!foundUnitMat)
        {
            var unitRenderer = unitVisu.GetComponent<Renderer>();
            unitMaterial = unitRenderer.material;
        }
        if (humanVisu != null && humanVisu.activeSelf) humanVisu.SetActive(false);
        
    }

    public void setHumanVisu(GameObject visu, GameObject panel)
    {
        //activeVisu = false;
        humanVisu = visu;
        avatarPanel = panel;
        setPanelData();
        //if(unitVisu != null && unitVisu.activeSelf) unitVisu.SetActive(false);
    }

    public void setPanelData()
    {
        _panelScript = avatarPanel.GetComponent<AvatarPanel>();
        var sceneManager = GameObject.FindAnyObjectByType<MySceneManager>();
        if (sceneManager != null)
        {
            // set avatar
            _panelScript.setAvatar(humanVisu);

            // set title
            string title = "";
            if (sceneManager.title != null)
            {
                foreach (var c in sceneManager.title)
                {
                    if (data.Length > c && c>=0) title = title + data[c] + " ";
                }
                _panelScript.setTitle(title);
            }
            else Debug.Log("No title list");

            // set info
            if (sceneManager.infoFields != null)
            {
                List<string> list = new List<string>();
                foreach (var i in sceneManager.infoFields)
                {
                    if (i.dataIdx > 0 && i.dataIdx < data.Length) list.Add(data[i.dataIdx]);
                    else Debug.Log("dataIdx = " + i.dataIdx + "   data length=" + data.Length);
                }
                _panelScript.setInfoFields(sceneManager.infoFields, list);
            }
            else Debug.Log("no infoFields");

            // set Image
            if (sceneManager.displayImage) _panelScript.setImage(true, data[sceneManager.imageID], sceneManager.imageSource);
            else _panelScript.setImage(false);



        }
        //if (data.Length >= 7) _panelScript.setContent(humanVisu, data[2] + " " + data[3], data[1], data[4], data[5], data[6]);
    }

    public void replaceVisu(GameObject newVisu)
    {
        //if(visu!=null) GameObject.Destroy(visu);
        unitVisu = newVisu;
        pointHandler = unitVisu.GetComponent<DataPointHandler>();
        setColor(coloration);
    }

    /*public void switchVisu(bool visu)
    {
        activeVisu = visu;
        if (humanVisu != null && unitVisu != null)
        {
            enableVisu(true, visu);
            enableVisu(false, !visu);
        }
    }*/

    public bool humanVisuExists() { return  humanVisu != null; }
    public bool unitVisuExists() { return  unitVisu != null; }

    public void destroyVisu()
    {
        GameObject.Destroy(unitVisu);
        GameObject.Destroy(humanVisu);
    }

    public void enableVisu(bool enable, bool visu)
    {
        if (visu) unitVisu.SetActive(enable);
        else humanVisu.SetActive(enable);
    }

    public void enableHumanVisu(bool enable)
    {
        activeVisu = !enable;
        if (humanVisu != null) humanVisu.SetActive(enable);
    }

    [BurstCompile]
    public void enableUnitVisu(bool enable)
    {
        activeVisu = enable;
        unitVisu.SetActive(enable);
    }

    public void moveVisuTo(Vector3 targetPosition)
    {
        pointHandler.movePointTo(targetPosition);
    }

    public void switchTransparency(bool transparency)
    {
        if (transparency) MaterialExtensions.ToFadeMode(unitMaterial);
        else MaterialExtensions.ToOpaqueMode(unitMaterial);
    }

    public float setTransparency(float delta)
    {
        float a = Mathf.Max(0f, Mathf.Min(1, unitMaterial.color.a + delta));
        unitMaterial.color = new Color(unitMaterial.color.r, unitMaterial.color.g, unitMaterial.color.b, a);
        return a;
    }

    [BurstCompile]
    public bool rescaleVisu(float coef, float targetX, float targetY, float targetZ )
    {
        var targetAchieved = false;
        float x = unitVisu.transform.localScale.x;
        float y = unitVisu.transform.localScale.y;
        float z = unitVisu.transform.localScale.z;

        // x
        if (targetX < unitVisu.transform.localScale.x)
        {
            x = Mathf.Max(unitVisu.transform.localScale.x * (1-coef), targetX);
        }
        else if (targetX > unitVisu.transform.localScale.x)
        {
            x = Mathf.Min(targetX, unitVisu.transform.localScale.x * (1+coef));
        }

        // y
        if (targetY < unitVisu.transform.localScale.y)
        {
            y = Mathf.Max(unitVisu.transform.localScale.y * (1 - coef), targetY);
        }
        else if (targetY > unitVisu.transform.localScale.y)
        {
            y = Mathf.Min(unitVisu.transform.localScale.y * (1 + coef), targetY);
        }

        // z
        if (targetZ < unitVisu.transform.localScale.z)
        {
            z = Mathf.Max(unitVisu.transform.localScale.z * (1 - coef), targetZ);
        }
        else if (targetZ > unitVisu.transform.localScale.z)
        {
            z = Mathf.Min(unitVisu.transform.localScale.z * (1 + coef), targetZ);
        }

        unitVisu.transform.localScale = new Vector3(x, y, z);

        if (Mathf.Abs(x - targetX) < 0.001f && Mathf.Abs(y - targetY) < 0.001f && Mathf.Abs(z - targetZ) < 0.001f) targetAchieved = true;
        return targetAchieved;
    }

    public void setScale(Vector3 targetScale)
    {
        unitVisu.transform.localScale = targetScale;
    }

    public void setMorpher()
    {
        _unitMorpher = unitVisu.gameObject.AddComponent<Morpher>();
        //_unitMorpher.IsDeforming = false;
        GameObject blenderSphere = Resources.Load<GameObject>("Objects/blenderSphere2");
        _unitMorpher.setObjects(unitVisu.gameObject, blenderSphere);
    }

    public bool setMorpherSlider(float delta)
    {
        return _unitMorpher.alterSlider(delta);
    }

    public void enableMorpher(bool enable)
    {
        _unitMorpher.IsDeforming = enable;
    }

    public void moveUnitToAvatarCenter(float target)
    {
        unitVisu.transform.localPosition = new Vector3(unitVisu.transform.localPosition.x, unitVisu.transform.localPosition.y, target);
        //if(unitVisu.transform.localPosition.z > target) unitVisu.transform.Translate(Vector3.back * Time.deltaTime);
        //else if (unitVisu.transform.localPosition.z < target) unitVisu.transform.Translate(Vector3.forward * Time.deltaTime);
    }

    public bool haveMorpher()
    {
        return _unitMorpher != null;
    }

    public void setImmersiveSphere(ImmersiveSphere immersiveSphere)
    {
        _360Sphere = immersiveSphere;
        var sceneManager = GameObject.FindAnyObjectByType<MySceneManager>();

        if (sceneManager.immersiveVisu == Type360Visu.Image360)
        {
            _360Sphere.setSphere(sceneManager.immersiveVisu, humanVisu);
            if(data.Length > sceneManager.immersiveLocationField)
            {
                var imageAdress = data[sceneManager.immersiveLocationField];
                _360Sphere.retrieveTexture(sceneManager.typeImmersiveImage, imageAdress);
            }
            else
            {
                _360Sphere.retrieveTexture(ImageSource.ResourcePath, "Materials_Textures/background");
            }

        }
        else
        {
            // TODO
        }
    }

    public Vector3 getPointPosition()
    {
        return humanVisu.transform.position;
    }

}
