using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;

public class AssetManager : ManagerBase<AssetManager>, IManager
{
    public BundleManager m_BundleManager;
	public AssetManager(){
	}

	public override IEnumerator Initialize ()
	{
        m_BundleManager = new BundleManager();
		yield return m_BundleManager.Initialize();
    }

    void Update() {
		m_BundleManager.Update ();
    }

	public LoadedAssetBundle GetLoadedAssetBundle (string assetBundleName, out string error){
		return m_BundleManager.GetLoadedAssetBundle (assetBundleName,out error);
	}

	public void LoadAssetSync<T>(string assetPath, UnityAction<T> callback) where T : UnityEngine.Object{
		Debug.Log ("[LoadAssetSync<" + typeof(T) + ">]:" + assetPath);
		StartCoroutine (LoadAssetSyncBase(assetPath,typeof(T),request=>{
			if(callback != null)
				callback(request.GetAsset<T>());
		}));
	}

	private IEnumerator LoadAssetSyncBase(string assetPath, System.Type type, UnityAction<AssetLoadOperationBase> callback){
		string assetbundlename = GetAssetBundleName (assetPath);
		Debug.Log ("assetbundlename:" + assetbundlename);
		string assetname = Path.GetFileNameWithoutExtension (assetPath);
		AssetLoadOperationBase request = AssetManager.Instance.m_BundleManager.LoadAssetAsync(assetbundlename, assetname, type);
		if (request == null)
			yield break;
		yield return request;
		if (callback != null) {
			callback (request);
		}
	}

	private string GetAssetBundleName(string assetPath){
		string dirName = Path.GetDirectoryName (assetPath);
		string fileName = Path.GetFileNameWithoutExtension (assetPath);
		return (dirName + "/" + fileName).Replace ('/', '_').Replace ('\\', '_').ToLower();
	}
}
