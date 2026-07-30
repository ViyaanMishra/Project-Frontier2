using UnityEngine;
namespace Frontier.MeshGen.Characters {
    public static class SkinTonePalette {
        public static Color[] GetSkinTones() {
            return new Color[] {
                new Color(0.95f, 0.8f, 0.7f), new Color(0.9f, 0.7f, 0.6f),
                new Color(0.8f, 0.6f, 0.5f), new Color(0.7f, 0.5f, 0.4f),
                new Color(0.6f, 0.4f, 0.3f), new Color(0.5f, 0.35f, 0.25f),
                new Color(0.4f, 0.3f, 0.2f), new Color(0.3f, 0.2f, 0.15f),
                new Color(0.7f, 0.75f, 0.8f), new Color(0.6f, 0.7f, 0.6f),
                new Color(0.5f, 0.6f, 0.7f), new Color(0.4f, 0.5f, 0.6f)
            };
        }
        public static Color GetMutantGreen() { return new Color(0.4f, 0.7f, 0.4f); }
        public static Color GetMutantGrey() { return new Color(0.5f, 0.5f, 0.55f); }
        public static Color GetMutantBlue() { return new Color(0.4f, 0.5f, 0.7f); }
    }
}
