using UnityEngine;
namespace Frontier.VFX {
    public static class SmokeTrailGen {
        public static Mesh GenerateSmokePuff() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddIcosphere(b, 0.2f, 2);
            return b.BuildFlat("SmokePuff");
        }
    }
}
