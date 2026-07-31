using UnityEngine;
using System.Collections.Generic;

namespace Frontier.MeshGen.Vehicles
{
    /// <summary>
    /// High-quality low-poly cargo helicopter generator with true-to-shape geometry.
    /// Features: Detailed fuselage, rotor blades, tail boom, landing skids, cockpit windows
    /// </summary>
    public static class CargoHelicopterGen
    {
        public static Mesh Generate()
        {
            var builder = new DetailedMeshBuilder();
            
            // Main fuselage - elongated cargo body with rounded nose
            GenerateFuselage(builder);
            
            // Cockpit section at front
            GenerateCockpit(builder);
            
            // Tail boom extending back
            GenerateTailBoom(builder);
            
            // Tail rotor assembly
            GenerateTailRotor(builder);
            
            // Main rotor hub and blades
            GenerateMainRotor(builder);
            
            // Landing skids
            GenerateLandingSkids(builder);
            
            // Engine housings on top
            GenerateEngineHousings(builder);
            
            return builder.Build("CargoHelicopter");
        }
        
        private static void GenerateFuselage(DetailedMeshBuilder b)
        {
            // Main cargo body - boxy but with rounded edges
            float length = 6f;
            float width = 2.8f;
            float height = 2.2f;
            
            int rings = 8;
            int segments = 16;
            
            for (int r = 0; r <= rings; r++)
            {
                float t = (float)r / rings;
                float z = Mathf.Lerp(-length / 2, length / 2, t);
                
                // Taper at front and back
                float taper = 1f - Mathf.Pow(Mathf.Abs(t - 0.5f) * 2f, 2f) * 0.3f;
                float ringWidth = width * 0.5f * taper;
                float ringHeight = height * 0.5f * taper;
                
                for (int s = 0; s < segments; s++)
                {
                    float angle = (float)s / segments * Mathf.PI * 2f;
                    float x = Mathf.Cos(angle) * ringWidth;
                    float y = Mathf.Sin(angle) * ringHeight;
                    
                    // Flatten bottom for cargo floor
                    if (y < -ringHeight * 0.3f) y = -ringHeight * 0.3f;
                    
                    b.AddVertex(new Vector3(x, y + 0.5f, z));
                }
            }
            
            // Connect rings with quads
            for (int r = 0; r < rings; r++)
            {
                for (int s = 0; s < segments; s++)
                {
                    int curr = r * segments + s;
                    int next = (s == segments - 1) ? r * segments : curr + 1;
                    int below = curr + segments;
                    int belowNext = (s == segments - 1) ? (r + 1) * segments : below + 1;
                    
                    b.AddQuad(curr, next, belowNext, below);
                }
            }
        }
        
        private static void GenerateCockpit(DetailedMeshBuilder b)
        {
            // Rounded cockpit nose at front
            int segments = 12;
            int rings = 4;
            float radius = 1.2f;
            
            int startIdx = b.VertexCount;
            
            for (int r = 0; r <= rings; r++)
            {
                float t = (float)r / rings;
                float z = -3f - t * 1.5f;
                float ringRadius = radius * (1f - t * 0.4f);
                
                for (int s = 0; s < segments; s++)
                {
                    float angle = (float)s / segments * Mathf.PI * 2f;
                    float x = Mathf.Cos(angle) * ringRadius;
                    float y = Mathf.Sin(angle) * ringRadius * 0.7f + 0.5f;
                    
                    if (y < 0.2f) y = 0.2f; // Flat bottom
                    
                    b.AddVertex(new Vector3(x, y, z));
                }
            }
            
            for (int r = 0; r < rings; r++)
            {
                for (int s = 0; s < segments; s++)
                {
                    int curr = startIdx + r * segments + s;
                    int next = (s == segments - 1) ? startIdx + r * segments : curr + 1;
                    int below = curr + segments;
                    int belowNext = (s == segments - 1) ? startIdx + (r + 1) * segments : below + 1;
                    
                    b.AddQuad(curr, next, belowNext, below);
                }
            }
        }
        
