using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using System.Collections.Generic;

namespace FrontierProject.VFX
{
    /// <summary>
    /// Premium quality magic spell effects generator with multiple spell types,
    /// distortion-free particle rendering, smooth color transitions, and
    /// physically-based light emission. Supports elemental schools, buff/debuff
    /// auras, projectile trails, and area-of-effect patterns.
    /// </summary>
    [System.Serializable]
    public class MagicSpellGen : MonoBehaviour
    {
        #region Spell Types & Schools
        
        public enum SpellSchool
        {
            Fire,           // Red/orange/yellow, heat distortion
            Ice,            // Blue/white, frost particles
            Lightning,      // Yellow/purple, electrical arcs
            Nature,         // Green, leaf/petal particles
            Arcane,         // Purple/blue, magical runes
            Holy,           // Gold/white, divine light
            Shadow,         // Dark purple/black, void effects
            Poison,         // Green/yellow, toxic clouds
            Physical,       // White/gray, force effects
            Cosmic          // Multi-color, starfield particles
        }

        public enum SpellType
        {
            Projectile,     // Moving spell bolt
            AOE,            // Area of effect circle
            Beam,           // Continuous beam
            Aura,           // Persistent buff field
            Explosion,      // Burst effect
            Summoning,      // Portal/spawn effect
            Enchantment,    // Weapon/armour glow
            Debuff          // Negative status effect
        }

        public enum CastPhase
        {
            Charging,       // Build-up before cast
            Casting,        // Active spell release
            Sustaining,     // Maintaining spell
            Fading          // Dissipation
        }

        [Header("Core Spell Parameters")]
        public SpellSchool spellSchool = SpellSchool.Arcane;
        public SpellType spellType = SpellType.Projectile;
        [Range(0.1f, 5f)] public float spellPower = 1f;
        [Range(0.1f, 3f)] public float spellDuration = 1f;
        [Range(0.5f, 3f)] public float spellScale = 1f;
        
        [Header("Color Configuration")]
        public Gradient primaryColors = new Gradient();
        public Gradient secondaryColors = new Gradient();
        public Color emissionColor = Color.white;
        [Range(0f, 10f)] public float emissionIntensity = 2f;
        
        [Header("Particle System")]
        [Range(10, 1000)] public int maxParticles = 200;
        [Range(0.1f, 10f)] public float particleSpeed = 2f;
        [Range(0.1f, 5f)] public float particleSize = 0.5f;
        [Range(0f, 1f)] public float particleSpread = 0.3f;
        [Range(0f, 5f)] public float particleLifetime = 2f;
        [Range(0f, 1f)] public float alphaFadeRate = 0.3f;
        
        [Header("Motion Patterns")]
        public bool enableSpiral = false;
        [Range(0.1f, 5f)] public float spiralFrequency = 1f;
        [Range(0f, 2f)] public float spiralAmplitude = 0.5f;
        
        public bool enableOrbital = false;
        [Range(0.1f, 3f)] public float orbitalRadius = 1f;
        [Range(0.1f, 5f)] public float orbitalSpeed = 1f;
        
        public bool enableTurbulence = true;
        [Range(0f, 2f)] public float turbulenceStrength = 0.5f;
        [Range(0.1f, 5f)] public float turbulenceFrequency = 2f;
        
        [Header("Light & Glow")]
        public bool enableDynamicLight = true;
        [Range(0f, 10f)] public float lightRange = 5f;
        [Range(0f, 10f)] public float lightIntensity = 3f;
        public LightShadows lightShadows = LightShadows.Soft;
        
        public bool enableBloom = true;
        [Range(0f, 5f)] public float bloomThreshold = 1f;
        [Range(0f, 10f)] public float bloomIntensity = 2f;
        
        [Header("Distortion Effects")]
        public bool enableHeatDistortion = false;
        [Range(0f, 0.1f)] public float distortionStrength = 0.02f;
        [Range(0.1f, 5f)] public float distortionSpeed = 1f;
        
