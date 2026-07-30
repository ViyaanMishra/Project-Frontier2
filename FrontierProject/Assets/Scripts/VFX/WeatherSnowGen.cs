using UnityEngine;
namespace Frontier.VFX {
    public static class WeatherSnowGen {
        public static Mesh GenerateSnowflake() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.05f, 0.01f, 0.05f);
            return b.BuildFlat("Snowflake");
        }
    }
}
