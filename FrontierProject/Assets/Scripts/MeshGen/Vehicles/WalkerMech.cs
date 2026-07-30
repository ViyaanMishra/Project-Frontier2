using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Vehicles {
    public static class WalkerMechGen {
        public static Mesh Generate() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 2f, 2.5f, 3f);
            PrimitiveShapes.AddCylinder(b, 0.4f, 2f, 8, new Vector3(-0.8f, -2f, 0));
            PrimitiveShapes.AddCylinder(b, 0.4f, 2f, 8, new Vector3(0.8f, -2f, 0));
            return b.BuildFlat("WalkerMech");
        }
    }
}
