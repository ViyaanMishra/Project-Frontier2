using System;
using UnityEngine;

namespace Frontier.Modding
{
    /// <summary>
    /// Mod loader supporting JSON data loading from user://mods/ directory.
    /// Provides hooks for items, recipes, dialogue, and events.
    /// </summary>
    public class ModLoader : MonoBehaviour
    {
        public static ModLoader Instance { get; private set; }
        
        [SerializeField] private string modsDirectory = "Mods";
        [SerializeField] private bool enableModdedContent = true;
        
        private NativeHashMap<string, ModInfo> loadedMods;
        private System.Collections.Generic.List<IModHook> activeHooks;
        
        public event Action<ModInfo> OnModLoaded;
        public event Action<ModInfo> OnModUnloaded;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            loadedMods = new NativeHashMap<string, ModInfo>(50);
            activeHooks = new System.Collections.Generic.List<IModHook>();
        }
        
        public void LoadAllMods()
        {
            if (!enableModdedContent) return;
            
            string modsPath = System.IO.Path.Combine(Application.persistentDataPath, modsDirectory);
            
            if (!System.IO.Directory.Exists(modsPath))
            {
                System.IO.Directory.CreateDirectory(modsPath);
                Debug.Log($"Created mods directory at: {modsPath}");
                return;
            }
            
            string[] modFolders = System.IO.Directory.GetDirectories(modsPath);
            
            foreach (string modFolder in modFolders)
            {
                string manifestPath = System.IO.Path.Combine(modFolder, "mod.json");
                if (System.IO.File.Exists(manifestPath))
                {
                    LoadMod(manifestPath);
                }
            }
            
            Debug.Log($"Loaded {loadedMods.Count()} mods");
        }
        
        public void LoadMod(string manifestPath)
        {
            try
            {
                string json = System.IO.File.ReadAllText(manifestPath);
                ModManifest manifest = JsonUtility.FromJson<ModManifest>(json);
                
                if (!manifest.IsValid())
                {
                    Debug.LogError($"Invalid mod manifest: {manifestPath}");
                    return;
                }
                
                // Check version compatibility
                if (!IsVersionCompatible(manifest.requiredGameVersion))
                {
                    Debug.LogWarning($"Mod {manifest.id} requires game version {manifest.requiredGameVersion}");
                }
                
                // Check for duplicates
                if (loadedMods.ContainsKey(manifest.id))
                {
                    Debug.LogWarning($"Mod {manifest.id} already loaded, skipping");
                    return;
                }
                
                var modInfo = new ModInfo
                {
                    id = manifest.id,
                    name = manifest.name,
                    version = manifest.version,
                    author = manifest.author,
                    description = manifest.description,
                    path = System.IO.Path.GetDirectoryName(manifestPath),
                    isEnabled = true,
                    loadOrder = manifest.loadOrder
                };
                
                loadedMods.Add(manifest.id, modInfo);
                
                // Load content based on manifest
                LoadModContent(modInfo, manifest);
                
                OnModLoaded?.Invoke(modInfo);
                Debug.Log($"Loaded mod: {manifest.name} v{manifest.version}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load mod from {manifestPath}: {e.Message}");
            }
        }
        
        private void LoadModContent(ModInfo modInfo, ModManifest manifest)
        {
            string modPath = modInfo.path;
            
            // Load item definitions
            if (manifest.itemsFile.Length > 0)
            {
                string itemsPath = System.IO.Path.Combine(modPath, manifest.itemsFile);
                if (System.IO.File.Exists(itemsPath))
                {
                    LoadModItems(itemsPath, modInfo);
                }
            }
            
            // Load recipe definitions
            if (manifest.recipesFile.Length > 0)
            {
                string recipesPath = System.IO.Path.Combine(modPath, manifest.recipesFile);
                if (System.IO.File.Exists(recipesPath))
                {
                    LoadModRecipes(recipesPath, modInfo);
                }
            }
            
            // Load dialogue trees
            if (manifest.dialogueFile.Length > 0)
            {
                string dialoguePath = System.IO.Path.Combine(modPath, manifest.dialogueFile);
                if (System.IO.File.Exists(dialoguePath))
                {
                    LoadModDialogue(dialoguePath, modInfo);
                }
            }
            
            // Initialize mod hooks
            if (manifest.hasCodeHooks && manifest.hooksAssembly.Length > 0)
            {
                InitializeModHooks(modInfo, manifest.hooksAssembly);
            }
        }
        
