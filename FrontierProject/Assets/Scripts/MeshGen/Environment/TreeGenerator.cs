using UnityEngine;

namespace Frontier.MeshGen.Environment
{
    /// <summary>
    /// Procedural tree generator with species variation by biome.
    /// Supports pine, oak, dead, palm, anomaly, birch, and redwood trees.
    /// </summary>
    public static class TreeGenerator
    {
        /// <summary>
        /// Generate a tree based on species type.
        /// </summary>
        public static Mesh GenerateTree(TreeSpecies species, int seed = 0, 
                                         float scale = 1f, string name = "Tree")
        {
            if (seed != 0)
                ProceduralRandom.SetSeed(seed);
            
            switch (species)
            {
                case TreeSpecies.Pine:
                    return GeneratePineTree(scale, name);
                case TreeSpecies.Oak:
                    return GenerateOakTree(scale, name);
                case TreeSpecies.Dead:
                    return GenerateDeadTree(scale, name);
                case TreeSpecies.Palm:
                    return GeneratePalmTree(scale, name);
                case TreeSpecies.Anomaly:
                    return GenerateAnomalyTree(scale, name);
                case TreeSpecies.Birch:
                    return GenerateBirchTree(scale, name);
                case TreeSpecies.Redwood:
                    return GenerateRedwoodTree(scale, name);
                default:
                    return GenerateOakTree(scale, name);
            }
        }
        
        /// <summary>
        /// Generate a pine/conifer tree using stacked cones.
        /// </summary>
        private static Mesh GeneratePineTree(float scale, string name)
        {
            var builder = new LowPolyMeshBuilder();
            var combiner = new MeshCombiner();
            
            float trunkHeight = 3f * scale;
            float trunkRadius = 0.15f * scale;
            
            // Trunk (cylinder)
            Mesh trunk = PrimitiveShapes.CreateCylinder(trunkRadius, trunkHeight, 6, true);
            trunk.name = "Trunk";
            combiner.AddSubMesh(trunk, Vector3.up * (trunkHeight * 0.5f), Quaternion.identity, 
                               new Color(0.4f, 0.25f, 0.15f));
            
            // Layered cone foliage
            int layers = 4;
            float layerSpacing = trunkHeight * 0.6f / layers;
            
            for (int i = 0; i < layers; i++)
            {
                float layerY = trunkHeight + (i - layers) * layerSpacing;
                float layerRadius = (layers - i) * 0.8f * scale;
                float layerHeight = layerSpacing * 1.5f;
                
                Mesh foliage = PrimitiveShapes.CreateCone(layerRadius, layerHeight, 8, true);
                foliage.name = $"Foliage_{i}";
                combiner.AddSubMesh(foliage, new Vector3(0, layerY, 0), Quaternion.identity,
                                   new Color(0.1f, 0.4f + i * 0.05f, 0.15f));
            }
            
            return combiner.Combine(name);
        }
        
        /// <summary>
        /// Generate an oak/deciduous tree with icosphere canopy clusters.
        /// </summary>
        private static Mesh GenerateOakTree(float scale, string name)
        {
            var builder = new LowPolyMeshBuilder();
            var combiner = new MeshCombiner();
            
            float trunkHeight = 2.5f * scale;
            float trunkRadius = 0.2f * scale;
            
            // Main trunk
            Mesh trunk = PrimitiveShapes.CreateCylinder(trunkRadius, trunkHeight, 6, true);
            trunk.name = "Trunk";
            combiner.AddSubMesh(trunk, Vector3.up * (trunkHeight * 0.5f), Quaternion.identity,
                               new Color(0.35f, 0.25f, 0.15f));
            
            // Branch clusters (multiple spheres)
            int clusterCount = 5;
            float canopyRadius = 1.5f * scale;
            
            Vector3[] clusterPositions = {
                Vector3.up * trunkHeight,
                new Vector3(-0.8f, trunkHeight * 0.9f, -0.5f) * scale,
                new Vector3(0.8f, trunkHeight * 0.85f, 0.5f) * scale,
                new Vector3(0.5f, trunkHeight * 0.95f, -0.8f) * scale,
                new Vector3(-0.5f, trunkHeight * 0.9f, 0.8f) * scale
            };
            
            foreach (var pos in clusterPositions)
            {
                float clusterScale = Random.Range(0.7f, 1.0f) * canopyRadius;
                Mesh cluster = PrimitiveShapes.CreateIcosphere(clusterScale, 1);
                cluster.name = "CanopyCluster";
                combiner.AddSubMesh(cluster, pos, Quaternion.Euler(Random.value * 360f, Random.value * 360f, Random.value * 360f),
                                   new Color(0.2f, 0.5f, 0.15f));
            }
            
            return combiner.Combine(name);
        }
        
