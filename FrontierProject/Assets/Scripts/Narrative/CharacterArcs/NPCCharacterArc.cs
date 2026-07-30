using System;
using Frontier.Core;
using Frontier.Core.Models;

namespace Frontier.Narrative.CharacterArcs
{
    public enum ArcType { Growth, Trauma, Redemption, Corruption, Betrayal, Sacrifice }
    public enum ArcPhase { Setup, RisingAction, Crisis, Climax, Resolution }

    public struct CharacterArcState
    {
        public Entity NpcId;
        public ArcType Type;
        public ArcPhase CurrentPhase;
        public float Progress; // 0.0 to 1.0
        public int TriggerCount;
        public FixedString512Bytes InternalMonologue;
    }

    public class NPCCharacterArc : IService
    {
        private NativeHashMap<Entity, CharacterArcState> _arcs;

        public int Priority => 8;

        public void Initialize()
        {
            _arcs = new NativeHashMap<Entity, CharacterArcState>(512, Allocator.Persistent);
        }

        public void Tick(float dt)
        {
            foreach (var key in _arcs.GetKeyArray(Allocator.Temp))
            {
                if (_arcs.TryGetValue(key, out var arc))
                {
                    UpdateArcProgress(ref arc);
                    _arcs[key] = arc;
                }
            }
        }

        public void Shutdown()
        {
            if (_arcs.IsCreated) _arcs.Dispose();
        }

        public void AssignArc(Entity npcId, ArcType type)
        {
            var state = new CharacterArcState
            {
                NpcId = npcId,
                Type = type,
                CurrentPhase = ArcPhase.Setup,
                Progress = 0.0f,
                TriggerCount = 0,
                InternalMonologue = new FixedString512Bytes("Beginning my journey...")
            };
            _arcs.Add(npcId, state);
        }

        private void UpdateArcProgress(ref CharacterArcState arc)
        {
            arc.Progress += 0.001f;
            if (arc.Progress > 0.25f && arc.CurrentPhase == ArcPhase.Setup)
                arc.CurrentPhase = ArcPhase.RisingAction;
            if (arc.Progress > 0.5f && arc.CurrentPhase == ArcPhase.RisingAction)
                arc.CurrentPhase = ArcPhase.Crisis;
            if (arc.Progress > 0.75f && arc.CurrentPhase == ArcPhase.Crisis)
                arc.CurrentPhase = ArcPhase.Climax;
            if (arc.Progress >= 1.0f && arc.CurrentPhase == ArcPhase.Climax)
                arc.CurrentPhase = ArcPhase.Resolution;
        }
    }
}
