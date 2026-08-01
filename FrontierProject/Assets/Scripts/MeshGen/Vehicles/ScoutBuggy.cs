using UnityEngine;
using System.Collections.Generic;
using Frontier.MeshGen;

namespace Frontier.MeshGen.Vehicles {
    public static class ScoutBuggyGen {
        public static Mesh Generate() {
            var b = new LowPolyMeshBuilder();
            
            // Main chassis dimensions
            float chassisLength = 4.2f;
            float chassisWidth = 2.2f;
            float chassisHeight = 0.8f;
            
            // Lower chassis frame
            AddChassisFrame(b, chassisWidth, chassisHeight * 0.5f, chassisLength, Vector3.zero);
            
            // Roll cage structure
            AddRollCage(b, chassisWidth * 0.7f, chassisHeight * 1.2f, chassisLength * 0.6f, new Vector3(0, chassisHeight * 0.3f, 0));
            
            // Front bumper/grille
            AddFrontBumper(b, chassisWidth * 0.9f, chassisHeight * 0.4f, new Vector3(0, -chassisHeight * 0.2f, chassisLength * 0.48f));
            
            // Rear section with engine cover
            AddRearEngineCover(b, chassisWidth * 0.5f, chassisHeight * 0.4f, chassisLength * 0.25f, new Vector3(0, chassisHeight * 0.4f, -chassisLength * 0.35f));
            
            // Four off-road wheels
            float wheelRadius = 0.45f;
            float wheelWidth = 0.35f;
            float[] wheelZPositions = new float[] { -1.3f, 1.3f };
            
            for (int i = 0; i < 4; i++) {
                float x = (i % 2 == 0) ? -(chassisWidth * 0.5f + wheelWidth * 0.5f) : (chassisWidth * 0.5f + wheelWidth * 0.5f);
                float z = wheelZPositions[i / 2];
                AddOffRoadWheel(b, wheelRadius, wheelWidth, new Vector3(x, -wheelRadius, z), i < 2);
            }
            
            // Wheel fenders
            for (int side = -1; side <= 1; side += 2) {
                AddFender(b, chassisWidth * 0.5f + 0.05f, chassisHeight * 0.25f, side * (chassisWidth * 0.5f + 0.1f));
            }
            
            // Windshield frame
            AddWindshield(b, chassisWidth * 0.6f, chassisHeight * 0.5f, new Vector3(0, chassisHeight * 0.8f, chassisLength * 0.25f));
            
            // Headlights
            AddHeadlight(b, 0.12f, 0.08f, new Vector3(-0.6f, -chassisHeight * 0.1f, chassisLength * 0.49f));
            AddHeadlight(b, 0.12f, 0.08f, new Vector3(0.6f, -chassisHeight * 0.1f, chassisLength * 0.49f));
            
            // Side mirrors
            AddMirror(b, 0.08f, 0.1f, new Vector3(-(chassisWidth * 0.5f + 0.15f), chassisHeight * 0.7f, chassisLength * 0.2f));
            AddMirror(b, 0.08f, 0.1f, new Vector3(chassisWidth * 0.5f + 0.15f, chassisHeight * 0.7f, chassisLength * 0.2f));
            
            // Spare tire mount on rear
            AddSpareTire(b, wheelRadius * 0.9f, wheelWidth * 0.8f, new Vector3(0, chassisHeight * 0.5f, -chassisLength * 0.48f));
            
            return b.Build("ScoutBuggy");
        }
        
