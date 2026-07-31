using UnityEngine;
using System.Collections.Generic;

namespace FrontierProject.MeshGen.Pipeline
{
    /// <summary>
    /// Mesh data pipeline for streaming and managing mesh data.
    /// Handles vertex buffers, index buffers, and memory optimization.
    /// </summary>
    public class MeshDataPipeline
    {
        private List<MeshChunk> activeChunks = new List<MeshChunk>();
        private Queue<MeshChunk> chunkPool = new Queue<MeshChunk>();
        
        public int maxActiveChunks = 100;
        public int verticesPerChunk = 65534; // Max for 16-bit indices
        public bool enablePooling = true;

        public MeshChunk RequestChunk()
        {
            MeshChunk chunk;
            
            if (enablePooling && chunkPool.Count > 0)
            {
                chunk = chunkPool.Dequeue();
                chunk.Reset();
            }
            else
            {
                chunk = new MeshChunk(verticesPerChunk);
            }

            if (activeChunks.Count >= maxActiveChunks)
            {
                ReleaseOldestChunk();
            }

            activeChunks.Add(chunk);
            return chunk;
        }

        public void ReleaseChunk(MeshChunk chunk)
        {
            activeChunks.Remove(chunk);
            
            if (enablePooling)
            {
                chunkPool.Enqueue(chunk);
            }
        }

        private void ReleaseOldestChunk()
        {
            if (activeChunks.Count > 0)
            {
                MeshChunk oldest = activeChunks[0];
                ReleaseChunk(oldest);
            }
        }

        public void ClearAll()
        {
            foreach (MeshChunk chunk in activeChunks)
            {
                chunk.Dispose();
            }
            activeChunks.Clear();
            
            foreach (MeshChunk chunk in chunkPool)
            {
                chunk.Dispose();
            }
            chunkPool.Clear();
        }

        public Mesh CombineChunks(List<MeshChunk> chunks, string meshName)
        {
            if (chunks == null || chunks.Count == 0) return null;

            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();
            List<Vector2> uv = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();

            int vertexOffset = 0;

            foreach (MeshChunk chunk in chunks)
            {
                Vector3[] chunkVerts = chunk.GetVertices();
                int[] chunkIndices = chunk.GetIndices();
                Vector2[] chunkUV = chunk.GetUV();
                Vector3[] chunkNormals = chunk.GetNormals();

                for (int i = 0; i < chunkVerts.Length; i++)
                {
                    vertices.Add(chunkVerts[i]);
                    
                    if (i < chunkUV.Length)
                        uv.Add(chunkUV[i]);
                    
                    if (i < chunkNormals.Length)
                        normals.Add(chunkNormals[i]);
                }

                for (int i = 0; i < chunkIndices.Length; i++)
                {
                    indices.Add(chunkIndices[i] + vertexOffset);
                }

                vertexOffset += chunkVerts.Length;
            }

            Mesh combined = new Mesh();
            combined.name = meshName;
            combined.vertices = vertices.ToArray();
            combined.triangles = indices.ToArray();
            
            if (uv.Count == vertices.Count)
                combined.uv = uv.ToArray();
            
            if (normals.Count == vertices.Count)
                combined.normals = normals.ToArray();

            combined.RecalculateBounds();
            combined.RecalculateTangents();

            return combined;
        }
    }

    public class MeshChunk
    {
        private List<Vector3> vertices;
        private List<int> indices;
        private List<Vector2> uv;
        private List<Vector3> normals;
        private int maxVertices;

        public int VertexCount => vertices?.Count ?? 0;
        public int IndexCount => indices?.Count ?? 0;
        public bool IsFull => VertexCount >= maxVertices;

        public MeshChunk(int maxVertexCount)
        {
            maxVertices = maxVertexCount;
            vertices = new List<Vector3>(maxVertexCount);
            indices = new List<int>(maxVertexCount * 3);
            uv = new List<Vector2>(maxVertexCount);
            normals = new List<Vector3>(maxVertexCount);
        }

        public int AddVertex(Vector3 vertex)
        {
            if (IsFull) return -1;
            
            vertices.Add(vertex);
            return VertexCount - 1;
        }

        public int AddTriangle(int v0, int v1, int v2)
        {
            if (IndexCount + 3 > indices.Capacity) return -1;
            
            indices.Add(v0);
            indices.Add(v1);
            indices.Add(v2);
            return IndexCount - 3;
        }

        public void SetUV(int vertexIndex, Vector2 uvCoord)
        {
            while (uv.Count <= vertexIndex)
            {
                uv.Add(Vector2.zero);
            }
            uv[vertexIndex] = uvCoord;
        }

        public void SetNormal(int vertexIndex, Vector3 normal)
        {
            while (normals.Count <= vertexIndex)
            {
                normals.Add(Vector3.up);
            }
            normals[vertexIndex] = normal;
        }

        public Vector3[] GetVertices()
        {
            return vertices?.ToArray() ?? new Vector3[0];
        }

        public int[] GetIndices()
        {
            return indices?.ToArray() ?? new int[0];
        }

        public Vector2[] GetUV()
        {
            return uv?.ToArray() ?? new Vector2[0];
        }

        public Vector3[] GetNormals()
        {
            return normals?.ToArray() ?? new Vector3[0];
        }

        public void Reset()
        {
            vertices?.Clear();
            indices?.Clear();
            uv?.Clear();
            normals?.Clear();
        }

        public void Dispose()
        {
            vertices = null;
            indices = null;
            uv = null;
            normals = null;
        }
    }
}
