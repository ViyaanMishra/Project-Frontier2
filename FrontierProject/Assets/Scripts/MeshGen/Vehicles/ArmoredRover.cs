using UnityEngine;
using Frontier.MeshGen;
namespace Frontier.MeshGen.Vehicles {
    public static class ArmoredRoverGen {
        public static Mesh Generate() {
            var b = new LowPolyMeshBuilder();
            PrimitiveShapes.AddBox(b, 2f, 1.8f, 4.5f);
            PrimitiveShapes.AddBox(b, 2.2f, 0.3f, 1f, new Vector3(0, 0.5f, -1.5f));
            for (int i = 0; i < 6; i++) {
                float x = (i % 2 == 0) ? -1.1f : 1.1f;
                float z = -1.5f + (i / 2) * 1.5f;
                PrimitiveShapes.AddCylinder(b, 0.4f, 0.4f, 12, new Vector3(x, -0.7f, z));
            }
            return b.BuildFlat("ArmoredRover");
        }
    }
}
