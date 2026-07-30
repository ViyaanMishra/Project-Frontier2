using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

namespace FrontierProject.Narrative.Cinematics
{
    /// <summary>
    /// Advanced cinematic director system for narrative sequences.
    /// Handles camera choreography, character blocking, timing, 
    /// and seamless integration of gameplay and cutscene elements.
    /// </summary>

    [Serializable]
    public struct CinematicSequence
    {
        public FixedString64Bytes SequenceID;
        public FixedString128Bytes DisplayName;
        
        public DynamicBuffer<CinematicShot> Shots;
        public DynamicBuffer<CinematicEvent> Events;
        
        // Timing
        public float TotalDuration;
        public float CurrentTime;
        public PlaybackState State;
        
        // Configuration
        public bool IsSkippable;
        public float SkipDelay;
        public bool CanPause;
        public bool AffectsGameplay;
        
        // Integration
        public FixedString64Bytes PreSequenceState;
        public FixedString64Bytes PostSequenceState;
        public DynamicBuffer<StateTransition> StateTransitions;
        
        // Quality settings
        public LODQuality ForcedLOD;
        public bool DisablePlayerInput;
        public bool HideHUD;
    }

    [Serializable]
    public struct CinematicShot
    {
        public FixedString64Bytes ShotID;
        public FixedString64Bytes CameraRigID;
        
        // Timing
        public float StartTime;
        public float Duration;
        public EaseType EaseIn;
        public EaseType EaseOut;
        
        // Camera data
        public CameraDefinition CameraDef;
        public DynamicBuffer<CameraKeyframe> Keyframes;
        
        // Focus
        public FixedString64Bytes FocusTargetID;
        public FocusType FocusMode;
        public float FocusDistance;
        public float DepthOfFieldStrength;
        
        // Composition
        public RuleOfThirdsGrid CompositionGrid;
        public float TargetFrameRate;
        
        // Transition to next shot
        public TransitionType OutTransition;
        public float TransitionDuration;
    }

    [Serializable]
    public struct CameraDefinition
    {
        public float3 Position;
        public quaternion Rotation;
        public float FieldOfView;
        public float NearClip;
        public float FarClip;
        
        // Movement
        public CameraMovementType MovementType;
        public float3 MoveTarget;
        public float MoveSpeed;
        
        // Rotation
        public CameraRotationType RotationType;
        public float3 LookAtTarget;
        public float RotationSpeed;
        
        // Effects
        public float ShakeIntensity;
        public float ShakeFrequency;
        public float ChromaticAberration;
        public float VignetteStrength;
    }

    [Serializable]
    public struct CameraKeyframe
    {
        public float Time;
        public float3 Position;
        public quaternion Rotation;
        public float FOV;
        
        public EaseType PositionEase;
        public EaseType RotationEase;
        public EaseType FOVEase;
    }

    [Serializable]
    public enum CameraMovementType
    {
        Static,
        Linear,
        Spline,
        Dolly,
        Crane,
        Handheld,
        Tracking,
        Arc
    }

    [Serializable]
    public enum CameraRotationType
    {
        Static,
        LookAt,
        Pan,
        Tilt,
        Roll,
        Free
    }

    [Serializable]
    public struct CinematicEvent
    {
        public FixedString64Bytes EventID;
        public FixedString64Bytes EventType;
        
        public float TriggerTime;
        public float Duration;
        
        // Event data
        public FixedString64Bytes TargetEntityRef;
        public FixedString512Bytes Parameters;
        
        // Categories
        public EventCategory Category;
        public int Priority;
        
        // Execution
        public bool HasExecuted;
        public double ExecutionTime;
    }

    [Serializable]
    public enum EventCategory
    {
        Animation,
        Dialogue,
        Audio,
        VFX,
        Lighting,
        Gameplay,
        Camera,
        UI,
        Environment
    }

    [Serializable]
    public struct CharacterBlocking
    {
        public FixedString64Bytes CharacterID;
        public Entity CharacterEntity;
        
        public DynamicBuffer<BlockPose> Poses;
        public DynamicBuffer<BlockMovement> Movements;
        public DynamicBuffer<BlockGesture> Gestures;
        
        public FixedString64Bytes CurrentAnimationState;
        public float AnimationBlendWeight;
        
        public bool IsSpeaking;
        public FixedString64Bytes CurrentDialogueLine;
        
        public float3 EyeTarget;
        public float HeadLookWeight;
    }

    [Serializable]
    public struct BlockPose
    {
        public FixedString64Bytes PoseID;
        public float StartTime;
        public float Duration;
        public float BlendIn;
        public float BlendOut;
        
