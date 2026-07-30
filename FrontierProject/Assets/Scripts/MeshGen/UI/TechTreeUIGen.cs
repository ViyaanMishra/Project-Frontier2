using UnityEngine;
namespace Frontier.MeshGen.UI {
    public static class TechTreeUIGen {
        public static Sprite GenerateNode(int size = 64, bool unlocked = false) {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            Color col = unlocked ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.3f, 0.3f, 0.3f);
            for (int i = 0; i < pixels.Length; i++) {
                float d = Mathf.Sqrt((i%size-size/2)*(i%size-size/2) + (i/size-size/2)*(i/size-size/2));
                pixels[i] = (d < size/2) ? col : Color.clear;
            }
            tex.SetPixels(pixels); tex.Apply();
            return Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f));
        }
    }
}
