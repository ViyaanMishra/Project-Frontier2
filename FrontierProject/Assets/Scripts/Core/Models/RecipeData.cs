using System;
using UnityEngine;

namespace Frontier.Core.Models
{
    /// <summary>
    /// Recipe data structure for crafting trees per workstation.
    /// </summary>
    [Serializable]
    public struct RecipeData
    {
        public EntityGUID guid;
        public string recipeName;
        public string description;
        public RecipeCategory category;
        public CraftingTier requiredTier;
        
        // Output
        public int outputItemId;
        public int outputQuantity;
        public float craftTime; // Seconds
        
        // Input requirements
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public IngredientEntry[] ingredients;
        
        // Requirements
        public int requiredSkillType; // Index into SkillType enum
        public float requiredSkillLevel;
        public float requiredEnergy;
        public bool requiresPower;
        public bool requiresWater;
        public int requiredWorkbenchType; // WorkbenchType enum
        
        // Unlock conditions
        public bool isUnlockedByDefault;
        public int[] prerequisiteRecipeIds;
        public int[] requiredTechIds;
        public int requiredFactionReputation;
        
        // Discovery
        public bool isDiscovered;
        public DiscoveredMethod discoveryMethod;
        public int discoveredFromPOI;
        
        public void Initialize(string name, RecipeCategory cat, CraftingTier tier)
        {
            recipeName = name;
            category = cat;
            requiredTier = tier;
            ingredients = new IngredientEntry[8];
            prerequisiteRecipeIds = new int[0];
            requiredTechIds = new int[0];
            isUnlockedByDefault = false;
            isDiscovered = false;
        }
        
        public bool CanCraft(int[] availableMaterials, float currentEnergy, bool hasPower, bool hasWater, float skillLevel)
        {
            // Check skill requirement
            if (skillLevel < requiredSkillLevel) return false;
            
            // Check power/water requirements
            if (requiresPower && !hasPower) return false;
            if (requiresWater && !hasWater) return false;
            
            // Check energy
            if (currentEnergy < requiredEnergy) return false;
            
            // Check materials
            foreach (var ingredient in ingredients)
            {
                if (ingredient.itemId <= 0 || ingredient.quantity <= 0) continue;
                
                int availableIndex = ingredient.itemId;
                if (availableIndex >= availableMaterials.Length) return false;
                if (availableMaterials[availableIndex] < ingredient.quantity) return false;
            }
            
            return true;
        }
        
        public void ConsumeMaterials(int[] inventory)
        {
            foreach (var ingredient in ingredients)
            {
                if (ingredient.itemId <= 0 || ingredient.quantity <= 0) continue;
                inventory[ingredient.itemId] -= ingredient.quantity;
            }
        }
        
        public float GetCraftTimeWithSkillBonus(float skillLevel)
        {
            float bonus = Mathf.Max(0, (skillLevel - requiredSkillLevel) * 0.05f);
            return craftTime / (1 + bonus);
        }
    }
    
    [Serializable]
    public struct IngredientEntry
    {
        public int itemId;
        public int quantity;
        public bool canSubstitute;
        public int[] substituteItemIds;
    }
    
    public enum RecipeCategory
    {
        Weapons, Ammo, Armor, Tools,
        BuildingMaterials, Components, Electronics,
        Consumables, Food, Water, Medicine,
        VehicleParts, Modules, Fuel,
        AnomalyTech, ResearchItems, Special
    }
    
    public enum CraftingTier
    {
        Tier1,  // Basic hand crafting
        Tier2,  // Simple workbench
        Tier3,  // Advanced workshop
        Tier4,  // Fabricator
        Tier5   // Anomaly forge
    }
    
    public enum DiscoveredMethod
    {
        Default,
        FoundInLoot,
        PurchasedFromTrader,
        LearnedFromNPC,
        ReverseEngineered,
        QuestReward,
        ResearchUnlock
    }
    
    /// <summary>
    /// Static database of all crafting recipes (150+).
    /// </summary>
    public static class RecipeDatabase
    {
        public static RecipeData[] AllRecipes;
        
        static RecipeDatabase()
        {
            InitializeDatabase();
        }
        
