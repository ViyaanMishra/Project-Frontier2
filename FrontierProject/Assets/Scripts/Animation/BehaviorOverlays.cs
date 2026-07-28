using UnityEngine;

namespace Frontier.Animation
{
    /// <summary>
    /// Behavior overlays: morale posture, disease shuffling, injury limping.
    /// Additive animation layers for status-based behavior.
    /// </summary>
    public class BehaviorOverlays : MonoBehaviour
    {
        private Animator _animator;

        [Header("Morale Parameters")]
        public string moraleParam = "Morale";
        public float morale = 1f; // 0-1
        
        [Header("Health Parameters")]
        public string isLimpingParam = "IsLimping";
        public string limpSeverityParam = "LimpSeverity";
        public float limpSeverity = 0f;
        
        [Header("Disease Parameters")]
        public string isShufflingParam = "IsShuffling";
        public string shuffleIntensityParam = "ShuffleIntensity";
        public float shuffleIntensity = 0f;
        
        [Header("Combat Stress")]
        public string isTremblingParam = "IsTrembling";
        public float trembleIntensity = 0f;
        
        [Header("Exhaustion")]
        public string exhaustionParam = "Exhaustion";
        public float exhaustion = 0f;

        public enum MoraleState
        {
            High, Normal, Low, Broken
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (_animator == null) return;

            // Update morale state
            UpdateMorale();
            
            // Update overlays
            _animator.SetFloat(moraleParam, morale);
            _animator.SetBool(isLimpingParam, limpSeverity > 0.1f);
            _animator.SetFloat(limpSeverityParam, limpSeverity);
            _animator.SetBool(isShufflingParam, shuffleIntensity > 0.1f);
            _animator.SetFloat(shuffleIntensityParam, shuffleIntensity);
            _animator.SetBool(isTremblingParam, trembleIntensity > 0.1f);
            _animator.SetFloat(exhaustionParam, exhaustion);
        }

        private void UpdateMorale()
        {
            // Morale affects posture and movement confidence
            if (morale < 0.2f)
            {
                // Broken morale - slumped posture, slow movement
                _animator.SetInteger("MoraleState", 3);
            }
            else if (morale < 0.5f)
            {
                // Low morale - cautious posture
                _animator.SetInteger("MoraleState", 2);
            }
            else if (morale < 0.8f)
            {
                // Normal morale
                _animator.SetInteger("MoraleState", 1);
            }
            else
            {
                // High morale - confident posture
                _animator.SetInteger("MoraleState", 0);
            }
        }

        public void SetMorale(float value)
        {
            morale = Mathf.Clamp01(value);
        }

        public void ModifyMorale(float delta)
        {
            morale = Mathf.Clamp01(morale + delta);
        }

        public void SetLimp(float severity)
        {
            limpSeverity = Mathf.Clamp01(severity);
        }

        public void SetShuffle(float intensity)
        {
            shuffleIntensity = Mathf.Clamp01(intensity);
        }

        public void SetTremble(float intensity)
        {
            trembleIntensity = Mathf.Clamp01(intensity);
        }

        public void SetExhaustion(float value)
        {
            exhaustion = Mathf.Clamp01(value);
        }

        public void ApplyInjury(string injuryType)
        {
            switch (injuryType)
            {
                case "Leg":
                    SetLimp(0.8f);
                    break;
                case "Arm":
                    _animator.SetTrigger("ArmInjury");
                    break;
                case "Head":
                    SetTremble(0.5f);
                    SetShuffle(0.3f);
                    break;
            }
        }

        public void ApplyDisease(string diseaseType)
        {
            switch (diseaseType)
            {
                case "Flu":
                    SetShuffle(0.4f);
                    SetExhaustion(0.5f);
                    break;
                case "Plague":
                    SetShuffle(0.8f);
                    SetLimp(0.5f);
                    SetExhaustion(0.8f);
                    break;
                case "Radiation":
                    SetTremble(0.6f);
                    SetShuffle(0.5f);
                    SetExhaustion(0.7f);
                    break;
            }
        }

        public void ClearAllOverlays()
        {
            morale = 1f;
            limpSeverity = 0f;
            shuffleIntensity = 0f;
            trembleIntensity = 0f;
            exhaustion = 0f;
        }

        public MoraleState GetMoraleState()
        {
            if (morale < 0.2f) return MoraleState.Broken;
            if (morale < 0.5f) return MoraleState.Low;
            if (morale < 0.8f) return MoraleState.Normal;
            return MoraleState.High;
        }
    }
}
