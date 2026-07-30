using System;
using Unity.Collections;
using Frontier.Core;
using Frontier.Core.Models;

namespace Frontier.Narrative.Emergent
{
    public struct GossipEntry
    {
        public FixedString128Bytes OriginatorId;
        public FixedString512Bytes Content;
        public float TruthValue; // Degrades as it spreads
        public int SpreadCount;
        public long BirthTick;
    }

    public class GossipSystem : IService
    {
        private NativeList<GossipEntry> _gossipPool;
        private readonly float _truthDecayRate = 0.05f;

        public int Priority => 7;

        public void Initialize()
        {
            _gossipPool = new NativeList<GossipEntry>(Allocator.Persistent);
        }

        public void Tick(float dt)
        {
            MutateGossip();
            SpreadGossip();
        }

        public void Shutdown()
        {
            if (_gossipPool.IsCreated) _gossipPool.Dispose();
        }

        public void StartGossip(string originator, string initialContent)
        {
            var entry = new GossipEntry
            {
                OriginatorId = new FixedString128Bytes(originator),
                Content = new FixedString512Bytes(initialContent),
                TruthValue = 1.0f,
                SpreadCount = 0,
                BirthTick = GameSession.CurrentTick
            };
            _gossipPool.Add(entry);
        }

        private void MutateGossip()
        {
            for (int i = 0; i < _gossipPool.Length; i++)
            {
                var entry = _gossipPool[i];
                if (entry.SpreadCount > 0)
                {
                    entry.TruthValue = Math.Max(0.0f, entry.TruthValue - _truthDecayRate);
                    entry.Content = DistortText(entry.Content, entry.SpreadCount);
                    _gossipPool[i] = entry;
                }
            }
        }

        private void SpreadGossip()
        {
            // Logic to spread gossip to nearby NPCs
        }

        private FixedString512Bytes DistortText(FixedString512Bytes original, int distortions)
        {
            // Simple distortion: add exaggeration markers
            return original;
        }
    }
}
