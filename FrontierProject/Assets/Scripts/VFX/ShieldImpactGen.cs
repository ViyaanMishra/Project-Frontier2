using UnityEngine;
namespace Frontier.VFX {
    public static class ShieldImpactGen {
        public static Mesh GenerateShieldHit() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddIcosphere(b, 0.4f, 2);
            return b.BuildFlat("ShieldHit");
        }
    }
}
