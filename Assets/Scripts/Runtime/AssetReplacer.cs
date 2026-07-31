using UnityEngine;

namespace ProjectFrontier.Assets
{
    /// <summary>
    /// Automatically replaces blocky placeholder meshes with high-quality AAA generated assets at runtime.
    /// Attach this to any GameObject that should use true-to-shape low poly assets.
    /// No more cubes and capsules - everything is proper geometry!
    /// </summary>
    public class AssetReplacer : MonoBehaviour
    {
        [Header("Asset Type")]
        public enum AssetType
        {
            Rock_Boulder,
            Rock_Medium,
            Rock_Small,
            Tree_Full,
            Tree_Trunk,
            Tree_Foliage,
            Barrel_Wood,
            Barrel_Metal,
            Crate_Wood,
            Character_Stylized,
            Post_Wood,
            SignPost,
            Custom
        }

        public AssetType assetType = AssetType.Rock_Boulder;
        
        [Header("Custom Mesh (if AssetType is Custom)")]
        public Mesh customMesh;

        [Header("Material Override")]
        public bool overrideMaterial = false;
        public Material customMaterial;

        [Header("Randomization")]
        public bool randomizeScale = true;
        public Vector2 scaleRange = new Vector2(0.8f, 1.2f);

        [Header("Variation")]
        public bool randomizeRotation = false;
        public bool randomizeVariant = true; // Pick random rock/tree variant

        private void Awake()
        {
            ApplyHighQualityAsset();
        }

        public void ApplyHighQualityAsset()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

            if (meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();

            if (meshRenderer == null)
                meshRenderer = gameObject.AddComponent<MeshRenderer>();

            string meshName = "";
            string materialName = "";

            // Select appropriate mesh and material based on type
            switch (assetType)
            {
                case AssetType.Rock_Boulder:
                    meshName = randomizeVariant ? GetRandomVariant("SM_Rock_Boulder", "SM_Rock_Medium") : "SM_Rock_Boulder_01";
                    materialName = "M_StoneGray";
                    break;
                case AssetType.Rock_Medium:
                    meshName = randomizeVariant ? GetRandomVariant("SM_Rock_Medium_01", "SM_Rock_Medium_02") : "SM_Rock_Medium_01";
                    materialName = "M_StoneGray";
                    break;
                case AssetType.Rock_Small:
                    meshName = randomizeVariant ? GetRandomVariant("SM_Rock_Small_01", "SM_Rock_Small_02", "SM_Rock_Flat_01") : "SM_Rock_Small_01";
                    materialName = "M_StoneGray";
                    break;

                case AssetType.Tree_Full:
                    CreateFullTree(meshFilter, meshRenderer);
                    return;
                case AssetType.Tree_Trunk:
                    meshName = "SM_Tree_Trunk_Medium";
                    materialName = "M_WoodBark";
                    break;
                case AssetType.Tree_Foliage:
                    meshName = randomizeVariant ? GetRandomVariant("SM_Tree_Foliage_Round", "SM_Tree_Foliage_Irregular") : "SM_Tree_Foliage_Round";
                    materialName = "M_ForestGreen";
                    break;

                case AssetType.Barrel_Wood:
                    meshName = "SM_Barrel_Wood_01";
                    materialName = "M_WoodBark";
                    break;
                case AssetType.Barrel_Metal:
                    meshName = "SM_Barrel_Metal_01";
                    materialName = "M_MetalRusted";
                    break;
                case AssetType.Crate_Wood:
                    meshName = randomizeVariant ? GetRandomVariant("SM_Crate_Wood_01", "SM_Crate_Wood_02") : "SM_Crate_Wood_01";
                    materialName = "M_WoodBark";
                    break;

                case AssetType.Character_Stylized:
                    CreateCharacter(meshFilter, meshRenderer);
                    return;

                case AssetType.Post_Wood:
                    meshName = "SM_Post_Wood_01";
                    materialName = "M_WoodBark";
                    break;
                case AssetType.SignPost:
                    meshName = "SM_SignPost_01";
                    materialName = "M_WoodBark";
                    break;

                case AssetType.Custom:
                    if (customMesh != null)
                        meshFilter.mesh = customMesh;
                    if (overrideMaterial && customMaterial != null)
                        meshRenderer.material = customMaterial;
                    ApplyRandomization();
                    return;
            }

            // Load and assign mesh
            Mesh mesh = GetGeneratedMesh(meshName);
            if (mesh != null)
                meshFilter.sharedMesh = mesh;
            else
                Debug.LogWarning($"[AssetReplacer] Could not find mesh: {meshName}");

            // Load and assign material
            if (overrideMaterial && customMaterial != null)
                meshRenderer.material = customMaterial;
            else if (!overrideMaterial)
                meshRenderer.material = GetGeneratedMaterial(materialName);

            ApplyRandomization();
        }

