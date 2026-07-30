using UnityEngine;
namespace Frontier.MeshGen.UI {
    public static class PanelFrameGen {
        public static Sprite GenerateWindowFrame(int w = 400, int h = 300) {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[w * h];
            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    if (x < 4 || x >= w-4 || y < 4 || y >= h-4) pixels[y*w+x] = new Color(0.1f,0.1f,0.15f);
                    else pixels[y*w+x] = new Color(0.2f,0.2f,0.25f, 0.9f);
                }
            }
            tex.SetPixels(pixels); tex.Apply();
            return Sprite.Create(tex, new Rect(0,0,w,h), new Vector2(0.5f,0.5f));
        }
    }
}
