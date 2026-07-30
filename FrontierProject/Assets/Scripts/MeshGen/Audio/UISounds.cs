using UnityEngine;
using Frontier.MeshGen.Audio;

namespace Frontier.MeshGen.Audio
{
    /// <summary>
    /// Procedural UI sound effects (clicks, hovers, notifications)
    /// </summary>
    public static class UISounds : AudioSynthBase
    {
        public static AudioClip GenerateClick()
        {
            float[] samples = GenerateSilence(0.1f);
            int numSamples = samples.Length;
            
            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / 44100f;
                float env = Mathf.Exp(-t * 50f);
                samples[i] = 0.4f * Mathf.Sin(2f * Mathf.PI * 1500f * t) * env;
            }
            
            return CreateClip(samples, "ui_click");
        }

        public static AudioClip GenerateHover()
        {
            float[] samples = GenerateSilence(0.08f);
            int numSamples = samples.Length;
            
            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / 44100f;
                float env = Mathf.Exp(-t * 30f);
                samples[i] = 0.2f * Mathf.Sin(2f * Mathf.PI * 2000f * t) * env;
            }
            
            return CreateClip(samples, "ui_hover");
        }

        public static AudioClip GenerateNotification()
        {
            float duration = 0.3f;
            float[] samples = GenerateSilence(duration);
            int numSamples = samples.Length;
            
            // Two-tone chime
            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / 44100f;
                float env = Mathf.Sin(t * Mathf.PI / duration);
                
                samples[i] = 0.3f * Mathf.Sin(2f * Mathf.PI * 800f * t) * env;
                if (i > numSamples / 2)
                    samples[i] += 0.3f * Mathf.Sin(2f * Mathf.PI * 1200f * t) * env;
            }
            
            return CreateClip(samples, "ui_notification");
        }

        public static AudioClip GenerateError()
        {
            float duration = 0.25f;
            float[] samples = GenerateSilence(duration);
            int numSamples = samples.Length;
            
            // Descending tones
            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / 44100f;
                float freq = Mathf.Lerp(600f, 300f, t);
                float env = Mathf.Sin(t * Mathf.PI);
                
                samples[i] = 0.4f * Mathf.Sin(2f * Mathf.PI * freq * t) * env;
            }
            
            return CreateClip(samples, "ui_error");
        }

        public static AudioClip GenerateSuccess()
        {
            float duration = 0.4f;
            float[] samples = GenerateSilence(duration);
            int numSamples = samples.Length;
            
            // Ascending arpeggio
            float[] notes = { 523f, 659f, 784f, 1047f }; // C5, E5, G5, C6
            float noteDuration = duration / notes.Length;
            
            for (int n = 0; n < notes.Length; n++)
            {
                for (int i = 0; i < noteDuration * 44100 && n * (int)(noteDuration * 44100) + i < numSamples; i++)
                {
                    int idx = n * (int)(noteDuration * 44100) + i;
                    float t = (float)i / 44100f;
                    float localEnv = Mathf.Sin(t * Mathf.PI / noteDuration);
                    samples[idx] += 0.3f * Mathf.Sin(2f * Mathf.PI * notes[n] * t) * localEnv;
                }
            }
            
            return CreateClip(samples, "ui_success");
        }

        public override AudioClip Generate(string name, params object[] parameters)
        {
            if (parameters.Length > 0 && parameters[0] is string type)
            {
                return type switch
                {
                    "click" => GenerateClick(),
                    "hover" => GenerateHover(),
                    "notification" => GenerateNotification(),
                    "error" => GenerateError(),
                    "success" => GenerateSuccess(),
                    _ => GenerateClick()
                };
            }
            return GenerateClick();
        }
    }
}
