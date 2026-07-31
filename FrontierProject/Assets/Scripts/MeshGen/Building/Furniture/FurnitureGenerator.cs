using UnityEngine;
using System.Collections.Generic;

namespace FrontierProject.MeshGen.Building.Furniture
{
    /// <summary>
    /// Generates procedural furniture pieces including tables, chairs, 
    /// cabinets, beds, and storage units.
    /// </summary>
    public class FurnitureGenerator : MonoBehaviour
    {
        public enum FurnitureType { Table, Chair, Cabinet, Bed, Shelf, Desk }
        
        [Header("Generation Settings")]
        public FurnitureType furnitureType = FurnitureType.Table;
        [Range(0.5f, 3f)] public float scaleMultiplier = 1f;
        
        [Header("Style Options")]
        public bool modernStyle = true;
        public bool rusticStyle = false;
        public bool industrialStyle = false;
        
        [Header("Material")]
        public Color woodColor = new Color(0.6f, 0.4f, 0.2f);
        public Color metalColor = Color.gray;
        
        private GameObject furnitureRoot;
        
        public GameObject GenerateFurniture(int seed)
        {
            Random.InitState(seed);
            
            furnitureRoot = new GameObject($"Furniture_{furnitureType}");
            
            switch (furnitureType)
            {
                case FurnitureType.Table:
                    GenerateTable();
                    break;
                case FurnitureType.Chair:
                    GenerateChair();
                    break;
                case FurnitureType.Cabinet:
                    GenerateCabinet();
                    break;
                case FurnitureType.Bed:
                    GenerateBed();
                    break;
                case FurnitureType.Shelf:
                    GenerateShelf();
                    break;
                case FurnitureType.Desk:
                    GenerateDesk();
                    break;
            }
            
            furnitureRoot.transform.localScale = Vector3.one * scaleMultiplier;
            Debug.Log($"Generated {furnitureType} (style: {(modernStyle ? "Modern" : rusticStyle ? "Rustic" : "Industrial")})");
            
            return furnitureRoot;
        }
        
        private void GenerateTable()
        {
            // Table top
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.transform.SetParent(furnitureRoot.transform);
            top.transform.localScale = new Vector3(1.2f, 0.05f, 0.8f);
            top.transform.localPosition = new Vector3(0, 0.75f, 0);
            ApplyMaterial(top, modernStyle ? metalColor : woodColor);
            
            // Legs
            float legSpacingX = 0.5f;
            float legSpacingZ = 0.3f;
            
            CreateLeg(new Vector3(-legSpacingX, 0.375f, -legSpacingZ));
            CreateLeg(new Vector3(legSpacingX, 0.375f, -legSpacingZ));
            CreateLeg(new Vector3(-legSpacingX, 0.375f, legSpacingZ));
            CreateLeg(new Vector3(legSpacingX, 0.375f, legSpacingZ));
            
            if (rusticStyle)
            {
                AddCrossBracing();
            }
        }
        
        private void CreateLeg(Vector3 position)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.transform.SetParent(furnitureRoot.transform);
            leg.transform.localPosition = position;
            leg.transform.localScale = new Vector3(0.05f, 0.75f, 0.05f);
            
            if (industrialStyle)
            {
                leg.transform.localScale = new Vector3(0.04f, 0.75f, 0.04f);
                ApplyMaterial(leg, metalColor);
            }
            else
            {
                ApplyMaterial(leg, woodColor);
            }
        }
        
        private void AddCrossBracing()
        {
            // Add horizontal supports between legs
            GameObject brace1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            brace1.transform.SetParent(furnitureRoot.transform);
            brace1.transform.localScale = new Vector3(0.08f, 0.05f, 0.6f);
            brace1.transform.localPosition = new Vector3(0, 0.2f, 0);
            ApplyMaterial(brace1, woodColor);
        }
        
        private void GenerateChair()
        {
            // Seat
            GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.transform.SetParent(furnitureRoot.transform);
            seat.transform.localScale = new Vector3(0.45f, 0.05f, 0.4f);
            seat.transform.localPosition = new Vector3(0, 0.45f, 0);
            ApplyMaterial(seat, modernStyle ? metalColor : woodColor);
            
            // Legs
            float legOffset = 0.18f;
            CreateChairLeg(new Vector3(-legOffset, 0.225f, -legOffset));
            CreateChairLeg(new Vector3(legOffset, 0.225f, -legOffset));
            CreateChairLeg(new Vector3(-legOffset, 0.225f, legOffset));
            CreateChairLeg(new Vector3(legOffset, 0.225f, legOffset));
            
            // Backrest
            GameObject backrest = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backrest.transform.SetParent(furnitureRoot.transform);
            backrest.transform.localScale = new Vector3(0.4f, 0.5f, 0.05f);
            backrest.transform.localPosition = new Vector3(0, 0.75f, -0.18f);
            backrest.transform.localEulerAngles = new Vector3(-5f, 0, 0);
            ApplyMaterial(backrest, modernStyle ? metalColor : woodColor);
            
            if (!modernStyle)
            {
                // Add armrests
                CreateArmrest(-0.25f);
                CreateArmrest(0.25f);
            }
        }
        
