using UnityEngine;
namespace Frontier.MeshGen.UI {
    public static class VehicleUIGen {
        public static Sprite GenerateFuelGauge(int w = 100, int h = 30) {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[w * h];
            for (int x = 0; x < w; x++) for (int y = 0; y < h; y++) {
                float t = (float)x/w;
                pixels[y*w+x] = t > 0.8f ? Color.red : (t > 0.3f ? Color.yellow : Color.green);
            }
            tex.SetPixels(pixels); tex.Apply();
            return Sprite.Create(tex, new Rect(0,0,w,h), new Vector2(0.5f,0.5f));
        }
    }
}