        public HumanPose HumanPoseData;
        public FacialExpression Expression;
    }

    [Serializable]
    public struct BlockMovement
    {
        public FixedString64Bytes MovementID;
        public float3 StartPosition;
        public float3 EndPosition;
        public float StartTime;
        public float Duration;
        public MovementStyle Style;
        public EaseType Ease;
    }

    [Serializable]
    public struct BlockGesture
    {
        public FixedString64Bytes GestureID;
        public FixedString64Bytes AnimationClip;
        public float StartTime;
        public float Duration;
        public float Weight;
        public GestureType Type;
    }

    [Serializable]
    public enum MovementStyle
    {
        Walk,
        Run,
        Sneak,
        Crouch,
        Dramatic,
        Casual,
        Urgent,
        Reluctant
    }

    [Serializable]
    public enum GestureType
    {
        Hand,
        Arm,
        Head,
        FullBody,
        Facial,
        Eye
    }

    [Serializable]
    public struct DialogueSync
    {
        public FixedString64Bytes LineID;
        public FixedString64Bytes SpeakerID;
        
        public FixedString512Bytes Text;
        public FixedString64Bytes AudioClipID;
        
        public float StartTime;
        public float Duration;
        public float PhonemeStartTime;
        
        public DynamicBuffer<PhonemeTiming> Phonemes;
        public DynamicBuffer<EmphasisMarker> Emphases;
        
        public LipSyncMethod SyncMethod;
        public float SyncAccuracy;
    }

    [Serializable]
    public struct PhonemeTiming
    {
        public FixedString16Bytes Phoneme;
        public float StartTime;
        public float Duration;
        public float Intensity;
    }

    [Serializable]
    public struct EmphasisMarker
    {
        public float Time;
        public EmphasisType Type;
        public float Strength;
    }

    [Serializable]
    public enum EmphasisType
    {
        Loud,
        Soft,
        Pause,
        PitchUp,
        PitchDown,
        Tremble,
        Whisper,
        Shout
    }

    [Serializable]
    public enum LipSyncMethod
    {
        Simple,
        Phoneme,
        Viseme,
        MLBased,
        Manual
    }

    [Serializable]
    public enum PlaybackState
    {
        NotStarted,
        PreRoll,
        Playing,
        Paused,
        Skipping,
        PostRoll,
        Completed,
        Interrupted
    }

    [Serializable]
    public enum EaseType
    {
        Linear,
        InQuad,
        OutQuad,
        InOutQuad,
        InCubic,
        OutCubic,
        InOutCubic,
        InQuart,
        OutQuart,
        InOutQuart,
        InQuint,
        OutQuint,
        InOutQuint,
        InSine,
        OutSine,
        InOutSine,
        InExpo,
        OutExpo,
        InOutExpo,
        InCirc,
        OutCirc,
        InOutCirc,
        Elastic,
        Bounce
    }

    [Serializable]
    public enum TransitionType
    {
        Cut,
        Fade,
        Dissolve,
        Wipe,
        MatchCut,
        JumpCut,
        Morph,
        Iris,
        Blur
    }

    [Serializable]
    public enum FocusType
    {
        None,
        Single,
        Multiple,
        Area,
        Follow,
        Predictive
    }

    [Serializable]
    public enum LODQuality
    {
        Cinematic,
        Ultra,
        High,
        Medium,
        Low
    }

    [Serializable]
    public struct RuleOfThirdsGrid
    {
        public bool Enabled;
        public float2 PrimaryIntersection;
        public float2 SecondaryIntersection;
        public AlignmentType Alignment;
    }

    [Serializable]
    public enum AlignmentType
    {
        Center,
        LeftThird,
        RightThird,
        TopThird,
        BottomThird,
        Custom
    }

    [Serializable]
    public struct StateTransition
    {
        public FixedString64Bytes FromState;
        public FixedString64Bytes ToState;
        public float TriggerTime;
        public TransitionMethod Method;
        public float BlendDuration;
    }

    [Serializable]
    public enum TransitionMethod
    {
        Instant,
        Blend,
        CrossFade,
        Conditional,
        Trigger
    }

    public struct CinematicComponent : IComponentData
    {
        public Entity OwnerEntity;
        public CinematicSequence ActiveSequence;
        
        public DynamicBuffer<ActiveCharacterBlocking> CharacterBlockings;
        public DynamicBuffer<PendingCinematicEvent> PendingEvents;
        
        public CinematicMetrics Metrics;
        public DirectorState DirectorState;
    }

