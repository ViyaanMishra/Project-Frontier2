using UnityEngine;
namespace Frontier.MeshGen.UI {
    public static class DiplomacyUIGen {
        public static Sprite GenerateRelationMeter(int w = 200, int h = 20) {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[w * h];
            for (int x = 0; x < w; x++) {
                float t = (float)x / w;
                Color col = t < 0.5f ? Color.red : (t > 0.75f ? Color.green : Color.yellow);
                for (int y = 0; y < h; y++) pixels[y*w+x] = col;
            }
            tex.SetPixels(pixels); tex.Apply();
            return Sprite.Create(tex, new Rect(0,0,w,h), new Vector2(0.5f,0.5f));
        }
    }
}
