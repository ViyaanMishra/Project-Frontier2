using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Characters {
    public static class HumanoidMeshGen {
        public static Mesh GenerateTorso(float height = 0.8f, float width = 0.4f) {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, width, height, width * 0.6f);
            return b.BuildFlat("Torso");
        }
        public static Mesh GenerateHead(float size = 0.25f) {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddIcosphere(b, size, 2);
            return b.BuildFlat("Head");
        }
        public static Mesh GenerateLimb(float length = 0.4f, float radius = 0.08f) {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddCylinder(b, radius, length, 8);
            return b.BuildFlat("Limb");
        }
        public static Mesh GenerateFullBody() {
            var b = new LowPolyMeshBuilder();
            GenerateTorso(0.8f, 0.4f);
            GenerateHead(0.25f);
            GenerateLimb(0.35f, 0.07f);
            return b.BuildFlat("Humanoid");
        }
    }
}
