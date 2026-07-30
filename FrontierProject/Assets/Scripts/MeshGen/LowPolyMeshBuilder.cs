using UnityEngine;
using System.Collections.Generic;

namespace Frontier.MeshGen
{
    /// <summary>
    /// High-performance low-poly mesh builder with vertex colors, flat normals, and sub-mesh support
    /// </summary>
    public static class LowPolyMeshBuilder
    {
        private static List<Vector3> vertices = new List<Vector3>();
        private static List<int> triangles = new List<int>();
        private static List<Vector3> normals = new List<Vector3>();
        private static List<Color> colors = new List<Color>();
        private static List<Vector2> uvs = new List<Vector2>();
        private static List<Vector4> tangents = new List<Vector4>();
        
        private static int subMeshStartIndex = 0;
        private static List<List<int>> subMeshTriangles = new List<List<int>>();
        
        public enum NormalMode { Flat, Smooth, Custom }
        
        /// <summary>
        /// Start a new mesh build session
        /// </summary>
        public static void Begin()
        {
            vertices.Clear();
            triangles.Clear();
            normals.Clear();
            colors.Clear();
            uvs.Clear();
            tangents.Clear();
            subMeshTriangles.Clear();
            subMeshStartIndex = 0;
        }
        
        /// <summary>
        /// Add a single vertex with optional properties
        /// </summary>
        public static int AddVertex(Vector3 position, Color? color = null, Vector2? uv = null, Vector3? normal = null)
        {
            int index = vertices.Count;
            vertices.Add(position);
            colors.Add(color ?? Color.white);
            uvs.Add(uv ?? Vector2.zero);
            
            if (normal.HasValue)
                normals.Add(normal.Value.normalized);
            else
                normals.Add(Vector3.up); // Placeholder, calculated later for flat shading
                
            tangents.Add(new Vector4(1, 0, 0, 1));
            
            return index;
        }
        
        /// <summary>
        /// Add multiple vertices at once
        /// </summary>
        public static int[] AddVertices(Vector3[] positions, Color[] colors = null, Vector2[] uvs = null)
        {
            int startIndex = vertices.Count;
            int count = positions.Length;
            
            for (int i = 0; i < count; i++)
            {
                vertices.Add(positions[i]);
                this.colors.Add(colors != null && i < colors.Length ? colors[i] : Color.white);
                this.uvs.Add(uvs != null && i < uvs.Length ? uvs[i] : Vector2.zero);
                normals.Add(Vector3.up);
                tangents.Add(new Vector4(1, 0, 0, 1));
            }
            
            int[] indices = new int[count];
            for (int i = 0; i < count; i++)
                indices[i] = startIndex + i;
                
            return indices;
        }
        
        /// <summary>
        /// Add a triangle (3 vertices)
        /// </summary>
        public static void AddTriangle(int v0, int v1, int v2, bool calculateFlatNormal = true)
        {
            triangles.Add(v0);
            triangles.Add(v1);
            triangles.Add(v2);
            
            if (calculateFlatNormal && normals.Count >= Mathf.Max(v0, v1, v2) + 1)
            {
                Vector3 normal = CalculateFlatNormal(vertices[v0], vertices[v1], vertices[v2]);
                normals[v0] = normal;
                normals[v1] = normal;
                normals[v2] = normal;
            }
        }
        
        /// <summary>
        /// Add a quad (4 vertices, 2 triangles)
        /// </summary>
        public static void AddQuad(int v0, int v1, int v2, int v3, bool calculateFlatNormal = true)
        {
            AddTriangle(v0, v1, v2, calculateFlatNormal);
            AddTriangle(v0, v2, v3, calculateFlatNormal);
        }
        
        /// <summary>
        /// Start a new sub-mesh (for multiple materials on one mesh)
        /// </summary>
        public static void BeginSubMesh()
        {
            subMeshTriangles.Add(new List<int>());
            subMeshStartIndex = triangles.Count;
        }
        
        /// <summary>
        /// End current sub-mesh
        /// </summary>
        public static void EndSubMesh()
        {
            if (subMeshTriangles.Count > 0)
            {
                int count = triangles.Count - subMeshStartIndex;
                int[] subTris = new int[count];
                for (int i = 0; i < count; i++)
                    subTris[i] = triangles[subMeshStartIndex + i];
                subMeshTriangles[subMeshTriangles.Count - 1].AddRange(subTris);
            }
        }
        
        /// <summary>
        /// Build the final Unity Mesh
        /// </summary>
        public static Mesh Build(string name = "LowPolyMesh", bool optimize = true)
        {
            Mesh mesh = new Mesh();
            mesh.name = name;
            
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetNormals(normals);
            mesh.SetColors(colors);
            mesh.SetUVs(0, uvs);
            mesh.SetTangents(tangents);
            
            if (subMeshTriangles.Count > 1)
            {
                mesh.subMeshCount = subMeshTriangles.Count;
                for (int i = 0; i < subMeshTriangles.Count; i++)
                {
                    mesh.SetTriangles(subMeshTriangles[i], i);
                }
            }
            
            if (optimize)
            {
                mesh.Optimize();
                mesh.RecalculateBounds();
            }
            
            return mesh;
        }
        
        /// <summary>
        /// Calculate flat normal for a triangle (for faceted low-poly look)
        /// </summary>
        public static Vector3 CalculateFlatNormal(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            Vector3 side0 = v1 - v0;
            Vector3 side1 = v2 - v0;
            Vector3 normal = Vector3.Cross(side0, side1).normalized;
            return float.IsNaN(normal.x) || float.IsNaN(normal.y) || float.IsNaN(normal.z) ? Vector3.up : normal;
        }
        
        /// <summary>
        /// Apply vertex color gradient (top to bottom)
        /// </summary>
        public static void ApplyVerticalGradient(Color topColor, Color bottomColor)
        {
            if (vertices.Count == 0) return;
            
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            
            foreach (var v in vertices)
            {
                minY = Mathf.Min(minY, v.y);
                maxY = Mathf.Max(maxY, v.y);
            }
            
            float range = maxY - minY;
            if (range < 0.001f) range = 1f;
            
            for (int i = 0; i < vertices.Count; i++)
            {
                float t = Mathf.InverseLerp(minY, maxY, vertices[i].y);
                colors[i] = Color.Lerp(bottomColor, topColor, t);
            }
        }
        
        /// <summary>
        /// Apply random color variation to vertices
        /// </summary>
        public static void ApplyColorVariation(float hueShift = 0.05f, float satShift = 0.1f, float valShift = 0.1f)
        {
            for (int i = 0; i < colors.Count; i++)
            {
                Color.RGBToHSV(colors[i], out float h, out float s, out float v);
                h += Random.Range(-hueShift, hueShift);
                s = Mathf.Clamp01(s + Random.Range(-satShift, satShift));
                v = Mathf.Clamp01(v + Random.Range(-valShift, valShift));
                colors[i] = Color.HSVToRGB(h, s, v);
            }
        }
        
        /// <summary>
        /// Get current vertex count
        /// </summary>
        public static int VertexCount => vertices.Count;
        
        /// <summary>
        /// Get current triangle count
        /// </summary>
        public static int TriangleCount => triangles.Count / 3;
    }
}
