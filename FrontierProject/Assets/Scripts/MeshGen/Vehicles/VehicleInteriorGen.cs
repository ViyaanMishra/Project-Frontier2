using UnityEngine;
using System.Collections.Generic;

namespace FrontierProject.MeshGen.Vehicles
{
    /// <summary>
    /// Generates interior components for vehicles including seats, 
    /// dashboard, controls, and cabin details.
    /// </summary>
    public class VehicleInteriorGen : MonoBehaviour
    {
        [Header("Seat Configuration")]
        public int seatCount = 2;
        public bool bucketSeats = true;
        public bool heatedSeats = false;
        public Color seatColor = Color.black;
        
        [Header("Dashboard Style")]
        public enum DashboardStyle { Minimal, Standard, Luxury, Tactical }
        public DashboardStyle dashboardStyle = DashboardStyle.Standard;
        
        [Header("Control Layout")]
        public bool analogGauges = true;
        public bool digitalDisplay = false;
        public bool touchscreenNav = false;
        
        [Header("Cabin Details")]
        public bool floorMats = true;
        public bool overheadConsole = false;
        public bool storageCompartments = true;
        public bool rollCage = false;
        
        private GameObject interiorRoot;
        
        public GameObject GenerateInterior(GameObject vehicleParent, int seed)
        {
            Random.InitState(seed);
            
            interiorRoot = new GameObject("VehicleInterior");
            interiorRoot.transform.SetParent(vehicleParent.transform);
            interiorRoot.transform.localPosition = Vector3.zero;
            
            GenerateSeats();
            GenerateDashboard();
            GenerateControls();
            GenerateCabinDetails();
            
            return interiorRoot;
        }
        
        private void GenerateSeats()
        {
            float seatSpacing = 0.6f;
            float startOffset = -(seatCount - 1) * seatSpacing / 2f;
            
            for (int i = 0; i < seatCount; i++)
            {
                GameObject seat = CreateSeatMesh(i, startOffset + (i * seatSpacing));
                seat.transform.SetParent(interiorRoot.transform);
            }
            
            Debug.Log($"Generated {seatCount} seats (bucket: {bucketSeats})");
        }
        
        private GameObject CreateSeatMesh(int index, float xOffset)
        {
            GameObject seat = new GameObject($"Seat_{index}");
            seat.transform.localPosition = new Vector3(xOffset, 0.5f, 0.5f);
            
            // Seat base
            GameObject seatBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seatBase.transform.SetParent(seat.transform);
            seatBase.transform.localScale = new Vector3(0.5f, 0.2f, 0.5f);
            seatBase.transform.localPosition = Vector3.zero;
            
            // Seat back
            GameObject seatBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seatBack.transform.SetParent(seat.transform);
            seatBack.transform.localScale = new Vector3(0.5f, 0.6f, 0.1f);
            seatBack.transform.localPosition = new Vector3(0, 0.4f, -0.2f);
            seatBack.transform.localEulerAngles = new Vector3(-10f, 0, 0);
            
            if (bucketSeats)
            {
                // Add side bolsters
                GameObject bolsterLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bolsterLeft.transform.SetParent(seat.transform);
                bolsterLeft.transform.localScale = new Vector3(0.1f, 0.4f, 0.4f);
                bolsterLeft.transform.localPosition = new Vector3(-0.2f, 0.2f, 0);
                
                GameObject bolsterRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bolsterRight.transform.SetParent(seat.transform);
                bolsterRight.transform.localScale = new Vector3(0.1f, 0.4f, 0.4f);
                bolsterRight.transform.localPosition = new Vector3(0.2f, 0.2f, 0);
            }
            
            // Apply material
            var renderer = seat.GetComponentInChildren<MeshRenderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Standard"));
                mat.color = seatColor;
                mat.SetFloat("_Smoothness", 0.3f);
                renderer.sharedMaterial = mat;
            }
            
            return seat;
        }
        
