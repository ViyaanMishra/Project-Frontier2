using Unity.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace Frontier.WorldGen
{
    /// <summary>
    /// Manages runtime destruction of buildings and terrain.
    /// Handles debris physics, force inheritance, and cleanup.
    /// </summary>
    public static class DestructionManager
    {
        public struct DebrisPiece
        {
            public int id;
            public Mesh mesh;
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 velocity;
            public Vector3 angularVelocity;
            public float mass;
            public float lifetime;
            public bool isSalvageable;
            public int originalItemId;
        }

        private static readonly List<DebrisPiece> _activeDebris = new List<DebrisPiece>();
        private static int _debrisIdCounter = 0;
        private const float MaxDebrisCount = 500;
        private const float DebrisCleanupDistance = 100f;

        public static void Initialize()
        {
            _activeDebris.Clear();
            _debrisIdCounter = 0;
        }

        public static void ApplyDamage(GameObject target, Vector3 impactPoint, float damage, Vector3 force)
        {
            // Check if target has destructible components
            var health = target.GetComponent<BuildingHealth>();
            if (health != null)
            {
                health.TakeDamage(damage, impactPoint);
                
                if (health.CurrentHealth <= 0)
                {
                    DestroyBuilding(target, impactPoint, force);
                }
            }
        }

        public static void DestroyBuilding(GameObject building, Vector3 impactPoint, Vector3 force)
        {
            var meshFilter = building.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null) return;

            Mesh originalMesh = meshFilter.sharedMesh;
            
            // Fracture the mesh
            var fracture = VoronoiFracture.FractureMesh(originalMesh, 8, impactPoint, force);
            
            // Create debris pieces
            for (int i = 0; i < fracture.cells.Length; i++)
            {
                ref var cell = ref fracture.cells[i];
                
                if (cell.vertices.Length < 3) continue;

                var debris = new DebrisPiece
                {
                    id = _debrisIdCounter++,
                    mass = cell.mass,
                    position = cell.centroid,
                    rotation = Quaternion.identity,
                    velocity = force * (1f / cell.mass) * 0.1f,
                    angularVelocity = new Vector3(
                        Random.Range(-5f, 5f),
                        Random.Range(-5f, 5f),
                        Random.Range(-5f, 5f)
                    ),
                    lifetime = 300f, // 5 minutes
                    isSalvageable = true,
                    originalItemId = GetBuildingItemId(building)
                };

                // Create mesh for this cell
                Mesh cellMesh = new Mesh();
                cellMesh.vertices = cell.vertices.ToArray();
                cellMesh.triangles = cell.triangles.ToArray();
                cellMesh.RecalculateNormals();
                cellMesh.RecalculateBounds();
                debris.mesh = cellMesh;

                if (_activeDebris.Count < MaxDebrisCount)
                {
                    _activeDebris.Add(debris);
                }
            }

            // Clean up native arrays
            for (int i = 0; i < fracture.cells.Length; i++)
            {
                fracture.cells[i].vertices.Dispose();
                fracture.cells[i].triangles.Dispose();
            }
            fracture.cells.Dispose();

            // Remove original building
                Object.Destroy(building);
        }

        public static void UpdateDebris(float deltaTime)
        {
            for (int i = _activeDebris.Count - 1; i >= 0; i--)
            {
                var debris = _activeDebris[i];
                
                // Apply gravity
                debris.velocity.y -= 9.81f * deltaTime;
                
                // Update position
                debris.position += debris.velocity * deltaTime;
                debris.rotation *= Quaternion.Euler(debris.angularVelocity * deltaTime);
                
                // Ground collision
                if (debris.position.y < 0)
                {
                    debris.position.y = 0;
                    debris.velocity *= 0.5f; // Damping
                    debris.angularVelocity *= 0.8f;
                }
                
                // Lifetime countdown
                debris.lifetime -= deltaTime;
                
                if (debris.lifetime <= 0)
                {
                    // Cleanup debris
                    if (debris.mesh != null)
                    {
                        Object.Destroy(debris.mesh);
                    }
                    _activeDebris.RemoveAt(i);
                }
                else
                {
                    _activeDebris[i] = debris;
                }
            }
        }

        public static List<DebrisPiece> GetSalvageableDebrisInRange(Vector3 center, float radius)
        {
            var result = new List<DebrisPiece>();
            foreach (var debris in _activeDebris)
            {
                if (debris.isSalvageable && math.distance(debris.position, center) <= radius)
                {
                    result.Add(debris);
                }
            }
            return result;
        }

        public static void ClearDebris()
        {
            foreach (var debris in _activeDebris)
            {
                if (debris.mesh != null)
                {
                    Object.Destroy(debris.mesh);
                }
            }
            _activeDebris.Clear();
        }

        private static int GetBuildingItemId(GameObject building)
        {
            var buildingData = building.GetComponent<BuildingDataComponent>();
            return buildingData != null ? buildingData.itemId : -1;
        }
    }

    // Placeholder component interfaces
    public class BuildingHealth : MonoBehaviour
    {
        public float MaxHealth = 100f;
        public float CurrentHealth { get; set; }

        void Start() => CurrentHealth = MaxHealth;

        public void TakeDamage(float damage, Vector3 impactPoint)
        {
            CurrentHealth -= damage;
        }
    }

    public class BuildingDataComponent : MonoBehaviour
    {
        public int itemId;
    }
}
