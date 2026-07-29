using UnityEngine;

namespace Frontier.MeshGen
{
    /// <summary>
    /// Mesh modifiers for deforming and manipulating geometry
    /// </summary>
    public static class MeshModifiers
    {
        /// <summary>
        /// Apply noise displacement to vertices
        /// </summary>
        public static void ApplyNoiseDisplacement(Mesh mesh, float strength = 0.1f, float scale = 1f, int seed = 0)
        {
            Vector3[] vertices = mesh.vertices;
            System.Random rng = new System.Random(seed);
            float offsetX = rng.Next(0, 10000);
            float offsetY = rng.Next(0, 10000);
            
            for (int i = 0; i < vertices.Length; i++)
            {
                float nx = vertices[i].x / scale + offsetX;
                float ny = vertices[i].y / scale + offsetY;
                float nz = vertices[i].z / scale;
                
                float noise = Mathf.PerlinNoise(nx, ny) * 2f - 1f;
                vertices[i] += mesh.normals[i] * noise * strength;
            }
            
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
        
        /// <summary>
        /// Bend mesh along an axis
        /// </summary>
        public static void ApplyBend(Mesh mesh, float bendAngle, Vector3 axis)
        {
            Vector3[] vertices = mesh.vertices;
            float angleRad = bendAngle * Mathf.Deg2Rad;
            
            for (int i = 0; i < vertices.Length; i++)
            {
                float dist = Vector3.Dot(vertices[i], axis.normalized);
                float offset = dist / axis.magnitude;
                
                float sinA = Mathf.Sin(offset * angleRad);
                float cosA = Mathf.Cos(offset * angleRad);
                
                Vector3 perp = Vector3.Cross(axis, Vector3.up).normalized;
                if (perp.sqrMagnitude < 0.01f)
                    perp = Vector3.Cross(axis, Vector3.right).normalized;
                
                vertices[i] = vertices[i] * cosA + perp * sinA * offset;
            }
            
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
        }
        
        /// <summary>
        /// Twist mesh along Y axis
        /// </summary>
        public static void ApplyTwist(Mesh mesh, float twistAngle, float height)
        {
            Vector3[] vertices = mesh.vertices;
            float angleRad = twistAngle * Mathf.Deg2Rad;
            
            for (int i = 0; i < vertices.Length; i++)
            {
                float t = Mathf.InverseLerp(-height / 2f, height / 2f, vertices[i].y);
                float rotation = t * angleRad;
                
                float cosR = Mathf.Cos(rotation);
                float sinR = Mathf.Sin(rotation);
                
                float x = vertices[i].x * cosR - vertices[i].z * sinR;
                float z = vertices[i].x * sinR + vertices[i].z * cosR;
                
                vertices[i].x = x;
                vertices[i].z = z;
            }
            
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
        }
        
        /// <summary>
        /// Taper mesh along Y axis
        /// </summary>
        public static void ApplyTaper(Mesh mesh, float taperFactor)
        {
            Vector3[] vertices = mesh.vertices;
            
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            
            foreach (var v in vertices)
            {
                minY = Mathf.Min(minY, v.y);
                maxY = Mathf.Max(maxY, v.y);
            }
            
            float range = maxY - minY;
            if (range < 0.001f) return;
            
            for (int i = 0; i < vertices.Length; i++)
            {
                float t = Mathf.InverseLerp(minY, maxY, vertices[i].y);
                float scale = Mathf.Lerp(1f, taperFactor, t);
                vertices[i].x *= scale;
                vertices[i].z *= scale;
            }
            
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
        }
        
        /// <summary>
        /// Bevel edges by adding supporting geometry
        /// </summary>
        public static void ApplyBevel(Mesh mesh, float bevelAmount, int iterations = 1)
        {
            // Simplified bevel - just smooths sharp edges via vertex displacement
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            
            for (int iter = 0; iter < iterations; iter++)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    // Move vertices slightly inward based on normal
                    vertices[i] -= normals[i] * bevelAmount * 0.1f;
                }
            }
            
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
        }
        
        /// <summary>
        /// Mirror mesh across plane
        /// </summary>
        public static Mesh Mirror(Mesh source, Vector3 planeNormal, float planeDistance = 0f)
        {
            LowPolyMeshBuilder.Begin();
            
            Vector3[] vertices = source.vertices;
            Vector3[] normals = source.normals;
            Vector2[] uvs = source.uv;
            Color[] colors = source.colors;
            
            int[] originalIndices = new int[vertices.Length];
            
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 mirrored = MirrorPoint(vertices[i], planeNormal, planeDistance);
                originalIndices[i] = LowPolyMeshBuilder.AddVertex(
                    mirrored,
                    colors != null && i < colors.Length ? colors[i] : Color.white,
                    uvs != null && i < uvs.Length ? uvs[i] : Vector2.zero,
                    -normals[i]
                );
            }
            
            // Reverse winding order for mirrored mesh
            int[] triangles = source.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                LowPolyMeshBuilder.AddTriangle(originalIndices[triangles[i + 2]], 
                                               originalIndices[triangles[i + 1]], 
                                               originalIndices[triangles[i]]);
            }
            
            return LowPolyMeshBuilder.Build($"{source.name}_Mirrored");
        }
        
        private static Vector3 MirrorPoint(Vector3 point, Vector3 planeNormal, float planeDistance)
        {
            float distToPlane = Vector3.Dot(point, planeNormal.normalized) - planeDistance;
            return point - planeNormal.normalized * distToPlane * 2f;
        }
        
        /// <summary>
        /// Flatten mesh along axis
        /// </summary>
        public static void ApplyFlatten(Mesh mesh, Vector3 axis, float flattenAmount = 1f)
        {
            Vector3[] vertices = mesh.vertices;
            Vector3 direction = axis.normalized;
            
            for (int i = 0; i < vertices.Length; i++)
            {
                float projection = Vector3.Dot(vertices[i], direction);
                vertices[i] -= direction * projection * flattenAmount;
            }
            
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
        }
    }
}