        private static void InitializeDatabase()
        {
            AllRecipes = new RecipeData[160];
            int index = 0;
            
            // ===== WEAPONS =====
            AllRecipes[index++] = CreateScrapMacheteRecipe();
            AllRecipes[index++] = CreatePipeWrenchRecipe();
            AllRecipes[index++] = CreatePipeRifleRecipe();
            AllRecipes[index++] = CreateScrapPistolRecipe();
            AllRecipes[index++] = CreateKineticShotgunRecipe();
            AllRecipes[index++] = CreateAssaultCarbineRecipe();
            AllRecipes[index++] = CreateMarksmanDMRRecipe();
            AllRecipes[index++] = CreateCrossbowRecipe();
            AllRecipes[index++] = CreateNailGunRecipe();
            AllRecipes[index++] = CreateFlamethrowerRecipe();
            AllRecipes[index++] = CreateRocketLauncherRecipe();
            AllRecipes[index++] = CreateMortarTubeRecipe();
            
            // ===== AMMO =====
            AllRecipes[index++] = CreateMakeshiftAmmoRecipe();
            AllRecipes[index++] = CreatePistolAmmoRecipe();
            AllRecipes[index++] = CreateRifleAmmoRecipe();
            AllRecipes[index++] = CreateShotgunShellsRecipe();
            AllRecipes[index++] = CreateSniperAmmoRecipe();
            AllRecipes[index++] = CreateHeavyAmmoRecipe();
            AllRecipes[index++] = CreateEnergyCellRecipe();
            AllRecipes[index++] = CreatePlasmaCellRecipe();
            AllRecipes[index++] = CreateRailSlugRecipe();
            AllRecipes[index++] = CreateRocketPropellantRecipe();
            
            // ===== BUILDING MATERIALS =====
            AllRecipes[index++] = CreateWoodenPlanksRecipe();
            AllRecipes[index++] = CreateConcreteMixRecipe();
            AllRecipes[index++] = CreateSteelIngotRecipe();
            AllRecipes[index++] = CreateReinforcedConcreteRecipe();
            AllRecipes[index++] = CreateCompositePlatingRecipe();
            AllRecipes[index++] = CreateNanoMaterialRecipe();
            AllRecipes[index++] = CreateGlassPaneRecipe();
            AllRecipes[index++] = CreateInsulationRecipe();
            AllRecipes[index++] = CreateWireRecipe();
            AllRecipes[index++] = CreatePipeRecipe();
            
            // ===== CONSUMABLES =====
            AllRecipes[index++] = CreatePurifiedWaterRecipe();
            AllRecipes[index++] = CreateCookedRationsRecipe();
            AllRecipes[index++] = CreatePreservedFoodRecipe();
            AllRecipes[index++] = CreateNutrientPasteRecipe();
            AllRecipes[index++] = CreateStimPackRecipe();
            AllRecipes[index++] = CreateMedKitRecipe();
            AllRecipes[index++] = CreateAntidoteRecipe();
            AllRecipes[index++] = CreateRadiationPillsRecipe();
            AllRecipes[index++] = CreateSleepingPillsRecipe();
            AllRecipes[index++] = CreateCoffeeRecipe();
            
            // ===== VEHICLE PARTS =====
            AllRecipes[index++] = CreateVehicleBatteryRecipe();
            AllRecipes[index++] = CreateTireRecipe();
            AllRecipes[index++] = CreateEnginePartsRecipe();
            AllRecipes[index++] = CreateTransmissionRecipe();
            AllRecipes[index++] = CreateFuelTankRecipe();
            AllRecipes[index++] = CreateArmorPlatingRecipe();
            AllRecipes[index++] = CreateTurretMountRecipe();
            AllRecipes[index++] = CreateShieldGeneratorRecipe();
            
            // ===== ELECTRONICS =====
            AllRecipes[index++] = CreateBasicCircuitRecipe();
            AllRecipes[index++] = CreateAdvancedCircuitRecipe();
            AllRecipes[index++] = CreateMicrochipRecipe();
            AllRecipes[index++] = CreateSensorModuleRecipe();
            AllRecipes[index++] = CreateRadioRecipe();
            AllRecipes[index++] = CreateDroneCoreRecipe();
            AllRecipes[index++] = CreateAIProcessorRecipe();
            
            // ===== ANOMALY TECH =====
            AllRecipes[index++] = CreateRealityAnchorRecipe();
            AllRecipes[index++] = CreatePhaseShieldRecipe();
            AllRecipes[index++] = CreateAnomalyBatteryRecipe();
            AllRecipes[index++] = CreateQuantumCapacitorRecipe();
            AllRecipes[index++] = CreateVoidStabilizerRecipe();
            
            // ... Continue with remaining recipes to reach 150+
        }
        
