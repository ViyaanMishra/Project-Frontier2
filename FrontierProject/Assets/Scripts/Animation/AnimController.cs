using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Frontier.Animation
{
    /// <summary>
    /// Master Playables API controller for animation system.
    /// Manages animation layers, blending, and state machines.
    /// </summary>
    public class AnimController : MonoBehaviour
    {
        private PlayableGraph _graph;
        private AnimationPlayableOutput _output;
        private MixerPlayable _mixer;
        
        [Header("Layers")]
        public int layerCount = 3; // Base, UpperBody, Additive
        
        [Header("Parameters")]
        public float speed = 1f;
        public float weight = 1f;

        private void OnEnable()
        {
            Initialize();
        }

        private void OnDisable()
        {
            Cleanup();
        }

        public void Initialize()
        {
            _graph = PlayableGraph.Create("AnimController");
            _output = AnimationPlayableOutput.Create(_graph, "Output", GetComponent<Animator>());
            
            _mixer = MixerPlayable.Create(_graph, layerCount);
            _output.SetSourcePlayable(_mixer);
            
            _graph.Play();
        }

        public void Cleanup()
        {
            if (_graph.IsValid())
            {
                _graph.Destroy();
            }
        }

        public void SetLayerWeight(int layerIndex, float weight)
        {
            if (!_graph.IsValid()) return;
            _mixer.SetInputWeight(layerIndex, Mathf.Clamp01(weight));
        }

        public void PlayAnimation(int layerIndex, AnimationClip clip, float fadeDuration = 0.2f)
        {
            if (!_graph.IsValid()) return;
            
            var clipPlayable = AnimationClipPlayable.Create(_graph, clip);
            _mixer.ConnectInput(layerIndex, clipPlayable, 0);
            SetLayerWeight(layerIndex, 1f);
        }

        public void CrossFade(int fromLayer, int toLayer, float duration)
        {
            StartCoroutine(CrossFadeCoroutine(fromLayer, toLayer, duration));
        }

        private System.Collections.IEnumerator CrossFadeCoroutine(int fromLayer, int toLayer, float duration)
        {
            float elapsed = 0f;
            float fromWeight = _mixer.GetInputWeight(fromLayer);
            float toWeight = _mixer.GetInputWeight(toLayer);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                _mixer.SetInputWeight(fromLayer, Mathf.Lerp(fromWeight, 0f, t));
                _mixer.SetInputWeight(toLayer, Mathf.Lerp(toWeight, 1f, t));
                
                yield return null;
            }
        }

        public void SetSpeed(float speed)
        {
            this.speed = speed;
            _graph.SetTimeScale(speed);
        }

        public void SetParameter(string name, float value)
        {
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetFloat(name, value);
            }
        }

        public void TriggerAnimation(string triggerName)
        {
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger(triggerName);
            }
        }
    }
}
