using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AssetBundles
{
    public class AssetBundlesMenuItems
    {
		[MenuItem("Assets/AssetBundles/Build AssetBundleName/All")]
		static public void BuildAssetBundleName_All()
		{
			BuildScript.BuildAssetBundleName ();
		}

        [MenuItem("Assets/AssetBundles/Build AssetBundles/Android")]
        static public void BuildAssetBundles_Android()
        {
            BuildScript.BuildAssetBundles(BuildTarget.Android);
        }
        [MenuItem("Assets/AssetBundles/Build AssetBundles/Windown")]
        static public void BuildAssetBundles_Windown()
        {
            BuildScript.BuildAssetBundles(BuildTarget.StandaloneWindows64);
        }
        [MenuItem("Assets/AssetBundles/Build AssetBundles/iOS")]
        static public void BuildAssetBundles_iOS()
        {
            BuildScript.BuildAssetBundles(BuildTarget.iOS);
        }
        [MenuItem("Assets/AssetBundles/Build AssetBundles/OSX")]
        static public void BuildAssetBundles_OSX()
        {
            BuildScript.BuildAssetBundles(BuildTarget.StandaloneOSX);
        }


        [MenuItem("Assets/AssetBundles/Build Player/Android")]
        static public void BuildPlayer_Android()
        {
            BuildScript.BuildPlayer(BuildTarget.Android);
        }
        [MenuItem("Assets/AssetBundles/Build Player/Windown")]
        static public void BuildPlayer_Windown()
        {
            BuildScript.BuildPlayer(BuildTarget.StandaloneWindows64);
        }
        [MenuItem("Assets/AssetBundles/Build Player/iOS")]
        static public void BuildPlayer_iOS()
        {
            BuildScript.BuildPlayer(BuildTarget.iOS);
        }
        [MenuItem("Assets/AssetBundles/Build Player/OSX")]
        static public void BuildPlayer_OSX()
        {
            BuildScript.BuildPlayer(BuildTarget.StandaloneOSX);
        }
    }
}