        private void CreateFullTree(MeshFilter rootFilter, MeshRenderer rootRenderer)
        {
            // Trunk
            Mesh trunkMesh = GetGeneratedMesh("SM_Tree_Trunk_Medium");
            if (trunkMesh != null)
            {
                rootFilter.sharedMesh = trunkMesh;
                rootRenderer.material = GetGeneratedMaterial("M_WoodBark");
            }

            // Foliage as child
            GameObject foliageObj = new GameObject("Foliage");
            foliageObj.transform.SetParent(transform);
            foliageObj.transform.localPosition = new Vector3(0, 2.5f, 0);
            
            MeshFilter foliageFilter = foliageObj.AddComponent<MeshFilter>();
            MeshRenderer foliageRenderer = foliageObj.AddComponent<MeshRenderer>();
            
            string foliageMesh = randomizeVariant ? GetRandomVariant("SM_Tree_Foliage_Round", "SM_Tree_Foliage_Irregular") : "SM_Tree_Foliage_Round";
            Mesh leavesMesh = GetGeneratedMesh(foliageMesh);
            if (leavesMesh != null)
            {
                foliageFilter.sharedMesh = leavesMesh;
                foliageRenderer.material = GetGeneratedMaterial("M_ForestGreen");
                
                if (randomizeScale)
                {
                    float randomScale = Random.Range(scaleRange.x, scaleRange.y);
                    foliageObj.transform.localScale = Vector3.one * randomScale;
                }
            }
        }

        private void CreateCharacter(MeshFilter bodyFilter, MeshRenderer bodyRenderer)
        {
            // Body
            Mesh bodyMesh = GetGeneratedMesh("SM_Character_Torso");
            if (bodyMesh != null)
            {
                bodyFilter.sharedMesh = bodyMesh;
                bodyRenderer.material = GetGeneratedMaterial("M_PlasticWhite");
            }

            // Head as child
            GameObject headObj = new GameObject("Head");
            headObj.transform.SetParent(transform);
            headObj.transform.localPosition = new Vector3(0, 0.9f, 0);
            
            MeshFilter headFilter = headObj.AddComponent<MeshFilter>();
            MeshRenderer headRenderer = headObj.AddComponent<MeshRenderer>();
            
            Mesh headMesh = GetGeneratedMesh("SM_Character_Head");
            if (headMesh != null)
            {
                headFilter.sharedMesh = headMesh;
                headRenderer.material = GetGeneratedMaterial("M_PlasticWhite");
            }
        }

        private void ApplyRandomization()
        {
            if (randomizeScale)
            {
                float randomScale = Random.Range(scaleRange.x, scaleRange.y);
                transform.localScale *= randomScale;
            }

            if (randomizeRotation)
            {
                transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            }
        }

        private string GetRandomVariant(params string[] variants)
        {
            return variants[Random.Range(0, variants.Length)];
        }

        private Mesh GetGeneratedMesh(string meshName)
        {
            string path = $"Assets/GeneratedAssets/Meshes/{meshName}.asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            
            if (mesh == null)
            {
                // Runtime fallback
                mesh = FindMeshByName(meshName);
            }
            
            return mesh;
        }

        private Material GetGeneratedMaterial(string materialName)
        {
            string path = $"Assets/GeneratedAssets/Materials/{materialName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            
            if (mat == null)
            {
                // Runtime fallback
                mat = FindMaterialByName(materialName);
            }
            
            return mat;
        }

        private Mesh FindMeshByName(string name)
        {
            Mesh[] meshes = Resources.FindObjectsOfTypeAll<Mesh>();
            foreach (var m in meshes)
            {
                if (m.name == name) return m;
            }
            return null;
        }

        private Material FindMaterialByName(string name)
        {
            Material[] materials = Resources.FindObjectsOfTypeAll<Material>();
            foreach (var m in materials)
            {
                if (m.name == name) return m;
            }
            return null;
        }
    }
}
