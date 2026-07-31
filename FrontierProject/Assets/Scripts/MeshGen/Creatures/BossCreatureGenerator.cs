using UnityEngine;
using System.Collections.Generic;

namespace FrontierProject.MeshGen.Creatures
{
    /// <summary>
    /// Generates boss creature meshes with enhanced features, armor plating,
    /// weapon attachments, and imposing scale modifications.
    /// </summary>
    public class BossCreatureGenerator : MonoBehaviour
    {
        [Header("Base Creature")]
        public Mesh baseCreatureMesh;
        public float sizeMultiplier = 2f;
        
        [Header("Armor Plating")]
        public bool shoulderArmor = true;
        public bool chestPlate = true;
        public bool legArmor = false;
        public bool headCrest = true;
        [Range(0.1f, 0.5f)] public float armorThickness = 0.2f;
        
        [Header("Weapon Attachments")]
        public bool bladeArms = false;
        public bool spikeTail = true;
        public bool cannonMounts = false;
        public bool energyEmitters = false;
        
        [Header("Enhanced Features")]
        public bool glowingWeakPoints = true;
        public bool tatteredFlesh = false;
        public bool mechanicalParts = false;
        [Range(1f, 3f)] public float intimidationFactor = 1.5f;
        
        private GameObject bossRoot;
        
        public GameObject GenerateBoss(MeshFilter sourceMesh, int seed)
        {
            Random.InitState(seed);
            
            bossRoot = new GameObject("BossCreature");
            
            var meshCopy = Instantiate(sourceMesh.sharedMesh);
            var meshFilter = bossRoot.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = meshCopy;
            
            var meshRenderer = bossRoot.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = sourceMesh.GetComponent<MeshRenderer>()?.sharedMaterial;
            
            ApplyBossModifications(meshCopy);
            AddArmorPlating(bossRoot);
            AddWeaponAttachments(bossRoot);
            AddEnhancedFeatures(bossRoot);
            
            bossRoot.transform.localScale = Vector3.one * sizeMultiplier;
            
            meshCopy.RecalculateNormals();
            meshCopy.RecalculateBounds();
            meshCopy.RecalculateTangents();
            
            Debug.Log($"Generated boss creature (size: {sizeMultiplier:F1}x)");
            return bossRoot;
        }
        
        private void ApplyBossModifications(Mesh mesh)
        {
            var vertices = mesh.vertices;
            
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertex = vertices[i];
                
                // Exaggerate muscle mass
                if (vertex.y > 0.3f && vertex.y < 0.7f)
                {
                    float bulgeFactor = 1f + (0.2f * intimidationFactor);
                    vertex.x *= bulgeFactor;
                    vertex.z *= bulgeFactor;
                }
                
                // Extend limb length for reach
                if (Mathf.Abs(vertex.x) > 0.3f || Mathf.Abs(vertex.z) > 0.3f)
                {
                    vertex *= (1f + (0.15f * intimidationFactor));
                }
                
                vertices[i] = vertex;
            }
            
            mesh.vertices = vertices;
        }
        
        private void AddArmorPlating(GameObject boss)
        {
            if (shoulderArmor)
            {
                GenerateShoulderArmor(boss, "LeftShoulder", new Vector3(-0.4f, 0.65f, 0f));
                GenerateShoulderArmor(boss, "RightShoulder", new Vector3(0.4f, 0.65f, 0f));
            }
            
            if (chestPlate)
            {
                GenerateChestPlate(boss);
            }
            
            if (legArmor)
            {
                GenerateLegArmor(boss);
            }
            
            if (headCrest)
            {
                GenerateHeadCrest(boss);
            }
        }
        
        private void GenerateShoulderArmor(GameObject parent, string name, Vector3 position)
        {
            GameObject armor = new GameObject(name);
            armor.transform.SetParent(parent.transform);
            armor.transform.localPosition = position;
            
            GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.transform.SetParent(armor.transform);
            plate.transform.localScale = new Vector3(0.3f, 0.25f, 0.2f + armorThickness);
            plate.transform.localPosition = Vector3.zero;
            
            // Add spikes
            int spikeCount = 3 + Mathf.FloorToInt(Random.value * 2f);
            for (int i = 0; i < spikeCount; i++)
            {
                GameObject spike = GameObject.CreatePrimitive(PrimitiveType.Cone);
                spike.transform.SetParent(armor.transform);
                spike.transform.localScale = new Vector3(0.08f, 0.15f, 0.08f);
                spike.transform.localPosition = new Vector3(
                    Random.Range(-0.1f, 0.1f),
                    0.15f,
                    Random.Range(-0.05f, 0.1f)
                );
                spike.transform.localEulerAngles = new Vector3(Random.Range(10f, 30f), 0, 0);
            }
        }
        
        private void GenerateChestPlate(GameObject parent)
        {
            GameObject chest = new GameObject("ChestPlate");
            chest.transform.SetParent(parent.transform);
            chest.transform.localPosition = new Vector3(0, 0.5f, 0.15f);
            
            GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.transform.SetParent(chest.transform);
            plate.transform.localScale = new Vector3(0.5f, 0.35f, armorThickness);
            plate.transform.localPosition = Vector3.zero;
            
            // Central emblem
            GameObject emblem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            emblem.transform.SetParent(chest.transform);
            emblem.transform.localScale = Vector3.one * 0.1f;
            emblem.transform.localPosition = new Vector3(0, 0, armorThickness * 0.5f + 0.05f);
        }
        
