using System;
using Unity.Collections;
using Frontier.Core;

namespace Frontier.Narrative.CharacterArcs
{
    /// <summary>
    /// Manages the complex web of relationships between all characters in the world.
    /// Tracks social dynamics, faction alliances, and interpersonal conflicts.
    /// </summary>
    public class RelationshipNetwork : IService
    {
        private NativeHashMap<FixedString64Bytes, SocialNode> _socialNodes;
        private NativeMultiHashMap<FixedString64Bytes, SocialLink> _socialLinks;
        private NativeHashMap<FixedString64Bytes, FactionAlignment> _factionAlignments;
        
        public int Priority => 6;

        public void Initialize()
        {
            _socialNodes = new NativeHashMap<FixedString64Bytes, SocialNode>(256, Allocator.Persistent);
            _socialLinks = new NativeMultiHashMap<FixedString64Bytes, SocialLink>(1024, Allocator.Persistent);
            _factionAlignments = new NativeHashMap<FixedString64Bytes, FactionAlignment>(64, Allocator.Persistent);
        }

        public void Tick(double deltaTime)
        {
            // Propagate relationship changes through the network
            PropagateRelationshipEffects();
            
            // Check for emerging conflicts or alliances
            DetectSocialDynamics();
        }

        public void Shutdown()
        {
            if (_socialNodes.IsCreated) _socialNodes.Dispose();
            if (_socialLinks.IsCreated) _socialLinks.Dispose();
            if (_factionAlignments.IsCreated) _factionAlignments.Dispose();
        }

        /// <summary>
        /// Creates a new social node for a character.
        /// </summary>
        public void CreateSocialNode(FixedString64Bytes characterId, SocialNode node)
        {
            _socialNodes[characterId] = node;
        }

        /// <summary>
        /// Establishes a link between two characters.
        /// </summary>
        public void CreateLink(FixedString64Bytes from, FixedString64Bytes to, LinkType type, float strength)
        {
            var link = new SocialLink
            {
                Target = to,
                Type = type,
                Strength = strength,
                EstablishedTimeTicks = MasterClock.Instance.TotalTicks
            };
            _socialLinks.Add(from, link);
        }

        /// <summary>
        /// Gets all outgoing links from a character.
        /// </summary>
        public NativeArray<SocialLink> GetOutgoingLinks(FixedString64Bytes characterId)
        {
            if (!_socialLinks.TryGetFirstValue(characterId, out var link, out var iterator))
                return new NativeArray<SocialLink>();

            var links = new NativeList<SocialLink>(Allocator.Temp);
            do
            {
                links.Add(link);
            } while (_socialLinks.TryGetNextValue(out link, ref iterator));

            return links.ToArray(Allocator.Persistent);
        }

        /// <summary>
        /// Finds the shortest path of influence between two characters.
        /// </summary>
        public NativeArray<FixedString64Bytes> FindInfluencePath(FixedString64Bytes from, FixedString64Bytes to, int maxDepth)
        {
            var visited = new NativeHashSet<FixedString64Bytes>(64, Allocator.Temp);
            var queue = new NativeQueue<PathNode>(Allocator.Temp);
            var result = new NativeList<FixedString64Bytes>(Allocator.Persistent);

            queue.Enqueue(new PathNode { CharacterId = from, Path = new NativeList<FixedString64Bytes>(Allocator.Temp) });
            visited.Add(from);

            while (queue.Count > 0 && result.Length == 0)
            {
                var current = queue.Dequeue();
                
                if (current.CharacterId == to)
                {
                    result.AddRange(current.Path);
                    result.Add(to);
                    break;
                }

                if (current.Path.Length >= maxDepth)
                    continue;

                var links = GetOutgoingLinks(current.CharacterId);
                for (int i = 0; i < links.Length; i++)
                {
                    if (!visited.Contains(links[i].Target))
                    {
                        visited.Add(links[i].Target);
                        var newPath = new NativeList<FixedString64Bytes>(Allocator.Temp);
                        newPath.AddRange(current.Path);
                        newPath.Add(current.CharacterId);
                        
                        queue.Enqueue(new PathNode 
                        { 
                            CharacterId = links[i].Target, 
                            Path = newPath 
                        });
                    }
                }
                links.Dispose();
            }

            visited.Dispose();
            queue.Dispose();
            return result.ToArray(Allocator.Persistent);
        }

        /// <summary>
        /// Registers a faction alignment for a character.
        /// </summary>
        public void SetFactionAlignment(FixedString64Bytes characterId, FactionAlignment alignment)
        {
            _factionAlignments[characterId] = alignment;
        }

