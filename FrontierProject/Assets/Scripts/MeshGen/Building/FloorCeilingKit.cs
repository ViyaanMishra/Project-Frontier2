using UnityEngine;

namespace Frontier.MeshGen.Building
{
    /// <summary>
    /// Generates modular floor and ceiling pieces for the building system.
    /// Supports floor tiles, ceiling panels, raised floors, grated floors, and foundation slabs.
    /// </summary>
    public static class FloorCeilingKit
    {
        /// <summary>
        /// Generate a standard floor tile.
        /// </summary>
        public static Mesh GenerateFloor(float width = 4f, float length = 4f, float thickness = 0.15f,
                                          bool hasPattern = true, string name = "Floor")
        {
            var builder = new LowPolyMeshBuilder();
            float hw = width * 0.5f;
            float hl = length * 0.5f;
            float ht = thickness * 0.5f;
            
            Color topColor = hasPattern ? CreateTilePattern() : new Color(0.75f, 0.75f, 0.75f);
            Color bottomColor = new Color(0.6f, 0.6f, 0.6f);
            Color edgeColor = new Color(0.65f, 0.65f, 0.65f);
            
            // Top face (with optional pattern)
            builder.AddQuad(new Vector3(-hw, ht, -hl), new Vector3(hw, ht, -hl),
                           new Vector3(hw, ht, hl), new Vector3(-hw, ht, hl),
                           topColor, topColor, topColor, topColor);
            
            // Bottom face
            builder.AddQuad(new Vector3(-hw, -ht, hl), new Vector3(hw, -ht, hl),
                           new Vector3(hw, -ht, -hl), new Vector3(-hw, -ht, -hl),
                           bottomColor, bottomColor, bottomColor, bottomColor);
            
            // Side faces
            AddSideFaces(builder, hw, hl, ht, edgeColor);
            
            return builder.Build(name);
        }
        
        /// <summary>
        /// Generate a ceiling panel (inverted floor).
        /// </summary>
        public static Mesh GenerateCeiling(float width = 4f, float length = 4f, float thickness = 0.1f,
                                            bool hasLightFixture = false, string name = "Ceiling")
        {
            var builder = new LowPolyMeshBuilder();
            float hw = width * 0.5f;
            float hl = length * 0.5f;
            float ht = thickness * 0.5f;
            
            Color topColor = new Color(0.8f, 0.8f, 0.8f);
            Color bottomColor = hasLightFixture ? new Color(0.9f, 0.9f, 0.85f) : new Color(0.75f, 0.75f, 0.75f);
            Color edgeColor = new Color(0.7f, 0.7f, 0.7f);
            
            // Top face
            builder.AddQuad(new Vector3(-hw, ht, -hl), new Vector3(hw, ht, -hl),
                           new Vector3(hw, ht, hl), new Vector3(-hw, ht, hl),
                           topColor, topColor, topColor, topColor);
            
            // Bottom face
            builder.AddQuad(new Vector3(-hw, -ht, hl), new Vector3(hw, -ht, hl),
                           new Vector3(hw, -ht, -hl), new Vector3(-hw, -ht, -hl),
                           bottomColor, bottomColor, bottomColor, bottomColor);
            
            // Side faces
            AddSideFaces(builder, hw, hl, ht, edgeColor);
            
            return builder.Build(name);
        }
        
        /// <summary>
        /// Generate a raised floor with visible support legs.
        /// </summary>
        public static Mesh GenerateRaisedFloor(float width = 4f, float length = 4f, float height = 0.5f,
                                                string name = "RaisedFloor")
        {
            var builder = new LowPolyMeshBuilder();
            float hw = width * 0.5f;
            float hl = length * 0.5f;
            float floorThickness = 0.1f;
            
            Color floorColor = new Color(0.7f, 0.7f, 0.7f);
            Color legColor = new Color(0.5f, 0.5f, 0.5f);
            
            // Floor platform
            builder.AddQuad(new Vector3(-hw, height, -hl), new Vector3(hw, height, -hl),
                           new Vector3(hw, height, hl), new Vector3(-hw, height, hl),
                           floorColor, floorColor, floorColor, floorColor);
            
            // Support legs at corners
            float legSize = 0.1f;
            float legHalf = legSize * 0.5f;
            
            // Four corner legs
            Vector3[] legPositions = {
                new Vector3(-hw + legHalf, height * 0.5f, -hl + legHalf),
                new Vector3(hw - legHalf, height * 0.5f, -hl + legHalf),
                new Vector3(hw - legHalf, height * 0.5f, hl - legHalf),
                new Vector3(-hw + legHalf, height * 0.5f, hl - legHalf)
            };
            
            foreach (var legPos in legPositions)
            {
                AddLeg(builder, legPos, legHalf, height, legColor);
            }
            
            return builder.Build(name);
        }
        
