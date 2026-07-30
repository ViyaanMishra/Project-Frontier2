using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Vehicles {
    public static class AnomalySkimmerGen {
        public static Mesh Generate() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddIcosphere(b, 2f, 2);
            PrimitiveShapes.AddCone(b, 1f, 2f, 8, new Vector3(0, 0, 2f));
            return b.BuildFlat("AnomalySkimmer");
        }
    }
}
