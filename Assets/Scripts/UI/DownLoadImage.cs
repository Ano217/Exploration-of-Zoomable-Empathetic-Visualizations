using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DownLoadImage : MonoBehaviour
{
    [SerializeField] private RawImage _rawImage;
    [SerializeField] private string _imageURL;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("GetTexture");
    }

    private IEnumerator GetTexture()
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(_imageURL);

        yield return request.SendWebRequest();
        if (request.isHttpError || request.isNetworkError) Debug.LogError(request.error);
        else
        {
            Debug.Log("Successfully downloaded image");
            var texture = DownloadHandlerTexture.GetContent(request);
            _rawImage.texture = texture;
        }
    }
}