    [Serializable]
    public struct ActiveCharacterBlocking
    {
        public CharacterBlocking Blocking;
        public int CurrentPoseIndex;
        public int CurrentMovementIndex;
        public int CurrentGestureIndex;
    }

    [Serializable]
    public struct PendingCinematicEvent
    {
        public CinematicEvent Event;
        public bool IsReady;
    }

    [Serializable]
    public struct CinematicMetrics
    {
        public int SequencesPlayed;
        public int ShotsCompleted;
        public int EventsTriggered;
        public int TimesSkipped;
        public int Interruptions;
        
        public float AverageWatchTime;
        public float CompletionRate;
        public float SkipRate;
        
        public float CameraSmoothnessScore;
        public float TimingPrecisionScore;
        public float EmotionalImpactEstimate;
    }

    [Serializable]
    public struct DirectorState
    {
        public FixedString64Bytes CurrentPhase;
        public float PhaseProgress;
        
        public Entity ActiveCameraRig;
        public Entity FocusTarget;
        
        public bool IsRecording;
        public bool IsPreviewing;
        
        public float GlobalTimeScale;
        public float DesiredFrameRate;
    }

    public class CinematicDirectorSystem : SystemBase
    {
        private NativeHashMap<FixedString64Bytes, CinematicSequence> _sequenceRegistry;
        private NativeHashMap<FixedString64Bytes, CameraDefinition> _cameraRigRegistry;
        
        protected override void OnCreate()
        {
            _sequenceRegistry = new NativeHashMap<FixedString64Bytes, CinematicSequence>(200, Allocator.Persistent);
            _cameraRigRegistry = new NativeHashMap<FixedString64Bytes, CameraDefinition>(100, Allocator.Persistent);
        }
        
        protected override void OnDestroy()
        {
            _sequenceRegistry.Dispose();
            _cameraRigRegistry.Dispose();
        }
        
        protected override void OnUpdate()
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            var currentTime = SystemAPI.Time.ElapsedTime;
            
            Entities
                .WithAll<CinematicComponent>()
                .ForEach((ref CinematicComponent cineComp) =>
                {
                    if (cineComp.ActiveSequence.State == PlaybackState.Playing)
                    {
                        // Update sequence time
                        cineComp.ActiveSequence.CurrentTime += deltaTime * cineComp.DirectorState.GlobalTimeScale;
                        
                        // Check for shot transitions
                        UpdateCurrentShot(ref cineComp, deltaTime);
                        
                        // Process pending events
                        ProcessPendingEvents(ref cineComp);
                        
                        // Update character blocking
                        UpdateCharacterBlockings(ref cineComp, deltaTime);
                        
                        // Check for sequence completion
                        if (cineComp.ActiveSequence.CurrentTime >= cineComp.ActiveSequence.TotalDuration)
                        {
                            cineComp.ActiveSequence.State = PlaybackState.PostRoll;
                            CompleteSequence(ref cineComp);
                        }
                    }
                    else if (cineComp.ActiveSequence.State == PlaybackState.PreRoll)
                    {
                        // Initialize sequence
                        InitializeSequence(ref cineComp);
                    }
                    
                }).WithoutBurst().Run();
        }
        
        private void UpdateCurrentShot(ref CinematicComponent cineComp, float deltaTime)
        {
            ref var sequence = ref cineComp.ActiveSequence;
            
            if (sequence.Shots.Length == 0) return;
            
            // Find current shot based on time
            int currentShotIndex = -1;
            for (int i = 0; i < sequence.Shots.Length; i++)
            {
                var shot = sequence.Shots[i];
                if (sequence.CurrentTime >= shot.StartTime && 
                    sequence.CurrentTime < shot.StartTime + shot.Duration)
                {
                    currentShotIndex = i;
                    break;
                }
            }
            
            // Would update camera rig and focus based on current shot
        }
        
        private void ProcessPendingEvents(ref CinematicComponent cineComp)
        {
            for (int i = cineComp.PendingEvents.Length - 1; i >= 0; i--)
            {
                var pending = cineComp.PendingEvents[i];
                
                if (cineComp.ActiveSequence.CurrentTime >= pending.Event.TriggerTime && !pending.Event.HasExecuted)
                {
                    ExecuteCinematicEvent(pending.Event, ref cineComp);
                    cineComp.PendingEvents.RemoveAt(i);
                }
            }
        }
        
        private void ExecuteCinematicEvent(CinematicEvent evt, ref CinematicComponent cineComp)
        {
            evt.HasExecuted = true;
            evt.ExecutionTime = SystemAPI.Time.ElapsedTime;
            
            cineComp.Metrics.EventsTriggered++;
            
            // Event execution would integrate with appropriate systems:
            // - Animation system for character animations
            // - Audio system for sound effects/music
            // - VFX system for visual effects
            // - Lighting system for lighting changes
            // - Dialogue system for voice lines
        }
        
