using UnityEngine;
using Frontier.MeshGen.Audio;

namespace Frontier.MeshGen.Audio
{
    /// <summary>
    /// Procedural weather sound effects (rain, thunder, wind storms)
    /// </summary>
    public static class WeatherAudio : AudioSynthBase
    {
        public static AudioClip GenerateRainLoop(int intensity = 1)
        {
            float duration = 3.0f;
            float[] samples = GenerateSilence(duration);
            
            System.Random rng = new System.Random(42);
            
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / 44100f;
                
                // Multiple layers of rain drops
                float drop1 = Mathf.Sin(t * 8000f * Mathf.PI) * ((float)rng.NextDouble() * 0.1f);
                float drop2 = Mathf.Sin(t * 6000f * Mathf.PI) * ((float)rng.NextDouble() * 0.08f);
                float hiss = ((float)rng.NextDouble() * 2f - 1f) * 0.05f * intensity;
                
                samples[i] = (drop1 + drop2 + hiss) * 0.3f;
            }
            
            return CreateClip(samples, "weather_rain");
        }

        public static AudioClip GenerateThunder(float distance = 1.0f)
        {
            float duration = 2.0f * distance;
            float[] samples = GenerateSilence(duration);
            
            System.Random rng = new System.Random(123);
            
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / 44100f;
                
                // Low frequency rumble with random variations
                float rumble = Mathf.Sin(2f * Mathf.PI * 40f * t);
                rumble += Mathf.Sin(2f * Mathf.PI * 60f * t) * 0.5f;
                
                // Add noise cracks
                float crack = ((float)rng.NextDouble() * 2f - 1f) * 0.3f;
                
                // Envelope based on distance
                float env = Mathf.Exp(-t * (2f / distance));
                
                samples[i] = (rumble * 0.5f + crack) * env * 0.8f;
            }
            
            return CreateClip(samples, "weather_thunder");
        }

        public static AudioClip GenerateWindStorm(int intensity = 1)
        {
            float duration = 4.0f;
            float[] samples = GenerateSilence(duration);
            
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / 44100f;
                
                // Howling wind with varying pitch
                float howl = Mathf.Sin(2f * Mathf.PI * (200f + 100f * Mathf.Sin(t)) * t);
                
                // Add gusts
                float gust = Mathf.Sin(t * 0.5f) * 0.3f + 0.7f;
                
                // Noise component
                float noise = ((float)new System.Random().NextDouble() * 2f - 1f) * 0.2f;
                
                samples[i] = (howl * 0.4f + noise) * gust * intensity;
            }
            
            return CreateClip(samples, "weather_storm");
        }

        public static AudioClip GenerateHail()
        {
            float duration = 2.0f;
            float[] samples = GenerateSilence(duration);
            
            System.Random rng = new System.Random(456);
            
            // Random impacts
            for (int h = 0; h < 50; h++)
            {
                int startSample = rng.Next(0, samples.Length - 1000);
                
                for (int i = 0; i < 1000 && startSample + i < samples.Length; i++)
                {
                    float t = (float)i / 44100f;
                    float env = Mathf.Exp(-t * 50f);
                    samples[startSample + i] += 0.2f * Mathf.Sin(2f * Mathf.PI * 3000f * t) * env;
                }
            }
            
            return CreateClip(samples, "weather_hail");
        }

        public override AudioClip Generate(string name, params object[] parameters)
        {
            if (parameters.Length > 0 && parameters[0] is string type)
            {
                return type switch
                {
                    "rain" => GenerateRainLoop(parameters.Length > 1 ? (int)parameters[1] : 1),
                    "thunder" => GenerateThunder(parameters.Length > 1 ? (float)parameters[1] : 1.0f),
                    "storm" => GenerateWindStorm(parameters.Length > 1 ? (int)parameters[1] : 1),
                    "hail" => GenerateHail(),
                    _ => GenerateRainLoop(1)
                };
            }
            return GenerateRainLoop(1);
        }
    }
}
