using UnityEngine;
using UnityEditor;
using System.IO;

namespace FrontierProject.Editor.Pipeline
{
    /// <summary>
    /// Build preparation processor for configuring and validating build settings.
    /// </summary>
    public class BuildPreparation
    {
        [MenuItem("Tools/Frontier/Prepare Build")]
        public static void PrepareBuildMenu()
        {
            PrepareForBuild();
        }

        public static void PrepareForBuild()
        {
            Debug.Log("[BuildPreparation] Starting build preparation...");

            // Validate scenes in build
            ValidateBuildScenes();

            // Check for missing references
            ValidateAssetReferences();

            // Optimize assets
            OptimizeAssetsForBuild();

            // Configure build settings
            ConfigureBuildSettings();

            Debug.Log("[BuildPreparation] Build preparation complete");
        }

        private static void ValidateBuildScenes()
        {
            string[] sceneGuids = AssetDatabase.FindAssets("t:SceneAsset");
            int validSceneCount = 0;

            foreach (string guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("/Scenes/") && !path.Contains("/Backup/"))
                {
                    validSceneCount++;
                    Debug.Log($"[BuildPreparation] Found scene: {path}");
                }
            }

            Debug.Log($"[BuildPreparation] Validated {validSceneCount} scenes");
        }

        private static void ValidateAssetReferences()
        {
            string[] assetGuids = AssetDatabase.FindAssets("");
            int missingRefs = 0;

            foreach (string guid in assetGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                
                // Skip non-script assets
                if (!path.EndsWith(".cs") && !path.EndsWith(".prefab")) continue;

                Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (asset == null)
                {
                    missingRefs++;
                    Debug.LogWarning($"[BuildPreparation] Missing reference: {path}");
                }
            }

            if (missingRefs > 0)
            {
                Debug.LogWarning($"[BuildPreparation] Found {missingRefs} missing references");
            }
        }

        private static void OptimizeAssetsForBuild()
        {
            // Strip unused shaders
            EditorUserBuildSettings.stripDebugSymbols = true;

            // Enable asset bundling optimization
            BuildCompression.DefaultCompression = BuildCompression.LZ4;

            Debug.Log("[BuildPreparation] Assets optimized for build");
        }

        private static void ConfigureBuildSettings()
        {
            // Set build platform
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);

            // Configure player settings
            PlayerSettings.companyName = "Frontier Studios";
            PlayerSettings.productName = "Frontier Project";
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;

            // Graphics quality
            QualitySettings.SetQualityLevel(3, true);

            Debug.Log("[BuildPreparation] Build settings configured");
        }

        public static void BuildPlayer(string outputPath, BuildTarget target)
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            
            // Get all enabled scenes
            string[] scenes = GetEnabledScenes();
            buildPlayerOptions.scenes = scenes;
            buildPlayerOptions.locationPathName = outputPath;
            buildPlayerOptions.target = target;
            buildPlayerOptions.options = BuildOptions.None;

            BuildPipeline.BuildPlayer(buildPlayerOptions);
            
            Debug.Log($"[BuildPreparation] Build completed: {outputPath}");
        }

        private static string[] GetEnabledScenes()
        {
            System.Collections.Generic.List<string> enabledScenes = new System.Collections.Generic.List<string>();
            
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                if (EditorBuildSettings.scenes[i].enabled)
                {
                    enabledScenes.Add(EditorBuildSettings.scenes[i].path);
                }
            }
            
            return enabledScenes.ToArray();
        }

        public static void CreateAssetBundle(string outputPath, string[] assetPaths, BuildTarget target)
        {
            AssetBundleBuild buildManifest = new AssetBundleBuild
            {
                assetNames = assetPaths,
                assetBundleName = Path.GetFileName(outputPath)
            };

            BuildPipeline.BuildAssetBundles(
                new AssetBundleBuild[] { buildManifest },
                BuildAssetBundleOptions.None,
                target,
                Path.GetDirectoryName(outputPath)
            );

            Debug.Log($"[BuildPreparation] Asset bundle created: {outputPath}");
        }

        public static void ValidateBuildSize()
        {
            long totalSize = 0;
            string[] assetGuids = AssetDatabase.FindAssets("");

            foreach (string guid in assetGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), path);

                if (File.Exists(fullPath))
                {
                    FileInfo info = new FileInfo(fullPath);
                    totalSize += info.Length;
                }
            }

            float sizeMB = totalSize / (1024f * 1024f);
            Debug.Log($"[BuildPreparation] Total asset size: {sizeMB:F2} MB");

            if (sizeMB > 2048)
            {
                Debug.LogWarning("[BuildPreparation] Build size exceeds 2GB threshold");
            }
        }
    }
}