        private void CreateChairLeg(Vector3 position)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.transform.SetParent(furnitureRoot.transform);
            leg.transform.localPosition = position;
            leg.transform.localScale = new Vector3(0.04f, 0.45f, 0.04f);
            ApplyMaterial(leg, woodColor);
        }
        
        private void CreateArmrest(float xOffset)
        {
            GameObject armrest = GameObject.CreatePrimitive(PrimitiveType.Cube);
            armrest.transform.SetParent(furnitureRoot.transform);
            armrest.transform.localScale = new Vector3(0.05f, 0.05f, 0.35f);
            armrest.transform.localPosition = new Vector3(xOffset, 0.65f, 0);
            ApplyMaterial(armrest, woodColor);
        }
        
        private void GenerateCabinet()
        {
            // Main body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(furnitureRoot.transform);
            body.transform.localScale = new Vector3(0.8f, 1.2f, 0.4f);
            body.transform.localPosition = new Vector3(0, 0.6f, 0);
            ApplyMaterial(body, woodColor);
            
            // Doors
            GameObject doorLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorLeft.transform.SetParent(furnitureRoot.transform);
            doorLeft.transform.localScale = new Vector3(0.38f, 0.55f, 0.05f);
            doorLeft.transform.localPosition = new Vector3(-0.2f, 0.6f, 0.2f);
            ApplyMaterial(doorLeft, woodColor);
            
            GameObject doorRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorRight.transform.SetParent(furnitureRoot.transform);
            doorRight.transform.localScale = new Vector3(0.38f, 0.55f, 0.05f);
            doorRight.transform.localPosition = new Vector3(0.2f, 0.6f, 0.2f);
            ApplyMaterial(doorRight, woodColor);
            
            // Handles
            CreateDoorHandle(-0.15f, 0.6f);
            CreateDoorHandle(0.15f, 0.6f);
            
            // Top surface
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.transform.SetParent(furnitureRoot.transform);
            top.transform.localScale = new Vector3(0.85f, 0.05f, 0.45f);
            top.transform.localPosition = new Vector3(0, 1.225f, 0);
            ApplyMaterial(top, woodColor);
        }
        
        private void CreateDoorHandle(float x, float y)
        {
            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            handle.transform.SetParent(furnitureRoot.transform);
            handle.transform.localScale = Vector3.one * 0.03f;
            handle.transform.localPosition = new Vector3(x, y, 0.23f);
            ApplyMaterial(handle, metalColor);
        }
        
        private void GenerateBed()
        {
            // Frame
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.transform.SetParent(furnitureRoot.transform);
            frame.transform.localScale = new Vector3(1.6f, 0.1f, 2.0f);
            frame.transform.localPosition = new Vector3(0, 0.3f, 0);
            ApplyMaterial(frame, woodColor);
            
            // Headboard
            GameObject headboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            headboard.transform.SetParent(furnitureRoot.transform);
            headboard.transform.localScale = new Vector3(1.65f, 0.8f, 0.1f);
            headboard.transform.localPosition = new Vector3(0, 0.75f, -0.95f);
            ApplyMaterial(headboard, woodColor);
            
            // Mattress
            GameObject mattress = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mattress.transform.SetParent(furnitureRoot.transform);
            mattress.transform.localScale = new Vector3(1.5f, 0.15f, 1.9f);
            mattress.transform.localPosition = new Vector3(0, 0.48f, 0);
            
            var matRenderer = mattress.GetComponent<MeshRenderer>();
            if (matRenderer != null)
            {
                var mat = new Material(Shader.Find("Standard"));
                mat.color = Color.white;
                mat.SetFloat("_Smoothness", 0.4f);
                matRenderer.sharedMaterial = mat;
            }
            
            // Legs
            float bedLegOffsetX = 0.7f;
            float bedLegOffsetZ = 0.9f;
            CreateBedLeg(new Vector3(-bedLegOffsetX, 0.15f, -bedLegOffsetZ));
            CreateBedLeg(new Vector3(bedLegOffsetX, 0.15f, -bedLegOffsetZ));
            CreateBedLeg(new Vector3(-bedLegOffsetX, 0.15f, bedLegOffsetZ));
            CreateBedLeg(new Vector3(bedLegOffsetX, 0.15f, bedLegOffsetZ));
        }
        
        private void CreateBedLeg(Vector3 position)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.transform.SetParent(furnitureRoot.transform);
            leg.transform.localPosition = position;
            leg.transform.localScale = new Vector3(0.06f, 0.3f, 0.06f);
            ApplyMaterial(leg, woodColor);
        }
        
        private void GenerateShelf()
        {
            // Side panels
            GameObject leftPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftPanel.transform.SetParent(furnitureRoot.transform);
            leftPanel.transform.localScale = new Vector3(0.05f, 1.5f, 0.3f);
            leftPanel.transform.localPosition = new Vector3(-0.4f, 0.75f, 0);
            ApplyMaterial(leftPanel, woodColor);
            
            GameObject rightPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightPanel.transform.SetParent(furnitureRoot.transform);
            rightPanel.transform.localScale = new Vector3(0.05f, 1.5f, 0.3f);
            rightPanel.transform.localPosition = new Vector3(0.4f, 0.75f, 0);
            ApplyMaterial(rightPanel, woodColor);
            
            // Shelves
            for (int i = 0; i < 4; i++)
            {
                GameObject shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shelf.transform.SetParent(furnitureRoot.transform);
                shelf.transform.localScale = new Vector3(0.75f, 0.03f, 0.28f);
                shelf.transform.localPosition = new Vector3(0, 0.2f + (i * 0.4f), 0);
                ApplyMaterial(shelf, woodColor);
            }
            
            // Top
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.transform.SetParent(furnitureRoot.transform);
            top.transform.localScale = new Vector3(0.85f, 0.05f, 0.3f);
            top.transform.localPosition = new Vector3(0, 1.525f, 0);
            ApplyMaterial(top, woodColor);
        }
        
        private void GenerateDesk()
        {
            // Desktop
            GameObject desktop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            desktop.transform.SetParent(furnitureRoot.transform);
            desktop.transform.localScale = new Vector3(1.4f, 0.05f, 0.7f);
            desktop.transform.localPosition = new Vector3(0, 0.75f, 0);
            ApplyMaterial(desktop, modernStyle ? metalColor : woodColor);
            
            // Leg panels
            GameObject leftPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftPanel.transform.SetParent(furnitureRoot.transform);
            leftPanel.transform.localScale = new Vector3(0.05f, 0.75f, 0.6f);
            leftPanel.transform.localPosition = new Vector3(-0.6f, 0.375f, 0);
            ApplyMaterial(leftPanel, metalColor);
            
            GameObject rightPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightPanel.transform.SetParent(furnitureRoot.transform);
            rightPanel.transform.localScale = new Vector3(0.05f, 0.75f, 0.6f);
            rightPanel.transform.localPosition = new Vector3(0.6f, 0.375f, 0);
            ApplyMaterial(rightPanel, metalColor);
            
            // Drawer unit
            if (!modernStyle)
            {
                GameObject drawers = GameObject.CreatePrimitive(PrimitiveType.Cube);
                drawers.transform.SetParent(furnitureRoot.transform);
                drawers.transform.localScale = new Vector3(0.3f, 0.5f, 0.6f);
                drawers.transform.localPosition = new Vector3(0.4f, 0.25f, 0);
                ApplyMaterial(drawers, woodColor);
                
                // Drawer fronts
                for (int i = 0; i < 3; i++)
                {
                    GameObject drawerFront = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    drawerFront.transform.SetParent(furnitureRoot.transform);
                    drawerFront.transform.localScale = new Vector3(0.28f, 0.15f, 0.05f);
                    drawerFront.transform.localPosition = new Vector3(0.4f, 0.15f + (i * 0.18f), 0.3f);
                    ApplyMaterial(drawerFront, woodColor);
                }
            }
        }
        
        private void ApplyMaterial(GameObject obj, Color color)
        {
            var renderer = obj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Standard"));
                mat.color = color;
                mat.SetFloat("_Smoothness", modernStyle ? 0.6f : 0.3f);
                mat.SetFloat("_Metallic", industrialStyle ? 0.7f : 0f);
                renderer.sharedMaterial = mat;
            }
        }
        
        public void ClearFurniture()
        {
            if (furnitureRoot != null)
            {
                DestroyImmediate(furnitureRoot);
            }
        }
    }
}
