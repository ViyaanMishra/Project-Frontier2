using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Vehicles {
    public static class HoverSkiffGen {
        public static Mesh Generate() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 2.5f, 0.8f, 5f);
            PrimitiveShapes.AddBox(b, 2f, 0.2f, 1f, new Vector3(0, -0.5f, 0));
            return b.BuildFlat("HoverSkiff");
        }
    }
}
