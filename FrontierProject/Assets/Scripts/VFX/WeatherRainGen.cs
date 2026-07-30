using UnityEngine;
namespace Frontier.VFX {
    public static class WeatherRainGen {
        public static Mesh GenerateRainDrop() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddCapsule(b, 0.02f, 0.3f);
            return b.BuildFlat("RainDrop");
        }
    }
}
