using UnityEngine;
namespace Frontier.MeshGen.Characters {
    public static class BodyProportions {
        public static Vector3 GetLeanScale() { return new Vector3(0.9f, 1.05f, 0.85f); }
        public static Vector3 GetAthleticScale() { return new Vector3(1.1f, 1.0f, 0.95f); }
        public static Vector3 GetHeavyScale() { return new Vector3(1.2f, 0.95f, 1.1f); }
        public static Vector3 GetTallScale() { return new Vector3(1.0f, 1.2f, 0.9f); }
        public static Vector3 GetShortScale() { return new Vector3(0.9f, 0.8f, 0.9f); }
    }
}
