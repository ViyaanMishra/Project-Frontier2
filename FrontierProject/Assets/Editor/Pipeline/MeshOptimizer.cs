using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace FrontierProject.Editor.Pipeline
{
    /// <summary>
    /// Mesh optimization processor for reducing polygon counts and optimizing mesh data.
    /// </summary>
    public class MeshOptimizer
    {
        [MenuItem("Tools/Frontier/Optimize Selected Meshes")]
        public static void OptimizeSelectedMeshes()
        {
            GameObject[] selected = Selection.gameObjects;
            if (selected.Length == 0)
            {
                Debug.LogWarning("[MeshOptimizer] No objects selected");
                return;
            }

            int optimizedCount = 0;
            foreach (GameObject obj in selected)
            {
                if (OptimizeGameObject(obj))
                {
                    optimizedCount++;
                }
            }

            Debug.Log($"[MeshOptimizer] Optimized {optimizedCount} objects");
        }

        public static bool OptimizeGameObject(GameObject obj)
        {
            MeshFilter[] meshFilters = obj.GetComponentsInChildren<MeshFilter>();
            bool modified = false;

            foreach (MeshFilter filter in meshFilters)
            {
                if (filter.sharedMesh != null)
                {
                    Mesh optimized = OptimizeMesh(filter.sharedMesh);
                    if (optimized != filter.sharedMesh)
                    {
                        Undo.RecordObject(filter, "Optimize Mesh");
                        filter.sharedMesh = optimized;
                        modified = true;
                    }
                }
            }

            return modified;
        }

        public static Mesh OptimizeMesh(Mesh inputMesh)
        {
            if (inputMesh == null) return null;

            // Check if mesh is read-only
            if (!inputMesh.isReadable)
            {
                Debug.LogWarning($"[MeshOptimizer] Mesh {inputMesh.name} is not readable. Skipping.");
                return inputMesh;
            }

            Vector3[] vertices = inputMesh.vertices;
            Vector3[] normals = inputMesh.normals;
            Vector4[] tangents = inputMesh.tangents;
            Vector2[] uv = inputMesh.uv;
            Vector2[] uv2 = inputMesh.uv2;
            Color[] colors = inputMesh.colors;
            int[] triangles = inputMesh.triangles;

            // Weld vertices (merge close vertices)
            Dictionary<Vector3, int> vertexMap = new Dictionary<Vector3, int>();
            List<Vector3> newVertices = new List<Vector3>();
            List<int> newIndexMap = new List<int>(vertices.Length);

            float weldThreshold = 0.001f;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 rounded = RoundVertex(vertices[i], weldThreshold);
                
                if (vertexMap.TryGetValue(rounded, out int existingIndex))
                {
                    newIndexMap.Add(existingIndex);
                }
                else
                {
                    int newIndex = newVertices.Count;
                    newVertices.Add(vertices[i]);
                    vertexMap[rounded] = newIndex;
                    newIndexMap.Add(newIndex);
                }
            }

            // Remap triangles
            int[] newTriangles = new int[triangles.Length];
            for (int i = 0; i < triangles.Length; i++)
            {
                newTriangles[i] = newIndexMap[triangles[i]];
            }

            // Create new mesh
            Mesh optimizedMesh = new Mesh();
            optimizedMesh.name = inputMesh.name + "_Optimized";
            optimizedMesh.vertices = newVertices.ToArray();
            optimizedMesh.triangles = newTriangles;

            // Copy attributes if they exist
            if (normals.Length > 0)
            {
                Vector3[] newNormals = new Vector3[newVertices.Count];
                for (int i = 0; i < newIndexMap.Count && i < normals.Length; i++)
                {
                    newNormals[newIndexMap[i]] = normals[i];
                }
                optimizedMesh.normals = newNormals;
            }

            if (tangents != null && tangents.Length > 0)
            {
                Vector4[] newTangents = new Vector4[newVertices.Count];
                for (int i = 0; i < newIndexMap.Count && i < tangents.Length; i++)
                {
                    newTangents[newIndexMap[i]] = tangents[i];
                }
                optimizedMesh.tangents = newTangents;
            }

            if (uv.Length > 0)
            {
                Vector2[] newUV = new Vector2[newVertices.Count];
                for (int i = 0; i < newIndexMap.Count && i < uv.Length; i++)
                {
                    newUV[newIndexMap[i]] = uv[i];
                }
                optimizedMesh.uv = newUV;
            }

            if (colors.Length > 0)
            {
                Color[] newColors = new Color[newVertices.Count];
                for (int i = 0; i < newIndexMap.Count && i < colors.Length; i++)
                {
                    newColors[newIndexMap[i]] = colors[i];
                }
                optimizedMesh.colors = newColors;
            }

            optimizedMesh.RecalculateBounds();
            
            Debug.Log($"[MeshOptimizer] Reduced {inputMesh.name}: {vertices.Length} -> {newVertices.Count} vertices");
            
            return optimizedMesh;
        }

        private static Vector3 RoundVertex(Vector3 vertex, float threshold)
        {
            return new Vector3(
                Mathf.Round(vertex.x / threshold) * threshold,
                Mathf.Round(vertex.y / threshold) * threshold,
                Mathf.Round(vertex.z / threshold) * threshold
            );
        }

        public static void SimplifyMesh(Mesh mesh, float reductionRatio)
        {
            // Placeholder for mesh simplification algorithm
            // In production, this would use a proper decimation algorithm
            Debug.Log($"[MeshOptimizer] Simplification requested for {mesh.name} with ratio {reductionRatio}");
        }
    }
}