        /// <summary>
        /// Generate a grated floor (metal grid with holes).
        /// </summary>
        public static Mesh GenerateGratedFloor(float width = 4f, float length = 4f, float thickness = 0.08f,
                                                 float gridSize = 0.25f, string name = "GratedFloor")
        {
            var builder = new LowPolyMeshBuilder();
            float hw = width * 0.5f;
            float hl = length * 0.5f;
            float ht = thickness * 0.5f;
            
            Color grateColor = new Color(0.55f, 0.55f, 0.55f);
            
            // Create grid pattern using multiple small quads
            int gridCountX = Mathf.FloorToInt(width / gridSize);
            int gridCountZ = Mathf.FloorToInt(length / gridSize);
            float barWidth = gridSize * 0.2f;
            float barHalf = barWidth * 0.5f;
            
            // Horizontal bars
            for (int z = 0; z < gridCountZ; z++)
            {
                float zPos = -hl + (z + 0.5f) * gridSize;
                
                for (int x = 0; x < gridCountX; x++)
                {
                    float xPos = -hw + (x + 0.5f) * gridSize;
                    
                    // Horizontal bar segment
                    builder.AddQuad(new Vector3(xPos - barHalf, ht, zPos - barHalf),
                                   new Vector3(xPos + barHalf, ht, zPos - barHalf),
                                   new Vector3(xPos + barHalf, ht, zPos + barHalf),
                                   new Vector3(xPos - barHalf, ht, zPos + barHalf),
                                   grateColor, grateColor, grateColor, grateColor);
                }
            }
            
            // Vertical bars
            for (int x = 0; x < gridCountX; x++)
            {
                float xPos = -hw + (x + 0.5f) * gridSize;
                
                for (int z = 0; z < gridCountZ; z++)
                {
                    float zPos = -hl + (z + 0.5f) * gridSize;
                    
                    // Vertical bar segment
                    builder.AddQuad(new Vector3(xPos - barHalf, ht, zPos - barHalf),
                                   new Vector3(xPos + barHalf, ht, zPos - barHalf),
                                   new Vector3(xPos + barHalf, ht, zPos + barHalf),
                                   new Vector3(xPos - barHalf, ht, zPos + barHalf),
                                   grateColor, grateColor, grateColor, grateColor);
                }
            }
            
            return builder.Build(name);
        }
        
        /// <summary>
        /// Generate a foundation slab (thick concrete base).
        /// </summary>
        public static Mesh GenerateFoundationSlab(float width = 4f, float length = 4f, float thickness = 0.5f,
                                                   string name = "FoundationSlab")
        {
            return GenerateFloor(width, length, thickness, false, name);
        }
        
        private static void AddSideFaces(LowPolyMeshBuilder builder, float hw, float hl, float ht, Color color)
        {
            // Front
            builder.AddQuad(new Vector3(-hw, -ht, hl), new Vector3(hw, -ht, hl),
                           new Vector3(hw, ht, hl), new Vector3(-hw, ht, hl),
                           color, color, color, color);
            
            // Back
            builder.AddQuad(new Vector3(hw, -ht, -hl), new Vector3(-hw, -ht, -hl),
                           new Vector3(-hw, ht, -hl), new Vector3(hw, ht, -hl),
                           color, color, color, color);
            
            // Left
            builder.AddQuad(new Vector3(-hw, -ht, -hl), new Vector3(-hw, -ht, hl),
                           new Vector3(-hw, ht, hl), new Vector3(-hw, ht, -hl),
                           color, color, color, color);
            
            // Right
            builder.AddQuad(new Vector3(hw, -ht, hl), new Vector3(hw, -ht, -hl),
                           new Vector3(hw, ht, -hl), new Vector3(hw, ht, hl),
                           color, color, color, color);
        }
        
        private static void AddLeg(LowPolyMeshBuilder builder, Vector3 center, float halfSize, float height, Color color)
        {
            float bottomY = 0f;
            float topY = height;
            
            // Four sides of the leg
            builder.AddQuad(new Vector3(center.x - halfSize, bottomY, center.z - halfSize),
                           new Vector3(center.x + halfSize, bottomY, center.z - halfSize),
                           new Vector3(center.x + halfSize, topY, center.z - halfSize),
                           new Vector3(center.x - halfSize, topY, center.z - halfSize),
                           color, color, color, color);
            
            builder.AddQuad(new Vector3(center.x + halfSize, bottomY, center.z + halfSize),
                           new Vector3(center.x - halfSize, bottomY, center.z + halfSize),
                           new Vector3(center.x - halfSize, topY, center.z + halfSize),
                           new Vector3(center.x + halfSize, topY, center.z + halfSize),
                           color, color, color, color);
            
            builder.AddQuad(new Vector3(center.x - halfSize, bottomY, center.z + halfSize),
                           new Vector3(center.x - halfSize, bottomY, center.z - halfSize),
                           new Vector3(center.x - halfSize, topY, center.z - halfSize),
                           new Vector3(center.x - halfSize, topY, center.z + halfSize),
                           color, color, color, color);
            
            builder.AddQuad(new Vector3(center.x + halfSize, bottomY, center.z - halfSize),
                           new Vector3(center.x + halfSize, bottomY, center.z + halfSize),
                           new Vector3(center.x + halfSize, topY, center.z + halfSize),
                           new Vector3(center.x + halfSize, topY, center.z - halfSize),
                           color, color, color, color);
        }
        
        private static Color CreateTilePattern()
        {
            // Simple variation for visual interest
            float variance = Random.value * 0.1f;
            return new Color(0.75f + variance, 0.75f + variance, 0.75f + variance);
        }
    }
}