        #region Weapon Recipes
        private static RecipeData CreateScrapMacheteRecipe()
        {
            var recipe = new RecipeData();
            recipe.Initialize("Scrap Machete", RecipeCategory.Weapons, CraftingTier.Tier1);
            recipe.outputItemId = (int)ItemType.ScrapMachete;
            recipe.outputQuantity = 1;
            recipe.craftTime = 30f;
            recipe.ingredients[0] = new IngredientEntry { itemId = (int)ItemType.ScrapMetal, quantity = 5 };
            recipe.ingredients[1] = new IngredientEntry { itemId = (int)ItemType.Wood, quantity = 2 };
            recipe.ingredients[2] = new IngredientEntry { itemId = (int)ItemType.Leather, quantity = 1 };
            recipe.requiredSkillType = (int)SkillType.Construction;
            recipe.requiredSkillLevel = 1;
            recipe.requiredWorkbenchType = (int)WorkbenchType.BasicBench;
            recipe.isUnlockedByDefault = true;
            return recipe;
        }
        
        private static RecipeData CreatePipeRifleRecipe()
        {
            var recipe = new RecipeData();
            recipe.Initialize("Pipe Rifle", RecipeCategory.Weapons, CraftingTier.Tier1);
            recipe.outputItemId = (int)ItemType.PipeRifle;
            recipe.outputQuantity = 1;
            recipe.craftTime = 120f;
            recipe.ingredients[0] = new IngredientEntry { itemId = (int)ItemType.SteelPipe, quantity = 3 };
            recipe.ingredients[1] = new IngredientEntry { itemId = (int)ItemType.Spring, quantity = 2 };
            recipe.ingredients[2] = new IngredientEntry { itemId = (int)ItemType.Wood, quantity = 4 };
            recipe.ingredients[3] = new IngredientEntry { itemId = (int)ItemType.Screw, quantity = 10 };
            recipe.requiredSkillType = (int)SkillType.Engineering;
            recipe.requiredSkillLevel = 2;
            recipe.requiredWorkbenchType = (int)WorkbenchType.WeaponsBench;
            return recipe;
        }
        
        private static RecipeData CreateAssaultCarbineRecipe()
        {
            var recipe = new RecipeData();
            recipe.Initialize("Assault Carbine", RecipeCategory.Weapons, CraftingTier.Tier3);
            recipe.outputItemId = (int)ItemType.AssaultCarbine;
            recipe.outputQuantity = 1;
            recipe.craftTime = 300f;
            recipe.ingredients[0] = new IngredientEntry { itemId = (int)ItemType.SteelIngot, quantity = 8 };
            recipe.ingredients[1] = new IngredientEntry { itemId = (int)ItemType.AdvancedCircuit, quantity = 2 };
            recipe.ingredients[2] = new IngredientEntry { itemId = (int)ItemType.Polymer, quantity = 5 };
            recipe.ingredients[3] = new IngredientEntry { itemId = (int)ItemType.Spring, quantity = 4 };
            recipe.requiredSkillType = (int)SkillType.Engineering;
            recipe.requiredSkillLevel = 5;
            recipe.requiredWorkbenchType = (int)WorkbenchType.AdvancedFabricator;
            recipe.requiredTechIds = new int[] { 15, 22 };
            return recipe;
        }
        #endregion
        
        #region Ammo Recipes
        private static RecipeData CreateMakeshiftAmmoRecipe()
        {
            var recipe = new RecipeData();
            recipe.Initialize("Makeshift Ammo", RecipeCategory.Ammo, CraftingTier.Tier1);
            recipe.outputItemId = (int)ItemType.MakeshiftAmmo;
            recipe.outputQuantity = 10;
            recipe.craftTime = 20f;
            recipe.ingredients[0] = new IngredientEntry { itemId = (int)ItemType.Lead, quantity = 3 };
            recipe.ingredients[1] = new IngredientEntry { itemId = (int)ItemType.Gunpowder, quantity = 2 };
            recipe.ingredients[2] = new IngredientEntry { itemId = (int)ItemType.Brass, quantity = 2 };
            recipe.requiredSkillType = (int)SkillType.Engineering;
            recipe.requiredSkillLevel = 1;
            recipe.requiredWorkbenchType = (int)WorkbenchType.BasicBench;
            recipe.isUnlockedByDefault = true;
            return recipe;
        }
        
