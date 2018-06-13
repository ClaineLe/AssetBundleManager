using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class BundleManager
{
	public enum LogMode
	{
		All,
		JustErrors
	}

	public enum LogType
	{
		Info,
		Warning,
		Error
	}

	string m_BaseDownloadingURL = "";
	AssetBundleManifest m_AssetBundleManifest = null;

	Dictionary<string, LoadedAssetBundle> m_LoadedAssetBundles = new Dictionary<string, LoadedAssetBundle> ();
	Dictionary<string, string> m_DownloadingErrors = new Dictionary<string, string> ();
	List<string> m_DownloadingBundles = new List<string> ();
	List<LoadOperation> m_InProgressOperations = new List<LoadOperation> ();
	Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]> ();


	private void Log (LogType logType, string text)
	{
		if (logType == LogType.Error)
			Debug.LogError ("[BundleManager] " + text);
		else if (logType == LogType.Warning)
			Debug.LogWarning ("[BundleManager] " + text);
		else
			Debug.Log ("[BundleManager] " + text);
	}

	public LoadedAssetBundle GetLoadedAssetBundle (string assetBundleName, out string error)
	{
		if (m_DownloadingErrors.TryGetValue (assetBundleName, out error))
			return null;

		LoadedAssetBundle bundle = null;
		m_LoadedAssetBundles.TryGetValue (assetBundleName, out bundle);
		if (bundle == null)
			return null;

		string[] dependencies = null;
		if (!m_Dependencies.TryGetValue (assetBundleName, out dependencies))
			return bundle;

		for (int i = 0; i < dependencies.Length; i++) {
			string dependency = dependencies [i];
			if (m_DownloadingErrors.TryGetValue (dependency, out error))
				return null;

			LoadedAssetBundle dependentBundle;
			m_LoadedAssetBundles.TryGetValue (dependency, out dependentBundle);
			if (dependentBundle == null)
				return null;
		}
		return bundle;
	}

	public IEnumerator Initialize ()
	{
#if !UNITY_EDITOR
            if (!BundleUtility.USER_ASSETBUNDLE_IN_EDITOR) {
                yield break;
            }
#endif
		m_BaseDownloadingURL = BundleUtility.StreamAssetPath + BundleUtility.GetPlatformName () + "/";

		string ManifestPath = m_BaseDownloadingURL + BundleUtility.GetPlatformName ();
		Debug.Log ("ManifestPath:" + ManifestPath);
		AssetBundleCreateRequest createRequest = AssetBundle.LoadFromFileAsync (ManifestPath);
		yield return createRequest;
		m_AssetBundleManifest = createRequest.assetBundle.LoadAsset<AssetBundleManifest> ("AssetBundleManifest");
		Debug.Log ("Initialize Finish");
	}


	protected void LoadAssetBundle (string assetBundleName)
	{
		Log (LogType.Info, "Loading Asset Bundle:" + assetBundleName);

#if UNITY_EDITOR
		if (!BundleUtility.USER_ASSETBUNDLE_IN_EDITOR) {
			return;
		}
#endif
		if (m_AssetBundleManifest == null) {
			Log (LogType.Error, "Please initialize AssetBundleManifest by calling BundleManager.Initialize()");
			return;
		}

		bool isAlreadyProcessed = LoadAssetBundleInternal (assetBundleName);
		if (!isAlreadyProcessed)
			LoadDependencies (assetBundleName);
	}

	protected bool LoadAssetBundleInternal (string assetBundleName)
	{
		LoadedAssetBundle bundle = null;
		m_LoadedAssetBundles.TryGetValue (assetBundleName, out bundle);
		if (bundle != null) {
			bundle.m_ReferencedCount++;
			return true;
		}

		if (m_DownloadingBundles.Contains (assetBundleName))
			return true;

		string url = m_BaseDownloadingURL + assetBundleName;

		AssetBundleCreateRequest createRequest = AssetBundle.LoadFromFileAsync (url);
		m_InProgressOperations.Add (new BundleLoadOperation (assetBundleName, createRequest));
		m_DownloadingBundles.Add (assetBundleName);
		return false;
	}

	protected void LoadDependencies (string assetBundleName)
	{
		if (m_AssetBundleManifest == null) {
			Log (LogType.Error, "Please initialize AssetBundleManifest by calling BundleManager.Initialize()");
			return;
		}

		string[] dependencies = m_AssetBundleManifest.GetAllDependencies (assetBundleName);
		if (dependencies.Length == 0)
			return;

		m_Dependencies.Add (assetBundleName, dependencies);
		for (int i = 0; i < dependencies.Length; i++)
			LoadAssetBundleInternal (dependencies [i]);
	}

	public void UnloadAssetBundle (string assetBundleName)
	{
#if UNITY_EDITOR
		if (!BundleUtility.USER_ASSETBUNDLE_IN_EDITOR) {
			return;
		}
#endif
		UnloadAssetBundleInternal (assetBundleName);
		UnloadDependencies (assetBundleName);
	}

	protected void UnloadDependencies (string assetBundleName)
	{
		string[] dependencies = null;
		if (!m_Dependencies.TryGetValue (assetBundleName, out dependencies))
			return;

		for (int i = 0; i < dependencies.Length; i++) {
			UnloadAssetBundleInternal (dependencies [i]);
		}

		m_Dependencies.Remove (assetBundleName);
	}

	protected void UnloadAssetBundleInternal (string assetBundleName)
	{
		string error;
		LoadedAssetBundle bundle = GetLoadedAssetBundle (assetBundleName, out error);
		if (bundle == null)
			return;

		if (--bundle.m_ReferencedCount == 0) {
			bundle.OnUnload ();
			m_LoadedAssetBundles.Remove (assetBundleName);

			Log (LogType.Info, assetBundleName + " has been unloaded successfully");
		}
	}

	public void Update ()
	{
		for (int i = 0; i < m_InProgressOperations.Count;) {
			var operation = m_InProgressOperations [i];
			if (operation.Update ()) {
				i++;
			} else {
				m_InProgressOperations.RemoveAt (i);
				ProcessFinishedOperation (operation);
			}
		}
	}

	void ProcessFinishedOperation (LoadOperation operation)
	{
		BundleLoadOperationBase download = operation as BundleLoadOperationBase;
		if (download == null)
			return;

		if (string.IsNullOrEmpty (download.error))
			m_LoadedAssetBundles.Add (download.assetBundleName, download.assetBundle);
		else {
			string msg = string.Format ("Failed downloading bundle {0} from: {1}",
				             download.assetBundleName, download.error);
			m_DownloadingErrors.Add (download.assetBundleName, msg);
		}

		m_DownloadingBundles.Remove (download.assetBundleName);
	}

	public AssetLoadOperationBase LoadAssetAsync (string assetBundleName, string assetName, System.Type type)
	{
		Log (LogType.Info, "Loading " + assetName + " from " + assetBundleName + " bundle");

		AssetLoadOperationBase operation = null;
#if UNITY_EDITOR
		if (!BundleUtility.USER_ASSETBUNDLE_IN_EDITOR) {
			operation = new AssetLoadOperationSimulation (assetBundleName, assetName, type);
		} else
#endif
		{
			LoadAssetBundle (assetBundleName);
			operation = new AssetLoadOperation (assetBundleName, assetName, type);
		}
		m_InProgressOperations.Add (operation);
		return operation;
	}

	public LoadOperation LoadLevelAsync (string assetBundleName, string levelName, bool isAdditive)
	{
		Log (LogType.Info, "Loading " + levelName + " from " + assetBundleName + " bundle");

		LoadOperation operation = null;
#if UNITY_EDITOR
		if (!BundleUtility.USER_ASSETBUNDLE_IN_EDITOR) {
			operation = new LevelLoadOperationSimulation (assetBundleName, levelName, isAdditive);
		} else
#endif
            {
			LoadAssetBundle (assetBundleName);
			operation = new LevelLoadOperation (assetBundleName, levelName, isAdditive);

			m_InProgressOperations.Add (operation);
		}
            
		return operation;
	}
}