        /// <summary>
        /// Generate a dead/bare tree with no leaves.
        /// </summary>
        private static Mesh GenerateDeadTree(float scale, string name)
        {
            var combiner = new MeshCombiner();
            
            float trunkHeight = 3f * scale;
            float trunkRadius = 0.18f * scale;
            
            // Main trunk (slightly tapered)
            Mesh trunk = PrimitiveShapes.CreateCylinder(trunkRadius, trunkHeight, 5, true);
            trunk.name = "DeadTrunk";
            combiner.AddSubMesh(trunk, Vector3.up * (trunkHeight * 0.5f), Quaternion.identity,
                               new Color(0.3f, 0.25f, 0.2f));
            
            // Bare branches
            int branchCount = 8;
            for (int i = 0; i < branchCount; i++)
            {
                float branchY = trunkHeight * (0.4f + Random.value * 0.5f);
                float branchLength = Random.Range(0.5f, 1.2f) * scale;
                float branchRadius = trunkRadius * Random.Range(0.3f, 0.5f);
                
                Vector3 branchDir = new Vector3(
                    Mathf.Cos(i * 45f * Mathf.Deg2Rad) * Random.Range(0.5f, 1f),
                    Random.Range(0.2f, 0.5f),
                    Mathf.Sin(i * 45f * Mathf.Deg2Rad) * Random.Range(0.5f, 1f)
                ).normalized;
                
                Mesh branch = PrimitiveShapes.CreateCylinder(branchRadius, branchLength, 4, true);
                branch.name = $"Branch_{i}";
                
                Quaternion rotation = Quaternion.LookRotation(branchDir, Vector3.up);
                Vector3 position = Vector3.up * branchY + branchDir * (branchLength * 0.3f);
                
                combiner.AddSubMesh(branch, position, rotation, new Color(0.35f, 0.3f, 0.25f));
            }
            
            return combiner.Combine(name);
        }
        
        /// <summary>
        /// Generate a palm tree with curved trunk and fronds.
        /// </summary>
        private static Mesh GeneratePalmTree(float scale, string name)
        {
            var combiner = new MeshCombiner();
            
            float trunkHeight = 4f * scale;
            float trunkRadius = 0.12f * scale;
            
            // Curved trunk (approximated with angled segments)
            int trunkSegments = 4;
            float segmentHeight = trunkHeight / trunkSegments;
            
            for (int i = 0; i < trunkSegments; i++)
            {
                float y = i * segmentHeight + segmentHeight * 0.5f;
                float angle = i * 5f * Mathf.Deg2Rad;
                
                Mesh segment = PrimitiveShapes.CreateCylinder(trunkRadius * (1 - i * 0.1f), segmentHeight, 6, true);
                segment.name = $"TrunkSegment_{i}";
                
                Vector3 position = new Vector3(Mathf.Sin(angle) * y * 0.3f, y, 0);
                Quaternion rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
                
                combiner.AddSubMesh(segment, position, rotation, new Color(0.5f, 0.4f, 0.3f));
            }
            
            // Fronds at top
            int frondCount = 8;
            float frondLength = 1.5f * scale;
            
            for (int i = 0; i < frondCount; i++)
            {
                float angle = i * (360f / frondCount) * Mathf.Deg2Rad;
                
                Mesh frond = CreateFrond(frondLength);
                frond.name = $"Frond_{i}";
                
                Vector3 position = Vector3.up * trunkHeight;
                Quaternion rotation = Quaternion.Euler(-60f, i * (360f / frondCount), 0);
                
                combiner.AddSubMesh(frond, position, rotation, new Color(0.15f, 0.5f, 0.2f));
            }
            
            return combiner.Combine(name);
        }
        
        /// <summary>
        /// Generate an anomaly/corrupted tree with twisted geometry and glow.
        /// </summary>
        private static Mesh GenerateAnomalyTree(float scale, string name)
        {
            var combiner = new MeshCombiner();
            
            float trunkHeight = 2.5f * scale;
            float trunkRadius = 0.2f * scale;
            
            // Twisted trunk
            Mesh trunk = PrimitiveShapes.CreateCylinder(trunkRadius, trunkHeight, 5, true);
            trunk.name = "AnomalyTrunk";
            combiner.AddSubMesh(trunk, Vector3.up * (trunkHeight * 0.5f), Quaternion.identity,
                               new Color(0.4f, 0.2f, 0.5f));
            
            // Floating shard clusters
            int shardCount = 6;
            for (int i = 0; i < shardCount; i++)
            {
                float shardY = Random.Range(trunkHeight * 0.3f, trunkHeight * 1.5f);
                Vector3 shardPos = new Vector3(
                    Random.Range(-1f, 1f) * scale,
                    shardY,
                    Random.Range(-1f, 1f) * scale
                );
                
                Mesh shard = PrimitiveShapes.CreateCrystal(0.3f * scale, 0.6f * scale, 4);
                shard.name = $"AnomalyShard_{i}";
                
                Quaternion rotation = Quaternion.Euler(Random.value * 360f, Random.value * 360f, Random.value * 360f);
                combiner.AddSubMesh(shard, shardPos, rotation, new Color(0.7f, 0.3f, 0.9f, 0.8f));
            }
            
            return combiner.Combine(name);
        }
        
