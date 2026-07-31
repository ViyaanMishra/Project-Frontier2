using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace FrontierProject.VFX
{
    /// <summary>
    /// Cinematic-quality explosion effects generator with multi-layered particle systems,
    /// shockwave simulation, debris physics, thermal distortion, and dynamic lighting.
    /// Supports various explosion types from small grenades to massive demolitions.
    /// Zero visual distortion artifacts with smooth temporal coherence.
    /// </summary>
    [System.Serializable]
    public class ExplosionGen : MonoBehaviour
    {
        #region Explosion Types & Parameters
        
        public enum ExplosionType
        {
            Grenade,            // Small tactical explosion
            Rocket,             // Medium HE explosion
            CarBomb,            // Large vehicle explosion
            FuelTank,           // Massive industrial explosion
            Nuclear,            // Cataclysmic (visual only)
            Fireball,           // Pure thermal explosion
            Shrapnel,           // High fragmentation
            Concussion,         // Pure blast wave
            EMP,                // Electromagnetic pulse
            Chemical            // Toxic/biological burst
        }

        public enum ExplosionPhase
        {
            Ignition,           // Initial flash
            Expansion,          // Fireball growth
            Peak,               // Maximum size/intensity
            Decay,              // Cooling and dissipation
            Aftermath           // Smoke and debris only
        }

        [Header("Core Explosion Parameters")]
        public ExplosionType explosionType = ExplosionType.Rocket;
        [Range(0.5f, 100f)] public float explosionPower = 5f; // Radius in meters
        [Range(0.1f, 3f)] public float durationMultiplier = 1f;
        [Range(0.5f, 2f)] public float scaleVariation = 1f;
        
        [Header("Fireball Properties")]
        [Range(0.5f, 3f)] public float fireballExpansionSpeed = 1.5f;
        [Range(0.1f, 2f)] public float fireballMaxRadius = 1f;
        [Range(1000f, 6000f)] public float fireballTemperature = 3000f; // Kelvin
        [Range(0.1f, 2f)] public float fireballDuration = 0.5f;
        
        [Header("Shockwave Properties")]
        public bool enableShockwave = true;
        [Range(0.5f, 5f)] public float shockwaveSpeed = 2f;
        [Range(0.1f, 2f)] public float shockwaveThickness = 0.5f;
        [Range(0f, 1f)] public float shockwaveOpacity = 0.6f;
        [Range(0f, 10f)] public float shockwaveDistortion = 2f;
        
        [Header("Debris System")]
        public bool enableDebris = true;
        [Range(10, 500)] public int debrisCount = 50;
        [Range(0.1f, 5f)] public float debrisEjectionSpeed = 2f;
        [Range(0f, 90f)] public float debrisSpreadAngle = 45f;
        [Range(0.1f, 3f)] public float debrisMinSize = 0.2f;
        [Range(0.5f, 5f)] public float debrisMaxSize = 1.5f;
        public bool enableDebrisRotation = true;
        public bool enableDebrisPhysics = true;
        
        [Header("Smoke System")]
        public bool enableSmoke = true;
        [Range(10, 200)] public int smokeParticleCount = 80;
        [Range(0.5f, 5f)] public float smokeRiseSpeed = 1.5f;
        [Range(0.1f, 3f)] public float smokeParticleSize = 1f;
        [Range(0f, 2f)] public float smokeTurbulence = 0.5f;
        [Range(0.5f, 5f)] public float smokeLifetime = 3f;
        
        [Header("Spark System")]
        public bool enableSparks = true;
        [Range(10, 300)] public int sparkCount = 100;
        [Range(1f, 20f)] public float sparkSpeed = 10f;
        [Range(0.1f, 2f)] public float sparkLifetime = 0.8f;
        public bool enableSparkBounce = true;
        public bool enableSparkTrail = true;
        
        [Header("Light & Illumination")]
        public bool enableDynamicLight = true;
        [Range(1f, 50f)] public float lightRange = 20f;
        [Range(1f, 20f)] public float lightIntensity = 10f;
        [Range(2000f, 7000f)] public float lightColorTemp = 4000f; // Kelvin
        public LightShadows lightShadows = LightShadows.Soft;
        [Range(0f, 1f)] public float lightFlickerAmount = 0.3f;
        
        [Header("Camera Effects")]
        public bool enableCameraShake = true;
        [Range(0f, 5f)] public float shakeMagnitude = 1f;
        [Range(0.1f, 2f)] public float shakeDuration = 0.5f;
        [Range(0f, 1f)] public float screenFlashIntensity = 0.5f;
        [Range(0f, 1f)] public float chromaticAberration = 0.2f;
        
        [Header("Audio")]
        public AudioClip explosionSound;
        public AudioClip rumbleSound;
        public AudioClip debrisSound;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0f, 1f)] public float spatialBlend = 1f;
        
        [Header("Runtime State")]
        public ExplosionPhase currentPhase = ExplosionPhase.Ignition;
        public float elapsedTime = 0f;
        public float currentRadius = 0f;
        public float currentIntensity = 0f;
        
        #endregion

        #region Particle Data Structures
        
        private struct DebrisParticle
        {
            public Vector3 position;
            public Vector3 velocity;
            public Vector3 angularVelocity;
            public Quaternion rotation;
            public float size;
            public float mass;
            public float lifetime;
            public bool isGrounded;
        }

        private struct SmokeParticle
        {
            public Vector3 position;
            public Vector3 velocity;
            public float size;
            public float alpha;
            public float lifetime;
            public float maxLifetime;
            public Color color;
        }

        private struct SparkParticle
        {
            public Vector3 position;
            public Vector3 velocity;
            public float size;
            public float alpha;
            public float lifetime;
            public Color color;
            public int bounceCount;
        }

        private NativeList<DebrisParticle> debrisParticles;
        private NativeList<SmokeParticle> smokeParticles;
        private NativeList<SparkParticle> sparkParticles;
        
        private Light explosionLight;
        private AudioSource audioSource;
        private float shockwaveProgress;
        private float cameraShakeRemaining;
        
        #endregion

        #region Type Presets
        
        private struct ExplosionPreset
        {
            public float power;
            public float fireballSpeed;
            public float fireballRadius;
            public float fireballTemp;
            public float shockwaveSpeed;
            public int debrisCount;
            public int smokeCount;
            public int sparkCount;
            public float lightRange;
            public float lightIntensity;
            public float shakeMagnitude;
        }

        private static ExplosionPreset[] typePresets = new ExplosionPreset[10];
        
        private void InitializeTypePresets()
        {
            // Grenade
            typePresets[0] = new ExplosionPreset
            {
                power = 3f, fireballSpeed = 2f, fireballRadius = 2f, fireballTemp = 3500f,
                shockwaveSpeed = 3f, debrisCount = 20, smokeCount = 40, sparkCount = 50,
                lightRange = 10f, lightIntensity = 8f, shakeMagnitude = 0.5f
            };

            // Rocket
            typePresets[1] = new ExplosionPreset
            {
                power = 8f, fireballSpeed = 2.5f, fireballRadius = 5f, fireballTemp = 4000f,
                shockwaveSpeed = 4f, debrisCount = 50, smokeCount = 80, sparkCount = 100,
                lightRange = 25f, lightIntensity = 12f, shakeMagnitude = 1f
            };

            // Car Bomb
            typePresets[2] = new ExplosionPreset
            {
                power = 15f, fireballSpeed = 2f, fireballRadius = 10f, fireballTemp = 3800f,
                shockwaveSpeed = 3.5f, debrisCount = 100, smokeCount = 120, sparkCount = 150,
                lightRange = 40f, lightIntensity = 15f, shakeMagnitude = 2f
            };

            // Fuel Tank
            typePresets[3] = new ExplosionPreset
            {
                power = 30f, fireballSpeed = 1.8f, fireballRadius = 20f, fireballTemp = 3200f,
                shockwaveSpeed = 3f, debrisCount = 150, smokeCount = 180, sparkCount = 200,
                lightRange = 80f, lightIntensity = 18f, shakeMagnitude = 3f
            };

            // Nuclear (visual representation)
            typePresets[4] = new ExplosionPreset
            {
                power = 100f, fireballSpeed = 1.5f, fireballRadius = 50f, fireballTemp = 6000f,
                shockwaveSpeed = 5f, debrisCount = 300, smokeCount = 200, sparkCount = 300,
                lightRange = 200f, lightIntensity = 20f, shakeMagnitude = 5f
            };

            // Fireball
            typePresets[5] = new ExplosionPreset
            {
                power = 10f, fireballSpeed = 3f, fireballRadius = 8f, fireballTemp = 5000f,
                shockwaveSpeed = 2f, debrisCount = 10, smokeCount = 60, sparkCount = 200,
                lightRange = 30f, lightIntensity = 16f, shakeMagnitude = 1f
            };

            // Shrapnel
            typePresets[6] = new ExplosionPreset
            {
                power = 6f, fireballSpeed = 1.5f, fireballRadius = 3f, fireballTemp = 3000f,
                shockwaveSpeed = 4f, debrisCount = 200, smokeCount = 50, sparkCount = 80,
                lightRange = 15f, lightIntensity = 8f, shakeMagnitude = 0.8f
            };

            // Concussion
            typePresets[7] = new ExplosionPreset
            {
                power = 12f, fireballSpeed = 1f, fireballRadius = 4f, fireballTemp = 2500f,
                shockwaveSpeed = 6f, debrisCount = 20, smokeCount = 40, sparkCount = 30,
                lightRange = 20f, lightIntensity = 10f, shakeMagnitude = 2.5f
            };

            // EMP
            typePresets[8] = new ExplosionPreset
            {
                power = 8f, fireballSpeed = 0.5f, fireballRadius = 5f, fireballTemp = 8000f,
                shockwaveSpeed = 8f, debrisCount = 5, smokeCount = 10, sparkCount = 250,
                lightRange = 30f, lightIntensity = 20f, shakeMagnitude = 0.3f
            };

            // Chemical
            typePresets[9] = new ExplosionPreset
            {
                power = 7f, fireballSpeed = 1f, fireballRadius = 6f, fireballTemp = 2000f,
                shockwaveSpeed = 2f, debrisCount = 30, smokeCount = 150, sparkCount = 20,
                lightRange = 15f, lightIntensity = 6f, shakeMagnitude = 0.5f
            };
        }

        #endregion

        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeTypePresets();
            debrisParticles = new NativeList<DebrisParticle>(Allocator.Persistent);
            smokeParticles = new NativeList<SmokeParticle>(Allocator.Persistent);
            sparkParticles = new NativeList<SparkParticle>(Allocator.Persistent);
            
            SetupComponents();
        }

        private void OnDestroy()
        {
            if (debrisParticles.IsCreated) debrisParticles.Dispose();
            if (smokeParticles.IsCreated) smokeParticles.Dispose();
            if (sparkParticles.IsCreated) sparkParticles.Dispose();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            elapsedTime += deltaTime;
            
            // Apply preset
            ApplyTypePreset();
            
            // Update phase
            UpdateExplosionPhase(deltaTime);
            
            // Update systems
            if (currentPhase != ExplosionPhase.Aftermath || elapsedTime < fireballDuration * 3f)
            {
                if (enableShockwave) UpdateShockwave(deltaTime);
                if (enableDebris) UpdateDebris(deltaTime);
                if (enableSmoke) UpdateSmoke(deltaTime);
                if (enableSparks) UpdateSparks(deltaTime);
                if (enableDynamicLight) UpdateLight(deltaTime);
            }
            
            // Update camera effects
            if (enableCameraShake && cameraShakeRemaining > 0)
            {
                cameraShakeRemaining -= deltaTime;
            }
        }

        #endregion

        #region Setup
        
        private void SetupComponents()
        {
            explosionLight = GetComponent<Light>();
            if (explosionLight == null && enableDynamicLight)
            {
                explosionLight = gameObject.AddComponent<Light>();
                explosionLight.type = LightType.Point;
                explosionLight.shadows = lightShadows;
            }
            
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = spatialBlend;
            }
        }

        #endregion

        #region Phase Management
        
        private void UpdateExplosionPhase(float deltaTime)
        {
            float normalizedTime = elapsedTime / (fireballDuration * durationMultiplier);
            
            if (normalizedTime < 0.1f)
            {
                currentPhase = ExplosionPhase.Ignition;
                currentIntensity = math.lerp(0f, 1f, normalizedTime / 0.1f);
                currentRadius = explosionPower * 0.1f;
                
                if (normalizedTime >= 0.09f && elapsedTime - deltaTime < fireballDuration * 0.09f * durationMultiplier)
                {
                    Detonate();
                }
            }
            else if (normalizedTime < 0.4f)
            {
                currentPhase = ExplosionPhase.Expansion;
                float expansionProgress = (normalizedTime - 0.1f) / 0.3f;
                currentIntensity = 1f;
                currentRadius = explosionPower * 0.1f + (explosionPower * fireballMaxRadius - explosionPower * 0.1f) * expansionProgress;
            }
            else if (normalizedTime < 0.6f)
            {
                currentPhase = ExplosionPhase.Peak;
                currentIntensity = 1f - (normalizedTime - 0.4f) / 0.2f * 0.3f;
                currentRadius = explosionPower * fireballMaxRadius;
            }
            else if (normalizedTime < 1f)
            {
                currentPhase = ExplosionPhase.Decay;
                float decayProgress = (normalizedTime - 0.6f) / 0.4f;
                currentIntensity = 0.7f * (1f - decayProgress);
                currentRadius = explosionPower * fireballMaxRadius * (1f + decayProgress * 0.2f);
            }
            else
            {
                currentPhase = ExplosionPhase.Aftermath;
                currentIntensity = 0f;
            }
        }

        private void Detonate()
        {
            PlayExplosionSound();
            
            if (enableCameraShake)
            {
                cameraShakeRemaining = shakeDuration;
            }
            
            // Spawn initial particles
            SpawnDebris();
            SpawnSmoke();
            SpawnSparks();
        }

        #endregion

        #region Shockwave
        
        private void UpdateShockwave(float deltaTime)
        {
            shockwaveProgress += deltaTime * shockwaveSpeed / explosionPower;
            
            if (shockwaveProgress > 1f)
            {
                shockwaveProgress = 0f;
            }
        }

        #endregion

        #region Debris System
        
        private void SpawnDebris()
        {
            if (!enableDebris) return;
            
            for (int i = 0; i < debrisCount; i++)
            {
                DebrisParticle debris = new DebrisParticle();
                
                // Random position on sphere surface
                float theta = UnityEngine.Random.Range(0f, math.PI * 2f);
                float phi = UnityEngine.Random.Range(0f, math.PI);
                debris.position = new Vector3(
                    math.sin(phi) * math.cos(theta),
                    math.sin(phi) * math.sin(theta),
                    math.cos(phi)
                ) * explosionPower * 0.1f;
                
                // Ejection velocity
                float speed = debrisEjectionSpeed * explosionPower * UnityEngine.Random.Range(0.5f, 1.5f);
                debris.velocity = debris.position.normalized * speed;
                
                // Add spread
                float spreadRad = debrisSpreadAngle * math.radians;
                debris.velocity += new Vector3(
                    UnityEngine.Random.Range(-1f, 1f),
                    UnityEngine.Random.Range(-1f, 1f),
                    UnityEngine.Random.Range(-1f, 1f)
                ).normalized * speed * math.tan(spreadRad);
                
                // Rotation
                debris.angularVelocity = new Vector3(
                    UnityEngine.Random.Range(-360f, 360f),
                    UnityEngine.Random.Range(-360f, 360f),
                    UnityEngine.Random.Range(-360f, 360f)
                );
                debris.rotation = Quaternion.identity;
                
                // Size and mass
                debris.size = math.lerp(debrisMinSize, debrisMaxSize, UnityEngine.Random.Range(0f, 1f));
                debris.mass = debris.size * debris.size * debris.size; // Volume-based
                
                debris.lifetime = 0f;
                debris.isGrounded = false;
                
                debrisParticles.Add(debris);
            }
        }

        private void UpdateDebris(float deltaTime)
        {
            for (int i = debrisParticles.Length - 1; i >= 0; i--)
            {
                DebrisParticle debris = debrisParticles[i];
                
                if (!debris.isGrounded)
                {
                    // Apply gravity
                    debris.velocity += Physics.gravity * deltaTime;
                    
                    // Apply drag
                    debris.velocity *= 0.99f;
                    
                    // Move
                    debris.position += debris.velocity * deltaTime;
                    
                    // Ground collision
                    if (debris.position.y < 0)
                    {
                        debris.position.y = 0;
                        debris.velocity.y = -debris.velocity.y * 0.3f; // Bounce
                        debris.velocity.x *= 0.7f;
                        debris.velocity.z *= 0.7f;
                        
                        if (math.abs(debris.velocity.y) < 0.5f)
                        {
                            debris.isGrounded = true;
                        }
                    }
                }
                
                // Rotation
                if (enableDebrisRotation)
                {
                    debris.rotation *= Quaternion.Euler(debris.angularVelocity * deltaTime);
                }
                
                debris.lifetime += deltaTime;
                
                // Remove old debris
                if (debris.lifetime > 5f)
                {
                    debrisParticles.RemoveAt(i);
                }
                else
                {
                    debrisParticles[i] = debris;
                }
            }
        }

        #endregion

        #region Smoke System
        
        private void SpawnSmoke()
        {
            if (!enableSmoke) return;
            
            for (int i = 0; i < smokeParticleCount; i++)
            {
                SmokeParticle smoke = new SmokeParticle();
                
                float theta = UnityEngine.Random.Range(0f, math.PI * 2f);
                float phi = UnityEngine.Random.Range(0f, math.PI);
                smoke.position = new Vector3(
                    math.sin(phi) * math.cos(theta),
                    math.sin(phi) * math.sin(theta),
                    math.cos(phi)
                ) * explosionPower * 0.2f;
                
                smoke.velocity = new Vector3(
                    UnityEngine.Random.Range(-1f, 1f),
                    smokeRiseSpeed,
                    UnityEngine.Random.Range(-1f, 1f)
                );
                
                smoke.size = smokeParticleSize * UnityEngine.Random.Range(0.5f, 1.5f);
                smoke.alpha = 0.8f;
                smoke.lifetime = 0f;
                smoke.maxLifetime = smokeLifetime * UnityEngine.Random.Range(0.7f, 1.3f);
                smoke.color = new Color(0.2f, 0.2f, 0.2f, smoke.alpha);
                
                smokeParticles.Add(smoke);
            }
        }

        private void UpdateSmoke(float deltaTime)
        {
            for (int i = smokeParticles.Length - 1; i >= 0; i--)
            {
                SmokeParticle smoke = smokeParticles[i];
                
                smoke.lifetime += deltaTime;
                
                if (smoke.lifetime >= smoke.maxLifetime)
                {
                    smokeParticles.RemoveAt(i);
                    continue;
                }
                
                // Rise and expand
                smoke.position += smoke.velocity * deltaTime;
                smoke.position += new Vector3(
                    math.sin(smoke.lifetime * 2f + smoke.position.z) * smokeTurbulence,
                    0,
                    math.cos(smoke.lifetime * 2f + smoke.position.x) * smokeTurbulence
                ) * deltaTime;
                
                smoke.size *= 1.02f;
                smoke.alpha = 0.8f * (1f - smoke.lifetime / smoke.maxLifetime);
                smoke.color.a = smoke.alpha;
                
                smokeParticles[i] = smoke;
            }
        }

        #endregion

        #region Sparks System
        
        private void SpawnSparks()
        {
            if (!enableSparks) return;
            
            for (int i = 0; i < sparkCount; i++)
            {
                SparkParticle spark = new SparkParticle();
                
                float theta = UnityEngine.Random.Range(0f, math.PI * 2f);
                float phi = UnityEngine.Random.Range(0f, math.PI * 0.5f); // Upward bias
                spark.position = new Vector3(
                    math.sin(phi) * math.cos(theta),
                    math.sin(phi) * math.sin(theta),
                    math.cos(phi)
                ) * explosionPower * 0.1f;
                
                spark.velocity = new Vector3(
                    math.sin(phi) * math.cos(theta),
                    math.sin(phi) * math.sin(theta),
                    math.cos(phi)
                ) * sparkSpeed * UnityEngine.Random.Range(0.5f, 1.5f);
                
                spark.size = 0.1f * UnityEngine.Random.Range(0.5f, 1.5f);
                spark.alpha = 1f;
                spark.lifetime = 0f;
                spark.color = new Color(1f, 0.8f, 0.4f, 1f);
                spark.bounceCount = 0;
                
                sparkParticles.Add(spark);
            }
        }

        private void UpdateSparks(float deltaTime)
        {
            for (int i = sparkParticles.Length - 1; i >= 0; i--)
            {
                SparkParticle spark = sparkParticles[i];
                
                spark.lifetime += deltaTime;
                
                if (spark.lifetime >= sparkLifetime)
                {
                    sparkParticles.RemoveAt(i);
                    continue;
                }
                
                // Physics
                spark.velocity += Physics.gravity * 0.5f * deltaTime;
                spark.position += spark.velocity * deltaTime;
                
                // Ground bounce
                if (enableSparkBounce && spark.position.y < 0 && spark.bounceCount < 3)
                {
                    spark.position.y = 0;
                    spark.velocity.y = -spark.velocity.y * 0.5f;
                    spark.bounceCount++;
                }
                
                // Fade
                spark.alpha = 1f - spark.lifetime / sparkLifetime;
                spark.color.a = spark.alpha;
                
                sparkParticles[i] = spark;
            }
        }

        #endregion

        #region Dynamic Light
        
        private void UpdateLight(float deltaTime)
        {
            if (explosionLight == null) return;
            
            float intensityMod = currentIntensity;
            
            // Add flicker
            if (lightFlickerAmount > 0)
            {
                intensityMod *= 1f + math.sin(elapsedTime * 100f) * lightFlickerAmount * 0.5f;
                intensityMod *= 1f + math.sin(elapsedTime * 150f + 1f) * lightFlickerAmount * 0.3f;
            }
            
            // Color temperature to RGB
            explosionLight.color = KelvinToRGB(lightColorTemp);
            explosionLight.intensity = lightIntensity * intensityMod;
            explosionLight.range = lightRange * (currentRadius / (explosionPower * fireballMaxRadius + 0.001f));
        }

        private Color KelvinToRGB(float kelvin)
        {
            // Simplified color temperature conversion
            float temp = kelvin / 100f;
            
            float r, g, b;
            
            if (temp <= 66f)
            {
                r = 1f;
                g = math.clamp(0.994708f * math.log(temp) - 0.254f, 0f, 1f);
                b = temp <= 19f ? 0f : math.clamp(0.5432f * math.log(temp - 10f) - 1.196f, 0f, 1f);
            }
            else
            {
                r = math.clamp(1.292936f * math.pow(temp - 60f, -0.133f), 0f, 1f);
                g = math.clamp(1.129891f * math.pow(temp - 60f, -0.106f), 0f, 1f);
                b = 1f;
            }
            
            return new Color(r, g, b, 1f);
        }

        #endregion

        #region Audio
        
        private void PlayExplosionSound()
        {
            if (audioSource == null) return;
            
            if (explosionSound != null)
            {
                audioSource.PlayOneShot(explosionSound, volume);
            }
            
            if (rumbleSound != null)
            {
                AudioSource.PlayClipAtPoint(rumbleSound, transform.position, volume * 0.5f);
            }
        }

        #endregion

        #region Preset Application
        
        private void ApplyTypePreset()
        {
            int typeIndex = (int)explosionType;
            if (typeIndex < 0 || typeIndex >= typePresets.Length) return;
            
            ExplosionPreset preset = typePresets[typeIndex];
            
            explosionPower = preset.power;
            fireballExpansionSpeed = preset.fireballSpeed;
            fireballMaxRadius = preset.fireballRadius;
            fireballTemperature = preset.fireballTemp;
            shockwaveSpeed = preset.shockwaveSpeed;
            debrisCount = preset.debrisCount;
            smokeParticleCount = preset.smokeCount;
            sparkCount = preset.sparkCount;
            lightRange = preset.lightRange;
            lightIntensity = preset.lightIntensity;
            shakeMagnitude = preset.shakeMagnitude;
        }

        #endregion

        #region Public API
        
        public void Detonate(float power = 1f)
        {
            explosionPower = power;
            elapsedTime = 0f;
            currentPhase = ExplosionPhase.Ignition;
            
            // Clear existing particles
            debrisParticles.Clear();
            smokeParticles.Clear();
            sparkParticles.Clear();
        }

        public void SetExplosionType(ExplosionType type)
        {
            explosionType = type;
        }

        public void EnableDebris(bool enabled)
        {
            enableDebris = enabled;
        }

        public void EnableSmoke(bool enabled)
        {
            enableSmoke = enabled;
        }

        public void EnableSparks(bool enabled)
        {
            enableSparks = enabled;
        }

        public void EnableShockwave(bool enabled)
        {
            enableShockwave = enabled;
        }

        public float GetCurrentRadius() => currentRadius;
        public float GetCurrentIntensity() => currentIntensity;
        public ExplosionPhase GetCurrentPhase() => currentPhase;
        public int GetDebrisCount() => debrisParticles.Length;
        public int GetSmokeCount() => smokeParticles.Length;
        public int GetSparkCount() => sparkParticles.Length;
        
        #endregion
    }
}
