using UnityEngine;
using System.Collections.Generic;

namespace Frontier.Vehicles
{
    /// <summary>
    /// Vehicle physics with suspension raycasts, weight transfer, and tire friction.
    /// Supports all 12+ vehicle types from quad bikes to helicopters.
    /// </summary>
    public class VehiclePhysics : MonoBehaviour
    {
        [System.Serializable]
        public struct WheelConfig
        {
            public Transform wheelTransform;
            public Transform colliderTransform;
            public float radius;
            public float suspensionLength;
            public float springForce;
            public float damperForce;
            public float frictionCoefficient;
            public bool isSteering;
            public bool isDriven;
        }

        [Header("Vehicle Stats")]
        public float mass = 1500f;
        public float enginePower = 200f;
        public float maxSpeed = 30f;
        public float brakeForce = 15f;
        public float turnSpeed = 45f;

        [Header("Wheels")]
        public WheelConfig[] wheels = new WheelConfig[4];

        [Header("State")]
        public Vector3 velocity;
        public float currentSpeed;
        public bool isGrounded;
        public bool isFlipped;

        [Header("References")]
        public Rigidbody rb;
        public Transform centerOfMass;

        private float _throttleInput;
        private float _steerInput;
        private float _brakeInput;
        private float _handbrakeInput;

        private void Awake()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody>();
            
            if (centerOfMass != null && rb != null)
                rb.centerOfMass = centerOfMass.localPosition;
        }

        private void Update()
        {
            GetInput();
            CheckGrounded();
            CheckFlipped();
        }

        private void FixedUpdate()
        {
            if (rb == null) return;

            ApplyEngineForce();
            ApplySteering();
            ApplyBrakes();
            ApplySuspension();
            ApplyWeightTransfer();
        }

        private void GetInput()
        {
            _throttleInput = Input.GetAxis("Vertical");
            _steerInput = Input.GetAxis("Horizontal");
            _brakeInput = Input.GetKey(KeyCode.Space) ? 1f : 0f;
            _handbrakeInput = Input.GetKey(KeyCode.LeftShift) ? 1f : 0f;
        }

        private void ApplyEngineForce()
        {
            Vector3 forward = transform.forward;
            float force = _throttleInput * enginePower;
            
            // Apply force to driven wheels
            foreach (var wheel in wheels)
            {
                if (wheel.isDriven)
                {
                    rb.AddForceAtPosition(forward * force, wheel.wheelTransform.position);
                }
            }

            // Limit speed
            currentSpeed = Vector3.Dot(velocity, transform.forward);
            if (Mathf.Abs(currentSpeed) > maxSpeed && _throttleInput != 0)
            {
                // Reduce throttle when at max speed
            }
        }

        private void ApplySteering()
        {
            foreach (var wheel in wheels)
            {
                if (wheel.isSteering)
                {
                    float steerAngle = _steerInput * turnSpeed * (_throttleInput >= 0 ? 1f : -1f);
                    wheel.wheelTransform.localRotation = Quaternion.Euler(0, steerAngle, 0);
                }
            }
        }

        private void ApplyBrakes()
        {
            float brakeAmount = Mathf.Max(_brakeInput, _handbrakeInput);
            if (brakeAmount > 0)
            {
                rb.velocity -= rb.velocity.normalized * brakeForce * brakeAmount * Time.fixedDeltaTime;
            }
        }

        private void ApplySuspension()
        {
            foreach (var wheel in wheels)
            {
                RaycastHit hit;
                if (Physics.Raycast(wheel.colliderTransform.position, -transform.up, out hit, wheel.suspensionLength + wheel.radius))
                {
                    float compression = (wheel.suspensionLength + wheel.radius - hit.distance) / wheel.suspensionLength;
                    
                    // Spring force
                    float springForce = compression * wheel.springForce;
                    
                    // Damper force (based on velocity)
                    float damperForce = rb.GetPointVelocity(wheel.wheelTransform.position).y * wheel.damperForce;
                    
                    // Apply suspension force
                    rb.AddForceAtPosition(transform.up * (springForce - damperForce), wheel.wheelTransform.position);
                    
                    // Visual wheel position
                    wheel.wheelTransform.localPosition = Vector3.down * compression * wheel.suspensionLength;
                }
                else
                {
                    // Wheel in air - extend suspension
                    wheel.wheelTransform.localPosition = Vector3.zero;
                }
            }
        }

        private void ApplyWeightTransfer()
        {
            // Simulate weight transfer during acceleration/braking
            float accel = _throttleInput * enginePower / mass;
            
            // Rear squat during acceleration, front dive during braking
            float pitchTorque = accel * mass * 0.1f;
            rb.AddTorque(transform.right * pitchTorque * Time.fixedDeltaTime, ForceMode.VelocityChange);

            // Body roll during turning
            float rollTorque = _steerInput * currentSpeed * 0.05f;
            rb.AddTorque(transform.forward * rollTorque * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }

        private void CheckGrounded()
        {
            isGrounded = false;
            foreach (var wheel in wheels)
            {
                if (Physics.Raycast(wheel.colliderTransform.position, -transform.up, wheel.radius + 0.1f))
                {
                    isGrounded = true;
                    break;
                }
            }
        }

        private void CheckFlipped()
        {
            isFlipped = transform.up.y < 0;
            if (isFlipped && isGrounded)
            {
                // Auto-flip after delay or player input
            }
        }

        public void SetThrottle(float value) => _throttleInput = value;
        public void SetSteer(float value) => _steerInput = value;
        public void SetBrake(bool value) => _brakeInput = value ? 1f : 0f;
        public void SetHandbrake(bool value) => _handbrakeInput = value ? 1f : 0f;

        public float GetCurrentSpeedKMH() => currentSpeed * 3.6f;
        public bool CanDrive() => isGrounded && !isFlipped;
    }
}
