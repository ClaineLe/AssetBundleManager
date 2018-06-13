using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace AssetBundles
{
	public struct sBundleNameRule{
		public string dirPath;
		public bool withRootDir;
		public SearchOption searchOption;
	}
	
    public class BuildScript
    {
        private const string AppName = "BundleManager";
		private const string ExportResDirPath = "ResourceExport/";
        private static string PlayerOutPutPath {
        get {
                return Application.dataPath + "/../PlayerOutPut/";
            }
        }

        private static void CopyAssetBundlesTo(string outputPath)
        {
            FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath);
            Directory.CreateDirectory(outputPath);

			string outputFolder = BundleUtility.GetPlatformName();

			var source = Path.Combine(BundleUtility.StreamAssetPath, outputFolder);
            if (!Directory.Exists(source))
                Debug.Log("No assetBundle output folder, try to build the assetBundles first.");

            var destination = Path.Combine(outputPath, outputFolder);
            if (Directory.Exists(destination))
                FileUtil.DeleteFileOrDirectory(destination);

            FileUtil.CopyFileOrDirectory(source, destination);
        }

        public static void BuildAssetBundles(BuildTarget target, AssetBundleBuild[] builds = null)
        {
            if (target != EditorUserBuildSettings.activeBuildTarget) {
                Debug.LogError("[BuildAssetBundles]Fail. ActiveBuildTarget:" + EditorUserBuildSettings.activeBuildTarget + ", destBuildTarget:" + target);
                return;
            }

			string outputPath = Path.Combine(BundleUtility.StreamAssetPath, BundleUtility.GetPlatformName());
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            BuildAssetBundleOptions options = BuildAssetBundleOptions.ChunkBasedCompression;
            if (builds == null || builds.Length == 0)
            {
                BuildPipeline.BuildAssetBundles(outputPath, options, EditorUserBuildSettings.activeBuildTarget);
            }
            else
            {
                BuildPipeline.BuildAssetBundles(outputPath, builds, options, EditorUserBuildSettings.activeBuildTarget);
            }
        }

        public static void BuildPlayer(BuildTarget target)
        {
			var outputPath = PlayerOutPutPath + BundleUtility.GetPlatformForAssetBundles(target);
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            List<string> levelList = new List<string>();
            for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
            {
                if (EditorBuildSettings.scenes[i].enabled)
                    levelList.Add(EditorBuildSettings.scenes[i].path);
            }
            string[] levels = levelList.ToArray();


            if (levels.Length == 0)
            {
                Debug.Log("Nothing to build.");
                return;
            }

            string targetName = AppName;
            switch (target)
            {
                case BuildTarget.Android:
                    {
                        targetName += ".apk";
                        break;
                    }
                case BuildTarget.StandaloneWindows64:
                    {
                        targetName += ".exe";
                        break;
                    }
                case BuildTarget.StandaloneOSX:
                    {
                        targetName += ".app";
                        break;
                    }
                case BuildTarget.iOS:
                    {
                        targetName = "";
                        break;
                    }
            }


            if (targetName == null)
                return;

            // Build and copy AssetBundles.
            BuildAssetBundles(target);
			CopyAssetBundlesTo(BundleUtility.StreamAssetPathInAsset);
            AssetDatabase.Refresh();

            BuildOptions option = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
            BuildPipeline.BuildPlayer(levels, Path.Combine(outputPath, targetName), target, option);
        }

		public static void BuildAssetBundleName(){

			foreach (string assetbundleName in AssetDatabase.GetAllAssetBundleNames())
				AssetDatabase.RemoveAssetBundleName (assetbundleName,true);

			string path = Application.dataPath + "/" + ExportResDirPath;
			string[] resFile = Directory.GetFiles (path, "*.*", SearchOption.AllDirectories);
			for (int i = 0; i < resFile.Length; i++) {
				if (Path.GetExtension (resFile [i]) == ".meta")
					continue;
				string dirName = Path.GetDirectoryName (resFile [i]);
				string fileName = Path.GetFileNameWithoutExtension (resFile [i]);
				AssetImporter assetImporter = AssetImporter.GetAtPath ("Assets/" + ExportResDirPath + resFile[i].Replace (path, string.Empty));
				string assetPath = Path.Combine (dirName, fileName).Replace (path, string.Empty);
				assetPath = assetPath.Replace('/','_').Replace('\\','_');
				if (assetImporter.assetBundleName != assetPath) {
					assetImporter.assetBundleName = assetPath;
				}
			}
		}
    }
}
