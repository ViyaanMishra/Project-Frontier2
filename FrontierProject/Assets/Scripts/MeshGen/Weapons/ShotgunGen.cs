using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Weapons {
    public static class ShotgunGen {
        public static Mesh GeneratePumpShotgun() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.1f, 0.12f, 0.6f);
            PrimitiveShapes.AddCylinder(b, 0.05f, 0.4f, 12, new Vector3(0, 0.08f, 0.2f));
            PrimitiveShapes.AddCylinder(b, 0.06f, 0.3f, 8, new Vector3(0, -0.1f, 0.3f));
            return b.BuildFlat("PumpShotgun");
        }
    }
}