        static void AddChassisFrame(LowPolyMeshBuilder b, float width, float height, float length, Vector3 center) {
            // Bottom plate
            b.AddQuad(
                new Vector3(-width * 0.5f, -height * 0.5f, -length * 0.5f) + center,
                new Vector3(width * 0.5f, -height * 0.5f, -length * 0.5f) + center,
                new Vector3(width * 0.5f, -height * 0.5f, length * 0.5f) + center,
                new Vector3(-width * 0.5f, -height * 0.5f, length * 0.5f) + center
            );
            
            // Top plate (open cockpit area)
            b.AddQuad(
                new Vector3(-width * 0.5f, height * 0.5f, -length * 0.5f) + center,
                new Vector3(width * 0.5f, height * 0.5f, -length * 0.5f) + center,
                new Vector3(width * 0.5f, height * 0.5f, length * 0.3f) + center,
                new Vector3(-width * 0.5f, height * 0.5f, length * 0.3f) + center
            );
            
            // Side panels
            for (int side = -1; side <= 1; side += 2) {
                // Lower side panel
                b.AddQuad(
                    new Vector3(width * 0.5f * side, -height * 0.5f, -length * 0.5f) + center,
                    new Vector3(width * 0.5f * side, height * 0.5f, -length * 0.5f) + center,
                    new Vector3(width * 0.5f * side, height * 0.5f, length * 0.3f) + center,
                    new Vector3(width * 0.5f * side, -height * 0.5f, length * 0.3f) + center
                );
                
                // Front side extension
                b.AddQuad(
                    new Vector3(width * 0.5f * side, -height * 0.5f, length * 0.3f) + center,
                    new Vector3(width * 0.5f * side, height * 0.3f, length * 0.3f) + center,
                    new Vector3(width * 0.5f * side, height * 0.3f, length * 0.5f) + center,
                    new Vector3(width * 0.5f * side, -height * 0.5f, length * 0.5f) + center
                );
            }
            
            // Front panel (sloped)
            b.AddQuad(
                new Vector3(-width * 0.5f, -height * 0.5f, length * 0.5f) + center,
                new Vector3(width * 0.5f, -height * 0.5f, length * 0.5f) + center,
                new Vector3(width * 0.4f, height * 0.3f, length * 0.45f) + center,
                new Vector3(-width * 0.4f, height * 0.3f, length * 0.45f) + center
            );
            
            // Rear panel
            b.AddQuad(
                new Vector3(-width * 0.5f, -height * 0.5f, -length * 0.5f) + center,
                new Vector3(width * 0.5f, -height * 0.5f, -length * 0.5f) + center,
                new Vector3(width * 0.5f, height * 0.5f, -length * 0.5f) + center,
                new Vector3(-width * 0.5f, height * 0.5f, -length * 0.5f) + center
            );
        }
        
        static void AddRollCage(LowPolyMeshBuilder b, float width, float height, float length, Vector3 center) {
            int tubeSegments = 8;
            float tubeRadius = 0.04f;
            
            // Main hoop (front)
            AddTubeHoop(b, width, height, tubeRadius, tubeSegments, new Vector3(0, 0, length * 0.3f) + center);
            
            // Rear hoop
            AddTubeHoop(b, width * 0.9f, height * 0.9f, tubeRadius, tubeSegments, new Vector3(0, 0, -length * 0.3f) + center);
            
            // Longitudinal bars connecting hoops
            for (int side = -1; side <= 1; side += 2) {
                float xPos = width * 0.5f * side;
                for (int i = 0; i < tubeSegments; i++) {
                    float z1 = -length * 0.3f + (float)i / tubeSegments * length * 0.6f;
                    float z2 = -length * 0.3f + (float)(i + 1) / tubeSegments * length * 0.6f;
                    
                    b.AddQuad(
                        new Vector3(xPos, height * 0.5f, z1) + center,
                        new Vector3(xPos, height * 0.5f, z2) + center,
                        new Vector3(xPos + tubeRadius * side, height * 0.5f - tubeRadius, z2) + center,
                        new Vector3(xPos + tubeRadius * side, height * 0.5f - tubeRadius, z1) + center
                    );
                }
            }
            
            // Cross bar on top
            b.AddQuad(
                new Vector3(-width * 0.3f, height * 0.5f, 0) + center,
                new Vector3(width * 0.3f, height * 0.5f, 0) + center,
                new Vector3(width * 0.3f, height * 0.5f, tubeRadius * 2f) + center,
                new Vector3(-width * 0.3f, height * 0.5f, tubeRadius * 2f) + center
            );
        }
        
        static void AddTubeHoop(LowPolyMeshBuilder b, float width, float height, float radius, int segments, Vector3 center) {
            // Top arc
            for (int i = 0; i < segments / 2; i++) {
                float angle1 = Mathf.PI + (float)i / (segments / 2) * Mathf.PI;
                float angle2 = Mathf.PI + (float)(i + 1) / (segments / 2) * Mathf.PI;
                
                float x1 = Mathf.Cos(angle1) * width * 0.5f;
                float y1 = Mathf.Sin(angle1) * height * 0.5f;
                float x2 = Mathf.Cos(angle2) * width * 0.5f;
                float y2 = Mathf.Sin(angle2) * height * 0.5f;
                
                b.AddQuad(
                    new Vector3(x1, y1, center.z),
                    new Vector3(x2, y2, center.z),
                    new Vector3(x2 + radius * Mathf.Cos(angle2), y2 + radius * Mathf.Sin(angle2), center.z),
                    new Vector3(x1 + radius * Mathf.Cos(angle1), y1 + radius * Mathf.Sin(angle1), center.z)
                );
            }
        }
        
