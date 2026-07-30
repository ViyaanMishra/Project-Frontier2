using UnityEngine;
namespace Frontier.MeshGen.UI {
    public static class InventoryUIGen {
        public static Sprite GenerateSlot(int size = 64) {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0.15f, 0.15f, 0.15f, 0.8f);
            for (int x = 0; x < size; x++) { pixels[x] = Color.white; pixels[(size-1)*size+x] = Color.white; }
            for (int y = 0; y < size; y++) { pixels[y*size] = Color.white; pixels[y*size+size-1] = Color.white; }
            tex.SetPixels(pixels); tex.Apply();
            return Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f));
        }
    }
}
