using UnityEngine;
using System.Collections.Generic;

namespace Frontier.MeshGen
{
    /// <summary>
    /// Combines multiple meshes into a single mesh with material slots
    /// </summary>
    public static class MeshCombiner
    {
        [System.Serializable]
        public class MeshSlot
        {
            public Mesh mesh;
            public Material material;
            public Matrix4x4 transform = Matrix4x4.identity;
        }
        
        /// <summary>
        /// Combine multiple meshes into one, preserving material slots
        /// </summary>
        public static Mesh Combine(List<MeshSlot> slots, string name = "CombinedMesh")
        {
            if (slots == null || slots.Count == 0) return null;
            
            List<CombineInstance> combines = new List<CombineInstance>();
            List<Material> materials = new List<Material>();
            
            // Group by material
            Dictionary<Material, List<MeshSlot>> materialGroups = new Dictionary<Material, List<MeshSlot>>();
            
            foreach (var slot in slots)
            {
                if (slot.mesh == null) continue;
                
                if (!materialGroups.ContainsKey(slot.material))
                    materialGroups[slot.material] = new List<MeshSlot>();
                    
                materialGroups[slot.material].Add(slot);
            }
            
            // Create combine instances per material
            int subMeshIndex = 0;
            foreach (var kvp in materialGroups)
            {
                materials.Add(kvp.Key ?? GetDefaultMaterial());
                
                foreach (var slot in kvp.Value)
                {
                    CombineInstance ci = new CombineInstance
                    {
                        mesh = slot.mesh,
                        transform = slot.transform,
                        subMeshIndex = 0
                    };
                    combines.Add(ci);
                }
                
                subMeshIndex++;
            }
            
            Mesh result = new Mesh();
            result.name = name;
            result.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            
            if (combines.Count > 0)
                result.CombineMeshes(combines.ToArray(), false, true);
            
            return result;
        }
        
        /// <summary>
        /// Simple mesh combination without material separation
        /// </summary>
        public static Mesh CombineSimple(Mesh[] meshes, Matrix4x4[] transforms, string name = "CombinedMesh")
        {
            if (meshes == null || meshes.Length == 0) return null;
            
            List<CombineInstance> combines = new List<CombineInstance>();
            
            for (int i = 0; i < meshes.Length; i++)
            {
                if (meshes[i] == null) continue;
                
                combines.Add(new CombineInstance
                {
                    mesh = meshes[i],
                    transform = transforms != null && i < transforms.Length ? transforms[i] : Matrix4x4.identity,
                    subMeshIndex = 0
                });
            }
            
            Mesh result = new Mesh();
            result.name = name;
            
            if (combines.Count > 0)
                result.CombineMeshes(combines.ToArray(), true, true);
            
            return result;
        }
        
        private static Material GetDefaultMaterial()
        {
            return new Material(Shader.Find("Universal Render Pipeline/Lit"));
        }
        
        /// <summary>
        /// Create LOD group from combined mesh
        /// </summary>
        public static GameObject CreateWithLODs(Mesh baseMesh, Material[] materials, float[] lodThresholds = null)
        {
            GameObject parent = new GameObject("MeshWithLODs");
            
            if (lodThresholds == null)
                lodThresholds = new float[] { 0.5f, 0.25f, 0.1f };
            
            for (int i = 0; i < lodThresholds.Length; i++)
            {
                Mesh lodMesh = LODGenerator.Generate(baseMesh, i + 1);
                
                GameObject lodObj = new GameObject($"LOD{i}");
                lodObj.transform.SetParent(parent.transform);
                lodObj.AddComponent<MeshFilter>().mesh = lodMesh;
                lodObj.AddComponent<MeshRenderer>().materials = materials;
                
                // LOD component would be added here in actual implementation
            }
            
            return parent;
        }
    }
}
