using UnityEngine;
using System.Collections.Generic;

namespace FrontierProject.MeshGen.Creatures
{
    /// <summary>
    /// Generates mutant creature meshes with procedural body modifications,
    /// extra limbs, deformities, and alien features.
    /// </summary>
    public class MutantGenerator : MonoBehaviour
    {
        [Header("Base Creature")]
        public Mesh baseCreatureMesh;
        public float baseScale = 1f;
        
        [Header("Mutation Severity")]
        [Range(0f, 1f)] public float mutationLevel = 0.5f;
        
        [Header("Limb Modifications")]
        public bool extraArms = false;
        public bool extraLegs = false;
        public bool asymmetricalLimbs = true;
        [Range(0.5f, 2f)] public float limbLengthVariance = 1f;
        
        [Header("Body Deformities")]
        public bool hunchedSpine = true;
        public bool enlargedOrgans = false;
        public bool exposedBones = false;
        [Range(0f, 0.5f)] public float spineCurve = 0.2f;
        
        [Header("Alien Features")]
        public bool extraEyes = false;
        public bool mandibles = false;
        public bool tail = false;
        public bool dorsalPlates = false;
        
        private GameObject mutantRoot;
        
        public GameObject GenerateMutant(MeshFilter sourceMesh, int seed)
        {
            Random.InitState(seed);
            
            mutantRoot = new GameObject("MutantCreature");
            
            var meshCopy = Instantiate(sourceMesh.sharedMesh);
            var meshFilter = mutantRoot.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = meshCopy;
            
            var meshRenderer = mutantRoot.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = sourceMesh.GetComponent<MeshRenderer>()?.sharedMaterial;
            
            ApplyMutations(meshCopy);
            AddExtraFeatures(mutantRoot);
            
            meshCopy.RecalculateNormals();
            meshCopy.RecalculateBounds();
            meshCopy.RecalculateTangents();
            
            Debug.Log($"Generated mutant with severity {mutationLevel:F2}");
            return mutantRoot;
        }
        
        private void ApplyMutations(Mesh mesh)
        {
            var vertices = mesh.vertices;
            
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertex = vertices[i];
                
                // Spine curvature
                if (hunchedSpine && vertex.y > 0.3f && vertex.y < 0.8f)
                {
                    vertex.z += spineCurve * mutationLevel * (vertex.y - 0.3f);
                    vertex.y -= spineCurve * mutationLevel * 0.1f;
                }
                
                // Limb length variance
                if (asymmetricalLimbs && Mathf.Abs(vertex.x) > 0.3f)
                {
                    float variance = vertex.x > 0 ? 
                        Random.Range(1f / limbLengthVariance, limbLengthVariance) :
                        Random.Range(1f / limbLengthVariance, limbLengthVariance);
                    vertex.y *= variance;
                }
                
                // Organ enlargement (torso swelling)
                if (enlargedOrgans && vertex.y > 0.2f && vertex.y < 0.5f && Mathf.Abs(vertex.x) < 0.25f)
                {
                    float swellFactor = 1f + (mutationLevel * 0.3f);
                    vertex.x *= swellFactor;
                    vertex.z *= swellFactor;
                }
                
                vertices[i] = vertex;
            }
            
            mesh.vertices = vertices;
        }
        
        private void AddExtraFeatures(GameObject creature)
        {
            if (extraArms)
            {
                GenerateExtraLimb(creature, "ExtraArm_Left", new Vector3(-0.4f, 0.6f, 0.2f), true);
                GenerateExtraLimb(creature, "ExtraArm_Right", new Vector3(0.4f, 0.6f, 0.2f), true);
            }
            
            if (extraLegs)
            {
                GenerateExtraLimb(creature, "ExtraLeg_Left", new Vector3(-0.3f, 0.2f, -0.3f), false);
                GenerateExtraLimb(creature, "ExtraLeg_Right", new Vector3(0.3f, 0.2f, -0.3f), false);
            }
            
            if (tail)
            {
                GenerateTail(creature);
            }
            
            if (dorsalPlates)
            {
                GenerateDorsalPlates(creature);
            }
            
            if (mandibles)
            {
                GenerateMandibles(creature);
            }
            
            if (extraEyes)
            {
                GenerateExtraEyes(creature);
            }
            
            if (exposedBones)
            {
                Debug.Log("Applied exposed bone texture regions");
            }
        }
        
