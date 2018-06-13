using UnityEngine;
using System.Collections;

public class Loader{
	static public void Create<T>(string assetPath, UnityEngine.Events.UnityAction<T> callback) where T : Object{
		AssetManager.Instance.LoadAssetSync<T> (assetPath, callback);
	}
}

public class LoadAssets : MonoBehaviour
{
	private string assetPath = "Assets/MyCube.prefab";
    IEnumerator Start()
    {
		yield return AssetManager.Instance.Initialize();
		Loader.Create<GameObject> (assetPath,asset=>GameObject.Instantiate(asset));
    }
}
