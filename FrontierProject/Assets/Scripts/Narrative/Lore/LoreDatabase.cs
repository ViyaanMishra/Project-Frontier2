using System;
using Unity.Collections;
using Frontier.Core;

namespace Frontier.Narrative.Lore
{
    /// <summary>
    /// Centralized database for all lore entries, codex information, and world knowledge.
    /// Supports progressive discovery, categorization, and relationship mapping.
    /// </summary>
    public class LoreDatabase : IService
    {
        private NativeHashMap<FixedString64Bytes, LoreEntry> _entries;
        private NativeMultiHashMap<FixedString64Bytes, FixedString64Bytes> _categories;
        private NativeHashMap<FixedString64Bytes, LoreDiscoveryState> _discoveryState;
        private NativeList<FixedString64Bytes> _recentlyDiscovered;
        
        public int Priority => 3;

        public void Initialize()
        {
            _entries = new NativeHashMap<FixedString64Bytes, LoreEntry>(512, Allocator.Persistent);
            _categories = new NativeMultiHashMap<FixedString64Bytes, FixedString64Bytes>(1024, Allocator.Persistent);
            _discoveryState = new NativeHashMap<FixedString64Bytes, LoreDiscoveryState>(256, Allocator.Persistent);
            _recentlyDiscovered = new NativeList<FixedString64Bytes>(Allocator.Persistent);
        }

        public void Tick(double deltaTime)
        {
            // Clear recently discovered list after display period
            if (_recentlyDiscovered.Length > 0)
            {
                _recentlyDiscovered.Clear();
            }
        }

        public void Shutdown()
        {
            if (_entries.IsCreated) _entries.Dispose();
            if (_categories.IsCreated) _categories.Dispose();
            if (_discoveryState.IsCreated) _discoveryState.Dispose();
            if (_recentlyDiscovered.IsCreated) _recentlyDiscovered.Dispose();
        }

        /// <summary>
        /// Registers a new lore entry in the database.
        /// </summary>
        public void RegisterEntry(LoreEntry entry)
        {
            _entries[entry.Id] = entry;
            
            foreach (var category in entry.Categories)
            {
                _categories.Add(category, entry.Id);
            }
            
            var state = new LoreDiscoveryState
            {
                EntryId = entry.Id,
                IsDiscovered = false,
                DiscoveryTimeTicks = 0,
                ReadCount = 0,
                FragmentProgress = 0f
            };
            _discoveryState[entry.Id] = state;
        }

        /// <summary>
        /// Marks a lore entry as discovered by the player.
        /// </summary>
        public bool DiscoverEntry(FixedString64Bytes entryId)
        {
            if (!_entries.TryGetValue(entryId, out var entry))
                return false;
            
            if (!_discoveryState.TryGetValue(entryId, out var state))
                return false;

            if (state.IsDiscovered)
                return false; // Already discovered

            state.IsDiscovered = true;
            state.DiscoveryTimeTicks = MasterClock.Instance.TotalTicks;
            _discoveryState[entryId] = state;
            
            _recentlyDiscovered.Add(entryId);

            EventBus.Publish(new LoreDiscoveredEvent 
            { 
                EntryId = entryId, 
                Category = entry.Categories.Length > 0 ? entry.Categories[0] : new FixedString64Bytes() 
            });

            return true;
        }

        /// <summary>
        /// Gets a lore entry by ID.
        /// </summary>
        public LoreEntry GetEntry(FixedString64Bytes entryId)
        {
            return _entries.TryGetValue(entryId, out var entry) ? entry : default;
        }

        /// <summary>
        /// Gets all entries in a category.
        /// </summary>
        public NativeArray<FixedString64Bytes> GetEntriesByCategory(FixedString64Bytes category)
        {
            if (!_categories.TryGetFirstValue(category, out var entryId, out var iterator))
                return new NativeArray<FixedString64Bytes>();

            var entries = new NativeList<FixedString64Bytes>(Allocator.Temp);
            do
            {
                entries.Add(entryId);
            } while (_categories.TryGetNextValue(out entryId, ref iterator));

            return entries.ToArray(Allocator.Persistent);
        }

