using UnityEngine;

namespace Frontier.Animation
{
    /// <summary>
    /// Aquatic locomotion: wade, swim, dive, and drown states.
    /// Handles water entry/exit transitions.
    /// </summary>
    public class AquaticLocomotion : MonoBehaviour
    {
        private Animator _animator;
        
        [Header("Water Parameters")]
        public string waterDepthParam = "WaterDepth";
        public string swimSpeedParam = "SwimSpeed";
        public string isSwimmingParam = "IsSwimming";
        public string isDivingParam = "IsDiving";
        public string oxygenParam = "Oxygen";
        
        [Header("Settings")]
        public float shallowWaterDepth = 0.5f;
        public float deepWaterDepth = 1.2f;
        public float swimSpeed = 3f;
        public float diveSpeed = 2f;
        public float maxOxygen = 100f;
        public float oxygenDepletionRate = 5f; // per second underwater
        
        [Header("State")]
        public WaterState currentState = WaterState.Dry;
        public float currentOxygen = 100f;

        public enum WaterState
        {
            Dry, Wading, Swimming, Diving, Drowning
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (_animator == null) return;

            float waterDepth = GetWaterDepth();
            
            // Determine state based on water depth
            WaterState newState;
            if (waterDepth <= 0)
                newState = WaterState.Dry;
            else if (waterDepth < shallowWaterDepth)
                newState = WaterState.Wading;
            else if (waterDepth < deepWaterDepth)
                newState = IsSubmerged() ? WaterState.Diving : WaterState.Swimming;
            else
                newState = IsSubmerged() ? WaterState.Diving : WaterState.Swimming;

            // Handle drowning
            if (newState == WaterState.Diving || newState == WaterState.Swimming && IsHeadSubmerged())
            {
                currentOxygen -= oxygenDepletionRate * Time.deltaTime;
                if (currentOxygen <= 0)
                {
                    newState = WaterState.Drowning;
                    currentOxygen = 0;
                }
            }
            else
            {
                // Recover oxygen
                currentOxygen = Mathf.Min(maxOxygen, currentOxygen + oxygenDepletionRate * Time.deltaTime);
            }

            // State transition handling
            if (newState != currentState)
            {
                HandleStateTransition(currentState, newState);
                currentState = newState;
            }

            // Update animator
            _animator.SetFloat(waterDepthParam, waterDepth, 0.2f, Time.deltaTime);
            _animator.SetFloat(swimSpeedParam, GetSwimVelocity(), 0.1f, Time.deltaTime);
            _animator.SetBool(isSwimmingParam, currentState == WaterState.Swimming);
            _animator.SetBool(isDivingParam, currentState == WaterState.Diving);
            _animator.SetFloat(oxygenParam, currentOxygen / maxOxygen);
        }

        private void HandleStateTransition(WaterState from, WaterState to)
        {
            if (from == WaterState.Dry && to == WaterState.Wading)
                _animator.SetTrigger("EnterShallowWater");
            else if (from == WaterState.Wading && to == WaterState.Swimming)
                _animator.SetTrigger("EnterDeepWater");
            else if (from == WaterState.Swimming && to == WaterState.Dry)
                _animator.SetTrigger("ExitWater");
            else if (from == WaterState.Diving && to == WaterState.Swimming)
                _animator.SetTrigger("Surface");
            else if (to == WaterState.Drowning)
                _animator.SetTrigger("StartDrowning");
        }

        public void TryDive()
        {
            if (currentState == WaterState.Swimming)
            {
                currentState = WaterState.Diving;
                _animator.SetTrigger("Dive");
            }
        }

        public void TrySurface()
        {
            if (currentState == WaterState.Diving)
            {
                currentState = WaterState.Swimming;
                _animator.SetTrigger("Surface");
            }
        }

        private float GetWaterDepth()
        {
            // Raycast down to find water surface
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 3f))
            {
                if (hit.transform.CompareTag("Water"))
                {
                    return transform.position.y - hit.point.y;
                }
            }
            return 0f;
        }

        private bool IsSubmerged()
        {
            return Physics.Raycast(transform.position, Vector3.down, 0.5f);
        }

        private bool IsHeadSubmerged()
        {
            return Physics.Raycast(transform.position + Vector3.up * 1.5f, Vector3.down, 0.3f);
        }

        private float GetSwimVelocity()
        {
            // Would integrate with actual movement controller
            return 0f;
        }

        public void AddOxygen(float amount)
        {
            currentOxygen = Mathf.Min(maxOxygen, currentOxygen + amount);
        }

        public bool CanBreathe() => currentState != WaterState.Drowning && currentOxygen > 0;
    }
}
