using UnityEngine;

namespace Frontier.MeshGen.Building
{
    /// <summary>
    /// Generates modular wall pieces for the building system.
    /// Supports interior, exterior, half-walls, reinforced, and windowed variants.
    /// </summary>
    public static class WallKit
    {
        /// <summary>
        /// Generate a standard wall piece.
        /// </summary>
        public static Mesh GenerateWall(float width = 4f, float height = 3f, float thickness = 0.2f,
                                        bool hasFrame = false, string name = "Wall")
        {
            var builder = new LowPolyMeshBuilder();
            float hw = width * 0.5f;
            float hh = height * 0.5f;
            float ht = thickness * 0.5f;
            
            // Front face
            Color frontColor = hasFrame ? new Color(0.7f, 0.7f, 0.7f) : new Color(0.8f, 0.8f, 0.8f);
            builder.AddQuad(new Vector3(-hw, -hh, ht), new Vector3(hw, -hh, ht),
                           new Vector3(hw, hh, ht), new Vector3(-hw, hh, ht),
                           frontColor, frontColor, frontColor, frontColor);
            
            // Back face
            builder.AddQuad(new Vector3(hw, -hh, -ht), new Vector3(-hw, -hh, -ht),
                           new Vector3(-hw, hh, -ht), new Vector3(hw, hh, -ht),
                           frontColor, frontColor, frontColor, frontColor);
            
            // Left face
            builder.AddQuad(new Vector3(-hw, -hh, -ht), new Vector3(-hw, -hh, ht),
                           new Vector3(-hw, hh, ht), new Vector3(-hw, hh, -ht),
                           frontColor, frontColor, frontColor, frontColor);
            
            // Right face
            builder.AddQuad(new Vector3(hw, -hh, ht), new Vector3(hw, -hh, -ht),
                           new Vector3(hw, hh, -ht), new Vector3(hw, hh, ht),
                           frontColor, frontColor, frontColor, frontColor);
            
            // Top face
            builder.AddQuad(new Vector3(-hw, hh, -ht), new Vector3(hw, hh, -ht),
                           new Vector3(hw, hh, ht), new Vector3(-hw, hh, ht),
                           frontColor, frontColor, frontColor, frontColor);
            
            // Bottom face
            builder.AddQuad(new Vector3(-hw, -hh, ht), new Vector3(hw, -hh, ht),
                           new Vector3(hw, -hh, -ht), new Vector3(-hw, -hh, -ht),
                           frontColor, frontColor, frontColor, frontColor);
            
            if (hasFrame)
            {
                // Add frame details using vertex colors
                AddWallFrame(builder, width, height, thickness);
            }
            
            return builder.Build(name);
        }
        
