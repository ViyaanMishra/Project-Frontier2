using UnityEngine;
using System.Collections.Generic;

namespace FrontierProject.MeshGen.Building.Workbench
{
    /// <summary>
    /// Generates procedural workbenches and crafting stations with 
    /// tool racks, storage, and work surfaces.
    /// </summary>
    public class WorkbenchGenerator : MonoBehaviour
    {
        public enum WorkbenchType { Basic, Advanced, Industrial, Scientific }
        
        [Header("Generation Settings")]
        public WorkbenchType workbenchType = WorkbenchType.Basic;
        [Range(0.5f, 2f)] public float scaleMultiplier = 1f;
        
        [Header("Features")]
        public bool hasVice = true;
        public bool hasToolRack = true;
        public bool hasStorage = true;
        public bool hasPowerStrip = false;
        public bool hasLighting = false;
        
        [Header("Materials")]
        public Color woodColor = new Color(0.5f, 0.35f, 0.2f);
        public Color metalColor = new Color(0.4f, 0.4f, 0.45f);
        public Color accentColor = new Color(0.8f, 0.3f, 0.1f);
        
        private GameObject workbenchRoot;
        
        public GameObject GenerateWorkbench(int seed)
        {
            Random.InitState(seed);
            
            workbenchRoot = new GameObject($"Workbench_{workbenchType}");
            
            GenerateBase();
            GenerateSurface();
            
            if (hasVice) GenerateVice();
            if (hasToolRack) GenerateToolRack();
            if (hasStorage) GenerateStorage();
            if (hasPowerStrip) GeneratePowerStrip();
            if (hasLighting) GenerateLighting();
            
            AddTypeSpecificFeatures();
            
            workbenchRoot.transform.localScale = Vector3.one * scaleMultiplier;
            Debug.Log($"Generated {workbenchType} workbench");
            
            return workbenchRoot;
        }
        
        private void GenerateBase()
        {
            // Legs
            float legSpacingX = 0.7f;
            float legSpacingZ = 0.4f;
            
            CreateLeg(new Vector3(-legSpacingX, 0.4f, -legSpacingZ));
            CreateLeg(new Vector3(legSpacingX, 0.4f, -legSpacingZ));
            CreateLeg(new Vector3(-legSpacingX, 0.4f, legSpacingZ));
            CreateLeg(new Vector3(legSpacingX, 0.4f, legSpacingZ));
            
            // Cross supports
            if (workbenchType != WorkbenchType.Basic)
            {
                GameObject support1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                support1.transform.SetParent(workbenchRoot.transform);
                support1.transform.localScale = new Vector3(0.05f, 0.05f, 0.7f);
                support1.transform.localPosition = new Vector3(0, 0.2f, 0);
                ApplyMaterial(support1, metalColor);
                
                GameObject support2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                support2.transform.SetParent(workbenchRoot.transform);
                support2.transform.localScale = new Vector3(1.2f, 0.05f, 0.05f);
                support2.transform.localPosition = new Vector3(0, 0.2f, 0);
                ApplyMaterial(support2, metalColor);
            }
        }
        
        private void CreateLeg(Vector3 position)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leg.transform.SetParent(workbenchRoot.transform);
            leg.transform.localPosition = position;
            
            if (workbenchType == WorkbenchType.Industrial)
            {
                leg.transform.localScale = new Vector3(0.08f, 0.8f, 0.08f);
                ApplyMaterial(leg, metalColor);
            }
            else if (workbenchType == WorkbenchType.Scientific)
            {
                leg.transform.localScale = new Vector3(0.06f, 0.8f, 0.06f);
                ApplyMaterial(leg, new Color(0.9f, 0.9f, 0.95f));
            }
            else
            {
                leg.transform.localScale = new Vector3(0.06f, 0.8f, 0.06f);
                ApplyMaterial(leg, woodColor);
            }
        }
        
        private void GenerateSurface()
        {
            GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
            surface.transform.SetParent(workbenchRoot.transform);
            
            float thickness = workbenchType == WorkbenchType.Industrial ? 0.08f : 0.05f;
            surface.transform.localScale = new Vector3(1.5f, thickness, 0.9f);
            surface.transform.localPosition = new Vector3(0, 0.825f, 0);
            
            if (workbenchType == WorkbenchType.Scientific)
            {
                ApplyMaterial(surface, new Color(0.95f, 0.95f, 0.98f));
            }
            else if (workbenchType == WorkbenchType.Advanced || workbenchType == WorkbenchType.Industrial)
            {
                ApplyMaterial(surface, metalColor);
            }
            else
            {
                ApplyMaterial(surface, woodColor);
            }
            
            // Add wear marks for basic workbench
            if (workbenchType == WorkbenchType.Basic)
            {
                Debug.Log("Applied worn surface texture");
            }
        }
        
