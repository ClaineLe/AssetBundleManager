using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public abstract class LoadOperation : IEnumerator
{
	public object Current {
		get {
			return null;
		}
	}

	public bool MoveNext ()
	{
		return !IsDone ();
	}

	public void Reset ()
	{
	}

	abstract public bool Update ();

	abstract public bool IsDone ();
}

public abstract class BundleLoadOperationBase : LoadOperation
{
	bool done;

	public string assetBundleName { get; private set; }

	public LoadedAssetBundle assetBundle { get; protected set; }

	public string error { get; protected set; }

	protected abstract bool downloadIsDone { get; }

	protected abstract void FinishDownload ();

	public override bool Update ()
	{
		if (!done && downloadIsDone) {
			FinishDownload ();
			done = true;
		}

		return !done;
	}

	public override bool IsDone ()
	{
		return done;
	}

	public BundleLoadOperationBase (string assetBundleName)
	{
		this.assetBundleName = assetBundleName;
	}
}

public abstract class AssetLoadOperationBase : LoadOperation
{
	public abstract T GetAsset<T> () where T : UnityEngine.Object;
}

public class LoadedAssetBundle
{
	public AssetBundle m_AssetBundle;
	public int m_ReferencedCount;

	internal event System.Action unload;

	internal void OnUnload ()
	{
		m_AssetBundle.Unload (false);
		if (unload != null)
			unload ();
	}

	public LoadedAssetBundle (AssetBundle assetBundle)
	{
		m_AssetBundle = assetBundle;
		m_ReferencedCount = 1;
	}
}

public class BundleLoadOperation : BundleLoadOperationBase
{
	AssetBundleCreateRequest m_CreateRequest;

	public BundleLoadOperation (string assetBundleName, AssetBundleCreateRequest createRequest)
		: base (assetBundleName)
	{
		if (createRequest == null)
			throw new System.ArgumentNullException ("createRequest");
		this.m_CreateRequest = createRequest;
	}

	protected override bool downloadIsDone { get { return (m_CreateRequest == null) || m_CreateRequest.isDone; } }

	protected override void FinishDownload ()
	{
		if (!string.IsNullOrEmpty (error))
			return;

		AssetBundle bundle = m_CreateRequest.assetBundle;
		if (bundle == null)
			error = string.Format ("{0} is not a valid asset bundle.", assetBundleName);
		else
			assetBundle = new LoadedAssetBundle (m_CreateRequest.assetBundle);

		m_CreateRequest = null;
	}

}

public class LevelLoadOperation : BundleLoadOperationBase
{
	protected string m_AssetBundleName;
	protected string m_LevelName;
	protected bool m_IsAdditive;
	protected string m_DownloadingError;
	protected AsyncOperation m_Request;

	protected override bool downloadIsDone {
		get {
			throw new System.NotImplementedException ();
		}
	}

	public LevelLoadOperation (string assetbundleName, string levelName, bool isAdditive)
		: base (assetbundleName)
	{
		m_AssetBundleName = assetbundleName;
		m_LevelName = levelName;
		m_IsAdditive = isAdditive;
	}

	public override bool Update ()
	{
		if (m_Request != null)
			return false;

		LoadedAssetBundle bundle = AssetManager.Instance.GetLoadedAssetBundle (m_AssetBundleName, out m_DownloadingError);
		if (bundle != null) {
			m_Request = SceneManager.LoadSceneAsync (m_LevelName, m_IsAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
			return false;
		} else
			return true;
	}

	public override bool IsDone ()
	{
		if (m_Request == null && m_DownloadingError != null) {
			Debug.LogError (m_DownloadingError);
			return true;
		}
		return m_Request != null && m_Request.isDone;
	}

	protected override void FinishDownload ()
	{
		throw new System.NotImplementedException ();
	}
}

public class AssetLoadOperation : AssetLoadOperationBase
{
	protected string m_AssetBundleName;
	protected string m_AssetName;
	protected string m_DownloadingError;
	protected System.Type m_Type;
	protected AssetBundleRequest m_Request = null;

	public AssetLoadOperation (string bundleName, string assetName, System.Type type)
	{
		m_AssetBundleName = bundleName;
		m_AssetName = assetName;
		m_Type = type;
	}

	public override T GetAsset<T> ()
	{
		if (m_Request != null && m_Request.isDone)
			return m_Request.asset as T;
		else
			return null;
	}

	public override bool Update ()
	{
		if (m_Request != null)
			return false;

		LoadedAssetBundle bundle = AssetManager.Instance.GetLoadedAssetBundle (m_AssetBundleName, out m_DownloadingError);
		if (bundle != null) {
			m_Request = bundle.m_AssetBundle.LoadAssetAsync (m_AssetName, m_Type);
			return false;
		} else {
			return true;
		}
	}

	public override bool IsDone ()
	{
		if (m_Request == null && m_DownloadingError != null) {
			Debug.LogError (m_DownloadingError);
			return true;
		}

		return m_Request != null && m_Request.isDone;
	}
}

#if UNITY_EDITOR
public class AssetLoadOperationSimulation : AssetLoadOperationBase
{
	protected string m_AssetBundleName;
	protected string m_AssetName;
	protected string m_DownloadingError;
	protected System.Type m_Type;
	protected Object m_SimulatedObject = null;

	public AssetLoadOperationSimulation (string bundleName, string assetName, System.Type type)
	{
		m_AssetBundleName = bundleName;
		m_AssetName = assetName;
		m_Type = type;
	}

	public override T GetAsset<T> ()
	{
		if (m_SimulatedObject != null)
			return m_SimulatedObject as T;
		else
			return null;
	}

	public override bool Update ()
	{
		if (m_SimulatedObject == null) {
			string[] assetPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName (m_AssetBundleName, m_AssetName);
			m_SimulatedObject = UnityEditor.AssetDatabase.LoadMainAssetAtPath (assetPaths [0]);
			Debug.Log ("m_SimulatedObject:" + m_SimulatedObject);
		}

		if (m_SimulatedObject == null) {
			Debug.LogError ("There is no asset with name \"" + m_AssetName + "\" in " + m_AssetBundleName);
		}
		return false;
	}

	public override bool IsDone ()
	{
		if (m_SimulatedObject == null && m_DownloadingError != null) {
			Debug.LogError (m_DownloadingError);
			return true;
		}
		return m_SimulatedObject != null;
	}
}

public class LevelLoadOperationSimulation : LoadOperation
{
	AsyncOperation m_Operation = null;

	public LevelLoadOperationSimulation (string assetBundleName, string levelName, bool isAdditive)
	{
		string[] levelPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName (assetBundleName, levelName);
		if (levelPaths.Length == 0) {
			Debug.LogError ("There is no scene with name \"" + levelName + "\" in " + assetBundleName);
			return;
		}

		if (isAdditive)
			m_Operation = UnityEditor.EditorApplication.LoadLevelAdditiveAsyncInPlayMode (levelPaths [0]);
		else
			m_Operation = UnityEditor.EditorApplication.LoadLevelAsyncInPlayMode (levelPaths [0]);
	}

	public override bool Update ()
	{
		return false;
	}

	public override bool IsDone ()
	{
		return m_Operation == null || m_Operation.isDone;
	}
}
#endif
