using UnityEngine;
using Frontier.MeshGen;

namespace Frontier.MeshGen.Items
{
    /// <summary>
    /// Generates low-poly armor piece meshes for 8 armor slots (light/heavy variants)
    /// </summary>
    public static class ArmorPieceGen
    {
        public static Mesh GenerateHelmetLight(int seed, int factionId)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Basic cap shape
            PrimitiveShapes.AddIcosphere(builder, Vector3.zero, 0.12f, 2);
            
            // Visor ridge
            PrimitiveShapes.AddBox(builder, new Vector3(0, -0.05f, 0.08f), 0.08f, 0.02f, 0.015f);
            
            // Side vents
            PrimitiveShapes.AddBox(builder, new Vector3(-0.1f, 0, 0), 0.01f, 0.04f, 0.03f);
            PrimitiveShapes.AddBox(builder, new Vector3(0.1f, 0, 0), 0.01f, 0.04f, 0.03f);
            
            return builder.BuildFlat("HelmetLight");
        }
        
        public static Mesh GenerateHelmetHeavy(int seed, int factionId)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Full helmet base
            PrimitiveShapes.AddIcosphere(builder, Vector3.zero, 0.13f, 3);
            
            // Reinforced visor plate
            PrimitiveShapes.AddBox(builder, new Vector3(0, -0.03f, 0.09f), 0.09f, 0.04f, 0.02f);
            
            // Neck guard
            PrimitiveShapes.AddBox(builder, new Vector3(0, -0.12f, -0.05f), 0.1f, 0.03f, 0.06f);
            
            // Top ridge reinforcement
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0.13f, 0), 0.02f, 0.02f, 0.12f);
            
            return builder.BuildFlat("HelmetHeavy");
        }
        
        public static Mesh GenerateChestPlateLight(int seed, int factionId)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Torso base (curved box approximation)
            PrimitiveShapes.AddBox(builder, Vector3.zero, 0.18f, 0.25f, 0.12f);
            
            // Shoulder straps
            PrimitiveShapes.AddBox(builder, new Vector3(-0.15f, 0.1f, 0), 0.06f, 0.08f, 0.04f);
            PrimitiveShapes.AddBox(builder, new Vector3(0.15f, 0.1f, 0), 0.06f, 0.08f, 0.04f);
            
            // Chest pocket/detail
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0.05f, 0.07f), 0.08f, 0.06f, 0.02f);
            
            return builder.BuildFlat("ChestPlateLight");
        }
        
        public static Mesh GenerateChestPlateHeavy(int seed, int factionId)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Heavy torso armor
            PrimitiveShapes.AddBox(builder, Vector3.zero, 0.2f, 0.28f, 0.15f);
            
            // Reinforced shoulder pauldrons
            PrimitiveShapes.AddBox(builder, new Vector3(-0.18f, 0.12f, 0), 0.08f, 0.1f, 0.06f);
            PrimitiveShapes.AddBox(builder, new Vector3(0.18f, 0.12f, 0), 0.08f, 0.1f, 0.06f);
            
            // Abdominal plate
            PrimitiveShapes.AddBox(builder, new Vector3(0, -0.1f, 0.08f), 0.14f, 0.1f, 0.03f);
            
            // Spine protection
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0, -0.08f), 0.04f, 0.25f, 0.03f);
            
            return builder.BuildFlat("ChestPlateHeavy");
        }
        
        public static Mesh GenerateLegPlateLight(int seed, int factionId)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Thigh armor
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0.15f, 0), 0.1f, 0.15f, 0.08f);
            
            // Knee pad
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0, 0.05f), 0.09f, 0.08f, 0.04f);
            
            // Shin guard
            PrimitiveShapes.AddBox(builder, new Vector3(0, -0.15f, 0.03f), 0.08f, 0.12f, 0.03f);
            
            return builder.BuildFlat("LegPlateLight");
        }
        
        public static Mesh GenerateLegPlateHeavy(int seed, int factionId)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Heavy thigh armor with side plates
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0.18f, 0), 0.12f, 0.18f, 0.1f);
            PrimitiveShapes.AddBox(builder, new Vector3(-0.08f, 0.18f, 0), 0.03f, 0.15f, 0.04f);
            PrimitiveShapes.AddBox(builder, new Vector3(0.08f, 0.18f, 0), 0.03f, 0.15f, 0.04f);
            
            // Reinforced knee
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0, 0.07f), 0.1f, 0.1f, 0.06f);
            
            // Full shin protection
            PrimitiveShapes.AddBox(builder, new Vector3(0, -0.18f, 0.05f), 0.1f, 0.15f, 0.05f);
            
            // Ankle guards
            PrimitiveShapes.AddBox(builder, new Vector3(0, -0.26f, 0.03f), 0.09f, 0.04f, 0.04f);
            
            return builder.BuildFlat("LegPlateHeavy");
        }
        
        public static Mesh GenerateBackpackLight(int seed)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Main pack body
            PrimitiveShapes.AddBox(builder, Vector3.zero, 0.15f, 0.2f, 0.08f);
            
            // Shoulder straps
            PrimitiveShapes.AddBox(builder, new Vector3(-0.1f, 0.08f, 0), 0.04f, 0.08f, 0.03f);
            PrimitiveShapes.AddBox(builder, new Vector3(0.1f, 0.08f, 0), 0.04f, 0.08f, 0.03f);
            
            // Waist strap
            PrimitiveShapes.AddBox(builder, new Vector3(0, -0.12f, 0), 0.18f, 0.03f, 0.02f);
            
            return builder.BuildFlat("BackpackLight");
        }
        
        public static Mesh GenerateBackpackHeavy(int seed)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Large frame pack
            PrimitiveShapes.AddBox(builder, Vector3.zero, 0.18f, 0.25f, 0.12f);
            
            // External frame bars
            PrimitiveShapes.AddBox(builder, new Vector3(-0.1f, 0, -0.07f), 0.02f, 0.25f, 0.02f);
            PrimitiveShapes.AddBox(builder, new Vector3(0.1f, 0, -0.07f), 0.02f, 0.25f, 0.02f);
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0.13f, -0.07f), 0.14f, 0.02f, 0.02f);
            PrimitiveShapes.AddBox(builder, new Vector3(0, -0.13f, -0.07f), 0.14f, 0.02f, 0.02f);
            
            // Side pouches
            PrimitiveShapes.AddBox(builder, new Vector3(-0.12f, 0, 0), 0.04f, 0.12f, 0.06f);
            PrimitiveShapes.AddBox(builder, new Vector3(0.12f, 0, 0), 0.04f, 0.12f, 0.06f);
            
            // Top handle
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0.14f, 0), 0.08f, 0.02f, 0.03f);
            
            return builder.BuildFlat("BackpackHeavy");
        }
    }
}
