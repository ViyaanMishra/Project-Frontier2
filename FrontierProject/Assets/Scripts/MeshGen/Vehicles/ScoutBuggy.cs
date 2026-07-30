using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Vehicles {
    public static class ScoutBuggyGen {
        public static Mesh Generate() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 1.8f, 0.6f, 3.5f);
            PrimitiveShapes.AddBox(b, 1.5f, 0.3f, 1f, new Vector3(0, 0.5f, -1f));
            for (int i = 0; i < 4; i++) {
                float x = (i % 2 == 0) ? -0.9f : 0.9f;
                float z = (i < 2) ? -1.2f : 1.2f;
                PrimitiveShapes.AddCylinder(b, 0.35f, 0.3f, 12, new Vector3(x, -0.5f, z));
            }
            return b.BuildFlat("ScoutBuggy");
        }
    }
}
