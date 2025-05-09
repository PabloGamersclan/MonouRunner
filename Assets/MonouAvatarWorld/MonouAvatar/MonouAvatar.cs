/*
MonouAvatar ma = GetComponent<MonouAvatar>();
ma.setToken(string token);

OnInitialDataLoaded += fn(mainUserId);
    string[] usrIds = {mainUserId};
    ma.LoadUserData(usrIds);

OnUserDataLoaded += fn(string usrId);
    GameObject go = ma.create(usrId[, Vector3 pos, Quaternion rot]);

OnUpdated += fn(GameObject go, bool isCreate, string userId);
    if(!isCreate) return;
    DynamicCharacterAvatar avatar = go.GetComponent<DynamicCharacterAvatar>();
    ma._updateAvatarDNA("Height", Random.Range(0.25f, 0.75f).ToString(), avatar);
    ma._updateAvatarColor("Skin", skinColors[idx], avatar);
    ma.AddCloth("asset_guantes", "slot_Hnds", "http:/asset_url", avatar);
    ma.putItemsAdded(avatar);

GameObject clonObj = ma.clon(GameObject goSource)
*/
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Globalization;
using UMA;
using UMA.CharacterSystem;


namespace Monou
{
    public class MonouAvatar : MonoBehaviour
    {
        public static MonouAvatar inst;
        void Awake(){
            if(MonouAvatar.inst == null){ MonouAvatar.inst = this; /*DontDestroyOnLoad(gameObject);*/ }
            else Destroy(gameObject);
        }

        public string GraphqlAPI;
        public GameObject prefab;
        public GameObject ParentObject;
        public bool skipAnimation = false;
        public int prelodedAvatars = 8;
        private static string monouToken = "";
        private static string monouUserId = "";
        private static int monouUserMoney = 0;
        private static string monouStorageURL = null;
        private Dictionary<string, string> settings = new Dictionary<string, string>();
        private Dictionary<string, string> userWardrobeIds =  new Dictionary<string, string>();
        private bool isLoaded = false;
        private PreloadedAvatar[] prelodedAvatarTable;

        private string queryInitial = "query MyQuery {" +
                "  getMe {id userWardrobeId money}" +
                "  listSettings {items {value name} }" +
                "}";
        private string queryUsers = "query MyQuery {" +
                " getUser(id: \"$userId\"){" +
                "  wardrobes {" +
                "    items {" +
                "      id" +
                "      userID" +
                "      wardrobeAssets {" +
                "        items {" +
                "          options" +
                "          asset {" +
                "            assetType {name}" +
                "            assetunity {url}" +
                "            assetandroid {url}" +
                "            assetdesk {url}" +
                "            assetios {url}" +
                "            assetosx {url}" +
                "            attributes {" +
                "              items {" +
                "                id" +
                "                name" +
                "                options {items {id value} }" +
                "              }" +
                "            }" +
                "            configs {items {name value} }" +
                "            name" +
                "            key" +
                "            zindex" +
                "          } } } } } } }";

        void Start() {
            if (ParentObject == null) ParentObject = this.gameObject;
            prelodedAvatarTable = new PreloadedAvatar[prelodedAvatars];
            for(int i=0; i<prelodedAvatars; i++){
                PreloadedAvatar pa = new PreloadedAvatar(GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity));
                pa.go.transform.parent = ParentObject.transform;
                pa.go.SetActive(false);
                prelodedAvatarTable[i] = pa;
            }
        }

        void OnEnable() {
            StartCoroutine(_setItems());
        }

    //    private DynamicCharacterAvatar avatar;
    //    private Animator animator;

        private UMAData UD;

        //private static List<UserAvatar> avatares = new List<UserAvatar>();
        private static List<Item> items = new List<Item>();
        private static List<UserData> usersData = new List<UserData>();

        public delegate void InitialDataLoaded(string usrId);
        public event InitialDataLoaded OnInitialDataLoaded;
        public delegate void UserDataLoaded(string usrId, bool requirePresets);
        public event UserDataLoaded OnUserDataLoaded;
        public delegate void Updated(GameObject go, bool isCreate, string userId);
        public event Updated OnUpdated;
        public delegate void AuthError(string token);
        public event AuthError OnAuthError;