        static void AddFrontBumper(LowPolyMeshBuilder b, float width, float height, Vector3 center) {
            // Main bumper bar
            b.AddBox(width, height * 0.4f, 0.15f, new Vector3(0, 0, 0) + center);
            
            // Grille mesh
            int grilleBars = 5;
            for (int i = 0; i < grilleBars; i++) {
                float x = -width * 0.4f + (float)i / (grilleBars - 1) * width * 0.8f;
                b.AddQuad(
                    new Vector3(x - 0.02f, -height * 0.4f, center.z + 0.08f),
                    new Vector3(x + 0.02f, -height * 0.4f, center.z + 0.08f),
                    new Vector3(x + 0.02f, height * 0.4f, center.z + 0.08f),
                    new Vector3(x - 0.02f, height * 0.4f, center.z + 0.08f)
                );
            }
        }
        
        static void AddRearEngineCover(LowPolyMeshBuilder b, float width, float height, float length, Vector3 center) {
            // Engine box
            b.AddBox(width, height, length, center);
            
            // Vent slats on top
            for (int i = 0; i < 3; i++) {
                float z = -length * 0.3f + (float)i / 2f * length * 0.6f;
                b.AddQuad(
                    new Vector3(-width * 0.3f, height * 0.5f, z) + center,
                    new Vector3(width * 0.3f, height * 0.5f, z) + center,
                    new Vector3(width * 0.3f, height * 0.5f, z + 0.03f) + center,
                    new Vector3(-width * 0.3f, height * 0.5f, z + 0.03f) + center
                );
            }
        }
        
        static void AddOffRoadWheel(LowPolyMeshBuilder b, float radius, float width, Vector3 center, bool leftSide) {
            int segments = 14;
            float angleStep = Mathf.PI * 2f / segments;
            
            // Tire outer surface with tread blocks
            for (int i = 0; i < segments; i++) {
                float angle1 = i * angleStep;
                float angle2 = (i + 1) * angleStep;
                
                float x1 = Mathf.Cos(angle1) * radius;
                float y1 = Mathf.Sin(angle1) * radius;
                float x2 = Mathf.Cos(angle2) * radius;
                float y2 = Mathf.Sin(angle2) * radius;
                
                float zOffset = width * 0.5f * (leftSide ? -1 : 1);
                
                b.AddQuad(
                    new Vector3(x1, y1, -zOffset) + center,
                    new Vector3(x2, y2, -zOffset) + center,
                    new Vector3(x2, y2, zOffset) + center,
                    new Vector3(x1, y1, zOffset) + center
                );
            }
            
            // Tire sidewalls
            for (int side = -1; side <= 1; side += 2) {
                float zPos = width * 0.5f * side * (leftSide ? -1 : 1);
                for (int i = 0; i < segments; i++) {
                    float angle1 = i * angleStep;
                    float angle2 = (i + 1) * angleStep;
                    
                    float x1 = Mathf.Cos(angle1) * radius;
                    float y1 = Mathf.Sin(angle1) * radius;
                    float x2 = Mathf.Cos(angle2) * radius;
                    float y2 = Mathf.Sin(angle2) * radius;
                    
                    b.AddTriangle(
                        new Vector3(0, 0, zPos) + center,
                        new Vector3(x1, y1, zPos) + center,
                        new Vector3(x2, y2, zPos) + center
                    );
                }
            }
            
            // Aggressive tread blocks
            int treadBlocks = 8;
            for (int r = 0; r < 3; r++) {
                float ridgeZ = -width * 0.35f + (float)r / 2f * width * 0.7f;
                for (int i = 0; i < treadBlocks; i++) {
                    float angle = (float)i / treadBlocks * Mathf.PI * 2f;
                    float x = Mathf.Cos(angle) * (radius + 0.03f);
                    float y = Mathf.Sin(angle) * (radius + 0.03f);
                    
                    b.AddQuad(
                        new Vector3(x, y, ridgeZ - 0.03f) + center,
                        new Vector3(x, y, ridgeZ + 0.03f) + center,
                        new Vector3(Mathf.Cos(angle + 0.3f) * (radius + 0.03f), Mathf.Sin(angle + 0.3f) * (radius + 0.03f), ridgeZ + 0.03f) + center,
                        new Vector3(Mathf.Cos(angle + 0.3f) * (radius + 0.03f), Mathf.Sin(angle + 0.3f) * (radius + 0.03f), ridgeZ - 0.03f) + center
                    );
                }
            }
        }
        