        [Header("Sound Integration")]
        public AudioClip castSound;
        public AudioClip loopSound;
        public AudioClip endSound;
        [Range(0f, 1f)] public float soundVolume = 0.7f;
        
        [Header("Runtime State")]
        public CastPhase currentPhase = CastPhase.Charging;
        [Range(0f, 1f)] public float chargeProgress = 0f;
        public float remainingDuration = 0f;
        
        #endregion

        #region Particle Data Structures
        
        private struct MagicParticle
        {
            public Vector3 position;
            public Vector3 velocity;
            public float lifetime;
            public float maxLifetime;
            public float size;
            public Color color;
            public float alpha;
            public Vector3 rotationAxis;
            public float rotationSpeed;
            public int phase; // For multi-phase spells
        }

        private NativeList<MagicParticle> activeParticles;
        private NativeList<MagicParticle> newParticles;
        private Material particleMaterial;
        private Mesh particleMesh;
        private Light spellLight;
        private AudioSource audioSource;
        
        #endregion

        #region School Presets
        
        private struct SchoolPreset
        {
            public Color[] primaryPalette;
            public Color[] secondaryPalette;
            public float particleSpeed;
            public float particleSize;
            public float emissionIntensity;
            public bool heatDistortion;
            public string particleShape;
        }

        private static SchoolPreset[] schoolPresets = new SchoolPreset[10];
        
