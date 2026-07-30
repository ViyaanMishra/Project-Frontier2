using UnityEngine;
namespace Frontier.MeshGen.Items {
    public static class ResourceProcessedGen {
        public static Mesh GenerateIronIngot() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.6f, 0.15f, 0.2f);
            return b.BuildFlat("IronIngot");
        }
        public static Mesh GenerateSteelPlate() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.7f, 0.05f, 0.5f);
            return b.BuildFlat("SteelPlate");
        }
        public static Mesh GenerateCircuitBoard() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.3f, 0.02f, 0.4f);
            return b.BuildFlat("CircuitBoard");
        }
    }
}
