using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Vehicles {
    public static class CargoHelicopterGen {
        public static Mesh Generate() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 2.5f, 2f, 6f);
            PrimitiveShapes.AddCylinder(b, 4f, 0.15f, 8, new Vector3(0, 2f, 0));
            PrimitiveShapes.AddBox(b, 0.8f, 0.8f, 3f, new Vector3(0, -0.5f, 3f));
            return b.BuildFlat("CargoHelicopter");
        }
    }
}
