using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Weapons {
    public static class HeavyGen {
        public static Mesh GenerateMinigun() {
            var b = new LowPolyMeshBuilder();
            for (int i = 0; i < 6; i++) {
                float angle = i * 60 * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 0.1f, 0, Mathf.Sin(angle) * 0.1f);
                PrimitiveShapes.AddCylinder(b, 0.03f, 0.8f, 8, pos);
            }
            PrimitiveShapes.AddBox(b, 0.15f, 0.2f, 0.3f, new Vector3(0, -0.3f, 0));
            return b.BuildFlat("Minigun");
        }
        public static Mesh GenerateRocketLauncher() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddCylinder(b, 0.1f, 1.0f, 12);
            PrimitiveShapes.AddBox(b, 0.15f, 0.1f, 0.3f, new Vector3(0, -0.2f, 0));
            return b.BuildFlat("RocketLauncher");
        }
    }
}
