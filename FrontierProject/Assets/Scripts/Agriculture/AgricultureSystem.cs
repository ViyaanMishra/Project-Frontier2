using Unity.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace Frontier.Agriculture
{
    /// <summary>
    /// Manages 24 crop types, soil quality, growth stages, pests, and animal husbandry.
    /// </summary>
    public static class AgricultureSystem
    {
        public enum CropType
        {
            Wheat, Corn, Rice, Potatoes, Carrots, Tomatoes, Lettuce, Strawberries,
            Soybeans, Cotton, Coffee, Tobacco, MedicinalHerbs, AnomalyFlora,
            Pumpkins, Beans, Peppers, Onions, Garlic, Mushrooms, Sugarcane,
            Barley, Oats, Hemp
        }

        public enum GrowthStage : byte
        {
            Seed, Sprout, Young, Mature, Flowering, Fruiting, Ready, Dead
        }

        [System.Serializable]
        public struct CropDefinition
        {
            public CropType type;
            public string displayName;
            public float growthTimeHours;
            public int yieldMin;
            public int yieldMax;
            public int[] requiredItems; // seeds, fertilizer
            public float waterRequirement;
            public float temperatureMin;
            public float temperatureMax;
            public float soilQualityMin;
            public bool isGreenhouseOnly;
            public bool isHydroponicCapable;
        }

        [System.Serializable]
        public struct AnimalDefinition
        {
            public string species;
            public string displayName;
            public float maturationDays;
            public float reproductionDays;
            public int offspringMin;
            public int offspringMax;
            public int[] products; // eggs, milk, meat, wool
            public float productYieldPerDay;
            public int[] foodRequirements;
        }

        public struct CropPlot
        {
            public int plotId;
            public CropType? plantedCrop;
            public GrowthStage stage;
            public float growthProgress; // 0-1
            public float moisture; // 0-1
            public float soilQuality; // 0-1
            public float temperature;
            public bool hasPestInfestation;
            public bool hasBlight;
            public bool isFertilized;
            public int daysPlanted;
        }

        public struct Livestock
        {
            public int id;
            public string species;
            public float age; // in days
            public float health; // 0-1
            public float happiness; // 0-1
            public float hunger; // 0-1
            public bool isPregnant;
            public float pregnancyProgress;
            public int productId;
            public float productAccumulation;
        }

        private static readonly CropDefinition[] CropDefinitions = new CropDefinition[]
        {
            new CropDefinition { type = CropType.Wheat, displayName = "Wheat", growthTimeHours = 72f, yieldMin = 3, yieldMax = 6, requiredItems = new[] { 1001 }, waterRequirement = 0.5f, temperatureMin = 10f, temperatureMax = 30f, soilQualityMin = 0.3f },
            new CropDefinition { type = CropType.Corn, displayName = "Corn", growthTimeHours = 96f, yieldMin = 2, yieldMax = 4, requiredItems = new[] { 1002 }, waterRequirement = 0.6f, temperatureMin = 15f, temperatureMax = 35f, soilQualityMin = 0.4f },
            new CropDefinition { type = CropType.Potatoes, displayName = "Potatoes", growthTimeHours = 84f, yieldMin = 4, yieldMax = 8, requiredItems = new[] { 1003 }, waterRequirement = 0.4f, temperatureMin = 10f, temperatureMax = 25f, soilQualityMin = 0.3f },
            new CropDefinition { type = CropType.Tomatoes, displayName = "Tomatoes", growthTimeHours = 60f, yieldMin = 5, yieldMax = 10, requiredItems = new[] { 1004 }, waterRequirement = 0.7f, temperatureMin = 18f, temperatureMax = 32f, soilQualityMin = 0.5f },
            new CropDefinition { type = CropType.MedicinalHerbs, displayName = "Medicinal Herbs", growthTimeHours = 48f, yieldMin = 2, yieldMax = 5, requiredItems = new[] { 1005 }, waterRequirement = 0.3f, temperatureMin = 15f, temperatureMax = 28f, soilQualityMin = 0.4f, isGreenhouseOnly = true },
            new CropDefinition { type = CropType.AnomalyFlora, displayName = "Anomaly Flora", growthTimeHours = 120f, yieldMin = 1, yieldMax = 3, requiredItems = new[] { 9001 }, waterRequirement = 0.2f, temperatureMin = 5f, temperatureMax = 40f, soilQualityMin = 0.6f, isGreenhouseOnly = true }
        };

        private static readonly AnimalDefinition[] AnimalDefinitions = new AnimalDefinition[]
        {
            new AnimalDefinition { species = "chicken", displayName = "Chicken", maturationDays = 30f, reproductionDays = 0f, products = new[] { 2001, 2002 }, productYieldPerDay = 0.7f, foodRequirements = new[] { 1001, 1002 } },
            new AnimalDefinition { species = "goat", displayName = "Goat", maturationDays = 180f, reproductionDays = 150f, offspringMin = 1, offspringMax = 3, products = new[] { 2003, 2004 }, productYieldPerDay = 0.3f, foodRequirements = new[] { 1001, 1006 } },
            new AnimalDefinition { species = "mutated_cattle", displayName = "Mutated Cattle", maturationDays = 365f, reproductionDays = 280f, offspringMin = 1, offspringMax = 2, products = new[] { 2005, 2006 }, productYieldPerDay = 0.5f, foodRequirements = new[] { 1001, 1002, 1006 } },
            new AnimalDefinition { species = "war_boar", displayName = "Armored War Boar", maturationDays = 120f, reproductionDays = 100f, offspringMin = 2, offspringMax = 5, products = new[] { 2007 }, productYieldPerDay = 0f, foodRequirements = new[] { 1003, 1007 } }
        };

        public static void UpdateCrop(ref CropPlot plot, float deltaTime, float ambientTemp, float rainfall)
        {
            if (plot.plantedCrop == null) return;

            var def = GetCropDefinition(plot.plantedCrop.Value);
            
            // Update moisture
            plot.moisture = Mathf.Clamp01(plot.moisture + rainfall * deltaTime - 0.01f * deltaTime);
            plot.temperature = ambientTemp;

            // Check conditions
            bool canGrow = plot.moisture >= def.waterRequirement * 0.5f &&
                          plot.temperature >= def.temperatureMin &&
                          plot.temperature <= def.temperatureMax &&
                          plot.soilQuality >= def.soilQualityMin &&
                          !plot.hasBlight;

            if (canGrow)
            {
                float growthRate = 1f / (def.growthTimeHours * 60f); // per minute
                plot.growthProgress += growthRate * deltaTime * 60f;
                plot.daysPlanted++;

                // Update stage
                if (plot.growthProgress >= 1f)
                {
                    plot.stage = GrowthStage.Ready;
                }
                else if (plot.growthProgress >= 0.8f)
                {
                    plot.stage = GrowthStage.Fruiting;
                }
                else if (plot.growthProgress >= 0.6f)
                {
                    plot.stage = GrowthStage.Flowering;
                }
                else if (plot.growthProgress >= 0.4f)
                {
                    plot.stage = GrowthStage.Mature;
                }
                else if (plot.growthProgress >= 0.2f)
                {
                    plot.stage = GrowthStage.Young;
                }
                else if (plot.growthProgress >= 0.1f)
                {
                    plot.stage = GrowthStage.Sprout;
                }
            }

            // Pest chance
            if (!plot.hasPestInfestation && Random.value < 0.001f)
            {
                plot.hasPestInfestation = true;
            }

            // Pests reduce growth
            if (plot.hasPestInfestation)
            {
                plot.growthProgress -= 0.001f * deltaTime;
            }
        }

        public static int HarvestCrop(ref CropPlot plot)
        {
            if (plot.stage != GrowthStage.Ready || plot.plantedCrop == null) return 0;

            var def = GetCropDefinition(plot.plantedCrop.Value);
            int yield = Random.Range(def.yieldMin, def.yieldMax + 1);

            // Reset plot
            plot.plantedCrop = null;
            plot.stage = GrowthStage.Seed;
            plot.growthProgress = 0;
            plot.hasPestInfestation = false;
            plot.isFertilized = false;

            return yield;
        }

        public static void UpdateLivestock(ref Livestock animal, float deltaTime, int[] availableFood)
        {
            // Hunger increase
            animal.hunger = Mathf.Clamp01(animal.hunger + 0.01f * deltaTime);

            // Feed if food available
            var def = GetAnimalDefinition(animal.species);
            if (animal.hunger > 0.5f && HasFood(availableFood, def.foodRequirements))
            {
                animal.hunger -= 0.1f * deltaTime;
                animal.happiness = Mathf.Clamp01(animal.happiness + 0.01f * deltaTime);
            }
            else
            {
                animal.happiness = Mathf.Max(0, animal.happiness - 0.01f * deltaTime);
            }

            // Age
            animal.age += deltaTime / 1440f; // days

            // Product accumulation
            if (animal.age >= GetAnimalDefinition(animal.species).maturationDays)
            {
                animal.productAccumulation += def.productYieldPerDay * deltaTime / 1440f;
            }

            // Health decay if unhappy/hungry
            if (animal.hunger > 0.8f || animal.happiness < 0.3f)
            {
                animal.health = Mathf.Max(0, animal.health - 0.001f * deltaTime);
            }
        }

        public static int CollectProduct(ref Livestock animal)
        {
            if (animal.productAccumulation < 1f) return 0;

            int amount = (int)animal.productAccumulation;
            animal.productAccumulation -= amount;
            return amount;
        }

        private static CropDefinition GetCropDefinition(CropType type)
        {
            foreach (var def in CropDefinitions)
            {
                if (def.type == type) return def;
            }
            return CropDefinitions[0];
        }

        private static AnimalDefinition GetAnimalDefinition(string species)
        {
            foreach (var def in AnimalDefinitions)
            {
                if (def.species == species) return def;
            }
            return AnimalDefinitions[0];
        }

        private static bool HasFood(int[] available, int[] required)
        {
            foreach (var req in required)
            {
                bool found = false;
                foreach (var avail in available)
                {
                    if (avail == req) { found = true; break; }
                }
                if (!found) return false;
            }
            return true;
        }
    }
}