        private void UpdateCharacterBlockings(ref CinematicComponent cineComp, float deltaTime)
        {
            for (int i = 0; i < cineComp.CharacterBlockings.Length; i++)
            {
                var blocking = cineComp.CharacterBlockings[i];
                
                // Update pose blending
                // Update movement interpolation
                // Trigger gestures at appropriate times
                
                cineComp.CharacterBlockings[i] = blocking;
            }
        }
        
        private void InitializeSequence(ref CinematicComponent cineComp)
        {
            cineComp.ActiveSequence.State = PlaybackState.Playing;
            cineComp.ActiveSequence.CurrentTime = 0;
            
            // Queue all events
            for (int i = 0; i < cineComp.ActiveSequence.Events.Length; i++)
            {
                var pending = new PendingCinematicEvent
                {
                    Event = cineComp.ActiveSequence.Events[i],
                    IsReady = false
                };
                cineComp.PendingEvents.Add(pending);
            }
            
            // Set up character blockings
            // Configure camera rigs
            
            cineComp.Metrics.SequencesPlayed++;
        }
        
        private void CompleteSequence(ref CinematicComponent cineComp)
        {
            // Apply post-sequence state transitions
            for (int i = 0; i < cineComp.ActiveSequence.StateTransitions.Length; i++)
            {
                var transition = cineComp.ActiveSequence.StateTransitions[i];
                // Apply state transition
            }
            
            cineComp.ActiveSequence.State = PlaybackState.Completed;
            cineComp.Metrics.CompletionRate = CalculateCompletionRate(cineComp);
        }
        
        private float CalculateCompletionRate(CinematicComponent cineComp)
        {
            if (cineComp.Metrics.SequencesPlayed == 0) return 0f;
            return (float)cineComp.Metrics.SequencesPlayed / 
                   (cineComp.Metrics.SequencesPlayed + cineComp.Metrics.TimesSkipped);
        }
        
        public void PlaySequence(Entity entity, FixedString64Bytes sequenceID)
        {
            if (!EntityManager.Exists(entity)) return;
            if (!_sequenceRegistry.ContainsKey(sequenceID)) return;
            
            var cineComp = EntityManager.GetComponentData<CinematicComponent>(entity);
            
            cineComp.ActiveSequence = _sequenceRegistry[sequenceID];
            cineComp.ActiveSequence.State = PlaybackState.PreRoll;
            cineComp.ActiveSequence.CurrentTime = 0;
            
            EntityManager.SetComponentData(entity, cineComp);
        }
        
        public void RegisterSequence(CinematicSequence sequence)
        {
            if (!_sequenceRegistry.ContainsKey(sequence.SequenceID))
            {
                _sequenceRegistry.Add(sequence.SequenceID, sequence);
            }
        }
        
        public void SkipSequence(Entity entity)
        {
            if (!EntityManager.Exists(entity)) return;
            
            var cineComp = EntityManager.GetComponentData<CinematicComponent>(entity);
            
            if (cineComp.ActiveSequence.IsSkippable && 
                cineComp.ActiveSequence.CurrentTime >= cineComp.ActiveSequence.SkipDelay)
            {
                cineComp.ActiveSequence.CurrentTime = cineComp.ActiveSequence.TotalDuration;
                cineComp.ActiveSequence.State = PlaybackState.Skipping;
                cineComp.Metrics.TimesSkipped++;
                
                EntityManager.SetComponentData(entity, cineComp);
            }
        }
        
        public void PauseSequence(Entity entity)
        {
            if (!EntityManager.Exists(entity)) return;
            
            var cineComp = EntityManager.GetComponentData<CinematicComponent>(entity);
            
            if (cineComp.ActiveSequence.CanPause && 
                cineComp.ActiveSequence.State == PlaybackState.Playing)
            {
                cineComp.ActiveSequence.State = PlaybackState.Paused;
                EntityManager.SetComponentData(entity, cineComp);
            }
        }
        
        public void ResumeSequence(Entity entity)
        {
            if (!EntityManager.Exists(entity)) return;
            
            var cineComp = EntityManager.GetComponentData<CinematicComponent>(entity);
            
            if (cineComp.ActiveSequence.State == PlaybackState.Paused)
            {
                cineComp.ActiveSequence.State = PlaybackState.Playing;
                EntityManager.SetComponentData(entity, cineComp);
            }
        }
    }
}
