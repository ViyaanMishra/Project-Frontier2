using UnityEngine;

namespace Frontier.MeshGen.Lighting
{
    /// <summary>
    /// Manages sun position, color temperature, and cascaded shadows for time-of-day lighting.
    /// </summary>
    public class SunController : MonoBehaviour
    {
        [Header("Sun Settings")]
        [SerializeField] private Light _sunLight;
        [SerializeField] private Transform _sunTransform;
        
        [Header("Time of Day")]
        [Range(0f, 24f)]
        [SerializeField] private float _timeOfDay = 12f;
        [SerializeField] private float _dayDurationSeconds = 600f; // 10 minutes per day
        
        [Header("Color Temperature")]
        [SerializeField] private Color _dawnColor = new Color(1f, 0.6f, 0.4f);
        [SerializeField] private Color _noonColor = Color.white;
        [SerializeField] private Color _duskColor = new Color(1f, 0.5f, 0.3f);
        [SerializeField] private Color _nightColor = new Color(0.2f, 0.2f, 0.4f);
        
        [Header("Shadow Settings")]
        [SerializeField] private int _shadowCascades = 4;
        [SerializeField] private float[] _cascadeRatios = { 0.05f, 0.15f, 0.3f, 1f };
        [SerializeField] private float _shadowBias = 0.05f;
        [SerializeField] private float _shadowNormalBias = 0.1f;
        
        [Header("Lens Flare")]
        [SerializeField] private bool _enableLensFlare = true;
        [SerializeField] private float _flareFadeThreshold = 0.95f;
        
        private float _sunAngle;
        private Color _currentLightColor;
        
        void Awake()
        {
            if (_sunLight == null)
                _sunLight = GetComponent<Light>();
            
            if (_sunTransform == null)
                _sunTransform = transform;
            
            SetupShadows();
        }
        
        void SetupShadows()
        {
            if (_sunLight != null)
            {
                QualitySettings.shadowCascadeCount = _shadowCascades;
                QualitySettings.shadowCascade4Split = new Vector3(
                    _cascadeRatios[0],
                    _cascadeRatios[1] - _cascadeRatios[0],
                    _cascadeRatios[2] - _cascadeRatios[1]
                );
                _sunLight.shadowBias = _shadowBias;
                _sunLight.shadowNormalBias = _shadowNormalBias;
                _sunLight.shadows = LightShadows.Hard;
            }
        }
        
        void Update()
        {
            // Advance time
            _timeOfDay += (24f / _dayDurationSeconds) * Time.deltaTime;
            if (_timeOfDay >= 24f)
                _timeOfDay -= 24f;
            
            UpdateSunPosition();
            UpdateLightColor();
        }
        
        void UpdateSunPosition()
        {
            // Calculate sun angle (0 = sunrise, 0.25 = noon, 0.5 = sunset, 0.75 = midnight)
            _sunAngle = (_timeOfDay / 24f) * 360f - 90f;
            
            _sunTransform.rotation = Quaternion.Euler(_sunAngle, 170f, 0f);
        }
        
        void UpdateLightColor()
        {
            float normalizedTime = _timeOfDay / 24f;
            
            if (normalizedTime < 0.25f) // Dawn to Noon
            {
                float t = normalizedTime * 4f;
                _currentLightColor = Color.Lerp(_dawnColor, _noonColor, t);
            }
            else if (normalizedTime < 0.5f) // Noon to Dusk
            {
                float t = (normalizedTime - 0.25f) * 4f;
                _currentLightColor = Color.Lerp(_noonColor, _duskColor, t);
            }
            else if (normalizedTime < 0.75f) // Dusk to Night
            {
                float t = (normalizedTime - 0.5f) * 4f;
                _currentLightColor = Color.Lerp(_duskColor, _nightColor, t);
            }
            else // Night to Dawn
            {
                float t = (normalizedTime - 0.75f) * 4f;
                _currentLightColor = Color.Lerp(_nightColor, _dawnColor, t);
            }
            
            if (_sunLight != null)
                _sunLight.color = _currentLightColor;
        }
        
        /// <summary>
        /// Get the current sun direction vector.
        /// </summary>
        public Vector3 GetSunDirection() => -_sunTransform.forward;
        
        /// <summary>
        /// Check if a point is in direct sunlight (not occluded).
        /// </summary>
        public bool IsInSunlight(Vector3 worldPosition)
        {
            if (Physics.Raycast(worldPosition, GetSunDirection(), out RaycastHit hit, Mathf.Infinity))
                return false;
            return true;
        }
        
        /// <summary>
        /// Set time of day directly (0-24 hours).
        /// </summary>
        public void SetTimeOfDay(float hours)
        {
            _timeOfDay = Mathf.Repeat(hours, 24f);
            UpdateSunPosition();
            UpdateLightColor();
        }
        
        /// <summary>
        /// Get the current time of day.
        /// </summary>
        public float GetTimeOfDay() => _timeOfDay;
    }
}
