using UnityEngine;
namespace Frontier.VFX {
    public static class TeleportGen {
        public static Mesh GenerateTeleportRing() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddTorus(b, 0.3f, 0.05f, 16);
            return b.BuildFlat("TeleportRing");
        }
    }
}
