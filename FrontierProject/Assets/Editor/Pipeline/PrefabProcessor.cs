using UnityEngine;
using UnityEditor;
using System.IO;

namespace FrontierProject.Editor.Pipeline
{
    /// <summary>
    /// Prefab processor for batch prefab operations including validation, assembly, and updates.
    /// </summary>
    public class PrefabProcessor
    {
        [MenuItem("Tools/Frontier/Validate Selected Prefabs")]
        public static void ValidatePrefabs()
        {
            GameObject[] selected = Selection.gameObjects;
            if (selected.Length == 0)
            {
                Debug.LogWarning("[PrefabProcessor] No objects selected");
                return;
            }

            int validCount = 0;
            int invalidCount = 0;

            foreach (GameObject obj in selected)
            {
                if (ValidatePrefab(obj))
                {
                    validCount++;
                }
                else
                {
                    invalidCount++;
                }
            }

            Debug.Log($"[PrefabProcessor] Validation complete: {validCount} valid, {invalidCount} invalid");
        }

        public static bool ValidatePrefab(GameObject prefab)
        {
            bool isValid = true;
            string issues = "";

            // Check for MeshRenderer without MeshFilter
            MeshRenderer[] renderers = prefab.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
            {
                if (renderer.GetComponent<MeshFilter>() == null)
                {
                    issues += $"Missing MeshFilter on {renderer.gameObject.name}\n";
                    isValid = false;
                }
            }

            // Check for colliders on interactive objects
            if (prefab.CompareTag("Interactive") || prefab.CompareTag("Player") || prefab.CompareTag("Enemy"))
            {
                Collider[] colliders = prefab.GetComponentsInChildren<Collider>();
                if (colliders.Length == 0)
                {
                    issues += "Missing Collider on interactive object\n";
                    isValid = false;
                }
            }

            // Check for Rigidbody requirements
            Rigidbody[] rigidbodies = prefab.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in rigidbodies)
            {
                if (rb.GetComponent<Collider>() == null)
                {
                    issues += $"Rigidbody without Collider on {rb.gameObject.name}\n";
                    isValid = false;
                }
            }

            // Check material assignments
            MeshRenderer[] allRenderers = prefab.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in allRenderers)
            {
                if (renderer.sharedMaterials == null || renderer.sharedMaterials.Length == 0)
                {
                    issues += $"No materials assigned to {renderer.gameObject.name}\n";
                    isValid = false;
                }
                else
                {
                    foreach (Material mat in renderer.sharedMaterials)
                    {
                        if (mat == null)
                        {
                            issues += $"Missing material slot on {renderer.gameObject.name}\n";
                            isValid = false;
                        }
                    }
                }
            }

            // Check for missing script references
            MonoBehaviour[] behaviours = prefab.GetComponentsInChildren<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                SerializedObject so = new SerializedObject(behaviour);
                SerializedProperty prop = so.GetIterator();
                while (prop.NextVisible(true))
                {
                    if (prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue == null)
                    {
                        // Check if it's a required field (not optional)
                        if (!prop.displayName.ToLower().Contains("optional"))
                        {
                            issues += $"Missing reference in {behaviour.GetType().Name} on {behaviour.gameObject.name}\n";
                            isValid = false;
                        }
                    }
                }
            }

            if (!isValid)
            {
                Debug.LogWarning($"[PrefabProcessor] {prefab.name} has issues:\n{issues}");
            }
            else
            {
                Debug.Log($"[PrefabProcessor] {prefab.name} is valid");
            }

            return isValid;
        }

        public static GameObject AssemblePrefab(string prefabName, GameObject sourceObject, string outputPath)
        {
            // Create prefab from source object
            GameObject prefab = Object.Instantiate(sourceObject);
            prefab.name = prefabName;

            // Ensure output directory exists
            Directory.CreateDirectory(outputPath);

            string fullPath = Path.Combine(outputPath, prefabName + ".prefab");
            
            // Create or replace prefab
            PrefabUtility.SaveAsPrefabAsset(prefab, fullPath);
            
            Object.DestroyImmediate(prefab);
            
            Debug.Log($"[PrefabProcessor] Created prefab at {fullPath}");
            
            return AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
        }

        public static void BatchUpdatePrefabs(string folderPath, System.Action<GameObject> updateFunction)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { folderPath });
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab != null)
                {
                    GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    if (instance != null)
                    {
                        updateFunction(instance);
                        PrefabUtility.SaveAsPrefabAsset(instance, path);
                        Object.DestroyImmediate(instance);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[PrefabProcessor] Batch update complete for {guids.Length} prefabs");
        }

        public static void ReplaceMaterialOnPrefabs(string folderPath, Material oldMaterial, Material newMaterial)
        {
            BatchUpdatePrefabs(folderPath, (prefab) =>
            {
                MeshRenderer[] renderers = prefab.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer renderer in renderers)
                {
                    Material[] materials = renderer.sharedMaterials;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        if (materials[i] == oldMaterial)
                        {
                            materials[i] = newMaterial;
                        }
                    }
                    renderer.sharedMaterials = materials;
                }
            });
        }
    }
}
