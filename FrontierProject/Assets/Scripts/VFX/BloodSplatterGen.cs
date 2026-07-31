using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Frontier.VFX
{
    /// <summary>
    /// Advanced blood splatter generator with arterial spray, impact pools, and drip trails
    /// </summary>
    public class BloodSplatterGen : ComponentSystem
    {
        [System.Serializable]
        public struct BloodParticle
        {
            public Vector3 position;
            public Vector3 velocity;
            public float size;
            public Color color;
            public float lifetime;
            public float age;
            bool isDripping;
            public BloodType type;
        }
        
        [System.Serializable]
        public struct SplatterInstance
        {
            public Entity sourceEntity;
            public Vector3 origin;
            public Vector3 direction;
            public float force;
            public float volume;
            public BloodType bloodType;
            public SplatterType splatterType;
            public NativeList<BloodParticle> particles;
            public float startTime;
            public bool isActive;
        }
        
        public enum BloodType { Fresh, Dried, Arterial, Venous, Mixed }
        public enum SplatterType { Impact, Spray, Pool, DripTrail, Smear, CastOff }
        
        private NativeList<SplatterInstance> _activeSplatters;
        private Gradient _bloodColorGradient;
        private AnimationCurve _decayCurve;
        
        protected override void OnCreate()
        {
            _activeSplatters = new NativeList<SplatterInstance>(Allocator.Persistent);
            InitializeBloodColors();
            InitializeDecayCurve();
        }
        
        protected override void OnDestroy()
        {
            for (int i = 0; i < _activeSplatters.Length; i++)
            {
                var splatter = _activeSplatters[i];
                if (splatter.particles.IsCreated)
                    splatter.particles.Dispose();
            }
            _activeSplatters.Dispose();
        }
        
        private void InitializeBloodColors()
        {
            _bloodColorGradient = new Gradient();
            
            var gradientKeys = new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.4f, 0.05f, 0.05f), 0f), // Dark red
                new GradientColorKey(new Color(0.7f, 0.1f, 0.1f), 0.3f), // Bright red
                new GradientColorKey(new Color(0.5f, 0.08f, 0.08f), 0.6f), // Medium red
                new GradientColorKey(new Color(0.3f, 0.05f, 0.05f), 1f)  // Dried dark
            };
            
            var alphaKeys = new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.9f, 0f),
                new GradientAlphaKey(0.95f, 0.3f),
                new GradientAlphaKey(0.8f, 0.6f),
                new GradientAlphaKey(0.6f, 1f)
            };
            
            _bloodColorGradient.SetKeys(gradientKeys, alphaKeys);
        }
        
        private void InitializeDecayCurve()
        {
            _decayCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.3f, 0.9f),
                new Keyframe(0.6f, 0.6f),
                new Keyframe(1f, 0.3f)
            );
        }
        
        public int CreateSplatter(Vector3 origin, Vector3 direction, float force, float volume,
                                 SplatterType type, BloodType bloodType = BloodType.Fresh)
        {
            var instance = new SplatterInstance
            {
                sourceEntity = Entity.Null,
                origin = origin,
                direction = direction.normalized,
                force = force,
                volume = volume,
                bloodType = bloodType,
                splatterType = type,
                particles = new NativeList<BloodParticle>(Allocator.Temp),
                startTime = Time.time,
                isActive = true
            };
            
            // Generate particles based on splatter type
            GenerateParticles(ref instance);
            
            _activeSplatters.Add(instance);
            return _activeSplatters.Length - 1;
        }
        
        public int CreateArterialSpray(Entity entity, Vector3 woundPosition, Vector3 arteryDirection, 
                                      float bloodPressure, float heartRate)
        {
            var instance = new SplatterInstance
            {
                sourceEntity = entity,
                origin = woundPosition,
                direction = arteryDirection.normalized,
                force = bloodPressure * 2f,
                volume = 50f,
                bloodType = BloodType.Arterial,
                splatterType = SplatterType.Spray,
                particles = new NativeList<BloodParticle>(Allocator.Temp),
                startTime = Time.time,
                isActive = true
            };
            
            // Pulsatile spray pattern synchronized with heart rate
            GenerateArterialSpray(ref instance, heartRate);
            
            _activeSplatters.Add(instance);
            return _activeSplatters.Length - 1;
        }
        
        private void GenerateParticles(ref SplatterInstance instance)
        {
            int particleCount = Mathf.RoundToInt(instance.volume * 10f);
            
            switch (instance.splatterType)
            {
                case SplatterType.Impact:
                    GenerateImpactPattern(ref instance, particleCount);
                    break;
                case SplatterType.Spray:
                    GenerateSprayPattern(ref instance, particleCount);
                    break;
                case SplatterType.Pool:
                    GeneratePoolPattern(ref instance, particleCount);
                    break;
                case SplatterType.DripTrail:
                    GenerateDripTrail(ref instance, particleCount);
                    break;
                case SplatterType.CastOff:
                    GenerateCastOffPattern(ref instance, particleCount);
                    break;
            }
        }
        
        private void GenerateImpactPattern(ref SplatterInstance instance, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = UnityEngine.Random.Range(0f, math.PI * 2f);
                float spread = UnityEngine.Random.Range(0.1f, 1f) * instance.force;
                
                var particle = new BloodParticle
                {
                    position = instance.origin,
                    velocity = new Vector3(
                        math.cos(angle) * spread,
                        UnityEngine.Random.Range(0.3f, 1f) * spread,
                        math.sin(angle) * spread
                    ),
                    size = UnityEngine.Random.Range(0.02f, 0.15f),
                    color = GetBloodColor(instance.bloodType),
                    lifetime = UnityEngine.Random.Range(30f, 120f),
                    age = 0f,
                    isDripping = false,
                    type = instance.bloodType
                };
                
                instance.particles.Add(particle);
            }
        }
        
        private void GenerateSprayPattern(ref SplatterInstance instance, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float coneAngle = UnityEngine.Random.Range(0f, 30f);
                float speed = UnityEngine.Random.Range(instance.force * 0.5f, instance.force);
                
                Quaternion rotation = Quaternion.Euler(
                    UnityEngine.Random.Range(-coneAngle, coneAngle),
                    UnityEngine.Random.Range(-coneAngle, coneAngle),
                    0f
                );
                
                var particle = new BloodParticle
                {
                    position = instance.origin,
                    velocity = rotation * instance.direction * speed,
                    size = UnityEngine.Random.Range(0.01f, 0.08f),
                    color = GetBloodColor(instance.bloodType),
                    lifetime = UnityEngine.Random.Range(20f, 60f),
                    age = 0f,
                    isDripping = false,
                    type = instance.bloodType
                };
                
                instance.particles.Add(particle);
            }
        }
        
        private void GenerateArterialSpray(ref SplatterInstance instance, float heartRate)
        {
            int pulsesPerSecond = Mathf.RoundToInt(heartRate / 60f);
            int totalPulses = pulsesPerSecond * 5; // 5 seconds of spraying
            
            for (int p = 0; p < totalPulses; p++)
            {
                float pulseTime = p / (float)pulsesPerSecond;
                int particlesPerPulse = Mathf.RoundToInt(instance.volume / totalPulses);
                
                for (int i = 0; i < particlesPerPulse; i++)
                {
                    float spread = UnityEngine.Random.Range(5f, 25f);
                    float speed = instance.force * UnityEngine.Random.Range(0.7f, 1.3f);
                    
                    var particle = new BloodParticle
                    {
                        position = instance.origin,
                        velocity = instance.direction * speed + 
                                  new Vector3(UnityEngine.Random.Range(-spread, spread),
                                             UnityEngine.Random.Range(spread, spread * 2f),
                                             UnityEngine.Random.Range(-spread, spread)),
                        size = UnityEngine.Random.Range(0.02f, 0.1f),
                        color = GetBloodColor(BloodType.Arterial),
                        lifetime = UnityEngine.Random.Range(30f, 90f),
                        age = pulseTime,
                        isDripping = false,
                        type = BloodType.Arterial
                    };
                    
                    instance.particles.Add(particle);
                }
            }
        }
        
        private void GeneratePoolPattern(ref SplatterInstance instance, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float radius = UnityEngine.Random.Range(0f, instance.volume * 0.3f);
                float angle = UnityEngine.Random.Range(0f, math.PI * 2f);
                
                var particle = new BloodParticle
                {
                    position = instance.origin + new Vector3(
                        math.cos(angle) * radius,
                        -0.01f,
                        math.sin(angle) * radius
                    ),
                    velocity = Vector3.zero,
                    size = UnityEngine.Random.Range(0.05f, 0.3f),
                    color = GetBloodColor(instance.bloodType),
                    lifetime = 300f, // Pools last longer
                    age = 0f,
                    isDripping = false,
                    type = instance.bloodType
                };
                
                instance.particles.Add(particle);
            }
        }
        
        private void GenerateDripTrail(ref SplatterInstance instance, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)count;
                Vector3 trailPos = instance.origin + instance.direction * t * instance.force * 2f;
                
                var particle = new BloodParticle
                {
                    position = trailPos + Vector3.up * UnityEngine.Random.Range(0f, 0.5f),
                    velocity = Vector3.down * UnityEngine.Random.Range(0.5f, 2f),
                    size = UnityEngine.Random.Range(0.03f, 0.1f),
                    color = GetBloodColor(instance.bloodType),
                    lifetime = 60f,
                    age = 0f,
                    isDripping = true,
                    type = instance.bloodType
                };
                
                instance.particles.Add(particle);
            }
        }
        
        private void GenerateCastOffPattern(ref SplatterInstance instance, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float arcHeight = UnityEngine.Random.Range(1f, 3f);
                float distance = UnityEngine.Random.Range(instance.force * 0.5f, instance.force * 1.5f);
                float angle = UnityEngine.Random.Range(-45f, 45f);
                
                var particle = new BloodParticle
                {
                    position = instance.origin,
                    velocity = new Vector3(
                        math.cos(math.radians(angle)) * distance,
                        arcHeight * 3f,
                        math.sin(math.radians(angle)) * distance * 0.3f
                    ),
                    size = UnityEngine.Random.Range(0.01f, 0.05f),
                    color = GetBloodColor(instance.bloodType),
                    lifetime = 45f,
                    age = 0f,
                    isDripping = false,
                    type = instance.bloodType
                };
                
                instance.particles.Add(particle);
            }
        }
        
        private Color GetBloodColor(BloodType type)
        {
            switch (type)
            {
                case BloodType.Fresh:
                    return _bloodColorGradient.Evaluate(0.3f);
                case BloodType.Dried:
                    return _bloodColorGradient.Evaluate(0.9f);
                case BloodType.Arterial:
                    return new Color(0.8f, 0.15f, 0.15f, 0.95f);
                case BloodType.Venous:
                    return new Color(0.5f, 0.05f, 0.05f, 0.9f);
                default:
                    return _bloodColorGradient.Evaluate(UnityEngine.Random.Range(0f, 1f));
            }
        }
        
        public void UpdateSplatter(int index, float deltaTime)
        {
            if (index < 0 || index >= _activeSplatters.Length) return;
            
            var splatter = _activeSplatters[index];
            if (!splatter.isActive) return;
            
            for (int i = splatter.particles.Length - 1; i >= 0; i--)
            {
                var particle = splatter.particles[i];
                particle.age += deltaTime;
                
                if (particle.age >= particle.lifetime)
                {
                    splatter.particles.RemoveAt(i);
                    continue;
                }
                
                // Apply gravity if not a pool
                if (splatter.splatterType != SplatterType.Pool)
                {
                    particle.velocity += Physics.gravity * deltaTime;
                    particle.position += particle.velocity * deltaTime;
                }
                
                // Simple ground collision
                if (particle.position.y < 0 && particle.velocity.y < 0)
                {
                    particle.position = new Vector3(particle.position.x, 0f, particle.position.z);
                    particle.velocity = Vector3.zero;
                    
                    if (!particle.isDripping)
                    {
                        // Spread on impact
                        particle.size *= 1.5f;
                    }
                }
                
                // Age-based color darkening
                float ageRatio = particle.age / particle.lifetime;
                particle.color.a = _decayCurve.Evaluate(ageRatio);
                
                splatter.particles[i] = particle;
            }
            
            // Deactivate if all particles are gone
            if (splatter.particles.Length == 0)
            {
                splatter.isActive = false;
            }
            
            _activeSplatters[index] = splatter;
        }
        
        public Mesh GenerateBloodMesh(int index)
        {
            // Would generate combined mesh from all particles for rendering
            return null;
        }
    }
}
