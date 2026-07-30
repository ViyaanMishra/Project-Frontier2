using UnityEngine;

namespace Frontier.MeshGen
{
    /// <summary>
    /// LOD generator for creating lower-detail mesh versions
    /// </summary>
    public static class LODGenerator
    {
        /// <summary>
        /// Generate LOD level (0 = original, higher = lower poly)
        /// </summary>
        public static Mesh Generate(Mesh sourceMesh, int lodLevel)
        {
            if (sourceMesh == null || lodLevel <= 0) return sourceMesh;
            
            Vector3[] vertices = sourceMesh.vertices;
            int[] triangles = sourceMesh.triangles;
            Vector3[] normals = sourceMesh.normals;
            Vector2[] uvs = sourceMesh.uv;
            Color[] colors = sourceMesh.colors;
            
            // Simple vertex decimation based on LOD level
            float decimationRate = Mathf.Clamp(0.2f * lodLevel, 0.2f, 0.8f);
            int targetVertexCount = Mathf.Max(10, Mathf.FloorToInt(vertices.Length * (1f - decimationRate)));
            
            if (targetVertexCount >= vertices.Length) return sourceMesh;
            
            // Create new mesh with reduced vertices
            LowPolyMeshBuilder.Begin();
            
            // Sample vertices (simple approach - in production use mesh simplification algorithm)
            int step = Mathf.Max(1, Mathf.FloorToInt(vertices.Length / (float)targetVertexCount));
            
            for (int i = 0; i < vertices.Length; i += step)
            {
                int idx = LowPolyMeshBuilder.AddVertex(
                    vertices[i],
                    colors != null && i < colors.Length ? colors[i] : Color.white,
                    uvs != null && i < uvs.Length ? uvs[i] : Vector2.zero,
                    normals != null && i < normals.Length ? normals[i] : Vector3.up
                );
            }
            
            // Rebuild triangles for remaining vertices
            // Note: This is simplified - proper LOD needs edge collapse algorithm
            
            Mesh result = LowPolyMeshBuilder.Build($"{sourceMesh.name}_LOD{lodLevel}");
            result.bounds = sourceMesh.bounds;
            
            return result;
        }
        
        /// <summary>
        /// Generate multiple LOD levels at once
        /// </summary>
        public static Mesh[] GenerateLODs(Mesh sourceMesh, int lodCount)
        {
            Mesh[] lods = new Mesh[lodCount];
            lods[0] = sourceMesh;
            
            for (int i = 1; i < lodCount; i++)
            {
                lods[i] = Generate(sourceMesh, i);
            }
            
            return lods;
        }
    }
}
