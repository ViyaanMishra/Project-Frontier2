using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Weapons {
    public static class ThrownGen {
        public static Mesh GenerateGrenade() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddIcosphere(b, 0.08f, 2);
            PrimitiveShapes.AddCylinder(b, 0.02f, 0.05f, 6, new Vector3(0, 0.1f, 0));
            return b.BuildFlat("Grenade");
        }
        public static Mesh GenerateMolotov() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddCylinder(b, 0.06f, 0.2f, 8);
            PrimitiveShapes.AddBox(b, 0.03f, 0.05f, 0.03f, new Vector3(0, 0.12f, 0));
            return b.BuildFlat("Molotov");
        }
    }
}
