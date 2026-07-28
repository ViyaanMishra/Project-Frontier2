using UnityEngine;

namespace Frontier.Weather
{
    /// <summary>
    /// Weather controller with 12 weather states, seasonal cycles, and wind simulation.
    /// </summary>
    public enum WeatherState
    {
        Clear,
        Overcast,
        LightRain,
        HeavyRain,
        Thunderstorm,
        Hail,
        Snow,
        Blizzard,
        Sandstorm,
        AcidRain,
        Fog,
        AnomalyStorm
    }

    public enum Season
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }

    [System.Serializable]
    public struct WeatherConfig
    {
        public WeatherState state;
        public float intensity;       // 0.0 - 1.0
        public float windSpeed;       // m/s
        public Vector3 windDirection;
        public float temperature;     // Celsius
        public float humidity;        // 0.0 - 1.0
        public float visibility;      // meters
        public float precipitationRate;
    }

    public class WeatherController : MonoBehaviour
    {
        [Header("Cycle Settings")]
        [SerializeField] private float seasonDurationDays = 28f;
        [SerializeField] private float weatherTransitionSpeed = 0.1f;
        
        [Header("Current State")]
        [SerializeField] private WeatherState currentWeather;
        [SerializeField] private Season currentSeason;
        
        private WeatherConfig _currentConfig;
        private WeatherConfig _targetConfig;
        private float _seasonProgress;
        private float _weatherTimer;
        private float _dayTime;
        
        public WeatherConfig CurrentWeather => _currentConfig;
        public Season CurrentSeason => currentSeason;
        public float Temperature => _currentConfig.temperature;
        public float WindSpeed => _currentConfig.windSpeed;
        
        private void Start()
        {
            InitializeWeather();
        }
        
        private void InitializeWeather()
        {
            _currentConfig = new WeatherConfig
            {
                state = WeatherState.Clear,
                intensity = 0f,
                windSpeed = 2f,
                windDirection = Vector3.forward,
                temperature = 20f,
                humidity = 0.5f,
                visibility = 1000f,
                precipitationRate = 0f
            };
            
            _targetConfig = _currentConfig;
            _seasonProgress = 0f;
            currentSeason = Season.Spring;
            _dayTime = 0f;
        }
        
        private void Update()
        {
            // Update day time (24 hour cycle)
            _dayTime += Time.deltaTime;
            if (_dayTime >= 86400f) _dayTime = 0f; // 24 hours in seconds
            
            // Update season progress
            _seasonProgress += Time.deltaTime / (seasonDurationDays * 86400f);
            if (_seasonProgress >= 1f)
            {
                _seasonProgress = 0f;
                currentSeason = (Season)(((int)currentSeason + 1) % 4);
                OnSeasonChanged();
            }
            
            // Update weather timer
            _weatherTimer -= Time.deltaTime;
            if (_weatherTimer <= 0f)
            {
                SelectNewWeather();
            }
            
            // Interpolate towards target weather
            InterpolateWeather(Time.deltaTime);
            
            // Apply temperature based on season and time of day
            ApplyTemperatureCycle();
        }
        
        private void OnSeasonChanged()
        {
            switch (currentSeason)
            {
                case Season.Spring:
                    _targetConfig.temperature = 15f;
                    _targetConfig.humidity = 0.6f;
                    break;
                case Season.Summer:
                    _targetConfig.temperature = 30f;
                    _targetConfig.humidity = 0.4f;
                    break;
                case Season.Autumn:
                    _targetConfig.temperature = 12f;
                    _targetConfig.humidity = 0.5f;
                    break;
                case Season.Winter:
                    _targetConfig.temperature = -5f;
                    _targetConfig.humidity = 0.3f;
                    break;
            }
        }
        
        private void SelectNewWeather()
        {
            // Weighted random selection based on season
            float roll = Random.value;
            
            switch (currentSeason)
            {
                case Season.Spring:
                    if (roll < 0.4f) SetTargetWeather(WeatherState.Clear);
                    else if (roll < 0.7f) SetTargetWeather(WeatherState.Overcast);
                    else if (roll < 0.9f) SetTargetWeather(WeatherState.LightRain);
                    else SetTargetWeather(WeatherState.Thunderstorm);
                    break;
                    
                case Season.Summer:
                    if (roll < 0.6f) SetTargetWeather(WeatherState.Clear);
                    else if (roll < 0.8f) SetTargetWeather(WeatherState.Overcast);
                    else if (roll < 0.95f) SetTargetWeather(WeatherState.HeavyRain);
                    else SetTargetWeather(WeatherState.Thunderstorm);
                    break;
                    
                case Season.Autumn:
                    if (roll < 0.3f) SetTargetWeather(WeatherState.Clear);
                    else if (roll < 0.5f) SetTargetWeather(WeatherState.Overcast);
                    else if (roll < 0.7f) SetTargetWeather(WeatherState.LightRain);
                    else if (roll < 0.85f) SetTargetWeather(WeatherState.Fog);
                    else SetTargetWeather(WeatherState.HeavyRain);
                    break;
                    
                case Season.Winter:
                    if (roll < 0.3f) SetTargetWeather(WeatherState.Clear);
                    else if (roll < 0.5f) SetTargetWeather(WeatherState.Overcast);
                    else if (roll < 0.75f) SetTargetWeather(WeatherState.Snow);
                    else if (roll < 0.9f) SetTargetWeather(WeatherState.Blizzard);
                    else SetTargetWeather(WeatherState.Fog);
                    break;
            }
            
            // Random weather duration (5-30 minutes)
            _weatherTimer = Random.Range(300f, 1800f);
        }
        
        private void SetTargetWeather(WeatherState newState)
        {
            _targetConfig.state = newState;
            
            switch (newState)
            {
                case WeatherState.Clear:
                    _targetConfig.intensity = 0f;
                    _targetConfig.windSpeed = Random.Range(1f, 5f);
                    _targetConfig.visibility = 1000f;
                    _targetConfig.precipitationRate = 0f;
                    break;
                    
                case WeatherState.Overcast:
                    _targetConfig.intensity = 0.3f;
                    _targetConfig.windSpeed = Random.Range(3f, 8f);
                    _targetConfig.visibility = 500f;
                    _targetConfig.precipitationRate = 0f;
                    break;
                    
                case WeatherState.LightRain:
                    _targetConfig.intensity = 0.4f;
                    _targetConfig.windSpeed = Random.Range(5f, 12f);
                    _targetConfig.visibility = 300f;
                    _targetConfig.precipitationRate = 0.3f;
                    break;
                    
                case WeatherState.HeavyRain:
                    _targetConfig.intensity = 0.7f;
                    _targetConfig.windSpeed = Random.Range(10f, 20f);
                    _targetConfig.visibility = 100f;
                    _targetConfig.precipitationRate = 0.8f;
                    break;
                    
                case WeatherState.Thunderstorm:
                    _targetConfig.intensity = 0.9f;
                    _targetConfig.windSpeed = Random.Range(15f, 30f);
                    _targetConfig.visibility = 80f;
                    _targetConfig.precipitationRate = 1f;
                    break;
                    
                case WeatherState.Snow:
                    _targetConfig.intensity = 0.5f;
                    _targetConfig.windSpeed = Random.Range(3f, 10f);
                    _targetConfig.visibility = 200f;
                    _targetConfig.precipitationRate = 0.4f;
                    _targetConfig.temperature = Mathf.Min(_targetConfig.temperature, 0f);
                    break;
                    
                case WeatherState.Blizzard:
                    _targetConfig.intensity = 0.95f;
                    _targetConfig.windSpeed = Random.Range(20f, 40f);
                    _targetConfig.visibility = 20f;
                    _targetConfig.precipitationRate = 0.9f;
                    _targetConfig.temperature = Mathf.Min(_targetConfig.temperature, -5f);
                    break;
                    
                case WeatherState.Sandstorm:
                    _targetConfig.intensity = 0.8f;
                    _targetConfig.windSpeed = Random.Range(25f, 50f);
                    _targetConfig.visibility = 30f;
                    _targetConfig.precipitationRate = 0f;
                    break;
                    
                case WeatherState.AcidRain:
                    _targetConfig.intensity = 0.7f;
                    _targetConfig.windSpeed = Random.Range(8f, 15f);
                    _targetConfig.visibility = 150f;
                    _targetConfig.precipitationRate = 0.6f;
                    break;
                    
                case WeatherState.Fog:
                    _targetConfig.intensity = 0.4f;
                    _targetConfig.windSpeed = Random.Range(1f, 5f);
                    _targetConfig.visibility = 50f;
                    _targetConfig.precipitationRate = 0.1f;
                    break;
                    
                case WeatherState.AnomalyStorm:
                    _targetConfig.intensity = 1f;
                    _targetConfig.windSpeed = Random.Range(30f, 60f);
                    _targetConfig.visibility = 10f;
                    _targetConfig.precipitationRate = 0.5f;
                    break;
            }
            
            _targetConfig.windDirection = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            ).normalized;
        }
        
        private void InterpolateWeather(float deltaTime)
        {
            _currentConfig.intensity = Mathf.Lerp(_currentConfig.intensity, _targetConfig.intensity, weatherTransitionSpeed * deltaTime);
            _currentConfig.windSpeed = Mathf.Lerp(_currentConfig.windSpeed, _targetConfig.windSpeed, weatherTransitionSpeed * deltaTime);
            _currentConfig.windDirection = Vector3.Lerp(_currentConfig.windDirection, _targetConfig.windDirection, weatherTransitionSpeed * deltaTime);
            _currentConfig.temperature = Mathf.Lerp(_currentConfig.temperature, _targetConfig.temperature, weatherTransitionSpeed * deltaTime);
            _currentConfig.humidity = Mathf.Lerp(_currentConfig.humidity, _targetConfig.humidity, weatherTransitionSpeed * deltaTime);
            _currentConfig.visibility = Mathf.Lerp(_currentConfig.visibility, _targetConfig.visibility, weatherTransitionSpeed * deltaTime);
            _currentConfig.precipitationRate = Mathf.Lerp(_currentConfig.precipitationRate, _targetConfig.precipitationRate, weatherTransitionSpeed * deltaTime);
            
            if (Mathf.Abs(_currentConfig.intensity - _targetConfig.intensity) < 0.01f)
                _currentConfig.state = _targetConfig.state;
        }
        
        private void ApplyTemperatureCycle()
        {
            // Daily temperature variation (coldest at 4am, warmest at 2pm)
            float hour = _dayTime / 3600f;
            float dailyVariation = Mathf.Sin((hour - 4f) / 24f * Mathf.PI * 2f) * 5f;
            _currentConfig.temperature += dailyVariation;
        }
        
        public bool IsPrecipitating() => _currentConfig.precipitationRate > 0.1f;
        public bool IsDangerous() => currentWeather == WeatherState.Thunderstorm || 
                                     currentWeather == WeatherState.Blizzard ||
                                     currentWeather == WeatherState.Sandstorm ||
                                     currentWeather == WeatherState.AnomalyStorm;
        
        public void ForceWeather(WeatherState state)
        {
            SetTargetWeather(state);
            _currentConfig = _targetConfig;
        }
    }
}
