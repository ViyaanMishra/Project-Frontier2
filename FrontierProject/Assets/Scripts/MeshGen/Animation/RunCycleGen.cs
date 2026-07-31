using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;

namespace FrontierProject.MeshGen.Animation
{
    /// <summary>
    /// Ultra-high quality run cycle generator with procedural foot placement,
    /// dynamic balance adjustment, and terrain-adaptive stride calculation.
    /// Supports multiple run styles, fatigue simulation, and injury compensation.
    /// </summary>
    [System.Serializable]
    public class RunCycleGen : MonoBehaviour
    {
        #region Run Styles & Parameters
        
        public enum RunStyle
        {
            Sprint,          // Maximum speed, forward lean
            Jog,             // Relaxed, upright posture
            Tactical,        // Low center of gravity, ready stance
            Exhausted,       // Heavy breathing, slumped shoulders
            Injured,         // Limping, favoring one leg
            Stealth,         // Quiet footfalls, controlled movement
            Panic,           // Erratic, high knee lift
            Encumbered       // Slow, weighted movement
        }

        [Header("Core Run Parameters")]
        [Range(0.5f, 3.0f)] public float baseSpeed = 1.0f;
        [Range(0.8f, 1.5f)] public float strideLength = 1.0f;
        [Range(0.5f, 2.0f)] public float cadence = 1.0f;
        
        [Header("Body Mechanics")]
        [Range(-30f, 30f)] public float forwardLean = 0f;
        [Range(0f, 45f)] public float kneeLiftAngle = 25f;
        [Range(0f, 60f)] public float hipExtension = 35f;
        [Range(0f, 30f)] public float armSwingAngle = 15f;
        [Range(0f, 1f)] public float torsoRotation = 0.3f;
        
        [Header("Foot Placement")]
        [Range(0.1f, 0.5f)] public float stepWidth = 0.25f;
        [Range(0f, 0.3f)] public float footRollAmount = 0.15f;
        [Range(0f, 1f)] public float toeOffPower = 0.8f;
        public bool enableHeelStrike = true;
        public bool enableMidfootStrike = false;
        public bool enableForefootStrike = false;
        
        [Header("Vertical Motion")]
        [Range(0f, 0.3f)] public float verticalBounce = 0.08f;
        [Range(0f, 0.2f)] public float pelvicDrop = 0.05f;
        [Range(0f, 0.1f)] public float shoulderShrug = 0.03f;
        
        [Header("Arm Dynamics")]
        [Range(0f, 90f)] public float elbowAngle = 75f;
        [Range(0f, 1f)] public float armSwingPhase = 0.5f; // Phase offset from legs
        public bool oppositeArmLegSwing = true;
        public float armRelaxation = 0.2f;
        
        [Header("Head & Gaze")]
        [Range(0f, 15f)] public float headBobAmount = 3f;
        [Range(0f, 1f)] public float gazeStability = 0.8f;
        public Vector3 gazeTarget = Vector3.zero;
        public bool stabilizeGazeOnTarget = true;
        
        [Header("Breathing Simulation")]
        [Range(0.5f, 3.0f)] public float breathRate = 1.5f;
        [Range(0.01f, 0.2f)] public float chestExpansion = 0.05f;
        [Range(0.01f, 0.1f)] public float shoulderBreathMotion = 0.02f;
        
        [Header("Fatigue System")]
        public bool enableFatigue = true;
        [Range(0f, 1f)] public float currentFatigue = 0f;
        [Range(0f, 1f)] public float maxFatigue = 1f;
        [Range(0.001f, 0.1f)] public float fatigueGainRate = 0.01f;
        [Range(0.001f, 0.05f)] public float fatigueRecoveryRate = 0.005f;
        
        [Header("Injury System")]
        public bool enableInjuries = true;
        public float leftLegInjury = 0f; // 0 = healthy, 1 = fully injured
        public float rightLegInjury = 0f;
        public float leftArmInjury = 0f;
        public float rightArmInjury = 0f;
        
