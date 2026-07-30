using UnityEngine;
using Frontier.MeshGen.Audio;

namespace Frontier.MeshGen.Audio
{
    /// <summary>
    /// Procedural footstep sound generation per surface type
    /// </summary>
    public static class FootstepSounds : AudioSynthBase
    {
        public static AudioClip GenerateForSurface(string surfaceType, bool isRun = false)
        {
            float duration = isRun ? 0.15f : 0.2f;
            float[] samples;
            
            switch (surfaceType.ToLower())
            {
                case "metal":
                    samples = GenerateMetalStep(duration);
                    break;
                case "wood":
                    samples = GenerateWoodStep(duration);
                    break;
                case "gravel":
                    samples = GenerateGravelStep(duration);
                    break;
                case "snow":
                    samples = GenerateSnowStep(duration);
                    break;
                case "water":
                    samples = GenerateWaterStep(duration);
                    break;
                default: // dirt/grass
                    samples = GenerateDirtStep(duration);
                    break;
            }
            
            return CreateClip(samples, $"footstep_{surfaceType}");
        }

        private static float[] GenerateMetalStep(float duration)
        {
            float[] samples = GenerateSilence(duration);
            int sampleRate = 44100;
            int numSamples = Mathf.FloorToInt(duration * sampleRate);
            
            // Sharp metallic click with ring
            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 30f);
                samples[i] = 0.5f * Mathf.Sin(2f * Mathf.PI * 800f * t) * env;
                samples[i] += 0.3f * Mathf.Sin(2f * Mathf.PI * 1200f * t) * env * 0.5f;
            }
            
            return samples;
        }

        private static float[] GenerateWoodStep(float duration)
        {
            float[] samples = GenerateSilence(duration);
            int numSamples = samples.Length;
            
            // Dull thud with creak
            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / 44100f;
                float env = Mathf.Exp(-t * 15f);
                samples[i] = 0.4f * Mathf.Sin(2f * Mathf.PI * 200f * t) * env;
            }
            
            return samples;
        }

        private static float[] GenerateGravelStep(float duration)
        {
            // Crunchy noise
            float[] noise = GenerateNoise(duration, 0.4f);
            
            for (int i = 0; i < noise.Length; i++)
            {
                float t = (float)i / noise.Length;
                float env = Mathf.Sin(t * Mathf.PI);
                noise[i] *= env;
            }
            
            return noise;
        }

        private static float[] GenerateSnowStep(float duration)
        {
            // Soft crunch
            float[] noise = GenerateNoise(duration, 0.2f);
            
            for (int i = 0; i < noise.Length; i++)
            {
                float t = (float)i / noise.Length;
                float env = Mathf.Sin(t * Mathf.PI) * 0.8f + 0.2f;
                noise[i] *= env;
            }
            
            return noise;
        }

        private static float[] GenerateWaterStep(float duration)
        {
            float[] samples = GenerateSilence(duration);
            int numSamples = samples.Length;
            
            // Splash with low frequency
            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / 44100f;
                float env = Mathf.Sin(t * Mathf.PI * 2f) * Mathf.Exp(-t * 5f);
                samples[i] = 0.3f * GenerateNoise(0.01f, 0.5f)[i % 441] * env;
            }
            
            return samples;
        }

        private static float[] GenerateDirtStep(float duration)
        {
            // Soft thud
            float[] samples = GenerateSilence(duration);
            int numSamples = samples.Length;
            
            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / 44100f;
                float env = Mathf.Exp(-t * 20f);
                samples[i] = 0.3f * Mathf.Sin(2f * Mathf.PI * 150f * t) * env;
            }
            
            return samples;
        }

        public override AudioClip Generate(string name, params object[] parameters)
        {
            string surface = parameters.Length > 0 ? parameters[0].ToString() : "dirt";
            bool run = parameters.Length > 1 && (bool)parameters[1];
            return GenerateForSurface(surface, run);
        }
    }
}
