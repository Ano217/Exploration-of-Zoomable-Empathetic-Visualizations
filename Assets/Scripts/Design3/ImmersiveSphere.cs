using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ImmersiveSphere : MonoBehaviour
{
    private Type360Visu _visuType;
    private Texture _texture;
    private string _optionalSceneName;
    private GameObject _parent;

    public void Start()
    {
        Outline outliner = GetComponent<Outline>();
        if(outliner != null)
        {
            outliner.enabled = false;
        }
    }


    public void setSphere(Type360Visu immersiveType, GameObject sphereParent, string optionalSceneName = "")
    {
        _visuType = immersiveType;
        _optionalSceneName = optionalSceneName;
        _parent = sphereParent;
    }

    public void resetPosition()
    {
        transform.SetParent(_parent.transform);
        transform.localPosition = new Vector3(0f, 1.2f, 0.4f);
    }

    public Texture getTexture() { return _texture; }

    public string getSceneName() { return _optionalSceneName; }

    public void retrieveTexture(ImageSource source, string adress)
    {
        if(source == ImageSource.ResourcePath)
        {
            //Debug.Log("Sphere path=" + adress);
            _texture = Resources.Load<Texture>(adress);
            if( _texture == null)
            {
                Debug.Log("Could not find image " + adress);
                _texture = Resources.Load<Texture>("Materials_Textures/background");
            }
        }
        else
        {
            StartCoroutine(LoadTexture(adress));
        }
        changeTexture();
    }

    IEnumerator LoadTexture(string adress)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(adress);
        yield return www.SendWebRequest();

        _texture = DownloadHandlerTexture.GetContent(www);
    }

    public void changeTexture()
    {
        var renderer = GetComponent<Renderer>();
        //Material material = new Material(Shader.Find("Standard"));
        renderer.material.mainTexture = _texture;
        /*Material imageMat = new Material(Shader.Find("Standard"));//"Unlit/Texture"));

        // smoothness
        imageMat.SetFloat("_Glossiness", 0f);
        //transparency
        imageMat.SetFloat("_Mode", 2);
        imageMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        imageMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        imageMat.SetInt("_ZWrite", 0);
        imageMat.DisableKeyword("_ALPHATEST_ON");
        imageMat.EnableKeyword("_ALPHABLEND_ON");
        imageMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        imageMat.renderQueue = 3000;

        //Texture2D imageTexture = imageToGet as Texture2D;
        imageMat.mainTexture = imageTexture;
        img.GetComponentInChildren<UnityEngine.UI.Image>().material = imageMat;*/
    }


    public void OnCollisionEnter(Collision collision)
    {
        var sceneManager = GameObject.FindAnyObjectByType<MySceneManager>();
        if (collision.gameObject.name == "VRCamera" && transform.parent.name != _parent.name && sceneManager.getCurrentImmersiveSphere() == null)
        {
            //Debug.Log("Theoretical parent: " + _parent.name + "   actual parent:" + transform.parent.name);
            
            sceneManager.switch360View(true, this, _texture);
        }
    }

    public void OnCollisionExit(Collision collision)
    {
        var sceneManager = GameObject.FindAnyObjectByType<MySceneManager>();
        if (collision.gameObject.name == "VRCamera" && transform.parent.name != _parent.name)
        {
            sceneManager.switch360View(false);
        }
    }

    public void AttachToHand(GameObject hand, Vector3 position)
    {
        this.transform.position = position;
        this.transform.parent = hand.transform;
    }
}
