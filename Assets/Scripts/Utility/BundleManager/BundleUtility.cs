using UnityEngine;

public class BundleUtility
{
	/** 编辑器模式下，是否使用AssetBundle */
	public const bool USER_ASSETBUNDLE_IN_EDITOR = false;

	private const string AssetBundlesOutputPath = "AssetBundles";

	public static string StreamAssetPathInAsset {
		get {
			return Application.streamingAssetsPath + "/" + AssetBundlesOutputPath + "/";
		}
	}

	public static string StreamAssetPath {
		get {
#if UNITY_EDITOR
			return Application.dataPath + "/../" + AssetBundlesOutputPath + "/";
#else
                return StreamAssetPathInAsset;
#endif
		}
	}

	public static string PersistentDataPath {
		get {
#if UNITY_EDITOR
			return Application.dataPath + "/../PersistentData/" + AssetBundlesOutputPath + "/";
#else
			return Application.persistentDataPath + "/" + AssetBundlesOutputPath + "/";
#endif
		}
	}

	public static string GetPlatformName ()
	{
#if UNITY_EDITOR
		return GetPlatformForAssetBundles (UnityEditor.EditorUserBuildSettings.activeBuildTarget);
#else
        return GetPlatformForAssetBundles(Application.platform);
#endif
	}

	#if UNITY_EDITOR
	public static string GetPlatformForAssetBundles (UnityEditor.BuildTarget target)
	{
		switch (target) {
		case UnityEditor.BuildTarget.Android:
			return "Android";
		case UnityEditor.BuildTarget.iOS:
			return "iOS";
		case UnityEditor.BuildTarget.WebGL:
			return "WebGL";
		case UnityEditor.BuildTarget.StandaloneWindows:
		case UnityEditor.BuildTarget.StandaloneWindows64:
			return "Windows";
		case UnityEditor.BuildTarget.StandaloneOSX:
			return "OSX";
		default:
			return null;
		}
	}
	#endif

	public static string GetPlatformForAssetBundles (RuntimePlatform platform)
	{
		switch (platform) {
		case RuntimePlatform.Android:
			return "Android";
		case RuntimePlatform.IPhonePlayer:
			return "iOS";
		case RuntimePlatform.WebGLPlayer:
			return "WebGL";
		case RuntimePlatform.WindowsPlayer:
			return "Windows";
		case RuntimePlatform.OSXPlayer:
			return "OSX";
		default:
			return null;
		}
	}
}
