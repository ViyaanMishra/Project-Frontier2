using UnityEngine;
using Frontier.MeshGen;

namespace Frontier.MeshGen.Items
{
    /// <summary>
    /// Generates low-poly ammunition meshes for all 6 ammo types
    /// </summary>
    public static class AmmoGen
    {
        public static Mesh GeneratePistolAmmo(int seed)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Bullet casing (cylinder)
            PrimitiveShapes.AddCylinder(builder, Vector3.zero, 0.015f, 0.04f, 8);
            
            // Bullet tip (cone)
            PrimitiveShapes.AddCone(builder, new Vector3(0, 0.025f, 0), 0.015f, 0.02f, 8);
            
            // Base rim
            PrimitiveShapes.AddCylinder(builder, new Vector3(0, -0.02f, 0), 0.018f, 0.005f, 8);
            
            return builder.BuildFlat("PistolAmmo");
        }
        
        public static Mesh GenerateRifleAmmo(int seed)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Longer casing
            PrimitiveShapes.AddCylinder(builder, Vector3.zero, 0.012f, 0.06f, 8);
            
            // Pointed tip
            PrimitiveShapes.AddCone(builder, new Vector3(0, 0.035f, 0), 0.012f, 0.03f, 8);
            
            // Base rim
            PrimitiveShapes.AddCylinder(builder, new Vector3(0, -0.03f, 0), 0.015f, 0.005f, 8);
            
            return builder.BuildFlat("RifleAmmo");
        }
        
        public static Mesh GenerateShotgunShell(int seed)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Main body (wider cylinder)
            PrimitiveShapes.AddCylinder(builder, Vector3.zero, 0.025f, 0.07f, 10);
            
            // Brass base
            PrimitiveShapes.AddCylinder(builder, new Vector3(0, -0.035f, 0), 0.025f, 0.015f, 10);
            
            // Crimped top
            PrimitiveShapes.AddCone(builder, new Vector3(0, 0.035f, 0), 0.025f, 0.01f, 8);
            
            return builder.BuildFlat("ShotgunShell");
        }
        
        public static Mesh GenerateHeavyAmmo(int seed)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Large caliber casing
            PrimitiveShapes.AddCylinder(builder, Vector3.zero, 0.02f, 0.1f, 10);
            
            // Armor-piercing tip
            PrimitiveShapes.AddCone(builder, new Vector3(0, 0.055f, 0), 0.02f, 0.04f, 8);
            
            // Belt link attachment points
            PrimitiveShapes.AddBox(builder, new Vector3(-0.025f, 0, 0), 0.01f, 0.03f, 0.015f);
            PrimitiveShapes.AddBox(builder, new Vector3(0.025f, 0, 0), 0.01f, 0.03f, 0.015f);
            
            return builder.BuildFlat("HeavyAmmo");
        }
        
        public static Mesh GenerateEnergyCell(int seed)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Rectangular cell body
            PrimitiveShapes.AddBox(builder, Vector3.zero, 0.03f, 0.08f, 0.02f);
            
            // Glowing core (emissive)
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0, 0), 0.02f, 0.06f, 0.01f);
            
            // Contact points
            PrimitiveShapes.AddCylinder(builder, new Vector3(0, 0.04f, 0), 0.005f, 0.002f, 6);
            PrimitiveShapes.AddCylinder(builder, new Vector3(0, -0.04f, 0), 0.005f, 0.002f, 6);
            
            return builder.BuildFlat("EnergyCell");
        }
        
        public static Mesh GenerateExplosiveRound(int seed)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Shell body
            PrimitiveShapes.AddCylinder(builder, Vector3.zero, 0.04f, 0.12f, 10);
            
            // Explosive tip (red)
            PrimitiveShapes.AddCone(builder, new Vector3(0, 0.07f, 0), 0.04f, 0.05f, 8);
            
            // Fuse
            PrimitiveShapes.AddCylinder(builder, new Vector3(0, 0.1f, 0), 0.01f, 0.02f, 6);
            
            // Stabilizing fins
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90 * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 0.02f, -0.05f, Mathf.Sin(angle) * 0.02f);
                PrimitiveShapes.AddBox(builder, pos, 0.005f, 0.03f, 0.015f);
            }
            
            return builder.BuildFlat("ExplosiveRound");
        }
    }
}
