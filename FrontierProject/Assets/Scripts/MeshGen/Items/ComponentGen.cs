using UnityEngine;
using Frontier.MeshGen;

namespace Frontier.MeshGen.Items
{
    /// <summary>
    /// Generates low-poly component meshes for crafting and electronics (12 types)
    /// </summary>
    public static class ComponentGen
    {
        public static Mesh GenerateGear(int seed)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Gear body (cylinder with teeth approximation)
            PrimitiveShapes.AddCylinder(builder, Vector3.zero, 0.05f, 0.015f, 12);
            
            // Center hole
            PrimitiveShapes.AddCylinder(builder, Vector3.zero, 0.02f, 0.02f, 8);
            
            // Teeth (simplified as boxes around edge)
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45 * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 0.06f, 0, Mathf.Sin(angle) * 0.06f);
                PrimitiveShapes.AddBox(builder, pos, 0.015f, 0.015f, 0.01f);
            }
            
            return builder.BuildFlat("Gear");
        }
        
        public static Mesh GenerateSpring(int seed)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Coiled spring (stacked rotated rings approximation)
            for (int i = 0; i < 6; i++)
            {
                float y = -0.05f + i * 0.02f;
                float rotation = i * 30 * Mathf.Deg2Rad;
                PrimitiveShapes.AddTorus(builder, new Vector3(0, y, 0), 0.03f, 0.005f, 8);
            }
            
            return builder.BuildFlat("Spring");
        }
        
        public static Mesh GenerateWireSpool(int seed)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Spool center
            PrimitiveShapes.AddCylinder(builder, Vector3.zero, 0.02f, 0.04f, 8);
            
            // End caps
            PrimitiveShapes.AddCylinder(builder, new Vector3(0, 0.025f, 0), 0.04f, 0.005f, 8);
            PrimitiveShapes.AddCylinder(builder, new Vector3(0, -0.025f, 0), 0.04f, 0.005f, 8);
            
            // Wire coil
            PrimitiveShapes.AddTorus(builder, new Vector3(0, 0, 0), 0.035f, 0.008f, 12);
            
            return builder.BuildFlat("WireSpool");
        }
        
        public static Mesh GenerateMotor(int seed)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Motor housing
            PrimitiveShapes.AddCylinder(builder, Vector3.zero, 0.04f, 0.06f, 10);
            
            // Shaft
            PrimitiveShapes.AddCylinder(builder, new Vector3(0, 0.04f, 0), 0.01f, 0.02f, 8);
            
            // Mounting tabs
            PrimitiveShapes.AddBox(builder, new Vector3(-0.05f, 0, 0), 0.015f, 0.02f, 0.03f);
            PrimitiveShapes.AddBox(builder, new Vector3(0.05f, 0, 0), 0.015f, 0.02f, 0.03f);
            
            // Terminal posts
            PrimitiveShapes.AddCylinder(builder, new Vector3(-0.025f, 0.03f, 0.03f), 0.005f, 0.01f, 6);
            PrimitiveShapes.AddCylinder(builder, new Vector3(0.025f, 0.03f, 0.03f), 0.005f, 0.01f, 6);
            
            return builder.BuildFlat("Motor");
        }
        
        public static Mesh GenerateCircuitBoard(int seed)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // PCB base
            PrimitiveShapes.AddBox(builder, Vector3.zero, 0.08f, 0.01f, 0.06f);
            
            // Chip packages
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0.008f, 0), 0.03f, 0.008f, 0.025f);
            
            // Capacitors
            PrimitiveShapes.AddCylinder(builder, new Vector3(-0.025f, 0.006f, -0.02f), 0.006f, 0.01f, 6);
            PrimitiveShapes.AddCylinder(builder, new Vector3(0.025f, 0.006f, -0.02f), 0.006f, 0.01f, 6);
            
            // Pin headers
            for (int i = 0; i < 4; i++)
            {
                PrimitiveShapes.AddCylinder(builder, new Vector3(-0.03f, 0.005f, 0.015f + i * 0.01f), 0.002f, 0.005f, 4);
            }
            
            return builder.BuildFlat("CircuitBoard");
        }
        
        public static Mesh GenerateLens(int seed)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Lens element (biconvex approximation)
            PrimitiveShapes.AddSphere(builder, Vector3.zero, 0.03f, 8);
            
            // Rim mount
            PrimitiveShapes.AddTorus(builder, Vector3.zero, 0.03f, 0.003f, 12);
            
            return builder.BuildFlat("Lens");
        }
        
        public static Mesh GenerateAntenna(int seed)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Base
            PrimitiveShapes.AddCylinder(builder, new Vector3(0, -0.05f, 0), 0.01f, 0.01f, 6);
            
            // Telescoping segments
            PrimitiveShapes.AddCylinder(builder, new Vector3(0, -0.02f, 0), 0.008f, 0.03f, 6);
            PrimitiveShapes.AddCylinder(builder, new Vector3(0, 0.02f, 0), 0.006f, 0.03f, 6);
            PrimitiveShapes.AddCylinder(builder, new Vector3(0, 0.06f, 0), 0.004f, 0.03f, 6);
            
            // Tip ball
            PrimitiveShapes.AddSphere(builder, new Vector3(0, 0.08f, 0), 0.003f, 4);
            
            return builder.BuildFlat("Antenna");
        }
        
        public static Mesh GenerateBattery(int seed)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Battery body
            PrimitiveShapes.AddBox(builder, Vector3.zero, 0.03f, 0.05f, 0.015f);
            
            // Positive terminal
            PrimitiveShapes.AddCylinder(builder, new Vector3(0, 0.028f, 0), 0.005f, 0.003f, 8);
            
            // Label area
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0, 0.008f), 0.025f, 0.04f, 0.002f);
            
            return builder.BuildFlat("Battery");
        }
        
        public static Mesh GenerateTransistor(int seed)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Body (half-cylinder)
            PrimitiveShapes.AddCylinder(builder, new Vector3(0, 0, 0), 0.008f, 0.015f, 6);
            
            // Flat side
            PrimitiveShapes.AddBox(builder, new Vector3(0, 0, 0.004f), 0.01f, 0.015f, 0.002f);
            
            // Leads
            PrimitiveShapes.AddCylinder(builder, new Vector3(-0.006f, -0.01f, 0), 0.002f, 0.02f, 4);
            PrimitiveShapes.AddCylinder(builder, new Vector3(0, -0.01f, 0), 0.002f, 0.02f, 4);
            PrimitiveShapes.AddCylinder(builder, new Vector3(0.006f, -0.01f, 0), 0.002f, 0.02f, 4);
            
            return builder.BuildFlat("Transistor");
        }
        
        public static Mesh GenerateResistor(int seed)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Body (cylinder with color bands)
            PrimitiveShapes.AddCylinder(builder, Vector3.zero, 0.005f, 0.015f, 8);
            
            // Leads
            PrimitiveShapes.AddCylinder(builder, new Vector3(0, 0, -0.015f), 0.002f, 0.01f, 4);
            PrimitiveShapes.AddCylinder(builder, new Vector3(0, 0, 0.015f), 0.002f, 0.01f, 4);
            
            return builder.BuildFlat("Resistor");
        }
        
        public static Mesh GenerateBearing(int seed)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Outer race
            PrimitiveShapes.AddTorus(builder, Vector3.zero, 0.04f, 0.005f, 16);
            
            // Inner race
            PrimitiveShapes.AddTorus(builder, Vector3.zero, 0.025f, 0.004f, 12);
            
            // Ball bearings (simplified)
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45 * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 0.0325f, 0, Mathf.Sin(angle) * 0.0325f);
                PrimitiveShapes.AddSphere(builder, pos, 0.004f, 4);
            }
            
            return builder.BuildFlat("Bearing");
        }
        
        public static Mesh GeneratePiston(int seed)
        {
            ProceduralRandom.Init(seed);
            var builder = new LowPolyMeshBuilder();
            
            // Piston head
            PrimitiveShapes.AddCylinder(builder, Vector3.zero, 0.03f, 0.02f, 10);
            
            // Ring grooves
            PrimitiveShapes.AddTorus(builder, new Vector3(0, 0.008f, 0), 0.03f, 0.002f, 10);
            PrimitiveShapes.AddTorus(builder, new Vector3(0, -0.008f, 0), 0.03f, 0.002f, 10);
            
            // Connecting rod attachment
            PrimitiveShapes.AddBox(builder, new Vector3(0, -0.015f, 0), 0.015f, 0.02f, 0.01f);
            
            return builder.BuildFlat("Piston");
        }
    }
}
