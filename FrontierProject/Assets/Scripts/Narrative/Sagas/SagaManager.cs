using System;
using Unity.Collections;
using Frontier.Core;

namespace Frontier.Narrative.Sagas
{
    /// <summary>
    /// Manages long-form narrative arcs composed of Acts and Beats.
    /// Dynamically adjusts tension and pacing based on player performance.
    /// </summary>
    public class SagaManager : IService
    {
        private NativeHashMap<int, SagaDefinition> _sagas;
        private NativeList<int> _activeSagaIds;
        private int _currentSagaId;

        public int Priority => 8;

        public void Initialize()
        {
            _sagas = new NativeHashMap<int, SagaDefinition>(64, Allocator.Persistent);
            _activeSagaIds = new NativeList<int>(Allocator.Persistent);
            _currentSagaId = -1;
        }

        public void Tick(float dt)
        {
            // Update active sagas
            for (int i = 0; i < _activeSagaIds.Length; i++)
            {
                int sagaId = _activeSagaIds[i];
                if (_sagas.TryGetValue(sagaId, out SagaDefinition saga))
                {
                    saga.ElapsedTime += dt;
                    CheckBeatCompletion(sagaId, ref saga);
                    _sagas[sagaId] = saga;
                }
            }
        }

        public void Shutdown()
        {
            if (_sagas.IsCreated) _sagas.Dispose();
            if (_activeSagaIds.IsCreated) _activeSagaIds.Dispose();
        }

        public void StartSaga(SagaDefinition saga)
        {
            if (!_sagas.ContainsKey(saga.Id))
            {
                _sagas.Add(saga.Id, saga);
                _activeSagaIds.Add(saga.Id);
                _currentSagaId = saga.Id;
                EventBus.Publish(new SagaStarted { SagaId = saga.Id });
            }
        }

        private void CheckBeatCompletion(int sagaId, ref SagaDefinition saga)
        {
            if (saga.CurrentBeatIndex < saga.Beats.Length)
            {
                ref var beat = ref saga.Beats[saga.CurrentBeatIndex];
                beat.ElapsedTime += saga.ElapsedTime - (beat.ElapsedTime > 0 ? saga.ElapsedTime - beat.ElapsedTime : 0); // Simplified logic

                if (beat.ElapsedTime >= beat.Duration || beat.ObjectivesCompleted >= beat.TotalObjectives)
                {
                    saga.CurrentBeatIndex++;
                    EventBus.Publish(new SagaBeatCompleted { SagaId = sagaId, BeatIndex = saga.CurrentBeatIndex - 1 });
                    
                    if (saga.CurrentBeatIndex >= saga.Beats.Length)
                    {
                        CompleteSaga(sagaId);
                    }
                }
            }
        }

        private void CompleteSaga(int sagaId)
        {
            _activeSagaIds.RemoveAtSwap(_activeSagaIds.IndexOf(sagaId));
            EventBus.Publish(new SagaCompleted { SagaId = sagaId });
        }

        public void AddObjective(int sagaId, int beatIndex)
        {
            if (_sagas.TryGetValue(sagaId, out SagaDefinition saga))
            {
                if (beatIndex < saga.Beats.Length)
                {
                    var beat = saga.Beats[beatIndex];
                    beat.ObjectivesCompleted++;
                    saga.Beats[beatIndex] = beat;
                    _sagas[sagaId] = saga;
                }
            }
        }
    }

    public struct SagaDefinition
    {
        public int Id;
        public FixedString128Bytes Title;
        public SagaTheme Theme;
        public NativeArray<SagaAct> Acts;
        public NativeArray<SagaBeat> Beats;
        public int CurrentBeatIndex;
        public float ElapsedTime;
        public float TensionCurve;
    }

    public struct SagaAct
    {
        public FixedString128Bytes Title;
        public int StartBeat;
        public int EndBeat;
    }

    public struct SagaBeat
    {
        public FixedString256Bytes Description;
        public float Duration;
        public float ElapsedTime;
        public int TotalObjectives;
        public int ObjectivesCompleted;
    }

    public enum SagaTheme
    {
        Survival,
        Expansion,
        Revenge,
        Discovery,
        Corruption,
        Redemption
    }

    public struct SagaStarted { public int SagaId; }
    public struct SagaBeatCompleted { public int SagaId; public int BeatIndex; }
    public struct SagaCompleted { public int SagaId; }
}
