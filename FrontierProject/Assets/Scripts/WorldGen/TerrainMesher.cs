using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Frontier.WorldGen
{
    /// <summary>
    /// Runtime low-poly terrain mesh generator with vertex colors based on biome.
    /// </summary>
    public static class TerrainMesher
    {
        public static Mesh GenerateTerrainMesh(int chunkX, int chunkZ, float[,] heights, byte[,] biomes, int resolution = 32)
        {
            Mesh mesh = new Mesh();
            mesh.name = $"Terrain_{chunkX}_{chunkZ}";

            int verticesPerSide = resolution + 1;
            Vector3[] vertices = new Vector3[verticesPerSide * verticesPerSide];
            Vector3[] normals = new Vector3[verticesPerSide * verticesPerSide];
            Color[] colors = new Color[verticesPerSide * verticesPerSide];
            int[] triangles = new int[resolution * resolution * 6];

            float step = WorldGenerator.ChunkSize / (float)resolution;

            for (int z = 0; z < verticesPerSide; z++)
            {
                for (int x = 0; x < verticesPerSide; x++)
                {
                    int idx = z * verticesPerSide + x;
                    float worldX = chunkX * WorldGenerator.ChunkSize + x * step;
                    float worldZ = chunkZ * WorldGenerator.ChunkSize + z * step;
                    
                    vertices[idx] = new Vector3(worldX, heights[z, x], worldZ);
                    
                    BiomeType biome = (BiomeType)biomes[z % 32, x % 32];
                    colors[idx] = GetBiomeColor(biome);
                }
            }

            CalculateNormals(vertices, resolution + 1, resolution + 1, normals);

            int triIdx = 0;
            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int current = z * verticesPerSide + x;
                    int right = current + 1;
                    int bottom = current + verticesPerSide;
                    int bottomRight = bottom + 1;

                    triangles[triIdx++] = current;
                    triangles[triIdx++] = bottom;
                    triangles[triIdx++] = right;

                    triangles[triIdx++] = right;
                    triangles[triIdx++] = bottom;
                    triangles[triIdx++] = bottomRight;
                }
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();

            return mesh;
        }

        private static void CalculateNormals(Vector3[] vertices, int width, int height, Vector3[] normals)
        {
            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = z * width + x;
                    
                    Vector3 up = z > 0 ? vertices[(z - 1) * width + x] - vertices[idx] : Vector3.up;
                    Vector3 down = z < height - 1 ? vertices[(z + 1) * width + x] - vertices[idx] : Vector3.up;
                    Vector3 left = x > 0 ? vertices[z * width + (x - 1)] - vertices[idx] : Vector3.left;
                    Vector3 right = x < width - 1 ? vertices[z * width + (x + 1)] - vertices[idx] : Vector3.right;

                    Vector3 cross1 = Vector3.Cross(up, right);
                    Vector3 cross2 = Vector3.Cross(left, down);
                    
                    normals[idx] = (cross1 + cross2).normalized;
                }
            }
        }

        private static Color GetBiomeColor(BiomeType biome)
        {
            switch (biome)
            {
                case BiomeType.Plains: return new Color(0.7f, 0.8f, 0.4f);
                case BiomeType.Forest: return new Color(0.2f, 0.5f, 0.2f);
                case BiomeType.Desert: return new Color(0.9f, 0.85f, 0.6f);
                case BiomeType.Tundra: return new Color(0.8f, 0.8f, 0.85f);
                case BiomeType.Jungle: return new Color(0.1f, 0.4f, 0.1f);
                case BiomeType.Alpine: return new Color(0.95f, 0.95f, 0.95f);
                case BiomeType.Coastal: return new Color(0.9f, 0.85f, 0.7f);
                default: return Color.white;
            }
        }
    }
}
