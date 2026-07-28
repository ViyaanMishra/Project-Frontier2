using UnityEngine;

namespace Frontier.Animation
{
    /// <summary>
    /// Continuum anomaly glitch effects: frame-skip phase, mesh distortion.
    /// Visual corruption for entities affected by reality anomalies.
    /// </summary>
    public class ContinuumGlitch : MonoBehaviour
    {
        private Animator _animator;
        private MeshRenderer _renderer;
        private Material _originalMaterial;
        
        [Header("Glitch Parameters")]
        public string glitchIntensityParam = "GlitchIntensity";
        public string isPhasingParam = "IsPhasing";
        
        [Header("Settings")]
        public float baseGlitchRate = 0.1f;
        public float maxGlitchOffset = 0.5f;
        public float phaseDuration = 2f;
        
        [Header("State")]
        public float glitchIntensity = 0f;
        public bool isPhasing;
        public bool isCorrupted;

        [Header("Materials")]
        public Material glitchMaterial;
        public Material corruptedMaterial;

        private float _glitchTimer;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _renderer = GetComponent<MeshRenderer>();
            
            if (_renderer != null)
            {
                _originalMaterial = _renderer.material;
            }
            
            _originalPosition = transform.position;
            _originalRotation = transform.rotation;
        }

        private void Update()
        {
            if (_animator == null) return;

            // Update glitch parameters
            _animator.SetFloat(glitchIntensityParam, glitchIntensity);
            _animator.SetBool(isPhasingParam, isPhasing);

            // Apply visual glitch effects
            if (glitchIntensity > 0f)
            {
                ApplyGlitchEffect();
            }
            else
            {
                ResetTransform();
            }

            // Apply material corruption
            if (isCorrupted && glitchMaterial != null && _renderer != null)
            {
                _renderer.material = glitchMaterial;
            }
            else if (!isCorrupted && _originalMaterial != null && _renderer != null)
            {
                _renderer.material = _originalMaterial;
            }
        }

        private void ApplyGlitchEffect()
        {
            _glitchTimer += Time.deltaTime * baseGlitchRate * glitchIntensity;

            // Random position jitter
            float jitterAmount = glitchIntensity * maxGlitchOffset;
            Vector3 jitter = new Vector3(
                Mathf.PerlinNoise(_glitchTimer, 0) * 2 - 1,
                Mathf.PerlinNoise(0, _glitchTimer) * 2 - 1,
                Mathf.PerlinNoise(_glitchTimer * 0.5f, _glitchTimer * 0.5f) * 2 - 1
            ) * jitterAmount;

            transform.position = _originalPosition + jitter;

            // Random rotation jitter
            float rotJitter = glitchIntensity * 10f;
            transform.rotation = _originalRotation * Quaternion.Euler(
                Mathf.Sin(_glitchTimer * 10f) * rotJitter,
                Mathf.Cos(_glitchTimer * 7f) * rotJitter,
                Mathf.Sin(_glitchTimer * 5f) * rotJitter
            );

            // Random scale flicker
            if (Random.value < glitchIntensity * 0.1f)
            {
                float scaleFlicker = 1f + (Random.value - 0.5f) * glitchIntensity * 0.2f;
                transform.localScale = Vector3.one * scaleFlicker;
            }
            else
            {
                transform.localScale = Vector3.one;
            }

            // Frame skip simulation (teleport effect)
            if (Random.value < glitchIntensity * 0.05f)
            {
                StartCoroutine(FrameSkipCoroutine());
            }
        }

        private System.Collections.IEnumerator FrameSkipCoroutine()
        {
            Vector3 targetPos = transform.position + Random.insideUnitSphere * glitchIntensity;
            float elapsed = 0f;
            
            while (elapsed < 0.1f)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            transform.position = targetPos;
            _originalPosition = targetPos;
        }

        public void SetGlitchIntensity(float intensity)
        {
            glitchIntensity = Mathf.Clamp01(intensity);
            
            if (glitchIntensity > 0.7f && !isPhasing)
            {
                StartPhase();
            }
            else if (glitchIntensity < 0.3f && isPhasing)
            {
                EndPhase();
            }
        }

        public void StartPhase()
        {
            isPhasing = true;
            _animator.SetTrigger("StartPhase");
            
            // Make semi-transparent during phase
            if (_renderer != null && glitchMaterial != null)
            {
                _renderer.material = glitchMaterial;
            }
        }

        public void EndPhase()
        {
            isPhasing = false;
            _animator.SetTrigger("EndPhase");
            ResetTransform();
        }

        public void Corrupt()
        {
            isCorrupted = true;
            glitchIntensity = Mathf.Max(glitchIntensity, 0.5f);
        }

        public void Purify()
        {
            isCorrupted = false;
            SetGlitchIntensity(0f);
        }

        private void ResetTransform()
        {
            transform.position = _originalPosition;
            transform.rotation = _originalRotation;
            transform.localScale = Vector3.one;
        }

        public void OnDrawGizmos()
        {
            if (glitchIntensity > 0f)
            {
                Gizmos.color = new Color(1f, 0f, 1f, glitchIntensity);
                Gizmos.DrawWireSphere(transform.position, 0.5f + glitchIntensity * 0.5f);
            }
        }
    }
}
