using UnityEngine;

namespace Frontier.Animation
{
    /// <summary>
    /// 2D velocity blend trees with encumbrance handling.
    /// Blends walk/run/sprint based on speed and direction.
    /// </summary>
    public class LocomotionBlend : MonoBehaviour
    {
        private Animator _animator;
        
        [Header("Parameters")]
        public string velocityXParam = "VelocityX";
        public string velocityYParam = "VelocityY";
        public string speedParam = "Speed";
        public string encumbranceParam = "Encumbrance";
        
        [Header("Settings")]
        public float walkSpeed = 1.5f;
        public float runSpeed = 4f;
        public float sprintSpeed = 7f;
        public float rotationSmoothTime = 0.1f;
        
        [Header("Encumbrance")]
        public float maxCarryWeight = 50f;
        public float currentCarryWeight = 0f;

        private Vector3 _velocity;
        private float _rotationVelocity;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void UpdateLocomotion(Vector3 moveDirection, bool isGrounded)
        {
            if (_animator == null) return;

            // Calculate velocity in local space
            Vector3 localVel = transform.InverseTransformDirection(_velocity);
            
            // Normalize for blend tree (X = strafe, Y = forward)
            float velMagnitude = _velocity.magnitude;
            float normalizedMagnitude = Mathf.Clamp01(velMagnitude / sprintSpeed);

            if (isGrounded)
            {
                float targetVelX = moveDirection.x * normalizedMagnitude;
                float targetVelY = moveDirection.z * normalizedMagnitude;

                _animator.SetFloat(velocityXParam, targetVelX, 0.1f, Time.deltaTime);
                _animator.SetFloat(velocityYParam, targetVelY, 0.1f, Time.deltaTime);
                _animator.SetFloat(speedParam, velMagnitude, 0.1f, Time.deltaTime);
            }
            else
            {
                // Airborne - reset ground velocities
                _animator.SetFloat(velocityXParam, 0f, 0.1f, Time.deltaTime);
                _animator.SetFloat(velocityYParam, 0f, 0.1f, Time.deltaTime);
            }

            // Update encumbrance
            float encumbrance = currentCarryWeight / maxCarryWeight;
            _animator.SetFloat(encumbranceParam, encumbrance, 0.2f, Time.deltaTime);

            // Apply movement speed reduction based on encumbrance
            float speedMultiplier = 1f - (encumbrance * 0.5f); // Max 50% reduction
        }

        public void SetVelocity(Vector3 velocity)
        {
            _velocity = velocity;
        }

        public void AddToCarryWeight(float weight)
        {
            currentCarryWeight = Mathf.Min(maxCarryWeight, currentCarryWeight + weight);
        }

        public void RemoveFromCarryWeight(float weight)
        {
            currentCarryWeight = Mathf.Max(0f, currentCarryWeight - weight);
        }

        public void SetSprinting(bool isSprinting)
        {
            if (_animator != null)
            {
                _animator.SetBool("IsSprinting", isSprinting);
            }
        }

        public void SetCrouching(bool isCrouching)
        {
            if (_animator != null)
            {
                _animator.SetBool("IsCrouching", isCrouching);
            }
        }

        public float GetCurrentSpeed()
        {
            return _velocity.magnitude;
        }

        public float GetEffectiveMaxSpeed()
        {
            float encumbrance = currentCarryWeight / maxCarryWeight;
            return sprintSpeed * (1f - (encumbrance * 0.5f));
        }
    }
}
