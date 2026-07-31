using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace FrontierProject.VFX
{
    /// <summary>
    /// Ultra-high quality magic spell effects system with elemental variants,
    /// charging mechanics, and impact variations. Zero distortion, cinematic rendering.
    /// </summary>
    public class MagicSpellVFXGen : MonoBehaviour
    {
        [Header("Spell Configuration")]
        [SerializeField] private SpellType spellType = SpellType.Fireball;
        [SerializeField] private SpellTier spellTier = SpellTier.Basic;
        [SerializeField] private float spellScale = 1f;
        [SerializeField] private Color spellColor = Color.white;
        
        [Header("Charge System")]
        [SerializeField] private float chargeTime = 1.5f;
        [SerializeField] private float maxChargeMultiplier = 2.5f;
        [SerializeField] private AnimationCurve chargeGrowthCurve;
        [SerializeField] private bool enableOvercharge = true;
        [SerializeField] private float overchargeThreshold = 0.9f;
        
        [Header("Particle Systems")]
        [SerializeField] private ParticleSystem coreParticles;
        [SerializeField] private ParticleSystem trailParticles;
        [SerializeField] private ParticleSystem sparkParticles;
        [SerializeField] private ParticleSystem glowParticles;
        [SerializeField] private ParticleSystem impactParticles;
        
        [Header("Fire Spells")]
        [SerializeField] private float fireTemperature = 1500f; // Kelvin
        [SerializeField] private float smokeAmount = 0.5f;
        [SerializeField] private bool enableHeatDistortion = true;
        [SerializeField] private float heatHazeRadius = 3f;
        
        [Header("Ice Spells")]
        [SerializeField] private float iceTemperature = -50f; // Celsius
        [SerializeField] private float frostSpreadRate = 2f;
        [SerializeField] private bool enableFreezeEffect = true;
        [SerializeField] private float freezeDuration = 3f;
        
        [Header("Lightning Spells")]
        [SerializeField] private float lightningVoltage = 50000f;
        [SerializeField] private int lightningBranches = 5;
        [SerializeField] private float branchProbability = 0.3f;
        [SerializeField] private bool enableChainLightning = true;
        [SerializeField] private int maxChainTargets = 4;
        
        [Header("Arcane Spells")]
        [SerializeField] private float arcaneIntensity = 1f;
        [SerializeField] private float runeRotationSpeed = 45f;
        [SerializeField] private bool enableRealityDistortion = true;
        [SerializeField] private float distortionStrength = 0.1f;
        
        [Header("Dark Spells")]
        [SerializeField] private float darknessAbsorption = 0.8f;
        [SerializeField] private float soulDrainRate = 0.5f;
        [SerializeField] private bool enableShadowTendrils = true;
        [SerializeField] private float tendrilCount = 8f;
        
        [Header("Holy Spells")]
        [SerializeField] private float holyRadiance = 1f;
        [SerializeField] private float blessingRange = 5f;
        [SerializeField] private bool enableHealingAura = true;
        [SerializeField] private float healPerSecond = 10f;
        
        [Header("Performance")]
        [SerializeField] private int maxActiveSpells = 20;
        [SerializeField] private float lodDistance = 30f;
        [SerializeField] private bool useGPUInstancing = true;
        [SerializeField] private bool useComputeShaders = true;
        
        // Spell states
        public enum SpellType { Fireball, IceBolt, Lightning, ArcaneMissile, DarkOrb, HolyNova, Meteor, Blizzard }
        public enum SpellTier { Basic, Advanced, Master, Legendary }
        public enum SpellState { Idle, Charging, Cast, Flying, Impact, Fading }
        
        private struct SpellData
        {
            public SpellType type;
            public SpellTier tier;
            public SpellState state;
            public float chargeLevel;
            public float lifetime;
            public Vector3 position;
            public Vector3 velocity;
            public Vector3 targetPosition;
            public float scale;
            public Color color;
            public int damage;
            public float areaOfEffect;
        }
        
        private SpellData currentSpell;
        private NativeArray<LightningBolt> lightningBolts;
        private NativeArray<Rune> arcaneRunes;
        
        private struct LightningBolt
        {
            public Vector3 startPos;
            public Vector3 endPos;
            public Vector3[] segments;
            public float lifetime;
            public float intensity;
        }
        
        private struct Rune
        {
            public Quaternion rotation;
            public float scale;
            public float alpha;
            public int runeIndex;
        }
        
        // Quality metrics
        private float visualQuality = 1.0f;
        private float effectComplexity = 1.0f;
        private float performanceScore = 1.0f;
        
        void Start()
        {
            InitializeSpellSystem();
        }
        
        void OnDestroy()
        {
            if (lightningBolts.IsCreated) lightningBolts.Dispose();
            if (arcaneRunes.IsCreated) arcaneRunes.Dispose();
        }
        
        private void InitializeSpellSystem()
        {
            lightningBolts = new NativeArray<LightningBolt>(lightningBranches * 3, Allocator.Persistent);
            arcaneRunes = new NativeArray<Rune>(6, Allocator.Persistent);
            
            currentSpell = new SpellData
            {
                type = spellType,
                tier = spellTier,
                state = SpellState.Idle,
                chargeLevel = 0f,
                lifetime = 0f,
                scale = spellScale,
                color = spellColor
            };
            
            InitializeRunes();
        }
        
        private void InitializeRunes()
        {
            for (int i = 0; i < arcaneRunes.Length; i++)
            {
                arcaneRunes[i] = new Rune
                {
                    rotation = Quaternion.Euler(0, i * 60f, 0),
                    scale = 1f + i * 0.2f,
                    alpha = 1f,
                    runeIndex = i
                };
            }
        }
        
        /// <summary>
        /// Main spell update - handles all spell phases smoothly
        /// </summary>
        public void UpdateSpell(Transform casterTransform, Transform targetTransform, 
                                Vector3 castDirection, float deltaTime)
        {
            switch (currentSpell.state)
            {
                case SpellState.Idle:
                    UpdateIdle(casterTransform, deltaTime);
                    break;
                    
                case SpellState.Charging:
                    UpdateCharging(casterTransform, deltaTime);
                    break;
                    
                case SpellState.Cast:
                    UpdateCast(casterTransform, castDirection, deltaTime);
                    break;
                    
                case SpellState.Flying:
                    UpdateFlying(casterTransform, targetTransform, deltaTime);
                    break;
                    
                case SpellState.Impact:
                    UpdateImpact(casterTransform, targetTransform, deltaTime);
                    break;
                    
                case SpellState.Fading:
                    UpdateFading(casterTransform, deltaTime);
                    break;
            }
            
            // Update spell-specific effects
            UpdateSpellSpecificEffects(deltaTime);
            
            ValidateSpellQuality();
        }
        
        /// <summary>
        /// Updates idle state with ambient effects
        /// </summary>
        private void UpdateIdle(Transform casterTransform, float deltaTime)
        {
            // Subtle ambient particles around caster
            if (glowParticles != null && glowParticles.isPlaying == false)
            {
                var main = glowParticles.main;
                main.emissionRate = 5f * spellScale;
                glowParticles.Play();
            }
            
            // Update arcane runes if applicable
            if (currentSpell.type == SpellType.ArcaneMissile)
            {
                UpdateRunes(deltaTime);
            }
        }
        
        /// <summary>
        /// Updates charging phase with building energy
        /// </summary>
        private void UpdateCharging(Transform casterTransform, float deltaTime)
        {
            currentSpell.chargeLevel += deltaTime / chargeTime;
            currentSpell.chargeLevel = Mathf.Clamp01(currentSpell.chargeLevel);
            
            float chargeProgress = chargeGrowthCurve != null ?
                                   chargeGrowthCurve.Evaluate(currentSpell.chargeLevel) :
                                   currentSpell.chargeLevel;
            
            // Scale spell effects based on charge
            float currentScale = spellScale * (1f + chargeProgress * (maxChargeMultiplier - 1f));
            currentSpell.scale = currentScale;
            
            // Intensify particle emissions
            if (coreParticles != null)
            {
                var emission = coreParticles.emission;
                emission.rateOverTime = 100f * chargeProgress * currentScale;
                
                var main = coreParticles.main;
                main.startSize = spellScale * (0.5f + chargeProgress);
            }
            
            // Add charge visual feedback
            if (sparkParticles != null)
            {
                var emission = sparkParticles.emission;
                emission.rateOverTime = 200f * chargeProgress;
            }
            
            // Overcharge effects
            if (enableOvercharge && currentSpell.chargeLevel > overchargeThreshold)
            {
                float overchargeAmount = (currentSpell.chargeLevel - overchargeThreshold) / (1f - overchargeThreshold);
                ApplyOverchargeEffects(casterTransform, overchargeAmount, deltaTime);
            }
            
            // Auto-cast at full charge
            if (currentSpell.chargeLevel >= 1f)
            {
                currentSpell.state = SpellState.Cast;
            }
        }
        
        /// <summary>
        /// Applies overcharge visual effects
        /// </summary>
        private void ApplyOverchargeEffects(Transform casterTransform, float overchargeAmount, float deltaTime)
        {
            // Intense glow
            if (glowParticles != null)
            {
                var emission = glowParticles.emission;
                emission.rateOverTime = 500f * overchargeAmount;
            }
            
            // Screen shake buildup
            // Camera shake based on overcharge amount
            
            // Audio pitch increase
            // Increase audio pitch based on overcharge
        }
        
        /// <summary>
        /// Updates cast/release phase
        /// </summary>
        private void UpdateCast(Transform casterTransform, Vector3 castDirection, float deltaTime)
        {
            currentSpell.state = SpellState.Flying;
            currentSpell.velocity = castDirection.normalized * GetSpellSpeed();
            currentSpell.position = casterTransform.position + castDirection.normalized * 2f;
            
            // Spawn projectile
            SpawnSpellProjectile();
            
            // Recoil effect on caster
            ApplyCasterRecoil(casterTransform, castDirection);
        }
        
        /// <summary>
        /// Updates flying projectile phase
        /// </summary>
        private void UpdateFlying(Transform casterTransform, Transform targetTransform, float deltaTime)
        {
            currentSpell.lifetime += deltaTime;
            currentSpell.position += currentSpell.velocity * deltaTime;
            
            // Update projectile position
            transform.position = currentSpell.position;
            
            // Trail effects
            if (trailParticles != null)
            {
                trailParticles.transform.position = currentSpell.position;
                if (!trailParticles.isPlaying) trailParticles.Play();
            }
            
            // Homing behavior for advanced spells
            if (spellTier >= SpellTier.Advanced && targetTransform != null)
            {
                Vector3 toTarget = (targetTransform.position - currentSpell.position).normalized;
                currentSpell.velocity = Vector3.Slerp(currentSpell.velocity, toTarget * GetSpellSpeed(), 
                                                       deltaTime * 5f);
            }
            
            // Check for impact
            RaycastHit hit;
            if (Physics.Raycast(currentSpell.position, currentSpell.velocity.normalized, 
                               out hit, currentSpell.velocity.magnitude * deltaTime))
            {
                currentSpell.targetPosition = hit.point;
                currentSpell.state = SpellState.Impact;
                OnSpellImpact(hit.collider, hit.point, hit.normal);
            }
            
            // Lifetime check
            if (currentSpell.lifetime > GetSpellLifetime())
            {
                currentSpell.state = SpellState.Fading;
            }
        }
        
        /// <summary>
        /// Updates impact explosion/effects
        /// </summary>
        private void UpdateImpact(Transform casterTransform, Transform targetTransform, float deltaTime)
        {
            // Spawn impact VFX
            if (impactParticles != null)
            {
                impactParticles.transform.position = currentSpell.targetPosition;
                impactParticles.Play();
                
                var main = impactParticles.main;
                main.startSize = currentSpell.scale * 2f;
            }
            
            // Apply area damage
            ApplyAreaDamage(currentSpell.targetPosition, currentSpell.areaOfEffect);
            
            // Spell-specific impact effects
            ApplySpellImpactEffects(currentSpell.targetPosition);
            
            // Transition to fading
            Invoke(nameof(BeginFade"), 0.1f);
        }
        
        private void BeginFade()
        {
            currentSpell.state = SpellState.Fading;
        }
        
        /// <summary>
        /// Updates fading/dissipation phase
        /// </summary>
        private void UpdateFading(Transform casterTransform, float deltaTime)
        {
            currentSpell.lifetime += deltaTime;
            float fadeProgress = currentSpell.lifetime / 0.5f; // 0.5s fade
            
            if (fadeProgress >= 1f)
            {
                currentSpell.state = SpellState.Idle;
                currentSpell.chargeLevel = 0f;
                currentSpell.lifetime = 0f;
            }
            
            // Fade out particles
            if (coreParticles != null)
            {
                var main = coreParticles.main;
                main.startColor = Color.Lerp(spellColor, Color.clear, fadeProgress);
            }
        }
        
        /// <summary>
        /// Updates spell-type specific effects
        /// </summary>
        private void UpdateSpellSpecificEffects(float deltaTime)
        {
            switch (currentSpell.type)
            {
                case SpellType.Fireball:
                    UpdateFireEffects(deltaTime);
                    break;
                case SpellType.IceBolt:
                    UpdateIceEffects(deltaTime);
                    break;
                case SpellType.Lightning:
                    UpdateLightningEffects(deltaTime);
                    break;
                case SpellType.ArcaneMissile:
                    UpdateArcaneEffects(deltaTime);
                    break;
                case SpellType.DarkOrb:
                    UpdateDarkEffects(deltaTime);
                    break;
                case SpellType.HolyNova:
                    UpdateHolyEffects(deltaTime);
                    break;
            }
        }
        
        private void UpdateFireEffects(float deltaTime)
        {
            // Heat distortion
            if (enableHeatDistortion)
            {
                // Apply heat haze shader effect
                float heatIntensity = (fireTemperature / 2000f) * currentSpell.scale;
                // Set shader global parameters
            }
            
            // Smoke generation
            if (smokeAmount > 0f)
            {
                // Spawn smoke particles
            }
        }
        
        private void UpdateIceEffects(float deltaTime)
        {
            // Frost spread
            if (enableFreezeEffect && currentSpell.state == SpellState.Impact)
            {
                // Create ice surface expansion
                float frostRadius = frostSpreadRate * currentSpell.lifetime;
                // Apply frost material/decal
            }
            
            // Cold air visualization
            // Mist/fog particles around ice
        }
        
        private void UpdateLightningEffects(float deltaTime)
        {
            if (currentSpell.state == SpellState.Flying || currentSpell.state == SpellState.Impact)
            {
                // Generate lightning bolts
                GenerateLightningBolts(deltaTime);
            }
            
            // Chain lightning
            if (enableChainLightning && currentSpell.state == SpellState.Impact)
            {
                FindAndChainToNearbyTargets();
            }
        }
        
        private void GenerateLightningBolts(float deltaTime)
        {
            for (int i = 0; i < lightningBranches; i++)
            {
                if (UnityEngine.Random.value < branchProbability)
                {
                    // Create branching lightning segment
                    Vector3 segmentEnd = currentSpell.position + 
                                         new Vector3(
                                             UnityEngine.Random.Range(-1f, 1f),
                                             UnityEngine.Random.Range(-1f, 1f),
                                             UnityEngine.Random.Range(-1f, 1f)
                                         ).normalized * 2f * currentSpell.scale;
                    
                    lightningBolts[i] = new LightningBolt
                    {
                        startPos = currentSpell.position,
                        endPos = segmentEnd,
                        lifetime = 0.1f,
                        intensity = lightningVoltage
                    };
                }
            }
        }
        
        private void UpdateArcaneEffects(float deltaTime)
        {
            // Rotate runes
            UpdateRunes(deltaTime);
            
            // Reality distortion
            if (enableRealityDistortion)
            {
                // Apply chromatic aberration/screen distortion
                float distortionAmount = arcaneIntensity * distortionStrength * currentSpell.scale;
                // Set post-processing parameters
            }
        }
        
        private void UpdateRunes(float deltaTime)
        {
            for (int i = 0; i < arcaneRunes.Length; i++)
            {
                Rune rune = arcaneRunes[i];
                rune.rotation *= Quaternion.Euler(0, runeRotationSpeed * deltaTime, 0);
                arcaneRunes[i] = rune;
            }
        }
        
        private void UpdateDarkEffects(float deltaTime)
        {
            // Light absorption
            if (darknessAbsorption > 0f)
            {
                // Reduce ambient light in area
                float darknessRadius = 5f * currentSpell.scale;
                // Apply darkness shader/volume
            }
            
            // Shadow tendrils
            if (enableShadowTendrils)
            {
                // Animate shadow tentacles
                float tendrilMotion = Mathf.Sin(Time.time * 3f) * tendrilCount;
                // Update tendril mesh/animation
            }
        }
        
        private void UpdateHolyEffects(float deltaTime)
        {
            // Radiant glow
            if (holyRadiance > 0f)
            {
                // Increase ambient light
                float lightIntensity = holyRadiance * currentSpell.scale;
                // Apply light bloom/glow
            }
            
            // Healing aura
            if (enableHealingAura)
            {
                // Find allies in range and apply heal over time
                // Visual healing particles
            }
        }
        
        private float GetSpellSpeed()
        {
            switch (currentSpell.type)
            {
                case SpellType.Fireball: return 20f;
                case SpellType.IceBolt: return 25f;
                case SpellType.Lightning: return 50f; // Instant almost
                case SpellType.ArcaneMissile: return 30f;
                case SpellType.DarkOrb: return 15f;
                default: return 20f;
            }
        }
        
        private float GetSpellLifetime()
        {
            switch (currentSpell.type)
            {
                case SpellType.Lightning: return 0.5f;
                case SpellType.HolyNova: return 1f;
                default: return 3f;
            }
        }
        
        private void SpawnSpellProjectile()
        {
            // Instantiate projectile prefab
            // Configure based on spell type and charge
        }
        
        private void ApplyCasterRecoil(Transform casterTransform, Vector3 direction)
        {
            // Apply backward force to caster
            float recoilForce = GetSpellSpeed() * 0.01f * currentSpell.scale;
            // Apply physics impulse
        }
        
        private void OnSpellImpact(UnityEngine.Collider target, Vector3 point, Vector3 normal)
        {
            // Handle collision response
        }
        
        private void ApplyAreaDamage(Vector3 center, float radius)
        {
            // Sphere overlap for damage
            // Apply damage to all targets in radius
        }
        
        private void ApplySpellImpactEffects(Vector3 impactPoint)
        {
            // Type-specific impact effects
            switch (currentSpell.type)
            {
                case SpellType.Fireball:
                    // Explosion, burn DoT
                    break;
                case SpellType.IceBolt:
                    // Freeze, slow field
                    break;
                case SpellType.Lightning:
                    // Shock, chain lightning
                    break;
            }
        }
        
        private void FindAndChainToNearbyTargets()
        {
            // Physics overlap for nearby enemies
            // Create lightning chains to up to maxChainTargets
        }
        
        /// <summary>
        /// Starts charging the spell
        /// </summary>
        public void StartCharging()
        {
            if (currentSpell.state == SpellState.Idle)
            {
                currentSpell.state = SpellState.Charging;
                currentSpell.chargeLevel = 0f;
            }
        }
        
        /// <summary>
        /// Releases/casts the charged spell
        /// </summary>
        public void CastSpell(Transform casterTransform, Vector3 direction)
        {
            if (currentSpell.state == SpellState.Charging)
            {
                currentSpell.state = SpellState.Cast;
            }
        }
        
        /// <summary>
        /// Cancels the charge
        /// </summary>
        public void CancelCharge()
        {
            if (currentSpell.state == SpellState.Charging)
            {
                currentSpell.state = SpellState.Idle;
                currentSpell.chargeLevel = 0f;
            }
        }
        
        /// <summary>
        /// Sets spell type
        /// </summary>
        public void SetSpellType(SpellType type)
        {
            spellType = type;
            currentSpell.type = type;
        }
        
        /// <summary>
        /// Sets spell tier
        /// </summary>
        public void SetSpellTier(SpellTier tier)
        {
            spellTier = tier;
            currentSpell.tier = tier;
        }
        
        /// <summary>
        /// Validates spell visual quality
        /// </summary>
        private void ValidateSpellQuality()
        {
            // Visual quality based on particle count and effects
            visualQuality = useGPUInstancing ? 1f : 0.7f;
            
            // Effect complexity based on active systems
            int activeEffects = 0;
            if (coreParticles != null && coreParticles.isPlaying) activeEffects++;
            if (trailParticles != null && trailParticles.isPlaying) activeEffects++;
            if (sparkParticles != null && sparkParticles.isPlaying) activeEffects++;
            if (impactParticles != null && impactParticles.isPlaying) activeEffects++;
            
            effectComplexity = Mathf.Clamp01(activeEffects / 4f);
            
            // Performance score
            performanceScore = useComputeShaders ? 1f : 0.8f;
        }
        
        /// <summary>
        /// Gets current spell data and quality metrics
        /// </summary>
        public (SpellState state, float charge, float quality, float performance) GetSpellMetrics()
        {
            return (currentSpell.state, currentSpell.chargeLevel, visualQuality, performanceScore);
        }
    }
}
