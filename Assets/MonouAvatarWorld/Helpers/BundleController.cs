using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class BundleController
{
	static void StartCoroutine(IEnumerator ienObj){ while(ienObj.MoveNext()){} }
	public static bool HasInternetConnection(){
		return Application.internetReachability != NetworkReachability.NotReachable;
	}
    public static void Get(string url, System.Action<AssetBundle> success, System.Action<string> error){
    	StartCoroutine( BundleController.LoadUrl(url, success, error) );
    }

    static IEnumerator LoadUrl(string url, System.Action<AssetBundle> success, System.Action<string> error){
    	UnityWebRequest www; Hash128 hash  = new Hash128(); hash.Append(url);
        using (www = UnityWebRequestAssetBundle.GetAssetBundle(url, hash, 0)){
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success) { error.Invoke(www.error); }
            else {
                AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(www);
                Caching.ClearOtherCachedVersions(assetBundle.name, hash);
                success.Invoke(assetBundle);
                //if(!mustPreserve) SetLoadding(false);
            }
        }
    }
}

public class ClowdfileControler
{
	static void StartCoroutine(IEnumerator ienObj){ while(ienObj.MoveNext()){} }
	public static bool HasInternetConnection(){
		return Application.internetReachability != NetworkReachability.NotReachable;
	}
	public static void Post(string url, string json, string token, System.Action<string> success, System.Action<string> error){
		if(!ClowdfileControler.HasInternetConnection()){
			string data = ClowdfileControler._LoadCache(url);
			if(data==""){ error.Invoke("empty"); }else success.Invoke(data);
		}else StartCoroutine( LoadUrl(url, json, token, success, error) );
	}

	public static IEnumerator LoadUrl(string url, string json, string token, System.Action<string> success, System.Action<string> error){
        Debug.Log(">>!0: " + url);
        Debug.Log(">>!1: " + json);
        Debug.Log(">>!2: " + token);
        UnityWebRequest www = UnityWebRequest.PostWwwForm(url, json);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Authorization", token);
        www.SetRequestHeader("Content-Type", "application/json");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success) {
            //error.Invoke(www.error);
            Debug.Log(www.error);
            Debug.Log(www.downloadHandler.text);
        }else {
            Debug.Log(">>!3: " + www.downloadHandler.text);
        	//success.Invoke(www.downloadHandler.text);
        	ClowdfileControler._SaveCache(url, www.downloadHandler.text);
        }
	}
	static string _GetPath(string url){
		Hash128 hash  = new Hash128(); hash.Append(url);
        return Application.persistentDataPath + "XOStudio"+hash.ToString()+".txt";
	}
    static string _LoadCache(string url){
        try{ return File.ReadAllText(ClowdfileControler._GetPath(url)); }
        catch( System.Exception ex ){ return ""; }
    }
    static void _SaveCache(string url, string data){ File.WriteAllText(ClowdfileControler._GetPath(url), data); }
}