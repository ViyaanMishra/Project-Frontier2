using UnityEngine;

namespace Frontier.MeshGen.TexGen
{
    /// <summary>
    /// Procedural pattern texture generator for low-poly materials.
    /// Generates hex grids, scales, bark, fabric, rivets, camo, brick patterns.
    /// </summary>
    public static class PatternTexture
    {
        /// <summary>
        /// Generate a hexagonal grid pattern.
        /// </summary>
        public static Texture2D CreateHexPattern(int size, Color baseColor, Color lineColor, float hexSize = 0.1f)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;
                    
                    // Hex grid calculation
                    float hexU = u / hexSize;
                    float hexV = v / (hexSize * Mathf.Sqrt(3) / 2);
                    
                    int hexX = Mathf.FloorToInt(hexU);
                    int hexY = Mathf.FloorToInt(hexV);
                    
                    float offsetX = (hexY % 2) * 0.5f;
                    float localU = (hexU - hexX - offsetX) * hexSize;
                    float localV = (hexV - hexY) * hexSize * Mathf.Sqrt(3) / 2;
                    
                    float dist = Mathf.Max(Mathf.Abs(localU), Mathf.Abs(localV));
                    float edgeDist = Mathf.Abs(dist - hexSize * 0.5f);
                    
                    if (edgeDist < 0.01f)
                        tex.SetPixel(x, y, lineColor);
                    else
                        tex.SetPixel(x, y, baseColor);
                }
            }
            
            tex.Apply();
            return tex;
        }
        
        /// <summary>
        /// Generate a brick pattern.
        /// </summary>
        public static Texture2D CreateBrickPattern(int size, Color brickColor, Color mortarColor, 
                                                   float brickWidth = 0.2f, float brickHeight = 0.1f)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;
                    
                    float brickRow = Mathf.Floor(v / brickHeight);
                    float rowOffset = (brickRow % 2) * (brickWidth * 0.5f);
                    
                    float brickCol = Mathf.Floor((u + rowOffset) / brickWidth);
                    
                    float localU = ((u + rowOffset) / brickWidth - brickCol) * brickWidth;
                    float localV = (v / brickHeight - brickRow) * brickHeight;
                    
                    float mortarSize = 0.02f;
                    bool isMortar = localU < mortarSize || localV < mortarSize || 
                                   localU > brickWidth - mortarSize || localV > brickHeight - mortarSize;
                    
                    tex.SetPixel(x, y, isMortar ? mortarColor : brickColor);
                }
            }
            
            tex.Apply();
            return tex;
        }
        
        /// <summary>
        /// Generate a camouflage pattern.
        /// </summary>
        public static Texture2D CreateCamoPattern(int size, Color[] camoColors, int blobCount = 50)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            
            // Fill with base color
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    tex.SetPixel(x, y, camoColors[0]);
            
            // Add blobs
            for (int b = 1; b < blobCount; b++)
            {
                Color blobColor = camoColors[b % camoColors.Length];
                Vector2 center = new Vector2(Random.value * size, Random.value * size);
                float radius = Random.Range(size * 0.05f, size * 0.2f);
                
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        Vector2 pos = new Vector2(x, y);
                        float dist = Vector2.Distance(pos, center);
                        
                        // Noise-based edge
                        float noise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                        float adjustedRadius = radius * (0.8f + 0.4f * noise);
                        
                        if (dist < adjustedRadius)
                            tex.SetPixel(x, y, blobColor);
                    }
                }
            }
            
            tex.Apply();
            return tex;
        }
        
        /// <summary>
        /// Generate a rivet pattern on metal plates.
        /// </summary>
        public static Texture2D CreateRivetPattern(int size, Color plateColor, Color rivetColor,
                                                    float plateWidth = 0.25f, float rivetSpacing = 0.08f)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;
                    
                    // Plate seams
                    float plateSeamX = Mathf.Abs(u % plateWidth - plateWidth * 0.5f);
                    float plateSeamY = Mathf.Abs(v % plateWidth - plateWidth * 0.5f);
                    
                    bool isSeam = plateSeamX < 0.01f || plateSeamY < 0.01f;
                    
                    // Rivets at intersections
                    bool isRivet = false;
                    if (!isSeam)
                    {
                        float rivX = u % rivetSpacing;
                        float rivY = v % rivetSpacing;
                        float rivDist = Mathf.Sqrt(rivX * rivX + rivY * rivY);
                        isRivet = rivDist < 0.015f;
                    }
                    
                    if (isRivet)
                        tex.SetPixel(x, y, rivetColor);
                    else if (isSeam)
                        tex.SetPixel(x, y, Color.Lerp(plateColor, rivetColor, 0.3f));
                    else
                        tex.SetPixel(x, y, plateColor);
                }
            }
            
            tex.Apply();
            return tex;
        }
        
        /// <summary>
        /// Generate a fabric/cloth weave pattern.
        /// </summary>
        public static Texture2D CreateFabricPattern(int size, Color baseColor, Color weaveColor, 
                                                     float weaveScale = 0.05f)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = x * weaveScale;
                    float v = y * weaveScale;
                    
                    // Weave pattern using sine waves
                    float horizontal = Mathf.Sin(u * Mathf.PI * 2);
                    float vertical = Mathf.Cos(v * Mathf.PI * 2);
                    
                    float weave = (horizontal + vertical) * 0.5f;
                    
                    Color pixel = Color.Lerp(baseColor, weaveColor, weave * 0.5f + 0.5f);
                    tex.SetPixel(x, y, pixel);
                }
            }
            
            tex.Apply();
            return tex;
        }
    }
}
