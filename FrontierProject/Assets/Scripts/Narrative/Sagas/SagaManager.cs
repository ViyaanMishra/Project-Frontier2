using System;
using Unity.Collections;
using Frontier.Core;

namespace Frontier.Narrative.Sagas
{
    /// <summary>
    /// Manages long-form narrative arcs that span multiple gameplay sessions.
    /// Sagas are composed of multiple chapters, each containing story nodes.
    /// Tracks player progress through epic narrative sequences.
    /// </summary>
    public class SagaManager : IService
    {
        private NativeHashMap<FixedString64Bytes, SagaDefinition> _sagaRegistry;
        private NativeHashMap<FixedString64Bytes, SagaProgress> _playerProgress;
        private FixedString64Bytes _activeSagaId;
        
        public int Priority => 8;

        public void Initialize()
        {
            _sagaRegistry = new NativeHashMap<FixedString64Bytes, SagaDefinition>(32, Allocator.Persistent);
            _playerProgress = new NativeHashMap<FixedString64Bytes, SagaProgress>(16, Allocator.Persistent);
            _activeSagaId = new FixedString64Bytes();
        }

        public void Tick(double deltaTime)
        {
            // Check for saga progression triggers
            if (!_activeSagaId.IsEmpty())
            {
                CheckChapterCompletion();
            }
        }

        public void Shutdown()
        {
            if (_sagaRegistry.IsCreated) _sagaRegistry.Dispose();
            if (_playerProgress.IsCreated) _playerProgress.Dispose();
        }

        /// <summary>
        /// Registers a new saga definition.
        /// </summary>
        public void RegisterSaga(SagaDefinition saga)
        {
            _sagaRegistry[saga.Id] = saga;
        }

        /// <summary>
        /// Begins a new saga for the player.
        /// </summary>
        public bool StartSaga(FixedString64Bytes sagaId)
        {
            if (!_sagaRegistry.TryGetValue(sagaId, out var saga))
            {
                UnityEngine.Debug.LogError($"Saga {sagaId} not found!");
                return false;
            }

            if (IsSagaCompleted(sagaId))
            {
                UnityEngine.Debug.LogWarning($"Saga {sagaId} already completed!");
                return false;
            }

            var progress = new SagaProgress
            {
                SagaId = sagaId,
                CurrentChapterIndex = 0,
                CompletedChapters = new NativeList<int>(Allocator.Persistent),
                StartTimeTicks = MasterClock.Instance.TotalTicks,
                IsActive = true
            };

            _playerProgress[sagaId] = progress;
            _activeSagaId = sagaId;

            EventBus.Publish(new SagaStartedEvent { SagaId = sagaId });

            // Start first chapter
            StartChapter(saga.Chapters[0].Id);

            return true;
        }

        /// <summary>
        /// Advances to the next chapter in the active saga.
        /// </summary>
        public void AdvanceChapter()
        {
            if (_activeSagaId.IsEmpty() || !_playerProgress.TryGetValue(_activeSagaId, out var progress))
                return;

            if (!_sagaRegistry.TryGetValue(_activeSagaId, out var saga))
                return;

            progress.CurrentChapterIndex++;
            
            if (progress.CurrentChapterIndex >= saga.Chapters.Length)
            {
                CompleteSaga(_activeSagaId);
            }
            else
            {
                var nextChapter = saga.Chapters[progress.CurrentChapterIndex];
                StartChapter(nextChapter.Id);
            }

            _playerProgress[_activeSagaId] = progress;
        }

        /// <summary>
        /// Marks the current saga as completed.
        /// </summary>
        private void CompleteSaga(FixedString64Bytes sagaId)
        {
            if (_playerProgress.TryGetValue(sagaId, out var progress))
            {
                progress.IsActive = false;
                progress.CompletionTimeTicks = MasterClock.Instance.TotalTicks;
                _playerProgress[sagaId] = progress;

                EventBus.Publish(new SagaCompletedEvent { SagaId = sagaId });

                _activeSagaId = new FixedString64Bytes();
            }
        }

