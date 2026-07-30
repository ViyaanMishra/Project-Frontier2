using UnityEngine;
namespace Frontier.MeshGen.Items {
    public static class IconGenerator {
        public static Texture2D RenderToIcon(Mesh mesh, int size = 64) {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
    }
}
