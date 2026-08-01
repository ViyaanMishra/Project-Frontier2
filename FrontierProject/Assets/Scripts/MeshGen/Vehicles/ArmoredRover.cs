using UnityEngine;
using System.Collections.Generic;
using Frontier.MeshGen;

namespace Frontier.MeshGen.Vehicles {
    public static class ArmoredRoverGen {
        public static Mesh Generate() {
            var b = new LowPolyMeshBuilder();
            
            // Main armored body with angled plates
            float bodyLength = 5.5f;
            float bodyWidth = 2.8f;
            float bodyHeight = 1.6f;
            
            // Lower hull - trapezoidal shape
            AddArmoredHull(b, bodyWidth, bodyHeight * 0.6f, bodyLength, Vector3.zero);
            
            // Upper turret/cabin section
            AddCabinSection(b, bodyWidth * 0.7f, bodyHeight * 0.7f, bodyLength * 0.5f, new Vector3(0, bodyHeight * 0.5f, bodyLength * 0.1f));
            
            // Front glacis plate (angled armor)
            AddGlacisPlate(b, bodyWidth, bodyHeight * 0.4f, new Vector3(0, bodyHeight * 0.3f, bodyLength * 0.4f));
            
            // Engine deck at rear
            AddEngineDeck(b, bodyWidth * 0.6f, bodyHeight * 0.3f, bodyLength * 0.3f, new Vector3(0, bodyHeight * 0.7f, -bodyLength * 0.35f));
            
            // Six wheels with detailed tires
            float wheelRadius = 0.55f;
            float wheelWidth = 0.4f;
            float[] wheelZPositions = new float[] { -1.8f, -0.9f, 0f, 0.9f, 1.8f, 2.7f };
            
            for (int i = 0; i < 6; i++) {
                float x = (i % 2 == 0) ? -(bodyWidth * 0.5f + wheelWidth * 0.5f) : (bodyWidth * 0.5f + wheelWidth * 0.5f);
                float z = wheelZPositions[i];
                AddDetailedWheel(b, wheelRadius, wheelWidth, new Vector3(x, -wheelRadius, z), i < 3);
            }
            
            // Wheel arches/fenders
            for (int side = -1; side <= 1; side += 2) {
                AddWheelArches(b, bodyWidth * 0.5f, bodyHeight * 0.3f, bodyLength, side * (bodyWidth * 0.5f + 0.1f));
            }
            
            // Weapon mount on top
            AddWeaponMount(b, 0.8f, 0.6f, 1.2f, new Vector3(0, bodyHeight * 1.1f, bodyLength * 0.15f));
            
            // Vision blocks/sensors
            AddSensorBlock(b, 0.3f, 0.2f, 0.15f, new Vector3(0, bodyHeight * 0.8f, bodyLength * 0.45f));
            
            // Exhaust vents at rear
            AddExhaustVents(b, 0.4f, 0.3f, new Vector3(-0.6f, bodyHeight * 0.9f, -bodyLength * 0.45f));
            AddExhaustVents(b, 0.4f, 0.3f, new Vector3(0.6f, bodyHeight * 0.9f, -bodyLength * 0.45f));
            
            // Headlights front
            AddHeadlight(b, 0.15f, 0.1f, new Vector3(-0.8f, bodyHeight * 0.4f, bodyLength * 0.48f));
            AddHeadlight(b, 0.15f, 0.1f, new Vector3(0.8f, bodyHeight * 0.4f, bodyLength * 0.48f));
            
            return b.Build("ArmoredRover");
        }
        
