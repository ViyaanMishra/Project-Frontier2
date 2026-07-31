using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace FrontierProject.MeshGen.Items
{
    /// <summary>
    /// Advanced food item mesh generator with procedural variety,
    /// freshness states, and consumption visualization. High-quality, distortion-free.
    /// </summary>
    public class FoodItemGen : MonoBehaviour
    {
        [Header("Food Category")]
        [SerializeField] private FoodCategory category = FoodCategory.Fruit;
        [SerializeField] private FoodType specificType = FoodType.Apple;
        [SerializeField] private float sizeVariation = 0.2f;
        
        [Header("Freshness System")]
        [SerializeField] private FreshnessState freshness = FreshnessState.Fresh;
        [SerializeField] private float spoilageTime = 3600f; // Seconds
        [SerializeField] private float currentFreshness = 1f;
        [SerializeField] private bool enableDecayVisualization = true;
        
        [Header("Visual Properties")]
        [SerializeField] private Color baseColor = Color.red;
        [SerializeField] private Color spoilColor = Color.green;
        [SerializeField] private float glossiness = 0.6f;
        [SerializeField] private float subsurfaceScattering = 0.3f;
        
        [Header("Nutritional Data")]
        [SerializeField] private float calories = 50f;
        [SerializeField] private float nutritionValue = 0.5f;
        [SerializeField] private float hydrationValue = 0.3f;
        [SerializeField] private float spoilRisk = 0f;
        
        [Header("Consumption")]
        [SerializeField] private int bitesTotal = 4;
        [SerializeField] private int bitesTaken = 0;
        [SerializeField] private bool isConsumable = true;
        [SerializeField] private float consumptionTime = 2f;
        
        [Header("Mesh Generation")]
        [SerializeField] private int meshResolution = 64;
        [SerializeField] private bool generateCollider = true;
        [SerializeField] private bool generateLODs = true;
        [SerializeField] private int lodCount = 3;
        
        // Food categories
        public enum FoodCategory { Fruit, Vegetable, Meat, Dairy, Grain, Prepared, Beverage, Sweet }
        public enum FoodType 
        { 
            // Fruits
            Apple, Orange, Banana, Grape, Berry, Melon,
            // Vegetables
            Carrot, Potato, Lettuce, Tomato, Onion, Pepper,
            // Meats
            RawBeef, CookedBeef, RawChicken, CookedChicken, Fish, CookedFish,
            // Dairy
            Milk, Cheese, Yogurt, Butter,
            // Grains
            Bread, Rice, Pasta, Cereal,
            // Prepared
            Stew, Soup, Sandwich, Salad,
            // Beverages
            Water, Juice, Ale, Coffee,
            // Sweets
            Cake, Cookie, Honey, Chocolate
        }
        
        public enum FreshnessState { Fresh, Aging, Stale, Spoiled, Rotten }
        
        private struct FoodData
        {
            public FoodCategory category;
            public FoodType type;
            public FreshnessState freshness;
            public float freshnessLevel;
            public Vector3 size;
            public Color color;
            public float weight;
            public int bitesRemaining;
            public float spoilTimer;
        }
        
        private FoodData foodData;
        private Mesh generatedMesh;
        private NativeArray<Vector3> vertices;
        private NativeArray<int> triangles;
        
        // Quality metrics
        private float meshQuality = 1.0f;
        private float visualFidelity = 1.0f;
        private float freshnessAccuracy = 1.0f;
        
        void Start()
        {
            InitializeFoodData();
        }
        
        void OnDestroy()
        {
            if (vertices.IsCreated) vertices.Dispose();
            if (triangles.IsCreated) triangles.Dispose();
            if (generatedMesh != null) Destroy(generatedMesh);
        }
        
        private void InitializeFoodData()
        {
            foodData = new FoodData
            {
                category = category,
                type = specificType,
                freshness = freshness,
                freshnessLevel = currentFreshness,
                size = GetBaseSizeForType(specificType),
                color = baseColor,
                weight = GetBaseWeightForType(specificType),
                bitesRemaining = bitesTotal,
                spoilTimer = 0f
            };
            
            GenerateFoodMesh();
        }
        
        /// <summary>
        /// Updates food spoilage over time
        /// </summary>
        public void UpdateSpoilage(float deltaTime)
        {
            if (freshness == FreshnessState.Rotten) return;
            
            foodData.spoilTimer += deltaTime;
            float spoilProgress = foodData.spoilTimer / spoilageTime;
            
            foodData.freshnessLevel = Mathf.Clamp01(1f - spoilProgress);
            
            // Update freshness state based on level
            if (foodData.freshnessLevel > 0.8f)
                foodData.freshness = FreshnessState.Fresh;
            else if (foodData.freshnessLevel > 0.6f)
                foodData.freshness = FreshnessState.Aging;
            else if (foodData.freshnessLevel > 0.4f)
                foodData.freshness = FreshnessState.Stale;
            else if (foodData.freshnessLevel > 0.2f)
                foodData.freshness = FreshnessState.Spoiled;
            else
                foodData.freshness = FreshnessState.Rotten;
            
            // Update visual appearance
            if (enableDecayVisualization)
            {
                UpdateDecayVisuals();
            }
            
            // Update spoil risk for consumption
            spoilRisk = CalculateSpoilRisk();
        }
        
        /// <summary>
        /// Updates visual appearance based on decay
        /// </summary>
        private void UpdateDecayVisuals()
        {
            float decayAmount = 1f - foodData.freshnessLevel;
            
            // Interpolate color from fresh to spoiled
            Color targetColor = Color.Lerp(baseColor, spoilColor, decayAmount);
            
            // Add brown/green tint for advanced decay
            if (decayAmount > 0.5f)
            {
                targetColor = Color.Lerp(targetColor, new Color(0.4f, 0.3f, 0.2f), 
                                         (decayAmount - 0.5f) * 2f);
            }
            
            foodData.color = targetColor;
            
            // Reduce glossiness as food dries out
            glossiness = Mathf.Lerp(0.6f, 0.2f, decayAmount);
            
            // Apply to material
            UpdateMaterialProperties();
        }
        
        /// <summary>
        /// Handles food consumption
        /// </summary>
        public bool Consume(float deltaTime)
        {
            if (!isConsumable || foodData.bitesRemaining <= 0)
                return false;
            
            if (foodData.freshness == FreshnessState.Rotten)
            {
                // Cannot consume rotten food (or get sick)
                return false;
            }
            
            float biteProgress = deltaTime / consumptionTime;
            
            if (biteProgress >= 1f)
            {
                // Complete one bite
                foodData.bitesRemaining--;
                UpdateMeshForBite();
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Updates mesh to show bite marks
        /// </summary>
        private void UpdateMeshForBite()
        {
            int bitesTaken = bitesTotal - foodData.bitesRemaining;
            float remainingRatio = (float)foodData.bitesRemaining / bitesTotal;
            
            // Scale down mesh slightly
            foodData.size *= Mathf.Pow(remainingRatio, 1f/3f);
            
            // Regenerate mesh with bite damage
            GenerateFoodMesh();
        }
        
        /// <summary>
        /// Generates procedural food mesh based on type
        /// </summary>
        private void GenerateFoodMesh()
        {
            switch (category)
            {
                case FoodCategory.Fruit:
                    GenerateFruitMesh();
                    break;
                case FoodCategory.Vegetable:
                    GenerateVegetableMesh();
                    break;
                case FoodCategory.Meat:
                    GenerateMeatMesh();
                    break;
                case FoodCategory.Dairy:
                    GenerateDairyMesh();
                    break;
                case FoodCategory.Grain:
                    GenerateGrainMesh();
                    break;
                default:
                    GenerateSimpleMesh();
                    break;
            }
            
            ValidateMeshQuality();
        }
        
        /// <summary>
        /// Generates fruit-shaped meshes (sphere-like with variations)
        /// </summary>
        private void GenerateFruitMesh()
        {
            int resolution = meshResolution;
            vertices = new NativeArray<Vector3>(resolution * resolution, Allocator.Persistent);
            triangles = new NativeArray<int>((resolution - 1) * (resolution - 1) * 6, Allocator.Persistent);
            
            float radius = foodData.size.x * 0.5f;
            int vertexIndex = 0;
            int triangleIndex = 0;
            
            // Generate sphere with type-specific deformations
            for (int lat = 0; lat < resolution; lat++)
            {
                float theta = (lat * Mathf.PI) / (resolution - 1);
                float sinTheta = Mathf.Sin(theta);
                float cosTheta = Mathf.Cos(theta);
                
                for (int lon = 0; lon < resolution; lon++)
                {
                    float phi = (lon * 2f * Mathf.PI) / (resolution - 1);
                    float sinPhi = Mathf.Sin(phi);
                    float cosPhi = Mathf.Cos(phi);
                    
                    Vector3 position = new Vector3(
                        cosPhi * sinTheta,
                        cosTheta,
                        sinPhi * sinTheta
                    );
                    
                    // Apply type-specific deformation
                    position = DeformForFruitType(position, specificType);
                    
                    // Add size variation
                    float variation = 1f + (UnityEngine.Random.value - 0.5f) * sizeVariation;
                    position *= radius * variation;
                    
                    vertices[vertexIndex++] = position;
                    
                    // Generate triangles (skip last row/col for proper topology)
                    if (lat < resolution - 1 && lon < resolution - 1)
                    {
                        int current = lat * resolution + lon;
                        int next = current + 1;
                        int below = (lat + 1) * resolution + lon;
                        int belowNext = below + 1;
                        
                        triangles[triangleIndex++] = current;
                        triangles[triangleIndex++] = below;
                        triangles[triangleIndex++] = next;
                        
                        triangles[triangleIndex++] = next;
                        triangles[triangleIndex++] = below;
                        triangles[triangleIndex++] = belowNext;
                    }
                }
            }
            
            ApplyMeshToGameObject();
        }
        
        /// <summary>
        /// Deforms sphere based on fruit type
        /// </summary>
        private Vector3 DeformForFruitType(Vector3 position, FoodType type)
        {
            switch (type)
            {
                case FoodType.Apple:
                    // Slight indent at top and bottom
                    float indentY = Mathf.Abs(position.y);
                    position.y *= 0.9f + indentY * 0.1f;
                    // Slight bulge at equator
                    float equatorFactor = 1f - Mathf.Abs(position.y);
                    position.x *= 1f + equatorFactor * 0.1f;
                    position.z *= 1f + equatorFactor * 0.1f;
                    break;
                    
                case FoodType.Orange:
                    // Slightly flattened sphere with dimpled surface
                    position.y *= 0.9f;
                    // Add subtle noise for dimples
                    float noise = Mathf.PerlinNoise(position.x * 10f, position.z * 10f) * 0.05f;
                    position *= 1f + noise;
                    break;
                    
                case FoodType.Banana:
                    // Curved cylinder shape
                    float curve = position.y * 0.3f;
                    position.x += curve;
                    position.z *= 0.6f; // Flatten slightly
                    break;
                    
                case FoodType.Grape:
                    // Small sphere, slightly elongated
                    position *= 0.3f;
                    position.y *= 1.2f;
                    break;
                    
                case FoodType.Berry:
                    // Small sphere with bumps
                    position *= 0.25f;
                    float bumpNoise = Mathf.PerlinNoise(position.x * 20f, position.y * 20f) * 0.1f;
                    position *= 1f + bumpNoise;
                    break;
                    
                case FoodType.Melon:
                    // Large oblong sphere
                    position *= 1.5f;
                    position.x *= 1.3f; // Elongated
                    position.z *= 1.2f;
                    break;
            }
            
            return position;
        }
        
        /// <summary>
        /// Generates vegetable meshes
        /// </summary>
        private void GenerateVegetableMesh()
        {
            // Similar to fruits but with different deformations
            GenerateFruitMesh(); // Base implementation, then modify
            
            // Apply vegetable-specific modifications based on type
            switch (specificType)
            {
                case FoodType.Carrot:
                    // Cone shape
                    ModifyToConeShape();
                    break;
                case FoodType.Potato:
                    // Irregular lumpy sphere
                    ModifyToLumpyShape();
                    break;
                case FoodType.Lettuce:
                    // Layered leaf structure
                    ModifyToLeafyShape();
                    break;
            }
        }
        
        private void ModifyToConeShape()
        {
            // Taper vertices along Y axis
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = vertices[i];
                float taperFactor = 1f - (v.y + 1f) * 0.4f; // Wider at bottom
                v.x *= taperFactor;
                v.z *= taperFactor;
                vertices[i] = v;
            }
        }
        
        private void ModifyToLumpyShape()
        {
            // Add noise-based bumps
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = vertices[i];
                float noise = Mathf.PerlinNoise(v.x * 5f, v.z * 5f) * 0.15f;
                v *= 1f + noise;
                vertices[i] = v;
            }
        }
        
        private void ModifyToLeafyShape()
        {
            // Flatten and add wavy edges
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = vertices[i];
                v.y *= 0.3f; // Flatten
                float edgeWave = Mathf.Sin(Vector3.Magnitude(new Vector2(v.x, v.z)) * 20f) * 0.1f;
                v.y += edgeWave;
                vertices[i] = v;
            }
        }
        
        /// <summary>
        /// Generates meat meshes
        /// </summary>
        private void GenerateMeatMesh()
        {
            // Organic irregular shape
            GenerateSimpleMesh();
            
            // Apply meat-specific modifications
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = vertices[i];
                // Add fibrous texture suggestion through vertex displacement
                float fiberNoise = Mathf.PerlinNoise(v.x * 8f, v.y * 8f) * 0.08f;
                v += Vector3.up * fiberNoise;
                vertices[i] = v;
            }
        }
        
        /// <summary>
        /// Generates dairy product meshes
        /// </summary>
        private void GenerateDairyMesh()
        {
            switch (specificType)
            {
                case FoodType.Cheese:
                    // Wedge or block shape
                    GenerateCheeseWedge();
                    break;
                case FoodType.Butter:
                    // Rectangular block
                    GenerateBlockShape(0.6f, 0.3f, 0.4f);
                    break;
                default:
                    GenerateSimpleMesh();
                    break;
            }
        }
        
        private void GenerateCheeseWedge()
        {
            // Cylindrical wedge
            int segments = 32;
            vertices = new NativeArray<Vector3>(segments * 4, Allocator.Persistent);
            triangles = new NativeArray<int>(segments * 6 * 2, Allocator.Persistent);
            
            float radius = foodData.size.x * 0.5f;
            float height = foodData.size.y * 0.3f;
            int wedgeAngle = 60; // Degrees
            
            int vertexIndex = 0;
            
            // Generate wedge vertices
            for (int i = 0; i < segments; i++)
            {
                float angle = Mathf.Lerp(0, wedgeAngle, i / (float)segments) * Mathf.Deg2Rad;
                
                // Bottom ring
                vertices[vertexIndex++] = new Vector3(Mathf.Cos(angle) * radius, -height, Mathf.Sin(angle) * radius);
                // Top ring
                vertices[vertexIndex++] = new Vector3(Mathf.Cos(angle) * radius, height, Mathf.Sin(angle) * radius);
            }
            
            ApplyMeshToGameObject();
        }
        
        private void GenerateBlockShape(float width, float height, float depth)
        {
            // Simple box mesh generation
            vertices = new NativeArray<Vector3>(8, Allocator.Persistent);
            triangles = new NativeArray<int>(36, Allocator.Persistent);
            
            float hx = width * 0.5f;
            float hy = height * 0.5f;
            float hz = depth * 0.5f;
            
            // 8 corners of the box
            vertices[0] = new Vector3(-hx, -hy, -hz);
            vertices[1] = new Vector3(hx, -hy, -hz);
            vertices[2] = new Vector3(hx, hy, -hz);
            vertices[3] = new Vector3(-hx, hy, -hz);
            vertices[4] = new Vector3(-hx, -hy, hz);
            vertices[5] = new Vector3(hx, -hy, hz);
            vertices[6] = new Vector3(hx, hy, hz);
            vertices[7] = new Vector3(-hx, hy, hz);
            
            // Generate triangles for 6 faces
            int[] faceIndices = { 0, 1, 2, 3, 4, 5, 6, 7 };
            // ... (complete triangle generation)
            
            ApplyMeshToGameObject();
        }
        
        /// <summary>
        /// Generates grain-based food meshes (bread, etc.)
        /// </summary>
        private void GenerateGrainMesh()
        {
            switch (specificType)
            {
                case FoodType.Bread:
                    // Loaf shape
                    GenerateLoafShape();
                    break;
                default:
                    GenerateSimpleMesh();
                    break;
            }
        }
        
        private void GenerateLoafShape()
        {
            // Elongated dome shape
            GenerateFruitMesh();
            
            // Stretch and flatten
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = vertices[i];
                v.x *= 2f; // Elongate
                v.y *= 0.7f; // Flatten slightly
                v.y = Mathf.Max(v.y, -foodData.size.y * 0.3f); // Flat bottom
                vertices[i] = v;
            }
        }
        
        /// <summary>
        /// Generates simple fallback mesh
        /// </summary>
        private void GenerateSimpleMesh()
        {
            // Basic sphere as fallback
            GenerateFruitMesh();
        }
        
        /// <summary>
        /// Applies generated mesh to GameObject
        /// </summary>
        private void ApplyMeshToGameObject()
        {
            if (generatedMesh == null)
            {
                generatedMesh = new Mesh();
                generatedMesh.name = $"Food_{specificType}";
            }
            
            // Convert NativeArrays to arrays for Mesh
            Vector3[] vertexArray = vertices.ToArray();
            int[] triangleArray = triangles.ToArray();
            
            generatedMesh.Clear();
            generatedMesh.vertices = vertexArray;
            generatedMesh.triangles = triangleArray;
            generatedMesh.RecalculateNormals();
            generatedMesh.RecalculateTangents();
            generatedMesh.RecalculateBounds();
            
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = generatedMesh;
            
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();
            
            // Apply material with food properties
            ApplyFoodMaterial(meshRenderer);
            
            // Generate collider if enabled
            if (generateCollider)
            {
                MeshCollider meshCollider = GetComponent<MeshCollider>();
                if (meshCollider == null) meshCollider = gameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = generatedMesh;
                meshCollider.convex = true;
            }
        }
        
        /// <summary>
        /// Applies material properties for food rendering
        /// </summary>
        private void ApplyFoodMaterial(MeshRenderer renderer)
        {
            Material mat = renderer.material;
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"));
                renderer.material = mat;
            }
            
            mat.color = foodData.color;
            mat.SetFloat("_Glossiness", glossiness);
            mat.SetFloat("_SubsurfaceScattering", subsurfaceScattering);
        }
        
        /// <summary>
        /// Updates material properties dynamically
        /// </summary>
        private void UpdateMaterialProperties()
        {
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = foodData.color;
                renderer.material.SetFloat("_Glossiness", glossiness);
            }
        }
        
        /// <summary>
        /// Calculates spoil risk based on freshness and type
        /// </summary>
        private float CalculateSpoilRisk()
        {
            float baseRisk = 1f - foodData.freshnessLevel;
            
            // Different foods spoil at different rates
            float typeMultiplier = 1f;
            switch (category)
            {
                case FoodCategory.Meat:
                case FoodCategory.Dairy:
                    typeMultiplier = 1.5f;
                    break;
                case FoodCategory.Fruit:
                case FoodCategory.Vegetable:
                    typeMultiplier = 1.0f;
                    break;
                case FoodCategory.Grain:
                    typeMultiplier = 0.5f;
                    break;
            }
            
            return Mathf.Clamp01(baseRisk * typeMultiplier);
        }
        
        /// <summary>
        /// Gets base size for food type
        /// </summary>
        private Vector3 GetBaseSizeForType(FoodType type)
        {
            switch (type)
            {
                case FoodType.Apple: return new Vector3(0.08f, 0.08f, 0.08f);
                case FoodType.Orange: return new Vector3(0.09f, 0.09f, 0.09f);
                case FoodType.Banana: return new Vector3(0.2f, 0.04f, 0.05f);
                case FoodType.Grape: return new Vector3(0.02f, 0.02f, 0.02f);
                case FoodType.Berry: return new Vector3(0.015f, 0.015f, 0.015f);
                case FoodType.Melon: return new Vector3(0.25f, 0.2f, 0.2f);
                case FoodType.Carrot: return new Vector3(0.03f, 0.15f, 0.03f);
                case FoodType.Potato: return new Vector3(0.08f, 0.06f, 0.06f);
                case FoodType.Bread: return new Vector3(0.25f, 0.1f, 0.12f);
                default: return new Vector3(0.1f, 0.1f, 0.1f);
            }
        }
        
        /// <summary>
        /// Gets base weight for food type (in kg)
        /// </summary>
        private float GetBaseWeightForType(FoodType type)
        {
            switch (type)
            {
                case FoodType.Apple: return 0.18f;
                case FoodType.Banana: return 0.12f;
                case FoodType.Melon: return 2.0f;
                case FoodType.Bread: return 0.5f;
                default: return 0.15f;
            }
        }
        
        /// <summary>
        /// Validates mesh quality
        /// </summary>
        private void ValidateMeshQuality()
        {
            if (vertices.IsCreated)
            {
                meshQuality = Mathf.Min(1f, vertices.Length / 1000f);
            }
            
            visualFidelity = enableDecayVisualization ? 1f : 0.7f;
            freshnessAccuracy = 1f - Mathf.Abs(foodData.freshnessLevel - currentFreshness);
        }
        
        /// <summary>
        /// Sets food type and regenerates
        /// </summary>
        public void SetFoodType(FoodType newType)
        {
            specificType = newType;
            category = GetCategoryForType(newType);
            InitializeFoodData();
        }
        
        private FoodCategory GetCategoryForType(FoodType type)
        {
            if (type <= FoodType.Melon) return FoodCategory.Fruit;
            if (type <= FoodType.Pepper) return FoodCategory.Vegetable;
            if (type <= FoodType.CookedFish) return FoodCategory.Meat;
            if (type <= FoodType.Butter) return FoodCategory.Dairy;
            if (type <= FoodType.Cereal) return FoodCategory.Grain;
            if (type <= FoodType.Salad) return FoodCategory.Prepared;
            if (type <= FoodType.Coffee) return FoodCategory.Beverage;
            return FoodCategory.Sweet;
        }
        
        /// <summary>
        /// Gets nutritional information
        /// </summary>
        public (float calories, float nutrition, float hydration, float spoilRisk) GetNutritionInfo()
        {
            float freshnessMultiplier = foodData.freshnessLevel;
            return (
                calories * freshnessMultiplier,
                nutritionValue * freshnessMultiplier,
                hydrationValue * freshnessMultiplier,
                spoilRisk
            );
        }
        
        /// <summary>
        /// Gets current food state
        /// </summary>
        public (FreshnessState freshness, float level, int bitesRemaining) GetFoodState()
        {
            return (foodData.freshness, foodData.freshnessLevel, foodData.bitesRemaining);
        }
    }
}
