using UnityEngine;
namespace Frontier.MeshGen.UI {
    public static class CraftingUIGen {
        public static Sprite GenerateRecipeCard(int w = 300, int h = 100) {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0.15f, 0.12f, 0.1f);
            tex.SetPixels(pixels); tex.Apply();
            return Sprite.Create(tex, new Rect(0,0,w,h), new Vector2(0.5f,0.5f));
        }
    }
}
