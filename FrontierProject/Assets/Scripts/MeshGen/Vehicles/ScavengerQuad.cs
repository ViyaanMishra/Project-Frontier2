using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Vehicles {
    public static class ScavengerQuadGen {
        public static Mesh Generate() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 1.5f, 0.5f, 2.5f);
            for (int i = 0; i < 4; i++) {
                float x = (i % 2 == 0) ? -1f : 1f;
                float z = (i < 2) ? -1f : 1f;
                PrimitiveShapes.AddCylinder(b, 0.35f, 0.3f, 12, new Vector3(x, -0.3f, z));
            }
            return b.BuildFlat("ScavengerQuad");
        }
    }
}
