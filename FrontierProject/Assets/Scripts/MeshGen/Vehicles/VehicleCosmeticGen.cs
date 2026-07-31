using UnityEngine;
using System.Collections.Generic;

namespace FrontierProject.MeshGen.Vehicles
{
    /// <summary>
    /// Generates cosmetic variations for vehicles including paint jobs, 
    /// decals, trim pieces, and visual modifications.
    /// </summary>
    public class VehicleCosmeticGen : MonoBehaviour
    {
        [Header("Paint Configuration")]
        public Color primaryColor = Color.white;
        public Color secondaryColor = Color.gray;
        public Color accentColor = Color.black;
        
        [Header("Decal Options")]
        public bool allowRacingStripes = true;
        public bool allowSideLogos = true;
        public bool allowHoodDecals = true;
        public bool allowWeathering = true;
        
        [Header("Trim Variations")]
        public bool chromeTrim = false;
        public bool matteFinish = false;
        public bool carbonFiberAccents = false;
        
        [Header("Wheel Options")]
        public int wheelStyleIndex = 0;
        [Range(1f, 1.5f)] public float wheelSizeMultiplier = 1f;
        
        private MeshRenderer bodyRenderer;
        private MeshRenderer wheelRenderer;
        
        public void ApplyCosmetics(GameObject vehicle, int seed)
        {
            Random.InitState(seed);
            
            bodyRenderer = vehicle.GetComponentInChildren<MeshRenderer>();
            if (bodyRenderer == null)
            {
                Debug.LogError("No MeshRenderer found on vehicle");
                return;
            }
            
            ApplyPaintScheme(bodyRenderer);
            ApplyDecals(vehicle);
            ApplyTrimModifications(vehicle);
            
            var wheels = vehicle.GetComponentsInChildren<MeshFilter>();
            foreach (var wheel in wheels)
            {
                if (wheel.name.ToLower().Contains("wheel"))
                {
                    ApplyWheelStyle(wheel);
                }
            }
        }
        
        private void ApplyPaintScheme(MeshRenderer renderer)
        {
            Material[] materials = renderer.sharedMaterials;
            
            if (materials.Length >= 3)
            {
                materials[0] = CreateMaterial(primaryColor, matteFinish ? 0.3f : 0.7f);
                materials[1] = CreateMaterial(secondaryColor, matteFinish ? 0.2f : 0.5f);
                materials[2] = CreateMaterial(accentColor, chromeTrim ? 0.9f : 0.4f);
            }
            else if (materials.Length > 0)
            {
                materials[0] = CreateMaterial(primaryColor, matteFinish ? 0.3f : 0.7f);
            }
            
            renderer.sharedMaterials = materials;
        }
        
        private Material CreateMaterial(Color color, float metallic)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", 0.5f + (metallic * 0.3f));
            return mat;
        }
        
        private void ApplyDecals(GameObject vehicle)
        {
            List<string> appliedDecals = new List<string>();
            
            if (allowRacingStripes && Random.value > 0.5f)
            {
                appliedDecals.Add("RacingStripes");
                // Would spawn decal mesh or modify UVs
            }
            
            if (allowSideLogos && Random.value > 0.6f)
            {
                appliedDecals.Add("SideLogos");
            }
            
            if (allowHoodDecals && Random.value > 0.7f)
            {
                appliedDecals.Add("HoodDecal");
            }
            
            if (allowWeathering)
            {
                float weatheringAmount = Random.Range(0.1f, 0.8f);
                appliedDecals.Add($"Weathering_{weatheringAmount:F2}");
            }
            
            Debug.Log($"Applied decals: {string.Join(", ", appliedDecals)}");
        }
        
        private void ApplyTrimModifications(GameObject vehicle)
        {
            var trimObjects = vehicle.GetComponentsInChildren<MeshFilter>();
            
            foreach (var trim in trimObjects)
            {
                if (trim.name.ToLower().Contains("trim") || trim.name.ToLower().Contains("grille"))
                {
                    var renderer = trim.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        float metallic = chromeTrim ? 0.95f : (carbonFiberAccents ? 0.3f : 0.5f);
                        var mat = CreateMaterial(accentColor, metallic);
                        renderer.sharedMaterial = mat;
                    }
                }
            }
        }
        
        private void ApplyWheelStyle(MeshFilter wheelMesh)
        {
            // Placeholder: Would swap wheel mesh or apply texture variations
            float sizeScale = wheelSizeMultiplier;
            wheelMesh.transform.localScale = Vector3.one * sizeScale;
            Debug.Log($"Applied wheel style {wheelStyleIndex} with scale {sizeScale:F2}");
        }
        
        public void GenerateRandomCosmetics(GameObject vehicle)
        {
            int seed = Random.Range(0, int.MaxValue);
            ApplyCosmetics(vehicle, seed);
        }
    }
}
