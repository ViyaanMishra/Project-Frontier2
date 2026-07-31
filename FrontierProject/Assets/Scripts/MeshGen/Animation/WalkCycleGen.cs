using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Frontier.MeshGen.Animation
{
    /// <summary>
    /// Procedural walk cycle generator with adaptive stride, terrain adaptation, and injury simulation
    /// </summary>
    public class WalkCycleGen : ComponentSystem
    {
        public enum WalkStyle { Normal, Run, Sprint, Crouch, Sneak, Limp, Exhausted, CarryingHeavy }
        
        [System.Serializable]
        public struct WalkCycleParams
        {
            public float strideLength;
            public float stepHeight;
            public float hipWidth;
            public float kneeBend;
            public float armSwing;
            public float torsoRotation;
            public float headBob;
            public float footRoll;
            public float speed;
            public WalkStyle style;
            public float fatigueLevel;
            public float injuryFactor;
            public float terrainSlope;
            public float groundUnevenness;
        }
        
        public struct WalkCycleInstance
        {
            public Entity entity;
            public WalkCycleParams parameters;
            public float cycleTime;
            public float leftLegPhase;
            public float rightLegPhase;
            public float leftArmPhase;
            public float rightArmPhase;
            public Vector3 rootPosition;
            public Quaternion rootRotation;
            public float verticalOffset;
            public float currentSpeed;
            public bool isGrounded;
            public float slopeAngle;
        }
        
        private NativeList<WalkCycleInstance> _activeCycles;
        private AnimationCurve _headBobCurve;
        private AnimationCurve _footLiftCurve;
        private AnimationCurve _armSwingCurve;
        
        protected override void OnCreate()
        {
            _activeCycles = new NativeList<WalkCycleInstance>(Allocator.Persistent);
            
            _headBobCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.25f, 1f),
                new Keyframe(0.5f, 0f),
                new Keyframe(0.75f, -1f),
                new Keyframe(1f, 0f)
            );
            
            _footLiftCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.2f, 0f),
                new Keyframe(0.35f, 1f),
                new Keyframe(0.5f, 0f),
                new Keyframe(1f, 0f)
            );
            
            _armSwingCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.25f, 1f),
                new Keyframe(0.5f, 0f),
                new Keyframe(0.75f, -1f),
                new Keyframe(1f, 0f)
            );
        }
        
        protected override void OnDestroy()
        {
            _activeCycles.Dispose();
        }
        
        public int CreateWalkCycle(Entity entity, WalkCycleParams initialParams)
        {
            var cycle = new WalkCycleInstance
            {
                entity = entity,
                parameters = initialParams,
                cycleTime = 0f,
                leftLegPhase = 0f,
                rightLegPhase = 0.5f,
                leftArmPhase = 0.5f,
                rightArmPhase = 0f,
                rootPosition = Vector3.zero,
                rootRotation = Quaternion.identity,
                verticalOffset = 0f,
                currentSpeed = 0f,
                isGrounded = true,
                slopeAngle = 0f
            };
            
            _activeCycles.Add(cycle);
            return _activeCycles.Length - 1;
        }
        
        public void UpdateWalkCycle(int index, float deltaTime, Vector3 targetVelocity, float groundSlope)
        {
            if (index < 0 || index >= _activeCycles.Length) return;
            
            var cycle = _activeCycles[index];
            var p = cycle.parameters;
            
            // Calculate speed factor based on velocity magnitude
            float targetSpeed = targetVelocity.magnitude;
            float speedFactor = math.clamp(targetSpeed / (p.speed + 0.001f), 0f, 2f);
            cycle.currentSpeed = math.lerp(cycle.currentSpeed, targetSpeed, deltaTime * 5f);
            
            // Adjust parameters based on walk style
            ApplyWalkStyle(ref p, cycle.currentSpeed, speedFactor);
            
            // Apply fatigue and injury modifiers
            float fatigueMod = 1f - (p.fatigueLevel * 0.3f);
            float injuryMod = 1f - (p.injuryFactor * 0.5f);
            
            // Update cycle time
            float adjustedSpeed = p.speed * speedFactor * fatigueMod * injuryMod;
            cycle.cycleTime += deltaTime * adjustedSpeed;
            if (cycle.cycleTime > 1f) cycle.cycleTime -= 1f;
            
            // Calculate phase positions
            cycle.leftLegPhase = (cycle.cycleTime) % 1f;
            cycle.rightLegPhase = (cycle.cycleTime + 0.5f) % 1f;
            cycle.leftArmPhase = (cycle.leftLegPhase + 0.5f) % 1f;
            cycle.rightArmPhase = cycle.leftLegPhase;
            
            // Terrain adaptation
            cycle.slopeAngle = groundSlope;
            float slopeMod = math.cos(math.radians(groundSlope));
            p.strideLength *= slopeMod;
            p.stepHeight *= math.lerp(1f, 1.5f, math.max(0, groundSlope) / 45f);
            
            // Calculate vertical offset (head bob)
            float bobFrequency = p.style == WalkStyle.Run ? 2f : 
                                p.style == WalkStyle.Sprint ? 3f : 1f;
            cycle.verticalOffset = _headBobCurve.Evaluate(cycle.cycleTime * bobFrequency) 
                                   * p.headBob * fatigueMod;
            
            // Ground contact detection
            cycle.isGrounded = CheckGroundContact(cycle.leftLegPhase, cycle.rightLegPhase, p);
            
            // Store updated values
            cycle.parameters = p;
            _activeCycles[index] = cycle;
        }
        
        private void ApplyWalkStyle(ref WalkCycleParams p, float currentSpeed, float speedFactor)
        {
            switch (p.style)
            {
                case WalkStyle.Run:
                    p.strideLength *= 1.5f;
                    p.stepHeight *= 1.3f;
                    p.armSwing *= 1.8f;
                    p.torsoRotation *= 1.2f;
                    break;
                case WalkStyle.Sprint:
                    p.strideLength *= 2f;
                    p.stepHeight *= 1.5f;
                    p.armSwing *= 2.2f;
                    p.torsoRotation *= 1.5f;
                    p.headBob *= 1.8f;
                    break;
                case WalkStyle.Crouch:
                    p.strideLength *= 0.5f;
                    p.stepHeight *= 0.3f;
                    p.armSwing *= 0.2f;
                    p.kneeBend *= 2f;
                    break;
                case WalkStyle.Sneak:
                    p.strideLength *= 0.4f;
                    p.stepHeight *= 0.1f;
                    p.armSwing *= 0.1f;
                    p.footRoll *= 0.5f;
                    break;
                case WalkStyle.Limp:
                    p.strideLength *= 0.6f;
                    p.injuryFactor = math.max(p.injuryFactor, 0.5f);
                    break;
                case WalkStyle.Exhausted:
                    p.strideLength *= 0.7f;
                    p.armSwing *= 0.5f;
                    p.headBob *= 1.5f;
                    p.fatigueLevel = math.max(p.fatigueLevel, 0.7f);
                    break;
                case WalkStyle.CarryingHeavy:
                    p.strideLength *= 0.6f;
                    p.stepHeight *= 0.5f;
                    p.armSwing *= 0.3f;
                    p.torsoRotation *= 0.5f;
                    p.kneeBend *= 1.5f;
                    break;
            }
        }
        
        private bool CheckGroundContact(float leftPhase, float rightPhase, WalkCycleParams p)
        {
            float leftFootHeight = _footLiftCurve.Evaluate(leftPhase) * p.stepHeight;
            float rightFootHeight = _footLiftCurve.Evaluate(rightPhase) * p.stepHeight;
            return leftFootHeight < 0.1f && rightFootHeight < 0.1f;
        }
        
        public Transform GetBoneTransform(string boneName, int cycleIndex)
        {
            if (cycleIndex < 0 || cycleIndex >= _activeCycles.Length) return null;
            
            var cycle = _activeCycles[cycleIndex];
            var p = cycle.parameters;
            float time = cycle.cycleTime;
            
            // Calculate local rotations for each bone
            switch (boneName.ToLower())
            {
                case "left_upper_leg":
                    return CalculateLegRotation(time, 0f, p, true);
                case "right_upper_leg":
                    return CalculateLegRotation(time, 0.5f, p, false);
                case "left_lower_leg":
                    return CalculateLowerLegRotation(time, 0f, p, true);
                case "right_lower_leg":
                    return CalculateLowerLegRotation(time, 0.5f, p, false);
                case "left_foot":
                    return CalculateFootRotation(time, 0f, p, true);
                case "right_foot":
                    return CalculateFootRotation(time, 0.5f, p, false);
                case "left_upper_arm":
                    return CalculateArmRotation(time, 0.5f, p, true);
                case "right_upper_arm":
                    return CalculateArmRotation(time, 0f, p, false);
                case "spine":
                    return CalculateSpineRotation(time, p);
                case "head":
                    return CalculateHeadRotation(time, p, cycle);
                default:
                    return null;
            }
        }
        
        private Transform CalculateLegRotation(float time, float phaseOffset, WalkCycleParams p, bool isLeft)
        {
            float phase = (time + phaseOffset) % 1f;
            float swing = _armSwingCurve.Evaluate(phase) * p.strideLength * 0.5f;
            float lift = _footLiftCurve.Evaluate(phase) * p.stepHeight * 0.3f;
            
            // Apply injury asymmetry
            if (p.injuryFactor > 0f && !isLeft)
            {
                swing *= (1f - p.injuryFactor * 0.5f);
                lift *= (1f - p.injuryFactor * 0.7f);
            }
            
            var rotation = Quaternion.Euler(swing * 60f, 0f, lift * 10f);
            return CreateVirtualTransform(rotation);
        }
        
        private Transform CalculateLowerLegRotation(float time, float phaseOffset, WalkCycleParams p, bool isLeft)
        {
            float phase = (time + phaseOffset) % 1f;
            float bend = _footLiftCurve.Evaluate(phase) * p.kneeBend;
            
            var rotation = Quaternion.Euler(-bend * 80f, 0f, 0f);
            return CreateVirtualTransform(rotation);
        }
        
        private Transform CalculateFootRotation(float time, float phaseOffset, WalkCycleParams p, bool isLeft)
        {
            float phase = (time + phaseOffset) % 1f;
            float roll = _armSwingCurve.Evaluate(phase) * p.footRoll;
            float slopeCompensation = -p.terrainSlope * 0.5f;
            
            var rotation = Quaternion.Euler(roll * 30f + slopeCompensation, 0f, 0f);
            return CreateVirtualTransform(rotation);
        }
        
        private Transform CalculateArmRotation(float time, float phaseOffset, WalkCycleParams p, bool isLeft)
        {
            float phase = (time + phaseOffset) % 1f;
            float swing = _armSwingCurve.Evaluate(phase) * p.armSwing;
            
            var rotation = Quaternion.Euler(swing * 45f, 0f, swing * 10f);
            return CreateVirtualTransform(rotation);
        }
        
        private Transform CalculateSpineRotation(float time, WalkCycleParams p)
        {
            float rotation = _armSwingCurve.Evaluate(time) * p.torsoRotation;
            float forwardLean = p.style == WalkStyle.Run || p.style == WalkStyle.Sprint ? -15f : 0f;
            
            var rotationQuat = Quaternion.Euler(forwardLean, rotation, 0f);
            return CreateVirtualTransform(rotationQuat);
        }
        
        private Transform CalculateHeadRotation(float time, WalkCycleParams p, WalkCycleInstance cycle)
        {
            float bob = cycle.verticalOffset * 0.5f;
            float stabilization = p.style == WalkStyle.Sneak ? 0.2f : 1f;
            
            var rotation = Quaternion.Euler(bob * stabilization, 0f, 0f);
            return CreateVirtualTransform(rotation);
        }
        
        private Transform CreateVirtualTransform(Quaternion rotation)
        {
            var go = new GameObject("VirtualBone");
            go.transform.localRotation = rotation;
            return go.transform;
        }
        
        public void BlendToNewStyle(WalkStyle newStyle, float blendDuration, int cycleIndex)
        {
            if (cycleIndex < 0 || cycleIndex >= _activeCycles.Length) return;
            
            var cycle = _activeCycles[cycleIndex];
            // Implementation for smooth style blending
            // Would use animation curve interpolation over blendDuration
        }
        
        public void SetTerrainProperties(int cycleIndex, float slope, float unevenness)
        {
            if (cycleIndex < 0 || cycleIndex >= _activeCycles.Length) return;
            
            var cycle = _activeCycles[cycleIndex];
            cycle.parameters.terrainSlope = slope;
            cycle.parameters.groundUnevenness = unevenness;
            _activeCycles[cycleIndex] = cycle;
        }
    }
}
