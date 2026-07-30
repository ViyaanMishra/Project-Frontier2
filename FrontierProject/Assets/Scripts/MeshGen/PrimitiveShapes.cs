using UnityEngine;

namespace Frontier.MeshGen
{
    /// <summary>
    /// Generate basic primitive shapes for low-poly assets
    /// </summary>
    public static class PrimitiveShapes
    {
        /// <summary>
        /// Create a low-poly box with optional bevel
        /// </summary>
        public static Mesh CreateBox(float width = 1f, float height = 1f, float depth = 1f, int segments = 1)
        {
            LowPolyMeshBuilder.Begin();
            
            float hx = width / 2f;
            float hy = height / 2f;
            float hz = depth / 2f;
            
            // 8 corners
            int v0 = LowPolyMeshBuilder.AddVertex(new Vector3(-hx, -hy, -hz));
            int v1 = LowPolyMeshBuilder.AddVertex(new Vector3(hx, -hy, -hz));
            int v2 = LowPolyMeshBuilder.AddVertex(new Vector3(hx, hy, -hz));
            int v3 = LowPolyMeshBuilder.AddVertex(new Vector3(-hx, hy, -hz));
            int v4 = LowPolyMeshBuilder.AddVertex(new Vector3(-hx, -hy, hz));
            int v5 = LowPolyMeshBuilder.AddVertex(new Vector3(hx, -hy, hz));
            int v6 = LowPolyMeshBuilder.AddVertex(new Vector3(hx, hy, hz));
            int v7 = LowPolyMeshBuilder.AddVertex(new Vector3(-hx, hy, hz));
            
            // Front face
            LowPolyMeshBuilder.AddQuad(v0, v1, v2, v3);
            // Back face
            LowPolyMeshBuilder.AddQuad(v5, v4, v7, v6);
            // Left face
            LowPolyMeshBuilder.AddQuad(v4, v0, v3, v7);
            // Right face
            LowPolyMeshBuilder.AddQuad(v1, v5, v6, v2);
            // Top face
            LowPolyMeshBuilder.AddQuad(v3, v2, v6, v7);
            // Bottom face
            LowPolyMeshBuilder.AddQuad(v4, v5, v1, v0);
            
            return LowPolyMeshBuilder.Build($"Box_{width}x{height}x{depth}");
        }
        
        /// <summary>
        /// Create a low-poly cylinder
        /// </summary>
        public static Mesh CreateCylinder(float radius = 0.5f, float height = 1f, int segments = 8, bool capped = true)
        {
            LowPolyMeshBuilder.Begin();
            
            float halfHeight = height / 2f;
            int centerBottom = LowPolyMeshBuilder.AddVertex(new Vector3(0, -halfHeight, 0));
            int centerTop = LowPolyMeshBuilder.AddVertex(new Vector3(0, halfHeight, 0));
            
            int[] bottomVerts = new int[segments];
            int[] topVerts = new int[segments];
            
            float angleStep = Mathf.PI * 2f / segments;
            
            for (int i = 0; i < segments; i++)
            {
                float angle = i * angleStep;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                
                bottomVerts[i] = LowPolyMeshBuilder.AddVertex(new Vector3(x, -halfHeight, z));
                topVerts[i] = LowPolyMeshBuilder.AddVertex(new Vector3(x, halfHeight, z));
            }
            
            // Side faces
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                LowPolyMeshBuilder.AddQuad(bottomVerts[i], bottomVerts[next], topVerts[next], topVerts[i]);
            }
            
            // Bottom cap
            if (capped)
            {
                for (int i = 0; i < segments; i++)
                {
                    int next = (i + 1) % segments;
                    LowPolyMeshBuilder.AddTriangle(centerBottom, bottomVerts[next], bottomVerts[i]);
                }
                
                // Top cap
                for (int i = 0; i < segments; i++)
                {
                    int next = (i + 1) % segments;
                    LowPolyMeshBuilder.AddTriangle(centerTop, topVerts[i], topVerts[next]);
                }
            }
            