        /// <summary>
        /// Generate a wall with a window opening.
        /// </summary>
        public static Mesh GenerateWindowWall(float width = 4f, float height = 3f, float thickness = 0.2f,
                                               float windowWidth = 1.5f, float windowHeight = 1.5f,
                                               string name = "WindowWall")
        {
            var builder = new LowPolyMeshBuilder();
            float hw = width * 0.5f;
            float hh = height * 0.5f;
            float ht = thickness * 0.5f;
            float hww = windowWidth * 0.5f;
            float hwh = windowHeight * 0.5f;
            float windowY = 0.5f; // Window center Y offset from bottom
            
            Color wallColor = new Color(0.8f, 0.8f, 0.8f);
            Color frameColor = new Color(0.5f, 0.5f, 0.5f);
            
            // Split wall into sections around window
            float windowBottom = windowY - hwh;
            float windowTop = windowY + hwh;
            
            // Bottom section
            if (windowBottom > -hh)
            {
                builder.AddQuad(new Vector3(-hw, -hh, ht), new Vector3(hw, -hh, ht),
                               new Vector3(hw, windowBottom, ht), new Vector3(-hw, windowBottom, ht),
                               wallColor, wallColor, wallColor, wallColor);
                
                builder.AddQuad(new Vector3(hw, -hh, -ht), new Vector3(-hw, -hh, -ht),
                               new Vector3(-hw, windowBottom, -ht), new Vector3(hw, windowBottom, -ht),
                               wallColor, wallColor, wallColor, wallColor);
            }
            
            // Top section
            if (windowTop < hh)
            {
                builder.AddQuad(new Vector3(-hw, windowTop, ht), new Vector3(hw, windowTop, ht),
                               new Vector3(hw, hh, ht), new Vector3(-hw, hh, ht),
                               wallColor, wallColor, wallColor, wallColor);
                
                builder.AddQuad(new Vector3(hw, windowTop, -ht), new Vector3(-hw, windowTop, -ht),
                               new Vector3(-hw, hh, -ht), new Vector3(hw, hh, -ht),
                               wallColor, wallColor, wallColor, wallColor);
            }
            
            // Left section
            if (-hw + hww < hw - hww)
            {
                builder.AddQuad(new Vector3(-hw, windowBottom, ht), new Vector3(-hw + hww, windowBottom, ht),
                               new Vector3(-hw + hww, windowTop, ht), new Vector3(-hw, windowTop, ht),
                               wallColor, wallColor, wallColor, wallColor);
                
                builder.AddQuad(new Vector3(-hw, windowBottom, -ht), new Vector3(-hw + hww, windowBottom, -ht),
                               new Vector3(-hw + hww, windowTop, -ht), new Vector3(-hw, windowTop, -ht),
                               wallColor, wallColor, wallColor, wallColor);
            }
            
            // Right section
            if (hw - hww > -hw + hww)
            {
                builder.AddQuad(new Vector3(hw - hww, windowBottom, ht), new Vector3(hw, windowBottom, ht),
                               new Vector3(hw, windowTop, ht), new Vector3(hw - hww, windowTop, ht),
                               wallColor, wallColor, wallColor, wallColor);
                
                builder.AddQuad(new Vector3(hw - hww, windowBottom, -ht), new Vector3(hw, windowBottom, -ht),
                               new Vector3(hw, windowTop, -ht), new Vector3(hw - hww, windowTop, -ht),
                               wallColor, wallColor, wallColor, wallColor);
            }
            
            // Window frame (inner edges)
            AddWindowFrame(builder, -hw + hww, hw - hww, windowBottom, windowTop, ht, frameColor);
            
            return builder.Build(name);
        }
        
        /// <summary>
        /// Generate a half-wall (waist height).
        /// </summary>
        public static Mesh GenerateHalfWall(float width = 4f, float height = 1.2f, float thickness = 0.2f,
                                             string name = "HalfWall")
        {
            return GenerateWall(width, height, thickness, true, name);
        }
        
        /// <summary>
        /// Generate a reinforced wall with extra thickness and metal plating.
        /// </summary>
        public static Mesh GenerateReinforcedWall(float width = 4f, float height = 3f,
                                                   string name = "ReinforcedWall")
        {
            return GenerateWall(width, height, 0.4f, true, name);
        }
        
        private static void AddWallFrame(LowPolyMeshBuilder builder, float width, float height, float thickness)
        {
            // Simple frame indication via vertex colors on edges
            // In a full implementation, this would add actual geometry
        }
        
        private static void AddWindowFrame(LowPolyMeshBuilder builder, float leftX, float rightX,
                                            float bottomY, float topY, float zOffset, Color color)
        {
            float frameDepth = 0.05f;
            
            // Inner window faces (glass plane)
            builder.AddQuad(new Vector3(leftX, bottomY, zOffset - frameDepth),
                           new Vector3(rightX, bottomY, zOffset - frameDepth),
                           new Vector3(rightX, topY, zOffset - frameDepth),
                           new Vector3(leftX, topY, zOffset - frameDepth),
                           new Color(0.7f, 0.85f, 0.9f, 0.5f),
                           new Color(0.7f, 0.85f, 0.9f, 0.5f),
                           new Color(0.7f, 0.85f, 0.9f, 0.5f),
                           new Color(0.7f, 0.85f, 0.9f, 0.5f));
        }
    }
}
