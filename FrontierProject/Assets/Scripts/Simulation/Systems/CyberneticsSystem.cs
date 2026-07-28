using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

namespace Frontier.Simulation.Systems
{
    /// <summary>
    /// Cybernetics system for limb replacement, power drain, neural rejection, and frenzy triggers.
    /// </summary>
    public enum CyberneticType
    {
        None = 0,
        Arm = 1,
        Leg = 2,
        Eye = 3,
        Neural = 4,
        Cardiac = 5,
        Dermal = 6
    }

    public struct CyberneticImplant
    {
        public CyberneticType type;
        public string implantId;
        public float quality;           // 0.0 - 1.0
        public float powerDrain;        // Watts
        public float rejectionRisk;     // 0.0 - 1.0 per day
        public bool isActive;
        public float installationTime;  // Game time when installed
        public byte modificationLevel;
        
        // Special effects
        public float strengthBonus;
        public float speedBonus;
        public float perceptionBonus;
        public bool hasFrenzyTrigger;
    }

    public class CyberneticsSystem : IDisposable
    {
        private NativeHashMap<int, NativeList<CyberneticImplant>> _entityImplants;
        private NativeHashMap<string, CyberneticImplant> _implantDefinitions;
        
        public CyberneticsSystem()
        {
            _entityImplants = new NativeHashMap<int, NativeList<CyberneticImplant>>(1000, Allocator.Persistent);
            _implantDefinitions = new NativeHashMap<string, CyberneticImplant>(100, Allocator.Persistent);
            InitializeImplantDefinitions();
        }
        
        private void InitializeImplantDefinitions()
        {
            // Define available implants
            var combatArm = new CyberneticImplant
            {
                type = CyberneticType.Arm,
                implantId = "COMBAT_ARM_MK1",
                quality = 0.8f,
                powerDrain = 5f,
                rejectionRisk = 0.02f,
                isActive = true,
                strengthBonus = 0.3f,
                speedBonus = 0.1f
            };
            _implantDefinitions.TryAdd("COMBAT_ARM_MK1", combatArm);
            
            var speedLeg = new CyberneticImplant
            {
                type = CyberneticType.Leg,
                implantId = "SPEED_LEG_MK1",
                quality = 0.75f,
                powerDrain = 8f,
                rejectionRisk = 0.03f,
                isActive = true,
                strengthBonus = 0.1f,
                speedBonus = 0.4f
            };
            _implantDefinitions.TryAdd("SPEED_LEG_MK1", speedLeg);
            
            var neuralBoost = new CyberneticImplant
            {
                type = CyberneticType.Neural,
                implantId = "NEURAL_BOOST_MK1",
                quality = 0.9f,
                powerDrain = 12f,
                rejectionRisk = 0.05f,
                isActive = true,
                perceptionBonus = 0.3f,
                hasFrenzyTrigger = true
            };
            _implantDefinitions.TryAdd("NEURAL_BOOST_MK1", neuralBoost);
        }
        
        public bool InstallImplant(int entityId, string implantDefId)
        {
            if (!_implantDefinitions.TryGetValue(implantDefId, out var definition))
                return false;
            
            // Check if entity already has implant of this type
            if (_entityImplants.TryGetValue(entityId, out var implants))
            {
                for (int i = 0; i < implants.Length; i++)
                {
                    if (implants[i].type == definition.type)
                        return false; // Already has implant of this type
                }
            }
            else
            {
                implants = new NativeList<CyberneticImplant>(Allocator.Persistent);
                _entityImplants.Add(entityId, implants);
            }
            
            var newImplant = definition;
            newImplant.installationTime = UnityEngine.Time.time;
            implants.Add(newImplant);
            
            return true;
        }
        
        public bool RemoveImplant(int entityId, string implantId)
        {
            if (!_entityImplants.TryGetValue(entityId, out var implants))
                return false;
            
            for (int i = 0; i < implants.Length; i++)
            {
                if (implants[i].implantId == implantId)
                {
                    implants.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
        
        public void SimulateRejection(int entityId, float deltaTime)
        {
            if (!_entityImplants.TryGetValue(entityId, out var implants))
                return;
            
            for (int i = 0; i < implants.Length; i++)
            {
                var implant = implants[i];
                if (!implant.isActive) continue;
                
                // Rejection chance increases over time
                float timeInstalled = UnityEngine.Time.time - implant.installationTime;
                float adjustedRejection = implant.rejectionRisk * (1f + timeInstalled / 86400f); // Increases per day
                
                if (UnityEngine.Random.value < adjustedRejection * deltaTime)
                {
                    // Rejection event
                    implant.isActive = false;
                    implant.quality *= 0.8f;
                    implants[i] = implant;
                    
                    // Trigger rejection event
                    // EventBus.Raise(new ImplantRejectedEvent(entityId, implant));
                }
            }
        }
        
        public float GetTotalPowerDrain(int entityId)
        {
            float total = 0f;
            if (_entityImplants.TryGetValue(entityId, out var implants))
            {
                for (int i = 0; i < implants.Length; i++)
                {
                    if (implants[i].isActive)
                        total += implants[i].powerDrain;
                }
            }
            return total;
        }
        
        public float GetStrengthBonus(int entityId)
        {
            float bonus = 0f;
            if (_entityImplants.TryGetValue(entityId, out var implants))
            {
                for (int i = 0; i < implants.Length; i++)
                {
                    if (implants[i].isActive)
                        bonus += implants[i].strengthBonus;
                }
            }
            return bonus;
        }
        
        public float GetSpeedBonus(int entityId)
        {
            float bonus = 0f;
            if (_entityImplants.TryGetValue(entityId, out var implants))
            {
                for (int i = 0; i < implants.Length; i++)
                {
                    if (implants[i].isActive)
                        bonus += implants[i].speedBonus;
                }
            }
            return bonus;
        }
        
        public bool CheckFrenzyTrigger(int entityId)
        {
            if (!_entityImplants.TryGetValue(entityId, out var implants))
                return false;
            
            for (int i = 0; i < implants.Length; i++)
            {
                if (implants[i].hasFrenzyTrigger && implants[i].isActive)
                {
                    // Chance to trigger frenzy based on stress/health
                    if (UnityEngine.Random.value < 0.01f)
                        return true;
                }
            }
            return false;
        }
        
        public NativeList<CyberneticImplant> GetEntityImplants(int entityId)
        {
            if (_entityImplants.TryGetValue(entityId, out var implants))
                return implants;
            return null;
        }
        
        public void Dispose()
        {
            var keys = _entityImplants.GetKeyArray(Allocator.Temp);
            foreach (var key in keys)
            {
                if (_entityImplants.TryGetValue(key, out var list))
                    list.Dispose();
            }
            _entityImplants.Dispose();
            _implantDefinitions.Dispose();
            keys.Dispose();
        }
    }
}
