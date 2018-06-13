using UnityEngine;
using System.Collections;


public class LoadScenes : MonoBehaviour
{
    public string sceneAssetBundle;
    public string sceneName;

    IEnumerator Start()
    {
		yield return AssetManager.Instance.Initialize ();
        yield return StartCoroutine(InitializeLevelAsync(sceneName, true));
    }

    protected IEnumerator InitializeLevelAsync(string levelName, bool isAdditive)
    {
        // This is simply to get the elapsed time for this phase of AssetLoading.
        float startTime = Time.realtimeSinceStartup;

        // Load level from assetBundle.
		LoadOperation request = AssetManager.Instance.m_BundleManager.LoadLevelAsync(sceneAssetBundle, levelName, isAdditive);
        if (request == null)
            yield break;
        yield return StartCoroutine(request);

        // Calculate and display the elapsed time.
        float elapsedTime = Time.realtimeSinceStartup - startTime;
        Debug.Log("Finished loading scene " + levelName + " in " + elapsedTime + " seconds");
    }
}
