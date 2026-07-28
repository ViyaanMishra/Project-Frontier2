using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Frontier.WorldGen
{
    /// <summary>
    /// Voronoi-based mesh partitioning for destruction.
    /// Splits meshes into convex cells for realistic fracture patterns.
    /// </summary>
    public static class VoronoiFracture
    {
        public struct VoronoiCell
        {
            public int cellId;
            public Vector3 centroid;
            public NativeArray<Vector3> vertices;
            public NativeArray<int> triangles;
            public float volume;
            public float mass;
        }

        public struct FractureResult
        {
            public NativeArray<VoronoiCell> cells;
            public int originalMeshId;
            public Vector3 impactPoint;
            public Vector3 impactForce;
        }

        public static FractureResult FractureMesh(Mesh mesh, int cellCount, Vector3 impactPoint, Vector3 force)
        {
            var result = new FractureResult
            {
                originalMeshId = mesh.GetInstanceID(),
                impactPoint = impactPoint,
                impactForce = force,
                cells = new NativeArray<VoronoiCell>(cellCount, Allocator.Persistent)
            };

            Vector3[] originalVerts = mesh.vertices;
            int[] originalTris = mesh.triangles;

            // Generate Voronoi seed points
            Vector3[] seeds = new Vector3[cellCount];
            System.Random rand = new System.Random(mesh.GetInstanceID());
            
            Bounds bounds = mesh.bounds;
            for (int i = 0; i < cellCount; i++)
            {
                seeds[i] = new Vector3(
                    Mathf.Lerp(bounds.min.x, bounds.max.x, (float)rand.NextDouble()),
                    Mathf.Lerp(bounds.min.y, bounds.max.y, (float)rand.NextDouble()),
                    Mathf.Lerp(bounds.min.z, bounds.max.z, (float)rand.NextDouble())
                );
            }

            // Assign vertices to nearest seed
            NativeArray<int> vertexOwnership = new NativeArray<int>(originalVerts.Length, Allocator.Temp);
            for (int v = 0; v < originalVerts.Length; v++)
            {
                int nearestSeed = 0;
                float minDist = float.MaxValue;
                
                for (int s = 0; s < cellCount; s++)
                {
                    float dist = math.distance(originalVerts[v], seeds[s]);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearestSeed = s;
                    }
                }
                vertexOwnership[v] = nearestSeed;
            }

            // Build cells
            for (int c = 0; c < cellCount; c++)
            {
                var cellVerts = new NativeList<Vector3>(Allocator.Temp);
                var cellTris = new NativeList<int>(Allocator.Temp);
                int vertOffset = 0;

                // Collect owned vertices
                for (int v = 0; v < originalVerts.Length; v++)
                {
                    if (vertexOwnership[v] == c)
                    {
                        cellVerts.Add(originalVerts[v]);
                    }
                }

                // Collect owned triangles
                for (int t = 0; t < originalTris.Length; t += 3)
                {
                    if (vertexOwnership[originalTris[t]] == c &&
                        vertexOwnership[originalTris[t + 1]] == c &&
                        vertexOwnership[originalTris[t + 2]] == c)
                    {
                        cellTris.Add(originalTris[t] - vertOffset);
                        cellTris.Add(originalTris[t + 1] - vertOffset);
                        cellTris.Add(originalTris[t + 2] - vertOffset);
                    }
                }

                // Calculate centroid
                Vector3 centroid = Vector3.zero;
                for (int i = 0; i < cellVerts.Length; i++)
                {
                    centroid += cellVerts[i];
                }
                centroid /= math.max(1, cellVerts.Length);

                // Calculate approximate volume and mass
                float volume = cellVerts.Length * 0.01f; // Simplified
                float mass = volume * 2.5f; // Density assumption

                var cell = new VoronoiCell
                {
                    cellId = c,
                    centroid = centroid,
                    vertices = cellVerts.AsArray().Reinterpret<Vector3>(4).Copy(),
                    triangles = cellTris.AsArray().Copy(),
                    volume = volume,
                    mass = mass
                };

                result.cells[c] = cell;
            }

            vertexOwnership.Dispose();
            return result;
        }

        public static void ApplyExplosionForce(ref FractureResult fracture, float explosionRadius, float explosionForce)
        {
            for (int i = 0; i < fracture.cells.Length; i++)
            {
                ref var cell = ref fracture.cells[i];
                float dist = math.distance(cell.centroid, fracture.impactPoint);
                
                if (dist < explosionRadius)
                {
                    float falloff = 1f - (dist / explosionRadius);
                    Vector3 dir = (cell.centroid - fracture.impactPoint).normalized;
                    // Force would be applied to rigidbody in Unity physics
                }
            }
        }
    }
}
