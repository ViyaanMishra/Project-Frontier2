using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Frontier.MeshGen.Animation
{
    /// <summary>
    /// Procedural jump and landing animation generator with physics-based motion and fall damage calculation
    /// </summary>
    public class JumpAnimationGen : ComponentSystem
    {
        public struct JumpInstance
        {
            public Entity entity;
            public float startTime;
            public float currentTime;
            public float jumpDuration;
            public Vector3 startPosition;
            public Vector3 apexPosition;
            public Vector3 endPosition;
            public float initialVelocity;
            public float currentVelocity;
            public bool isAscending;
            public bool isGrounded;
            public float fallDistance;
            public JumpType jumpType;
            public LandingType landingType;
        }
        
        public enum JumpType { Normal, Running, Crouching, Backward, Sideways, Vault, Drop }
        public enum LandingType { Soft, Medium, Hard, Crash, Roll }
        
        private NativeList<JumpInstance> _activeJumps;
        private float gravity = -9.81f;
        
        protected override void OnCreate()
        {
            _activeJumps = new NativeList<JumpInstance>(Allocator.Persistent);
        }
        
        protected override void OnDestroy()
        {
            _activeJumps.Dispose();
        }
        
        public int StartJump(Entity entity, JumpType jumpType, float initialVelocity, Vector3 direction)
        {
            var instance = new JumpInstance
            {
                entity = entity,
                startTime = Time.time,
                currentTime = 0f,
                jumpDuration = 0f,
                startPosition = Vector3.zero,
                apexPosition = Vector3.zero,
                endPosition = Vector3.zero,
                initialVelocity = initialVelocity,
                currentVelocity = initialVelocity,
                isAscending = true,
                isGrounded = false,
                fallDistance = 0f,
                jumpType = jumpType,
                landingType = LandingType.Soft
            };
            
            _activeJumps.Add(instance);
            return _activeJumps.Length - 1;
        }
        
        public void UpdateJump(int jumpIndex, float deltaTime, Vector3 currentPosition)
        {
            if (jumpIndex < 0 || jumpIndex >= _activeJumps.Length) return;
            
            var jump = _activeJumps[jumpIndex];
            if (jump.isGrounded) return;
            
            jump.currentTime += deltaTime;
            
            // Calculate vertical velocity
            jump.currentVelocity = jump.initialVelocity + (gravity * jump.currentTime);
            
            // Calculate current height
            float deltaY = (jump.initialVelocity * jump.currentTime) + (0.5f * gravity * jump.currentTime * jump.currentTime);
            
            // Track apex
            if (jump.isAscending && jump.currentVelocity <= 0)
            {
                jump.isAscending = false;
                jump.apexPosition = currentPosition + Vector3.up * deltaY;
                jump.fallDistance = 0f;
            }
            
            // Track fall distance for landing calculation
            if (!jump.isAscending)
            {
                jump.fallDistance += math.abs(jump.currentVelocity * deltaTime);
            }
            
            _activeJumps[jumpIndex] = jump;
        }
        
        public void Land(int jumpIndex, float groundHeight)
        {
            if (jumpIndex < 0 || jumpIndex >= _activeJumps.Length) return;
            
            var jump = _activeJumps[jumpIndex];
            if (jump.isGrounded) return;
            
            jump.isGrounded = true;
            jump.endPosition = new Vector3(jump.startPosition.x, groundHeight, jump.startPosition.z);
            
            // Determine landing type based on fall distance and velocity
            float impactVelocity = math.abs(jump.currentVelocity);
            if (impactVelocity > 15f)
                jump.landingType = LandingType.Crash;
            else if (impactVelocity > 10f)
                jump.landingType = LandingType.Hard;
            else if (impactVelocity > 5f)
                jump.landingType = LandingType.Medium;
            else
                jump.landingType = LandingType.Soft;
            
            _activeJumps[jumpIndex] = jump;
        }
        
        public float GetFallDamage(int jumpIndex, float mass = 70f)
        {
            if (jumpIndex < 0 || jumpIndex >= _activeJumps.Length) return 0f;
            
            var jump = _activeJumps[jumpIndex];
            float impactVelocity = math.abs(jump.currentVelocity);
            
            // Simplified fall damage calculation
            float threshold = 8f; // Velocity threshold before damage starts
            if (impactVelocity <= threshold) return 0f;
            
            float damage = (impactVelocity - threshold) * mass * 0.1f;
            
            // Apply landing type modifier
            switch (jump.landingType)
            {
                case LandingType.Roll:
                    damage *= 0.3f;
                    break;
                case LandingType.Soft:
                    damage *= 0.5f;
                    break;
                case LandingType.Medium:
                    damage *= 0.8f;
                    break;
            }
            
            return damage;
        }
        
        public Quaternion GetBodyRotation(int jumpIndex)
        {
            if (jumpIndex < 0 || jumpIndex >= _activeJumps.Length) return Quaternion.identity;
            
            var jump = _activeJumps[jumpIndex];
            
            // Calculate body rotation based on jump phase
            if (jump.isAscending)
            {
                float t = jump.currentTime / (jump.initialVelocity / math.abs(gravity));
                return Quaternion.Euler(-15f * t, 0f, 0f); // Lean forward during ascent
            }
            else
            {
                // Prepare for landing
                switch (jump.landingType)
                {
                    case LandingType.Crash:
                        return Quaternion.Euler(-45f, 0f, 0f);
                    case LandingType.Hard:
                        return Quaternion.Euler(-30f, 0f, 0f);
                    default:
                        return Quaternion.Euler(-10f, 0f, 0f);
                }
            }
        }
        
        public Vector3 GetLegPositions(int jumpIndex)
        {
            if (jumpIndex < 0 || jumpIndex >= _activeJumps.Length) return Vector3.zero;
            
            var jump = _activeJumps[jumpIndex];
            
            if (jump.isAscending)
            {
                // Tuck legs during ascent
                return new Vector3(0.3f, 0.5f, 0.3f); // knee bend values
            }
            else
            {
                // Extend legs for landing
                float t = math.clamp(jump.fallDistance / 10f, 0f, 1f);
                return new Vector3(math.lerp(0.5f, 0.2f, t), 
                                  math.lerp(0.3f, 0.1f, t), 
                                  math.lerp(0.5f, 0.2f, t));
            }
        }
        
        public void CancelJump(int jumpIndex)
        {
            if (jumpIndex < 0 || jumpIndex >= _activeJumps.Length) return;
            
            var jump = _activeJumps[jumpIndex];
            jump.isGrounded = true;
            _activeJumps[jumpIndex] = jump;
        }
    }
}
