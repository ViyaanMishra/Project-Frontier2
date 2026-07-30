using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Characters {
    public static class HairGenerator {
        public static Mesh GenerateShortHair() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddIcosphere(b, 0.27f, 1);
            return b.BuildFlat("ShortHair");
        }
        public static Mesh GenerateLongHair() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddIcosphere(b, 0.27f, 1);
            PrimitiveShapes.AddBox(b, 0.3f, 0.4f, 0.2f, new Vector3(0, -0.3f, -0.1f));
            return b.BuildFlat("LongHair");
        }
        public static Mesh GenerateMohawk() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 0.08f, 0.2f, 0.4f, new Vector3(0, 0.15f, 0));
            return b.BuildFlat("Mohawk");
        }
        public static Mesh GenerateBald() { return null; }
    }
}