        static void AddFender(LowPolyMeshBuilder b, float width, float height, float xPos) {
            float[] wheelZPositions = new float[] { -1.3f, 1.3f };
            
            foreach (float z in wheelZPositions) {
                // Curved fender over wheel
                int archSegments = 6;
                for (int i = 0; i < archSegments; i++) {
                    float angle1 = Mathf.PI + (float)i / archSegments * Mathf.PI;
                    float angle2 = Mathf.PI + (float)(i + 1) / archSegments * Mathf.PI;
                    
                    float y1 = Mathf.Sin(angle1) * height;
                    float y2 = Mathf.Sin(angle2) * height;
                    float z1 = z + Mathf.Cos(angle1) * height * 0.5f;
                    float z2 = z + Mathf.Cos(angle2) * height * 0.5f;
                    
                    b.AddQuad(
                        new Vector3(xPos, -height * 0.5f, z1),
                        new Vector3(xPos, y1, z1),
                        new Vector3(xPos, y2, z2),
                        new Vector3(xPos, -height * 0.5f, z2)
                    );
                }
            }
        }
        
        static void AddWindshield(LowPolyMeshBuilder b, float width, float height, Vector3 center) {
            // Frame
            float frameThickness = 0.05f;
            
            // Top frame
            b.AddBox(width, frameThickness, frameThickness, new Vector3(0, height * 0.5f, 0) + center);
            
            // Side frames
            for (int side = -1; side <= 1; side += 2) {
                b.AddBox(frameThickness, height, frameThickness, new Vector3(side * width * 0.5f, 0, 0) + center);
            }
            
            // Bottom frame
            b.AddBox(width, frameThickness, frameThickness, new Vector3(0, -height * 0.5f, 0) + center);
            
            // Glass panel (slightly inset)
            b.AddQuad(
                new Vector3(-width * 0.45f, -height * 0.4f, 0.02f) + center,
                new Vector3(width * 0.45f, -height * 0.4f, 0.02f) + center,
                new Vector3(width * 0.45f, height * 0.4f, 0.02f) + center,
                new Vector3(-width * 0.45f, height * 0.4f, 0.02f) + center
            );
        }
        
        static void AddHeadlight(LowPolyMeshBuilder b, float width, float height, Vector3 center) {
            b.AddCylinder(width, 0.08f, 8, center);
            
            // Lens
            b.AddQuad(
                new Vector3(-width * 0.8f, -height * 0.8f, center.z + 0.04f),
                new Vector3(width * 0.8f, -height * 0.8f, center.z + 0.04f),
                new Vector3(width * 0.8f, height * 0.8f, center.z + 0.04f),
                new Vector3(-width * 0.8f, height * 0.8f, center.z + 0.04f)
            );
        }
        
        static void AddMirror(LowPolyMeshBuilder b, float width, float height, Vector3 center) {
            // Mirror housing
            b.AddBox(width, height, 0.05f, center);
            
            // Mirror stalk
            b.AddCylinder(0.02f, 0.15f, 6, new Vector3(0, -height * 0.5f - 0.07f, 0) + center);
        }
        
        static void AddSpareTire(LowPolyMeshBuilder b, float radius, float width, Vector3 center) {
            int segments = 12;
            float angleStep = Mathf.PI * 2f / segments;
            
            // Tire outer
            for (int i = 0; i < segments; i++) {
                float angle1 = i * angleStep;
                float angle2 = (i + 1) * angleStep;
                
                float x1 = Mathf.Cos(angle1) * radius;
                float y1 = Mathf.Sin(angle1) * radius;
                float x2 = Mathf.Cos(angle2) * radius;
                float y2 = Mathf.Sin(angle2) * radius;
                
                b.AddQuad(
                    new Vector3(x1, y1, -width) + center,
                    new Vector3(x2, y2, -width) + center,
                    new Vector3(x2, y2, width) + center,
                    new Vector3(x1, y1, width) + center
                );
            }
            
            // Sidewalls
            for (int side = -1; side <= 1; side += 2) {
                float zPos = width * side;
                for (int i = 0; i < segments; i++) {
                    float angle1 = i * angleStep;
                    float angle2 = (i + 1) * angleStep;
                    
                    float x1 = Mathf.Cos(angle1) * radius;
                    float y1 = Mathf.Sin(angle1) * radius;
                    float x2 = Mathf.Cos(angle2) * radius;
                    float y2 = Mathf.Sin(angle2) * radius;
                    
                    b.AddTriangle(
                        new Vector3(0, 0, zPos) + center,
                        new Vector3(x1, y1, zPos) + center,
                        new Vector3(x2, y2, zPos) + center
                    );
                }
            }
        }
    }
}
