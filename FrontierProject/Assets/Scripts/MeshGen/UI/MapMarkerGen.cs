using UnityEngine;
namespace Frontier.MeshGen.UI {
    public static class MapMarkerGen {
        public static Sprite GeneratePlayerArrow(int size = 32) {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
            for (int y = 0; y < size/2; y++) {
                for (int x = size/2 - y; x <= size/2 + y; x++) {
                    if (x >= 0 && x < size) pixels[y*size+x] = Color.green;
                }
            }
            tex.SetPixels(pixels); tex.Apply();
            return Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f));
        }
        public static Sprite GeneratePOI(int size = 32) {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
            int c = size/2; for (int y = c-8; y <= c+8; y++) for (int x = c-8; x <= c+8; x++) if ((x-c)*(x-c)+(y-c)*(y-c) < 64) pixels[y*size+x] = Color.yellow;
            tex.SetPixels(pixels); tex.Apply();
            return Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f));
        }
    }
}