        static void AddArmoredHull(LowPolyMeshBuilder b, float width, float height, float length, Vector3 center) {
            // Bottom plate
            b.AddQuad(
                new Vector3(-width * 0.5f, -height * 0.5f, -length * 0.5f) + center,
                new Vector3(width * 0.5f, -height * 0.5f, -length * 0.5f) + center,
                new Vector3(width * 0.5f, -height * 0.5f, length * 0.5f) + center,
                new Vector3(-width * 0.5f, -height * 0.5f, length * 0.5f) + center
            );
            
            // Top plate (slightly narrower for angled sides)
            float topNarrow = width * 0.85f;
            b.AddQuad(
                new Vector3(-topNarrow * 0.5f, height * 0.5f, -length * 0.5f) + center,
                new Vector3(topNarrow * 0.5f, height * 0.5f, -length * 0.5f) + center,
                new Vector3(topNarrow * 0.5f, height * 0.5f, length * 0.5f) + center,
                new Vector3(-topNarrow * 0.5f, height * 0.5f, length * 0.5f) + center
            );
            
            // Side plates (angled)
            int segments = 8;
            for (int side = -1; side <= 1; side += 2) {
                for (int i = 0; i < segments; i++) {
                    float z1 = -length * 0.5f + (float)i / segments * length;
                    float z2 = -length * 0.5f + (float)(i + 1) / segments * length;
                    
                    float xBottom = width * 0.5f * side;
                    float xTop = topNarrow * 0.5f * side;
                    
                    b.AddQuad(
                        new Vector3(xBottom, -height * 0.5f, z1) + center,
                        new Vector3(xTop, height * 0.5f, z1) + center,
                        new Vector3(xTop, height * 0.5f, z2) + center,
                        new Vector3(xBottom, -height * 0.5f, z2) + center
                    );
                }
            }
            
            // Front plate (angled)
            float frontSlope = width * 0.15f;
            b.AddQuad(
                new Vector3(-width * 0.5f, -height * 0.5f, length * 0.5f) + center,
                new Vector3(width * 0.5f, -height * 0.5f, length * 0.5f) + center,
                new Vector3(topNarrow * 0.5f - frontSlope, height * 0.5f, length * 0.5f - frontSlope) + center,
                new Vector3(-topNarrow * 0.5f + frontSlope, height * 0.5f, length * 0.5f - frontSlope) + center
            );
            
            // Rear plate
            b.AddQuad(
                new Vector3(-width * 0.5f, -height * 0.5f, -length * 0.5f) + center,
                new Vector3(width * 0.5f, -height * 0.5f, -length * 0.5f) + center,
                new Vector3(topNarrow * 0.5f, height * 0.5f, -length * 0.5f) + center,
                new Vector3(-topNarrow * 0.5f, height * 0.5f, -length * 0.5f) + center
            );
        }
        
        static void AddCabinSection(LowPolyMeshBuilder b, float width, float height, float length, Vector3 center) {
            // Box-like cabin with slight angles
            float narrow = width * 0.85f;
            
            // Cabin floor
            b.AddQuad(
                new Vector3(-width * 0.5f, -height * 0.5f, -length * 0.5f) + center,
                new Vector3(width * 0.5f, -height * 0.5f, -length * 0.5f) + center,
                new Vector3(width * 0.5f, -height * 0.5f, length * 0.5f) + center,
                new Vector3(-width * 0.5f, -height * 0.5f, length * 0.5f) + center
            );
            
            // Cabin roof
            b.AddQuad(
                new Vector3(-narrow * 0.5f, height * 0.5f, -length * 0.5f) + center,
                new Vector3(narrow * 0.5f, height * 0.5f, -length * 0.5f) + center,
                new Vector3(narrow * 0.5f, height * 0.5f, length * 0.5f) + center,
                new Vector3(-narrow * 0.5f, height * 0.5f, length * 0.5f) + center
            );
            
            // Cabin sides
            for (int side = -1; side <= 1; side += 2) {
                b.AddQuad(
                    new Vector3(width * 0.5f * side, -height * 0.5f, -length * 0.5f) + center,
                    new Vector3(narrow * 0.5f * side, height * 0.5f, -length * 0.5f) + center,
                    new Vector3(narrow * 0.5f * side, height * 0.5f, length * 0.5f) + center,
                    new Vector3(width * 0.5f * side, -height * 0.5f, length * 0.5f) + center
                );
                
                // Window cutout suggestion (darker inset panel)
                b.AddQuad(
                    new Vector3((width * 0.5f - 0.1f) * side, -height * 0.2f, -length * 0.3f) + center,
                    new Vector3((narrow * 0.5f + 0.05f) * side, height * 0.3f, -length * 0.3f) + center,
                    new Vector3((narrow * 0.5f + 0.05f) * side, height * 0.3f, length * 0.2f) + center,
                    new Vector3((width * 0.5f - 0.1f) * side, -height * 0.2f, length * 0.2f) + center
                );
            }
            
            // Cabin front (sloped)
            b.AddQuad(
                new Vector3(-width * 0.5f, -height * 0.5f, length * 0.5f) + center,
                new Vector3(width * 0.5f, -height * 0.5f, length * 0.5f) + center,
                new Vector3(narrow * 0.5f - 0.1f, height * 0.5f, length * 0.5f - 0.15f) + center,
                new Vector3(-narrow * 0.5f + 0.1f, height * 0.5f, length * 0.5f - 0.15f) + center
            );
            
            // Cabin rear
            b.AddQuad(
                new Vector3(-width * 0.5f, -height * 0.5f, -length * 0.5f) + center,
                new Vector3(width * 0.5f, -height * 0.5f, -length * 0.5f) + center,
                new Vector3(narrow * 0.5f, height * 0.5f, -length * 0.5f) + center,
                new Vector3(-narrow * 0.5f, height * 0.5f, -length * 0.5f) + center
            );
        }
        
