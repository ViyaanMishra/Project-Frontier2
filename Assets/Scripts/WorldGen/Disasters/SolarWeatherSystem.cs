using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace AdvancedSimulation.Disasters
{
    /// <summary>
    /// Solar Weather System simulating coronal mass ejections (CMEs), solar flares,
    /// geomagnetic storms, radiation belts, and impacts on power grids and satellites.
    /// </summary>
    [Serializable]
    public struct SolarWeatherComponent : IComponentData
    {
        // Solar Cycle Phase
        public float SolarCyclePhase; // 0-1, 0=solar min, 0.5=solar max
        public int SolarCycleNumber; // ~11 year cycle
        public float SunspotCount;
        public float SolarFlareProbability; // Daily probability of X-class flare
        
        // CME Tracking
        public int ActiveCMEs;
        public float LargestCMEVelocity; // km/s
        public float EarthDirectedCMEsYTD;
        
        // Geomagnetic Indices
        public float KpIndex; // 0-9, planetary magnetic activity
        public float DstIndex; // nT, ring current strength (negative = storm)
        public float APOIndex; // Auroral electrojet
        
        // Radiation Environment
        public float GOESProtonFlux; // >10 MeV protons / cm²/s/sr
        public float ElectronFluxMeV; // >2 MeV electrons at GEO
        public float RadiationBeltIntensity; // Van Allen belts
        
        // Impact Metrics
        public float PowerGridStressIndex; // 0-1
        public int SatellitesAtRisk;
        public float AviationRadiationDose; // μSv/hour at cruise altitude
        public float GNSSDegradationPercent; // GPS accuracy loss
        public float HF RadioBlackoutSeverity; // 0-1
    }

    [Serializable]
    public struct CMEventElement : IBufferElementData
    {
        public int EventType; // 0=CME, 1=SolarFlare, 2=SEP(SolarEnergeticParticles)
        public float Magnitude; // X-class for flares, speed for CMEs
        public DateTime LaunchTime;
        public DateTime ExpectedArrival;
        public bool IsEarthDirected;
        public float MagneticFieldBz; // Southward Bz = geoeffective (nT)
        public float PlasmaDensity; // protons/cm³
        public float PlasmaVelocity; // km/s
        public float ImpactSeverity; // 0-1
    }

    [Serializable]
    public struct GroundInducedCurrentElement : IBufferElementData
    {
        public Entity GridRegion;
        public float GICMagnitude; // Amps per km
        public float TransformerHeatingRate; // °C/hour
        public bool IsAtRiskOfDamage;
        public float LatitudeFactor; // Higher latitudes = more vulnerable
        public float GeologyFactor; // Resistive bedrock = higher GIC
        public float MitigationActive; // 0-1, capacitor banks engaged
    }

    public class SolarWeatherSystem : SystemBase
    {
        private Random _random;

        protected override void OnCreate()
        {
            _random = new Random((uint)DateTime.Now.Ticks);
        }

        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime;
            var random = _random;

            // Update Macro Solar Weather State
            Entities
                .WithAll<SolarWeatherComponent>()
                .ForEach((ref SolarWeatherComponent solar) =>
                {
                    // 1. Solar Cycle Progression (~11 years)
                    float cyclePeriod = 11f * 365f; // days
                    float cycleProgress = deltaTime / cyclePeriod;
                    
                    solar.SolarCyclePhase = (solar.SolarCyclePhase + cycleProgress) % 1f;
                    
                    // Sunspot count follows sine-like pattern
                    float sunspotBase = math.sin(solar.SolarCyclePhase * 2f * math.PI);
                    solar.SunspotCount = math.max(0, 50f + (sunspotBase * 100f));
                    
                    // Flare probability peaks at solar max
                    solar.SolarFlareProbability = 0.01f + (math.abs(sunspotBase) * 0.1f);

                    // 2. CME Arrival Processing
                    if (solar.ActiveCMEs > 0)
                    {
                        // Chance of CME arrival per tick
                        float arrivalChance = 0.05f * deltaTime;
                        if (random.NextFloat() < arrivalChance)
                        {
                            solar.ActiveCMEs--;
                            
                            // Trigger geomagnetic storm if Earth-directed
                            if (solar.EarthDirectedCMEsYTD > 0)
                            {
                                // Storm intensity depends on Bz and velocity
                                float stormIntensity = random.NextFloat(0.3f, 1f);
                                solar.KpIndex = math.min(9f, stormIntensity * 9f);
                                solar.DstIndex = -stormIntensity * 200f; // Can reach -400 in extreme
                                
                                // Radiation spike
                                solar.GOESProtonFlux *= (1f + stormIntensity * 10f);
                            }
                        }
                    }
                    else
                    {
                        // Quiet time decay
                        solar.KpIndex = math.max(0f, solar.KpIndex - deltaTime * 0.1f);
                        solar.DstIndex = math.lerp(solar.DstIndex, 0f, deltaTime * 0.01f);
                        solar.GOESProtonFlux = math.lerp(solar.GOESProtonFlux, 1f, deltaTime * 0.05f);
                    }

                    // 3. Radiation Belt Response
                    // Geomagnetic storms inject particles into belts
                    if (solar.KpIndex > 5f)
                    {
                        solar.RadiationBeltIntensity = math.min(1f, 
                            solar.RadiationBeltIntensity + deltaTime * 0.05f);
                        solar.ElectronFluxMeV *= (1f + deltaTime * 0.1f);
                    }
                    else
                    {
                        solar.RadiationBeltIntensity = math.max(0f, 
                            solar.RadiationBeltIntensity - deltaTime * 0.01f);
                    }

                    // 4. Power Grid Stress
                    // GICs flow during geomagnetic storms, especially at high latitudes
                    float gicDriver = math.max(0f, solar.KpIndex - 4f) * 0.1f;
                    solar.PowerGridStressIndex = math.min(1f, gicDriver);
                    
                    if (solar.PowerGridStressIndex > 0.7f)
                    {
                        // Risk of transformer damage, voltage instability
                        solar.SatellitesAtRisk = (int)(solar.SatellitesAtRisk * 1.1f);
                    }

                    // 5. Aviation Radiation
                    // Increases during solar particle events and at high latitudes/altitudes
                    float baseDose = 3f; // μSv/hour normal cruise
                    float stormDose = solar.GOESProtonFlux * 0.01f;
                    solar.AviationRadiationDose = baseDose + stormDose;
                    
                    // Flight diversions needed if dose too high (>25 μSv/hour)
                    if (solar.AviationRadiationDose > 25f)
                    {
                        // Polar routes must be diverted
                    }

                    // 6. GNSS/GPS Degradation
                    // Ionospheric scintillation during storms
                    float ionosphericDisturbance = math.max(0f, solar.KpIndex - 3f) * 0.1f;
                    solar.GNSSDegradationPercent = math.min(50f, ionosphericDisturbance * 20f);
                    
                    // HF Radio Blackouts during X-ray flares
                    if (solar.SolarFlareProbability > 0.05f)
                    {
                        solar.HF RadioBlackoutSeverity = math.min(1f, 
                            solar.SolarFlareProbability * 5f);
                    }
                    else
                    {
                        solar.HF RadioBlackoutSeverity = math.max(0f, 
                            solar.HF RadioBlackoutSeverity - deltaTime * 0.1f);
                    }

                    _random = random;
                }).WithoutBurst().Run();

            // Update Individual CME/Flare Events
            Entities
                .WithAll<CMEventElement>()
                .ForEach((ref CMEventElement evt) =>
                {
                    // Propagate CME through space
                    if (evt.EventType == 0 && !evt.IsEarthDirected)
                    {
                        // Skip non-Earth-directed for now
                        return;
                    }

                    // Calculate arrival based on velocity
                    // Distance to Sun ~150 million km
                    float distanceToEarth = 150000000f; // km
                    float travelTimeHours = distanceToEarth / (evt.PlasmaVelocity * 3600f);
                    
                    // Update expected arrival
                    // evt.ExpectedArrival = evt.LaunchTime.AddHours(travelTimeHours);
                    
                    // Impact severity calculation
                    // Southward Bz is critical for geomagnetic coupling
                    float bzFactor = math.max(0f, -evt.MagneticFieldBz) * 0.1f; // Negative Bz = good for coupling
                    float velocityFactor = evt.PlasmaVelocity / 1000f;
                    float densityFactor = math.log(evt.PlasmaDensity + 1) * 0.1f;
                    
                    evt.ImpactSeverity = math.min(1f, (bzFactor + velocityFactor * 0.3f + densityFactor) * 0.5f);
                    
                    // Carrington-level event: V > 2000 km/s, Bz < -50 nT
                    if (evt.PlasmaVelocity > 2000f && evt.MagneticFieldBz < -50f)
                    {
                        evt.ImpactSeverity = 1f; // Maximum
                    }
                }).WithoutBurst().Run();

            // Update Ground Induced Currents per Grid Region
            Entities
                .WithAll<GroundInducedCurrentElement>()
                .ForEach((ref GroundInducedCurrentElement gic) =>
                {
                    // GIC magnitude depends on dB/dt (rate of magnetic change)
                    // Simplified: proportional to Kp index and latitude
                    float kpDriver = math.max(0f, 5f - 5f) * 0.1f; // Mock Kp from global
                    gic.GICMagnitude = kpDriver * gic.LatitudeFactor * gic.GeologyFactor;
                    
                    // Transformer heating accumulates
                    gic.TransformerHeatingRate = gic.GICMagnitude * gic.GICMagnitude * 0.01f; // I²R heating
                    
                    // Damage threshold ~150°C hotspot
                    if (gic.TransformerHeatingRate > 10f) // °C/hour
                    {
                        gic.IsAtRiskOfDamage = true;
                    }
                    else
                    {
                        gic.IsAtRiskOfDamage = false;
                    }
                    
                    // Mitigation reduces GIC
                    if (gic.MitigationActive > 0.5f)
                    {
                        gic.GICMagnitude *= (1f - gic.MitigationActive * 0.8f);
                    }
                }).WithoutBurst().Run();
        }
    }

    /// <summary>
    /// System to simulate satellite anomalies and orbital decay during space weather events
    /// </summary>
    public class SatelliteImpactSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Process satellite vulnerabilities during geomagnetic storms
            
            Entities
                .WithAll<SolarWeatherComponent>()
                .ForEach((ref SolarWeatherComponent solar) =>
                {
                    // Atmospheric drag increases during storms (thermosphere expands)
                    float dragIncrease = math.max(0f, solar.KpIndex - 4f) * 0.05f;
                    
                    // LEO satellites experience increased drag, orbital decay
                    // GEO satellites face charging risks from electron flux
                    
                    if (solar.ElectronFluxMeV > 1e9) // >1 MeV electrons
                    {
                        // Deep dielectric charging risk for GEO sats
                        solar.SatellitesAtRisk += (int)(solar.SatellitesAtRisk * 0.01f);
                    }
                    
                    // Single Event Upsets (SEUs) from proton flux
                    float seuRate = solar.GOESProtonFlux * 0.0001f;
                    // Memory bit flips, computer reboots
                }).WithoutBurst().Run();
        }
    }
}
