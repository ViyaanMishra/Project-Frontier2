using System;
using Unity.Collections;
using Unity.Entities;
using Frontier.Core;
using Frontier.Simulation;

namespace Frontier.Narrative.StoryGraph
{
    /// <summary>
    /// Manages dialogue presentation, timing, and player choices.
    /// Handles subtitle rendering, voice-over synchronization, and response selection.
    /// </summary>
    public class DialogueManager : IService
    {
        private NativeQueue<DialogueLine> _dialogueQueue;
        private NativeHashMap<FixedString64Bytes, DialogueTemplate> _templates;
        private DialogueState _currentState;
        private float _currentTimer;
        
        public int Priority => 15; // High priority for responsive UI

        public void Initialize()
        {
            _dialogueQueue = new NativeQueue<DialogueLine>(Allocator.Persistent);
            _templates = new NativeHashMap<FixedString64Bytes, DialogueTemplate>(64, Allocator.Persistent);
            _currentState = DialogueState.Idle;
            
            EventBus.Subscribe<StoryNodeExecutedEvent>(OnStoryNodeExecuted);
        }

        public void Tick(double deltaTime)
        {
            if (_currentState != DialogueState.Playing)
                return;

            _currentTimer -= (float)deltaTime;

            if (_currentTimer <= 0 && _dialogueQueue.Count > 0)
            {
                DisplayNextLine();
            }
            else if (_currentTimer <= 0 && _dialogueQueue.Count == 0)
            {
                EndDialogue();
            }
        }

        public void Shutdown()
        {
            if (_dialogueQueue.IsCreated) _dialogueQueue.Dispose();
            if (_templates.IsCreated) _templates.Dispose();
        }

        /// <summary>
        /// Queues a dialogue sequence for playback.
        /// </summary>
        public void StartDialogue(FixedString64Bytes dialogueId, FixedString64Bytes speakerId)
        {
            if (!_templates.TryGetValue(dialogueId, out var template))
            {
                UnityEngine.Debug.LogError($"Dialogue template {dialogueId} not found!");
                return;
            }

            _currentState = DialogueState.Playing;
            
            foreach (var line in template.Lines)
            {
                var processedLine = new DialogueLine
                {
                    SpeakerId = speakerId,
                    Text = line.Text,
                    DurationSeconds = CalculateReadingTime(line.Text),
                    Emotion = line.Emotion,
                    VoiceOverId = line.VoiceOverId
                };
                _dialogueQueue.Enqueue(processedLine);
            }

            DisplayNextLine();
        }

        /// <summary>
        /// Presents player with dialogue choices.
        /// </summary>
        public void PresentChoices(NativeArray<DialogueChoice> choices)
        {
            _currentState = DialogueState.WaitingForChoice;
            EventBus.Publish(new DialogueChoicesPresentedEvent { Choices = choices });
        }

        /// <summary>
        /// Player selects a dialogue choice.
        /// </summary>
        public void SelectChoice(int choiceIndex)
        {
            EventBus.Publish(new DialogueChoiceSelectedEvent { ChoiceIndex = choiceIndex });
            _currentState = DialogueState.Playing;
        }

        /// <summary>
        /// Skips the current dialogue line.
        /// </summary>
        public void SkipLine()
        {
            if (_dialogueQueue.Count > 0)
            {
                _dialogueQueue.Dequeue();
                if (_dialogueQueue.Count > 0)
                {
                    DisplayNextLine();
                }
                else
                {
                    EndDialogue();
                }
            }
        }

        /// <summary>
        /// Registers a dialogue template for reuse.
        /// </summary>
        public void RegisterTemplate(FixedString64Bytes id, DialogueTemplate template)
        {
            _templates[id] = template;
        }

        private void DisplayNextLine()
        {
            if (_dialogueQueue.Count == 0)
                return;

            var line = _dialogueQueue.Dequeue();
            _currentTimer = line.DurationSeconds;

            EventBus.Publish(new DialogueLineDisplayedEvent
            {
                SpeakerId = line.SpeakerId,
                Text = line.Text,
                Emotion = line.Emotion,
                VoiceOverId = line.VoiceOverId
            });
        }

        private void EndDialogue()
        {
            _currentState = DialogueState.Idle;
            EventBus.Publish(new DialogueEndedEvent());
        }

        private float CalculateReadingTime(string text)
        {
            // Average reading speed: ~200 words per minute
            int wordCount = text.Split(' ').Length;
            return (wordCount / 200f) * 60f;
        }

        private void OnStoryNodeExecuted(StoryNodeExecutedEvent evt)
        {
            if (evt.Type == NodeType.Dialogue)
            {
                var engine = ServiceRegistry.Get<StoryGraphEngine>();
                var node = engine.GetNode(evt.NodeId);
                
                // Parse dialogue from node content
                StartDialogue(evt.NodeId, new FixedString64Bytes("narrator"));
            }
        }
    }

    [Serializable]
    public struct DialogueTemplate
    {
        public FixedString64Bytes Id;
        public NativeArray<DialogueLineData> Lines;
    }

    [Serializable]
    public struct DialogueLineData
    {
        public FixedString512Bytes Text;
        public FixedString64Bytes Emotion;
        public FixedString64Bytes VoiceOverId;
    }

    public struct DialogueLine
    {
        public FixedString64Bytes SpeakerId;
        public FixedString512Bytes Text;
        public float DurationSeconds;
        public FixedString64Bytes Emotion;
        public FixedString64Bytes VoiceOverId;
    }

    [Serializable]
    public struct DialogueChoice
    {
        public FixedString128Bytes Label;
        public FixedString64Bytes TargetNodeId;
        public int RequirementConditionId; // Empty = always available
    }

    public enum DialogueState
    {
        Idle,
        Playing,
        WaitingForChoice
    }

    #region Events
    public struct DialogueLineDisplayedEvent : IEvent
    {
        public FixedString64Bytes SpeakerId;
        public FixedString512Bytes Text;
        public FixedString64Bytes Emotion;
        public FixedString64Bytes VoiceOverId;
    }

    public struct DialogueChoicesPresentedEvent : IEvent
    {
        public NativeArray<DialogueChoice> Choices;
    }

    public struct DialogueChoiceSelectedEvent : IEvent
    {
        public int ChoiceIndex;
    }

    public struct DialogueEndedEvent : IEvent { }
    #endregion
}
