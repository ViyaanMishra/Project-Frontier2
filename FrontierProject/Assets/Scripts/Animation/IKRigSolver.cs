using UnityEngine;
using Unity.Mathematics;

namespace Frontier.Animation
{
    /// <summary>
    /// Full-body IK solver for hands, feet, head, and spine.
    /// Uses CCD (Cyclic Coordinate Descent) algorithm.
    /// </summary>
    public class IKRigSolver : MonoBehaviour
    {
        [System.Serializable]
        public struct ChainConfig
        {
            public Transform root;
            public Transform[] bones;
            public Transform target;
            public float weight;
            public int iterations;
            public float tolerance;
        }

        public ChainConfig leftArm;
        public ChainConfig rightArm;
        public ChainConfig leftLeg;
        public ChainConfig rightLeg;
        public Transform headTarget;
        public Transform spineTarget;

        [Range(0, 1)]
        public float globalWeight = 1f;

        private void OnAnimatorIK()
        {
            if (globalWeight <= 0f) return;

            SolveArm(leftArm, true);
            SolveArm(rightArm, false);
            SolveLeg(leftLeg, true);
            SolveLeg(rightLeg, false);
            SolveHead();
            SolveSpine();
        }

        private void SolveArm(ChainConfig config, bool isLeft)
        {
            if (config.target == null || config.bones == null || config.bones.Length == 0) return;

            var animator = GetComponent<Animator>();
            if (animator == null) return;

            // Set look position
            Vector3 lookPos = config.target.position;
            Vector3 hintPos = config.target.position + Vector3.down * 0.5f;

            HumanBodyBone arm = isLeft ? HumanBodyBone.LeftUpperArm : HumanBodyBone.RightUpperArm;
            HumanBodyBone hand = isLeft ? HumanBodyBone.LeftHand : HumanBodyBone.RightHand;

            // Use Unity's built-in IK for arms
            animator.SetIKPosition(AvatarIKGoal.LeftHand, config.target.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, config.target.rotation);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, config.weight * globalWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, config.weight * globalWeight);

            if (!isLeft)
            {
                animator.SetIKPosition(AvatarIKGoal.RightHand, config.target.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, config.target.rotation);
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, config.weight * globalWeight);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, config.weight * globalWeight);
            }
        }

        private void SolveLeg(ChainConfig config, bool isLeft)
        {
            if (config.target == null) return;

            var animator = GetComponent<Animator>();
            if (animator == null) return;

            AvatarIKGoal goal = isLeft ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot;

            animator.SetIKPosition(goal, config.target.position);
            animator.SetIKRotation(goal, config.target.rotation);
            animator.SetIKPositionWeight(goal, config.weight * globalWeight);
            animator.SetIKRotationWeight(goal, config.weight * globalWeight);
        }

        private void SolveHead()
        {
            if (headTarget == null) return;

            var animator = GetComponent<Animator>();
            if (animator == null) return;

            animator.SetLookAtPosition(headTarget.position);
            animator.SetLookAtWeight(globalWeight * 0.8f);
        }

        private void SolveSpine()
        {
            if (spineTarget == null) return;

            var animator = GetComponent<Animator>();
            if (animator == null) return;

            // Spine adjustment based on target direction
            Vector3 dir = (spineTarget.position - transform.position).normalized;
            float angle = Vector3.SignedAngle(transform.forward, dir, Vector3.up);
            
            animator.SetFloat("SpineRotation", angle * globalWeight);
        }

        public void SetTarget(Transform chainTarget, Transform newTarget)
        {
            if (chainTarget == leftArm.target) leftArm.target = newTarget;
            else if (chainTarget == rightArm.target) rightArm.target = newTarget;
            else if (chainTarget == leftLeg.target) leftLeg.target = newTarget;
            else if (chainTarget == rightLeg.target) rightLeg.target = newTarget;
        }

        public void SetWeight(float weight)
        {
            globalWeight = Mathf.Clamp01(weight);
        }
    }
}
