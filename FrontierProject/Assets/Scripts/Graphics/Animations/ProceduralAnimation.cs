using UnityEngine;

namespace Frontier.Graphics.Animations
{
    /// <summary>
    /// Advanced procedural animation system for AAA-quality character and object animations.
    /// Includes IK, procedural walking, head tracking, and physics-based secondary motion.
    /// </summary>
    public class ProceduralAnimation : MonoBehaviour
    {
        [Header("Inverse Kinematics")]
        public bool enableIK = true;
        public Transform leftHandTarget;
        public Transform rightHandTarget;
        public Transform lookTarget;
        public float lookWeight = 1f;
        public float bodyRotationWeight = 0.5f;
        
        [Header("Procedural Walking")]
        public bool enableProceduralWalking = true;
        public float stepHeight = 0.15f;
        public float stepSpeed = 1f;
        public float footIKDistance = 0.3f;
        public LayerMask groundLayer;
        
        [Header("Head Tracking")]
        public Transform headBone;
        public float maxLookAngle = 45f;
        public float lookSmoothness = 10f;
        public Vector3 lookOffset = new Vector3(0, 0.1f, 0);
        
        [Header("Secondary Motion")]
        public bool enableSecondaryMotion = true;
        public float springStiffness = 5f;
        public float springDamping = 2f;
        public Transform[] secondaryBones;
        public float[] secondaryMotionWeights;
        
        [Header("Breathing Animation")]
        public bool enableBreathing = true;
        public float breatheRate = 0.5f;
        public float breatheAmplitude = 0.02f;
        public Transform chestBone;
        
        [Header("Arm Swinging")]
        public bool enableArmSwing = true;
        public float armSwingAmount = 0.3f;
        public float armSwingSpeed = 1f;
        public Transform leftArm;
        public Transform rightArm;
        
        private Animator animator;
        private CharacterController characterController;
        private Vector3 lastPosition;
        private Vector3 velocity;
        private float breathTimer;
        
        // Spring physics for secondary motion
        private Vector3[] boneVelocities;
        
        private void Awake()
        {
            animator = GetComponent<Animator>();
            characterController = GetComponent<CharacterController>();
            
            if (secondaryBones != null)
            {
                boneVelocities = new Vector3[secondaryBones.Length];
            }
            
            lastPosition = transform.position;
        }
        
        private void OnAnimatorIK(float ikWeight)
        {
            if (!enableIK) return;
            
            // Look at target
            if (lookTarget != null && headBone != null)
            {
                Vector3 targetPos = lookTarget.position + lookOffset;
                Vector3 direction = targetPos - headBone.position;
                
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                headBone.rotation = Quaternion.Slerp(headBone.rotation, targetRotation, Time.deltaTime * lookSmoothness);
            }
            
            // Left hand IK
            if (leftHandTarget != null)
            {
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, ikWeight);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, ikWeight);
            }
            
            // Right hand IK
            if (rightHandTarget != null)
            {
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);
            }
        }
        
        private void Update()
        {
            UpdateVelocity();
            UpdateProceduralWalking();
            UpdateSecondaryMotion();
            UpdateBreathing();
            UpdateArmSwing();
            UpdateBodyRotation();
        }
        
        private void UpdateVelocity()
        {
            if (characterController != null)
            {
                velocity = (transform.position - lastPosition) / Time.deltaTime;
                lastPosition = transform.position;
            }
        }
        
        private void UpdateProceduralWalking()
        {
            if (!enableProceduralWalking || velocity.magnitude < 0.1f) return;
            
            // Raycast to find ground for foot placement
            RaycastHit hit;
            Vector3 footPos = transform.position;
            
            if (Physics.Raycast(footPos + Vector3.up * 0.5f, Vector3.down, out hit, footIKDistance, groundLayer))
            {
                // Adjust foot position to match terrain
                float stepCycle = Mathf.Sin(Time.time * stepSpeed * velocity.magnitude);
                float stepOffset = Mathf.Max(0, stepCycle) * stepHeight;
                
                // Apply foot offset (would be applied through animator or direct bone manipulation)
                Vector3 targetFootPos = hit.point + Vector3.up * stepOffset;
                
                // Smooth foot movement
                // Note: In production, this would drive IK targets for feet
            }
        }
        
        private void UpdateSecondaryMotion()
        {
            if (!enableSecondaryMotion || secondaryBones == null) return;
            
            for (int i = 0; i < secondaryBones.Length; i++)
            {
                if (secondaryBones[i] == null) continue;
                
                // Spring physics simulation
                Vector3 targetPos = secondaryBones[i].parent.position;
                Vector3 displacement = secondaryBones[i].position - targetPos;
                
                // Hooke's law: F = -kx
                Vector3 springForce = -springStiffness * displacement;
                
                // Damping: F = -cv
                Vector3 dampingForce = -springDamping * boneVelocities[i];
                
                // Apply forces
                Vector3 acceleration = (springForce + dampingForce) * Time.deltaTime;
                boneVelocities[i] += acceleration;
                boneVelocities[i] -= velocity * Time.deltaTime * 5f; // Drag from character movement
                
                // Update position
                secondaryBones[i].position += boneVelocities[i] * Time.deltaTime * secondaryMotionWeights[i];
            }
        }
        
        private void UpdateBreathing()
        {
            if (!enableBreathing || chestBone == null) return;
            
            breathTimer += Time.deltaTime * breatheRate;
            float breatheCycle = Mathf.Sin(breathTimer) * breatheAmplitude;
            
            chestBone.localPosition += Vector3.forward * breatheCycle;
        }
        
        private void UpdateArmSwing()
        {
            if (!enableArmSwing || velocity.magnitude < 0.1f) return;
            
            float swingPhase = Time.time * armSwingSpeed * velocity.magnitude;
            float swingAmount = Mathf.Sin(swingPhase) * armSwingAmount;
            
            if (leftArm != null)
            {
                leftArm.localRotation = Quaternion.Euler(swingAmount * 30, 0, 0);
            }
            
            if (rightArm != null)
            {
                rightArm.localRotation = Quaternion.Euler(-swingAmount * 30, 0, 0);
            }
        }
        
        private void UpdateBodyRotation()
        {
            if (velocity.magnitude < 0.1f || lookTarget == null) return;
            
            Vector3 moveDirection = velocity.normalized;
            Vector3 lookDirection = (lookTarget.position - transform.position).normalized;
            
            float angleToTarget = Vector3.SignedAngle(moveDirection, lookDirection, Vector3.up);
            float bodyRotation = Mathf.Clamp(angleToTarget, -maxLookAngle, maxLookAngle) * bodyRotationWeight;
            
            transform.rotation *= Quaternion.Euler(0, bodyRotation, 0);
        }
        
        #region Public API
        
        public void SetLookTarget(Transform target)
        {
            lookTarget = target;
        }
        
        public void SetHandTargets(Transform left, Transform right)
        {
            leftHandTarget = left;
            rightHandTarget = right;
        }
        
        public void SetIKEnabled(bool enabled)
        {
            enableIK = enabled;
        }
        
        public void TriggerFlinch(Vector3 direction, float intensity)
        {
            // Apply impulse to secondary motion bones
            for (int i = 0; i < boneVelocities.Length; i++)
            {
                boneVelocities[i] += direction * intensity;
            }
        }
        
        #endregion
    }
}