        [Header("Terrain Adaptation")]
        public bool adaptToTerrain = true;
        [Range(0f, 45f)] public float slopeAngle = 0f;
        public float groundHeightL = 0f;
        public float groundHeightR = 0f;
        public float terrainRoughness = 0f;
        
        [Header("Run Style")]
        public RunStyle currentStyle = RunStyle.Sprint;
        
        #endregion

        #region Runtime Data
        
        private float runCycleTime;
        private float leftLegPhase;
        private float rightLegPhase;
        private float currentVerticalOffset;
        private Vector3 pelvisPosition;
        private Quaternion pelvisRotation;
        private Vector3 spinePosition;
        private Quaternion spineRotation;
        private Vector3 headPosition;
        private Quaternion headRotation;
        
        // Breath animation
        private float breathCycle;
        private float chestScale;
        
        // Fatigue modifiers
        private float fatigueStrideReduction;
        private float fatigueCadenceReduction;
        private float fatiguePostureChange;
        
        #endregion

        #region Run Style Presets
        
        private struct RunStylePreset
        {
            public float forwardLean;
            public float kneeLift;
            public float armSwing;
            public float verticalBounce;
            public float cadence;
            public float strideLength;
            public float elbowAngle;
            public float torsoRotation;
        }
        
        private static readonly NativeArray<RunStylePreset> stylePresets = new NativeArray<RunStylePreset>(8, Allocator.Persistent);
        
        private void InitializeStylePresets()
        {
            stylePresets[0] = new RunStylePreset { forwardLean = 25f, kneeLift = 35f, armSwing = 25f, verticalBounce = 0.12f, cadence = 1.8f, strideLength = 1.4f, elbowAngle = 60f, torsoRotation = 0.4f }; // Sprint
            stylePresets[1] = new RunStylePreset { forwardLean = 5f, kneeLift = 20f, armSwing = 12f, verticalBounce = 0.05f, cadence = 1.2f, strideLength = 0.9f, elbowAngle = 80f, torsoRotation = 0.2f }; // Jog
            stylePresets[2] = new RunStylePreset { forwardLean = 10f, kneeLift = 18f, armSwing = 10f, verticalBounce = 0.03f, cadence = 1.3f, strideLength = 0.85f, elbowAngle = 90f, torsoRotation = 0.15f }; // Tactical
            stylePresets[3] = new RunStylePreset { forwardLean = -5f, kneeLift = 15f, armSwing = 8f, verticalBounce = 0.02f, cadence = 0.9f, strideLength = 0.7f, elbowAngle = 100f, torsoRotation = 0.1f }; // Exhausted
            stylePresets[4] = new RunStylePreset { forwardLean = 0f, kneeLift = 12f, armSwing = 6f, verticalBounce = 0.01f, cadence = 0.8f, strideLength = 0.6f, elbowAngle = 85f, torsoRotation = 0.05f }; // Injured
            stylePresets[5] = new RunStylePreset { forwardLean = 8f, kneeLift = 15f, armSwing = 5f, verticalBounce = 0.02f, cadence = 1.0f, strideLength = 0.65f, elbowAngle = 95f, torsoRotation = 0.08f }; // Stealth
            stylePresets[6] = new RunStylePreset { forwardLean = 20f, kneeLift = 45f, armSwing = 30f, verticalBounce = 0.15f, cadence = 2.0f, strideLength = 1.2f, elbowAngle = 50f, torsoRotation = 0.5f }; // Panic
            stylePresets[7] = new RunStylePreset { forwardLean = -10f, kneeLift = 10f, armSwing = 8f, verticalBounce = 0.02f, cadence = 0.7f, strideLength = 0.5f, elbowAngle = 110f, torsoRotation = 0.1f }; // Encumbered
        }
        
        #endregion

        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeStylePresets();
            ResetRunCycle();
        }
        
        private void OnDestroy()
        {
            if (stylePresets.IsCreated)
                stylePresets.Dispose();
        }
        
