using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Vehicles {
    public static class AttackHelicopterGen {
        public static Mesh Generate() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 1.5f, 1.5f, 4f);
            PrimitiveShapes.AddCylinder(b, 3f, 0.1f, 8, new Vector3(0, 1.5f, 0));
            PrimitiveShapes.AddCylinder(b, 1f, 0.1f, 8, new Vector3(0, 1.5f, -3f));
            PrimitiveShapes.AddBox(b, 0.5f, 0.5f, 2f, new Vector3(0, -0.5f, 2f));
            return b.BuildFlat("AttackHelicopter");
        }
    }
}
