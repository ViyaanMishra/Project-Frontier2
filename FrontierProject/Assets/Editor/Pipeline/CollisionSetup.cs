using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace FrontierProject.Editor.Pipeline
{
    /// <summary>
    /// Collision setup processor for generating and configuring colliders on meshes.
    /// </summary>
    public class CollisionSetup
    {
        [MenuItem("Tools/Frontier/Setup Colliders")]
        public static void SetupCollidersMenu()
        {
            GameObject[] selected = Selection.gameObjects;
            if (selected.Length == 0)
            {
                Debug.LogWarning("[CollisionSetup] No objects selected");
                return;
            }

            foreach (GameObject obj in selected)
            {
                SetupColliderForObject(obj);
            }
        }

        public static void SetupColliderForObject(GameObject obj, ColliderType colliderType = ColliderType.Auto)
        {
            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();

            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                // Check children for meshes
                MeshFilter[] childFilters = obj.GetComponentsInChildren<MeshFilter>();
                if (childFilters.Length > 0)
                {
                    foreach (MeshFilter childFilter in childFilters)
                    {
                        SetupColliderForObject(childFilter.gameObject, colliderType);
                    }
                }
                return;
            }

            // Remove existing colliders
            Collider existingCollider = obj.GetComponent<Collider>();
            if (existingCollider != null)
            {
                Object.DestroyImmediate(existingCollider);
            }

            Mesh mesh = meshFilter.sharedMesh;
            Bounds bounds = mesh.bounds;

            switch (colliderType)
            {
                case ColliderType.Auto:
                    AutoSelectCollider(obj, mesh, bounds);
                    break;
                case ColliderType.Box:
                    obj.AddComponent<BoxCollider>();
                    break;
                case ColliderType.Sphere:
                    SphereCollider sphere = obj.AddComponent<SphereCollider>();
                    sphere.radius = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
                    break;
                case ColliderType.Capsule:
                    CapsuleCollider capsule = obj.AddComponent<CapsuleCollider>();
                    capsule.height = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                    capsule.radius = Mathf.Min(bounds.extents.x, bounds.extents.y, bounds.extents.z);
                    break;
                case ColliderType.Mesh:
                    MeshCollider meshCollider = obj.AddComponent<MeshCollider>();
                    meshCollider.sharedMesh = mesh;
                    meshCollider.convex = true;
                    break;
            }

            Debug.Log($"[CollisionSetup] Added collider to {obj.name}");
        }

        private static void AutoSelectCollider(GameObject obj, Mesh mesh, Bounds bounds)
        {
            float aspectRatio = bounds.size.x / Mathf.Max(bounds.size.y, bounds.size.z);
            float heightRatio = bounds.size.y / Mathf.Max(bounds.size.x, bounds.size.z);

            // Tall thin objects -> Capsule
            if (heightRatio > 2f)
            {
                CapsuleCollider capsule = obj.AddComponent<CapsuleCollider>();
                capsule.direction = 1; // Y-axis
                capsule.height = bounds.size.y;
                capsule.radius = Mathf.Max(bounds.extents.x, bounds.extents.z);
            }
            // Wide flat objects -> Box
            else if (aspectRatio > 3f || bounds.size.y < Mathf.Min(bounds.size.x, bounds.size.z) * 0.3f)
            {
                obj.AddComponent<BoxCollider>();
            }
            // Round/spherical objects -> Sphere
            else if (Mathf.Abs(bounds.extents.x - bounds.extents.y) < 0.1f && 
                     Mathf.Abs(bounds.extents.x - bounds.extents.z) < 0.1f)
            {
                SphereCollider sphere = obj.AddComponent<SphereCollider>();
                sphere.radius = bounds.extents.x;
            }
            // Complex shapes -> Mesh collider (convex for rigidbodies)
            else
            {
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                MeshCollider meshCollider = obj.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = mesh;
                meshCollider.convex = rb != null;
            }
        }

        public static void SetupComplexColliders(GameObject obj, int complexityLevel = 3)
        {
            // Generate multiple primitive colliders to approximate complex shape
            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null) return;

            Mesh mesh = meshFilter.sharedMesh;
            
            // Simple decomposition into boxes based on mesh sections
            // In production, this would use a proper convex decomposition algorithm
            
            Vector3[] vertices = mesh.vertices;
            Bounds totalBounds = mesh.bounds;
            
            // Divide into sections along the longest axis
            float maxLength = Mathf.Max(totalBounds.size.x, totalBounds.size.y, totalBounds.size.z);
            int divisions = Mathf.CeilToInt(maxLength / 0.5f); // 0.5m per collider
            
            List<BoxCollider> colliders = new List<BoxCollider>();
            
            for (int i = 0; i < Mathf.Min(divisions, complexityLevel); i++)
            {
                BoxCollider bc = obj.AddComponent<BoxCollider>();
                bc.size = new Vector3(totalBounds.size.x / divisions, totalBounds.size.y, totalBounds.size.z);
                bc.center = new Vector3(
                    totalBounds.min.x + (totalBounds.size.x / divisions) * (i + 0.5f) - totalBounds.center.x,
                    0,
                    0
                );
                colliders.Add(bc);
            }

            Debug.Log($"[CollisionSetup] Created {colliders.Count} colliders for {obj.name}");
        }

        public enum ColliderType
        {
            Auto,
            Box,
            Sphere,
            Capsule,
            Mesh
        }
    }
}