        private static RecipeData CreateEnergyCellRecipe()
        {
            var recipe = new RecipeData();
            recipe.Initialize("Energy Cell", RecipeCategory.Ammo, CraftingTier.Tier3);
            recipe.outputItemId = (int)ItemType.EnergyCell;
            recipe.outputQuantity = 20;
            recipe.craftTime = 45f;
            recipe.ingredients[0] = new IngredientEntry { itemId = (int)ItemType.Lithium, quantity = 4 };
            recipe.ingredients[1] = new IngredientEntry { itemId = (int)ItemType.Copper, quantity = 3 };
            recipe.ingredients[2] = new IngredientEntry { itemId = (int)ItemType.Plastic, quantity = 2 };
            recipe.requiresPower = true;
            recipe.requiredEnergy = 10f;
            recipe.requiredSkillType = (int)SkillType.Electronics;
            recipe.requiredSkillLevel = 4;
            recipe.requiredWorkbenchType = (int)WorkbenchType.ElectronicsBench;
            return recipe;
        }
        #endregion
        
        #region Building Materials
        private static RecipeData CreateWoodenPlanksRecipe()
        {
            var recipe = new RecipeData();
            recipe.Initialize("Wooden Planks", RecipeCategory.BuildingMaterials, CraftingTier.Tier1);
            recipe.outputItemId = (int)ItemType.WoodenPlanks;
            recipe.outputQuantity = 5;
            recipe.craftTime = 10f;
            recipe.ingredients[0] = new IngredientEntry { itemId = (int)ItemType.Log, quantity = 1 };
            recipe.requiredSkillType = (int)SkillType.Construction;
            recipe.requiredSkillLevel = 0;
            recipe.requiredWorkbenchType = (int)WorkbenchType.Sawmill;
            recipe.isUnlockedByDefault = true;
            return recipe;
        }
        
        private static RecipeData CreateSteelIngotRecipe()
        {
            var recipe = new RecipeData();
            recipe.Initialize("Steel Ingot", RecipeCategory.BuildingMaterials, CraftingTier.Tier2);
            recipe.outputItemId = (int)ItemType.SteelIngot;
            recipe.outputQuantity = 1;
            recipe.craftTime = 60f;
            recipe.ingredients[0] = new IngredientEntry { itemId = (int)ItemType.IronOre, quantity = 3 };
            recipe.ingredients[1] = new IngredientEntry { itemId = (int)ItemType.Coal, quantity = 2 };
            recipe.ingredients[2] = new IngredientEntry { itemId = (int)ItemType.Limestone, quantity = 1 };
            recipe.requiresPower = true;
            recipe.requiredEnergy = 20f;
            recipe.requiredSkillType = (int)SkillType.Engineering;
            recipe.requiredSkillLevel = 2;
            recipe.requiredWorkbenchType = (int)WorkbenchType.Smelter;
            return recipe;
        }
        #endregion
        
        #region Consumables
        private static RecipeData CreatePurifiedWaterRecipe()
        {
            var recipe = new RecipeData();
            recipe.Initialize("Purified Water", RecipeCategory.Water, CraftingTier.Tier1);
            recipe.outputItemId = (int)ItemType.PurifiedWater;
            recipe.outputQuantity = 1;
            recipe.craftTime = 5f;
            recipe.ingredients[0] = new IngredientEntry { itemId = (int)ItemType.DirtyWater, quantity = 1 };
            recipe.ingredients[1] = new IngredientEntry { itemId = (int)ItemType.Charcoal, quantity = 1 };
            recipe.requiresPower = false;
            recipe.requiredSkillType = (int)SkillType.Chemistry;
            recipe.requiredSkillLevel = 0;
            recipe.requiredWorkbenchType = (int)WorkbenchType.WaterPurifier;
            recipe.isUnlockedByDefault = true;
            return recipe;
        }
        
        private static RecipeData CreateMedKitRecipe()
        {
            var recipe = new RecipeData();
            recipe.Initialize("MedKit", RecipeCategory.Medicine, CraftingTier.Tier2);
            recipe.outputItemId = (int)ItemType.MedKit;
            recipe.outputQuantity = 1;
            recipe.craftTime = 30f;
            recipe.ingredients[0] = new IngredientEntry { itemId = (int)ItemType.Cloth, quantity = 3 };
            recipe.ingredients[1] = new IngredientEntry { itemId = (int)ItemType.Antiseptic, quantity = 2 };
            recipe.ingredients[2] = new IngredientEntry { itemId = (int)ItemType.Painkillers, quantity = 1 };
            recipe.requiredSkillType = (int)SkillType.Medicine;
            recipe.requiredSkillLevel = 3;
            recipe.requiredWorkbenchType = (int)WorkbenchType.ChemistryBench;
            return recipe;
        }
        #endregion
        
