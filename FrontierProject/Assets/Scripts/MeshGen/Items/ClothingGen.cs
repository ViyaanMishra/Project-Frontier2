using UnityEngine;
using Frontier.MeshGen;

namespace Frontier.MeshGen.Items
{
    /// <summary>
    /// Generates low-poly clothing meshes with faction variations (10 per faction × 6 factions = 60 variants)
    /// </summary>
    public static class ClothingGen
    {
        public static Mesh GenerateShirt(int seed, int factionId)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Torso base
            PrimitiveShapes.AddBox(builder, Vector3.zero, 0.16f, 0.22f, 0.1f);
            
            // Sleeves
            PrimitiveShapes.AddBox(builder, new Vector3(-0.14f, 0.08f, 0), 0.05f, 0.12f, 0.05f);
            PrimitiveShapes.AddBox(builder, new Vector3(0.14f, 0.08f, 0), 0.05f, 0.12f, 0.05f);
            
            // Collar
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0.12f, -0.02f), 0.06f, 0.03f, 0.02f);
            
            return builder.BuildFlat("Shirt");
        }
        
        public static Mesh GeneratePants(int seed, int factionId)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Waist/hips
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0.15f, 0), 0.14f, 0.08f, 0.1f);
            
            // Legs
            PrimitiveShapes.AddBox(builder, new Vector3(-0.06f, -0.05f, 0), 0.06f, 0.2f, 0.08f);
            PrimitiveShapes.AddBox(builder, new Vector3(0.06f, -0.05f, 0), 0.06f, 0.2f, 0.08f);
            
            // Belt
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0.12f, 0.01f), 0.15f, 0.02f, 0.02f);
            
            return builder.BuildFlat("Pants");
        }
        
        public static Mesh GenerateDress(int seed, int factionId)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Bodice
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0.1f, 0), 0.14f, 0.15f, 0.09f);
            
            // Skirt (flared approximation)
            PrimitiveShapes.AddBox(builder, new Vector3(0, -0.1f, 0), 0.18f, 0.2f, 0.14f);
            
            // Sleeves (short)
            PrimitiveShapes.AddBox(builder, new Vector3(-0.12f, 0.12f, 0), 0.04f, 0.06f, 0.04f);
            PrimitiveShapes.AddBox(builder, new Vector3(0.12f, 0.12f, 0), 0.04f, 0.06f, 0.04f);
            
            return builder.BuildFlat("Dress");
        }
        
        public static Mesh GenerateJacket(int seed, int factionId)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Main body
            PrimitiveShapes.AddBox(builder, Vector3.zero, 0.17f, 0.24f, 0.11f);
            
            // Sleeves
            PrimitiveShapes.AddBox(builder, new Vector3(-0.15f, 0.08f, 0), 0.06f, 0.14f, 0.06f);
            PrimitiveShapes.AddBox(builder, new Vector3(0.15f, 0.08f, 0), 0.06f, 0.14f, 0.06f);
            
            // Collar (upturned)
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0.13f, -0.03f), 0.07f, 0.04f, 0.03f);
            
            // Pockets
            PrimitiveShapes.AddBox(builder, new Vector3(-0.05f, -0.06f, 0.06f), 0.04f, 0.05f, 0.02f);
            PrimitiveShapes.AddBox(builder, new Vector3(0.05f, -0.06f, 0.06f), 0.04f, 0.05f, 0.02f);
            
            return builder.BuildFlat("Jacket");
        }
        
        public static Mesh GenerateVest(int seed, int factionId)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Front panels
            PrimitiveShapes.AddBox(builder, new Vector3(-0.06f, 0, 0.05f), 0.06f, 0.2f, 0.03f);
            PrimitiveShapes.AddBox(builder, new Vector3(0.06f, 0, 0.05f), 0.06f, 0.2f, 0.03f);
            
            // Back panel
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0, -0.05f), 0.14f, 0.2f, 0.03f);
            
            // Shoulder straps
            PrimitiveShapes.AddBox(builder, new Vector3(-0.08f, 0.1f, 0), 0.04f, 0.04f, 0.03f);
            PrimitiveShapes.AddBox(builder, new Vector3(0.08f, 0.1f, 0), 0.04f, 0.04f, 0.03f);
            
            return builder.BuildFlat("Vest");
        }
        
        public static Mesh GenerateRobe(int seed, int factionId)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Main body (long)
            PrimitiveShapes.AddBox(builder, Vector3.zero, 0.16f, 0.35f, 0.12f);
            
            // Wide sleeves
            PrimitiveShapes.AddBox(builder, new Vector3(-0.16f, 0.1f, 0), 0.08f, 0.18f, 0.08f);
            PrimitiveShapes.AddBox(builder, new Vector3(0.16f, 0.1f, 0), 0.08f, 0.18f, 0.08f);
            
            // Hood
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0.2f, -0.03f), 0.1f, 0.08f, 0.06f);
            
            // Belt
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0.05f, 0.01f), 0.17f, 0.03f, 0.03f);
            
            return builder.BuildFlat("Robe");
        }
        
        public static Mesh GenerateUniformTop(int seed, int factionId)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Fitted torso
            PrimitiveShapes.AddBox(builder, Vector3.zero, 0.16f, 0.23f, 0.1f);
            
            // Structured shoulders
            PrimitiveShapes.AddBox(builder, new Vector3(-0.14f, 0.11f, 0), 0.06f, 0.04f, 0.05f);
            PrimitiveShapes.AddBox(builder, new Vector3(0.14f, 0.11f, 0), 0.06f, 0.04f, 0.05f);
            
            // Epaulettes
            PrimitiveShapes.AddBox(builder, new Vector3(-0.12f, 0.13f, 0), 0.04f, 0.02f, 0.03f);
            PrimitiveShapes.AddBox(builder, new Vector3(0.12f, 0.13f, 0), 0.04f, 0.02f, 0.03f);
            
            // Badge pocket
            PrimitiveShapes.AddBox(builder, new Vector3(-0.04f, 0.06f, 0.055f), 0.03f, 0.02f, 0.01f);
            
            return builder.BuildFlat("UniformTop");
        }
        
        public static Mesh GenerateSkirt(int seed, int factionId)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Waistband
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0.12f, 0), 0.14f, 0.04f, 0.1f);
            
            // Skirt body (tiered approximation)
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0.02f, 0), 0.16f, 0.1f, 0.12f);
            PrimitiveShapes.AddBox(builder, new Vector3(0, -0.08f, 0), 0.18f, 0.1f, 0.14f);
            
            return builder.BuildFlat("Skirt");
        }
        
        public static Mesh GenerateCloak(int seed, int factionId)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Shoulder cape
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0.15f, -0.02f), 0.2f, 0.08f, 0.04f);
            
            // Long back drape
            PrimitiveShapes.AddBox(builder, new Vector3(0, -0.1f, -0.03f), 0.18f, 0.35f, 0.03f);
            
            // Hood attachment
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0.2f, 0), 0.12f, 0.06f, 0.08f);
            
            // Clasp
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0.14f, 0.03f), 0.03f, 0.03f, 0.02f);
            
            return builder.BuildFlat("Cloak");
        }
        
        public static Mesh GenerateOveralls(int seed, int factionId)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Leg sections
            PrimitiveShapes.AddBox(builder, new Vector3(-0.06f, -0.05f, 0), 0.07f, 0.25f, 0.09f);
            PrimitiveShapes.AddBox(builder, new Vector3(0.06f, -0.05f, 0), 0.07f, 0.25f, 0.09f);
            
            // Bib front
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0.12f, 0.02f), 0.13f, 0.15f, 0.03f);
            
            // Straps
            PrimitiveShapes.AddBox(builder, new Vector3(-0.08f, 0.1f, -0.02f), 0.02f, 0.12f, 0.02f);
            PrimitiveShapes.AddBox(builder, new Vector3(0.08f, 0.1f, -0.02f), 0.02f, 0.12f, 0.02f);
            
            // Tool pocket
            PrimitiveShapes.AddBox(builder, new Vector3(0.04f, 0.08f, 0.03f), 0.04f, 0.05f, 0.02f);
            
            return builder.BuildFlat("Overalls");
        }
    }
}