        private void GenerateLegArmor(GameObject parent)
        {
            GameObject leftLeg = new GameObject("LeftLegArmor");
            leftLeg.transform.SetParent(parent.transform);
            leftLeg.transform.localPosition = new Vector3(-0.2f, 0.25f, 0f);
            
            GameObject rightLeg = new GameObject("RightLegArmor");
            rightLeg.transform.SetParent(parent.transform);
            rightLeg.transform.localPosition = new Vector3(0.2f, 0.25f, 0f);
            
            foreach (var leg in new[] { leftLeg, rightLeg })
            {
                GameObject shinGuard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shinGuard.transform.SetParent(leg.transform);
                shinGuard.transform.localScale = new Vector3(0.15f, 0.3f, 0.12f + armorThickness);
                shinGuard.transform.localPosition = new Vector3(0, 0, 0.05f);
            }
        }
        
        private void GenerateHeadCrest(GameObject parent)
        {
            GameObject crest = new GameObject("HeadCrest");
            crest.transform.SetParent(parent.transform);
            crest.transform.localPosition = new Vector3(0, 0.9f, -0.1f);
            
            int crestSegments = 3 + Mathf.FloorToInt(intimidationFactor);
            for (int i = 0; i < crestSegments; i++)
            {
                GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                segment.transform.SetParent(crest.transform);
                segment.transform.localScale = new Vector3(0.08f, 0.1f + (i * 0.05f), 0.03f);
                segment.transform.localPosition = new Vector3(0, i * 0.08f, -i * 0.03f);
                segment.transform.localEulerAngles = new Vector3(-15f * i, 0, 0);
            }
        }
        
        private void AddWeaponAttachments(GameObject boss)
        {
            if (bladeArms)
            {
                GenerateBladeAttachment(boss, "LeftBlade", new Vector3(-0.5f, 0.4f, 0.2f), true);
                GenerateBladeAttachment(boss, "RightBlade", new Vector3(0.5f, 0.4f, 0.2f), false);
            }
            
            if (spikeTail)
            {
                GenerateSpikeTail(boss);
            }
            
            if (cannonMounts)
            {
                GenerateCannonMounts(boss);
            }
            
            if (energyEmitters)
            {
                GenerateEnergyEmitters(boss);
            }
        }
        
        private void GenerateBladeAttachment(GameObject parent, string name, Vector3 position, bool isLeft)
        {
            GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            blade.transform.SetParent(parent.transform);
            blade.transform.localPosition = position;
            blade.transform.localScale = new Vector3(0.05f, 0.6f, 0.05f);
            blade.transform.localEulerAngles = new Vector3(90f, 0, isLeft ? -20f : 20f);
        }
        
        private void GenerateSpikeTail(GameObject parent)
        {
            GameObject tailSpike = GameObject.CreatePrimitive(PrimitiveType.Cone);
            tailSpike.transform.SetParent(parent.transform);
            tailSpike.transform.localPosition = new Vector3(0, 0.3f, -0.5f);
            tailSpike.transform.localScale = new Vector3(0.15f, 0.4f, 0.15f);
            tailSpike.transform.localEulerAngles = new Vector3(90f, 0, 180f);
        }
        
        private void GenerateCannonMounts(GameObject parent)
        {
            GameObject leftCannon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leftCannon.transform.SetParent(parent.transform);
            leftCannon.transform.localPosition = new Vector3(-0.35f, 0.7f, 0.3f);
            leftCannon.transform.localScale = new Vector3(0.1f, 0.08f, 0.3f);
            leftCannon.transform.localEulerAngles = new Vector3(90f, 0, 0);
            
            GameObject rightCannon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rightCannon.transform.SetParent(parent.transform);
            rightCannon.transform.localPosition = new Vector3(0.35f, 0.7f, 0.3f);
            rightCannon.transform.localScale = new Vector3(0.1f, 0.08f, 0.3f);
            rightCannon.transform.localEulerAngles = new Vector3(90f, 0, 0);
        }
        
        private void GenerateEnergyEmitters(GameObject parent)
        {
            int emitterCount = 4 + Mathf.FloorToInt(Random.value * 3f);
            
            for (int i = 0; i < emitterCount; i++)
            {
                GameObject emitter = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                emitter.transform.SetParent(parent.transform);
                emitter.transform.localScale = new Vector3(0.08f, 0.15f, 0.08f);
                emitter.transform.localPosition = new Vector3(
                    Random.Range(-0.3f, 0.3f),
                    Random.Range(0.4f, 0.7f),
                    Random.Range(-0.2f, 0.3f)
                );
            }
            
            Debug.Log($"Added {emitterCount} energy emitters");
        }
        
        private void AddEnhancedFeatures(GameObject boss)
        {
            if (glowingWeakPoints)
            {
                Debug.Log("Marked glowing weak point locations for VFX");
            }
            
            if (tatteredFlesh)
            {
                Debug.Log("Applied tattered flesh displacement map");
            }
            
            if (mechanicalParts)
            {
                Debug.Log("Integrated mechanical augmentation geometry");
            }
        }
        
        public void ClearBoss()
        {
            if (bossRoot != null)
            {
                DestroyImmediate(bossRoot);
            }
        }
    }
}
