using UnityEngine;
using Frontier.MeshGen.Audio;

namespace Frontier.MeshGen.Audio
{
    /// <summary>
    /// Procedural creature vocalizations (barks, howls, screeches, roars)
    /// </summary>
    public static class CreatureCalls : AudioSynthBase
    {
        public static AudioClip GenerateBark(string size = "medium")
        {
            float duration = 0.2f;
            float[] samples = GenerateSilence(duration);
            
            float baseFreq = size switch
            {
                "small" => 800f,
                "medium" => 500f,
                "large" => 200f,
                _ => 500f
            };
            
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / 44100f;
                float env = Mathf.Exp(-t * 20f);
                
                // Rough bark texture
                float noise = ((float)new System.Random().NextDouble() * 2f - 1f) * 0.2f;
                samples[i] = (0.6f * Mathf.Sin(2f * Mathf.PI * baseFreq * t) + noise) * env;
            }
            
            return CreateClip(samples, $"creature_bark_{size}");
        }

        public static AudioClip GenerateHowl(float duration = 1.5f)
        {
            float[] samples = GenerateSilence(duration);
            
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / samples.Length;
                float freq = Mathf.Lerp(300f, 600f, t) + Mathf.Sin(t * 10f) * 50f;
                float env = Mathf.Sin(t * Mathf.PI);
                
                samples[i] = 0.5f * Mathf.Sin(2f * Mathf.PI * freq * t) * env;
            }
            
            return CreateClip(samples, "creature_howl");
        }

        public static AudioClip GenerateScreech(float duration = 0.4f)
        {
            float[] noise = GenerateNoise(duration, 0.7f);
            
            // High-pass and frequency modulation
            for (int i = 0; i < noise.Length; i++)
            {
                float t = (float)i / noise.Length;
                float mod = Mathf.Sin(t * 50f * Mathf.PI) * 0.5f;
                noise[i] = mod * Mathf.Sin(t * Mathf.PI);
            }
            
            return CreateClip(noise, "creature_screech");
        }

        public static AudioClip GenerateRoar(float duration = 1.0f)
        {
            float[] samples = GenerateSilence(duration);
            
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / 44100f;
                float baseFreq = 80f + Mathf.Sin(t * 5f) * 20f;
                float env = Mathf.Sin(t * Mathf.PI / duration);
                
                // Add roughness with sub-harmonics
                float roughness = ((float)new System.Random().NextDouble() * 2f - 1f) * 0.3f;
                samples[i] = (0.5f * Mathf.Sin(2f * Mathf.PI * baseFreq * t) + 
                             0.3f * Mathf.Sin(2f * Mathf.PI * baseFreq * 0.5f * t) + 
                             roughness) * env;
            }
            
            return CreateClip(samples, "creature_roar");
        }

        public static AudioClip GenerateChirp(float duration = 0.1f)
        {
            float[] samples = GenerateSilence(duration);
            
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / 44100f;
                float freq = Mathf.Lerp(2000f, 3000f, t);
                float env = Mathf.Exp(-t * 30f);
                
                samples[i] = 0.3f * Mathf.Sin(2f * Mathf.PI * freq * t) * env;
            }
            
            return CreateClip(samples, "creature_chirp");
        }

        public static AudioClip GenerateBuzz(float duration = 0.3f)
        {
            float[] samples = GenerateSilence(duration);
            int numSamples = samples.Length;
            
            for (int i = 0; i < numSamples; i++)
            {
                float t = (float)i / 44100f;
                float freq = 150f + ((float)new System.Random().NextDouble() * 20f);
                samples[i] = 0.3f * Mathf.Sin(2f * Mathf.PI * freq * t);
            }
            
            return CreateClip(samples, "creature_buzz");
        }

        public override AudioClip Generate(string name, params object[] parameters)
        {
            if (parameters.Length > 0 && parameters[0] is string type)
            {
                return type switch
                {
                    "bark" => GenerateBark(parameters.Length > 1 ? parameters[1].ToString() : "medium"),
                    "howl" => GenerateHowl(),
                    "screech" => GenerateScreech(),
                    "roar" => GenerateRoar(),
                    "chirp" => GenerateChirp(),
                    "buzz" => GenerateBuzz(),
                    _ => GenerateBark("medium")
                };
            }
            return GenerateBark("medium");
        }
    }
}
