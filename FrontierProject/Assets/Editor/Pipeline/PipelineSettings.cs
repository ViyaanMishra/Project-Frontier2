using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace FrontierProject.Editor.Pipeline
{
    /// <summary>
    /// Pipeline configuration and settings manager.
    /// </summary>
    public class PipelineSettings : ScriptableObject
    {
        private const string SettingsPath = "Assets/Editor/Pipeline/PipelineSettings.asset";

        [Header("General Settings")]
        [Tooltip("Enable automatic asset processing on import")]
        public bool autoProcessOnImport = true;
        
        [Tooltip("Maximum number of concurrent processing tasks")]
        public int maxConcurrentTasks = 4;

        [Header("Mesh Settings")]
        [Tooltip("Default LOD bias for generated LODs")]
        [Range(0f, 1f)]
        public float lodBias = 0.5f;
        
        [Tooltip("Enable mesh compression")]
        public bool enableMeshCompression = true;

        [Header("Texture Settings")]
        [Tooltip("Default texture compression quality")]
        [Range(0, 100)]
        public int textureQuality = 75;
        
        [Tooltip("Generate mipmaps by default")]
        public bool generateMipmaps = true;

        [Header("Build Settings")]
        [Tooltip("Strip engine code in release builds")]
        public bool stripEngineCode = true;
        
        [Tooltip("Enable incremental build support")]
        public bool incrementalBuild = false;

        [Header("Validation")]
        [Tooltip("Run validation before build")]
        public bool validateBeforeBuild = true;
        
        [Tooltip("Treat warnings as errors")]
        public bool warningsAsErrors = false;

        private static PipelineSettings instance;

        public static PipelineSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = LoadOrCreateSettings();
                }
                return instance;
            }
        }

        private static PipelineSettings LoadOrCreateSettings()
        {
            // Try to load existing settings
            PipelineSettings settings = AssetDatabase.LoadAssetAtPath<PipelineSettings>(SettingsPath);
            
            if (settings == null)
            {
                // Create new settings
                settings = CreateInstance<PipelineSettings>();
                
                // Ensure directory exists
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(SettingsPath));
                
                AssetDatabase.CreateAsset(settings, SettingsPath);
                AssetDatabase.SaveAssets();
                
                Debug.Log("[PipelineSettings] Created new settings asset");
            }
            else
            {
                Debug.Log("[PipelineSettings] Loaded existing settings");
            }
            
            return settings;
        }

        [MenuItem("Tools/Frontier/Pipeline Settings")]
        public static void OpenSettings()
        {
            Selection.activeObject = Instance;
        }

        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public void ResetToDefaults()
        {
            autoProcessOnImport = true;
            maxConcurrentTasks = 4;
            lodBias = 0.5f;
            enableMeshCompression = true;
            textureQuality = 75;
            generateMipmaps = true;
            stripEngineCode = true;
            incrementalBuild = false;
            validateBeforeBuild = true;
            warningsAsErrors = false;
            
            Save();
        }
    }

    /// <summary>
    /// Pipeline event definitions for extensibility.
    /// </summary>
    public static class PipelineEvents
    {
        public delegate void PipelineEventHandler(string stageName, PipelineStageStatus status);
        
        public enum PipelineStageStatus
        {
            Started,
            Completed,
            Failed,
            Skipped
        }

        public static event PipelineEventHandler OnStageStarted;
        public static event PipelineEventHandler OnStageCompleted;
        public static event PipelineEventHandler OnStageFailed;
        public static event PipelineEventHandler OnStageSkipped;

        public static void InvokeStageStarted(string stageName)
        {
            OnStageStarted?.Invoke(stageName, PipelineStageStatus.Started);
        }

        public static void InvokeStageCompleted(string stageName)
        {
            OnStageCompleted?.Invoke(stageName, PipelineStageStatus.Completed);
        }

        public static void InvokeStageFailed(string stageName)
        {
            OnStageFailed?.Invoke(stageName, PipelineStageStatus.Failed);
        }

        public static void InvokeStageSkipped(string stageName)
        {
            OnStageSkipped?.Invoke(stageName, PipelineStageStatus.Skipped);
        }
    }
}
