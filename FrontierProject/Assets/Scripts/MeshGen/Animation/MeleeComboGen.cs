using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

namespace Frontier.MeshGen.Animation
{
    /// <summary>
    /// Advanced melee combo system with procedural hit reactions, combo chaining, and contextual animations
    /// </summary>
    public class MeleeComboGen : ComponentSystem
    {
        [System.Serializable]
        public struct AttackMove
        {
            public string name;
            public float duration;
            public float windupTime;
            public float activeTime;
            public float recoveryTime;
            public float damageMultiplier;
            public float staggerForce;
            public AnimationCurve motionArc;
            public Vector3 hitBoxOffset;
            public Vector3 hitBoxSize;
            public int comboBranch;
            public bool canChain;
            public bool isHeavy;
            public bool isLight;
            public bool isThrust;
            public bool isSlash;
            public bool isOverhead;
        }
        
        [System.Serializable]
        public struct ComboChain
        {
            public string comboName;
            public List<int> moveIndices;
            public float totalDuration;
            public float optimalTimingWindow;
            public float damageBonus;
            public float speedBonus;
            public bool requiresWeaponType;
            public string weaponType;
        }
        
        public struct ActiveComboInstance
        {
            public Entity attackerEntity;
            public Entity targetEntity;
            public int currentMoveIndex;
            public int comboChainIndex;
            public float currentTime;
            public float comboTimer;
            public bool isActive;
            public bool isLocked;
            public int hitCount;
            public float damageAccumulator;
            public float staggerAccumulator;
            public ComboChain activeChain;
            public WeaponType weaponType;
        }
        
        public enum WeaponType { Sword, Axe, Mace, Spear, Dagger, Hammer, DualWield, Polearm }
        public enum HitReactionType { Light, Medium, Heavy, Knockback, Stun, Launch, Parry, Dodge }
        
        private NativeList<ActiveComboInstance> _activeCombos;
        private Dictionary<WeaponType, List<AttackMove>> _weaponMoves;
        private List<ComboChain> _comboChains;
        
        protected override void OnCreate()
        {
            _activeCombos = new NativeList<ActiveComboInstance>(Allocator.Persistent);
            _weaponMoves = new Dictionary<WeaponType, List<AttackMove>>();
            _comboChains = new List<ComboChain>();
            
            InitializeWeaponMoves();
            InitializeComboChains();
        }
        
        protected override void OnDestroy()
        {
            _activeCombos.Dispose();
        }
        
        private void InitializeWeaponMoves()
        {
            // Sword moves
            _weaponMoves[WeaponType.Sword] = new List<AttackMove>
            {
                CreateAttackMove("Slash_Right", 0.6f, 0.15f, 0.25f, 0.2f, 1.0f, 15f, true, false, true, false, false),
                CreateAttackMove("Slash_Left", 0.6f, 0.15f, 0.25f, 0.2f, 1.0f, 15f, true, false, true, false, false),
                CreateAttackMove("Thrust", 0.5f, 0.1f, 0.2f, 0.2f, 1.2f, 20f, false, false, false, true, false),
                CreateAttackMove("Overhead", 0.8f, 0.25f, 0.3f, 0.25f, 1.5f, 30f, false, true, true, false, true),
                CreateAttackMove("Riposte", 0.4f, 0.05f, 0.15f, 0.2f, 1.3f, 10f, false, false, false, true, false),
                CreateAttackMove("Spin", 1.0f, 0.3f, 0.4f, 0.3f, 0.8f, 25f, true, false, true, false, false)
            };
            
            // Axe moves
            _weaponMoves[WeaponType.Axe] = new List<AttackMove>
            {
                CreateAttackMove("Chop_Down", 0.9f, 0.3f, 0.35f, 0.25f, 1.8f, 40f, false, true, true, false, true),
                CreateAttackMove("Chop_Side", 0.8f, 0.25f, 0.3f, 0.25f, 1.6f, 35f, true, true, true, false, false),
                CreateAttackMove("Hook", 0.7f, 0.2f, 0.25f, 0.25f, 1.1f, 20f, true, false, false, true, false),
                CreateAttackMove("Cleave", 1.2f, 0.4f, 0.5f, 0.3f, 2.0f, 50f, false, true, true, false, false)
            };
            
            // Spear moves
            _weaponMoves[WeaponType.Spear] = new List<AttackMove>
            {
                CreateAttackMove("Jab", 0.4f, 0.08f, 0.15f, 0.17f, 1.0f, 15f, true, false, false, true, false),
                CreateAttackMove("Thrust_Combo", 0.5f, 0.1f, 0.2f, 0.2f, 1.1f, 18f, true, false, false, true, false),
                CreateAttackMove("Sweep", 0.7f, 0.2f, 0.25f, 0.25f, 0.9f, 25f, true, false, true, false, false),
                CreateAttackMove("Vault_Strike", 0.9f, 0.3f, 0.35f, 0.25f, 1.4f, 30f, false, true, true, false, true)
            };
            
            // Additional weapon types would be initialized here
        }
        
