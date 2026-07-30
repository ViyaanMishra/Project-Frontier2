using UnityEngine;
namespace Frontier.VFX {
    public static class ImpactSparkGen {
        public static Mesh GenerateSpark() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddIcosphere(b, 0.05f, 1);
            return b.BuildFlat("Spark");
        }
    }
}
