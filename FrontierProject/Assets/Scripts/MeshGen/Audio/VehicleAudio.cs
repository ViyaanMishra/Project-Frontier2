using UnityEngine;
using Frontier.MeshGen.Audio;

namespace Frontier.MeshGen.Audio
{
    /// <summary>
    /// Procedural vehicle engine and movement sounds
    /// </summary>
    public static class VehicleAudio : AudioSynthBase
    {
        public static AudioClip GenerateEngineIdle(string engineType)
        {
            float duration = 3.0f;
            float baseFreq = engineType switch
            {
                "small" => 80f,
                "medium" => 60f,
                "large" => 40f,
                "electric" => 200f,
                _ => 60f
            };
            
            float[] samples = GenerateSilence(duration);
            
            // Base drone
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / 44100f;
                float rumble = Mathf.Sin(2f * Mathf.PI * baseFreq * t);
                float harmonic = Mathf.Sin(2f * Mathf.PI * baseFreq * 1.5f * t) * 0.5f;
                
                // Add some irregularity
                float variation = 0.1f * Mathf.Sin(t * 3f);
                
                samples[i] = (rumble + harmonic) * (0.3f + variation);
            }
            
            return CreateClip(samples, $"engine_idle_{engineType}");
        }

        public static AudioClip GenerateEngineRev(float duration = 1.0f)
        {
            float[] samples = GenerateSilence(duration);
            
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / samples.Length;
                float freq = Mathf.Lerp(60f, 150f, t);
                float env = Mathf.Sin(t * Mathf.PI);
                
                samples[i] = 0.5f * Mathf.Sin(2f * Mathf.PI * freq * t) * env;
            }
            
            return CreateClip(samples, "engine_rev");
        }

        public static AudioClip GenerateTireScreech(float duration = 0.5f)
        {
            float[] noise = GenerateNoise(duration, 0.6f);
            
            // High-pass filter simulation
            for (int i = 1; i < noise.Length; i++)
            {
                noise[i] = noise[i] - noise[i - 1];
            }
            
            return CreateClip(noise, "tire_screech");
        }

        public static AudioClip GenerateHorn()
        {
            float duration = 0.8f;
            float[] samples = GenerateSilence(duration);
            
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / 44100f;
                float env = Mathf.Sin(t * Mathf.PI / duration);
                
                // Dual tone horn
                samples[i] = 0.4f * Mathf.Sin(2f * Mathf.PI * 400f * t) * env;
                samples[i] += 0.4f * Mathf.Sin(2f * Mathf.PI * 500f * t) * env;
            }
            
            return CreateClip(samples, "vehicle_horn");
        }

        public override AudioClip Generate(string name, params object[] parameters)
        {
            if (parameters.Length > 0 && parameters[0] is string type)
            {
                return type switch
                {
                    "idle" => GenerateEngineIdle(parameters.Length > 1 ? parameters[1].ToString() : "medium"),
                    "rev" => GenerateEngineRev(),
                    "screech" => GenerateTireScreech(),
                    "horn" => GenerateHorn(),
                    _ => GenerateEngineIdle("medium")
                };
            }
            return GenerateEngineIdle("medium");
        }
    }
}