        private void LoadModItems(string itemsPath, ModInfo modInfo)
        {
            string json = System.IO.File.ReadAllText(itemsPath);
            ModItemDefinition[] items = JsonUtility.FromJson<ModItemArray>(json).items;
            
            foreach (var item in items)
            {
                // Register with item database
                ModAPI.RegisterModItem(modInfo.id, item);
            }
            
            Debug.Log($"Loaded {items.Length} items from mod {modInfo.name}");
        }
        
        private void LoadModRecipes(string recipesPath, ModInfo modInfo)
        {
            string json = System.IO.File.ReadAllText(recipesPath);
            ModRecipeDefinition[] recipes = JsonUtility.FromJson<ModRecipeArray>(json).recipes;
            
            foreach (var recipe in recipes)
            {
                // Register with recipe database
                ModAPI.RegisterModRecipe(modInfo.id, recipe);
            }
            
            Debug.Log($"Loaded {recipes.Length} recipes from mod {modInfo.name}");
        }
        
        private void LoadModDialogue(string dialoguePath, ModInfo modInfo)
        {
            string json = System.IO.File.ReadAllText(dialoguePath);
            ModDialogueTree[] dialogues = JsonUtility.FromJson<ModDialogueArray>(json).dialogues;
            
            foreach (var dialogue in dialogues)
            {
                ModAPI.RegisterModDialogue(modInfo.id, dialogue);
            }
        }
        
        private void InitializeModHooks(ModInfo modInfo, string assemblyName)
        {
            // In a real implementation, this would load the assembly and find IModHook implementations
            // For now, we'll just log that hooks would be initialized
            Debug.Log($"Initializing code hooks for mod {modInfo.name} from {assemblyName}");
        }
        
        public void UnloadMod(string modId)
        {
            if (!loadedMods.TryGetValue(modId, out var modInfo)) return;
            
            // Remove hooks
            var modHooks = activeHooks.FindAll(h => h.ModId == modId);
            foreach (var hook in modHooks)
            {
                hook.OnUnload();
                activeHooks.Remove(hook);
            }
            
            // Remove content
            ModAPI.UnregisterModContent(modId);
            
            loadedMods.Remove(modId);
            OnModUnloaded?.Invoke(modInfo);
            
            Debug.Log($"Unloaded mod: {modInfo.name}");
        }
        
        public bool IsModLoaded(string modId)
        {
            return loadedMods.ContainsKey(modId);
        }
        
        public bool IsModEnabled(string modId)
        {
            return loadedMods.TryGetValue(modId, out var info) && info.isEnabled;
        }
        
        public void SetModEnabled(string modId, bool enabled)
        {
            if (!loadedMods.TryGetValue(modId, out var info)) return;
            
            info.isEnabled = enabled;
            loadedMods[modId] = info;
            
            if (!enabled)
            {
                UnloadMod(modId);
            }
        }
        
        public ModInfo[] GetLoadedMods()
        {
            var result = new ModInfo[loadedMods.Count()];
            int i = 0;
            foreach (var kvp in loadedMods)
            {
                result[i++] = kvp.Value;
            }
            return result;
        }
        
        private bool IsVersionCompatible(string requiredVersion)
        {
            // Simple version check - in production would use proper semver comparison
            string gameVersion = Application.version;
            return requiredVersion == "*" || requiredVersion == gameVersion;
        }
        
        public void RegisterHook(IModHook hook)
        {
            if (!activeHooks.Contains(hook))
            {
                activeHooks.Add(hook);
                hook.OnLoad();
            }
        }
        
