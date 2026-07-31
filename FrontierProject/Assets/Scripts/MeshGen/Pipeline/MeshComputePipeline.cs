using UnityEngine;
using System.Collections.Generic;

namespace FrontierProject.MeshGen.Pipeline
{
    /// <summary>
    /// GPU compute pipeline for mesh processing operations.
    /// Handles compute shader dispatch for vertex transformations and mesh modifications.
    /// </summary>
    public class MeshComputePipeline
    {
        private ComputeShader computeShader;
        private int kernelHandle;
        private bool isInitialized = false;

        // Buffer references
        private ComputeBuffer vertexBuffer;
        private ComputeBuffer indexBuffer;
        private ComputeBuffer transformBuffer;

        public int maxVertices = 65534;
        public bool enableGPUAcceleration = true;

        public void Initialize(ComputeShader shader, string kernelName = "ProcessVertices")
        {
            computeShader = shader;
            
            if (computeShader != null)
            {
                kernelHandle = computeShader.FindKernel(kernelName);
                isInitialized = true;
                Debug.Log("[MeshComputePipeline] Initialized with kernel: " + kernelName);
            }
            else
            {
                Debug.LogWarning("[MeshComputePipeline] No compute shader provided, using CPU fallback");
                isInitialized = false;
            }
        }

        public void ProcessVertices(Mesh mesh, TransformData transforms)
        {
            if (!enableGPUAcceleration || !isInitialized)
            {
                ProcessVerticesCPU(mesh, transforms);
                return;
            }

            Vector3[] vertices = mesh.vertices;
            int vertexCount = vertices.Length;

            // Create compute buffers
            vertexBuffer = new ComputeBuffer(vertexCount, sizeof(float) * 3);
            transformBuffer = new ComputeBuffer(1, sizeof(float) * 16);

            // Upload data to GPU
            vertexBuffer.SetData(vertices);
            transformBuffer.SetData(transforms.ToFloatArray());

            // Set compute shader parameters
            computeShader.SetBuffer(kernelHandle, "vertices", vertexBuffer);
            computeShader.SetBuffer(kernelHandle, "transforms", transformBuffer);
            computeShader.SetInt("vertexCount", vertexCount);
            computeShader.SetFloat("deltaTime", Time.deltaTime);

            // Dispatch compute shader
            int threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
            computeShader.GetKernelThreadGroupSizes(kernelHandle, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
            int numGroups = Mathf.CeilToInt(vertexCount / (float)threadGroupSizeX);
            
            computeShader.Dispatch(kernelHandle, numGroups, 1, 1);

            // Download results
            vertexBuffer.GetData(vertices);
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            // Cleanup
            ReleaseBuffers();
        }

        private void ProcessVerticesCPU(Mesh mesh, TransformData transforms)
        {
            Vector3[] vertices = mesh.vertices;
            
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = transforms.Apply(vertices[i]);
            }

            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            Debug.Log("[MeshComputePipeline] Processed " + vertices.Length + " vertices on CPU");
        }

        public void MorphMesh(Mesh source, Mesh target, float blendFactor)
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[MeshComputePipeline] Not initialized, using CPU morph");
                MorphMeshCPU(source, target, blendFactor);
                return;
            }

            Vector3[] sourceVerts = source.vertices;
            Vector3[] targetVerts = target.vertices;
            int vertexCount = Mathf.Min(sourceVerts.Length, targetVerts.Length);

            vertexBuffer = new ComputeBuffer(vertexCount, sizeof(float) * 3);
            
            // Upload source vertices
            vertexBuffer.SetData(sourceVerts);

            // Set morph parameters
            computeShader.SetBuffer(kernelHandle, "vertices", vertexBuffer);
            computeShader.SetInt("vertexCount", vertexCount);
            computeShader.SetFloat("blendFactor", blendFactor);

            // Need target vertices buffer - simplified for this example
            Vector3[] targetData = new Vector3[vertexCount];
            System.Array.Copy(targetVerts, targetData, vertexCount);
            
            ComputeBuffer targetBuf = new ComputeBuffer(vertexCount, sizeof(float) * 3);
            targetBuf.SetData(targetData);
            computeShader.SetBuffer(kernelHandle, "targetVertices", targetBuf);

            // Dispatch
            int threadGroupSizeX;
            computeShader.GetKernelThreadGroupSizes(kernelHandle, out threadGroupSizeX, out _, out _);
            int numGroups = Mathf.CeilToInt(vertexCount / (float)threadGroupSizeX);
            computeShader.Dispatch(kernelHandle, numGroups, 1, 1);

            // Download
            vertexBuffer.GetData(sourceVerts);
            source.vertices = sourceVerts;
            source.RecalculateNormals();

            targetBuf.Release();
            ReleaseBuffers();
        }

        private void MorphMeshCPU(Mesh source, Mesh target, float blendFactor)
        {
            Vector3[] sourceVerts = source.vertices;
            Vector3[] targetVerts = target.vertices;
            int vertexCount = Mathf.Min(sourceVerts.Length, targetVerts.Length);

            for (int i = 0; i < vertexCount; i++)
            {
                sourceVerts[i] = Vector3.Lerp(sourceVerts[i], targetVerts[i], blendFactor);
            }

            source.vertices = sourceVerts;
            source.RecalculateNormals();
        }

        public void TessellateMesh(Mesh mesh, int subdivisionLevel)
        {
            Debug.Log("[MeshComputePipeline] Tessellation requested at level " + subdivisionLevel);
            // Placeholder for tessellation implementation
        }

        private void ReleaseBuffers()
        {
            vertexBuffer?.Release();
            indexBuffer?.Release();
            transformBuffer?.Release();
            
            vertexBuffer = null;
            indexBuffer = null;
            transformBuffer = null;
        }

        public void Dispose()
        {
            ReleaseBuffers();
            isInitialized = false;
        }
    }

    [System.Serializable]
    public class TransformData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public float time;

        public Vector3 Apply(Vector3 vertex)
        {
            Vector3 result = Vector3.Scale(vertex, scale);
            result = rotation * result;
            result += position;
            return result;
        }

        public float[] ToFloatArray()
        {
            Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, scale);
            float[] result = new float[16];
            
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    result[i * 4 + j] = matrix[i, j];
                }
            }
            
            return result;
        }
    }
}
