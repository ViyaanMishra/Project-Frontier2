using UnityEngine;
using Frontier.MeshGen;

namespace Frontier.MeshGen.UI
{
    public static class HUDFrameGen
    {
        public static Texture2D GenerateHealthBar(int width = 200, int height = 20)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBorder = x == 0 || x == width - 1 || y == 0 || y == height - 1;
                    if (isBorder)
                        pixels[y * width + x] = Color.black;
                    else
                        pixels[y * width + x] = new Color(0.8f, 0.1f, 0.1f, 1f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        public static Texture2D GenerateStaminaBar(int width = 200, int height = 20)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBorder = x == 0 || x == width - 1 || y == 0 || y == height - 1;
                    if (isBorder)
                        pixels[y * width + x] = Color.black;
                    else
                        pixels[y * width + x] = new Color(0.2f, 0.8f, 0.2f, 1f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        public static Texture2D GenerateHungerBar(int width = 200, int height = 20)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBorder = x == 0 || x == width - 1 || y == 0 || y == height - 1;
                    if (isBorder)
                        pixels[y * width + x] = Color.black;
                    else
                        pixels[y * width + x] = new Color(0.9f, 0.6f, 0.1f, 1f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        public static Texture2D GenerateThirstBar(int width = 200, int height = 20)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBorder = x == 0 || x == width - 1 || y == 0 || y == height - 1;
                    if (isBorder)
                        pixels[y * width + x] = Color.black;
                    else
                        pixels[y * width + x] = new Color(0.1f, 0.4f, 0.9f, 1f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        public static Texture2D GenerateHotbarSlot(int size = 64)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Mathf.Sqrt(Mathf.Pow(x - size / 2f, 2) + Mathf.Pow(y - size / 2f, 2));
                    if (dist > size / 2f - 2)
                        pixels[y * size + x] = Color.white;
                    else
                        pixels[y * size + x] = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
    }
}
