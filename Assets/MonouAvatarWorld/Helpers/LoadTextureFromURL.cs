using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LoadTextureFromURL : MonoBehaviour
{
    private Material myMaterial;
    //public RawImage myRawImage;
    public string url;
    // Start is called before the first frame update
    void Start(){
        myMaterial = GetComponent<Renderer>().material;
        StartCoroutine(DownloadImageFromURL(url));
    }

    // Update is called once per frame
    void Update()
    {
    }

    IEnumerator DownloadImageFromURL(string url){
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if(request.result == UnityWebRequest.Result.Success){
            Texture downloadedTerture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            myMaterial.mainTexture = downloadedTerture;
            //myRawImage.texture = downloadedTerture;
        }else{
            Debug.Log(request.error);
        }
    }
}