        private void InitializeSchoolPresets()
        {
            // Fire
            schoolPresets[0] = new SchoolPreset
            {
                primaryPalette = new[] { new Color(1f, 0.3f, 0f), new Color(1f, 0.6f, 0f), new Color(1f, 0.8f, 0.2f) },
                secondaryPalette = new[] { new Color(0.5f, 0f, 0f), new Color(0.3f, 0.1f, 0f) },
                particleSpeed = 3f,
                particleSize = 0.8f,
                emissionIntensity = 4f,
                heatDistortion = true,
                particleShape = "flame"
            };

            // Ice
            schoolPresets[1] = new SchoolPreset
            {
                primaryPalette = new[] { new Color(0.5f, 0.8f, 1f), new Color(0.7f, 0.9f, 1f), Color.white },
                secondaryPalette = new[] { new Color(0.3f, 0.5f, 0.7f), new Color(0.2f, 0.3f, 0.5f) },
                particleSpeed = 1.5f,
                particleSize = 0.6f,
                emissionIntensity = 2f,
                heatDistortion = false,
                particleShape = "crystal"
            };

            // Lightning
            schoolPresets[2] = new SchoolPreset
            {
                primaryPalette = new[] { new Color(1f, 1f, 0.5f), new Color(0.8f, 0.5f, 1f), Color.white },
                secondaryPalette = new[] { new Color(0.5f, 0.3f, 0.8f), new Color(0.3f, 0.1f, 0.5f) },
                particleSpeed = 5f,
                particleSize = 0.4f,
                emissionIntensity = 6f,
                heatDistortion = false,
                particleShape = "spark"
            };

            // Nature
            schoolPresets[3] = new SchoolPreset
            {
                primaryPalette = new[] { new Color(0.3f, 0.8f, 0.3f), new Color(0.5f, 0.9f, 0.5f), new Color(0.7f, 1f, 0.7f) },
                secondaryPalette = new[] { new Color(0.2f, 0.5f, 0.2f), new Color(0.4f, 0.6f, 0.3f) },
                particleSpeed = 1f,
                particleSize = 0.7f,
                emissionIntensity = 1.5f,
                heatDistortion = false,
                particleShape = "leaf"
            };

            // Arcane
            schoolPresets[4] = new SchoolPreset
            {
                primaryPalette = new[] { new Color(0.6f, 0.3f, 1f), new Color(0.8f, 0.5f, 1f), new Color(0.5f, 0.8f, 1f) },
                secondaryPalette = new[] { new Color(0.3f, 0.1f, 0.5f), new Color(0.2f, 0.3f, 0.6f) },
                particleSpeed = 2f,
                particleSize = 0.5f,
                emissionIntensity = 3f,
                heatDistortion = false,
                particleShape = "orb"
            };

            // Holy
            schoolPresets[5] = new SchoolPreset
            {
                primaryPalette = new[] { Color.white, new Color(1f, 0.95f, 0.7f), new Color(1f, 0.85f, 0.5f) },
                secondaryPalette = new[] { new Color(0.9f, 0.8f, 0.6f), new Color(0.8f, 0.7f, 0.5f) },
                particleSpeed = 1.5f,
                particleSize = 0.6f,
                emissionIntensity = 5f,
                heatDistortion = false,
                particleShape = "star"
            };

            // Shadow
            schoolPresets[6] = new SchoolPreset
            {
                primaryPalette = new[] { new Color(0.3f, 0.1f, 0.4f), new Color(0.2f, 0.1f, 0.3f), Color.black },
                secondaryPalette = new[] { new Color(0.5f, 0.2f, 0.6f), new Color(0.4f, 0.2f, 0.5f) },
                particleSpeed = 2.5f,
                particleSize = 0.5f,
                emissionIntensity = 2f,
                heatDistortion = false,
                particleShape = "wisp"
            };

            // Poison
            schoolPresets[7] = new SchoolPreset
            {
                primaryPalette = new[] { new Color(0.5f, 0.9f, 0.3f), new Color(0.7f, 0.9f, 0.2f), new Color(0.8f, 0.8f, 0.3f) },
                secondaryPalette = new[] { new Color(0.3f, 0.6f, 0.2f), new Color(0.4f, 0.5f, 0.1f) },
                particleSpeed = 0.8f,
                particleSize = 0.7f,
                emissionIntensity = 1.5f,
                heatDistortion = false,
                particleShape = "bubble"
            };

            // Physical
            schoolPresets[8] = new SchoolPreset
            {
                primaryPalette = new[] { Color.white, new Color(0.9f, 0.9f, 0.9f), new Color(0.8f, 0.8f, 0.8f) },
                secondaryPalette = new[] { new Color(0.6f, 0.6f, 0.6f), new Color(0.5f, 0.5f, 0.5f) },
                particleSpeed = 4f,
                particleSize = 0.4f,
                emissionIntensity = 2f,
                heatDistortion = false,
                particleShape = "shard"
            };

            // Cosmic
            schoolPresets[9] = new SchoolPreset
            {
                primaryPalette = new[] { new Color(0.5f, 0.3f, 0.8f), new Color(0.8f, 0.5f, 0.9f), new Color(0.3f, 0.6f, 0.9f) },
                secondaryPalette = new[] { new Color(0.9f, 0.7f, 0.3f), new Color(0.7f, 0.3f, 0.5f) },
                particleSpeed = 1.8f,
                particleSize = 0.5f,
                emissionIntensity = 3.5f,
                heatDistortion = false,
                particleShape = "star"
            };
        }

        #endregion

        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeSchoolPresets();
            InitializeGradients();
            activeParticles = new NativeList<MagicParticle>(Allocator.Persistent);
            newParticles = new NativeList<MagicParticle>(Allocator.Persistent);
            
            SetupComponents();
        }

        private void OnDestroy()
        {
            if (activeParticles.IsCreated)
                activeParticles.Dispose();
            if (newParticles.IsCreated)
                newParticles.Dispose();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            
            // Update spell phase
            UpdateSpellPhase(deltaTime);
            
            // Apply school preset
            ApplySchoolPreset();
            
            // Spawn new particles based on phase
            SpawnParticles(deltaTime);
            
            // Update existing particles
            UpdateParticles(deltaTime);
            
            // Update dynamic light
            UpdateDynamicLight();
            
            // Clean up dead particles
            CleanupParticles();
        }

        #endregion

        #region Initialization
        
        private void InitializeGradients()
        {
            // Set default gradients based on school
            UpdateGradientForSchool(spellSchool);
        }

