using UnityEngine;

namespace Frontier.MeshGen.TexGen
{
    /// <summary>
    /// Builds texture atlases by packing multiple textures into one.
    /// Automatically remaps UVs for efficient batching.
    /// </summary>
    public class TextureAtlasBuilder
    {
        private class AtlasEntry
        {
            public Texture2D texture;
            public Rect uvRect;
            public string name;
        }
        
        private readonly System.Collections.Generic.List<AtlasEntry> _entries = new();
        private int _atlasSize = 2048;
        private int _padding = 4;
        
        public void SetAtlasSize(int size) => _atlasSize = size;
        public void SetPadding(int padding) => _padding = padding;
        
        /// <summary>
        /// Add a texture to be packed into the atlas.
        /// </summary>
        public void AddTexture(Texture2D texture, string name)
        {
            _entries.Add(new AtlasEntry
            {
                texture = texture,
                name = name,
                uvRect = Rect.zero
            });
        }
        
        /// <summary>
        /// Pack all added textures into a single atlas.
        /// Returns the atlas texture and a mapping of names to UV rects.
        /// </summary>
        public (Texture2D atlas, System.Collections.Generic.Dictionary<string, Rect> uvMap) Build(string atlasName = "TextureAtlas")
        {
            if (_entries.Count == 0)
            {
                Debug.LogWarning("TextureAtlasBuilder: No textures to pack");
                return (null, new System.Collections.Generic.Dictionary<string, Rect>());
            }
            
            // Sort by size (largest first) for better packing
            _entries.Sort((a, b) => b.texture.width.CompareTo(a.texture.width));
            
            var atlas = new Texture2D(_atlasSize, _atlasSize, TextureFormat.RGBA32, true);
            var uvMap = new System.Collections.Generic.Dictionary<string, Rect>();
            
            // Simple bin packing (could be optimized with more complex algorithms)
            int currentX = _padding;
            int currentY = _atlasSize - _padding;
            int rowHeight = 0;
            
            foreach (var entry in _entries)
            {
                Texture2D tex = entry.texture;
                
                // Check if we need to start a new row
                if (currentX + tex.width + _padding > _atlasSize)
                {
                    currentX = _padding;
                    currentY -= rowHeight + _padding * 2;
                    rowHeight = 0;
                }
                
                if (currentY - tex.height - _padding < 0)
                {
                    Debug.LogWarning($"TextureAtlasBuilder: Atlas full, skipping {entry.name}");
                    continue;
                }
                
                // Copy texture pixels to atlas
                Color[] pixels = tex.GetPixels();
                atlas.SetPixels(currentX, currentY - tex.height, tex.width, tex.height, pixels);
                
                // Calculate UV rect
                float uMin = (float)currentX / _atlasSize;
                float vMin = (float)(currentY - tex.height) / _atlasSize;
                float uMax = (float)(currentX + tex.width) / _atlasSize;
                float vMax = (float)currentY / _atlasSize;
                
                entry.uvRect = new Rect(uMin, vMin, uMax - uMin, vMax - vMin);
                uvMap[entry.name] = entry.uvRect;
                
                // Update position for next texture
                currentX += tex.width + _padding * 2;
                rowHeight = Mathf.Max(rowHeight, tex.height);
            }
            
            atlas.Apply(true);
            atlas.name = atlasName;
            
            return (atlas, uvMap);
        }
        
        /// <summary>
        /// Remap UV coordinates from original texture space to atlas space.
        /// </summary>
        public static Vector2 RemapUV(Vector2 uv, Rect sourceRect, Rect atlasRect)
        {
            // Normalize within source rect
            float localU = (uv.x - sourceRect.x) / sourceRect.width;
            float localV = (uv.y - sourceRect.y) / sourceRect.height;
            
            // Map to atlas rect
            float atlasU = atlasRect.x + localU * atlasRect.width;
            float atlasV = atlasRect.y + localV * atlasRect.height;
            
            return new Vector2(atlasU, atlasV);
        }
        
        /// <summary>
        /// Clear all entries for reuse.
        /// </summary>
        public void Clear()
        {
            _entries.Clear();
        }
    }
}
