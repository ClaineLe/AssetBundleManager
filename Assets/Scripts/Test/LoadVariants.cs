using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class LoadVariants : MonoBehaviour
{
    const string variantSceneAssetBundle = "variants/variant-scene";
    const string variantSceneName = "VariantScene";

    // The following are used only if app slicing is not enabled.
    private string[] activeVariants;
    private bool bundlesLoaded;             // used to remove the loading buttons

    void Awake()
    {
        activeVariants = new string[1];
        bundlesLoaded = false;
    }

    void OnGUI()
    {
        if (!bundlesLoaded)
        {
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginVertical();
            if (GUILayout.Button("Load SD"))
            {
                activeVariants[0] = "sd";
                bundlesLoaded = true;
                StartCoroutine(BeginExample());
                Debug.Log("Loading SD");
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Load HD"))
            {
                activeVariants[0] = "hd";
                bundlesLoaded = true;
                StartCoroutine(BeginExample());
                Debug.Log("Loading HD");
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }

    // Use this for initialization
    IEnumerator BeginExample()
    {
        yield return Initialize();
        yield return InitializeLevelAsync(variantSceneName, true);
    }

    // Initialize the downloading url and AssetBundleManifest object.
    protected IEnumerator Initialize()
    {
        // Don't destroy the game object as we base on it to run the loading script.
        DontDestroyOnLoad(gameObject);

        // Initialize AssetBundleManifest which loads the AssetBundleManifest object.
		yield return AssetManager.Instance.Initialize ();
    }

    protected IEnumerator InitializeLevelAsync(string levelName, bool isAdditive)
    {
        // This is simply to get the elapsed time for this phase of AssetLoading.
        float startTime = Time.realtimeSinceStartup;

        // Load level from assetBundle.
		LoadOperation request = AssetManager.Instance.m_BundleManager.LoadLevelAsync(variantSceneAssetBundle, levelName, isAdditive);
        if (request == null)
            yield break;

        yield return StartCoroutine(request);

        // Calculate and display the elapsed time.
        float elapsedTime = Time.realtimeSinceStartup - startTime;
        Debug.Log("Finished loading scene " + levelName + " in " + elapsedTime + " seconds");
    }
}