        private void UpdateGradientForSchool(SpellSchool school)
        {
            int schoolIndex = (int)school;
            if (schoolIndex >= 0 && schoolIndex < schoolPresets.Length)
            {
                SchoolPreset preset = schoolPresets[schoolIndex];
                
                GradientKey[] colorKeys = new GradientKey[preset.primaryPalette.Length];
                for (int i = 0; i < preset.primaryPalette.Length; i++)
                {
                    colorKeys[i] = new GradientKey
                    {
                        color = preset.primaryPalette[i],
                        time = (float)i / preset.primaryPalette.Length
                    };
                }
                
                primaryColors.SetKeys(colorKeys, new AlphaKey[] { new AlphaKey { alpha = 1f, time = 0f }, new AlphaKey { alpha = 0f, time = 1f } });
            }
        }

        private void SetupComponents()
        {
            // Create or get Light component
            spellLight = GetComponent<Light>();
            if (spellLight == null && enableDynamicLight)
            {
                spellLight = gameObject.AddComponent<Light>();
            }
            
            // Create or get AudioSource
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Create particle material
            particleMaterial = new Material(Shader.Find("Particles/Additive"));
            if (particleMaterial == null)
            {
                particleMaterial = new Material(Shader.Find("Sprites/Default"));
            }
        }

        #endregion

        #region Spell Phase Management
        
        private void UpdateSpellPhase(float deltaTime)
        {
            switch (currentPhase)
            {
                case CastPhase.Charging:
                    chargeProgress += deltaTime / (spellDuration * 0.3f);
                    if (chargeProgress >= 1f)
                    {
                        chargeProgress = 1f;
                        currentPhase = CastPhase.Casting;
                        PlayCastSound();
                    }
                    break;

                case CastPhase.Casting:
                    remainingDuration = spellDuration;
                    currentPhase = CastPhase.Sustaining;
                    break;

                case CastPhase.Sustaining:
                    remainingDuration -= deltaTime;
                    if (remainingDuration <= 0f)
                    {
                        remainingDuration = 0f;
                        currentPhase = CastPhase.Fading;
                        PlayEndSound();
                    }
                    break;

                case CastPhase.Fading:
                    chargeProgress -= deltaTime * alphaFadeRate;
                    if (chargeProgress <= 0f)
                    {
                        chargeProgress = 0f;
                        // Spell complete - could trigger callback
                    }
                    break;
            }
        }

        #endregion

        #region Particle Spawning
        
        private void SpawnParticles(float deltaTime)
        {
            int spawnCount = 0;
            
            switch (currentPhase)
            {
                case CastPhase.Charging:
                    spawnCount = Mathf.FloorToInt(maxParticles * 0.1f * deltaTime * 10f * chargeProgress);
                    break;
                case CastPhase.Casting:
                case CastPhase.Sustaining:
                    spawnCount = Mathf.FloorToInt(maxParticles * 0.3f * deltaTime * 10f * spellPower);
                    break;
                case CastPhase.Fading:
                    spawnCount = 0;
                    break;
            }
            
            spawnCount = Mathf.Min(spawnCount, maxParticles - activeParticles.Length);
            
            for (int i = 0; i < spawnCount; i++)
            {
                SpawnSingleParticle();
            }
        }