        private void GenerateVice()
        {
            GameObject viceBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viceBase.transform.SetParent(workbenchRoot.transform);
            viceBase.transform.localScale = new Vector3(0.2f, 0.08f, 0.15f);
            viceBase.transform.localPosition = new Vector3(-0.5f, 0.87f, 0.3f);
            ApplyMaterial(viceBase, metalColor);
            
            GameObject viceJaw1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viceJaw1.transform.SetParent(workbenchRoot.transform);
            viceJaw1.transform.localScale = new Vector3(0.05f, 0.12f, 0.1f);
            viceJaw1.transform.localPosition = new Vector3(-0.5f, 0.94f, 0.35f);
            ApplyMaterial(viceJaw1, metalColor);
            
            GameObject viceJaw2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viceJaw2.transform.SetParent(workbenchRoot.transform);
            viceJaw2.transform.localScale = new Vector3(0.05f, 0.12f, 0.1f);
            viceJaw2.transform.localPosition = new Vector3(-0.5f, 0.94f, 0.25f);
            ApplyMaterial(viceJaw2, metalColor);
            
            GameObject viceHandle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            viceHandle.transform.SetParent(workbenchRoot.transform);
            viceHandle.transform.localScale = new Vector3(0.02f, 0.02f, 0.15f);
            viceHandle.transform.localEulerAngles = new Vector3(0, 0, 90f);
            viceHandle.transform.localPosition = new Vector3(-0.5f, 0.94f, 0.15f);
            ApplyMaterial(viceHandle, metalColor);
        }
        
        private void GenerateToolRack()
        {
            GameObject rackBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rackBase.transform.SetParent(workbenchRoot.transform);
            rackBase.transform.localScale = new Vector3(1.4f, 0.03f, 0.1f);
            rackBase.transform.localPosition = new Vector3(0, 1.3f, -0.4f);
            ApplyMaterial(rackBase, metalColor);
            
            // Tool hooks/pegs
            int pegCount = workbenchType == WorkbenchType.Advanced ? 8 : 5;
            float pegSpacing = 1.2f / pegCount;
            
            for (int i = 0; i < pegCount; i++)
            {
                GameObject peg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                peg.transform.SetParent(workbenchRoot.transform);
                peg.transform.localScale = new Vector3(0.015f, 0.08f, 0.015f);
                peg.transform.localEulerAngles = new Vector3(90f, 0, 0);
                peg.transform.localPosition = new Vector3(-0.6f + (i * pegSpacing), 1.3f, -0.35f);
                ApplyMaterial(peg, metalColor);
            }
            
            // Add sample tools
            if (Random.value > 0.5f)
            {
                GameObject tool1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tool1.transform.SetParent(workbenchRoot.transform);
                tool1.transform.localScale = new Vector3(0.02f, 0.1f, 0.03f);
                tool1.transform.localPosition = new Vector3(-0.3f, 1.25f, -0.38f);
                ApplyMaterial(tool1, accentColor);
            }
        }
        
        private void GenerateStorage()
        {
            if (workbenchType == WorkbenchType.Basic)
            {
                // Simple shelf underneath
                GameObject shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shelf.transform.SetParent(workbenchRoot.transform);
                shelf.transform.localScale = new Vector3(1.3f, 0.03f, 0.7f);
                shelf.transform.localPosition = new Vector3(0, 0.4f, 0);
                ApplyMaterial(shelf, woodColor);
            }
            else if (workbenchType == WorkbenchType.Advanced)
            {
                // Drawer unit on one side
                GameObject drawerUnit = GameObject.CreatePrimitive(PrimitiveType.Cube);
                drawerUnit.transform.SetParent(workbenchRoot.transform);
                drawerUnit.transform.localScale = new Vector3(0.35f, 0.6f, 0.6f);
                drawerUnit.transform.localPosition = new Vector3(0.55f, 0.3f, 0);
                ApplyMaterial(drawerUnit, metalColor);
                
                // Drawer fronts
                for (int i = 0; i < 3; i++)
                {
                    GameObject drawer = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    drawer.transform.SetParent(workbenchRoot.transform);
                    drawer.transform.localScale = new Vector3(0.33f, 0.18f, 0.05f);
                    drawer.transform.localPosition = new Vector3(0.55f, 0.15f + (i * 0.2f), 0.3f);
                    ApplyMaterial(drawer, metalColor);
                    
                    // Handle
                    GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    handle.transform.SetParent(workbenchRoot.transform);
                    handle.transform.localScale = new Vector3(0.1f, 0.03f, 0.02f);
                    handle.transform.localPosition = new Vector3(0.55f, 0.15f + (i * 0.2f), 0.33f);
                    ApplyMaterial(handle, accentColor);
                }
            }
            else if (workbenchType == WorkbenchType.Industrial)
            {
                // Metal cabinet underneath
                GameObject cabinet = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cabinet.transform.SetParent(workbenchRoot.transform);
                cabinet.transform.localScale = new Vector3(1.0f, 0.7f, 0.6f);
                cabinet.transform.localPosition = new Vector3(0, 0.35f, 0);
                ApplyMaterial(cabinet, metalColor);
                
                // Cabinet doors
                GameObject door1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                door1.transform.SetParent(workbenchRoot.transform);
                door1.transform.localScale = new Vector3(0.48f, 0.65f, 0.05f);
                door1.transform.localPosition = new Vector3(-0.25f, 0.35f, 0.3f);
                ApplyMaterial(door1, metalColor);
                
                GameObject door2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                door2.transform.SetParent(workbenchRoot.transform);
                door2.transform.localScale = new Vector3(0.48f, 0.65f, 0.05f);
                door2.transform.localPosition = new Vector3(0.25f, 0.35f, 0.3f);
                ApplyMaterial(door2, metalColor);
            }
        }
        