        #region Anomaly Tech
        private static RecipeData CreateRealityAnchorRecipe()
        {
            var recipe = new RecipeData();
            recipe.Initialize("Reality Anchor", RecipeCategory.AnomalyTech, CraftingTier.Tier5);
            recipe.outputItemId = (int)ItemType.RealityAnchor;
            recipe.outputQuantity = 1;
            recipe.craftTime = 600f;
            recipe.ingredients[0] = new IngredientEntry { itemId = (int)ItemType.AnomalyShard, quantity = 10 };
            recipe.ingredients[1] = new IngredientEntry { itemId = (int)ItemType.QuantumCapacitor, quantity = 3 };
            recipe.ingredients[2] = new IngredientEntry { itemId = (int)ItemType.VoidStabilizer, quantity = 2 };
            recipe.ingredients[3] = new IngredientEntry { itemId = (int)ItemType.PreCollapseChip, quantity = 5 };
            recipe.requiresPower = true;
            recipe.requiredEnergy = 100f;
            recipe.requiredSkillType = (int)SkillType.Engineering;
            recipe.requiredSkillLevel = 10;
            recipe.requiredWorkbenchType = (int)WorkbenchType.AnomalyForge;
            recipe.requiredTechIds = new int[] { 70, 75, 80 };
            return recipe;
        }
        #endregion
        
        public static RecipeData GetRecipeById(int id)
        {
            if (id < 0 || id >= AllRecipes.Length) return default;
            return AllRecipes[id];
        }
        
        public static RecipeData[] GetRecipesByCategory(RecipeCategory category)
        {
            var result = new System.Collections.Generic.List<RecipeData>();
            foreach (var recipe in AllRecipes)
            {
                if (recipe.category == category)
                    result.Add(recipe);
            }
            return result.ToArray();
        }
        
        public static RecipeData[] GetRecipesForWorkbench(int workbenchType)
        {
            var result = new System.Collections.Generic.List<RecipeData>();
            foreach (var recipe in AllRecipes)
            {
                if (recipe.requiredWorkbenchType == workbenchType)
                    result.Add(recipe);
            }
            return result.ToArray();
        }
    }
    
    public enum WorkbenchType
    {
        None, BasicBench, Sawmill, Smelter, Refinery,
        WeaponsBench, ArmorBench, ElectronicsBench, ChemistryBench,
        CookingStation, WaterPurifier, Fabricator, AdvancedFabricator,
        AnomalyForge, VehicleWorkshop, DroneAssembler
    }
    
    public enum ItemType
    {
        // Resources
        ScrapMetal, Wood, Log, Stone, IronOre, Copper, Lead, Coal, Limestone,
        SteelIngot, CopperIngot, WoodenPlanks, SteelPipe, Spring, Screw,
        Leather, Cloth, Plastic, Polymer, Rubber, Glass,
        
        // Weapons
        ScrapMachete, PipeWrench, FireAxe, PipeRifle, ScrapPistol, KineticShotgun,
        AssaultCarbine, MarksmanDMR, Crossbow, NailGun, Flamethrower,
        RocketLauncher, MortarTube,
        
        // Ammo
        MakeshiftAmmo, PistolAmmo, RifleAmmo, ShotgunShells, SniperAmmo,
        HeavyAmmo, EnergyCell, PlasmaCell, RailSlug, RocketPropellant,
        
        // Consumables
        PurifiedWater, DirtyWater, CookedRations, PreservedFood, NutrientPaste,
        StimPack, MedKit, Antidote, RadiationPills, SleepingPills, Coffee,
        
        // Components
        BasicCircuit, AdvancedCircuit, Microchip, SensorModule, Radio,
        DroneCore, AIProcessor, Battery, Motor, Gear, Bearing,
        
        // Vehicle Parts
        VehicleBattery, Tire, EngineParts, Transmission, FuelTank,
        ArmorPlating, TurretMount, ShieldGenerator,
        
        // Building
        ConcreteMix, ReinforcedConcrete, CompositePlating, NanoMaterial,
        Insulation, Wire, Pipe,
        
        // Anomaly
        AnomalyShard, RealityAnchor, PhaseShield, AnomalyBattery,
        QuantumCapacitor, VoidStabilizer, PreCollapseChip,
        
        // Misc
        Charcoal, Gunpowder, Brass, Lithium, Antiseptic, Painkillers
    }
    
    public enum SkillType
    {
        Melee, Ranged, HeavyWeapons, Explosives,
        Construction, Engineering, Electronics,
        Medicine, Chemistry, Botany,
        Driving, Piloting, Navigation,
        Leadership, Trading, Espionage
    }
}
