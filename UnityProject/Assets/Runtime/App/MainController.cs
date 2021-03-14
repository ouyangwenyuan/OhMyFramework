using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using UnityEngine;

using QFramework;
using  DragonPlus.Assets;
using Newtonsoft.Json;

public class MainController : MonoBehaviour {
    AssetsMgr assetsMgr;
    private void Awake () {
//        Gloabal.ConfigLoader.Instance.Load ();
//        Gloabal.ConfigLoader1.Instance.Load ();
//        Gloabal.ConfigLoader2.Instance.Load ();
//        ResKit.Init();
        assetsMgr = new AssetsMgr();
        if (!File.Exists(Path.Combine(Application.persistentDataPath,AssetBundlePathConst.RuntimeAssetsRoot, "Version.txt")))
        {
            Debug.LogError("CopyToPersistentDataPath");
            CopyToPersistentDataPath();
        }
    }
    
    static void CopyToPersistentDataPath()
    {

        var sourcesPath = Path.Combine(Application.streamingAssetsPath, AssetBundlePathConst.RuntimeAssetsRoot);
        var targetPath = Path.Combine(Application.persistentDataPath, AssetBundlePathConst.RuntimeAssetsRoot);
        AssetBundlePathTools.CreateFolder(targetPath);
        AssetBundlePathTools.CopyDirectory(sourcesPath,targetPath);
//        var filename = "Version.txt";
//        AssetBundlePathTools.CopyFile(Path.Combine(sourcesPath, filename),Path.Combine(targetPath, filename));
    }
    // Start is called before the first frame update
    void Start () {
//        var obj = assetsMgr.LoadAsset<GameObject>(AssetsAssetBundlePrefabsGame_2d.BundleName,AssetsAssetBundlePrefabsGame_2d.PREFAB_WORLD);
        var obj = assetsMgr.LoadAsset<GameObject>(AssetsAssetBundlePrefabsGame_2d.BundleName,"Egg");
        if (obj == null)
        {
            Debug.LogError($"prefab is not exist ");
            return;
        }
        AssetsMgr.CreateGameObject(obj,null);
    }
    void LoadRes(){
        // var needPatchAB = VersionManager.Instance.IsNeedPatchVersionAfterOverLayInstall();
        // if(VersionManager.Instance.IsFirstInstall()){
        //     VersionManager.Instance.StartProtect();
        //     Debug.Log($"needPatchAB={needPatchAB},isfirstInstall={VersionManager.Instance.IsFirstInstall()}");
        //     StartCoroutine(VersionManager.Instance.PatchVersionAfterOverLayInstall());
        // }
        var canvas = transform.Find ("Canvas");
        var laym = new LayerAndTagManager();
        var layers = laym.Layers();
        var sortlayers = laym.SortingLayers();
        Debug.Log("layers = "+layers);
        Debug.Log("sortlayers = "+sortlayers);
        // var obj =  ResourcesManager.Instance.LoadResource<GameObject>(Assetbundle_prefabs_game.BundleName +"/" + Assetbundle_prefabs_game.BASEPANEL);
        // initItem(obj,transform);
        // obj =  ResourcesManager.Instance.LoadResource<GameObject>(Assetbundle_models.BundleName+"/"+Assetbundle_models.GAMEOBJECT);
        // initItem(obj,null);
        
//        ResLoader resLoader = ResLoader.Allocate();
//        var obj = resLoader.LoadSync<GameObject>(Assetbundle_prefabs_game.BASEPANEL);
//        // var ab = AssetBundle.LoadFromFile (Path.Combine (Application.streamingAssetsPath, Assetbundle_prefabs_game.BundleName));
//        // GameObject obj = ab.LoadAsset<GameObject> (Assetbundle_prefabs_game.BASEPANEL);
//        initItem (obj, transform);
//        obj = resLoader.LoadSync<GameObject>(Assetbundle_models.GAMEOBJECT);
//        // ab = AssetBundle.LoadFromFile (Path.Combine (Application.streamingAssetsPath, Assetbundle_models.BundleName));
//        // obj = ab.LoadAsset<GameObject> (Assetbundle_models.GAMEOBJECT);
//        initItem (obj, null);

    }

    private GameObject initItem (GameObject itemPrefab, Transform parent) {
        GameObject item = Instantiate (itemPrefab, parent, true);
//        item.transform.Reset ();
        return item;
    }

    // Update is called once per frame
    // void Update () {

    // }
    public class LayerAndTagManager {
        public string SortingLayers(){
            StringBuilder sb = new StringBuilder();
            var layers = SortingLayer.layers;
            foreach(var layer in layers){
                sb.Append(layer.name+":"+layer.id);
                sb.AppendLine();
            }
            return sb.ToString();
        }
        public string Layers(){
             StringBuilder sb = new StringBuilder();
            var layers = LayerMask.NameToLayer("Text");
            var name = LayerMask.LayerToName(25);
            return layers +":" + name;
            // foreach(var layer in layers){
            //     sb.Append(layer.name+":"+layer.id);
            //     sb.AppendLine();
            // }
            // return sb.ToString();
        }
        public string Tags(){
            StringBuilder sb = new StringBuilder();
            
            return sb.ToString();
        }
    }
}