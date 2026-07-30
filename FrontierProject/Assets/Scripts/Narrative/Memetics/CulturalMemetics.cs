using System;
using Unity.Collections;
using Frontier.Core;
using Frontier.Simulation;

namespace Frontier.Narrative.Memetics
{
    /// <summary>
    /// Simulates the spread and evolution of cultural ideas, traditions, and taboos.
    /// Memes (cultural units) replicate, mutate, and die based on social pressure and utility.
    /// </summary>
    public class CulturalMemetics : IService
    {
        public int Priority => 18;

        private NativeHashMap<int, CulturalMeme> _memePool;
        private NativeHashMap<int, NativeList<int>> _factionMemes; // FactionID -> List of MemeIDs

        public struct CulturalMeme
        {
            public FixedString128Bytes Id;
            public FixedString512Bytes Description;
            public MemeType Type;
            public float Virality;       // Likelihood to spread
            public float Mutability;     // Likelihood to change when spreading
            public float Utility;        // Survival benefit to holder
            public float Stigma;         // Social cost if discovered
            public int OriginFaction;
            public int AgeInDays;
        }

        public enum MemeType
        {
            Tradition, Taboo, Superstition, Technique, ArtStyle, MusicStyle, Ritual, Myth, Law, LanguageDialect
        }

        public void Initialize()
        {
            _memePool = new NativeHashMap<int, CulturalMeme>(256, Allocator.Persistent);
            _factionMemes = new NativeHashMap<int, NativeList<int>>(64, Allocator.Persistent);
            
            SeedInitialMemes();
            UnityEngine.Debug.Log("[CulturalMemetics] Initialized cultural evolution engine.");
        }

        public void Tick(float dt)
        {
            float days = dt / 60f; // Assuming 60 ticks per second
            
            var iterator = _memePool.GetEnumerator();
            while (iterator.MoveNext())
            {
                var meme = iterator.Current.Value;
                meme.AgeInDays += (int)days;
                
                // Decay virality over time unless reinforced
                meme.Virality *= 0.99f;
                
                _memePool[iterator.Current.Key] = meme;
            }
        }

        public void Shutdown()
        {
            if (_memePool.IsCreated) _memePool.Dispose();
            if (_factionMemes.IsCreated)
            {
                var listIter = _factionMemes.GetEnumerator();
                while (listIter.MoveNext())
                {
                    listIter.Current.Value.Dispose();
                }
                _factionMemes.Dispose();
            }
        }

        private void SeedInitialMemes()
        {
            // Seed base cultural memes for factions
            CreateMeme("Tradition_Harvest", "Annual harvest festival with bonfires", MemeType.Tradition, 0.3f, 0.1f, 0.2f, 0f, 1);
            CreateMeme("Taboo_Whistling", "Whistling at night summons spirits", MemeType.Taboo, 0.5f, 0.2f, 0f, 0.1f, 1);
            CreateMeme("Technique_Smelt", "Advanced ore smelting method", MemeType.Technique, 0.4f, 0.05f, 0.8f, 0f, 2);
        }

        private void CreateMeme(string id, string desc, MemeType type, float virality, float mutability, float utility, float stigma, int originFaction)
        {
            var meme = new CulturalMeme
            {
                Id = new FixedString128Bytes(id),
                Description = new FixedString512Bytes(desc),
                Type = type,
                Virality = virality,
                Mutability = mutability,
                Utility = utility,
                Stigma = stigma,
                OriginFaction = originFaction,
                AgeInDays = 0
            };
            
            int hash = id.GetHashCode();
            _memePool.Add(hash, meme);

            if (!_factionMemes.ContainsKey(originFaction))
            {
                _factionMemes.Add(originFaction, new NativeList<int>(Allocator.Temp));
            }
            var list = _factionMemes[originFaction];
            list.Add(hash);
            _factionMemes[originFaction] = list;
        }

        public FixedString512Bytes ApplyCulturalDialect(FixedString512Bytes text)
        {
            // Modify text based on active cultural memes (dialects, slang)
            // Placeholder for complex linguistic transformation
            return text;
        }

        public bool HasMeme(int factionId, MemeType type)
        {
            if (_factionMemes.TryGetValue(factionId, out var memes))
            {
                for (int i = 0; i < memes.Length; i++)
                {
                    if (_memePool.TryGetValue(memes[i], out var meme) && meme.Type == type)
                        return true;
                }
            }
            return false;
        }

        public void SpreadMeme(int sourceFaction, int targetFaction, int memeId)
        {
            // Logic to copy meme to another faction with potential mutation
            if (_memePool.TryGetValue(memeId, out var meme))
            {
                // Roll for mutation
                if (UnityEngine.Random.value < meme.Mutability)
                {
                    meme.Description = new FixedString512Bytes(meme.Description.ToString() + " (Mutated)");
                }
                
                if (!_factionMemes.ContainsKey(targetFaction))
                {
                    _factionMemes.Add(targetFaction, new NativeList<int>(Allocator.Temp));
                }
                var list = _factionMemes[targetFaction];
                list.Add(memeId);
                _factionMemes[targetFaction] = list;
            }
        }
    }
}