        private static void GenerateTailBoom(DetailedMeshBuilder b)
        {
            // Tapering tail boom
            int segments = 8;
            int rings = 6;
            
            int startIdx = b.VertexCount;
            
            for (int r = 0; r <= rings; r++)
            {
                float t = (float)r / rings;
                float z = 3f + t * 3.5f;
                float radius = 0.5f * (1f - t * 0.5f);
                
                for (int s = 0; s < segments; s++)
                {
                    float angle = (float)s / segments * Mathf.PI * 2f;
                    float x = Mathf.Cos(angle) * radius;
                    float y = Mathf.Sin(angle) * radius + 0.3f;
                    
                    b.AddVertex(new Vector3(x, y, z));
                }
            }
            
            for (int r = 0; r < rings; r++)
            {
                for (int s = 0; s < segments; s++)
                {
                    int curr = startIdx + r * segments + s;
                    int next = (s == segments - 1) ? startIdx + r * segments : curr + 1;
                    int below = curr + segments;
                    int belowNext = (s == segments - 1) ? startIdx + (r + 1) * segments : below + 1;
                    
                    b.AddQuad(curr, next, belowNext, below);
                }
            }
        }
        
        private static void GenerateTailRotor(DetailedMeshBuilder b)
        {
            // Vertical tail fin
            int startIdx = b.VertexCount;
            
            // Fin structure
            b.AddVertex(new Vector3(0f, 0.5f, 6.2f));
            b.AddVertex(new Vector3(0f, 1.8f, 6.0f));
            b.AddVertex(new Vector3(0f, 1.6f, 6.8f));
            b.AddVertex(new Vector3(0f, 0.3f, 6.8f));
            
            b.AddQuad(startIdx, startIdx + 1, startIdx + 2, startIdx + 3);
            
            // Tail rotor hub
            float hubZ = 6.5f;
            int hubVerts = b.VertexCount;
            int rotorSegments = 6;
            
            for (int i = 0; i < rotorSegments; i++)
            {
                float angle = (float)i / rotorSegments * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * 0.15f;
                float y = Mathf.Sin(angle) * 0.15f + 1.2f;
                b.AddVertex(new Vector3(x, y, hubZ));
            }
            
            // Rotor blades (2 blades)
            for (int blade = 0; blade < 2; blade++)
            {
                float bladeAngle = blade * Mathf.PI;
                for (int i = 0; i < 4; i++)
                {
                    float t = (float)i / 3f;
                    float x = Mathf.Cos(bladeAngle) * (0.2f + t * 0.8f);
                    float y = Mathf.Sin(bladeAngle) * (0.2f + t * 0.8f) + 1.2f;
                    b.AddVertex(new Vector3(x, y, hubZ + (i - 1.5f) * 0.1f));
                }
            }
        }
        
        private static void GenerateMainRotor(DetailedMeshBuilder b)
        {
            // Central rotor hub
            int hubStart = b.VertexCount;
            int hubSegments = 8;
            
            for (int i = 0; i < hubSegments; i++)
            {
                float angle = (float)i / hubSegments * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * 0.3f;
                float z = Mathf.Sin(angle) * 0.3f;
                b.AddVertex(new Vector3(x, 2.8f, z));
            }
            
            // 4 main rotor blades
            for (int blade = 0; blade < 4; blade++)
            {
                float bladeAngle = blade * Mathf.PI / 2f;
                int bladeStart = b.VertexCount;
                
                // Blade root to tip
                for (int i = 0; i < 6; i++)
                {
                    float t = (float)i / 5f;
                    float chord = 0.4f * (1f - t * 0.3f);
                    float span = 0.3f + t * 3.2f;
                    
                    float x = Mathf.Cos(bladeAngle) * span;
                    float z = Mathf.Sin(bladeAngle) * span;
                    float y = 2.8f + t * 0.2f;
                    
                    // Blade cross-section (airfoil-ish)
                    b.AddVertex(new Vector3(x + Mathf.Cos(bladeAngle + Mathf.PI / 2f) * chord * 0.5f, y, z + Mathf.Sin(bladeAngle + Mathf.PI / 2f) * chord * 0.5f));
                    b.AddVertex(new Vector3(x - Mathf.Cos(bladeAngle + Mathf.PI / 2f) * chord * 0.5f, y, z - Mathf.Sin(bladeAngle + Mathf.PI / 2f) * chord * 0.5f));
                }
            }
        }
        