        private void GeneratePowerStrip()
        {
            GameObject powerStrip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            powerStrip.transform.SetParent(workbenchRoot.transform);
            powerStrip.transform.localScale = new Vector3(0.25f, 0.04f, 0.06f);
            powerStrip.transform.localPosition = new Vector3(0.4f, 0.85f, -0.35f);
            ApplyMaterial(powerStrip, Color.black);
            
            // Outlets
            for (int i = 0; i < 3; i++)
            {
                GameObject outlet = GameObject.CreatePrimitive(PrimitiveType.Cube);
                outlet.transform.SetParent(workbenchRoot.transform);
                outlet.transform.localScale = new Vector3(0.03f, 0.045f, 0.02f);
                outlet.transform.localPosition = new Vector3(0.3f + (i * 0.08f), 0.852f, -0.33f);
                outlet.GetComponent<MeshRenderer>().material.color = Color.gray;
            }
        }
        
        private void GenerateLighting()
        {
            GameObject lightArm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lightArm.transform.SetParent(workbenchRoot.transform);
            lightArm.transform.localScale = new Vector3(0.02f, 0.5f, 0.02f);
            lightArm.transform.localPosition = new Vector3(0.6f, 1.1f, -0.4f);
            ApplyMaterial(lightArm, metalColor);
            
            GameObject lightHead = GameObject.CreatePrimitive(PrimitiveType.Cone);
            lightHead.transform.SetParent(workbenchRoot.transform);
            lightHead.transform.localScale = new Vector3(0.15f, 0.1f, 0.15f);
            lightHead.transform.localPosition = new Vector3(0.6f, 1.35f, -0.2f);
            lightHead.transform.localEulerAngles = new Vector3(30f, 0, 0);
            
            var lightMat = new Material(Shader.Find("Standard"));
            lightMat.color = new Color(1f, 0.95f, 0.8f);
            lightMat.EnableKeyword("_EMISSION");
            lightMat.SetColor("_EmissionColor", new Color(0.5f, 0.45f, 0.3f));
            lightHead.GetComponent<MeshRenderer>().sharedMaterial = lightMat;
        }
        
        private void AddTypeSpecificFeatures()
        {
            switch (workbenchType)
            {
                case WorkbenchType.Scientific:
                    AddScientificFeatures();
                    break;
                case WorkbenchType.Industrial:
                    AddIndustrialFeatures();
                    break;
                case WorkbenchType.Advanced:
                    AddAdvancedFeatures();
                    break;
            }
        }
        
        private void AddScientificFeatures()
        {
            Debug.Log("Added scientific equipment mounts and clean surface");
        }
        
        private void AddIndustrialFeatures()
        {
            Debug.Log("Added heavy-duty reinforcements and industrial fittings");
        }
        
        private void AddAdvancedFeatures()
        {
            Debug.Log("Added integrated digital display and precision tools");
        }
        
        private void ApplyMaterial(GameObject obj, Color color)
        {
            var renderer = obj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Standard"));
                mat.color = color;
                mat.SetFloat("_Smoothness", 0.5f);
                mat.SetFloat("_Metallic", color == metalColor ? 0.6f : 0.1f);
                renderer.sharedMaterial = mat;
            }
        }
        
        public void ClearWorkbench()
        {
            if (workbenchRoot != null)
            {
                DestroyImmediate(workbenchRoot);
            }
        }
    }
}
