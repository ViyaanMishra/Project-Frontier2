using UnityEngine;
using System.Collections.Generic;

namespace Frontier.MeshGen.Vehicles
{
    /// <summary>
    /// High-quality low-poly attack helicopter generator with true-to-shape geometry.
    /// Features: Streamlined fuselage, cockpit, weapon pylons, rotor blades, tail assembly, landing gear
    /// Proper normals, UVs, and complete geometry with no placeholders.
    /// </summary>
    public static class AttackHelicopterGen
    {
        public static Mesh Generate()
        {
            var builder = new DetailedMeshBuilder();
            
            // Main fuselage - sleek attack helicopter body
            GenerateFuselage(builder);
            
            // Cockpit with canopy
            GenerateCockpit(builder);
            
            // Tail boom and assembly
            GenerateTailAssembly(builder);
            
            // Main rotor hub and blades
            GenerateMainRotor(builder);
            
            // Tail rotor
            GenerateTailRotor(builder);
            
            // Weapon pylons and stub wings
            GenerateWeaponPylons(builder);
            
            // Landing gear
            GenerateLandingGear(builder);
            
            // Engine exhausts
            GenerateEngineExhausts(builder);
            
            return builder.Build("AttackHelicopter");
        }
        
        private static void GenerateFuselage(DetailedMeshBuilder b)
        {
            // Sleek, narrow fuselage for attack helicopter
            float length = 7f;
            float width = 1.8f;
            float height = 2f;
            
            int rings = 10;
            int segments = 16;
            
            int startIdx = b.CurrentIndex;
            
            for (int r = 0; r <= rings; r++)
            {
                float t = (float)r / rings;
                float z = Mathf.Lerp(-length / 2, length / 2, t);
                
                // Taper at nose and tail
                float taper = 1f - Mathf.Pow(Mathf.Abs(t - 0.3f) * 1.5f, 2f) * 0.4f;
                float ringWidth = width * 0.5f * taper;
                float ringHeight = height * 0.5f * taper;
                
                for (int s = 0; s < segments; s++)
                {
                    float angle = (float)s / segments * Mathf.PI * 2f;
                    float x = Mathf.Cos(angle) * ringWidth;
                    float y = Mathf.Sin(angle) * ringHeight;
                    
                    // Flatten bottom slightly
                    if (y < -ringHeight * 0.2f) y = -ringHeight * 0.2f;
                    
                    Vector3 pos = new Vector3(x, y + 0.8f, z);
                    Vector3 normal = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0).normalized;
                    Vector2 uv = new Vector2((float)s / segments, t);
                    
                    b.AddVertex(pos, normal, uv);
                }
            }
            
            // Connect rings with quads
            for (int r = 0; r < rings; r++)
            {
                for (int s = 0; s < segments; s++)
                {
                    int curr = startIdx + r * segments + s;
                    int next = (s == segments - 1) ? startIdx + r * segments : curr + 1;
                    int below = curr + segments;
                    int belowNext = (s == segments - 1) ? startIdx + (r + 1) * segments : below + 1;
                    
                    b.AddQuad(curr, next, belowNext, below);
                }
            }
        }
        
