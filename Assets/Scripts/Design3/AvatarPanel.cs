using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using static Unity.VisualScripting.Member;

public class AvatarPanel : MonoBehaviour
{

    public TextMeshProUGUI title;
    //public  img;
    public RawImage rawImage;
    private string _imageURL;
    private ImageSource _imageSource;
    private bool _displayImage;
    //public GameObject img;
    public GameObject panel;
    private bool _loadedImage = false;

    //public TextMeshProUGUI info1;

    private List<TextMeshProUGUI> _infoTextMeshPros;
    private GameObject _avatar;

    public void setTitle(string titleTxt) { title.text = titleTxt; }

    public void setAvatar(GameObject avatar) { _avatar = avatar; }

    public void setInfoFields(List<PanelInfoField> fieldsDescr, List<string> content)
    {
        _infoTextMeshPros = new List<TextMeshProUGUI>();
        var y = 0;
        for (var i=0; i<Mathf.Min(fieldsDescr.Count, content.Count); i++)
        {
            //var inf = new TextMeshProUGUI();
            var infContener = new GameObject();
            var inf = infContener.AddComponent<TextMeshProUGUI>();
            infContener.transform.SetParent(panel.transform);
            inf.rectTransform.localPosition = new Vector3(10, 60 - y * 50, 0);
            inf.rectTransform.rotation = new Quaternion(0, 0, 0, 0); 
            inf.rectTransform.localScale = Vector3.one;
            inf.rectTransform.sizeDelta = new Vector2(450, 100);

            inf.color = Color.black;
            
            string fieldTxt = fieldsDescr[i].label+":  " + content[i];
            
            //if (content[i].Length > 10) y += 2;
            //else y++;
            inf.text = fieldTxt;
            int nbLines = 1 + (inf.text.Length / 26);
            y += Mathf.Max(1, nbLines);
            //TODO IF LINK
        }
    }

    public void displayPanel()
    {
        if (!_loadedImage && _displayImage)
        {
            downloadImage();
        }
    }

    public void setImage(bool display, string address= "", ImageSource source=ImageSource.ResourcePath)
    {
        _displayImage = display;
        _imageURL = address;
        _imageSource = source;
        downloadImage();
    }
    public void downloadImage()
    {
        if (_displayImage && _imageURL.Length > 0 && _imageURL != "")
        {
            if (_imageSource == ImageSource.ResourcePath)
            {
                Texture2D text = Resources.Load<Texture2D>(_imageURL);
                if (text == null) rawImage.gameObject.SetActive(false);
                else changeTexture(text);
                _loadedImage = true;
            }
            else if (_imageSource == ImageSource.WebUrl)
            {
                StartCoroutine("GetTexture");
            }
        }
        else
        {
            _displayImage = false;
            rawImage.gameObject.SetActive(false);
        }
    }



    // source: https://m-ansley.medium.com/unity-web-requests-downloading-an-image-e88d7389dd5a
    private IEnumerator GetTexture()
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(_imageURL);

        yield return request.SendWebRequest();
        if (request.isHttpError || request.isNetworkError) Debug.Log("Loading failed: " + request.error);//Debug.LogError(request.error);
        else
        {
            var texture = DownloadHandlerTexture.GetContent(request);
            rawImage.texture = texture;
            _loadedImage = true;
        }
    }

    /*IEnumerator LoadTexture(string address)
    {
        Debug.Log("Couroutine LoadTexture");
       
        UnityWebRequest www = UnityWebRequestTexture.GetTexture("https://www.my-server.com/image.png");
        yield return www.SendWebRequest();

        Debug.Log("Request sent");
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            Texture2D texture2D = myTexture as Texture2D;
            changeTexture(texture2D);
        }

    }*/

    public void changeTexture(Texture2D imageTexture)
    {
        Material imageMat = new Material(Shader.Find("Standard"));//"Unlit/Texture"));

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
        rawImage.GetComponentInChildren<UnityEngine.UI.Image>().material = imageMat;
    }

    public void setContent(GameObject avatar, string titleTxt, string info_1, string info_2, string info_3, string info_4)
    {
        _avatar = avatar;
        title.text = titleTxt;
    }

    
}

public enum ImageSource { WebUrl, ResourcePath }
public enum InfoFieldType { Text, Link}

[Serializable]
public class PanelInspector : ScriptableObject
{
    [SerializeField] public List<int> title;
    public bool displayImage;
    public ImageSource imageSource;
    public int imageID;
    [SerializeField] public List<PanelInfoField> infoFields;
}

[Serializable]
public struct PanelInfoField
{
    public int dataIdx;
    public string label;
    public InfoFieldType fieldType;
}
