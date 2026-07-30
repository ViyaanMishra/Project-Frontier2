using UnityEngine;
namespace Frontier.MeshGen.UI {
    public static class BuildMenuGen {
        public static Sprite GenerateCategoryIcon(int size = 64) {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0.2f, 0.2f, 0.2f);
            tex.SetPixels(pixels); tex.Apply();
            return Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f));
        }
    }
}