        private void Update()
        {
            float deltaTime = Time.deltaTime;
            
            // Update fatigue system
            if (enableFatigue)
            {
                UpdateFatigue(deltaTime);
                ApplyFatigueModifiers();
            }
            
            // Update breath cycle
            breathCycle += deltaTime * breathRate * (1f + currentFatigue * 0.5f);
            chestScale = math.sin(breathCycle * math.PI * 2f) * chestExpansion;
            
            // Advance run cycle
            float effectiveCadence = cadence * (1f - fatigueCadenceReduction);
            runCycleTime += deltaTime * effectiveCadence;
            
            if (runCycleTime >= 1f)
                runCycleTime -= 1f;
            
            // Calculate leg phases
            leftLegPhase = runCycleTime;
            rightLegPhase = math.frac(runCycleTime + 0.5f);
            
            // Apply run style modifications
            ApplyRunStyle();
            
            // Calculate all body positions
            CalculatePelvisMotion();
            CalculateSpineMotion();
            CalculateHeadMotion();
            CalculateLeftLeg();
            CalculateRightLeg();
            CalculateLeftArm();
            CalculateRightArm();
            
            // Terrain adaptation
            if (adaptToTerrain)
            {
                AdaptToTerrain();
            }
        }
        
        #endregion

        #region Core Motion Calculations
        
        private void ResetRunCycle()
        {
            runCycleTime = 0f;
            leftLegPhase = 0f;
            rightLegPhase = 0.5f;
            currentVerticalOffset = 0f;
            breathCycle = 0f;
            currentFatigue = 0f;
        }
        
        private void CalculatePelvisMotion()
        {
            // Vertical bounce (sinusoidal with double frequency for two steps)
            float verticalMotion = math.sin(runCycleTime * math.PI * 4f) * verticalBounce;
            currentVerticalOffset = verticalMotion * (1f - fatiguePostureChange * 0.3f);
            
            // Pelvic drop during swing phase
            float pelvicDropL = CalculatePelvicDrop(leftLegPhase) * pelvicDrop;
            float pelvicDropR = CalculatePelvicDrop(rightLegPhase) * pelvicDrop;
            float netPelvicDrop = (pelvicDropL - pelvicDropR) * 0.5f;
            
            // Forward lean based on style and fatigue
            float effectiveLean = forwardLean * (1f - fatiguePostureChange);
            
            pelvisPosition = new Vector3(0f, currentVerticalOffset + netPelvicDrop, 0f);
            pelvisRotation = Quaternion.Euler(effectiveLean, 0f, netPelvicDrop * 10f);
        }
        
        private float CalculatePelvicDrop(float phase)
        {
            // Pelvis drops on the stance leg when opposite leg is in swing
            float stancePhase = math.frac(phase + 0.5f);
            return math.smoothstep(0f, 1f, stancePhase * 2f) * (1f - math.smoothstep(0.5f, 1f, stancePhase * 2f - 1f));
        }
        
        private void CalculateSpineMotion()
        {
            // Torso rotation counter to pelvis
            float spineRotationZ = -math.sin(runCycleTime * math.PI * 4f) * torsoRotation * 15f;
            
            // Shoulder shrug from breathing
            float shoulderBreath = math.sin(breathCycle * math.PI * 2f) * shoulderBreathMotion;
            
            spinePosition = pelvisPosition + new Vector3(0f, 0.4f, 0f);
            spineRotation = Quaternion.Euler(forwardLean * 0.5f, 0f, spineRotationZ) * 
                           Quaternion.Euler(0f, 0f, shoulderBreath * 5f);
        }
        
        private void CalculateHeadMotion()
        {
            // Head bob (reduced by gaze stability)
            float headBob = math.sin(runCycleTime * math.PI * 4f) * headBobAmount * (1f - gazeStability);
            
            // Gaze stabilization
            if (stabilizeGazeOnTarget && gazeTarget != Vector3.zero)
            {
                Vector3 lookDirection = gazeTarget - headPosition;
                headRotation = Quaternion.LookRotation(lookDirection);
            }
            else
            {
                headRotation = Quaternion.Euler(headBob, 0f, 0f);
            }
            
            headPosition = spinePosition + new Vector3(0f, 0.25f, 0f);
        }
        
