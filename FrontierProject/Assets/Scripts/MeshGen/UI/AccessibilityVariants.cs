using UnityEngine;
namespace Frontier.MeshGen.UI {
    public static class AccessibilityVariants {
        public static Color BlindDeuteranopia(Color c) { return new Color(c.r * 0.5f + c.g * 0.5f, c.g, c.b); }
        public static Color BlindProtanopia(Color c) { return new Color(c.r, c.g * 0.5f + c.b * 0.5f, c.b); }
        public static Color HighContrast(Color c) { return (c.grayscale > 0.5f) ? Color.white : Color.black; }
    }
}