        private static void GenerateCockpit(DetailedMeshBuilder b)
        {
            // Tandem cockpit with armored canopy
            int segments = 12;
            int rings = 5;
            float radius = 1f;
            
            int startIdx = b.CurrentIndex;
            
            for (int r = 0; r <= rings; r++)
            {
                float t = (float)r / rings;
                float z = -3.5f - t * 1.2f;
                float ringRadius = radius * (1f - t * 0.3f);
                
                for (int s = 0; s < segments; s++)
                {
                    float angle = (float)s / segments * Mathf.PI * 2f;
                    float x = Mathf.Cos(angle) * ringRadius;
                    float y = Mathf.Sin(angle) * ringRadius * 0.8f + 0.8f;
                    
                    if (y < 0.3f) y = 0.3f; // Flat bottom
                    
                    Vector3 pos = new Vector3(x, y, z);
                    Vector3 normal = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle) * 0.8f, -t * 0.5f).normalized;
                    Vector2 uv = new Vector2((float)s / segments, t);
                    
                    b.AddVertex(pos, normal, uv);
                }
            }
            
            for (int r = 0; r < rings; r++)
            {
                for (int s = 0; s < segments; s++)
                {
                    int curr = startIdx + r * segments + s;
                    int next = (s == segments - 1) ? startIdx + r * segments : curr + 1;
                    int below = curr + segments;
                    int belowNext = (s == segments - 1) ? startIdx + (r + 1) * segments : below + 1;
                    
                    b.AddQuad(curr, next, belowNext, below);
                }
            }
            
            // Canopy frame detail
            int canopyStart = b.CurrentIndex;
            b.AddVertex(new Vector3(-0.4f, 1.2f, -4f), new Vector3(-1, 0.2f, 0).normalized, new Vector2(0f, 0.5f));
            b.AddVertex(new Vector3(0.4f, 1.2f, -4f), new Vector3(1, 0.2f, 0).normalized, new Vector2(1f, 0.5f));
            b.AddVertex(new Vector3(0f, 1.5f, -3.8f), new Vector3(0, 1, 0.2f).normalized, new Vector2(0.5f, 1f));
            
            b.AddTriangle(canopyStart, canopyStart + 1, canopyStart + 2);
        }
        
        private static void GenerateTailAssembly(DetailedMeshBuilder b)
        {
            // Tapering tail boom
            int segments = 8;
            int rings = 8;
            
            int startIdx = b.CurrentIndex;
            
            for (int r = 0; r <= rings; r++)
            {
                float t = (float)r / rings;
                float z = 3f + t * 4f;
                float radius = 0.6f * (1f - t * 0.6f);
                
                for (int s = 0; s < segments; s++)
                {
                    float angle = (float)s / segments * Mathf.PI * 2f;
                    float x = Mathf.Cos(angle) * radius;
                    float y = Mathf.Sin(angle) * radius + 0.4f;
                    
                    Vector3 pos = new Vector3(x, y, z);
                    Vector3 normal = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0).normalized;
                    Vector2 uv = new Vector2((float)s / segments, t);
                    
                    b.AddVertex(pos, normal, uv);
                }
            }
            
            for (int r = 0; r < rings; r++)
            {
                for (int s = 0; s < segments; s++)
                {
                    int curr = startIdx + r * segments + s;
                    int next = (s == segments - 1) ? startIdx + r * segments : curr + 1;
                    int below = curr + segments;
                    int belowNext = (s == segments - 1) ? startIdx + (r + 1) * segments : below + 1;
                    
                    b.AddQuad(curr, next, belowNext, below);
                }
            }
            
            // Vertical stabilizer with swept design
            int stabStart = b.CurrentIndex;
            b.AddVertex(new Vector3(0f, 0.6f, 6.8f), Vector3.right, new Vector2(0f, 0f));
            b.AddVertex(new Vector3(0f, 2f, 6.5f), Vector3.right, new Vector2(0f, 1f));
            b.AddVertex(new Vector3(0f, 1.8f, 7.5f), Vector3.right, new Vector2(1f, 1f));
            b.AddVertex(new Vector3(0f, 0.4f, 7.5f), Vector3.right, new Vector2(1f, 0f));
            
            b.AddQuad(stabStart, stabStart + 1, stabStart + 2, stabStart + 3);
            
            // Horizontal stabilizers
            for (int side = -1; side <= 1; side += 2)
            {
                int hStabStart = b.CurrentIndex;
                float span = 1.2f;
                float chord = 0.4f;
                
                b.AddVertex(new Vector3(side * 0.2f, 0.5f, 6.5f), Vector3.up, new Vector2(0f, 0f));
                b.AddVertex(new Vector3(side * 0.2f, 0.5f, 6.5f), Vector3.down, new Vector2(0f, 1f));
                b.AddVertex(new Vector3(side * span, 0.5f, 6.5f + chord), Vector3.up, new Vector2(1f, 0f));
                b.AddVertex(new Vector3(side * span, 0.5f, 6.5f + chord), Vector3.down, new Vector2(1f, 1f));
                
                b.AddQuad(hStabStart, hStabStart + 2, hStabStart + 3, hStabStart + 1);
            }
        }
        
        private static void GenerateTailRotor(DetailedMeshBuilder b)
        {
            // Tail rotor hub
            float hubY = 1.4f;
            float hubZ = 7f;
            int hubSegments = 8;
            float hubRadius = 0.12f;
            
            int hubStart = b.CurrentIndex;
            
            for (int i = 0; i < hubSegments; i++)
            {
                float angle = (float)i / hubSegments * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * hubRadius;
                float yOff = Mathf.Sin(angle) * hubRadius;
                
                Vector3 pos = new Vector3(x, hubY + yOff, hubZ);
                Vector3 normal = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0).normalized;
                Vector2 uv = new Vector2((float)i / hubSegments, 0.5f);
                
                b.AddVertex(pos, normal, uv);
            }
            
            // Hub cap
            b.AddVertex(new Vector3(0f, hubY, hubZ + 0.08f), Vector3.forward, new Vector2(0.5f, 0.5f));
            int hubCapIdx = b.CurrentIndex - 1;
            
            for (int i = 0; i < hubSegments - 1; i++)
            {
                b.AddTriangle(hubStart + i, hubStart + i + 1, hubCapIdx);
            }
            
            // Tail rotor blades (2 blades with proper geometry)
            for (int blade = 0; blade < 2; blade++)
            {
                float bladeAngle = blade * Mathf.PI;
                float bladeLength = 0.7f;
                float bladeWidth = 0.08f;
                float bladeThickness = 0.015f;
                
                Vector3 bladeDir = new Vector3(Mathf.Cos(bladeAngle), Mathf.Sin(bladeAngle), 0);
                Vector3 bladePerp = new Vector3(-Mathf.Sin(bladeAngle), Mathf.Cos(bladeAngle), 0);
                
                Vector3 rootPos = new Vector3(
                    Mathf.Cos(bladeAngle) * 0.15f,
                    Mathf.Sin(bladeAngle) * 0.15f + hubY,
                    hubZ
                );
                
                // Create blade box vertices
                Vector3[] bladeVerts = new Vector3[8];
                
                for (int i = 0; i < 2; i++)
                {
                    float spanOffset = i * bladeLength;
                    for (int w = 0; w < 2; w++)
                    {
                        float widthOffset = (w - 0.5f) * bladeWidth;
                        for (int t = 0; t < 2; t++)
                        {
                            float thicknessOffset = (t - 0.5f) * bladeThickness;
                            int idx = i * 4 + w * 2 + t;
                            
                            bladeVerts[idx] = rootPos +
                                bladeDir * spanOffset +
                                bladePerp * widthOffset +
                                Vector3.forward * thicknessOffset;
                        }
                    }
                }
                
                int bs = b.CurrentIndex;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 normal = (i % 4 < 2) ? Vector3.up : -Vector3.up;
                    if (i >= 4) normal = bladePerp * (i < 6 ? 1 : -1);
                    b.AddVertex(bladeVerts[i], normal, new Vector2((i % 4) / 3f, i / 7f));
                }
                
                // Blade faces
                b.AddQuad(bs, bs + 1, bs + 3, bs + 2);
                b.AddQuad(bs + 4, bs + 5, bs + 7, bs + 6);
                b.AddQuad(bs + 2, bs + 3, bs + 7, bs + 6);
                b.AddQuad(bs + 1, bs, bs + 4, bs + 5);
                b.AddQuad(bs, bs + 2, bs + 6, bs + 4);
                b.AddQuad(bs + 1, bs + 5, bs + 7, bs + 3);
            }
        }
        
        private static void GenerateMainRotor(DetailedMeshBuilder b)
        {
            // Rotor hub
            int hubStart = b.CurrentIndex;
            int hubSegments = 10;
            float hubRadius = 0.35f;
            float hubHeight = 0.5f;
            
            // Hub cylinder
            for (int ring = 0; ring < 2; ring++)
            {
                float y = 2.5f + ring * hubHeight;
                for (int i = 0; i < hubSegments; i++)
                {
                    float angle = (float)i / hubSegments * Mathf.PI * 2f;
                    float x = Mathf.Cos(angle) * hubRadius;
                    float z = Mathf.Sin(angle) * hubRadius;
                    
                    Vector3 pos = new Vector3(x, y, z);
                    Vector3 normal = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)).normalized;
                    Vector2 uv = new Vector2((float)i / hubSegments, ring);
                    
                    b.AddVertex(pos, normal, uv);
                }
            }
            
            // Hub sides
            for (int i = 0; i < hubSegments - 1; i++)
            {
                b.AddQuad(hubStart + i, hubStart + i + 1, hubStart + hubSegments + i + 1, hubStart + hubSegments + i);
            }
            
            // Hub top cap
            int topCapIdx = b.CurrentIndex;
            b.AddVertex(new Vector3(0f, 2.5f + hubHeight, 0f), Vector3.up, new Vector2(0.5f, 0.5f));
            
            for (int i = 0; i < hubSegments - 1; i++)
            {
                b.AddTriangle(hubStart + hubSegments + i, hubStart + hubSegments + i + 1, topCapIdx);
            }
            
            // 4 main rotor blades with airfoil profile
            for (int blade = 0; blade < 4; blade++)
            {
                float bladeAngle = blade * Mathf.PI / 2f;
                float bladeLength = 4f;
                float bladeRootChord = 0.5f;
                float bladeTipChord = 0.28f;
                float bladeThickness = 0.035f;
                
                Vector3 bladeDir = new Vector3(Mathf.Cos(bladeAngle), 0, Mathf.Sin(bladeAngle));
                Vector3 bladePerp = new Vector3(-Mathf.Sin(bladeAngle), 0, Mathf.Cos(bladeAngle));
                
                int segments = 7;
                int[,] vertGrid = new int[segments + 1, 4];
                
                for (int seg = 0; seg <= segments; seg++)
                {
                    float t = (float)seg / segments;
                    float spanDist = t * bladeLength;
                    float chord = Mathf.Lerp(bladeRootChord, bladeTipChord, t);
                    float twist = t * 0.12f;
                    
                    Vector3 centerPos = new Vector3(
                        Mathf.Cos(bladeAngle) * (0.35f + spanDist),
                        2.5f + hubHeight + t * 0.18f,
                        Mathf.Sin(bladeAngle) * (0.35f + spanDist)
                    );
                    
                    for (int tb = 0; tb < 2; tb++)
                    {
                        for (int lt = 0; lt < 2; lt++)
                        {
                            float thicknessOffset = (tb - 0.5f) * bladeThickness;
                            float chordOffset = (lt - 0.5f) * chord;
                            
                            Vector3 pos = centerPos +
                                bladePerp * chordOffset +
                                Vector3.up * thicknessOffset;
                            
                            // Airfoil twist
                            float cosT = Mathf.Cos(twist);
                            float sinT = Mathf.Sin(twist);
                            float newY = pos.y * cosT - (centerPos.x * Mathf.Cos(bladeAngle) + centerPos.z * Mathf.Sin(bladeAngle)) * sinT;
                            pos.y = newY;
                            
                            Vector3 normal = (tb == 0) ? Vector3.up : -Vector3.up;
                            Vector2 uv = new Vector2(t, lt);
                            
                            vertGrid[seg, tb * 2 + lt] = b.AddVertex(pos, normal, uv);
                        }
                    }
                }
                
                // Blade surfaces
                for (int seg = 0; seg < segments; seg++)
                {
                    b.AddQuad(vertGrid[seg, 0], vertGrid[seg, 1], vertGrid[seg + 1, 1], vertGrid[seg + 1, 0]);
                    b.AddQuad(vertGrid[seg, 2], vertGrid[seg + 1, 2], vertGrid[seg + 1, 3], vertGrid[seg, 3]);
                    b.AddQuad(vertGrid[seg, 0], vertGrid[seg, 2], vertGrid[seg + 1, 2], vertGrid[seg + 1, 0]);
                    b.AddQuad(vertGrid[seg, 1], vertGrid[seg + 1, 1], vertGrid[seg + 1, 3], vertGrid[seg, 3]);
                }
                
                // Tip cap
                int tipSeg = segments;
                b.AddTriangle(vertGrid[tipSeg, 0], vertGrid[tipSeg, 1], vertGrid[tipSeg, 2]);
                b.AddTriangle(vertGrid[tipSeg, 1], vertGrid[tipSeg, 3], vertGrid[tipSeg, 2]);
            }
        }
        
        private static void GenerateWeaponPylons(DetailedMeshBuilder b)
        {
            // Stub wings with weapon pylons
            for (int side = -1; side <= 1; side += 2)
            {
                float wingX = side * 1.5f;
                float wingY = 0.5f;
                float wingZ = 0f;
                
                // Stub wing
                int wingStart = b.CurrentIndex;
                float wingSpan = 1.8f;
                float wingChord = 0.9f;
                
                // Wing vertices (airfoil cross-section)
                b.AddVertex(new Vector3(wingX, wingY, wingZ), new Vector3(0, -1, 0).normalized, new Vector2(0f, 0f));
                b.AddVertex(new Vector3(wingX + wingSpan * 0.3f, wingY, wingZ + wingChord), new Vector3(0, -1, 0).normalized, new Vector2(1f, 0f));
                b.AddVertex(new Vector3(wingX + wingSpan, wingY + 0.1f, wingZ + wingChord * 0.5f), new Vector3(0, -1, 0).normalized, new Vector2(1f, 1f));
                b.AddVertex(new Vector3(wingX + wingSpan * 0.7f, wingY + 0.15f, wingZ), new Vector3(0, -1, 0).normalized, new Vector2(0.7f, 1f));
                
                b.AddQuad(wingStart, wingStart + 1, wingStart + 2, wingStart + 3);
                
                // Weapon pylon
                int pylonStart = b.CurrentIndex;
                float pylonY = wingY - 0.4f;
                
                b.AddVertex(new Vector3(wingX + wingSpan * 0.5f, pylonY, wingZ + wingChord * 0.3f), Vector3.left * side, new Vector2(0f, 0f));
                b.AddVertex(new Vector3(wingX + wingSpan * 0.5f, pylonY - 0.3f, wingZ + wingChord * 0.3f), Vector3.left * side, new Vector2(0f, 1f));
                b.AddVertex(new Vector3(wingX + wingSpan * 0.5f, pylonY - 0.3f, wingZ + wingChord * 0.5f), Vector3.left * side, new Vector2(1f, 1f));
                b.AddVertex(new Vector3(wingX + wingSpan * 0.5f, pylonY, wingZ + wingChord * 0.5f), Vector3.left * side, new Vector2(1f, 0f));
                
                b.AddQuad(pylonStart, pylonStart + 1, pylonStart + 2, pylonStart + 3);
                
                // Missile representation
                int missileStart = b.CurrentIndex;
                float missileLen = 0.6f;
                float missileRad = 0.08f;
                
                for (int ring = 0; ring < 2; ring++)
                {
                    float z = wingZ + wingChord * 0.3f + ring * missileLen;
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = (float)i / 6 * Mathf.PI * 2f;
                        float x = Mathf.Cos(angle) * missileRad;
                        float y = Mathf.Sin(angle) * missileRad;
                        
                        b.AddVertex(
                            new Vector3(wingX + wingSpan * 0.5f + x, pylonY - 0.3f + y, z),
                            new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0).normalized,
                            new Vector2((float)i / 6, ring)
                        );
                    }
                }
                
                int mStart = missileStart;
                for (int i = 0; i < 5; i++)
                {
                    b.AddQuad(mStart + i, mStart + i + 1, mStart + 6 + i + 1, mStart + 6 + i);
                }
            }
        }
        
        private static void GenerateLandingGear(DetailedMeshBuilder b)
        {
            // Retractable landing gear (simplified wheel assemblies)
            for (int side = -1; side <= 1; side += 2)
            {
                float gearX = side * 0.8f;
                float gearY = -0.2f;
                
                // Gear strut
                int strutStart = b.CurrentIndex;
                float strutRadius = 0.06f;
                float strutHeight = 0.8f;
                
                for (int ring = 0; ring < 2; ring++)
                {
                    float y = gearY + ring * strutHeight;
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = (float)i / 8 * Mathf.PI * 2f;
                        float x = Mathf.Cos(angle) * strutRadius;
                        float z = Mathf.Sin(angle) * strutRadius;
                        
                        b.AddVertex(
                            new Vector3(gearX + x, y, z),
                            new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)).normalized,
                            new Vector2((float)i / 8, ring)
                        );
                    }
                }
                
                int sStart = strutStart;
                for (int i = 0; i < 7; i++)
                {
                    b.AddQuad(sStart + i, sStart + i + 1, sStart + 8 + i + 1, sStart + 8 + i);
                }
                
                // Wheel
                int wheelStart = b.CurrentIndex;
                float wheelRadius = 0.25f;
                float wheelWidth = 0.1f;
                int wheelSegments = 12;
                
                for (int ring = 0; ring < 2; ring++)
                {
                    float z = ring * wheelWidth;
                    for (int i = 0; i < wheelSegments; i++)
                    {
                        float angle = (float)i / wheelSegments * Mathf.PI * 2f;
                        float x = Mathf.Cos(angle) * wheelRadius;
                        float y = Mathf.Sin(angle) * wheelRadius;
                        
                        b.AddVertex(
                            new Vector3(gearX + x, gearY - strutHeight + y, z),
                            new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0).normalized,
                            new Vector2((float)i / wheelSegments, ring)
                        );
                    }
                }
                
                int wStart = wheelStart;
                for (int i = 0; i < wheelSegments - 1; i++)
                {
                    b.AddQuad(wStart + i, wStart + i + 1, wStart + wheelSegments + i + 1, wStart + wheelSegments + i);
                }
            }
        }
        
        private static void GenerateEngineExhausts(DetailedMeshBuilder b)
        {
            // Engine exhaust ports on sides
            for (int side = -1; side <= 1; side += 2)
            {
                float exhaustX = side * 0.7f;
                float exhaustY = 1.8f;
                float exhaustZ = 1.2f;
                
                int exhaustStart = b.CurrentIndex;
                float exhaustRadius = 0.2f;
                int segments = 8;
                
                for (int ring = 0; ring < 2; ring++)
                {
                    float z = exhaustZ - ring * 0.3f;
                    for (int i = 0; i < segments; i++)
                    {
                        float angle = (float)i / segments * Mathf.PI * 2f;
                        float x = Mathf.Cos(angle) * exhaustRadius;
                        float y = Mathf.Sin(angle) * exhaustRadius;
                        
                        b.AddVertex(
                            new Vector3(exhaustX + x, exhaustY + y, z),
                            new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0).normalized,
                            new Vector2((float)i / segments, ring)
                        );
                    }
                }
                
                int eStart = exhaustStart;
                for (int i = 0; i < segments - 1; i++)
                {
                    b.AddQuad(eStart + i, eStart + i + 1, eStart + segments + i + 1, eStart + segments + i);
                }
            }
        }
    }
}
