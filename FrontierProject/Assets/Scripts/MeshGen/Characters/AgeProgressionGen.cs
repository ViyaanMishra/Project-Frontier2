using UnityEngine;
using System.Collections.Generic;

namespace FrontierProject.MeshGen.Characters
{
    /// <summary>
    /// Handles age progression for character meshes, simulating aging effects
    /// on facial structure, skin, and body proportions.
    /// </summary>
    public class AgeProgressionGen : MonoBehaviour
    {
        [Header("Age Parameters")]
        [Range(0f, 1f)] public float ageFactor = 0f; // 0 = young, 1 = elderly
        
        [Header("Facial Aging")]
        [Range(0f, 0.1f)] public float skinSagAmount = 0.02f;
        [Range(0f, 0.05f)] public float wrinkleDepth = 0.01f;
        [Range(0f, 0.3f)] public float noseGrowth = 0.1f;
        [Range(0f, 0.2f)] public float earGrowth = 0.1f;
        
        [Header("Body Aging")]
        [Range(-0.1f, 0.1f)] public float heightLoss = -0.02f;
        [Range(0f, 0.2f)] public float bellyGrowth = 0.05f;
        [Range(0f, 0.1f)] public float shoulderDroop = 0.03f;
        
        [Header("Hair Changes")]
        public bool recedingHairline = true;
        [Range(0f, 0.4f)] public float hairlineRecession = 0.1f;
        
        private MeshFilter targetMesh;
        
        public void ApplyAging(MeshFilter mesh, float ageNormalized)
        {
            targetMesh = mesh;
            ageFactor = Mathf.Clamp01(ageNormalized);
            
            if (targetMesh.sharedMesh == null)
            {
                Debug.LogError("No mesh found for aging");
                return;
            }
            
            var meshCopy = Instantiate(targetMesh.sharedMesh);
            ApplyFacialAging(meshCopy);
            ApplyBodyAging(meshCopy);
            
            if (recedingHairline) ApplyHairlineRecession(meshCopy);
            
            meshCopy.RecalculateNormals();
            meshCopy.RecalculateBounds();
            targetMesh.sharedMesh = meshCopy;
        }
        
        private void ApplyFacialAging(Mesh mesh)
        {
            var vertices = mesh.vertices;
            float effectiveAging = ageFactor * ageFactor; // Non-linear aging
            
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertex = vertices[i];
                
                // Skin sag (downward pull on lower face)
                if (vertex.y < 0.5f && Mathf.Abs(vertex.x) < 0.25f)
                {
                    vertex.y -= skinSagAmount * effectiveAging * (0.5f - vertex.y);
                }
                
                // Nose growth (cartilage continues growing)
                if (vertex.y > 0.4f && vertex.y < 0.6f && Mathf.Abs(vertex.x) < 0.1f)
                {
                    vertex.z += noseGrowth * effectiveAging * 0.05f;
                    vertex.x *= (1f + noseGrowth * effectiveAging * 0.1f);
                }
                
                // Ear growth
                if (Mathf.Abs(vertex.x) > 0.2f && vertex.y > 0.5f && vertex.y < 0.7f)
                {
                    vertex.x *= (1f + earGrowth * effectiveAging * 0.2f);
                    vertex.y -= earGrowth * effectiveAging * 0.05f;
                }
                
                // Wrinkle zones (subtle displacement)
                if (ShouldAddWrinkle(vertex))
                {
                    vertex += Vector3.forward * wrinkleDepth * effectiveAging * Random.value;
                }
                
                vertices[i] = vertex;
            }
            
            mesh.vertices = vertices;
        }
        
        private bool ShouldAddWrinkle(Vector3 vertex)
        {
            // Forehead wrinkles
            if (vertex.y > 0.75f && vertex.y < 0.85f && Mathf.Abs(vertex.x) < 0.15f)
                return Random.value < 0.3f;
            
            // Crow's feet (eye corners)
            if (vertex.y > 0.65f && vertex.y < 0.7f && Mathf.Abs(vertex.x) > 0.12f && Mathf.Abs(vertex.x) < 0.18f)
                return Random.value < 0.4f;
            
            // Nasolabial folds
            if (vertex.y > 0.45f && vertex.y < 0.55f && Mathf.Abs(vertex.x) > 0.05f && Mathf.Abs(vertex.x) < 0.12f)
                return Random.value < 0.35f;
            
            return false;
        }
        
        private void ApplyBodyAging(Mesh mesh)
        {
            var vertices = mesh.vertices;
            
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertex = vertices[i];
                
                // Height loss (compression of spine)
                if (vertex.y > 0.3f)
                {
                    vertex.y *= (1f + heightLoss * ageFactor);
                }
                
                // Belly growth (midsection expansion)
                if (vertex.y > 0.2f && vertex.y < 0.45f && Mathf.Abs(vertex.x) < 0.2f)
                {
                    float bellyFactor = bellyGrowth * ageFactor * (1f - Mathf.Abs(vertex.x) / 0.2f);
                    vertex.x *= (1f + bellyFactor);
                    vertex.z += bellyFactor * 0.1f;
                }
                
                // Shoulder droop
                if (vertex.y > 0.6f && vertex.y < 0.75f && Mathf.Abs(vertex.x) > 0.15f)
                {
                    vertex.y -= shoulderDroop * ageFactor * (Mathf.Abs(vertex.x) - 0.15f);
                }
                
                vertices[i] = vertex;
            }
            
            mesh.vertices = vertices;
        }
        
        private void ApplyHairlineRecession(Mesh mesh)
        {
            // Placeholder: Would modify scalp mesh or hair attachment points
            float recessionAmount = hairlineRecession * ageFactor;
            Debug.Log($"Applied hairline recession: {recessionAmount:F3}");
        }
        
        public void ResetToBase(Mesh baseMesh)
        {
            if (targetMesh != null)
            {
                targetMesh.sharedMesh = baseMesh;
            }
        }
        
        public static float CalculateAgeFromFloat(float years, float minAge = 18f, float maxAge = 80f)
        {
            return Mathf.InverseLerp(minAge, maxAge, years);
        }
    }
}
