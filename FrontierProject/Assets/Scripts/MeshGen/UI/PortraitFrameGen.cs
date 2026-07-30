using UnityEngine;
namespace Frontier.MeshGen.UI {
    public static class PortraitFrameGen {
        public static Sprite GenerateFactionFrame(int size = 128, Color factionColor) {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) {
                float d = Mathf.Sqrt((x-size/2)*(x-size/2) + (y-size/2)*(y-size/2));
                if (d > size/2 - 4 && d < size/2) pixels[y*size+x] = factionColor;
                else if (d <= size/2 - 4) pixels[y*size+x] = new Color(0.1f,0.1f,0.1f);
            }
            tex.SetPixels(pixels); tex.Apply();
            return Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f));
        }
    }
}
