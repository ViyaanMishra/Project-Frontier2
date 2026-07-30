using UnityEngine;
namespace Frontier.MeshGen.UI {
    public static class StatusIconGen {
        public static Texture2D GenerateBurning(int size = 32) { return GenerateIcon(size, Color.red, "fire"); }
        public static Texture2D GenerateFrozen(int size = 32) { return GenerateIcon(size, Color.cyan, "ice"); }
        public static Texture2D GeneratePoisoned(int size = 32) { return GenerateIcon(size, Color.green, "skull"); }
        public static Texture2D GenerateRadiation(int size = 32) { return GenerateIcon(size, new Color(0,1,0), "rad"); }
        private static Texture2D GenerateIcon(int size, Color col, string type) {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(col.r, col.g, col.b, 0.2f);
            tex.SetPixels(pixels); tex.Apply(); return tex;
        }
    }
}