        /// <summary>
        /// Gets the discovery state of an entry.
        /// </summary>
        public LoreDiscoveryState GetDiscoveryState(FixedString64Bytes entryId)
        {
            return _discoveryState.TryGetValue(entryId, out var state) ? state : default;
        }

        /// <summary>
        /// Finds related lore entries based on tags or content.
        /// </summary>
        public NativeArray<FixedString64Bytes> FindRelatedEntries(FixedString64Bytes entryId, int maxResults)
        {
            if (!_entries.TryGetValue(entryId, out var entry))
                return new NativeArray<FixedString64Bytes>();

            var related = new NativeList<FixedString64Bytes>(Allocator.Temp);
            var enumerator = _entries.GetEnumerator();
            
            while (enumerator.MoveNext() && related.Length < maxResults)
            {
                var other = enumerator.Current.Value;
                if (other.Id == entryId)
                    continue;

                float relevance = CalculateRelevance(entry, other);
                if (relevance > 0.3f)
                {
                    related.Add(other.Id);
                }
            }

            return related.ToArray(Allocator.Persistent);
        }

        /// <summary>
        /// Updates fragment progress for collectible lore entries.
        /// </summary>
        public void UpdateFragmentProgress(FixedString64Bytes entryId, float progress)
        {
            if (!_discoveryState.TryGetValue(entryId, out var state))
                return;

            state.FragmentProgress = Math.Min(1.0f, state.FragmentProgress + progress);
            
            if (state.FragmentProgress >= 1.0f && !state.IsDiscovered)
            {
                DiscoverEntry(entryId);
            }
            
            _discoveryState[entryId] = state;
        }

        /// <summary>
        /// Increments the read count for an entry.
        /// </summary>
        public void MarkAsRead(FixedString64Bytes entryId)
        {
            if (!_discoveryState.TryGetValue(entryId, out var state))
                return;

            state.ReadCount++;
            state.LastReadTimeTicks = MasterClock.Instance.TotalTicks;
            _discoveryState[entryId] = state;
        }

        /// <summary>
        /// Calculates relevance between two lore entries.
        /// </summary>
        private float CalculateRelevance(LoreEntry a, LoreEntry b)
        {
            float score = 0f;

            // Category overlap
            foreach (var catA in a.Categories)
            {
                foreach (var catB in b.Categories)
                {
                    if (catA == catB)
                        score += 0.3f;
                }
            }

            // Shared characters
            foreach (var charA in a.RelatedCharacters)
            {
                foreach (var charB in b.RelatedCharacters)
                {
                    if (charA == charB)
                        score += 0.2f;
                }
            }

            // Shared locations
            foreach (var locA in a.RelatedLocations)
            {
                foreach (var locB in b.RelatedLocations)
                {
                    if (locA == locB)
                        score += 0.2f;
                }
            }

            return Math.Min(1.0f, score);
        }
    }

    [Serializable]
    public struct LoreEntry
    {
        public FixedString64Bytes Id;
        public FixedString128Bytes Title;
        public FixedString2048Bytes Content;
        public FixedString512Bytes Summary;
        public NativeArray<FixedString64Bytes> Categories;
        public NativeArray<FixedString64Bytes> RelatedCharacters;
        public NativeArray<FixedString64Bytes> RelatedLocations;
        public NativeArray<FixedString64Bytes> RelatedEvents;
        public LoreTier Tier;
        public bool IsSpoiler;
        public int MinPlayerLevel;
    }

    [Serializable]
    public struct LoreDiscoveryState
    {
        public FixedString64Bytes EntryId;
        public bool IsDiscovered;
        public double DiscoveryTimeTicks;
        public double LastReadTimeTicks;
        public int ReadCount;
        public float FragmentProgress;
    }

    public enum LoreTier
    {
        Common,     // Basic information, easily found
        Uncommon,   // Requires some exploration
        Rare,       // Hidden or well-guarded
        Epic,       // Major story revelations
        Legendary   // Ultimate secrets of the universe
    }

    public enum LoreCategory
    {
        Characters,
        Factions,
        Locations,
        History,
        Technology,
        Creatures,
        Artifacts,
        Events,
        Mysteries
    }

    #region Events
    public struct LoreDiscoveredEvent : IEvent
    {
        public FixedString64Bytes EntryId;
        public FixedString64Bytes Category;
    }
    #endregion
}
