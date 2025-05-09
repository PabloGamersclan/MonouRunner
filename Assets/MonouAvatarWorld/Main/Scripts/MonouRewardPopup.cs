using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace Monou
{
    [RequireComponent(typeof(UIDocument))]
    public class MonouRewardPopup : MonoBehaviour
    {

        private UIDocument ui;
        private VisualElement root;
        private VisualElement container;
        private int ScreenWidth = 0;
        private int ScreenHeight = 0;
        private bool isPortrate = false;
        private string GraphqlAPI;
        private string token;
        private string storageUrl;


        void Start(){
            GraphqlAPI = Main.inst.GetGraphqlAPI();
            token = Main.inst.GetToken();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateOrientation(false);
        }
        private void Prepare(){
            ui = GetComponent<UIDocument>();
            root = ui.rootVisualElement;
            container = root.Query<VisualElement>("MainContainer").First();
            root.Query<Button>().First().RegisterCallback<ClickEvent>(ev => {  ev.StopPropagation(); gameObject.SetActive(false); });
            root.Query<VisualElement>("Particles").First().Query<VisualElement>().ForEach(elem => {
                if(!elem.ClassListContains("cross")&&!elem.ClassListContains("white")&&!elem.ClassListContains("purple")&&!elem.ClassListContains("red")&&!elem.ClassListContains("donut")) return;
                float ss = UnityEngine.Random.Range(0.2f, 1f);
                float rotateOffset = UnityEngine.Random.Range(0f, 180f);
                float left = UnityEngine.Random.Range(0f, 100f);
                float top = UnityEngine.Random.Range(0f, 100f);
                elem.style.scale = new StyleScale(new Scale(new Vector2(ss,ss)));
                elem.style.left = Length.Percent(left);
                elem.style.top = Length.Percent(top);
                elem.schedule.Execute(() => {
                    top += ss; if(top>105) top=-5;
                    elem.style.top = Length.Percent(top);
                    elem.style.rotate = new Rotate(Time.time * 180 + rotateOffset);
                }).Every(32);
            });
            UpdateOrientation(true);

        }

        public void SetData(string title, string[] assetIds, string[] optionIds){
            storageUrl = GameObject.Find("MonouAvatar").GetComponent<MonouAvatar>().GetSetting("storage_url");
                    Debug.Log(">>>>>>AcquiredBundle");
                    Debug.Log(title);
                    Debug.Log(string.Join(",", assetIds));
                    Debug.Log(string.Join(",", optionIds));

            Prepare();
            root.Query<Label>().First().text = title;
            var list = root.Query<VisualElement>("List").First();
            list.Clear();
            foreach(string assetId in assetIds){
                var itm = new VisualElement();
                itm.AddToClassList("item");
                list.Add(itm);
                StartCoroutine(GetAssetIconUrl(itm, assetId));
            }
            foreach(string optionId in optionIds){
                var itm = new VisualElement();
                itm.AddToClassList("item");
                list.Add(itm);
                StartCoroutine(GetOptionIconUrl(itm, optionId));
            }
        }

            private void UpdateOrientation(bool force){
                if(!force && isPortrate==Screen.width<=Screen.height){
                    bool orientationIsChange = false;
                    if(Input.deviceOrientation == DeviceOrientation.Portrait && Screen.orientation != ScreenOrientation.Portrait){
                        Screen.orientation = ScreenOrientation.Portrait; orientationIsChange = true;
                    }
                    if(Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown && Screen.orientation != ScreenOrientation.PortraitUpsideDown){
                        Screen.orientation = ScreenOrientation.PortraitUpsideDown; orientationIsChange = true;
                    }
                    if(Input.deviceOrientation == DeviceOrientation.LandscapeLeft && Screen.orientation != ScreenOrientation.LandscapeLeft){
                        Screen.orientation = ScreenOrientation.LandscapeLeft; orientationIsChange = true;
                    }
                    if(Input.deviceOrientation == DeviceOrientation.LandscapeRight && Screen.orientation != ScreenOrientation.LandscapeRight){
                        Screen.orientation = ScreenOrientation.LandscapeRight; orientationIsChange = true;
                    }
                    if(!orientationIsChange) return;
                }
                if(Screen.width!=ScreenWidth||Screen.height!=ScreenHeight){
                    ScreenWidth = Screen.width; ScreenHeight = Screen.height;
                    if(ScreenWidth>ScreenHeight){
                        if(container.ClassListContains("portrate")) container.RemoveFromClassList("portrate");
                        if(!container.ClassListContains("landscape")) container.AddToClassList("landscape");
                        isPortrate = false;
                    }else{
                        if(container.ClassListContains("landscape")) container.RemoveFromClassList("landscape");
                        if(!container.ClassListContains("portrate")) container.AddToClassList("portrate");
                        isPortrate = true;
                    }
                }
            }

        IEnumerator SetElmBackground(VisualElement elm, string MediaUrl , bool IsSpriteSheet64) {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
            yield return request.SendWebRequest();
            if(request.result == UnityWebRequest.Result.ConnectionError) Debug.Log(request.error);
            else{
                Texture2D bg = (Texture2D)((DownloadHandlerTexture) request.downloadHandler).texture;
                if(!IsSpriteSheet64){
                    elm.style.backgroundImage = new StyleBackground(bg);
                }else{
                    int i=0;
                    int w = bg.width/8;
                    int h = bg.height/8;
                    elm.schedule.Execute(() => {
                        int x=(i%8);
                        int y=(i/8);
                        Sprite sp = Sprite.Create(bg, new Rect(x*w, y*h, w, h), Vector2.zero);
                        elm.style.backgroundImage = new StyleBackground(sp);
                        i++; if(i>=64) i=0;
                    }).Every(32);
                }


            }
        }


            private string queryGetAssetIconUrl = "query MyQuery { getAsset(id:\"$id\") { icon { url } assetType { name } } }";
        IEnumerator GetAssetIconUrl(VisualElement elm, string assetId) {
            queryGQL q = new queryGQL(queryGetAssetIconUrl.Replace("$id", assetId));
            string json = JsonUtility.ToJson(q);
            Debug.Log(json);
            UnityWebRequest www = UnityWebRequest.PostWwwForm(GraphqlAPI, json);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", token);
            www.SetRequestHeader("Content-Type", "application/json");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success) { Debug.Log(www.error); }
            else {
                Debug.Log(www.downloadHandler.text);
                var n = JsonUtility.FromJson<GQLBundleAssetReq>(www.downloadHandler.text);
                StartCoroutine( SetElmBackground(elm, storageUrl + n.data.getAsset.icon.url, n.data.getAsset.assetType.name=="anim") );
            }
        }
            private string queryGetOptionIconUrl = "query MyQuery { getAssetAttrOption(id:\"$id\") { icon { url } } }";
        IEnumerator GetOptionIconUrl(VisualElement elm, string optionId) {
            queryGQL q = new queryGQL(queryGetOptionIconUrl.Replace("$id", optionId));
            string json = JsonUtility.ToJson(q);
            Debug.Log(json);
            UnityWebRequest www = UnityWebRequest.PostWwwForm(GraphqlAPI, json);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", token);
            www.SetRequestHeader("Content-Type", "application/json");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success) { Debug.Log(www.error); }
            else {
                Debug.Log(www.downloadHandler.text);
                var n = JsonUtility.FromJson<GQLBundleOptionReq>(www.downloadHandler.text);

                StartCoroutine( SetElmBackground(elm, storageUrl + n.data.getAssetAttrOption.icon.url, false) );
            }
        }
    }


    [System.Serializable]
    public class GQLBundleAssetReq { public GQLBundleAssetData data; }
    [System.Serializable]
    public class GQLBundleAssetData { public GQLBundleAsset getAsset; }
        [System.Serializable]
        public class GQLBundleAsset {
            public GQLBundleAssetFile icon;
            public GQLBundleAssetType assetType;
        }
            [System.Serializable]
            public class GQLBundleAssetType { public string name; }
            [System.Serializable]
            public class GQLBundleAssetFile { public string url; }

    [System.Serializable]
    public class GQLBundleOptionReq { public GQLBundleOptionData data; }
    [System.Serializable]
    public class GQLBundleOptionData { public GQLBundleOption getAssetAttrOption; }
        [System.Serializable]
        public class GQLBundleOption {
            public GQLBundleOptionFile icon;
        }
            [System.Serializable]
            public class GQLBundleOptionFile { public string url; }
}