        private void GenerateDashboard()
        {
            GameObject dash = new GameObject("Dashboard");
            dash.transform.SetParent(interiorRoot.transform);
            dash.transform.localPosition = new Vector3(0, 0.8f, 2.5f);
            
            switch (dashboardStyle)
            {
                case DashboardStyle.Minimal:
                    dash.transform.localScale = new Vector3(1.8f, 0.3f, 0.4f);
                    break;
                case DashboardStyle.Standard:
                    dash.transform.localScale = new Vector3(1.9f, 0.4f, 0.5f);
                    break;
                case DashboardStyle.Luxury:
                    dash.transform.localScale = new Vector3(2f, 0.5f, 0.6f);
                    break;
                case DashboardStyle.Tactical:
                    dash.transform.localScale = new Vector3(1.8f, 0.4f, 0.7f);
                    break;
            }
            
            GameObject dashMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dashMesh.transform.SetParent(dash.transform);
            dashMesh.transform.localPosition = Vector3.zero;
            dashMesh.transform.localScale = Vector3.one;
            
            Debug.Log($"Generated dashboard ({dashboardStyle})");
        }
        
        private void GenerateControls()
        {
            GameObject controls = new GameObject("Controls");
            controls.transform.SetParent(interiorRoot.transform);
            controls.transform.localPosition = new Vector3(0.7f, 0.9f, 2.3f);
            
            // Steering wheel placeholder
            GameObject steeringWheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            steeringWheel.transform.SetParent(controls.transform);
            steeringWheel.transform.localScale = new Vector3(0.05f, 0.02f, 0.35f);
            steeringWheel.transform.localEulerAngles = new Vector3(90f, 0, 0);
            steeringWheel.transform.localPosition = new Vector3(0, 0, 0.1f);
            
            // Instrument cluster
            GameObject cluster = new GameObject("InstrumentCluster");
            cluster.transform.SetParent(controls.transform);
            cluster.transform.localPosition = new Vector3(-0.3f, 0.05f, 0.15f);
            
            if (analogGauges)
            {
                Debug.Log("Generated analog gauge cluster");
            }
            
            if (digitalDisplay)
            {
                GameObject display = GameObject.CreatePrimitive(PrimitiveType.Cube);
                display.transform.SetParent(cluster.transform);
                display.transform.localScale = new Vector3(0.2f, 0.1f, 0.02f);
                Debug.Log("Added digital display panel");
            }
            
            if (touchscreenNav)
            {
                GameObject navScreen = GameObject.CreatePrimitive(PrimitiveType.Cube);
                navScreen.transform.SetParent(controls.transform);
                navScreen.transform.localPosition = new Vector3(0.2f, 0.1f, 0.1f);
                navScreen.transform.localScale = new Vector3(0.15f, 0.1f, 0.02f);
                Debug.Log("Added touchscreen navigation");
            }
        }
        
        private void GenerateCabinDetails()
        {
            if (floorMats)
            {
                GameObject mats = new GameObject("FloorMats");
                mats.transform.SetParent(interiorRoot.transform);
                mats.transform.localPosition = new Vector3(0, 0.01f, 1f);
                mats.transform.localScale = new Vector3(1.6f, 0.01f, 0.8f);
            }
            
            if (overheadConsole)
            {
                GameObject console = new GameObject("OverheadConsole");
                console.transform.SetParent(interiorRoot.transform);
                console.transform.localPosition = new Vector3(0, 1.4f, 1.5f);
                console.transform.localScale = new Vector3(0.8f, 0.1f, 0.4f);
            }
            
            if (storageCompartments)
            {
                GameObject glovebox = new GameObject("GloveBox");
                glovebox.transform.SetParent(interiorRoot.transform);
                glovebox.transform.localPosition = new Vector3(0.6f, 0.6f, 2.6f);
                glovebox.transform.localScale = new Vector3(0.3f, 0.15f, 0.2f);
            }
            
            if (rollCage)
            {
                GameObject cage = new GameObject("RollCage");
                cage.transform.SetParent(interiorRoot.transform);
                // Would generate tube mesh structure
                Debug.Log("Generated roll cage structure");
            }
        }
        
        public void ClearInterior()
        {
            if (interiorRoot != null)
            {
                DestroyImmediate(interiorRoot);
            }
        }
    }
}
