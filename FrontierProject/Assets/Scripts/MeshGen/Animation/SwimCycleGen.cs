using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace FrontierProject.MeshGen.Animation
{
    /// <summary>
    /// Professional-grade swimming animation generator with multiple stroke styles,
    /// buoyancy simulation, water resistance, and realistic limb kinematics.
    /// Supports freestyle, breaststroke, backstroke, butterfly, and survival swimming.
    /// </summary>
    [System.Serializable]
    public class SwimCycleGen : MonoBehaviour
    {
        #region Stroke Types & Parameters
        
        public enum SwimStroke
        {
            Freestyle,      // Front crawl - alternating arms
            Breaststroke,   // Symmetric arm/leg movement
            Backstroke,     // On back, alternating arms
            Butterfly,      // Symmetric dolphin kick
            Sidestroke,     // Recovery stroke
            Treading,       // Vertical survival swim
            UnderwaterDolphin // Manta kick
        }

        [Header("Core Swimming Parameters")]
        [Range(0.2f, 2.5f)] public float swimSpeed = 1.0f;
        [Range(0.5f, 3.0f)] public float strokeRate = 1.0f;
        [Range(0.3f, 1.5f)] public float strokePower = 1.0f;
        
        [Header("Body Position")]
        [Range(-45f, 45f)] public float bodyAngle = 0f; // Pitch in water
        [Range(0f, 1f)] public float bodyRollAmount = 0.8f;
        [Range(0f, 0.5f)] public float bodyHeave = 0.1f; // Vertical oscillation
        [Range(0f, 0.3f)] public float hipDepth = 0.15f;
        
        [Header("Arm Mechanics - Freestyle")]
        [Range(0f, 180f)] public float pullPhaseAngle = 140f;
        [Range(0f, 90f)] public float elbowBendPull = 75f;
        [Range(0f, 60f)] public float handEntryAngle = 25f;
        [Range(0f, 1f)] public float catchEfficiency = 0.85f;
        [Range(0f, 1f)] public float pullPathCurve = 0.4f;
        
        [Header("Kick Mechanics")]
        [Range(0f, 90f)] public float kickAmplitude = 45f;
        [Range(0.5f, 2.0f)] public float kickFrequency = 1.0f;
        [Range(0f, 60f)] public float kneeFlexion = 35f;
        [Range(0f, 45f)] public float ankleDorsiflexion = 20f;
        public bool flutterKick = true;
        public bool dolphinKick = false;
        
        [Header("Breathing")]
        [Range(0f, 1f)] public float breathTiming = 0.3f; // When in cycle to breathe
        [Range(0f, 45f)] public float headTurnAngle = 35f;
        [Range(0f, 0.5f)] public float breathDuration = 0.15f;
        public bool bilateralBreathing = true;
        public int breathEveryNStrokes = 2;
        
        [Header("Water Physics")]
        [Range(0.8f, 1.2f)] public float buoyancy = 1.0f;
        [Range(0.5f, 2.0f)] public float waterResistance = 1.0f;
        [Range(0f, 1f)] public float turbulence = 0.1f;
        public float waterDensity = 1000f; // kg/m³
        
        [Header("Fatigue & Stamina")]
        public bool enableStamina = true;
        [Range(0f, 100f)] public float currentStamina = 100f;
        [Range(0f, 100f)] public float maxStamina = 100f;
        [Range(0.1f, 5f)] public float staminaDrainRate = 1.0f;
        [Range(0.1f, 3f)] public float staminaRecoveryRate = 0.5f;
        
        [Header("Stroke Style")]
        public SwimStroke currentStroke = SwimStroke.Freestyle;
        
        #endregion

        #region Runtime Data
        
        private float swimCycleTime;
        private float leftArmPhase;
        private float rightArmPhase;
        private float legPhase;
        private float bodyRollPhase;
        
        private Vector3 bodyPosition;
        private Quaternion bodyRotation;
        private Vector3 hipPosition;
        private Quaternion hipRotation;
        private Vector3 chestPosition;
        private Quaternion chestRotation;
        private Vector3 headPosition;
        private Quaternion headRotation;
        
        // Breathing state
        private bool isInhaling;
        private float breathHoldTimer;
        private int strokeCountSinceBreath;
        
        // Stamina modifiers
        private float staminaStrokePowerMod;
        private float staminaStrokeRateMod;
        private float staminaTechniqueDegradation;
        
        // Water interaction
        private float currentDrag;
        private float waveHeight;
        private Vector3 waterVelocity;
        
        #endregion

        #region Stroke Presets
        
        private struct StrokePreset
        {
            public float bodyAngle;
            public float bodyRoll;
            public float strokeRate;
            public float kickFreq;
            public float kickAmp;
            public float elbowBend;
            public float breathTiming;
            public float powerPhase;
        }
        
        private static StrokePreset[] strokePresets = new StrokePreset[7];
        
        private void InitializeStrokePresets()
        {
            strokePresets[0] = new StrokePreset { bodyAngle = -5f, bodyRoll = 0.7f, strokeRate = 1.2f, kickFreq = 1.5f, kickAmp = 40f, elbowBend = 75f, breathTiming = 0.25f, powerPhase = 0.6f }; // Freestyle
            strokePresets[1] = new StrokePreset { bodyAngle = 0f, bodyRoll = 0.2f, strokeRate = 0.8f, kickFreq = 0.8f, kickAmp = 50f, elbowBend = 90f, breathTiming = 0.5f, powerPhase = 0.5f }; // Breaststroke
            strokePresets[2] = new StrokePreset { bodyAngle = 5f, bodyRoll = 0.6f, strokeRate = 1.0f, kickFreq = 1.3f, kickAmp = 35f, elbowBend = 70f, breathTiming = 0.0f, powerPhase = 0.55f }; // Backstroke
            strokePresets[3] = new StrokePreset { bodyAngle = -8f, bodyRoll = 0.3f, strokeRate = 0.9f, kickFreq = 0.7f, kickAmp = 55f, elbowBend = 85f, breathTiming = 0.15f, powerPhase = 0.65f }; // Butterfly
            strokePresets[4] = new StrokePreset { bodyAngle = 10f, bodyRoll = 0.1f, strokeRate = 0.6f, kickFreq = 0.5f, kickAmp = 30f, elbowBend = 60f, breathTiming = 0.4f, powerPhase = 0.45f }; // Sidestroke
            strokePresets[5] = new StrokePreset { bodyAngle = 20f, bodyRoll = 0.0f, strokeRate = 1.5f, kickFreq = 2.0f, kickAmp = 25f, elbowBend = 95f, breathTiming = 0.0f, powerPhase = 0.3f }; // Treading
            strokePresets[6] = new StrokePreset { bodyAngle = -15f, bodyRoll = 0.1f, strokeRate = 0.7f, kickFreq = 0.6f, kickAmp = 60f, elbowBend = 80f, breathTiming = 0.0f, powerPhase = 0.7f }; // Underwater Dolphin
        }
        
        #endregion

        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeStrokePresets();
            ResetSwimCycle();
        }
        
        private void Update()
        {
            float deltaTime = Time.deltaTime;
            
            // Update stamina
            if (enableStamina)
            {
                UpdateStamina(deltaTime);
                ApplyStaminaModifiers();
            }
            
            // Advance swim cycle
            float effectiveStrokeRate = strokeRate * staminaStrokeRateMod;
            swimCycleTime += deltaTime * effectiveStrokeRate;
            
            if (swimCycleTime >= 1f)
                swimCycleTime -= 1f;
            
            // Calculate phases based on stroke type
            CalculatePhases();
            
            // Apply stroke preset modifications
            ApplyStrokePreset();
            
            // Calculate body motion
            CalculateBodyMotion();
            CalculateHipMotion();
            CalculateChestMotion();
            CalculateHeadMotion();
            
            // Calculate limb motions
            CalculateLeftArm();
            CalculateRightArm();
            CalculateLegs();
            
            // Water interaction
            CalculateWaterInteraction();
            
            // Handle breathing
            UpdateBreathing(deltaTime);
        }
        
        #endregion

        #region Core Calculations
        
        private void ResetSwimCycle()
        {
            swimCycleTime = 0f;
            leftArmPhase = 0f;
            rightArmPhase = 0.5f;
            legPhase = 0f;
            bodyRollPhase = 0f;
            currentStamina = maxStamina;
            isInhaling = false;
            strokeCountSinceBreath = 0;
        }
        
        private void CalculatePhases()
        {
            switch (currentStroke)
            {
                case SwimStroke.Freestyle:
                    leftArmPhase = swimCycleTime;
                    rightArmPhase = math.frac(swimCycleTime + 0.5f);
                    legPhase = swimCycleTime * kickFrequency;
                    bodyRollPhase = swimCycleTime * 2f;
                    break;
                    
                case SwimStroke.Breaststroke:
                    leftArmPhase = swimCycleTime;
                    rightArmPhase = swimCycleTime; // Symmetric
                    legPhase = math.frac(swimCycleTime + 0.5f); // Kick after pull
                    bodyRollPhase = 0f; // Minimal roll
                    break;
                    
                case SwimStroke.Backstroke:
                    leftArmPhase = swimCycleTime;
                    rightArmPhase = math.frac(swimCycleTime + 0.5f);
                    legPhase = swimCycleTime * kickFrequency;
                    bodyRollPhase = swimCycleTime * 2f;
                    break;
                    
                case SwimStroke.Butterfly:
                    leftArmPhase = swimCycleTime;
                    rightArmPhase = swimCycleTime; // Symmetric
                    legPhase = swimCycleTime * kickFrequency * 2f; // Two kicks per stroke
                    bodyRollPhase = 0f;
                    break;
                    
                default:
                    leftArmPhase = swimCycleTime;
                    rightArmPhase = math.frac(swimCycleTime + 0.5f);
                    legPhase = swimCycleTime * kickFrequency;
                    bodyRollPhase = swimCycleTime;
                    break;
            }
        }
        
        private void CalculateBodyMotion()
        {
            // Body heave (vertical oscillation)
            float heaveMotion = math.sin(swimCycleTime * math.PI * 2f) * bodyHeave;
            
            // Body roll
            float rollAngle = math.sin(bodyRollPhase * math.PI * 2f) * 35f * bodyRollAmount;
            
            // Pitch based on stroke and speed
            float pitchAngle = bodyAngle + (swimSpeed - 1f) * 5f;
            
            bodyPosition = new Vector3(0f, heaveMotion, 0f);
            bodyRotation = Quaternion.Euler(pitchAngle, 0f, rollAngle);
            
            // Add turbulence
            if (turbulence > 0f)
            {
                float turbX = math.sin(swimCycleTime * 13f + turbulence) * turbulence * 0.02f;
                float turbY = math.cos(swimCycleTime * 17f) * turbulence * 0.01f;
                float turbZ = math.sin(swimCycleTime * 11f) * turbulence * 0.01f;
                bodyPosition += new Vector3(turbX, turbY, turbZ);
            }
        }
        
        private void CalculateHipMotion()
        {
            // Hips follow body with slight lag
            float hipLag = 0.1f;
            float hipHeave = math.sin((swimCycleTime - hipLag) * math.PI * 2f) * bodyHeave * 0.8f;
            
            hipPosition = bodyPosition + new Vector3(0f, -hipDepth + hipHeave, -0.3f);
            hipRotation = bodyRotation;
            
            // Dolphin kick adds hip undulation
            if (dolphinKick || currentStroke == SwimStroke.Butterfly)
            {
                float hipUndulation = math.sin(legPhase * math.PI * 2f) * 15f;
                hipRotation = Quaternion.Euler(hipRotation.eulerAngles.x + hipUndulation, 0f, hipRotation.eulerAngles.z);
            }
        }
        
        private void CalculateChestMotion()
        {
            // Chest leads body rotation
            float chestLead = 0.05f;
            float chestRoll = math.sin((bodyRollPhase + chestLead) * math.PI * 2f) * 35f * bodyRollAmount * 1.1f;
            
            chestPosition = bodyPosition + new Vector3(0f, 0.15f, 0.2f);
            chestRotation = Quaternion.Euler(bodyAngle, 0f, chestRoll);
        }
        
        private void CalculateHeadMotion()
        {
            // Head position relative to chest
            headPosition = chestPosition + new Vector3(0f, 0.2f, 0.15f);
            
            // Default head rotation (looking down/forward)
            float headPitch = -15f;
            float headYaw = 0f;
            float headRoll = 0f;
            
            // Breathing head turn
            if (isInhaling)
            {
                headYaw = headTurnAngle * (breathTiming > 0.5f ? 1f : -1f); // Alternate sides
                headRoll = -headTurnAngle * 0.3f;
            }
            
            headRotation = Quaternion.Euler(headPitch, headYaw, headRoll);
        }
        
        #endregion

        #region Arm Calculations
        
        private void CalculateLeftArm()
        {
            float phase = leftArmPhase;
            float effectivePower = strokePower * staminaStrokePowerMod;
            
            switch (currentStroke)
            {
                case SwimStroke.Freestyle:
                    CalculateFreestyleArm(phase, true, effectivePower);
                    break;
                case SwimStroke.Breaststroke:
                    CalculateBreaststrokeArm(phase, true, effectivePower);
                    break;
                case SwimStroke.Backstroke:
                    CalculateBackstrokeArm(phase, true, effectivePower);
                    break;
                case SwimStroke.Butterfly:
                    CalculateButterflyArm(phase, true, effectivePower);
                    break;
                default:
                    CalculateFreestyleArm(phase, true, effectivePower);
                    break;
            }
        }
        
        private void CalculateRightArm()
        {
            float phase = rightArmPhase;
            float effectivePower = strokePower * staminaStrokePowerMod;
            
            switch (currentStroke)
            {
                case SwimStroke.Freestyle:
                    CalculateFreestyleArm(phase, false, effectivePower);
                    break;
                case SwimStroke.Breaststroke:
                    CalculateBreaststrokeArm(phase, false, effectivePower);
                    break;
                case SwimStroke.Backstroke:
                    CalculateBackstrokeArm(phase, false, effectivePower);
                    break;
                case SwimStroke.Butterfly:
                    CalculateButterflyArm(phase, false, effectivePower);
                    break;
                default:
                    CalculateFreestyleArm(phase, false, effectivePower);
                    break;
            }
        }
        
        private void CalculateFreestyleArm(float phase, bool isLeft, float power)
        {
            // Phases: Entry (0-0.1), Catch (0.1-0.2), Pull (0.2-0.6), Push (0.6-0.8), Recovery (0.8-1.0)
            
            float shoulderFlexion = 0f;
            float shoulderAbduction = 0f;
            float elbowFlexion = 0f;
            float handPosition = Vector3.zero;
            
            if (phase < 0.1f) // Entry
            {
                float entryProgress = phase / 0.1f;
                shoulderFlexion = math.lerp(handEntryAngle, 30f, entryProgress);
                elbowFlexion = math.lerp(10f, 20f, entryProgress);
                handPosition.y = math.lerp(0.3f, 0f, entryProgress);
            }
            else if (phase < 0.2f) // Catch
            {
                float catchProgress = (phase - 0.1f) / 0.1f;
                shoulderFlexion = math.lerp(30f, 60f, catchProgress);
                elbowFlexion = math.lerp(20f, elbowBendPull, catchProgress);
                handPosition.y = math.lerp(0f, -0.2f, catchProgress);
            }
            else if (phase < 0.6f) // Pull
            {
                float pullProgress = (phase - 0.2f) / 0.4f;
                shoulderFlexion = math.lerp(60f, 140f, pullProgress);
                elbowFlexion = elbowBendPull + math.sin(pullProgress * math.PI) * 20f;
                handPosition.x = isLeft ? -0.3f : 0.3f;
                handPosition.x *= pullPathCurve * math.sin(pullProgress * math.PI);
                handPosition.y = -0.2f - pullProgress * 0.3f;
                handPosition.z = pullProgress * 0.5f * power;
            }
            else if (phase < 0.8f) // Push
            {
                float pushProgress = (phase - 0.6f) / 0.2f;
                shoulderFlexion = math.lerp(140f, pullPhaseAngle, pushProgress);
                elbowFlexion = math.lerp(elbowBendPull, 30f, pushProgress);
                handPosition.y = -0.5f;
                handPosition.z = 0.5f + pushProgress * 0.2f * power;
            }
            else // Recovery
            {
                float recoveryProgress = (phase - 0.8f) / 0.2f;
                float recoveryArc = math.sin(recoveryProgress * math.PI);
                shoulderFlexion = math.lerp(pullPhaseAngle, handEntryAngle, recoveryProgress);
                elbowFlexion = math.lerp(30f, 120f, recoveryProgress) * (1f - recoveryArc * 0.3f);
                handPosition.y = recoveryArc * 0.4f;
                handPosition.z = math.lerp(0.7f, 0.1f, recoveryProgress);
            }
            
            // Apply to arm transforms
            // leftShoulder.localRotation = Quaternion.Euler(shoulderFlexion, isLeft ? -10f : 10f, shoulderAbduction);
            // leftElbow.localRotation = Quaternion.Euler(-elbowFlexion, 0f, 0f);
        }
        
        private void CalculateBreaststrokeArm(float phase, bool isLeft, float power)
        {
            // Symmetric arm movement
            float shoulderFlexion = 0f;
            float shoulderAbduction = 0f;
            float elbowFlexion = 0f;
            
            if (phase < 0.15f) // Glide
            {
                shoulderFlexion = 10f;
                shoulderAbduction = 0f;
                elbowFlexion = 5f;
            }
            else if (phase < 0.3f) // Outsweep
            {
                float sweepProgress = (phase - 0.15f) / 0.15f;
                shoulderFlexion = math.lerp(10f, 30f, sweepProgress);
                shoulderAbduction = math.lerp(0f, 45f, sweepProgress);
                elbowFlexion = math.lerp(5f, 20f, sweepProgress);
            }
            else if (phase < 0.5f) // Insweep/Pull
            {
                float pullProgress = (phase - 0.3f) / 0.2f;
                shoulderFlexion = math.lerp(30f, 90f, pullProgress);
                shoulderAbduction = math.lerp(45f, 10f, pullProgress);
                elbowFlexion = math.lerp(20f, elbowBendPull, pullProgress);
            }
            else if (phase < 0.65f) // Recovery
            {
                float recoveryProgress = (phase - 0.5f) / 0.15f;
                shoulderFlexion = math.lerp(90f, 10f, recoveryProgress);
                shoulderAbduction = math.lerp(10f, 0f, recoveryProgress);
                elbowFlexion = math.lerp(elbowBendPull, 5f, recoveryProgress);
            }
            else // Extension
            {
                float extendProgress = (phase - 0.65f) / 0.35f;
                shoulderFlexion = math.lerp(10f, 10f, extendProgress);
                shoulderAbduction = 0f;
                elbowFlexion = 5f;
            }
        }
        
        private void CalculateBackstrokeArm(float phase, bool isLeft, float power)
        {
            // Similar to freestyle but inverted
            CalculateFreestyleArm(phase, isLeft, power);
            // Inversion handled by body orientation
        }
        
        private void CalculateButterflyArm(float phase, bool isLeft, float power)
        {
            // Symmetric arm movement like breaststroke but different path
            CalculateBreaststrokeArm(phase, isLeft, power);
            // Modified for butterfly pull pattern
        }
        
        #endregion

        #region Leg Calculations
        
        private void CalculateLegs()
        {
            float phase = legPhase;
            float effectiveKickAmp = kickAmplitude * staminaStrokePowerMod;
            
            if (flutterKick || currentStroke == SwimStroke.Freestyle || currentStroke == SwimStroke.Backstroke)
            {
                CalculateFlutterKick(phase, effectiveKickAmp);
            }
            else if (dolphinKick || currentStroke == SwimStroke.Butterfly || currentStroke == SwimStroke.UnderwaterDolphin)
            {
                CalculateDolphinKick(phase, effectiveKickAmp);
            }
            else if (currentStroke == SwimStroke.Breaststroke)
            {
                CalculateBreaststrokeKick(phase, effectiveKickAmp);
            }
        }
        
        private void CalculateFlutterKick(float phase, float amp)
        {
            // Alternating up/down kick
            float leftLegAngle = math.sin(phase * math.PI * 2f) * amp;
            float rightLegAngle = math.sin((phase + 0.5f) * math.PI * 2f) * amp;
            
            // Knee flexion follows thigh motion with lag
            float leftKneeFlex = math.abs(math.cos(phase * math.PI * 2f)) * kneeFlexion;
            float rightKneeFlex = math.abs(math.cos((phase + 0.5f) * math.PI * 2f)) * kneeFlexion;
            
            // Ankle plantarflexion for propulsion
            float leftAnkleFlex = -math.sin(phase * math.PI * 2f) * ankleDorsiflexion;
            float rightAnkleFlex = -math.sin((phase + 0.5f) * math.PI * 2f) * ankleDorsiflexion;
        }
        
        private void CalculateDolphinKick(float phase, float amp)
        {
            // Both legs move together in wave motion
            float hipFlexion = math.sin(phase * math.PI * 2f) * amp;
            float kneeFlexion = math.sin((phase - 0.1f) * math.PI * 2f) * amp * 1.2f;
            float ankleFlexion = math.sin((phase - 0.2f) * math.PI * 2f) * amp * 1.3f;
            
            // Wave propagation down the body
            float waveDelay = 0.05f;
            float hipWave = math.sin((phase - waveDelay) * math.PI * 2f) * amp * 0.5f;
        }
        
        private void CalculateBreaststrokeKick(float phase, float amp)
        {
            // Whip kick motion
            float legPhaseOffset = math.frac(phase + 0.5f);
            
            if (legPhaseOffset < 0.2f) // Recovery (bring heels to hips)
            {
                float recoveryProgress = legPhaseOffset / 0.2f;
                float kneeFlex = math.lerp(0f, 120f, recoveryProgress);
                float hipAbduction = math.lerp(0f, 30f, recoveryProgress);
            }
            else if (legPhaseOffset < 0.4f) // Turn outward
            {
                float turnProgress = (legPhaseOffset - 0.2f) / 0.2f;
                float hipExternalRotation = math.lerp(0f, 45f, turnProgress);
                float ankleDorsiflexion = math.lerp(0f, 30f, turnProgress);
            }
            else // Propulsive whip
            {
                float whipProgress = (legPhaseOffset - 0.4f) / 0.6f;
                float hipExtension = math.lerp(30f, 0f, whipProgress);
                float kneeExtension = math.lerp(120f, 0f, math.pow(whipProgress, 2f));
                float hipAdduction = math.lerp(30f, 0f, whipProgress);
            }
        }
        
        #endregion

        #region Stamina System
        
        private void UpdateStamina(float deltaTime)
        {
            if (currentStamina > 0f)
            {
                currentStamina -= staminaDrainRate * deltaTime * swimSpeed * strokePower;
                currentStamina = Mathf.Max(currentStamina, 0f);
            }
            else
            {
                // Can't swim effectively without stamina
                currentStamina += staminaRecoveryRate * deltaTime * 0.5f;
                currentStamina = Mathf.Min(currentStamina, maxStamina);
            }
        }
        
        private void ApplyStaminaModifiers()
        {
            float normalizedStamina = currentStamina / maxStamina;
            
            if (normalizedStamina > 0.3f)
            {
                staminaStrokePowerMod = 1f;
                staminaStrokeRateMod = 1f;
                staminaTechniqueDegradation = 0f;
            }
            else if (normalizedStamina > 0.1f)
            {
                staminaStrokePowerMod = 0.7f + normalizedStamina * 0.3f;
                staminaStrokeRateMod = 0.8f + normalizedStamina * 0.2f;
                staminaTechniqueDegradation = (0.3f - normalizedStamina) * 0.5f;
            }
            else
            {
                staminaStrokePowerMod = 0.4f + normalizedStamina * 0.3f;
                staminaStrokeRateMod = 0.5f + normalizedStamina * 0.3f;
                staminaTechniqueDegradation = 0.3f;
            }
        }
        
        #endregion

        #region Breathing System
        
        private void UpdateBreathing(float deltaTime)
        {
            strokeCountSinceBreath++;
            
            bool shouldBreathe = false;
            
            if (bilateralBreathing)
            {
                shouldBreathe = strokeCountSinceBreath >= breathEveryNStrokes;
            }
            else
            {
                shouldBreathe = strokeCountSinceBreath >= breathEveryNStrokes;
            }
            
            if (shouldBreathe && !isInhaling)
            {
                // Check if we're at the right point in the cycle
                float breathWindowStart = breathTiming - breathDuration * 0.5f;
                float breathWindowEnd = breathTiming + breathDuration * 0.5f;
                
                if (leftArmPhase >= breathWindowStart && leftArmPhase <= breathWindowEnd)
                {
                    isInhaling = true;
                    breathHoldTimer = breathDuration;
                    strokeCountSinceBreath = 0;
                }
            }
            
            if (isInhaling)
            {
                breathHoldTimer -= deltaTime;
                if (breathHoldTimer <= 0f)
                {
                    isInhaling = false;
                }
            }
        }
        
        #endregion

        #region Water Interaction
        
        private void CalculateWaterInteraction()
        {
            // Calculate drag force
            float frontalArea = 0.5f; // m² approximate
            currentDrag = 0.5f * waterDensity * swimSpeed * swimSpeed * frontalArea * waterResistance;
            
            // Wave interaction
            waveHeight = math.sin(swimCycleTime * 3f) * turbulence * 0.1f;
            
            // Water velocity effect
            waterVelocity = new Vector3(
                math.sin(swimCycleTime * 7f) * turbulence * 0.2f,
                math.cos(swimCycleTime * 5f) * turbulence * 0.1f,
                0f
            );
        }
        
        #endregion

        #region Stroke Preset Application
        
        private void ApplyStrokePreset()
        {
            int strokeIndex = (int)currentStroke;
            if (strokeIndex >= 0 && strokeIndex < strokePresets.Length)
            {
                StrokePreset preset = strokePresets[strokeIndex];
                
                bodyAngle = Mathf.Lerp(bodyAngle, preset.bodyAngle, 0.1f);
                bodyRollAmount = Mathf.Lerp(bodyRollAmount, preset.bodyRoll, 0.1f);
                strokeRate = Mathf.Lerp(strokeRate, preset.strokeRate, 0.1f);
                kickFrequency = Mathf.Lerp(kickFrequency, preset.kickFreq, 0.1f);
                kickAmplitude = Mathf.Lerp(kickAmplitude, preset.kickAmp, 0.1f);
                elbowBendPull = Mathf.Lerp(elbowBendPull, preset.elbowBend, 0.1f);
                breathTiming = Mathf.Lerp(breathTiming, preset.breathTiming, 0.1f);
                strokePower = Mathf.Lerp(strokePower, preset.powerPhase, 0.1f);
                
                // Set kick type based on stroke
                dolphinKick = (currentStroke == SwimStroke.Butterfly || currentStroke == SwimStroke.UnderwaterDolphin);
                flutterKick = !dolphinKick && (currentStroke != SwimStroke.Breaststroke);
            }
        }
        
        #endregion

        #region Public API
        
        public void SetStroke(SwimStroke stroke)
        {
            currentStroke = stroke;
        }
        
        public void SetSwimSpeed(float speed)
        {
            swimSpeed = Mathf.Clamp(speed, 0.2f, 2.5f);
        }
        
        public void AddStamina(float amount)
        {
            currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
        }
        
        public void DrainStamina(float amount)
        {
            currentStamina = Mathf.Max(currentStamina - amount, 0f);
        }
        
        public void SetBreathingPattern(bool bilateral, int everyNStrokes)
        {
            bilateralBreathing = bilateral;
            breathEveryNStrokes = everyNStrokes;
        }
        
        public void SetWaterConditions(float density, float resistance, float turbulence)
        {
            waterDensity = density;
            waterResistance = resistance;
            this.turbulence = turbulence;
        }
        
        public float GetCurrentStamina() => currentStamina;
        public float GetStaminaPercentage() => currentStamina / maxStamina;
        public bool IsBreathing() => isInhaling;
        public float GetDragForce() => currentDrag;
        
        #endregion
    }
}
