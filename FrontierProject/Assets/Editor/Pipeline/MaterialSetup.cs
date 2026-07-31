using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace FrontierProject.Editor.Pipeline
{
    /// <summary>
    /// Material setup processor for batch material creation and configuration.
    /// </summary>
    public class MaterialSetup
    {
        [MenuItem("Tools/Frontier/Setup Materials")]
        public static void SetupMaterialsMenu()
        {
            GameObject[] selected = Selection.gameObjects;
            if (selected.Length == 0)
            {
                Debug.LogWarning("[MaterialSetup] No objects selected");
                return;
            }

            foreach (GameObject obj in selected)
            {
                SetupMaterialsForObject(obj);
            }
        }

        public static void SetupMaterialsForObject(GameObject obj)
        {
            MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>();
            
            foreach (MeshRenderer renderer in renderers)
            {
                if (renderer.sharedMaterials == null || renderer.sharedMaterials.Length == 0)
                {
                    // Create default material
                    Material defaultMat = GetDefaultMaterial();
                    renderer.material = defaultMat;
                    Debug.Log($"[MaterialSetup] Assigned default material to {renderer.gameObject.name}");
                }
                else
                {
                    // Validate existing materials
                    Material[] materials = renderer.sharedMaterials;
                    bool needsUpdate = false;
                    
                    for (int i = 0; i < materials.Length; i++)
                    {
                        if (materials[i] == null)
                        {
                            materials[i] = GetDefaultMaterial();
                            needsUpdate = true;
                        }
                    }
                    
                    if (needsUpdate)
                    {
                        renderer.sharedMaterials = materials;
                    }
                }
            }
        }

        private static Material GetDefaultMaterial()
        {
            // Try to find existing default material
            string[] guids = AssetDatabase.FindAssets("t:Material", new string[] { "Assets/Materials" });
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.ToLower().Contains("default"))
                {
                    return AssetDatabase.LoadAssetAtPath<Material>(path);
                }
            }
            
            // Create new default material if none exists
            Material defaultMat = new Material(Shader.Find("Standard"));
            defaultMat.name = "DefaultMaterial";
            defaultMat.color = Color.gray;
            
            // Save to Assets/Materials
            System.IO.Directory.CreateDirectory("Assets/Materials");
            AssetDatabase.CreateAsset(defaultMat, "Assets/Materials/DefaultMaterial.mat");
            AssetDatabase.SaveAssets();
            
            return defaultMat;
        }

        public static Material CreatePBRMaterial(string name, Texture albedo, Texture normal, Texture metallic, Texture occlusion)
        {
            Shader standardShader = Shader.Find("Standard");
            if (standardShader == null)
            {
                Debug.LogError("[MaterialSetup] Standard shader not found");
                return null;
            }

            Material mat = new Material(standardShader);
            mat.name = name;

            if (albedo != null)
            {
                mat.SetTexture("_MainTex", albedo);
                mat.EnableKeyword("_ALPHATEST_ON");
            }

            if (normal != null)
            {
                mat.SetTexture("_BumpMap", normal);
                mat.EnableKeyword("_NORMALMAP");
            }

            if (metallic != null)
            {
                mat.SetTexture("_MetallicGlossMap", metallic);
            }

            if (occlusion != null)
            {
                mat.SetTexture("_OcclusionMap", occlusion);
            }

            // Save material
            string savePath = $"Assets/Materials/{name}.mat";
            System.IO.Directory.CreateDirectory("Assets/Materials");
            AssetDatabase.CreateAsset(mat, savePath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[MaterialSetup] Created PBR material: {name}");
            return mat;
        }

        public static void BatchAssignMaterial(string folderPath, Material material, string objectTag = "")
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { folderPath });
            
            int assignedCount = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab != null)
                {
                    GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    if (instance != null)
                    {
                        MeshRenderer[] renderers = instance.GetComponentsInChildren<MeshRenderer>();
                        foreach (MeshRenderer renderer in renderers)
                        {
                            if (string.IsNullOrEmpty(objectTag) || renderer.CompareTag(objectTag))
                            {
                                renderer.material = material;
                                assignedCount++;
                            }
                        }
                        
                        PrefabUtility.SaveAsPrefabAsset(instance, path);
                        Object.DestroyImmediate(instance);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[MaterialSetup] Assigned material to {assignedCount} objects");
        }

        public static Material ConvertToURP(Material standardMaterial)
        {
            // Placeholder for URP conversion
            Debug.Log("[MaterialSetup] URP conversion requested");
            return standardMaterial;
        }

        public static Material ConvertToHDRP(Material standardMaterial)
        {
            // Placeholder for HDRP conversion
            Debug.Log("[MaterialSetup] HDRP conversion requested");
            return standardMaterial;
        }
    }
}