        #endregion

        #region Leg Calculations
        
        private void CalculateLeftLeg()
        {
            float phase = leftLegPhase;
            float injuryFactor = 1f - leftLegInjury;
            
            // Determine gait phase (stance vs swing)
            bool isStance = phase < 0.5f;
            float gaitProgress = isStance ? phase * 2f : (phase - 0.5f) * 2f;
            
            // Hip flexion/extension
            float hipAngle = CalculateHipAngle(phase, injuryFactor);
            
            // Knee flexion
            float kneeAngle = CalculateKneeAngle(phase, gaitProgress, isStance, injuryFactor);
            
            // Ankle dorsiflexion/plantarflexion
            float ankleAngle = CalculateAnkleAngle(phase, gaitProgress, isStance, injuryFactor);
            
            // Foot placement with terrain adaptation
            float footY = isStance ? Mathf.Max(0f, groundHeightL) : 
                         CalculateSwingFootHeight(gaitProgress, injuryFactor);
            float footX = isStance ? stepWidth : stepWidth * (1f - gaitProgress * 0.3f);
            float footZ = CalculateFootForwardPosition(phase, gaitProgress, isStance);
            
            // Apply to transform hierarchy (pseudo-code for actual bone transforms)
            // leftHip.localRotation = Quaternion.Euler(hipAngle, 0f, 0f);
            // leftKnee.localRotation = Quaternion.Euler(kneeAngle, 0f, 0f);
            // leftAnkle.localRotation = Quaternion.Euler(ankleAngle, 0f, 0f);
        }
        
        private void CalculateRightLeg()
        {
            float phase = rightLegPhase;
            float injuryFactor = 1f - rightLegInjury;
            
            bool isStance = phase < 0.5f;
            float gaitProgress = isStance ? phase * 2f : (phase - 0.5f) * 2f;
            
            float hipAngle = CalculateHipAngle(phase, injuryFactor);
            float kneeAngle = CalculateKneeAngle(phase, gaitProgress, isStance, injuryFactor);
            float ankleAngle = CalculateAnkleAngle(phase, gaitProgress, isStance, injuryFactor);
            
            float footY = isStance ? Mathf.Max(0f, groundHeightR) : 
                         CalculateSwingFootHeight(gaitProgress, injuryFactor);
            float footX = -stepWidth * (isStance ? 1f : (1f - gaitProgress * 0.3f));
            float footZ = CalculateFootForwardPosition(phase, gaitProgress, isStance);
        }
        
        private float CalculateHipAngle(float phase, float injuryFactor)
        {
            // Hip goes from extension (negative) to flexion (positive)
            float baseAngle = math.sin(phase * math.PI * 2f) * hipExtension;
            
            // Injury compensation - reduce range of motion
            return baseAngle * injuryFactor;
        }
        
        private float CalculateKneeAngle(float phase, float gaitProgress, bool isStance, float injuryFactor)
        {
            if (isStance)
            {
                // Slight knee flexion during stance for shock absorption
                float stanceFlexion = math.sin(gaitProgress * math.PI) * 10f;
                return stanceFlexion * injuryFactor;
            }
            else
            {
                // Knee flexion during swing for foot clearance
                float swingFlexion = math.sin(gaitProgress * math.PI) * kneeLiftAngle;
                
                // Add extra lift at mid-swing
                float midSwingBoost = math.pow(math.sin(gaitProgress * math.PI), 2f) * 15f;
                
                return (swingFlexion + midSwingBoost) * injuryFactor;
            }
        }
        
        private float CalculateAnkleAngle(float phase, float gaitProgress, bool isStance, float injuryFactor)
        {
            if (isStance)
            {
                // Heel strike -> foot flat -> toe off
                if (gaitProgress < 0.1f && enableHeelStrike)
                {
                    return 10f * injuryFactor; // Dorsiflexion for heel strike
                }
                else if (gaitProgress < 0.6f)
                {
                    return 0f; // Foot flat
                }
                else
                {
                    // Toe off with plantarflexion
                    float toeOff = (gaitProgress - 0.6f) / 0.4f * 25f * toeOffPower;
                    return -toeOff * injuryFactor;
                }
            }
            else
            {
                // Dorsiflexion during swing for toe clearance
                float dorsiflexion = math.sin(gaitProgress * math.PI) * 15f;
                return dorsiflexion * injuryFactor;
            }
        }
        