        public void setToken(string token){
            if(monouToken.Length>0) return;
            monouToken=token;
            StartCoroutine(_loadInitialData());
        }
        public bool LoadAndCreateUserPreloaded(string userId, int level, out GameObject go){

            foreach(var pa in prelodedAvatarTable) {
                if(!pa.isActive || level<pa.level) {
                    pa.id=userId;
                    pa.isActive = true;
                    pa.go.SetActive(true);
                    pa.level = level;
                    go = pa.go;
                    var avatar = go.GetComponent<DynamicCharacterAvatar>();
                    foreach (SlotData slot in avatar.umaData.umaRecipe.slotDataList) avatar.ClearSlot(slot.slotName);
                    GameObject[] gos = new GameObject[1]; gos[0] = go; string[] userIds = {userId}; StartCoroutine(_loadUserData(userIds, gos));
                    return true;
                }
            }
            go = new GameObject(); return false;
        }
        public void RemoveUserPreloaded(string userId){
            Debug.Log("Try RemoveAvatar "+ userId);
            foreach(var pa in prelodedAvatarTable){
                Debug.Log("test "+pa.id);
                if(pa.id==userId){ pa.go.SetActive(false); pa.isActive = false;  return; }
            }
        }
        public GameObject LoadAndCreateUser(string userId){
            Debug.Log("Try create user "+userId);
            string[] userIds = {userId};
            GameObject[] gos = new GameObject[1];
            gos[0] = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);
            var avatar = gos[0].AddComponent<DynamicCharacterAvatar>();
            avatar.context = UMAContextBase.Instance;
            avatar.ChangeRace("Nobody");
            avatar.umaGenerator = Component.FindObjectOfType<UMAGeneratorBase>();
            avatar.raceAnimationControllers.defaultAnimationController = Resources.Load("poset.anim") as RuntimeAnimatorController;
            StartCoroutine(_loadUserData(userIds, gos));
            return gos[0];
        }
        public void LoadUserData(string[] userIds){
            GameObject[] gos = new GameObject[0];
            StartCoroutine(_loadUserData(userIds, gos));
        }
        public void LoadUserData(){
            string[] userIds = {};
            GameObject[] gos = new GameObject[1];
            StartCoroutine(_loadUserData(userIds, gos));
        }
        public GameObject create(string usrId, GameObject go){
            if (prefab == null) return go;
            go.name = "MonouCharacter_" + usrId;
            if (ParentObject != null) go.transform.parent = ParentObject.transform;
            DynamicCharacterAvatar avatar = go.GetComponent<DynamicCharacterAvatar>();
            avatar.SetAnimatorController(true);
            avatar.WardrobeRecipes.Clear();
            BuildUser(usrId, avatar);
            return go;
        }
        public GameObject create(string usrId, Vector3 pos, Quaternion rot){
            if (prefab == null) return null;
            GameObject go = GameObject.Instantiate(prefab, pos, rot);
            go.name = "MonouCharacter_" + usrId;
            if (ParentObject != null) go.transform.parent = ParentObject.transform;
            DynamicCharacterAvatar avatar = go.GetComponent<DynamicCharacterAvatar>();
            avatar.SetAnimatorController(true);
            avatar.WardrobeRecipes.Clear();
            BuildUser(usrId, avatar);
            return go;
        }
        public GameObject create(string userId){
            return create(userId, Vector3.zero, Quaternion.identity);
        }

        private int fromScratchCounter = 0;
        public GameObject createFromScratch(string urlRace, string keyRace, string urlAnim, string keyAnim){
            if (prefab == null) return null;
            GameObject go = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);
            go.name = "MonouCharacter_" + fromScratchCounter.ToString();
            if (ParentObject != null) go.transform.parent = ParentObject.transform;
            DynamicCharacterAvatar avatar = go.GetComponent<DynamicCharacterAvatar>();
            avatar.SetAnimatorController(true);
            avatar.WardrobeRecipes.Clear();
            List<Item> items = new List<Item>();
            List<ItemAttribute> optionsAttr = new List<ItemAttribute>();
            items.Add(new Item(keyRace, "race", "", urlRace, optionsAttr));
            items.Add(new Item(keyAnim, "anim", "", urlAnim, optionsAttr));
            Queue ddd = new Queue(avatar, items, true, "");
            itemsQueue.Add(ddd);
            fromScratchCounter++;
            return go;
        }
        public bool createFromScratchPreloaded(string urlRace, string keyRace, string urlAnim, string keyAnim, out GameObject go){
            foreach(var pa in prelodedAvatarTable) {
                if(!pa.isActive) {
                    pa.id="MonouCharacter_" + fromScratchCounter.ToString();
                    pa.isActive = true;
                    pa.go.SetActive(true);
                    pa.level = 1;
                    go = pa.go;
                    var avatar = go.GetComponent<DynamicCharacterAvatar>();
                    foreach (SlotData slot in avatar.umaData.umaRecipe.slotDataList) avatar.ClearSlot(slot.slotName);
                    // avatar.SetAnimatorController(true);
                    // avatar.WardrobeRecipes.Clear();
                    // List<Item> items = new List<Item>();
                    // List<ItemAttribute> optionsAttr = new List<ItemAttribute>();
                    // items.Add(new Item(keyRace, "race", "", urlRace, optionsAttr));
                    // items.Add(new Item(keyAnim, "anim", "", urlAnim, optionsAttr));
                    // Queue ddd = new Queue(avatar, items, true, "");
                    // itemsQueue.Add(ddd);
                    // fromScratchCounter++;
                    avatar.ChangeRace("MonouT");
                    avatar.BuildCharacter();
                    return true;
                }
            }
            go = new GameObject(); return false;
        }

        private List<Queue> itemsQueue = new List<Queue>();
        public bool BuildUser(string usrId, DynamicCharacterAvatar avatar){
            int idx = usersData.FindIndex(x=>x.userId == usrId);
            if(idx >= 0){
                if(monouUserId=="") monouUserId = usrId;
                foreach(UserData ud in usersData) if(ud.userId == usrId) { ud.avatar=avatar; break; }
                Queue ddd = new Queue(avatar, usersData[idx].items, true, usrId);
                itemsQueue.Add(ddd);
                return true;
            }
            return false;
        }

        private List<Item> putItemList = new List<Item>();
        public void AddItem(string name, string type, string category, string url, Dictionary<string, string> options,  DynamicCharacterAvatar avatar){
            List<ItemAttribute> optionsAttr = new List<ItemAttribute>();
            foreach (var opt in options) optionsAttr.Add(new ItemAttribute(opt.Key,"",opt.Value));
            putItemList.Add(new Item(name, type, category, url, optionsAttr));
        }
        public void AddItem(string name, string type, string category, string url, DynamicCharacterAvatar avatar){
            Dictionary<string, string> options = new Dictionary<string, string>();
            AddItem(name, type, category, url, options, avatar);
        }

        public void AddRace(string name, string url, DynamicCharacterAvatar avatar){
            AddItem(name, Item.RACETYPE, "", url, avatar);
        }
        public void AddRace(string name, string url, Dictionary<string, string> options, DynamicCharacterAvatar avatar){
            AddItem(name, Item.RACETYPE, "", url, options, avatar);
        }

        public void AddCloth(string name, string category, string url, DynamicCharacterAvatar avatar){
            AddItem(name, Item.CLOTHTYPE, category, url, avatar);
        }
        public void AddCloth(string name, string category, string url, Dictionary<string, string> options, DynamicCharacterAvatar avatar){
            AddItem(name, Item.CLOTHTYPE, category, url, options, avatar);
        }

        public void AddAnim(string name, string url, DynamicCharacterAvatar avatar){
            AddItem(name, Item.ANIMTYPE, "", url, avatar);
        }
        public void putItemsAdded(DynamicCharacterAvatar avatar){
            string usrId = "";
            foreach(UserData ud in usersData) if(ud.avatar == avatar) { usrId = ud.userId; break; }
            Queue ddd = new Queue(avatar, putItemList, false, usrId);
            itemsQueue.Add(ddd);
            putItemList = new List<Item>();
        }
        public void RemoveCloth(string category, DynamicCharacterAvatar avatar){
            avatar.ClearSlot(category);
        }
        public void ReBuild(DynamicCharacterAvatar avatar){
            avatar.BuildCharacter();
            string usrId = "";
            foreach(UserData ud in usersData) if(ud.avatar == avatar) { usrId = ud.userId; break; }
            if(OnUpdated != null) OnUpdated(avatar.gameObject, false, usrId);
        }
        public string GetData(DynamicCharacterAvatar avatar){
            return avatar.GetCurrentRecipe();
        }
        public void SetData(string data, DynamicCharacterAvatar avatar){
            avatar.SetLoadString(data);
        }


        public GameObject clon(GameObject goSource){
            if (prefab == null) return null;
            GameObject go = GameObject.Instantiate(prefab, goSource.transform.position, goSource.transform.rotation);
            if (ParentObject != null) go.transform.parent = ParentObject.transform;
            DynamicCharacterAvatar avatarSource = goSource.GetComponent<DynamicCharacterAvatar>();
            if (avatarSource == null) return null;
            string data = avatarSource.GetCurrentRecipe();
            DynamicCharacterAvatar avatar = go.GetComponent<DynamicCharacterAvatar>();
            avatar.SetLoadString(data);
            return go;
        }


        public void addItemFromId(string itemId, DynamicCharacterAvatar avatar){
            StartCoroutine(_addItemFromId(itemId, avatar));
        }
        private string queryGetAsset = "query MyQuery {getAsset(id: \"$id\") { assetType {name} assetandroid {url} assetdesk {url} assetios {url} assetosx{url} assetunity {url} configs { items { name value } } key name zindex} }";
        IEnumerator _addItemFromId(string itemId, DynamicCharacterAvatar avatar){
            queryGQL q = new queryGQL(queryGetAsset.Replace("$id", itemId));
            string json = JsonUtility.ToJson(q);
            Debug.Log(json);
            UnityWebRequest www = UnityWebRequest.PostWwwForm(GraphqlAPI, json);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", monouToken);
            www.SetRequestHeader("Content-Type", "application/json");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            //Debug.Log("sending json");
            yield return www.SendWebRequest();
            //Debug.Log("sending json successs");
            if (www.result != UnityWebRequest.Result.Success) {
                monouToken = "";
                Debug.Log(www.error); Debug.Log(www.downloadHandler.text);
                if(OnAuthError != null) OnAuthError(monouToken);
            } else {
                //Debug.Log(www.downloadHandler.text);
                var n = JsonUtility.FromJson<GQLGetAsset>(www.downloadHandler.text);
                if(n.data != null && n.data.getAsset != null){
                    string category = "";
                    foreach(var cnf in n.data.getAsset.configs.items) if(cnf.name =="slot") category=cnf.value;
                    string asseturl="";
                    if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer) asseturl = n.data.getAsset.assetosx.url;
                    if (Application.platform == RuntimePlatform.WindowsEditor) asseturl = n.data.getAsset.assetdesk.url;
                    if (Application.platform == RuntimePlatform.WebGLPlayer) asseturl = n.data.getAsset.assetunity.url;
                    if (Application.platform == RuntimePlatform.Android) asseturl = n.data.getAsset.assetandroid.url;
                    if (Application.platform == RuntimePlatform.WindowsPlayer) asseturl = n.data.getAsset.assetdesk.url;
                    if (Application.platform == RuntimePlatform.IPhonePlayer) asseturl = n.data.getAsset.assetios.url;
                    AddItem(n.data.getAsset.key, n.data.getAsset.assetType.name, category, asseturl, avatar);
                    putItemsAdded(avatar);
                }
            }
        }

        IEnumerator _loadInitialData(){
            // realiza la query inicial al servicio de GraphQL
            queryGQL q = new queryGQL(queryInitial);
            string json = JsonUtility.ToJson(q);
            Debug.Log(json);
            UnityWebRequest www = UnityWebRequest.PostWwwForm(GraphqlAPI, json);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", monouToken);
            www.SetRequestHeader("Content-Type", "application/json");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            Debug.Log("sending json");
            yield return www.SendWebRequest();
            Debug.Log("sending json successs");
            if (www.result != UnityWebRequest.Result.Success) {
                monouToken = "";
                Debug.Log(www.error); Debug.Log(www.downloadHandler.text);
                if(OnAuthError != null) OnAuthError(monouToken);
            } else {
                //Debug.Log(www.downloadHandler.text);
                var n = JsonUtility.FromJson<GQLInitialData>(www.downloadHandler.text);
                if(n.data != null && n.data.getMe != null){
                    // define el Id del usuario principal
                    monouUserId = n.data.getMe.id;
                    monouUserMoney = n.data.getMe.money;
                    if(n.data.listSettings != null){
                        var items = n.data.listSettings.items;
                        for(int s=0; s<items.Count; s++){
                            settings[items[s].name] = items[s].value;
                            string settingName = items[s].name;
                            switch(settingName){
                                // define la url del storage
                                case "storage_url": monouStorageURL = items[s].value; break;
                            }
                        };
                        if(OnInitialDataLoaded != null) OnInitialDataLoaded(monouUserId);
                        isLoaded = true;
                    }
                }
            }
        }

        IEnumerator _loadUserData(string[] userIds, GameObject[] gos){
            // construye la query de busqueda de usuarios
            Debug.Log("loadData from "+ userIds);

        for(int i=0; i<userIds.Length; i++){
            // flujo de carga normal
            Debug.Log(userIds[i]);
            queryGQL q = new queryGQL(queryUsers.Replace("$userId", (userIds[i].Length>=5 && userIds[i].Substring(0,5)=="guest")?"guest":userIds[i] ));
            string json = JsonUtility.ToJson(q);
            Debug.Log(json);
            UnityWebRequest www = UnityWebRequest.PostWwwForm(GraphqlAPI, json);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", monouToken);
            www.SetRequestHeader("Content-Type", "application/json");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success) { Debug.Log(www.error); Debug.Log(www.downloadHandler.text); }
            else {
                Debug.Log("success");
                Debug.Log(www.downloadHandler.text);
                var n = JsonUtility.FromJson<GQLUData>(www.downloadHandler.text);
                if(n.data != null && n.data.getUser != null && n.data.getUser.wardrobes != null && n.data.getUser.wardrobes.items != null && n.data.getUser.wardrobes.items.Count > 0){
                    Debug.Log("-->1");
                    var wardrobe = n.data.getUser.wardrobes.items[0];
                    var uId = userIds[i];//wardrobe.userID;
                    int idx = usersData.FindIndex(x=>x.userId == uId);
                    if(idx >= 0) continue;
                    Debug.Log("-->1.1");
                    Debug.Log(wardrobe.wardrobeAssets.items.Count);
                    List<Item> itemList = new List<Item>();
                    for(int wa=0; wa<wardrobe.wardrobeAssets.items.Count; wa++){
                        Debug.Log("-->1.2");
                        var wAssets = wardrobe.wardrobeAssets.items[wa];
                        var options = wAssets.options;
                        var asset = wAssets.asset;
                        string category = "";
                        List<ItemAttribute> attributes = new List<ItemAttribute>();
                        if(asset.configs!=null && asset.configs.items!=null)
                            for(int c=0; c<asset.configs.items.Count; c++){
                                var conf = asset.configs.items[c];
                                string confName = conf.name;
                                switch(confName){
                                    case "slot": category = conf.value; break;
                                    default: attributes.Add( new ItemAttribute(confName, "", conf.value) );  break;
                                }
                            }
                        Debug.Log("-->1.3");
                        if(asset.attributes!=null && asset.attributes.items!=null)
                        for(int a=0; a<asset.attributes.items.Count; a++){
                            var attr = asset.attributes.items[a];
                            if(attr.options!=null && attr.options.items!=null)
                            for(int o=0; o<attr.options.items.Count; o++){
                                var opt = attr.options.items[o];
                                for(int p=0; p<options.Count; p++) if(opt.id == options[p]){
                                    bool isPrefinded = false;
                                    for(int g=0; g<attributes.Count; g++) if(attributes[g].name == attr.name){
                                        isPrefinded=true;
                                        attributes[g].val = opt.value;
                                        break;
                                    }
                                    if(!isPrefinded) attributes.Add( new ItemAttribute(attr.name, "", opt.value) );
                                    continue;
                                }
                            }
                        }
                        string asseturl="";
                        if (Application.platform == RuntimePlatform.WindowsEditor) asseturl = asset.assetdesk.url;
                        if (Application.platform == RuntimePlatform.WebGLPlayer) asseturl = asset.assetunity.url;
                        if (Application.platform == RuntimePlatform.Android) asseturl = asset.assetandroid.url;
                        if (Application.platform == RuntimePlatform.WindowsPlayer) asseturl = asset.assetdesk.url;
                        if (Application.platform == RuntimePlatform.IPhonePlayer) asseturl = asset.assetios.url;
                        if (Application.platform == RuntimePlatform.OSXPlayer) asseturl = asset.assetosx.url;
                        if (Application.platform == RuntimePlatform.OSXEditor) asseturl = asset.assetosx.url;
                        Debug.Log(asset.key+", "+asset.assetType.name+", "+category+", "+asseturl);
                        itemList.Add(new Item(asset.key, asset.assetType.name, category, asseturl, attributes));
                    }
                    Debug.Log("-->3");
                    usersData.Add(new UserData(uId, itemList));
                    userWardrobeIds[uId] = n.data.getUser.wardrobes.items[0].id;
                    int counter = 0;
                    foreach(var itm in n.data.getUser.wardrobes.items[0].wardrobeAssets.items){ counter ++; if(itm.options!=null) counter += itm.options.Count; }
                    bool requirePresets = counter<=2;
                    if(OnUserDataLoaded != null) OnUserDataLoaded(uId, requirePresets);
                    if(gos.Length>i) create(uId, gos[i]);
                }
                Debug.Log("-->4");
            }
        }
        }

        IEnumerator _setItems() {
            List<Item> _items;
            DynamicCharacterAvatar avatar;
            bool isCreate;
            string userId;
            UnityWebRequest www;
            AssetBundle assetBundle;
            int idx;
            do{
                if(itemsQueue.Count == 0){ yield return new WaitForSeconds(.01F); continue; }
                _items = itemsQueue[0].items;
                avatar = itemsQueue[0].avatar;
                isCreate = itemsQueue[0].isCreate;
                userId = itemsQueue[0].userId;
                itemsQueue.RemoveAt(0);
                _items.Sort((p1,p2) => Item.typeSort(p1)-Item.typeSort(p2) );
                foreach(Item i in _items){
                    idx = items.FindIndex(x=>x.url == i.url);
                    if(idx >= 0){ i.reppeted = true; }else items.Add(i);
                }
                bool haveRace = false;
                bool haveAnime = false;
                foreach(Item i in _items){
                    if(i.url == null) continue;
                    string url = i.url.Substring(0, 4) == "http"? i.url: monouStorageURL + i.url;
                    if(i.type == Item.ANIMTYPE) haveAnime=true;
                    if(skipAnimation && i.type == Item.ANIMTYPE) continue;
                    if(i.type == Item.RACETYPE) haveRace=true;
                    print("setItem");
                    print(url);
                    if(i.reppeted||url==""){ workAssetBundle(i, null, avatar, true); }else
                    using (www = UnityWebRequestAssetBundle.GetAssetBundle(url)){
                        yield return www.SendWebRequest();
                        if (www.result != UnityWebRequest.Result.Success) { Debug.Log(www.error); }
                        else {
                            print("step 0");
                            assetBundle = DownloadHandlerAssetBundle.GetContent(www);
                            workAssetBundle(i, assetBundle, avatar, false);
                            if(i.type != Item.RACETYPE && i.type != Item.ANIMTYPE) assetBundle.Unload(false);
                           // if(OnEquip != null) OnEquip(i.name);
                            yield return new WaitForSeconds(.01F);
                        }
                    }          
                }
                GQLGetAsset n;
                string asseturl;
                Item j;
                queryGQL q;
                string json;
                byte[] jsonToSend;
                if(!haveRace && isCreate){
                    if(!settings.ContainsKey("default_asset_race")) continue;

            // *** TODO LO SIGUIENTE ES POR SI EL USUARIO ES PINCHES DESESPERADO Y NO TERMINA DE GUARDAR CUANDO ACTUALIZA
            // *** COMO NO SE GURDO LA RAZA, LE ASIGNA UNA POR DEFAULT
            q = new queryGQL("query MyQuery{ getAsset(id:\""+settings["default_asset_race"]+"\"){ key, assetunity {url}, assetandroid {url}, assetdesk {url}, assetios {url} assetosx {url} } }");
            json = JsonUtility.ToJson(q); Debug.Log(json);
            www = UnityWebRequest.PostWwwForm(GraphqlAPI, json);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", monouToken);
            www.SetRequestHeader("Content-Type", "application/json");
            jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success) {
                Debug.Log(www.error); Debug.Log(www.downloadHandler.text);
                if(OnAuthError != null) OnAuthError(monouToken);
            } else {
                //Debug.Log(www.downloadHandler.text);
                n = JsonUtility.FromJson<GQLGetAsset>(www.downloadHandler.text);
                asseturl="";
                if (Application.platform == RuntimePlatform.WindowsEditor) asseturl = n.data.getAsset.assetdesk.url;
                if (Application.platform == RuntimePlatform.WebGLPlayer) asseturl = n.data.getAsset.assetunity.url;
                if (Application.platform == RuntimePlatform.Android) asseturl = n.data.getAsset.assetandroid.url;
                if (Application.platform == RuntimePlatform.WindowsPlayer) asseturl = n.data.getAsset.assetdesk.url;
                if (Application.platform == RuntimePlatform.IPhonePlayer) asseturl = n.data.getAsset.assetios.url;
                if (Application.platform == RuntimePlatform.OSXPlayer) asseturl = n.data.getAsset.assetosx.url;
                if (Application.platform == RuntimePlatform.OSXEditor) asseturl = n.data.getAsset.assetosx.url;
                List<ItemAttribute> optionsAttr = new List<ItemAttribute>();
                j = new Item(n.data.getAsset.key, "race", "", asseturl, optionsAttr);
                idx = items.FindIndex(_=>_.url == j.url);
                if(idx >= 0){ workAssetBundle(j, null, avatar, true); }else{
                    using (www = UnityWebRequestAssetBundle.GetAssetBundle(monouStorageURL + asseturl)){
                        yield return www.SendWebRequest();
                        if (www.result != UnityWebRequest.Result.Success) { Debug.Log(www.error); }
                        else {
                            items.Add(j);
                            assetBundle = DownloadHandlerAssetBundle.GetContent(www);
                            workAssetBundle(j, assetBundle, avatar, false);
                            yield return new WaitForSeconds(.01F);
                        }
                    } 
                }
            }
                }
                if(!haveAnime && isCreate){
                    if(!settings.ContainsKey("default_asset_animation")) continue;
            // *** TODO LO SIGUIENTE ES POR SI EL USUARIO ES PINCHES DESESPERADO Y NO TERMINA DE GUARDAR CUANDO ACTUALIZA
            // *** COMO NO SE GURDO LA RAZA, LE ASIGNA UNA POR DEFAULT
            q = new queryGQL("query MyQuery{ getAsset(id:\""+settings["default_asset_animation"]+"\"){ key, assetunity {url}, assetandroid {url}, assetdesk {url}, assetios {url} assetosx {url} } }");
            json = JsonUtility.ToJson(q); Debug.Log(json);
            www = UnityWebRequest.PostWwwForm(GraphqlAPI, json);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", monouToken);
            www.SetRequestHeader("Content-Type", "application/json");
            jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success) {
                Debug.Log(www.error); Debug.Log(www.downloadHandler.text);
                if(OnAuthError != null) OnAuthError(monouToken);
            } else {
                Debug.Log(www.downloadHandler.text);
                n = JsonUtility.FromJson<GQLGetAsset>(www.downloadHandler.text);
                asseturl="";
                if (Application.platform == RuntimePlatform.WindowsEditor) asseturl = n.data.getAsset.assetdesk.url;
                if (Application.platform == RuntimePlatform.WebGLPlayer) asseturl = n.data.getAsset.assetunity.url;
                if (Application.platform == RuntimePlatform.Android) asseturl = n.data.getAsset.assetandroid.url;
                if (Application.platform == RuntimePlatform.WindowsPlayer) asseturl = n.data.getAsset.assetdesk.url;
                if (Application.platform == RuntimePlatform.IPhonePlayer) asseturl = n.data.getAsset.assetios.url;
                if (Application.platform == RuntimePlatform.OSXPlayer) asseturl = n.data.getAsset.assetosx.url;
                if (Application.platform == RuntimePlatform.OSXEditor) asseturl = n.data.getAsset.assetosx.url;
                List<ItemAttribute> optionsAttr = new List<ItemAttribute>();
                j = new Item(n.data.getAsset.key, "anim", "", asseturl, optionsAttr);
                idx = items.FindIndex(_=>_.url == j.url);
                if(idx >= 0){ workAssetBundle(j, null, avatar, true); }else{
                    using (www = UnityWebRequestAssetBundle.GetAssetBundle(monouStorageURL + asseturl)){
                        yield return www.SendWebRequest();
                        if (www.result != UnityWebRequest.Result.Success) { Debug.Log(www.error); }
                        else {
                            items.Add(j);
                            assetBundle = DownloadHandlerAssetBundle.GetContent(www);
                            workAssetBundle(j, assetBundle, avatar, false);
                            yield return new WaitForSeconds(.01F);
                        }
                    } 
                }
            }
                }
                avatar.BuildCharacter();

                yield return new WaitForSeconds(0.01F);
                foreach(UserData ud in usersData) if(ud.avatar == avatar) {
                    foreach(Item i in _items) foreach(ItemAttribute iattr in i.options) workAttributesColor(iattr.name, iattr.val, avatar);
                    foreach(Item i in _items) foreach(ItemAttribute iattr in i.options) if(iattr.name=="Overlay"){
                        string[] words = iattr.val.Split('?');
                        if(words.Length>1){ ud.overlays.Add(new Overlay(words[0], words[1])); }
                    }
                    workOverlays(avatar);
                    break;
                }
                avatar.BuildCharacter();
                if(OnUpdated != null) OnUpdated(avatar.gameObject, isCreate, userId);
            }while(true);
        }

        public void workOverlays(DynamicCharacterAvatar avatar){
            foreach(UserData ud in usersData) if(ud.avatar == avatar) {
                foreach(Overlay ov in ud.overlays) _addOverlay(ov.slot, ov.overlay, avatar, false);
                avatar.UpdateSameRace();
                break;
            }
        }

        private List<AnimCtrl> anims = new List<AnimCtrl>();
        private void workAssetBundle(Item i, AssetBundle assetBundle, DynamicCharacterAvatar avatar, bool isCached) {
            foreach(ItemAttribute iattr in i.options){
                switch(iattr.name){
                    case "Version":
                        i.name = iattr.val; break;
                }
            }
            switch(i.type){
                case Item.RACETYPE:
                    print("work RACE: "+avatar.activeRace.name + " != " + i.name);
                    if(assetBundle != null) UMAAssetIndexer.Instance.AddFromAssetBundle(assetBundle);
                    if(avatar.activeRace.name != i.name) avatar.ChangeRace(i.name);
                    //avatar.BuildCharacter();
                    break;
                case Item.CLOTHTYPE:
                Debug.Log("--<<2");
                    if(assetBundle != null) UMAAssetIndexer.Instance.AddFromAssetBundle(assetBundle);
                    avatar.ClearSlot(i.category);
                    avatar.SetSlot(i.category, i.name);
                    //foreach (UMAWardrobeRecipe uwr in avatar.WardrobeRecipes.Values)
                     //   if(uwr.name == i.name && uwr.UserField.Length>0){
                      //      Debug.Log(">>>---DEU "+uwr.UserField);
                       // }
                    //avatar.BuildCharacter();
                    break;
                case Item.ANIMTYPE:
                    //if(assetBundle == null) break;
                    print("work ANIM: " + i.name);
                    Animator animator = avatar.gameObject.GetComponent<Animator>();
                    if(assetBundle == null){
                        for(int j=0; j<anims.Count; j++)
                            if(anims[j].name==i.name) {
                                animator.runtimeAnimatorController = anims[j].anim;
                                break;
                            }
                    } else {
                        RuntimeAnimatorController aa = assetBundle.LoadAsset(i.name) as RuntimeAnimatorController;
                        animator.runtimeAnimatorController = aa;
                        anims.Add(new AnimCtrl(i.name, aa));
                    }
                    break;
            }
        }
        public void workAttributesColor(string key, string val, DynamicCharacterAvatar avatar) {
            print(">DNA: "+ key +" = "+ val);
            switch(key){
                case "Height": case "Weight": case "OffsetTop": case "Blendshapetest": case "Blendeshapeother": 
                    _updateAvatarDNA(key, val, avatar); break;
                case "Hair": case "Skin": case "Eyebrows": case "Beard": case "Eyes": case "Lips": case "Blush":
                    _updateAvatarColor(key, val, avatar); break;
                case "ClearSlot":
                    string[] slots = val.Split('|');
                    foreach(string slot in slots) avatar.ClearSlot(slot);
                    break;
                default:
                    if(val.Substring(0, 1)=="#"){ _updateAvatarColor(key, val, avatar); }
                    else if(key.Substring(0, 1)=="_"){ _updateAvatarDNA(key, val, avatar); }
                    break;
            }
        }

        public void _updateAvatarDNA(string name, string val, DynamicCharacterAvatar avatar) {
            Dictionary<string, DnaSetter> AllDNA = avatar.GetDNA();
            Debug.Log("float parse > " + name + ": " + val);
            float floatValue = float.Parse(val, CultureInfo.InvariantCulture.NumberFormat);
            AllDNA[name].Set(floatValue);
        }
        public void _updateAvatarColor(string name, string val, DynamicCharacterAvatar avatar) {
            Color color;
            if (ColorUtility.TryParseHtmlString(val, out color))
                avatar.SetColor(name, color);
        }
        public void _addOverlay(string slotName, string overlayName, DynamicCharacterAvatar avatar, bool update){
            Debug.Log("--<<Overlay");
            foreach (SlotData slot in avatar.umaData.umaRecipe.slotDataList){
                print(slot.slotName);
                if(slot.slotName==slotName)
                    slot.AddOverlay( UMAContext.Instance.InstantiateOverlay(overlayName) );
            }
            if(update) avatar.UpdateSameRace();
        }
        public void _removeOverlay(string slotName, string overlayName, DynamicCharacterAvatar avatar){
            foreach(UserData ud in usersData) if(ud.avatar == avatar)
                for(int i=0; i<ud.overlays.Count; i++)
                    if(ud.overlays[i].slot == slotName && ud.overlays[i].overlay == overlayName){
                        ud.overlays.RemoveAt(i); break; 
                    }
            string[] names = {slotName};
            foreach (SlotData slot in avatar.umaData.umaRecipe.slotDataList)
                if(slot.slotName==slotName)
                    slot.RemoveOverlay(names);
            avatar.UpdateSameRace();
        }

        public string GetSetting(string name){ return settings.ContainsKey(name)? settings[name]: ""; }
        public string GetMainUserId(){ return monouUserId; }
        public int GetMainUserMoney(){ return monouUserMoney; }
        public string GetWardrobeIdByUserId(string usrId){ if(userWardrobeIds.ContainsKey(usrId)) return userWardrobeIds[usrId]; return ""; }
        public bool IsLoaded(){ return isLoaded; }
        public DynamicCharacterAvatar GetAvatarByUser(string userid){
            for(int i=0; i<usersData.Count; i++) if(usersData[i].userId == userid) return usersData[i].avatar;
            return new DynamicCharacterAvatar();
        }
        public GameObject RefreshUser(string usrId){
            int idx = usersData.FindIndex(x=>x.userId == usrId);
            if(idx >= 0){
                Destroy(usersData[idx].avatar.gameObject);
                usersData.RemoveAt(idx);
            }
            return LoadAndCreateUser(usrId);
        }

        public void LoadApparenceFromWardrobeString(string wd, DynamicCharacterAvatar avatar){
            if(wd.Length<4) return;
            StartCoroutine(_loadApparenceFromWardrobeString(wd, avatar));
        }
        IEnumerator _loadApparenceFromWardrobeString(string wd, DynamicCharacterAvatar avatar){
            string[] data = wd.Substring(2, wd.Length-4).Split("},{");
            foreach(var d in data) {
               var wardrobeItem = JsonUtility.FromJson<GQLMainWardrobeItem>("{"+d+"}");
                addItemFromId(wardrobeItem.wardrobeAssetAssetId, avatar);
                yield return new WaitForSeconds(1F);
            }
        }

    }

    // ------- Class GraphQL Initial --------
    [System.Serializable]
    public class GQLInitialData {
        public GQLInitialDataData data;
    }
    [System.Serializable]
    public class GQLInitialDataData {
        public GQLInitialDataDatagetMe getMe;
        public GQLInitialDataDatalistSettings listSettings;
    }
    [System.Serializable]
    public class GQLInitialDataDatagetMe {
        public string id;
        public int money;
        public string userWardrobeId;
    }
    [System.Serializable]
    public class GQLInitialDataDatalistSettings {
        public List<GQLInitialDataDatalistSettingsSettings> items;
    }
    [System.Serializable]
    public class GQLInitialDataDatalistSettingsSettings {
        public string name;
        public string value;
    }
    // ------- Class GraphQL Users --------
    [System.Serializable]
    public class GQLUData {
        public GQLUDataData data;
    }
    [System.Serializable]
    public class GQLUDataData {
        public GQLUDataUser getUser;
    }
    [System.Serializable]
    public class GQLUDataUser {
        public QLUDataListUsrWard wardrobes;
    }
    [System.Serializable]
    public class QLUDataListUsrWard {
        public List<QLUDataListUsrWardItm> items;
    }
    [System.Serializable]
    public class QLUDataListUsrWardItm {
        public string id;
        public QLUDataListUsrWardItmWardAsset wardrobeAssets;
    }
    [System.Serializable]
    public class QLUDataListUsrWardItmWardAsset {
        public  List<QLUDataListUsrWardItmWardAssetItm> items;
    }
    [System.Serializable]
    public class QLUDataListUsrWardItmWardAssetItm {
        public List<string> options;
        public QLUDataListUsrWardItmWardAssetItmAsset asset;
    }
    [System.Serializable]
    public class QLUDataListUsrWardItmWardAssetItmAsset {
        public QLUDataListUsrWardItmWardAssetItmAssetObj assetType;
        public QLUDataListUsrWardItmWardAssetItmAssetObj assetunity;
        public QLUDataListUsrWardItmWardAssetItmAssetObj assetandroid;
        public QLUDataListUsrWardItmWardAssetItmAssetObj assetios;
        public QLUDataListUsrWardItmWardAssetItmAssetObj assetosx;
        public QLUDataListUsrWardItmWardAssetItmAssetObj assetdesk;
        public QLUDataListUsrWardItmWardAssetItmAssetAttr attributes;
        public QLUDataListUsrWardItmWardAssetItmAssetAttrItmObj configs;
        public string name;
        public string key;
        public int zindex;
    }
    [System.Serializable]
    public class QLUDataListUsrWardItmWardAssetItmAssetObj {
        public string id;
        public string name;
        public string value;
        public string url;
    }
    [System.Serializable]
    public class QLUDataListUsrWardItmWardAssetItmAssetAttr {
        public List<QLUDataListUsrWardItmWardAssetItmAssetAttrItm> items;
    }
    [System.Serializable]
    public class QLUDataListUsrWardItmWardAssetItmAssetAttrItm {
        public string id;
        public string name;
        public QLUDataListUsrWardItmWardAssetItmAssetAttrItmObj options;
    }
    [System.Serializable]
    public class QLUDataListUsrWardItmWardAssetItmAssetAttrItmObj {
        public List<QLUDataListUsrWardItmWardAssetItmAssetObj> items;
    }
    [System.Serializable]
    public class QLUDataListUsrWardItmWardAssetItmAssetAttrItmOptsItm {
        public QLUDataListUsrWardItmWardAssetItmAssetAttrItmOptsItm items;
    }



    [System.Serializable]
    public class GQLGetAsset { public GQLGetAssetData data; }
    [System.Serializable]
    public class GQLGetAssetData { public QLUDataListUsrWardItmWardAssetItmAsset getAsset; }




    public class UserAvatar {
        public GameObject gameObject;
        public string userId;
        public UserAvatar(string usrId, GameObject gobj){
            gameObject = gobj;
            userId = usrId;
        }
    }

    public class Item {
        public const string RACETYPE = "race";
        public const string CLOTHTYPE = "cloth";
        public const string ANIMTYPE = "anim";
        public string name;
        public string url;
        public string type;
        public string category;
        public int zindex = 0;
        public bool reppeted = false;
        public List<ItemAttribute> options;
        public Item(string thename, string thetype, string thecategory, string theurl, List<ItemAttribute> theoptions){
            name = thename;
            url = theurl;
            type = thetype;
            category = thecategory;
            options = theoptions;
        }
        public static int typeSort(Item p){
            switch(p.type){
                case RACETYPE: return 1000 + p.zindex;
                case ANIMTYPE: return 2000 + p.zindex;
                case CLOTHTYPE: return 3000 + p.zindex;
                default: return 99000 + p.zindex;
            }
        }
    }

    public class ItemAttribute {
        public string name;
        public string type;
        public string val;
        public ItemAttribute(string thename, string thetype, string thevalue){
            name = thename;
            type = thetype;
            val = thevalue;
        }
    }

    public class Overlay {
        public string slot;
        public string overlay;
        public Color color;
        public Overlay(string theslot, string theoverlay, string thecolor){
            slot = theslot;
            overlay = theoverlay;
            ColorUtility.TryParseHtmlString(thecolor, out color);
        }
        public Overlay(string theslot, string theoverlay){
            slot = theslot;
            overlay = theoverlay;
            ColorUtility.TryParseHtmlString("#ffffff", out color);
        }
    }

    public class UserData {
        public string userId;
        public List<Item> items;
        public List<Overlay> overlays;
        public DynamicCharacterAvatar avatar;
        public UserData(string id, List<Item> theitems){
            userId = id;
            items = theitems;
            overlays = new List<Overlay>();
        } 
    }

    public class Queue {
        public DynamicCharacterAvatar avatar;
        public List<Item> items;
        public bool isCreate = false;
        public string userId;
        public Queue(DynamicCharacterAvatar theavatar, List<Item> theitems, bool theisCreate, string theUserId) {
            avatar = theavatar;
            items = theitems;
            isCreate = theisCreate;
            userId = theUserId;
        }
    }

    public class queryGQL {
        public string query;
        public queryGQL(string q){
            query = q;
        }
    }

    public class AnimCtrl {
        public string name;
        public RuntimeAnimatorController anim;
        public AnimCtrl(string n, RuntimeAnimatorController a){
            name = n;
            anim = a;
        }
    }


    public class PreloadedAvatar {
        public string id;
        public GameObject go;
        public int level;
        public bool isActive;
        public PreloadedAvatar(GameObject _go){ id = ""; go = _go; level = 0; isActive = false; }
    }

    [System.Serializable]
    public class GQLMainWardrobeItem {
        public string[] options;
        public string wardrobeAssetAssetId;
    }
}