        static void AddGlacisPlate(LowPolyMeshBuilder b, float width, float height, Vector3 center) {
            // Angled front armor plate
            float slope = 0.3f;
            b.AddQuad(
                new Vector3(-width * 0.5f, -height * 0.5f, 0) + center,
                new Vector3(width * 0.5f, -height * 0.5f, 0) + center,
                new Vector3(width * 0.5f - slope, height * 0.5f, slope) + center,
                new Vector3(-width * 0.5f + slope, height * 0.5f, slope) + center
            );
        }
        
        static void AddEngineDeck(LowPolyMeshBuilder b, float width, float height, float length, Vector3 center) {
            // Grilled engine deck
            b.AddQuad(
                new Vector3(-width * 0.5f, -height * 0.5f, -length * 0.5f) + center,
                new Vector3(width * 0.5f, -height * 0.5f, -length * 0.5f) + center,
                new Vector3(width * 0.5f, -height * 0.5f, length * 0.5f) + center,
                new Vector3(-width * 0.5f, -height * 0.5f, length * 0.5f) + center
            );
            
            // Side panels
            for (int side = -1; side <= 1; side += 2) {
                b.AddQuad(
                    new Vector3(width * 0.5f * side, -height * 0.5f, -length * 0.5f) + center,
                    new Vector3(width * 0.5f * side, height * 0.5f, -length * 0.5f) + center,
                    new Vector3(width * 0.5f * side, height * 0.5f, length * 0.5f) + center,
                    new Vector3(width * 0.5f * side, -height * 0.5f, length * 0.5f) + center
                );
            }
            
            // Top grill lines
            for (int i = 0; i < 4; i++) {
                float z = -length * 0.3f + (float)i / 3f * length * 0.6f;
                b.AddQuad(
                    new Vector3(-width * 0.3f, height * 0.5f, z) + center,
                    new Vector3(width * 0.3f, height * 0.5f, z) + center,
                    new Vector3(width * 0.3f, height * 0.5f, z + 0.05f) + center,
                    new Vector3(-width * 0.3f, height * 0.5f, z + 0.05f) + center
                );
            }
        }
        