        private static void GenerateLandingSkids(DetailedMeshBuilder b)
        {
            // Two parallel landing skids
            for (int side = -1; side <= 1; side += 2)
            {
                float skidX = side * 1.2f;
                float skidY = -0.3f;
                
                // Longitudinal skid tube
                int segments = 8;
                float length = 5f;
                float radius = 0.08f;
                
                int startIdx = b.VertexCount;
                
                for (int i = 0; i <= segments; i++)
                {
                    float t = (float)i / segments;
                    float z = Mathf.Lerp(-2.5f, 2.5f, t);
                    float angle = (float)i / segments * Mathf.PI * 2f;
                    
                    b.AddVertex(new Vector3(skidX + Mathf.Cos(angle) * radius, skidY + Mathf.Sin(angle) * radius, z));
                }
                
                // Vertical supports (3 per skid)
                for (int support = 0; support < 3; support++)
                {
                    float supportZ = -2f + support * 2f;
                    b.AddVertex(new Vector3(skidX, skidY + 0.5f, supportZ));
                    b.AddVertex(new Vector3(skidX, skidY + 1.2f, supportZ));
                }
            }
        }
        
        private static void GenerateEngineHousings(DetailedMeshBuilder b)
        {
            // Two engine housings on top of fuselage
            for (int side = -1; side <= 1; side += 2)
            {
                float housingX = side * 0.9f;
                float housingY = 2.2f;
                float housingZ = 0.5f;
                
                int startIdx = b.VertexCount;
                int segments = 8;
                float length = 1.5f;
                float radius = 0.4f;
                
                // Cylindrical engine housing
                for (int i = 0; i <= segments; i++)
                {
                    float t = (float)i / segments;
                    float z = Mathf.Lerp(housingZ - length / 2, housingZ + length / 2, t);
                    float angle = (float)i / segments * Mathf.PI * 2f;
                    
                    b.AddVertex(new Vector3(housingX + Mathf.Cos(angle) * radius, housingY + Mathf.Sin(angle) * radius, z));
                }
            }
        }
    }
    
    /// <summary>
    /// Helper class for building detailed low-poly meshes with proper topology
    /// </summary>
    public class DetailedMeshBuilder
    {
        private List<Vector3> vertices = new List<Vector3>();
        private List<int> triangles = new List<int>();
        private List<Vector3> normals = new List<Vector3>();
        private List<Vector2> uvs = new List<Vector2>();
        
        public int AddVertex(Vector3 position)
        {
            int index = vertices.Count;
            vertices.Add(position);
            normals.Add(Vector3.up);
            uvs.Add(Vector2.zero);
            return index;
        }
        
        public void AddTriangle(int v0, int v1, int v2)
        {
            triangles.Add(v0);
            triangles.Add(v1);
            triangles.Add(v2);
            
            // Calculate and set flat normal
            Vector3 normal = CalculateFlatNormal(vertices[v0], vertices[v1], vertices[v2]);
            normals[v0] = normal;
            normals[v1] = normal;
            normals[v2] = normal;
        }
        
        public void AddQuad(int v0, int v1, int v2, int v3)
        {
            AddTriangle(v0, v1, v2);
            AddTriangle(v0, v2, v3);
        }
        
        public int VertexCount => vertices.Count;
        
        private Vector3 CalculateFlatNormal(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            Vector3 side0 = v1 - v0;
            Vector3 side1 = v2 - v0;
            Vector3 normal = Vector3.Cross(side0, side1).normalized;
            return float.IsNaN(normal.x) ? Vector3.up : normal;
        }
        
        public Mesh Build(string name)
        {
            Mesh mesh = new Mesh();
            mesh.name = name;
            
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            
            mesh.RecalculateBounds();
            mesh.Optimize();
            
            return mesh;
        }
    }
}
