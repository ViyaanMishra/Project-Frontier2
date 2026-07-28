using System;
using UnityEngine;

namespace Frontier.Core.Models
{
    /// <summary>
    /// Item definition for all game items (resources, weapons, ammo, food, meds, tech).
    /// Supports 200+ item types with categorization and stacking.
    /// </summary>
    [Serializable]
    public struct ItemData
    {
        public ushort ItemId;
        public string ItemName;
        public ItemType Category;
        public ItemRarity Rarity;
        public int MaxStack;
        public float Weight;
        public float Volume;
        public Sprite Icon;
        public string Description;
        
        // Value and trading
        public int BaseValue;
        public CurrencyType CurrencyType;
        
        // Usage properties
        public bool IsConsumable;
        public bool IsEquippable;
        public bool IsThrowable;
        public float UseDuration;
        
        // Effect values
        public int HealthRestore;
        public int StaminaRestore;
        public int HungerRestore;
        public int ThirstRestore;
        public float Toxicity;
        public float Radiation;
        
        // Crafting properties
        public bool IsCraftable;
        public ushort RecipeId;
        public ushort DismantleItemId;
        public int DismantleQuantity;
        
        // Equipment slot
        public EquipmentSlot EquipSlot;
        
        // Tags for filtering
        public ItemTag Tags;
    }

    public enum ItemType
    {
        Resource,
        Weapon,
        Ammo,
        Food,
        Drink,
        Medicine,
        Tool,
        Component,
        Armor,
        Clothing,
        Tech,
        KeyItem,
        Consumable,
        Material,
        Ammunition,
        Fuel,
        Explosive,
        Implant,
        Blueprint,
        Junk
    }

    public enum ItemRarity
    {
        Common,      // White
        Uncommon,    // Green
        Rare,        // Blue
        Epic,        // Purple
        Legendary,   // Orange
        Artifact     // Red (unique)
    }

    public enum CurrencyType
    {
        Scrap,       // Common currency
        Credits,     // Faction currency
        AnomalyShards, // Rare currency
        Fuel,        // Barter currency
        FoodUnits,   // Subsistence currency
        DataCores    // Information currency
    }

    [Flags]
    public enum ItemTag
    {
        None = 0,
        Flammable = 1 << 0,
        Explosive = 1 << 1,
        Radioactive = 1 << 2,
        Toxic = 1 << 3,
        Corrosive = 1 << 4,
        Organic = 1 << 5,
        Metallic = 1 << 6,
        Electronic = 1 << 7,
        Medical = 1 << 8,
        Food = 1 << 9,
        Weapon = 1 << 10,
        Armor = 1 << 11,
        Tool = 1 << 12,
        Container = 1 << 13,
        Quest = 1 << 14,
        Trade = 1 << 15
    }

    public enum EquipmentSlot
    {
        None,
        Head,
        Face,
        Neck,
        Chest,
        Back,
        Shoulder,
        Arm,
        Hands,
        Waist,
        Legs,
        Feet,
        Ring,
        Trinket,
        PrimaryWeapon,
        SecondaryWeapon,
        Utility1,
        Utility2,
        Utility3,
        Utility4
    }

    /// <summary>
    /// Item database containing all item definitions.
    /// Loaded from JSON/SO at startup.
    /// </summary>
    public class ItemDatabase
    {
        private ItemData[] _items;
        private NativeHashMap<ushort, int> _itemIndexMap;

        public int ItemCount => _items?.Length ?? 0;

        public void Initialize(ItemData[] items)
        {
            _items = items;
            _itemIndexMap = new NativeHashMap<ushort, int>(items.Length, Unity.Collections.Allocator.Persistent);
            
            for (int i = 0; i < items.Length; i++)
            {
                _itemIndexMap.Add(items[i].ItemId, i);
            }

            Debug.Log($"[ItemDatabase] Initialized with {items.Length} items");
        }

        public bool TryGetItem(ushort itemId, out ItemData item)
        {
            item = default;
            
            if (!_itemIndexMap.TryGetValue(itemId, out int index))
                return false;

            item = _items[index];
            return true;
        }

        public ItemData GetItemById(ushort itemId)
        {
            if (TryGetItem(itemId, out var item))
                return item;
            
            throw new KeyNotFoundException($"Item ID {itemId} not found");
        }

        public ItemData GetItemByName(string name)
        {
            for (int i = 0; i < _items.Length; i++)
            {
                if (_items[i].ItemName.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return _items[i];
            }
            throw new KeyNotFoundException($"Item name '{name}' not found");
        }

        public ItemData[] GetItemsByCategory(ItemType category)
        {
            var result = new System.Collections.Generic.List<ItemData>();
            
            for (int i = 0; i < _items.Length; i++)
            {
                if (_items[i].Category == category)
                    result.Add(_items[i]);
            }
            
            return result.ToArray();
        }

        public void Dispose()
        {
            if (_itemIndexMap.IsCreated)
                _itemIndexMap.Dispose();
        }
    }

    /// <summary>
    /// Inventory slot for holding items.
    /// </summary>
    [Serializable]
    public struct InventorySlot
    {
        public ushort ItemId;
        public int Quantity;
        public ulong InstanceId; // For tracking unique instances (durability, mods, etc.)

        public bool IsEmpty => ItemId == 0 || Quantity <= 0;
        public bool IsFull => Quantity >= GetMaxStack();

        public int GetMaxStack()
        {
            if (ItemId == 0) return 0;
            
            // Would look up from ItemDatabase in production
            return 99;
        }

        public bool CanAdd(int quantity)
        {
            return Quantity + quantity <= GetMaxStack();
        }

        public void Add(int quantity)
        {
            Quantity = Mathf.Min(Quantity + quantity, GetMaxStack());
        }

        public void Remove(int quantity)
        {
            Quantity = Mathf.Max(Quantity - quantity, 0);
            if (Quantity <= 0)
            {
                ItemId = 0;
                InstanceId = 0;
            }
        }
    }
}
