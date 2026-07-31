using UnityEngine;
using System.Collections.Generic;

namespace FrontierProject.MeshGen.Pipeline
{
    /// <summary>
    /// Runtime mesh pipeline for procedural mesh generation and processing.
    /// Handles mesh creation, modification, and optimization at runtime.
    /// </summary>
    public class RuntimeMeshPipeline
    {
        private Queue<MeshOperation> operationQueue = new Queue<MeshOperation>();
        private bool isProcessing = false;
        
        public int maxOperationsPerFrame = 5;
        public bool enableAsyncProcessing = true;

        public delegate void MeshReadyHandler(Mesh mesh);
        public event MeshReadyHandler OnMeshReady;

        public void QueueOperation(MeshOperation operation)
        {
            operationQueue.Enqueue(operation);
            
            if (!isProcessing)
            {
                ProcessQueue();
            }
        }

        private async void ProcessQueue()
        {
            isProcessing = true;

            while (operationQueue.Count > 0)
            {
                int operationsThisFrame = 0;
                
                while (operationQueue.Count > 0 && operationsThisFrame < maxOperationsPerFrame)
                {
                    MeshOperation op = operationQueue.Dequeue();
                    
                    if (enableAsyncProcessing)
                    {
                        await System.Threading.Tasks.Task.Run(() => op.Execute());
                    }
                    else
                    {
                        op.Execute();
                    }
                    
                    operationsThisFrame++;
                    
                    if (op.ResultMesh != null)
                    {
                        OnMeshReady?.Invoke(op.ResultMesh);
                    }
                }

                if (operationQueue.Count > 0)
                {
                    await System.Threading.Tasks.Task.Yield();
                }
            }

            isProcessing = false;
        }

        public void ClearQueue()
        {
            operationQueue.Clear();
            isProcessing = false;
        }

        public static Mesh GenerateProceduralMesh(MeshParameters parameters)
        {
            Mesh mesh = new Mesh();
            mesh.name = parameters.meshName;

            switch (parameters.meshType)
            {
                case MeshType.Plane:
                    GeneratePlane(mesh, parameters.width, parameters.height, parameters.segments);
                    break;
                case MeshType.Sphere:
                    GenerateSphere(mesh, parameters.radius, parameters.segments);
                    break;
                case MeshType.Cylinder:
                    GenerateCylinder(mesh, parameters.radius, parameters.height, parameters.segments);
                    break;
                case MeshType.Terrain:
                    GenerateTerrain(mesh, parameters.width, parameters.height, parameters.segments, parameters.noiseSettings);
                    break;
            }

            return mesh;
        }

        private static void GeneratePlane(Mesh mesh, float width, float height, int segments)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uv = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();

            float halfWidth = width / 2f;
            float halfHeight = height / 2f;
            float segmentSizeX = width / segments;
            float segmentSizeY = height / segments;

            for (int y = 0; y <= segments; y++)
            {
                for (int x = 0; x <= segments; x++)
                {
                    vertices.Add(new Vector3(
                        -halfWidth + x * segmentSizeX,
                        0,
                        -halfHeight + y * segmentSizeY
                    ));

                    uv.Add(new Vector2((float)x / segments, (float)y / segments));
                    normals.Add(Vector3.up);
                }
            }

            for (int y = 0; y < segments; y++)
            {
                for (int x = 0; x < segments; x++)
                {
                    int current = y * (segments + 1) + x;
                    int next = current + 1;
                    int below = current + (segments + 1);
                    int belowNext = below + 1;

                    triangles.Add(current);
                    triangles.Add(below);
                    triangles.Add(next);

                    triangles.Add(next);
                    triangles.Add(below);
                    triangles.Add(belowNext);
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uv.ToArray();
            mesh.normals = normals.ToArray();
        }

        private static void GenerateSphere(Mesh mesh, float radius, int segments)
        {
            // Placeholder for sphere generation
            Debug.Log("[RuntimeMeshPipeline] Generating sphere with radius " + radius);
        }

        private static void GenerateCylinder(Mesh mesh, float radius, float height, int segments)
        {
            // Placeholder for cylinder generation
            Debug.Log("[RuntimeMeshPipeline] Generating cylinder");
        }

        private static void GenerateTerrain(Mesh mesh, float width, float height, int segments, NoiseSettings noise)
        {
            // Placeholder for terrain generation
            Debug.Log("[RuntimeMeshPipeline] Generating terrain");
        }
    }

    [System.Serializable]
    public class MeshOperation
    {
        public string name;
        public Mesh inputMesh;
        public MeshParameters parameters;
        public Mesh ResultMesh { get; private set; }

        public void Execute()
        {
            ResultMesh = RuntimeMeshPipeline.GenerateProceduralMesh(parameters);
        }
    }

    [System.Serializable]
    public class MeshParameters
    {
        public string meshName = "ProceduralMesh";
        public MeshType meshType = MeshType.Plane;
        public float width = 10f;
        public float height = 10f;
        public float radius = 1f;
        public int segments = 10;
        public NoiseSettings noiseSettings;
    }

    public enum MeshType
    {
        Plane,
        Sphere,
        Cylinder,
        Terrain
    }

    [System.Serializable]
    public class NoiseSettings
    {
        public float scale = 1f;
        public float amplitude = 1f;
        public int octaves = 4;
        public float persistence = 0.5f;
        public float lacunarity = 2f;
        public int seed = 0;
    }
}