        public void UnregisterHook(IModHook hook)
        {
            if (activeHooks.Contains(hook))
            {
                hook.OnUnload();
                activeHooks.Remove(hook);
            }
        }
    }
    
    [Serializable]
    public struct ModManifest
    {
        public string id;
        public string name;
        public string version;
        public string author;
        public string description;
        public string requiredGameVersion;
        public int loadOrder;
        public string[] dependencies;
        public string itemsFile;
        public string recipesFile;
        public string dialogueFile;
        public bool hasCodeHooks;
        public string hooksAssembly;
        
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(id) && 
                   !string.IsNullOrEmpty(name) && 
                   !string.IsNullOrEmpty(version);
        }
    }
    
    [Serializable]
    public struct ModInfo
    {
        public string id;
        public string name;
        public string version;
        public string author;
        public string description;
        public string path;
        public bool isEnabled;
        public int loadOrder;
    }
    
    [Serializable]
    public struct ModItemDefinition
    {
        public string itemId;
        public string itemName;
        public string itemType;
        public int maxStack;
        public float weight;
        public string iconPath;
        public string description;
        public ModItemStats stats;
    }
    
    [Serializable]
    public struct ModItemStats
    {
        public float damage;
        public float defense;
        public float durability;
        public int value;
    }
    
    [Serializable]
    public struct ModRecipeDefinition
    {
        public string recipeId;
        public string recipeName;
        public string outputItemId;
        public int outputQuantity;
        public float craftTime;
        public ModIngredient[] ingredients;
        public string requiredWorkbench;
        public string[] prerequisites;
    }
    
    [Serializable]
    public struct ModIngredient
    {
        public string itemId;
        public int quantity;
    }
    
    [Serializable]
    public struct ModDialogueTree
    {
        public string dialogueId;
        public string speakerId;
        public ModDialogueNode[] nodes;
    }
    
    [Serializable]
    public struct ModDialogueNode
    {
        public string nodeId;
        public string text;
        public string[] responses;
        public string[] nextNodes;
        public string[] conditions;
    }
    
    // Wrapper classes for JSON arrays
    [Serializable]
    public class ModItemArray
    {
        public ModItemDefinition[] items;
    }
    
    [Serializable]
    public class ModRecipeArray
    {
        public ModRecipeDefinition[] recipes;
    }
    
    [Serializable]
    public class ModDialogueArray
    {
        public ModDialogueTree[] dialogues;
    }
    
    public interface IModHook
    {
        string ModId { get; }
        void OnLoad();
        void OnUnload();
        void OnUpdate(float deltaTime);
    }
    
    /// <summary>
    /// Static API for mod content registration.
    /// </summary>
    public static class ModAPI
    {
        public static event Action<string, ModItemDefinition> OnItemRegistered;
        public static event Action<string, ModRecipeDefinition> OnRecipeRegistered;
        public static event Action<string, ModDialogueTree> OnDialogueRegistered;
        
        private static NativeHashMap<string, ModItemDefinition> modItems;
        private static NativeHashMap<string, ModRecipeDefinition> modRecipes;
        private static NativeHashMap<string, ModDialogueTree> modDialogues;
        
        static ModAPI()
        {
            modItems = new NativeHashMap<string, ModItemDefinition>(1000);
            modRecipes = new NativeHashMap<string, ModRecipeDefinition>(500);
            modDialogues = new NativeHashMap<string, ModDialogueTree>(200);
        }
        
        public static void RegisterModItem(string modId, ModItemDefinition item)
        {
            string key = $"{modId}:{item.itemId}";
            if (!modItems.ContainsKey(key))
            {
                modItems.Add(key, item);
                OnItemRegistered?.Invoke(modId, item);
            }
        }
        
        public static void RegisterModRecipe(string modId, ModRecipeDefinition recipe)
        {
            string key = $"{modId}:{recipe.recipeId}";
            if (!modRecipes.ContainsKey(key))
            {
                modRecipes.Add(key, recipe);
                OnRecipeRegistered?.Invoke(modId, recipe);
            }
        }
        
        public static void RegisterModDialogue(string modId, ModDialogueTree dialogue)
        {
            string key = $"{modId}:{dialogue.dialogueId}";
            if (!modDialogues.ContainsKey(key))
            {
                modDialogues.Add(key, dialogue);
                OnDialogueRegistered?.Invoke(modId, dialogue);
            }
        }
        
        public static void UnregisterModContent(string modId)
        {
            // Remove all content for this mod
            var itemsToRemove = new System.Collections.Generic.List<string>();
            foreach (var kvp in modItems)
            {
                if (kvp.Key.StartsWith(modId + ":"))
                    itemsToRemove.Add(kvp.Key);
            }
            foreach (var key in itemsToRemove)
                modItems.Remove(key);
            
            var recipesToRemove = new System.Collections.Generic.List<string>();
            foreach (var kvp in modRecipes)
            {
                if (kvp.Key.StartsWith(modId + ":"))
                    recipesToRemove.Add(kvp.Key);
            }
            foreach (var key in recipesToRemove)
                modRecipes.Remove(key);
            
            var dialoguesToRemove = new System.Collections.Generic.List<string>();
            foreach (var kvp in modDialogues)
            {
                if (kvp.Key.StartsWith(modId + ":"))
                    dialoguesToRemove.Add(kvp.Key);
            }
            foreach (var key in dialoguesToRemove)
                modDialogues.Remove(key);
        }
        
        public static ModItemDefinition GetModItem(string modId, string itemId)
        {
            string key = $"{modId}:{itemId}";
            return modItems.TryGetValue(key, out var item) ? item : default;
        }
        
        public static ModRecipeDefinition GetModRecipe(string modId, string recipeId)
        {
            string key = $"{modId}:{recipeId}";
            return modRecipes.TryGetValue(key, out var recipe) ? recipe : default;
        }
    }
}