        /// <summary>
        /// Gets the faction alignment of a character.
        /// </summary>
        public FactionAlignment GetFactionAlignment(FixedString64Bytes characterId)
        {
            return _factionAlignments.TryGetValue(characterId, out var alignment) ? alignment : default;
        }

        /// <summary>
        /// Calculates the overall social influence score of a character.
        /// </summary>
        public float CalculateInfluenceScore(FixedString64Bytes characterId)
        {
            if (!_socialNodes.TryGetValue(characterId, out var node))
                return 0f;

            float score = node.BaseInfluence;
            
            // Add influence from connections
            var links = GetOutgoingLinks(characterId);
            for (int i = 0; i < links.Length; i++)
            {
                score += links[i].Strength * GetConnectionWeight(links[i].Type);
            }
            links.Dispose();

            return score;
        }

        /// <summary>
        /// Propagates relationship changes through the network (friend of friend, etc.).
        /// </summary>
        private void PropagateRelationshipEffects()
        {
            // Simplified propagation - would be more complex in full implementation
            var enumerator = _socialNodes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var charId = enumerator.Current.Key;
                var links = GetOutgoingLinks(charId);
                
                for (int i = 0; i < links.Length; i++)
                {
                    if (links[i].Type == LinkType.Alliance || links[i].Type == LinkType.Friendship)
                    {
                        // Allies' allies get slight positive boost
                        var secondDegreeLinks = GetOutgoingLinks(links[i].Target);
                        for (int j = 0; j < secondDegreeLinks.Length; j++)
                        {
                            if (secondDegreeLinks[j].Target != charId)
                            {
                                // Tiny propagation effect
                            }
                        }
                        secondDegreeLinks.Dispose();
                    }
                }
                links.Dispose();
            }
        }

        /// <summary>
        /// Detects emerging social dynamics like rivalries or power struggles.
        /// </summary>
        private void DetectSocialDynamics()
        {
            // Look for characters with many high-strength rivalry links
            var enumerator = _socialNodes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var charId = enumerator.Current.Key;
                var links = GetOutgoingLinks(charId);
                
                int rivalryCount = 0;
                float totalRivalryStrength = 0f;
                
                for (int i = 0; i < links.Length; i++)
                {
                    if (links[i].Type == LinkType.Rivalry)
                    {
                        rivalryCount++;
                        totalRivalryStrength += links[i].Strength;
                    }
                }
                links.Dispose();

                if (rivalryCount >= 3 && totalRivalryStrength > 2.0f)
                {
                    EventBus.Publish(new SocialDynamicDetectedEvent
                    {
                        CharacterId = charId,
                        DynamicType = DynamicType.PowerStruggle,
                        Intensity = totalRivalryStrength / rivalryCount
                    });
                }
            }
        }

        private float GetConnectionWeight(LinkType type)
        {
            switch (type)
            {
                case LinkType.Alliance: return 1.5f;
                case LinkType.Friendship: return 1.2f;
                case LinkType.Romance: return 1.8f;
                case LinkType.Rivalry: return -0.8f;
                case LinkType.Enmity: return -1.5f;
                default: return 0.5f;
            }
        }
    }

    [Serializable]
    public struct SocialNode
    {
        public FixedString64Bytes CharacterId;
        public float BaseInfluence;
        public float Charisma;
        public float Reputation;
        public SocialStatus Status;
    }

    [Serializable]
    public struct SocialLink
    {
        public FixedString64Bytes Target;
        public LinkType Type;
        public float Strength;
        public double EstablishedTimeTicks;
    }

    [Serializable]
    public struct FactionAlignment
    {
        public FixedString64Bytes PrimaryFaction;
        public FixedString64Bytes SecondaryFaction;
        public float LoyaltyToPrimary;
        public float LoyaltyToSecondary;
        public bool IsLeader;
    }

    public struct PathNode
    {
        public FixedString64Bytes CharacterId;
        public NativeList<FixedString64Bytes> Path;
    }

    public enum LinkType
    {
        Neutral,
        Acquaintance,
        Friendship,
        Alliance,
        Romance,
        Family,
        Rivalry,
        Enmity,
        MasterServant,
        MentorStudent
    }

    public enum SocialStatus
    {
        Outcast,
        Commoner,
        Respected,
        Noble,
        Leader,
        Legendary
    }

    public enum DynamicType
    {
        PowerStruggle,
        EmergingAlliance,
        FeudEscalation,
        SocialClimbing,
        BetrayalPlot
    }

    #region Events
    public struct SocialDynamicDetectedEvent : IEvent
    {
        public FixedString64Bytes CharacterId;
        public DynamicType DynamicType;
        public float Intensity;
    }
    #endregion
}
