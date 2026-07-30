using System;
using Unity.Collections;
using Frontier.Core;

namespace Frontier.Narrative.Emergent
{
    /// <summary>
    /// Simulates the spread of information through NPC social networks.
    /// Facts mutate as they pass between NPCs based on speaker traits (Liar, Drunk, Honest).
    /// Creates a "telephone game" effect generating unique local legends and misunderstandings.
    /// </summary>
    public class GossipSystem : IService
    {
        private NativeHashMap<ulong, GossipNode> _gossipNetwork;
        private NativeList<GossipTransmission> _transmissionQueue;
        
        public int Priority => 60;
        public float SpreadRate = 0.5f; // Transmissions per second
        public float MutationRate = 0.1f; // Chance of fact distortion

        public void Initialize()
        {
            _gossipNetwork = new NativeHashMap<ulong, GossipNode>(2048, Allocator.Persistent);
            _transmissionQueue = new NativeList<GossipTransmission>(Allocator.Persistent);
            
            EventBus.Subscribe<NarrativeEventOccurred>(OnEventCreated);
        }

        public void Tick(float dt)
        {
            // Process transmissions
            for (int i = 0; i < _transmissionQueue.Length; i++)
            {
                var transmission = _transmissionQueue[i];
                
                if (transmission.Timer <= 0)
                {
                    TransmitGossip(transmission);
                    _transmissionQueue.RemoveAtSwapBack(i);
                    i--;
                }
                else
                {
                    transmission.Timer -= dt;
                    _transmissionQueue[i] = transmission;
                }
            }
            
            // Random spontaneous transmissions
            if (UnityEngine.Random.value < SpreadRate * dt)
            {
                SpontaneousTransmission();
            }
        }

        public void Shutdown()
        {
            if (_gossipNetwork.IsCreated) _gossipNetwork.Dispose();
            if (_transmissionQueue.IsCreated) _transmissionQueue.Dispose();
            EventBus.Unsubscribe<NarrativeEventOccurred>(OnEventCreated);
        }

        public void SeedGossip(ulong originNPC, FixedString128Bytes fact, float truthfulness = 1.0f)
        {
            var node = new GossipNode
            {
                Fact = fact,
                Originator = originNPC,
                Truthfulness = truthfulness,
                SpreadCount = 0,
                Timestamp = MasterClock.ElapsedSeconds
            };
            
            ulong gossipId = HashUtility.Hash64(fact.ToString() + originNPC);
            if (!_gossipNetwork.ContainsKey(gossipId))
            {
                _gossipNetwork.Add(gossipId, node);
                QueueTransmission(originNPC, gossipId);
            }
        }

        private void TransmitGossip(GossipTransmission transmission)
        {
            if (!_gossipNetwork.ContainsKey(transmission.GossipId)) return;
            
            var gossip = _gossipNetwork[transmission.GossipId];
            var speaker = transmission.SpeakerId;
            var listener = transmission.ListenerId;
            
            // Mutate fact based on speaker traits
            float mutation = UnityEngine.Random.Range(0f, 1f);
            if (mutation < MutationRate)
            {
                gossip.Fact = MutateFact(gossip.Fact, speaker);
                gossip.Truthfulness *= 0.8f; // Reduce truthfulness with each mutation
            }
            
            gossip.SpreadCount++;
            _gossipNetwork[transmission.GossipId] = gossip;
            
            // Listener now knows the gossip and may spread it
            QueueTransmission(listener, transmission.GossipId);
            
            // Publish event for quest/dialogue updates
            EventBus.Publish(new GossipSpread 
            { 
                GossipId = transmission.GossipId,
                Speaker = speaker,
                Listener = listener,
                CurrentTruthfulness = gossip.Truthfulness
            });
        }

        private FixedString128Bytes MutateFact(FixedString128Bytes original, ulong speakerId)
        {
            // Simplified mutation logic
            // In full implementation: exaggerate numbers, swap entities, change locations
            return original; 
        }

        private void QueueTransmission(ulong npcId, ulong gossipId)
        {
            // Find nearby NPCs to transmit to
            // For now, queue a delayed transmission
            _transmissionQueue.Add(new GossipTransmission
            {
                SpeakerId = npcId,
                ListenerId = npcId, // Placeholder
                GossipId = gossipId,
                Timer = UnityEngine.Random.Range(10f, 60f)
            });
        }

        private void SpontaneousTransmission()
        {
            // Random NPC starts talking about random gossip they know
        }

        private void OnEventCreated(NarrativeEventOccurred evt)
        {
            // High intensity events automatically seed gossip
            if (evt.Payload.Intensity > 0.7f)
            {
                // Find NPCs near the event and seed them with knowledge
                SeedGossip(evt.Payload.SourceId, evt.Payload.Tags[0], 1.0f);
            }
        }
    }

    public struct GossipNode
    {
        public FixedString128Bytes Fact;
        public ulong Originator;
        public float Truthfulness;
        public int SpreadCount;
        public double Timestamp;
    }

    public struct GossipTransmission
    {
        public ulong SpeakerId;
        public ulong ListenerId;
        public ulong GossipId;
        public float Timer;
    }

    public struct GossipSpread
    {
        public ulong GossipId;
        public ulong Speaker;
        public ulong Listener;
        public float CurrentTruthfulness;
    }
}