        /// <summary>
        /// Starts a specific chapter within a saga.
        /// </summary>
        private void StartChapter(FixedString64Bytes chapterId)
        {
            EventBus.Publish(new ChapterStartedEvent { ChapterId = chapterId });
            
            // Trigger the opening node of this chapter
            var engine = ServiceRegistry.Get<StoryGraphEngine>();
            // Would need to lookup the starting node for this chapter
        }

        /// <summary>
        /// Checks if the current chapter's objectives are complete.
        /// </summary>
        private void CheckChapterCompletion()
        {
            if (!_activeSagaId.IsEmpty() && _playerProgress.TryGetValue(_activeSagaId, out var progress))
            {
                if (_sagaRegistry.TryGetValue(_activeSagaId, out var saga))
                {
                    var currentChapter = saga.Chapters[progress.CurrentChapterIndex];
                    
                    bool allComplete = true;
                    for (int i = 0; i < currentChapter.RequiredNodes.Length; i++)
                    {
                        var node = ServiceRegistry.Get<StoryGraphEngine>().GetNode(currentChapter.RequiredNodes[i]);
                        if (!node.IsCompleted)
                        {
                            allComplete = false;
                            break;
                        }
                    }

                    if (allComplete)
                    {
                        progress.CompletedChapters.Add(progress.CurrentChapterIndex);
                        _playerProgress[_activeSagaId] = progress;
                        
                        EventBus.Publish(new ChapterCompletedEvent 
                        { 
                            SagaId = _activeSagaId, 
                            ChapterIndex = progress.CurrentChapterIndex 
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Returns whether a saga is completed.
        /// </summary>
        public bool IsSagaCompleted(FixedString64Bytes sagaId)
        {
            if (!_playerProgress.TryGetValue(sagaId, out var progress))
                return false;
            
            return !progress.IsActive && progress.CompletionTimeTicks > 0;
        }

        /// <summary>
        /// Gets the current progress percentage for a saga.
        /// </summary>
        public float GetSagaProgress(FixedString64Bytes sagaId)
        {
            if (!_sagaRegistry.TryGetValue(sagaId, out var saga))
                return 0f;
            
            if (!_playerProgress.TryGetValue(sagaId, out var progress))
                return 0f;

            return (float)(progress.CurrentChapterIndex + 1) / saga.Chapters.Length;
        }

        /// <summary>
        /// Saves saga state for persistence.
        /// </summary>
        public void SaveState(NativeBuffer<byte> buffer)
        {
            // Serialization logic
        }

        /// <summary>
        /// Loads saga state from persistence.
        /// </summary>
        public void LoadState(NativeBuffer<byte> buffer)
        {
            // Deserialization logic
        }
    }

    [Serializable]
    public struct SagaDefinition
    {
        public FixedString64Bytes Id;
        public FixedString128Bytes Title;
        public FixedString512Bytes Description;
        public NativeArray<SagaChapter> Chapters;
        public SagaTier Tier;
        public bool IsMainStory;
    }

    [Serializable]
    public struct SagaChapter
    {
        public FixedString64Bytes Id;
        public FixedString128Bytes Title;
        public FixedString512Bytes Description;
        public NativeArray<FixedString64Bytes> RequiredNodes;
        public NativeArray<FixedString64Bytes> OptionalNodes;
    }

    [Serializable]
    public struct SagaProgress
    {
        public FixedString64Bytes SagaId;
        public int CurrentChapterIndex;
        public NativeList<int> CompletedChapters;
        public double StartTimeTicks;
        public double CompletionTimeTicks;
        public bool IsActive;
    }

    public enum SagaTier
    {
        Minor,      // Side stories, 1-2 chapters
        Major,      // Significant arcs, 3-5 chapters
        Epic,       // Main storyline, 6+ chapters
        Legendary   // Multi-game spanning narratives
    }

    #region Events
    public struct SagaStartedEvent : IEvent
    {
        public FixedString64Bytes SagaId;
    }

    public struct SagaCompletedEvent : IEvent
    {
        public FixedString64Bytes SagaId;
    }

    public struct ChapterStartedEvent : IEvent
    {
        public FixedString64Bytes ChapterId;
    }

    public struct ChapterCompletedEvent : IEvent
    {
        public FixedString64Bytes SagaId;
        public int ChapterIndex;
    }
    #endregion
}
