using UnityEngine;
namespace Frontier.VFX {
    public static class MuzzleFlashGen {
        public static Mesh GeneratePistolFlash(int seed = 0) {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddCone(b, 0.1f, 0.3f, 8);
            return b.BuildFlat("PistolFlash");
        }
        public static Mesh GenerateRifleBurst(int seed = 0) {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddCone(b, 0.15f, 0.5f, 12);
            return b.BuildFlat("RifleBurst");
        }
    }
}