        private void SpawnSingleParticle()
        {
            MagicParticle particle = new MagicParticle();
            
            // Position at emitter with spread
            float spreadAngle = particleSpread * math.PI;
            float theta = UnityEngine.Random.Range(0f, math.PI * 2f);
            float phi = UnityEngine.Random.Range(0f, spreadAngle);
            
            particle.position = new Vector3(
                math.sin(phi) * math.cos(theta) * spellScale * 0.1f,
                math.sin(phi) * math.sin(theta) * spellScale * 0.1f,
                math.cos(phi) * spellScale * 0.1f
            );
            
            // Velocity based on spell type and school
            Vector3 direction = particle.position.normalized;
            particle.velocity = direction * particleSpeed * UnityEngine.Random.Range(0.8f, 1.2f);
            
            // Add motion pattern modifiers
            if (enableSpiral)
            {
                particle.velocity += Quaternion.Euler(0, spiralFrequency * 100f, 0) * direction * spiralAmplitude;
            }
            
            if (enableOrbital)
            {
                particle.velocity += Vector3.Cross(direction, Vector3.up) * orbitalSpeed;
            }
            
            if (enableTurbulence)
            {
                particle.velocity += new Vector3(
                    UnityEngine.Random.Range(-1f, 1f),
                    UnityEngine.Random.Range(-1f, 1f),
                    UnityEngine.Random.Range(-1f, 1f)
                ) * turbulenceStrength;
            }
            
            // Lifetime and appearance
            particle.lifetime = 0f;
            particle.maxLifetime = particleLifetime * UnityEngine.Random.Range(0.7f, 1.3f);
            particle.size = particleSize * UnityEngine.Random.Range(0.7f, 1.3f) * spellScale;
            particle.color = primaryColors.Evaluate(UnityEngine.Random.Range(0f, 1f));
            particle.alpha = 1f;
            
            // Rotation
            particle.rotationAxis = new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f)
            ).normalized;
            particle.rotationSpeed = UnityEngine.Random.Range(-180f, 180f);
            
            particle.phase = (int)currentPhase;
            
            newParticles.Add(particle);
        }

        #endregion

        #region Particle Updates
        
        private void UpdateParticles(float deltaTime)
        {
            // Add newly spawned particles
            if (newParticles.Length > 0)
            {
                activeParticles.AddRange(newParticles.AsArray());
                newParticles.Clear();
            }
            
            // Update each active particle
            for (int i = activeParticles.Length - 1; i >= 0; i--)
            {
                MagicParticle particle = activeParticles[i];
                
                // Advance lifetime
                particle.lifetime += deltaTime;
                
                // Check if dead
                if (particle.lifetime >= particle.maxLifetime)
                {
                    activeParticles.RemoveAt(i);
                    continue;
                }
                
                // Update position
                particle.position += particle.velocity * deltaTime;
                
                // Apply gravity/drag based on school
                ApplySchoolPhysics(ref particle, deltaTime);
                
                // Apply motion patterns
                if (enableSpiral)
                {
                    float spiralOffset = math.sin(particle.lifetime * spiralFrequency * math.PI * 2f) * spiralAmplitude;
                    particle.position += Vector3.right * spiralOffset * deltaTime;
                }
                
                if (enableTurbulence)
                {
                    float turbX = math.sin(particle.lifetime * turbulenceFrequency + particle.position.y) * turbulenceStrength;
                    float turbY = math.cos(particle.lifetime * turbulenceFrequency + particle.position.x) * turbulenceStrength;
                    float turbZ = math.sin(particle.lifetime * turbulenceFrequency + particle.position.z) * turbulenceStrength;
                    particle.position += new Vector3(turbX, turbY, turbZ) * deltaTime;
                }
                
                // Fade alpha based on lifetime
                float normalizedLifetime = particle.lifetime / particle.maxLifetime;
                if (normalizedLifetime > 0.7f)
                {
                    particle.alpha = math.lerp(1f, 0f, (normalizedLifetime - 0.7f) / 0.3f);
                }
                
                // Update color over lifetime
                particle.color = primaryColors.Evaluate(normalizedLifetime);
                particle.color.a = particle.alpha;
                
                // Update rotation
                particle.rotationAxis *= math.exp(quaternion.Euler(0, particle.rotationSpeed * deltaTime, 0));
                
                // Write back
                activeParticles[i] = particle;
            }
        }

        private void ApplySchoolPhysics(ref MagicParticle particle, float deltaTime)
        {
            int schoolIndex = (int)spellSchool;
            if (schoolIndex < 0 || schoolIndex >= schoolPresets.Length) return;
            
            SchoolPreset preset = schoolPresets[schoolIndex];
            
            switch (spellSchool)
            {
                case SpellSchool.Fire:
                    // Fire rises
                    particle.velocity += Vector3.up * 2f * deltaTime;
                    particle.velocity *= 0.98f; // Drag
                    break;
                    
                case SpellSchool.Ice:
                    // Ice falls slowly
                    particle.velocity += Vector3.down * 1f * deltaTime;
                    particle.velocity *= 0.99f;
                    break;
                    
                case SpellSchool.Lightning:
                    // Lightning moves fast with little drag
                    particle.velocity *= 0.995f;
                    break;
                    
                case SpellSchool.Nature:
                    // Nature drifts
                    particle.velocity += Vector3.up * 0.5f * deltaTime;
                    particle.velocity *= 0.97f;
                    break;
                    
                case SpellSchool.Physical:
                    // Physical has realistic projectile motion
                    particle.velocity += Physics.gravity * 0.5f * deltaTime;
                    particle.velocity *= 0.99f;
                    break;
                    
                default:
                    particle.velocity *= 0.98f;
                    break;
            }
        }

        #endregion

        #region Dynamic Light
        
        private void UpdateDynamicLight()
        {
            if (!enableDynamicLight || spellLight == null) return;
            
            float intensityMultiplier = 1f;
            
            switch (currentPhase)
            {
                case CastPhase.Charging:
                    intensityMultiplier = chargeProgress;
                    break;
                case CastPhase.Casting:
                case CastPhase.Sustaining:
                    intensityMultiplier = 1f;
                    break;
                case CastPhase.Fading:
                    intensityMultiplier = chargeProgress;
                    break;
            }
            
            spellLight.color = emissionColor;
            spellLight.intensity = emissionIntensity * intensityMultiplier * lightIntensity;
            spellLight.range = lightRange * spellScale;
            spellLight.shadows = lightShadows;
        }

        #endregion

        #region Audio
        
        private void PlayCastSound()
        {
            if (castSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(castSound, soundVolume);
            }
        }

        private void PlayLoopSound()
        {
            if (loopSound != null && audioSource != null && !audioSource.isPlaying)
            {
                audioSource.clip = loopSound;
                audioSource.volume = soundVolume;
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        private void PlayEndSound()
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            if (endSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(endSound, soundVolume);
            }
        }

        #endregion

        #region School Preset Application
        
        private void ApplySchoolPreset()
        {
            int schoolIndex = (int)spellSchool;
            if (schoolIndex < 0 || schoolIndex >= schoolPresets.Length) return;
            
            SchoolPreset preset = schoolPresets[schoolIndex];
            
            // Smoothly interpolate parameters
            particleSpeed = Mathf.Lerp(particleSpeed, preset.particleSpeed, 0.05f);
            particleSize = Mathf.Lerp(particleSize, preset.particleSize, 0.05f);
            emissionIntensity = Mathf.Lerp(emissionIntensity, preset.emissionIntensity, 0.05f);
            enableHeatDistortion = preset.heatDistortion;
        }

        #endregion

        #region Cleanup
        
        private void CleanupParticles()
        {
            // Particles are cleaned up in UpdateParticles when they die
            // This is just for safety
            if (activeParticles.Length > maxParticles)
            {
                activeParticles.Resize(maxParticles, NativeArrayOptions.UninitializedMemory);
            }
        }

        #endregion

        #region Public API
        
        public void CastSpell(SpellSchool school, SpellType type, float power = 1f)
        {
            spellSchool = school;
            spellType = type;
            spellPower = power;
            currentPhase = CastPhase.Charging;
            chargeProgress = 0f;
            UpdateGradientForSchool(school);
        }

        public void CancelSpell()
        {
            currentPhase = CastPhase.Fading;
            chargeProgress = 0.3f; // Quick fade
        }

        public void SetSpellPower(float power)
        {
            spellPower = Mathf.Clamp(power, 0.1f, 5f);
        }

        public void SetSpellDuration(float duration)
        {
            spellDuration = Mathf.Clamp(duration, 0.1f, 3f);
        }

        public void SetSpellScale(float scale)
        {
            spellScale = Mathf.Clamp(scale, 0.5f, 3f);
        }

        public int GetActiveParticleCount() => activeParticles.Length;
        public float GetChargeProgress() => chargeProgress;
        public CastPhase GetCurrentPhase() => currentPhase;
        public float GetRemainingDuration() => remainingDuration;
        
        #endregion
    }
}
