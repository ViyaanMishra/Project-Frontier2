using UnityEngine;
namespace Frontier.MeshGen.UI {
    public static class CrosshairGen {
        public static Texture2D GenerateDefault(int size = 32) {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
            int c = size / 2;
            for (int i = c - 2; i <= c + 2; i++) {
                pixels[c * size + i] = Color.white;
                pixels[i * size + c] = Color.white;
            }
            tex.SetPixels(pixels); tex.Apply(); return tex;
        }
        public static Texture2D GenerateDot(int size = 16) {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
            pixels[(size/2) * size + (size/2)] = Color.red;
            tex.SetPixels(pixels); tex.Apply(); return tex;
        }
    }
}
