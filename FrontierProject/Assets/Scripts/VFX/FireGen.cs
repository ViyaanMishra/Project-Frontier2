using UnityEngine;
namespace Frontier.VFX {
    public static class FireGen {
        public static Mesh GenerateFlame() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddCone(b, 0.3f, 0.8f, 8);
            return b.BuildFlat("Flame");
        }
    }
}