        private float CalculateSwingFootHeight(float gaitProgress, float injuryFactor)
        {
            // Parabolic arc for natural foot trajectory
            float baseHeight = math.sin(gaitProgress * math.PI) * 0.15f;
            
            // Extra clearance at mid-swing
            float clearance = math.pow(math.sin(gaitProgress * math.PI), 3f) * 0.08f;
            
            return (baseHeight + clearance) * injuryFactor;
        }
        
        private float CalculateFootForwardPosition(float phase, float gaitProgress, bool isStance)
        {
            float effectiveStride = strideLength * (1f - fatigueStrideReduction);
            
            if (isStance)
            {
                // Foot moves backward relative to body during stance
                return -gaitProgress * effectiveStride * 0.5f;
            }
            else
            {
                // Foot swings forward
                float swingProgress = math.sin(gaitProgress * math.PI - math.PI / 2f) * 0.5f + 0.5f;
                return (swingProgress - 0.5f) * effectiveStride;
            }
        }
        
        #endregion

        #region Arm Calculations
        
        private void CalculateLeftArm()
        {
            float phase = leftLegPhase;
            float armPhase = oppositeArmLegSwing ? math.frac(phase + 0.5f) : phase;
            float injuryFactor = 1f - leftArmInjury;
            
            // Shoulder flexion/extension
            float shoulderAngle = math.sin(armPhase * math.PI * 2f) * armSwingAngle * armSwingPhase * injuryFactor;
            
            // Elbow flexion (more flexion during swing)
            float elbowFlexion = (1f - math.abs(math.cos(armPhase * math.PI * 2f))) * (90f - elbowAngle);
            float effectiveElbow = elbowAngle + elbowFlexion * (1f - armRelaxation) * injuryFactor;
            
            // Apply rotations
            // leftShoulder.localRotation = Quaternion.Euler(shoulderAngle, 0f, 0f);
            // leftElbow.localRotation = Quaternion.Euler(-effectiveElbow, 0f, 0f);
        }
        
        private void CalculateRightArm()
        {
            float phase = rightLegPhase;
            float armPhase = oppositeArmLegSwing ? math.frac(phase + 0.5f) : phase;
            float injuryFactor = 1f - rightArmInjury;
            
            float shoulderAngle = math.sin(armPhase * math.PI * 2f) * armSwingAngle * armSwingPhase * injuryFactor;
            float elbowFlexion = (1f - math.abs(math.cos(armPhase * math.PI * 2f))) * (90f - elbowAngle);
            float effectiveElbow = elbowAngle + elbowFlexion * (1f - armRelaxation) * injuryFactor;
        }
        
        #endregion

        #region Fatigue System
        
        private void UpdateFatigue(float deltaTime)
        {
            if (currentFatigue < maxFatigue)
            {
                currentFatigue += fatigueGainRate * deltaTime * baseSpeed;
                currentFatigue = Mathf.Min(currentFatigue, maxFatigue);
            }
            else
            {
                // Recovery when not running (would be triggered externally)
                currentFatigue -= fatigueRecoveryRate * deltaTime;
                currentFatigue = Mathf.Max(currentFatigue, 0f);
            }
        }
        
        private void ApplyFatigueModifiers()
        {
            float normalizedFatigue = currentFatigue / maxFatigue;
            
            // Reduce stride length
            fatigueStrideReduction = normalizedFatigue * 0.25f;
            
            // Reduce cadence
            fatigueCadenceReduction = normalizedFatigue * 0.15f;
            
            // Change posture (more slumped)
            fatiguePostureChange = normalizedFatigue * 0.3f;
        }
        
        #endregion

        #region Run Style Application
        