        /// <summary>
        /// Generate a birch tree with thin white trunk.
        /// </summary>
        private static Mesh GenerateBirchTree(float scale, string name)
        {
            var combiner = new MeshCombiner();
            
            float trunkHeight = 4f * scale;
            float trunkRadius = 0.1f * scale;
            
            // Thin white trunk
            Mesh trunk = PrimitiveShapes.CreateCylinder(trunkRadius, trunkHeight, 6, true);
            trunk.name = "BirchTrunk";
            combiner.AddSubMesh(trunk, Vector3.up * (trunkHeight * 0.5f), Quaternion.identity,
                               new Color(0.85f, 0.8f, 0.75f));
            
            // Light foliage clusters
            int clusterCount = 4;
            for (int i = 0; i < clusterCount; i++)
            {
                float clusterY = trunkHeight * (0.7f + i * 0.15f);
                Vector3 clusterPos = new Vector3(
                    Random.Range(-0.5f, 0.5f) * scale,
                    clusterY,
                    Random.Range(-0.5f, 0.5f) * scale
                );
                
                Mesh cluster = PrimitiveShapes.CreateIcosphere(0.6f * scale, 1);
                cluster.name = $"BirchCluster_{i}";
                combiner.AddSubMesh(cluster, clusterPos, Quaternion.identity,
                                   new Color(0.6f, 0.75f, 0.4f));
            }
            
            return combiner.Combine(name);
        }
        
        /// <summary>
        /// Generate a massive redwood tree with buttress roots.
        /// </summary>
        private static Mesh GenerateRedwoodTree(float scale, string name)
        {
            var combiner = new MeshCombiner();
            
            float trunkHeight = 8f * scale;
            float trunkRadius = 0.5f * scale;
            
            // Massive trunk
            Mesh trunk = PrimitiveShapes.CreateCylinder(trunkRadius, trunkHeight, 8, true);
            trunk.name = "RedwoodTrunk";
            combiner.AddSubMesh(trunk, Vector3.up * (trunkHeight * 0.5f), Quaternion.identity,
                               new Color(0.5f, 0.3f, 0.2f));
            
            // Buttress roots
            int rootCount = 6;
            for (int i = 0; i < rootCount; i++)
            {
                float angle = i * (360f / rootCount) * Mathf.Deg2Rad;
                Vector3 rootPos = new Vector3(
                    Mathf.Cos(angle) * trunkRadius * 0.8f,
                    trunkHeight * 0.15f,
                    Mathf.Sin(angle) * trunkRadius * 0.8f
                );
                
                Mesh root = PrimitiveShapes.CreateWedge(trunkRadius * 0.5f, trunkHeight * 0.3f, trunkRadius);
                root.name = $"Root_{i}";
                
                Quaternion rotation = Quaternion.Euler(90f, i * (360f / rootCount), 0);
                combiner.AddSubMesh(root, rootPos, rotation, new Color(0.45f, 0.28f, 0.18f));
            }
            
            // Top foliage cluster
            Mesh foliage = PrimitiveShapes.CreateIcosphere(1.5f * scale, 2);
            foliage.name = "RedwoodFoliage";
            combiner.AddSubMesh(foliage, Vector3.up * (trunkHeight + 0.5f * scale), Quaternion.identity,
                               new Color(0.15f, 0.45f, 0.2f));
            
            return combiner.Combine(name);
        }
        
        /// <summary>
        /// Create a single palm frond mesh.
        /// </summary>
        private static Mesh CreateFrond(float length)
        {
            var builder = new LowPolyMeshBuilder();
            
            // Simple elongated quad with slight curve
            builder.AddQuad(
                new Vector3(-0.1f, 0, 0),
                new Vector3(0.1f, 0, 0),
                new Vector3(0.05f, length, -0.2f),
                new Vector3(-0.05f, length, -0.2f),
                Color.green, Color.green, Color.green, Color.green
            );
            
            return builder.Build("Frond");
        }
    }
    
    /// <summary>
    /// Tree species enumeration.
    /// </summary>
    public enum TreeSpecies
    {
        Pine,
        Oak,
        Dead,
        Palm,
        Anomaly,
        Birch,
        Redwood
    }
}