            return LowPolyMeshBuilder.Build($"Cylinder_r{radius}_h{height}_s{segments}");
        }
        
        /// <summary>
        /// Create a low-poly cone
        /// </summary>
        public static Mesh CreateCone(float radius = 0.5f, float height = 1f, int segments = 8)
        {
            LowPolyMeshBuilder.Begin();
            
            int apex = LowPolyMeshBuilder.AddVertex(new Vector3(0, height / 2f, 0));
            int center = LowPolyMeshBuilder.AddVertex(new Vector3(0, -height / 2f, 0));
            
            int[] baseVerts = new int[segments];
            float angleStep = Mathf.PI * 2f / segments;
            
            for (int i = 0; i < segments; i++)
            {
                float angle = i * angleStep;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                baseVerts[i] = LowPolyMeshBuilder.AddVertex(new Vector3(x, -height / 2f, z));
            }
            
            // Side faces
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                LowPolyMeshBuilder.AddTriangle(apex, baseVerts[next], baseVerts[i]);
            }
            
            // Bottom cap
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                LowPolyMeshBuilder.AddTriangle(center, baseVerts[i], baseVerts[next]);
            }
            
            return LowPolyMeshBuilder.Build($"Cone_r{radius}_h{height}_s{segments}");
        }
        
        /// <summary>
        /// Create a low-poly icosphere
        /// </summary>
        public static Mesh CreateIcosphere(float radius = 0.5f, int subdivisions = 0)
        {
            LowPolyMeshBuilder.Begin();
            
            float t = (1f + Mathf.Sqrt(5f)) / 2f;
            
            Vector3[] vertices = new Vector3[12]
            {
                new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
                new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
                new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1)
            };
            
            int[] indices = new int[60]
            {
                0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11,
                1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
                3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9,
                4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1
            };
            
            int[] baseVerts = new int[12];
            for (int i = 0; i < 12; i++)
            {
                baseVerts[i] = LowPolyMeshBuilder.AddVertex(vertices[i].normalized * radius);
            }
            
            for (int i = 0; i < 60; i += 3)
            {
                LowPolyMeshBuilder.AddTriangle(baseVerts[indices[i]], baseVerts[indices[i + 1]], baseVerts[indices[i + 2]]);
            }
            
            return LowPolyMeshBuilder.Build($"Icosphere_r{radius}_sub{subdivisions}");
        }
        
        /// <summary>
        /// Create a wedge/prism shape
        /// </summary>
        public static Mesh CreateWedge(float width = 1f, float height = 1f, float depth = 1f)
        {
            LowPolyMeshBuilder.Begin();
            
            float hw = width / 2f;
            
            int v0 = LowPolyMeshBuilder.AddVertex(new Vector3(-hw, 0, 0));
            int v1 = LowPolyMeshBuilder.AddVertex(new Vector3(hw, 0, 0));
            int v2 = LowPolyMeshBuilder.AddVertex(new Vector3(0, height, 0));
            int v3 = LowPolyMeshBuilder.AddVertex(new Vector3(-hw, 0, depth));
            int v4 = LowPolyMeshBuilder.AddVertex(new Vector3(hw, 0, depth));
            int v5 = LowPolyMeshBuilder.AddVertex(new Vector3(0, height, depth));
            
            // Front triangle
            LowPolyMeshBuilder.AddTriangle(v0, v1, v2);
            // Back triangle
            LowPolyMeshBuilder.AddTriangle(v4, v3, v5);
            // Left face
            LowPolyMeshBuilder.AddQuad(v0, v3, v5, v2);
            // Right face
            LowPolyMeshBuilder.AddQuad(v1, v4, v5, v2);
            // Bottom face
            LowPolyMeshBuilder.AddQuad(v0, v1, v4, v3);
            
            return LowPolyMeshBuilder.Build($"Wedge_{width}x{height}x{depth}");
        }
    }
}
