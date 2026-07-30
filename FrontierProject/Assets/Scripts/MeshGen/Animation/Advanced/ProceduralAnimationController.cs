using UnityEngine;

namespace Frontier.MeshGen.Animation.Advanced
{
    /// <summary>
    /// Advanced procedural animation system using inverse kinematics and dynamic bone simulation.
    /// Provides realistic foot placement, look-at targeting, and secondary motion for low-poly characters.
    /// </summary>
    public class ProceduralAnimationController : MonoBehaviour
    {
        [Header("IK Settings")]
        public Transform leftFootTarget;
        public Transform rightFootTarget;
        public Transform headTarget;
        public float footRaycastDistance = 1.5f;
        public LayerMask groundLayer;
        public float ikBlendWeight = 1.0f;

        [Header("Look At Settings")]
        public Transform lookAtTarget;
        public float lookSmoothness = 10.0f;
        public float maxLookAngle = 45.0f;
        public bool clampVertical = true;

        [Header("Secondary Motion")]
        public float hipSwayAmount = 0.05f;
        public float armSwingAmount = 0.1f;
        public float headBobAmount = 0.02f;
        public float breathingAmount = 0.01f;
        public float breathingSpeed = 2.0f;

        [Header("References")]
        public Animator animator;
        public Transform hips;
        public Transform spine;
        public Transform head;
        public Transform leftHand;
        public Transform rightHand;

        private Vector3 originalHeadPosition;
        private Quaternion originalHeadRotation;
        private float walkCycleTime;
        private bool isGrounded;

        private void Start()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (hips == null) hips = animator.GetBoneTransform(HumanoidBones.Hips);
            if (spine == null) spine = animator.GetBoneTransform(HumanoidBones.Spine);
            if (head == null) head = animator.GetBoneTransform(HumanoidBones.Head);
            if (leftHand == null) leftHand = animator.GetBoneTransform(HumanoidBones.LeftHand);
            if (rightHand == null) rightHand = animator.GetBoneTransform(HumanoidBones.RightHand);

            if (head != null)
            {
                originalHeadPosition = head.localPosition;
                originalHeadRotation = head.localRotation;
            }
        }

        private void LateUpdate()
        {
            UpdateSecondaryMotion();
            UpdateLookAt();
            UpdateFootIK();
        }

        private void UpdateSecondaryMotion()
        {
            if (hips == null || head == null) return;

            float time = Time.time;
            float speed = animator.velocity.magnitude;
            bool isMoving = speed > 0.1f;

            // Breathing
            float breath = Mathf.Sin(time * breathingSpeed) * breathingAmount;
            if (spine != null)
                spine.localPosition += Vector3.up * breath;

            // Head bob when moving
            if (isMoving && isGrounded)
            {
                walkCycleTime += Time.deltaTime * (speed * 2.0f);
                float bob = Mathf.Sin(walkCycleTime * 2.0f) * headBobAmount * (speed / 5.0f);
                head.localPosition = originalHeadPosition + Vector3.up * bob;
            }
            else
            {
                walkCycleTime = Mathf.Lerp(walkCycleTime, 0, Time.deltaTime * 5.0f);
            }

            // Hip sway
            if (isMoving)
            {
                float sway = Mathf.Sin(walkCycleTime) * hipSwayAmount;
                hips.localPosition += Vector3.right * sway;
            }
        }

        private void UpdateLookAt()
        {
            if (head == null || lookAtTarget == null) return;

            Vector3 directionToTarget = (lookAtTarget.position - head.position).normalized;
            Vector3 localDirection = head.InverseTransformDirection(directionToTarget);

            // Clamp angles
            float horizontalAngle = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
            float verticalAngle = Mathf.Asin(localDirection.y) * Mathf.Rad2Deg;

            if (clampVertical)
                verticalAngle = Mathf.Clamp(verticalAngle, -maxLookAngle, maxLookAngle);
            horizontalAngle = Mathf.Clamp(horizontalAngle, -maxLookAngle, maxLookAngle);

            Quaternion targetRotation = Quaternion.Euler(-verticalAngle, horizontalAngle, 0);
            head.localRotation = Quaternion.Slerp(originalHeadRotation, targetRotation, Time.deltaTime * lookSmoothness);
        }

        private void UpdateFootIK()
        {
            if (ikBlendWeight <= 0 || animator == null) return;

            // Raycast for ground detection
            RaycastHit hitLeft, hitRight;
            bool leftGrounded = Physics.Raycast(leftFootTarget.position, Vector3.down, out hitLeft, footRaycastDistance, groundLayer);
            bool rightGrounded = Physics.Raycast(rightFootTarget.position, Vector3.down, out hitRight, footRaycastDistance, groundLayer);

            isGrounded = leftGrounded || rightGrounded;

            // Apply IK offsets (simplified - full implementation would use AvatarIKGoals)
            // This is a placeholder for Unity's built-in OnAnimatorIK system
            if (leftGrounded)
            {
                // In a full implementation, we'd adjust the foot position to match hitLeft.point
                Debug.DrawLine(leftFootTarget.position, hitLeft.point, Color.green);
            }
            if (rightGrounded)
            {
                Debug.DrawLine(rightFootTarget.position, hitRight.point, Color.green);
            }
        }

        // Unity's built-in IK callback
        private void OnAnimatorIK(float weight)
        {
            if (animator == null || ikBlendWeight <= 0) return;

            float finalWeight = weight * ikBlendWeight;

            // Set look at
            if (lookAtTarget != null && head != null)
            {
                animator.SetLookAtWeight(finalWeight, 0.5f, 0.5f, 0.5f, 0.5f);
                animator.SetLookAtPosition(lookAtTarget.position);
            }

            // Set foot positions
            if (leftFootTarget != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, finalWeight);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, finalWeight);
                animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootTarget.position);
                animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootTarget.rotation);
            }

            if (rightFootTarget != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, finalWeight);
                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, finalWeight);
                animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootTarget.position);
                animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootTarget.rotation);
            }

            // Set hand positions if targets exist
            if (leftHand != null && leftHand.hasChanged)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, finalWeight * 0.8f);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, finalWeight * 0.8f);
            }
        }
    }
}
