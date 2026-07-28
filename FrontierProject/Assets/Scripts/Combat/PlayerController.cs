using UnityEngine;
using System.Collections.Generic;

namespace Frontier.Combat
{
    /// <summary>
    /// Player controller with WASD + mouse aim, top-down camera support.
    /// Handles movement, aiming, and basic interaction.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 4f;
        public float runSpeed = 7f;
        public float crouchSpeed = 2f;
        public float acceleration = 10f;
        public float deceleration = 8f;

        [Header("Aiming")]
        public float aimSensitivity = 2f;
        public bool useTopDownCamera = true;
        public float topDownAngle = 60f;

        [Header("Camera")]
        public Transform cameraTarget;
        public float cameraDistance = 8f;
        public float cameraHeight = 5f;
        public float cameraSmoothTime = 0.2f;

        [Header("State")]
        public bool isCrouching;
        public bool isSprinting;
        public bool isAiming;

        private Vector3 _velocity;
        private Vector3 _moveDirection;
        private float _currentSpeed;
        private Quaternion _targetRotation;
        private Vector3 _cameraVelocity;

        private void Update()
        {
            HandleInput();
            HandleMovement();
            HandleAiming();
            HandleCamera();
        }

        private void HandleInput()
        {
            // Movement input
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            _moveDirection = new Vector3(h, 0, v).normalized;

            // Sprint
            isSprinting = Input.GetKey(KeyCode.LeftShift) && !isCrouching;
            
            // Crouch
            isCrouching = Input.GetKey(KeyCode.LeftControl);

            // Aim
            isAiming = Input.GetMouseButton(1); // Right click

            // Calculate current speed
            if (isCrouching)
                _currentSpeed = crouchSpeed;
            else if (isSprinting)
                _currentSpeed = runSpeed;
            else
                _currentSpeed = walkSpeed;
        }

        private void HandleMovement()
        {
            if (_moveDirection.magnitude > 0)
            {
                // Convert input to world direction (relative to camera)
                Vector3 camForward = Camera.main.transform.forward;
                camForward.y = 0;
                camForward.Normalize();
                Vector3 camRight = Camera.main.transform.right;
                camRight.y = 0;
                camRight.Normalize();

                Vector3 targetMoveDir = (camForward * _moveDirection.z + camRight * _moveDirection.x).normalized;

                // Accelerate towards target direction
                _velocity = Vector3.Lerp(_velocity, targetMoveDir * _currentSpeed, acceleration * Time.deltaTime);
            }
            else
            {
                // Decelerate
                _velocity = Vector3.Lerp(_velocity, Vector3.zero, deceleration * Time.deltaTime);
            }

            // Apply movement
            transform.position += _velocity * Time.deltaTime;

            // Rotate character towards movement direction when moving
            if (_velocity.magnitude > 0.1f)
            {
                _targetRotation = Quaternion.LookRotation(_velocity);
                transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, 10f * Time.deltaTime);
            }
        }

        private void HandleAiming()
        {
            if (isAiming)
            {
                // Mouse aim
                float mouseX = Input.GetAxis("Mouse X") * aimSensitivity;
                float mouseY = Input.GetAxis("Mouse Y") * aimSensitivity;

                transform.Rotate(Vector3.up, mouseX);
                
                if (cameraTarget != null)
                {
                    Vector3 currentEuler = cameraTarget.eulerAngles;
                    currentEuler.x = Mathf.Clamp(currentEuler.x - mouseY, -45f, 10f);
                    cameraTarget.eulerAngles = currentEuler;
                }
            }
        }

        private void HandleCamera()
        {
            if (useTopDownCamera && cameraTarget != null)
            {
                Vector3 targetPos = transform.position + Vector3.up * cameraHeight;
                targetPos -= transform.forward * cameraDistance * Mathf.Sin(topDownAngle * Mathf.Deg2Rad);
                targetPos.y = transform.position.y + cameraDistance * Mathf.Cos(topDownAngle * Mathf.Deg2Rad);

                Camera.main.transform.position = Vector3.SmoothDamp(
                    Camera.main.transform.position, 
                    targetPos, 
                    ref _cameraVelocity, 
                    cameraSmoothTime
                );
                Camera.main.transform.LookAt(transform.position + Vector3.up * 1f);
            }
        }

        public Vector3 GetVelocity() => _velocity;
        public bool IsMoving() => _velocity.magnitude > 0.1f;
        public Vector3 GetMoveDirection() => _moveDirection;

        public void SetCameraDistance(float distance)
        {
            cameraDistance = Mathf.Clamp(distance, 3f, 15f);
        }
    }
}
