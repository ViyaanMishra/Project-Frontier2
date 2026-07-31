using UnityEngine;
using System.Collections.Generic;

namespace FrontierProject.MeshGen.Characters
{
    /// <summary>
    /// Generates appearance variations for characters including facial features, 
    /// body types, and distinguishing marks.
    /// </summary>
    public class AppearanceVariationGen : MonoBehaviour
    {
        [Header("Facial Feature Ranges")]
        [Range(0f, 1f)] public float noseWidthRange = 0.3f;
        [Range(0f, 1f)] public float noseLengthRange = 0.3f;
        [Range(0f, 1f)] public float jawWidthRange = 0.4f;
        [Range(0f, 1f)] public float cheekboneHeightRange = 0.3f;
        [Range(0f, 1f)] public float eyeSpacingRange = 0.2f;
        
        [Header("Body Variation")]
        [Range(0.8f, 1.2f)] public float heightMultiplier = 1f;
        [Range(0.7f, 1.3f)] public float buildMultiplier = 1f;
        
        [Header("Distinguishing Features")]
        public bool allowScars = true;
        public bool allowTattoos = true;
        public bool allowBirthmarks = true;
        
        private MeshFilter targetMesh;
        private Vector3[] originalVertices;
        
        public void GenerateVariation(MeshFilter mesh, int seed)
        {
            targetMesh = mesh;
            Random.InitState(seed);
            
            if (targetMesh.sharedMesh == null)
            {
                Debug.LogError("No mesh found on target MeshFilter");
                return;
            }
            
            var meshCopy = Instantiate(targetMesh.sharedMesh);
            originalVertices = meshCopy.vertices;
            
            ApplyFacialVariations(meshCopy);
            ApplyBodyVariations(meshCopy);
            
            if (allowScars) ApplyScars(meshCopy);
            if (allowTattoos) ApplyTattooMarkers(meshCopy);
            if (allowBirthmarks) ApplyBirthmarks(meshCopy);
            
            meshCopy.RecalculateNormals();
            meshCopy.RecalculateBounds();
            targetMesh.sharedMesh = meshCopy;
        }
        
        private void ApplyFacialVariations(Mesh mesh)
        {
            var vertices = mesh.vertices;
            
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertex = vertices[i];
                
                // Nose region modification
                if (vertex.y > 0.4f && vertex.y < 0.6f && Mathf.Abs(vertex.x) < 0.15f)
                {
                    vertex.x *= (1f + Random.Range(-noseWidthRange, noseWidthRange));
                    vertex.z += Random.Range(-noseLengthRange, noseLengthRange) * 0.05f;
                }
                
                // Jaw modification
                if (vertex.y < 0.3f && Mathf.Abs(vertex.x) > 0.1f)
                {
                    vertex.x *= (1f + Random.Range(-jawWidthRange, jawWidthRange));
                }
                
                // Cheekbone modification
                if (vertex.y > 0.5f && vertex.y < 0.7f && Mathf.Abs(vertex.x) > 0.1f)
                {
                    vertex.y += Random.Range(-cheekboneHeightRange, cheekboneHeightRange) * 0.03f;
                }
                
                // Eye spacing
                if (vertex.y > 0.65f && vertex.y < 0.75f && Mathf.Abs(vertex.x) < 0.2f)
                {
                    vertex.x *= (1f + Random.Range(-eyeSpacingRange, eyeSpacingRange));
                }
                
                vertices[i] = vertex;
            }
            
            mesh.vertices = vertices;
        }
        
        private void ApplyBodyVariations(Mesh mesh)
        {
            var vertices = mesh.vertices;
            
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertex = vertices[i];
                
                // Height scaling (Y axis)
                vertex.y *= heightMultiplier;
                
                // Build scaling (X and Z axes)
                vertex.x *= buildMultiplier;
                vertex.z *= buildMultiplier;
                
                vertices[i] = vertex;
            }
            
            mesh.vertices = vertices;
        }
        
        private void ApplyScars(Mesh mesh)
        {
            // Placeholder: In production, this would modify UVs or add decal projections
            int scarCount = Random.Range(1, 4);
            Debug.Log($"Generated {scarCount} scar markers for character");
        }
        
        private void ApplyTattooMarkers(Mesh mesh)
        {
            // Placeholder: Creates UV coordinate sets for tattoo decals
            int tattooCount = Random.Range(0, 3);
            Debug.Log($"Generated {tattooCount} tattoo marker regions");
        }
        
        private void ApplyBirthmarks(Mesh mesh)
        {
            // Placeholder: Adds color variation zones for skin texture
            int birthmarkCount = Random.Range(0, 2);
            Debug.Log($"Generated {birthmarkCount} birthmark regions");
        }
        
        public void ResetToBase(Mesh baseMesh)
        {
            if (targetMesh != null)
            {
                targetMesh.sharedMesh = baseMesh;
            }
        }
    }
}
