using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace Frontier.Nav
{
    /// <summary>
    /// Wrapper for Unity NavMesh integration for Tier 1/2 local navigation.
    /// Handles dynamic obstacle addition and navmesh baking triggers.
    /// </summary>
    public class NavMeshWrapper : MonoBehaviour
    {
        [Header("NavMesh Settings")]
        [SerializeField] private float _agentRadius = 0.5f;
        [SerializeField] private float _agentHeight = 2.0f;
        [SerializeField] private float _maxSlope = 45f;
        [SerializeField] private float _stepHeight = 0.5f;

        [Header("Dynamic Obstacles")]
        [SerializeField] private List<GameObject> _dynamicObstacles = new List<GameObject>();
        
        private NavMeshData _navMeshData;
        private NavMeshDataInstance _navMeshInstance;
        private bool _isBaking = false;

        public float AgentRadius => _agentRadius;
        public float AgentHeight => _agentHeight;
        public bool IsReady { get; private set; }

        private void OnEnable()
        {
            BakeNavMesh();
        }

        private void OnDisable()
        {
            if (_navMeshInstance.valid)
                NavMesh.RemoveNavMeshData(_navMeshInstance);
        }

        /// <summary>
        /// Bake the navmesh from scene geometry.
        /// </summary>
        public void BakeNavMesh()
        {
            if (_isBaking) return;
            
            _isBaking = true;
            IsReady = false;

            // Create new navmesh data
            if (_navMeshData == null)
                _navMeshData = new NavMeshData();

            // Collect sources
            var sources = new List<NavMeshBuildSource>();
            
            // Add static geometry
            var staticGeometries = FindObjectsOfType<MeshFilter>(includeInactive: true);
            foreach (var mf in staticGeometries)
            {
                if (!mf.gameObject.isStatic || mf.gameObject.layer == LayerMask.NameToLayer("Ignore NavMesh"))
                    continue;

                var source = new NavMeshBuildSource
                {
                    shape = NavMeshBuildSourceShape.Mesh,
                    component = mf,
                    area = 0, // Walkable
                    transform = mf.transform.localToWorldMatrix
                };
                sources.Add(source);
            }

            // Add dynamic obstacles as non-walkable
            foreach (var obs in _dynamicObstacles)
            {
                if (obs == null) continue;

                var mf = obs.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    var source = new NavMeshBuildSource
                    {
                        shape = NavMeshBuildSourceShape.Mesh,
                        component = mf,
                        area = 1, // Not Walkable
                        transform = obs.transform.localToWorldMatrix
                    };
                    sources.Add(source);
                }
                else
                {
                    // Use bounding box for objects without mesh
                    var renderer = obs.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        var source = new NavMeshBuildSource
                        {
                            shape = NavMeshBuildSourceShape.Box,
                            size = renderer.bounds.size,
                            area = 1,
                            transform = Matrix4x4.TRS(renderer.bounds.center, Quaternion.identity, Vector3.one)
                        };
                        sources.Add(source);
                    }
                }
            }

            // Build settings
            var buildSettings = new NavMeshBuildSettings
            {
                agentRadius = _agentRadius,
                agentHeight = _agentHeight,
                maxSlope = _maxSlope,
                stepHeight = _stepHeight,
                minRegionArea = 0.5f,
                tileAgentClipSize = 0.5f
            };

            // Build async
            var buildTask = NavMeshBuilder.UpdateNavMeshDataAsync(
                _navMeshData, 
                buildSettings, 
                sources
            );

            // Wait for completion (in production would use coroutine)
            while (!buildTask.isDone)
            {
                System.Threading.Thread.Sleep(1);
            }

            // Install navmesh
            if (_navMeshInstance.valid)
                NavMesh.RemoveNavMeshData(_navMeshInstance);

            _navMeshInstance = NavMesh.AddNavMeshData(_navMeshData, transform.position, transform.rotation);
            
            _isBaking = false;
            IsReady = true;

            Debug.Log($"[NavMeshWrapper] NavMesh baked with {sources.Count} sources");
        }

        /// <summary>
        /// Add a dynamic obstacle at runtime.
        /// </summary>
        public void AddObstacle(GameObject obstacle)
        {
            if (!_dynamicObstacles.Contains(obstacle))
                _dynamicObstacles.Add(obstacle);
            
            // Trigger rebake (throttled in production)
            Invoke(nameof(BakeNavMesh), 0.1f);
        }

        /// <summary>
        /// Remove a dynamic obstacle.
        /// </summary>
        public void RemoveObstacle(GameObject obstacle)
        {
            if (_dynamicObstacles.Remove(obstacle))
                Invoke(nameof(BakeNavMesh), 0.1f);
        }

        /// <summary>
        /// Calculate a path using Unity's built-in pathfinding.
        /// </summary>
        public bool CalculatePath(Vector3 startPos, Vector3 endPos, out NavMeshPath path)
        {
            path = new NavMeshPath();
            
            if (!IsReady)
                return false;

            bool result = NavMesh.CalculatePath(startPos, endPos, NavMesh.AllAreas, path);
            
            if (result && path.corners.Length > 0)
            {
                Debug.Log($"[NavMeshWrapper] Path found with {path.corners.Length} corners");
            }
            
            return result;
        }

        /// <summary>
        /// Sample the navmesh to find nearest valid point.
        /// </summary>
        public bool SamplePosition(Vector3 position, out Vector3 sampledPosition, float maxDistance = 1f)
        {
            sampledPosition = Vector3.zero;
            
            if (!IsReady)
                return false;

            if (NavMesh.SamplePosition(position, out NavMeshHit hit, maxDistance, NavMesh.AllAreas))
            {
                sampledPosition = hit.position;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if a position is on the navmesh.
        /// </summary>
        public bool IsValidPosition(Vector3 position, float radius = 0.1f)
        {
            if (!IsReady)
                return false;

            return NavMesh.SamplePosition(position, out _, radius, NavMesh.AllAreas);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw agent capsule
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _agentRadius);
            
            // Draw height
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * _agentHeight);
        }
    }
}
