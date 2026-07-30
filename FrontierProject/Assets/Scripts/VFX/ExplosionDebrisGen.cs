using UnityEngine;
namespace Frontier.VFX {
    public static class ExplosionDebrisGen {
        public static Mesh GenerateDebris() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.1f, 0.1f, 0.1f);
            return b.BuildFlat("ExplosionDebris");
        }
    }
}
