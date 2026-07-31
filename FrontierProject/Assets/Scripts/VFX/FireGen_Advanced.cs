using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Frontier.VFX
{
    /// <summary>
    /// Advanced fire generator with fuel types, heat propagation, smoke integration, and fire spread simulation
    /// </summary>
    public class FireGen : ComponentSystem
    {
        [System.Serializable]
        public struct FireParticle
        {
            public Vector3 position;
            public Vector3 velocity;
            public float size;
            public float temperature;
            public float lifetime;
            public float age;
            public FuelType fuelType;
            public ParticlePhase phase;
            public Color color;
        }
        
        [System.Serializable]
        public struct FireInstance
        {
            public Entity sourceEntity;
            public Vector3 origin;
            public float baseSize;
            public float currentSize;
            public float fuelAmount;
            public float maxFuel;
            public FuelType fuelType;
            public FireIntensity intensity;
            public NativeList<FireParticle> particles;
            public float startTime;
            public bool isActive;
            public float heatRadius;
            public float smokeProduction;
        }
        
        public enum FuelType { Wood, Paper, Fabric, Oil, Gas, Chemical, Magical }
        public enum FireIntensity { Smolder, Small, Medium, Large, Inferno }
        public enum ParticlePhase { Birth, Growth, Mature, Decay, Smoke }
        
        private NativeList<FireInstance> _activeFires;
        private Gradient _fireColorGradient;
        private AnimationCurve _flickerCurve;
        private AnimationCurve _riseCurve;
        
        protected override void OnCreate()
        {
            _activeFires = new NativeList<FireInstance>(Allocator.Persistent);
            InitializeFireColors();
            InitializeAnimationCurves();
        }
        
        protected override void OnDestroy()
        {
            for (int i = 0; i < _activeFires.Length; i++)
            {
                var fire = _activeFires[i];
                if (fire.particles.IsCreated)
                    fire.particles.Dispose();
            }
            _activeFires.Dispose();
        }
        
        private void InitializeFireColors()
        {
            _fireColorGradient = new Gradient();
            
            var gradientKeys = new GradientColorKey[]
            {
                new GradientColorKey(new Color(1f, 0.3f, 0.1f), 0f),      // Core (hottest)
                new GradientColorKey(new Color(1f, 0.5f, 0.1f), 0.2f),   // Inner flame
                new GradientColorKey(new Color(1f, 0.7f, 0.2f), 0.4f),   // Mid flame
                new GradientColorKey(new Color(1f, 0.9f, 0.4f), 0.6f),   // Outer flame
                new GradientColorKey(new Color(0.8f, 0.8f, 0.8f), 0.8f), // Hot gas
                new GradientColorKey(new Color(0.3f, 0.3f, 0.3f), 1f)    // Smoke
            };
            
            var alphaKeys = new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.9f, 0f),
                new GradientAlphaKey(0.8f, 0.3f),
                new GradientAlphaKey(0.6f, 0.6f),
                new GradientAlphaKey(0.4f, 0.8f),
                new GradientAlphaKey(0.2f, 1f)
            };
            
            _fireColorGradient.SetKeys(gradientKeys, alphaKeys);
        }
        
        private void InitializeAnimationCurves()
        {
            _flickerCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.1f, 1f),
                new Keyframe(0.2f, -0.5f),
                new Keyframe(0.3f, 0.8f),
                new Keyframe(0.4f, -0.3f),
                new Keyframe(0.5f, 0.5f),
                new Keyframe(0.6f, -0.8f),
                new Keyframe(0.7f, 0.3f),
                new Keyframe(0.8f, -0.5f),
                new Keyframe(0.9f, 0.7f),
                new Keyframe(1f, 0f)
            );
            
            _riseCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.2f, 0.3f),
                new Keyframe(0.5f, 0.7f),
                new Keyframe(0.8f, 0.9f),
                new Keyframe(1f, 1f)
            );
        }
        
        public int CreateFire(Vector3 origin, float initialFuel, FuelType fuelType, 
                             FireIntensity initialIntensity = FireIntensity.Medium)
        {
            var instance = new FireInstance
            {
                sourceEntity = Entity.Null,
                origin = origin,
                baseSize = GetBaseSizeForIntensity(initialIntensity),
                currentSize = GetBaseSizeForIntensity(initialIntensity),
                fuelAmount = initialFuel,
                maxFuel = initialFuel,
                fuelType = fuelType,
                intensity = initialIntensity,
                particles = new NativeList<FireParticle>(Allocator.Temp),
                startTime = Time.time,
                isActive = true,
                heatRadius = GetBaseSizeForIntensity(initialIntensity) * 3f,
                smokeProduction = GetSmokeProduction(fuelType, initialIntensity)
            };
            
            // Generate initial particles
            GenerateFireParticles(ref instance, Mathf.RoundToInt(instance.baseSize * 20f));
            
            _activeFires.Add(instance);
            return _activeFires.Length - 1;
        }
        
        public int CreateExplosionFire(Vector3 origin, float explosionForce, FuelType ignitedFuel)
        {
            var intensity = explosionForce > 50f ? FireIntensity.Inferno :
                           explosionForce > 30f ? FireIntensity.Large :
                           explosionForce > 15f ? FireIntensity.Medium : FireIntensity.Small;
            
            return CreateFire(origin, explosionForce * 2f, ignitedFuel, intensity);
        }
        
        private float GetBaseSizeForIntensity(FireIntensity intensity)
        {
            switch (intensity)
            {
                case FireIntensity.Smolder: return 0.5f;
                case FireIntensity.Small: return 1f;
                case FireIntensity.Medium: return 2f;
                case FireIntensity.Large: return 4f;
                case FireIntensity.Inferno: return 8f;
                default: return 2f;
            }
        }
        
        private float GetSmokeProduction(FuelType fuel, FireIntensity intensity)
        {
            float baseSmoke = 0f;
            switch (fuel)
            {
                case FuelType.Wood: baseSmoke = 0.5f; break;
                case FuelType.Paper: baseSmoke = 0.3f; break;
                case FuelType.Fabric: baseSmoke = 0.6f; break;
                case FuelType.Oil: baseSmoke = 0.8f; break;
                case FuelType.Gas: baseSmoke = 0.2f; break;
                case FuelType.Chemical: baseSmoke = 0.9f; break;
                case FuelType.Magical: baseSmoke = 0.1f; break;
            }
            
            float intensityMod = (float)intensity / 4f;
            return baseSmoke * (1f + intensityMod);
        }
        
        private void GenerateFireParticles(ref FireInstance instance, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var particle = CreateFireParticle(instance.origin, instance.fuelType, instance.intensity);
                instance.particles.Add(particle);
            }
        }
        
        private FireParticle CreateFireParticle(Vector3 origin, FuelType fuelType, FireIntensity intensity)
        {
            float sizeMult = GetBaseSizeForIntensity(intensity);
            
            return new FireParticle
            {
                position = origin + new Vector3(
                    UnityEngine.Random.Range(-sizeMult * 0.3f, sizeMult * 0.3f),
                    UnityEngine.Random.Range(0f, sizeMult * 0.2f),
                    UnityEngine.Random.Range(-sizeMult * 0.3f, sizeMult * 0.3f)
                ),
                velocity = new Vector3(
                    UnityEngine.Random.Range(-0.5f, 0.5f),
                    UnityEngine.Random.Range(2f, 5f) * sizeMult,
                    UnityEngine.Random.Range(-0.5f, 0.5f)
                ),
                size = UnityEngine.Random.Range(0.1f, 0.5f) * sizeMult,
                temperature = GetInitialTemperature(fuelType, intensity),
                lifetime = UnityEngine.Random.Range(1f, 3f),
                age = 0f,
                fuelType = fuelType,
                phase = ParticlePhase.Birth,
                color = _fireColorGradient.Evaluate(UnityEngine.Random.Range(0f, 0.3f))
            };
        }
        
        private float GetInitialTemperature(FuelType fuel, FireIntensity intensity)
        {
            float baseTemp = 600f; // Celsius
            
            switch (fuel)
            {
                case FuelType.Wood: baseTemp = 800f; break;
                case FuelType.Paper: baseTemp = 700f; break;
                case FuelType.Fabric: baseTemp = 750f; break;
                case FuelType.Oil: baseTemp = 1000f; break;
                case FuelType.Gas: baseTemp = 1200f; break;
                case FuelType.Chemical: baseTemp = 1500f; break;
                case FuelType.Magical: baseTemp = 2000f; break;
            }
            
            float intensityMult = 1f + ((float)intensity / 4f);
            return baseTemp * intensityMult;
        }
        
        public void UpdateFire(int index, float deltaTime, Vector3 windDirection, float windStrength)
        {
            if (index < 0 || index >= _activeFires.Length) return;
            
            var fire = _activeFires[index];
            if (!fire.isActive) return;
            
            // Consume fuel
            float fuelConsumption = GetFuelConsumptionRate(fire.fuelType, fire.intensity);
            fire.fuelAmount -= fuelConsumption * deltaTime;
            
            if (fire.fuelAmount <= 0)
            {
                fire.isActive = false;
                _activeFires[index] = fire;
                return;
            }
            
            // Update intensity based on remaining fuel
            float fuelRatio = fire.fuelAmount / fire.maxFuel;
            fire.intensity = GetIntensityFromFuelRatio(fuelRatio, fire.intensity);
            fire.currentSize = fire.baseSize * fuelRatio;
            fire.heatRadius = fire.currentSize * 3f;
            
            // Update particles
            for (int i = fire.particles.Length - 1; i >= 0; i--)
            {
                var particle = fire.particles[i];
                particle.age += deltaTime;
                
                if (particle.age >= particle.lifetime)
                {
                    fire.particles.RemoveAt(i);
                    continue;
                }
                
                // Update particle phase
                particle.phase = GetParticlePhase(particle.age / particle.lifetime);
                
                // Apply wind
                particle.velocity += windDirection * windStrength * deltaTime;
                
                // Apply turbulence
                float flicker = _flickerCurve.Evaluate(particle.age / particle.lifetime);
                particle.velocity.x += flicker * 0.5f * deltaTime;
                particle.velocity.z += flicker * 0.5f * deltaTime;
                
                // Move particle
                particle.position += particle.velocity * deltaTime;
                
                // Cool down over time
                particle.temperature -= 100f * deltaTime;
                
                // Update color based on phase and temperature
                float colorT = GetParticleColorT(particle.phase, particle.temperature);
                particle.color = _fireColorGradient.Evaluate(colorT);
                
                fire.particles[i] = particle;
            }
            
            // Replenish particles
            int targetParticleCount = Mathf.RoundToInt(fire.currentSize * 20f);
            if (fire.particles.Length < targetParticleCount)
            {
                int toSpawn = targetParticleCount - fire.particles.Length;
                for (int i = 0; i < toSpawn; i++)
                {
                    var newParticle = CreateFireParticle(fire.origin, fire.fuelType, fire.intensity);
                    fire.particles.Add(newParticle);
                }
            }
            
            _activeFires[index] = fire;
        }
        
        private float GetFuelConsumptionRate(FuelType fuel, FireIntensity intensity)
        {
            float baseRate = 1f;
            
            switch (fuel)
            {
                case FuelType.Wood: baseRate = 0.8f; break;
                case FuelType.Paper: baseRate = 2f; break;
                case FuelType.Fabric: baseRate = 1.2f; break;
                case FuelType.Oil: baseRate = 1.5f; break;
                case FuelType.Gas: baseRate = 3f; break;
                case FuelType.Chemical: baseRate = 2.5f; break;
                case FuelType.Magical: baseRate = 0.5f; break;
            }
            
            float intensityMult = 1f + ((float)intensity / 2f);
            return baseRate * intensityMult;
        }
        
        private FireIntensity GetIntensityFromFuelRatio(float ratio, FireIntensity currentIntensity)
        {
            if (ratio > 0.8f) return FireIntensity.Inferno;
            if (ratio > 0.6f) return FireIntensity.Large;
            if (ratio > 0.4f) return FireIntensity.Medium;
            if (ratio > 0.2f) return FireIntensity.Small;
            return FireIntensity.Smolder;
        }
        
        private ParticlePhase GetParticlePhase(float ageRatio)
        {
            if (ageRatio < 0.1f) return ParticlePhase.Birth;
            if (ageRatio < 0.3f) return ParticlePhase.Growth;
            if (ageRatio < 0.6f) return ParticlePhase.Mature;
            if (ageRatio < 0.8f) return ParticlePhase.Decay;
            return ParticlePhase.Smoke;
        }
        
        private float GetParticleColorT(ParticlePhase phase, float temperature)
        {
            switch (phase)
            {
                case ParticlePhase.Birth: return UnityEngine.Random.Range(0f, 0.2f);
                case ParticlePhase.Growth: return UnityEngine.Random.Range(0.2f, 0.4f);
                case ParticlePhase.Mature: return UnityEngine.Random.Range(0.4f, 0.6f);
                case ParticlePhase.Decay: return UnityEngine.Random.Range(0.6f, 0.8f);
                case ParticlePhase.Smoke: return UnityEngine.Random.Range(0.8f, 1f);
                default: return 0.5f;
            }
        }
        
        public float GetHeatDamage(int index, float distance)
        {
            if (index < 0 || index >= _activeFires.Length) return 0f;
            
            var fire = _activeFires[index];
            if (!fire.isActive) return 0f;
            
            if (distance > fire.heatRadius) return 0f;
            
            float heatIntensity = 1f - (distance / fire.heatRadius);
            float baseDamage = (float)fire.intensity * 10f;
            
            return baseDamage * heatIntensity;
        }
        
        public void ExtinguishFire(int index, float extinguishAmount)
        {
            if (index < 0 || index >= _activeFires.Length) return;
            
            var fire = _activeFires[index];
            fire.fuelAmount -= extinguishAmount;
            
            if (fire.fuelAmount <= 0)
            {
                fire.isActive = false;
            }
            
            _activeFires[index] = fire;
        }
    }
}
