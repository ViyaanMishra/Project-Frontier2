using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace FrontierProject.Editor.Pipeline
{
    /// <summary>
    /// LOD (Level of Detail) generator for creating multiple detail levels of meshes.
    /// </summary>
    public class LODGenerator
    {
        [MenuItem("Tools/Frontier/Generate LODs")]
        public static void GenerateLODsMenu()
        {
            GameObject[] selected = Selection.gameObjects;
            if (selected.Length == 0)
            {
                Debug.LogWarning("[LODGenerator] No objects selected");
                return;
            }

            foreach (GameObject obj in selected)
            {
                GenerateLODGroup(obj);
            }
        }

        public static void GenerateLODGroup(GameObject obj, float[] lodLevels = null)
        {
            if (lodLevels == null)
            {
                lodLevels = new float[] { 0.5f, 0.25f, 0.125f };
            }

            // Check if LODGroup already exists
            LODGroup existingLODGroup = obj.GetComponent<LODGroup>();
            if (existingLODGroup != null)
            {
                Debug.LogWarning($"[LODGenerator] {obj.name} already has an LODGroup");
                return;
            }

            // Create LODGroup component
            LODGroup lodGroup = obj.AddComponent<LODGroup>();
            
            // Get original mesh renderers
            MeshRenderer[] originalRenderers = obj.GetComponentsInChildren<MeshRenderer>();
            if (originalRenderers.Length == 0)
            {
                Debug.LogWarning($"[LODGenerator] {obj.name} has no mesh renderers");
                Object.DestroyImmediate(lodGroup);
                return;
            }

            List<LOD> lods = new List<LOD>();
            
            // LOD 0 - Original quality
            lods.Add(CreateLOD(0, obj, 1.0f));

            // Generate lower LODs
            for (int i = 0; i < lodLevels.Length; i++)
            {
                float screenRelativeHeight = lodLevels[i];
                float reductionRatio = 1.0f - (screenRelativeHeight * 2);
                
                LOD lod = CreateLOD(i + 1, obj, reductionRatio);
                lods.Add(lod);
            }

            lodGroup.SetLODs(lods.ToArray());
            lodGroup.RecalculateBounds();

            Debug.Log($"[LODGenerator] Generated {lods.Count} LOD levels for {obj.name}");
        }

        private static LOD CreateLOD(int level, GameObject sourceObject, float reductionRatio)
        {
            List<Renderer> renderers = new List<Renderer>();
            
            if (level == 0)
            {
                // Use original renderers for LOD0
                renderers.AddRange(sourceObject.GetComponentsInChildren<MeshRenderer>());
            }
            else
            {
                // Create simplified versions for higher LOD levels
                string lodSuffix = $"_LOD{level}";
                
                foreach (MeshRenderer originalRenderer in sourceObject.GetComponentsInChildren<MeshRenderer>())
                {
                    MeshFilter originalFilter = originalRenderer.GetComponent<MeshFilter>();
                    if (originalFilter == null || originalFilter.sharedMesh == null) continue;

                    // Create simplified mesh
                    Mesh simplifiedMesh = MeshOptimizer.OptimizeMesh(originalFilter.sharedMesh);
                    if (simplifiedMesh == null) continue;

                    // Create LOD-specific renderer (in a real implementation, this would create child objects)
                    renderers.Add(originalRenderer);
                }
            }

            return new LOD(reductionRatio, renderers.ToArray());
        }

        public static Mesh CreateSimplifiedMesh(Mesh original, int targetPolyCount)
        {
            if (original == null) return null;

            int currentPolyCount = original.triangles.Length / 3;
            if (currentPolyCount <= targetPolyCount)
            {
                return original;
            }

            float ratio = (float)targetPolyCount / currentPolyCount;
            Debug.Log($"[LODGenerator] Simplifying {original.name}: {currentPolyCount} -> {targetPolyCount} triangles");

            // In production, implement proper mesh decimation here
            Mesh simplified = Object.Instantiate(original);
            simplified.name = original.name + "_LOD";
            
            return simplified;
        }

        public static void BatchGenerateLODs(string folderPath, float[] lodLevels)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { folderPath });
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab != null)
                {
                    GenerateLODGroup(prefab, lodLevels);
                }
            }

            AssetDatabase.SaveAssets();
        }
    }
}