        private void ApplyRunStyle()
        {
            if (!stylePresets.IsCreated)
                InitializeStylePresets();
            
            int styleIndex = (int)currentStyle;
            RunStylePreset preset = stylePresets[styleIndex];
            
            // Blend current parameters with preset
            forwardLean = Mathf.Lerp(forwardLean, preset.forwardLean, 0.1f);
            kneeLiftAngle = Mathf.Lerp(kneeLiftAngle, preset.kneeLift, 0.1f);
            armSwingAngle = Mathf.Lerp(armSwingAngle, preset.armSwing, 0.1f);
            verticalBounce = Mathf.Lerp(verticalBounce, preset.verticalBounce, 0.1f);
            cadence = Mathf.Lerp(cadence, preset.cadence, 0.1f);
            strideLength = Mathf.Lerp(strideLength, preset.strideLength, 0.1f);
            elbowAngle = Mathf.Lerp(elbowAngle, preset.elbowAngle, 0.1f);
            torsoRotation = Mathf.Lerp(torsoRotation, preset.torsoRotation, 0.1f);
        }
        
        #endregion

        #region Terrain Adaptation
        
        private void AdaptToTerrain()
        {
            // Adjust for slope
            float slopeAdjustment = slopeAngle * 0.5f;
            forwardLean += slopeAdjustment;
            
            // Adjust stride for uphill/downhill
            if (slopeAngle > 0)
            {
                strideLength *= (1f - slopeAngle / 90f * 0.3f);
                kneeLiftAngle *= (1f + slopeAngle / 90f * 0.2f);
            }
            
            // Rough terrain adaptation
            if (terrainRoughness > 0.1f)
            {
                verticalBounce *= (1f + terrainRoughness * 0.5f);
                cadence *= (1f - terrainRoughness * 0.2f);
                
                // Increase foot clearance
                kneeLiftAngle *= (1f + terrainRoughness * 0.3f);
            }
            
            // Height difference between feet
            float heightDiff = groundHeightL - groundHeightR;
            if (math.abs(heightDiff) > 0.05f)
            {
                // Tilt pelvis to match terrain
                float tiltAngle = math.atan2(heightDiff, stepWidth * 2f) * math.degrees;
                pelvisRotation = Quaternion.Euler(pelvisRotation.eulerAngles.x, pelvisRotation.eulerAngles.y, tiltAngle);
            }
        }
        
        #endregion

        #region Public API
        
        public void SetRunStyle(RunStyle style)
        {
            currentStyle = style;
        }
        
        public void SetSpeed(float speed)
        {
            baseSpeed = Mathf.Clamp(speed, 0.5f, 3.0f);
        }
        
        public void AddFatigue(float amount)
        {
            if (enableFatigue)
            {
                currentFatigue = Mathf.Min(currentFatigue + amount, maxFatigue);
            }
        }
        
        public void ClearFatigue()
        {
            currentFatigue = 0f;
        }
        
        public void SetInjury(string limb, float severity)
        {
            if (!enableInjuries) return;
            
            switch (limb.ToLower())
            {
                case "leftleg": leftLegInjury = Mathf.Clamp01(severity); break;
                case "rightleg": rightLegInjury = Mathf.Clamp01(severity); break;
                case "leftarm": leftArmInjury = Mathf.Clamp01(severity); break;
                case "rightarm": rightArmInjury = Mathf.Clamp01(severity); break;
            }
        }
        
        public void ClearInjuries()
        {
            leftLegInjury = 0f;
            rightLegInjury = 0f;
            leftArmInjury = 0f;
            rightArmInjury = 0f;
        }
        
        public void SetTerrainData(float slope, float heightL, float heightR, float roughness)
        {
            slopeAngle = slope;
            groundHeightL = heightL;
            groundHeightR = heightR;
            terrainRoughness = roughness;
        }
        
        public Vector3 GetPelvisPosition() => pelvisPosition;
        public Quaternion GetPelvisRotation() => pelvisRotation;
        public float GetCurrentFatigue() => currentFatigue;
        public float GetRunCycleProgress() => runCycleTime;
        
        #endregion
    }
}