        static void AddDetailedWheel(LowPolyMeshBuilder b, float radius, float width, Vector3 center, bool leftSide) {
            int segments = 16;
            float angleStep = Mathf.PI * 2f / segments;
            
            // Tire outer surface
            for (int i = 0; i < segments; i++) {
                float angle1 = i * angleStep;
                float angle2 = (i + 1) * angleStep;
                
                float x1 = Mathf.Cos(angle1) * radius;
                float y1 = Mathf.Sin(angle1) * radius;
                float x2 = Mathf.Cos(angle2) * radius;
                float y2 = Mathf.Sin(angle2) * radius;
                
                float zOffset = width * 0.5f * (leftSide ? -1 : 1);
                
                // Outer tire face
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
            
            // Tread pattern ridges
            int treadRidges = 6;
            for (int r = 0; r < treadRidges; r++) {
                float ridgeZ = -width * 0.4f + (float)r / (treadRidges - 1) * width * 0.8f;
                for (int i = 0; i < segments; i += 2) {
                    float angle = i * angleStep;
                    float x = Mathf.Cos(angle) * (radius + 0.02f);
                    float y = Mathf.Sin(angle) * (radius + 0.02f);
                    
                    b.AddQuad(
                        new Vector3(x, y, ridgeZ - 0.02f) + center,
                        new Vector3(x, y, ridgeZ + 0.02f) + center,
                        new Vector3(Mathf.Cos((i + 1) * angleStep) * (radius + 0.02f), Mathf.Sin((i + 1) * angleStep) * (radius + 0.02f), ridgeZ + 0.02f) + center,
                        new Vector3(Mathf.Cos((i + 1) * angleStep) * (radius + 0.02f), Mathf.Sin((i + 1) * angleStep) * (radius + 0.02f), ridgeZ - 0.02f) + center
                    );
                }
            }
        }
        
        static void AddWheelArches(LowPolyMeshBuilder b, float width, float height, float length, float xPos) {
            // Simplified fender/wheel arch coverage
            float[] wheelZPositions = new float[] { -1.8f, -0.9f, 0f, 0.9f, 1.8f, 2.7f };
            
            foreach (float z in wheelZPositions) {
                // Small arch segment over each wheel
                int archSegments = 4;
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
        
        static void AddWeaponMount(LowPolyMeshBuilder b, float width, float height, float length, Vector3 center) {
            // Base ring
            int segments = 12;
            float angleStep = Mathf.PI * 2f / segments;
            float radius = width * 0.5f;
            
            for (int i = 0; i < segments; i++) {
                float angle1 = i * angleStep;
                float angle2 = (i + 1) * angleStep;
                
                float x1 = Mathf.Cos(angle1) * radius;
                float z1 = Mathf.Sin(angle1) * radius;
                float x2 = Mathf.Cos(angle2) * radius;
                float z2 = Mathf.Sin(angle2) * radius;
                
                b.AddQuad(
                    new Vector3(x1, -height * 0.5f, z1) + center,
                    new Vector3(x2, -height * 0.5f, z2) + center,
                    new Vector3(x2, height * 0.5f, z2) + center,
                    new Vector3(x1, height * 0.5f, z1) + center
                );
            }
            
            // Gun barrel
            float barrelLength = 2f;
            float barrelRadius = 0.1f;
            b.AddCylinder(barrelRadius, barrelLength, 8, new Vector3(0, 0, barrelLength * 0.5f) + center);
            
            // Mantlet/armor around gun
            b.AddBox(width * 0.6f, height * 0.4f, 0.3f, new Vector3(0, 0, 0.2f) + center);
        }
        
        static void AddSensorBlock(LowPolyMeshBuilder b, float width, float height, float depth, Vector3 center) {
            b.AddBox(width, height, depth, center);
        }
        
        static void AddExhaustVents(LowPolyMeshBuilder b, float width, float height, Vector3 center) {
            // Vertical exhaust stack
            b.AddCylinder(width * 0.5f, height, 8, center);
            
            // Vent slats
            for (int i = 0; i < 3; i++) {
                float y = -height * 0.3f + (float)i / 2f * height * 0.6f;
                b.AddQuad(
                    new Vector3(-width * 0.4f, y, center.z + 0.02f),
                    new Vector3(width * 0.4f, y, center.z + 0.02f),
                    new Vector3(width * 0.4f, y, center.z - 0.02f),
                    new Vector3(-width * 0.4f, y, center.z - 0.02f)
                );
            }
        }
        
        static void AddHeadlight(LowPolyMeshBuilder b, float width, float height, Vector3 center) {
            // Light housing
            b.AddCylinder(width, 0.1f, 8, center);
            
            // Lens (slightly protruding)
            b.AddQuad(
                new Vector3(-width * 0.8f, -height * 0.8f, center.z + 0.05f),
                new Vector3(width * 0.8f, -height * 0.8f, center.z + 0.05f),
                new Vector3(width * 0.8f, height * 0.8f, center.z + 0.05f),
                new Vector3(-width * 0.8f, height * 0.8f, center.z + 0.05f)
            );
        }
    }
}
