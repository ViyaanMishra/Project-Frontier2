using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using FrontierProject.Core;

namespace FrontierProject.Editor.Pipeline
{
    /// <summary>
    /// Master asset pipeline controller that orchestrates all asset processing operations.
    /// Handles batch imports, validations, transformations, and build preparations.
    /// </summary>
    public class ProjectAssetPipeline : EditorWindow
    {
        [MenuItem("Tools/Frontier/Asset Pipeline")]
        public static void ShowWindow()
        {
            var window = GetWindow<ProjectAssetPipeline>("Asset Pipeline");
            window.minSize = new Vector2(400, 600);
        }

        private List<PipelineStage> stages = new List<PipelineStage>();
        private bool isRunning = false;
        private int currentStageIndex = 0;
        private float progress = 0f;

        private Vector2 scrollPosition;
        private bool autoSave = true;
        private bool validateOnly = false;
        private LogLevel logLevel = LogLevel.Info;

        private enum LogLevel
        {
            Error,
            Warning,
            Info,
            Debug
        }

        private void OnEnable()
        {
            InitializeStages();
            EditorApplication.update += OnUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnUpdate;
        }

        private void InitializeStages()
        {
            stages.Clear();
            stages.Add(new PipelineStage("Import Validation", ValidateImports));
            stages.Add(new PipelineStage("Mesh Optimization", OptimizeMeshes));
            stages.Add(new PipelineStage("Texture Processing", ProcessTextures));
            stages.Add(new PipelineStage("Material Setup", SetupMaterials));
            stages.Add(new PipelineStage("LOD Generation", GenerateLODs));
            stages.Add(new PipelineStage("Collision Setup", SetupColliders));
            stages.Add(new PipelineStage("Prefab Assembly", AssemblePrefabs));
            stages.Add(new PipelineStage("Build Preparation", PrepareBuild));
        }

        private void OnGUI()
        {
            GUILayout.Label("Project Asset Pipeline", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Settings
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Pipeline Settings", EditorStyles.boldLabel);
            autoSave = EditorGUILayout.Toggle("Auto Save", autoSave);
            validateOnly = EditorGUILayout.Toggle("Validate Only", validateOnly);
            logLevel = (LogLevel)EditorGUILayout.EnumPopup("Log Level", logLevel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Stage list
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Pipeline Stages", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            for (int i = 0; i < stages.Count; i++)
            {
                DrawStage(stages[i], i);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Progress bar
            if (isRunning)
            {
                EditorGUILayout.ProgressBar(progress, $"Processing: {currentStageIndex + 1}/{stages.Count}");
            }

            EditorGUILayout.Space();

            // Controls
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Run Pipeline", GUILayout.Height(30)))
            {
                RunPipeline();
            }
            if (GUILayout.Button("Stop", GUILayout.Height(30)))
            {
                StopPipeline();
            }
            if (GUILayout.Button("Reset", GUILayout.Height(30)))
            {
                ResetPipeline();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawStage(PipelineStage stage, int index)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(isRunning);
            stage.enabled = EditorGUILayout.Toggle(stage.enabled, GUILayout.Width(20));
            EditorGUI.EndDisabledGroup();
            
            GUIStyle style = new GUIStyle(EditorStyles.label);
            if (index == currentStageIndex && isRunning)
            {
                style.fontStyle = FontStyle.Bold;
                style.normal.textColor = Color.green;
            }
            else if (!stage.enabled)
            {
                style.normal.textColor = Color.gray;
            }
            
            EditorGUILayout.LabelField(stage.name, style);
            EditorGUILayout.LabelField(stage.status, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();
        }

        private async void RunPipeline()
        {
            if (isRunning) return;
            
            isRunning = true;
            currentStageIndex = 0;
            progress = 0f;

            Log($"Starting asset pipeline - {stages.Count} stages");

            for (int i = 0; i < stages.Count; i++)
            {
                if (!stages[i].enabled) continue;

                currentStageIndex = i;
                stages[i].status = "Running...";
                
                try
                {
                    await stages[i].action.Invoke();
                    stages[i].status = "Complete";
                    progress = (float)(i + 1) / stages.Count;
                }
                catch (System.Exception e)
                {
                    stages[i].status = "Failed";
                    LogError($"Stage '{stages[i].name}' failed: {e.Message}");
                    if (!validateOnly)
                    {
                        StopPipeline();
                        return;
                    }
                }
            }

            Log("Pipeline completed successfully");
            isRunning = false;
            
            if (autoSave)
            {
                AssetDatabase.SaveAssets();
            }
        }

        private void StopPipeline()
        {
            isRunning = false;
            Log("Pipeline stopped by user");
        }

        private void ResetPipeline()
        {
            StopPipeline();
            currentStageIndex = 0;
            progress = 0f;
            foreach (var stage in stages)
            {
                stage.status = "Pending";
            }
        }

        private void OnUpdate()
        {
            if (isRunning)
            {
                Repaint();
            }
        }

        private void Log(string message)
        {
            if (logLevel >= LogLevel.Info)
            {
                Debug.Log($"[AssetPipeline] {message}");
            }
        }

        private void LogWarning(string message)
        {
            if (logLevel >= LogLevel.Warning)
            {
                Debug.LogWarning($"[AssetPipeline] {message}");
            }
        }

        private void LogError(string message)
        {
            if (logLevel >= LogLevel.Error)
            {
                Debug.LogError($"[AssetPipeline] {message}");
            }
        }

        // Stage action delegates
        private delegate System.Threading.Tasks.Task PipelineAction();

        private System.Threading.Tasks.Task ValidateImports()
        {
            Log("Validating imported assets...");
            // Implementation for import validation
            return System.Threading.Tasks.Task.CompletedTask;
        }

        private System.Threading.Tasks.Task OptimizeMeshes()
        {
            Log("Optimizing meshes...");
            // Implementation for mesh optimization
            return System.Threading.Tasks.Task.CompletedTask;
        }

        private System.Threading.Tasks.Task ProcessTextures()
        {
            Log("Processing textures...");
            // Implementation for texture processing
            return System.Threading.Tasks.Task.CompletedTask;
        }

        private System.Threading.Tasks.Task SetupMaterials()
        {
            Log("Setting up materials...");
            // Implementation for material setup
            return System.Threading.Tasks.Task.CompletedTask;
        }

        private System.Threading.Tasks.Task GenerateLODs()
        {
            Log("Generating LODs...");
            // Implementation for LOD generation
            return System.Threading.Tasks.Task.CompletedTask;
        }

        private System.Threading.Tasks.Task SetupColliders()
        {
            Log("Setting up colliders...");
            // Implementation for collider setup
            return System.Threading.Tasks.Task.CompletedTask;
        }

        private System.Threading.Tasks.Task AssemblePrefabs()
        {
            Log("Assembling prefabs...");
            // Implementation for prefab assembly
            return System.Threading.Tasks.Task.CompletedTask;
        }

        private System.Threading.Tasks.Task PrepareBuild()
        {
            Log("Preparing for build...");
            // Implementation for build preparation
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }

    public class PipelineStage
    {
        public string name;
        public bool enabled = true;
        public string status = "Pending";
        public System.Func<System.Threading.Tasks.Task> action;

        public PipelineStage(string name, System.Func<System.Threading.Tasks.Task> action)
        {
            this.name = name;
            this.action = action;
        }
    }
}