        private AttackMove CreateAttackMove(string name, float duration, float windup, float active, 
                                           float recovery, float dmgMult, float stagger, bool canChain,
                                           bool isHeavy, bool isSlash, bool isThrust, bool isOverhead)
        {
            return new AttackMove
            {
                name = name,
                duration = duration,
                windupTime = windup,
                activeTime = active,
                recoveryTime = recovery,
                damageMultiplier = dmgMult,
                staggerForce = stagger,
                motionArc = CreateDefaultMotionArc(),
                hitBoxOffset = new Vector3(0f, 1f, 1.5f),
                hitBoxSize = new Vector3(0.5f, 0.5f, 1f),
                comboBranch = 0,
                canChain = canChain,
                isHeavy = isHeavy,
                isLight = !isHeavy,
                isThrust = isThrust,
                isSlash = isSlash,
                isOverhead = isOverhead
            };
        }
        
        private AnimationCurve CreateDefaultMotionArc()
        {
            return new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.3f, 0.2f),
                new Keyframe(0.5f, 1f),
                new Keyframe(0.7f, 0.8f),
                new Keyframe(1f, 0f)
            );
        }
        
        private void InitializeComboChains()
        {
            _comboChains = new List<ComboChain>
            {
                new ComboChain
                {
                    comboName = "Basic_Trilogy",
                    moveIndices = new List<int> { 0, 1, 2 },
                    totalDuration = 1.7f,
                    optimalTimingWindow = 0.15f,
                    damageBonus = 1.2f,
                    speedBonus = 1.1f,
                    requiresWeaponType = true,
                    weaponType = "Sword"
                },
                new ComboChain
                {
                    comboName = "Heavy_Finisher",
                    moveIndices = new List<int> { 0, 3 },
                    totalDuration = 1.4f,
                    optimalTimingWindow = 0.2f,
                    damageBonus = 1.5f,
                    speedBonus = 0.9f,
                    requiresWeaponType = true,
                    weaponType = "Sword"
                },
                new ComboChain
                {
                    comboName = "Axe_Cleave_Combo",
                    moveIndices = new List<int> { 0, 1, 3 },
                    totalDuration = 2.9f,
                    optimalTimingWindow = 0.25f,
                    damageBonus = 1.8f,
                    speedBonus = 0.85f,
                    requiresWeaponType = true,
                    weaponType = "Axe"
                }
            };
        }
        
        public int StartCombo(Entity attacker, Entity target, WeaponType weaponType, int starterMove = 0)
        {
            if (!_weaponMoves.ContainsKey(weaponType)) return -1;
            
            var moves = _weaponMoves[weaponType];
            if (starterMove >= moves.Count) return -1;
            
            var combo = FindMatchingCombo(weaponType, starterMove);
            
            var instance = new ActiveComboInstance
            {
                attackerEntity = attacker,
                targetEntity = target,
                currentMoveIndex = 0,
                comboChainIndex = combo >= 0 ? combo : -1,
                currentTime = 0f,
                comboTimer = 0f,
                isActive = true,
                isLocked = false,
                hitCount = 0,
                damageAccumulator = 0f,
                staggerAccumulator = 0f,
                activeChain = combo >= 0 ? _comboChains[combo] : new ComboChain(),
                weaponType = weaponType
            };
            
            _activeCombos.Add(instance);
            return _activeCombos.Length - 1;
        }
        
        private int FindMatchingCombo(WeaponType weaponType, int starterMove)
        {
            for (int i = 0; i < _comboChains.Count; i++)
            {
                var chain = _comboChains[i];
                if (chain.moveIndices.Count > 0 && chain.moveIndices[0] == starterMove)
                {
                    return i;
                }
            }
            return -1;
        }
        
        public void UpdateCombo(int comboIndex, float deltaTime)
        {
            if (comboIndex < 0 || comboIndex >= _activeCombos.Length) return;
            
            var combo = _activeCombos[comboIndex];
            if (!combo.isActive) return;
            
            var moves = _weaponMoves[combo.weaponType];
            var currentMove = moves[combo.currentMoveIndex];
            
            combo.currentTime += deltaTime;
            combo.comboTimer += deltaTime;
            
            // Check for move transition
            if (combo.currentTime >= currentMove.duration)
            {
                combo.currentTime = 0f;
                combo.currentMoveIndex++;
                
                // End combo if no more moves
                if (combo.currentMoveIndex >= moves.Count || 
                    (combo.comboChainIndex >= 0 && 
                     combo.currentMoveIndex >= combo.activeChain.moveIndices.Count))
                {
                    combo.isActive = false;
                }
            }
            
            // Check for input to chain next move
            if (currentMove.canChain && combo.currentTime >= currentMove.windupTime)
            {
                // Would check player input here for combo continuation
            }
            
            _activeCombos[comboIndex] = combo;
        }
        
        public bool CheckHitFrame(int comboIndex)
        {
            if (comboIndex < 0 || comboIndex >= _activeCombos.Length) return false;
            
            var combo = _activeCombos[comboIndex];
            var moves = _weaponMoves[combo.weaponType];
            var currentMove = moves[combo.currentMoveIndex];
            
            float normalizedTime = combo.currentTime / currentMove.duration;
            return normalizedTime >= currentMove.windupTime / currentMove.duration &&
                   normalizedTime <= (currentMove.windupTime + currentMove.activeTime) / currentMove.duration;
        }
        
        public HitReactionType CalculateHitReaction(float damage, float defenderStaggerThreshold, 
                                                    bool isBlocking, bool isParrying)
        {
            if (isParrying) return HitReactionType.Parry;
            if (isBlocking) return HitReactionType.Light;
            
            float staggerRatio = damage / defenderStaggerThreshold;
            
            if (staggerRatio >= 2.0f) return HitReactionType.Launch;
            if (staggerRatio >= 1.5f) return HitReactionType.Knockback;
            if (staggerRatio >= 1.0f) return HitReactionType.Stun;
            if (staggerRatio >= 0.7f) return HitReactionType.Heavy;
            if (staggerRatio >= 0.4f) return HitReactionType.Medium;
            return HitReactionType.Light;
        }
        
        public Vector3 GetHitBoxPosition(int comboIndex)
        {
            if (comboIndex < 0 || comboIndex >= _activeCombos.Length) return Vector3.zero;
            
            var combo = _activeCombos[comboIndex];
            var moves = _weaponMoves[combo.weaponType];
            var currentMove = moves[combo.currentMoveIndex];
            
            float normalizedTime = combo.currentTime / currentMove.duration;
            float arcValue = currentMove.motionArc.Evaluate(normalizedTime);
            
            return currentMove.hitBoxOffset + Vector3.forward * arcValue * 0.5f;
        }
        
        public void CancelCombo(int comboIndex, bool forceCancel = false)
        {
            if (comboIndex < 0 || comboIndex >= _activeCombos.Length) return;
            
            var combo = _activeCombos[comboIndex];
            var moves = _weaponMoves[combo.weaponType];
            var currentMove = moves[combo.currentMoveIndex];
            
            // Can only cancel during recovery or with force
            float normalizedTime = combo.currentTime / currentMove.duration;
            if (normalizedTime >= (currentMove.windupTime + currentMove.activeTime) / currentMove.duration || forceCancel)
            {
                combo.isActive = false;
                _activeCombos[comboIndex] = combo;
            }
        }
        
        public float GetComboDamage(int comboIndex)
        {
            if (comboIndex < 0 || comboIndex >= _activeCombos.Length) return 0f;
            
            var combo = _activeCombos[comboIndex];
            var moves = _weaponMoves[combo.weaponType];
            var currentMove = moves[combo.currentMoveIndex];
            
            float bonus = combo.comboChainIndex >= 0 ? combo.activeChain.damageBonus : 1f;
            return currentMove.damageMultiplier * bonus;
        }
    }
}