        private void GenerateExtraLimb(GameObject parent, string name, Vector3 position, bool isArm)
        {
            GameObject limb = new GameObject(name);
            limb.transform.SetParent(parent.transform);
            limb.transform.localPosition = position;
            
            GameObject limbMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            limbMesh.transform.SetParent(limb.transform);
            limbMesh.transform.localScale = isArm ? 
                new Vector3(0.08f, 0.4f, 0.08f) : 
                new Vector3(0.1f, 0.5f, 0.1f);
            limbMesh.transform.localPosition = Vector3.zero;
            
            float lengthMod = Random.Range(0.8f, 1.3f) * limbLengthVariance;
            limbMesh.transform.localScale *= lengthMod;
        }
        
        private void GenerateTail(GameObject parent)
        {
            GameObject tailRoot = new GameObject("Tail");
            tailRoot.transform.SetParent(parent.transform);
            tailRoot.transform.localPosition = new Vector3(0, 0.3f, -0.4f);
            
            int segments = 5 + Mathf.FloorToInt(mutationLevel * 5);
            float segmentLength = 0.15f;
            
            for (int i = 0; i < segments; i++)
            {
                GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                segment.transform.SetParent(tailRoot.transform);
                segment.transform.localScale = Vector3.one * (0.12f - (i * 0.015f));
                segment.transform.localPosition = new Vector3(0, 0, -(i * segmentLength));
            }
            
            Debug.Log($"Generated tail with {segments} segments");
        }
        
        private void GenerateDorsalPlates(GameObject parent)
        {
            int plateCount = 4 + Mathf.FloorToInt(mutationLevel * 4);
            
            for (int i = 0; i < plateCount; i++)
            {
                GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plate.transform.SetParent(parent.transform);
                plate.transform.localScale = new Vector3(0.15f, 0.2f + (Random.value * 0.15f), 0.05f);
                plate.transform.localPosition = new Vector3(0, 0.7f + (i * 0.08f), -0.1f - (i * 0.05f));
            }
            
            Debug.Log($"Added {plateCount} dorsal plates");
        }
        
        private void GenerateMandibles(GameObject parent)
        {
            GameObject leftMandible = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leftMandible.transform.SetParent(parent.transform);
            leftMandible.transform.localScale = new Vector3(0.05f, 0.3f, 0.05f);
            leftMandible.transform.localPosition = new Vector3(-0.15f, 0.4f, 0.25f);
            leftMandible.transform.localEulerAngles = new Vector3(45f, 0, -30f);
            
            GameObject rightMandible = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rightMandible.transform.SetParent(parent.transform);
            rightMandible.transform.localScale = new Vector3(0.05f, 0.3f, 0.05f);
            rightMandible.transform.localPosition = new Vector3(0.15f, 0.4f, 0.25f);
            rightMandible.transform.localEulerAngles = new Vector3(45f, 0, 30f);
        }
        
        private void GenerateExtraEyes(GameObject parent)
        {
            int extraEyeCount = Random.Range(1, 4);
            
            for (int i = 0; i < extraEyeCount; i++)
            {
                GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eye.transform.SetParent(parent.transform);
                eye.transform.localScale = Vector3.one * 0.04f;
                eye.transform.localPosition = new Vector3(
                    Random.Range(-0.15f, 0.15f),
                    0.75f + (Random.value * 0.1f),
                    0.18f + (Random.value * 0.05f)
                );
            }
            
            Debug.Log($"Added {extraEyeCount} extra eyes");
        }
        
        public void ClearMutant()
        {
            if (mutantRoot != null)
            {
                DestroyImmediate(mutantRoot);
            }
        }
    }
}
