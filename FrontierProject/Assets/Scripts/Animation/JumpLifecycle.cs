using UnityEngine;

namespace Frontier.Animation
{
    /// <summary>
    /// Procedural jump lifecycle: launch, apex, landing phases.
    /// Handles jump anticipation and landing recovery animations.
    /// </summary>
    public class JumpLifecycle : MonoBehaviour
    {
        private Animator _animator;
        private CharacterController _controller;

        [Header("Jump Parameters")]
        public string jumpTriggerParam = "Jump";
        public string groundDistanceParam = "GroundDistance";
        public string verticalVelocityParam = "VerticalVelocity";
        
        [Header("Settings")]
        public float jumpForce = 5f;
        public float gravity = -9.81f;
        public float groundCheckDistance = 0.2f;
        
        [Header("Landing")]
        public float hardLandingThreshold = -8f;
        public float softLandingThreshold = -3f;

        private Vector3 _velocity;
        private bool _isInAir;
        private bool _wasInAir;
        private float _timeInAir;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (_controller == null || _animator == null) return;

            _wasInAir = _isInAir;
            
            // Check if grounded
            bool isGrounded = _controller.isGrounded;
            _isInAir = !isGrounded;

            // Apply gravity
            if (_isInAir)
            {
                _velocity.y += gravity * Time.deltaTime;
                _timeInAir += Time.deltaTime;
            }
            else
            {
                // Just landed
                if (_wasInAir)
                {
                    HandleLanding();
                    _timeInAir = 0f;
                }
                
                // Reset vertical velocity when grounded
                if (_velocity.y < 0)
                {
                    _velocity.y = -2f; // Small downward force to keep grounded
                }
            }

            // Update animator parameters
            _animator.SetFloat(verticalVelocityParam, _velocity.y, 0.1f, Time.deltaTime);
            _animator.SetFloat(groundDistanceParam, GetGroundDistance(), 0.1f, Time.deltaTime);
            _animator.SetBool("IsInAir", _isInAir);
        }

        public void TryJump()
        {
            if (_controller != null && _controller.isGrounded)
            {
                _velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
                _animator.SetTrigger(jumpTriggerParam);
                
                // Set jump phase
                _animator.SetInteger("JumpPhase", 0); // Launch
            }
        }

        private void HandleLanding()
        {
            // Determine landing type based on impact velocity
            if (_velocity.y < hardLandingThreshold)
            {
                // Hard landing - play stumble/recovery
                _animator.SetTrigger("HardLand");
                _animator.SetInteger("JumpPhase", 3); // Recovery
            }
            else if (_velocity.y < softLandingThreshold)
            {
                // Medium landing
                _animator.SetTrigger("MediumLand");
                _animator.SetInteger("JumpPhase", 2); // Land
            }
            else
            {
                // Soft landing - just transition to idle/run
                _animator.SetInteger("JumpPhase", 2); // Land
            }
        }

        private float GetGroundDistance()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance + 0.5f))
            {
                return hit.distance;
            }
            return groundCheckDistance + 0.5f;
        }

        public void AddVerticalForce(float force)
        {
            _velocity.y += force;
        }

        public bool IsInAir() => _isInAir;
        public float GetTimeInAir() => _timeInAir;
        public float GetVerticalVelocity() => _velocity.y;
    }
}
