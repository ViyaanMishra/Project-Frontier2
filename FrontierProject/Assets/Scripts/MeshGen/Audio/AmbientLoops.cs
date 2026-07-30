using UnityEngine;
using Frontier.MeshGen.Audio;

namespace Frontier.MeshGen.Audio
{
    /// <summary>
    /// Procedural ambient sound generation for biomes and environments
    /// </summary>
    public static class AmbientLoops : AudioSynthBase
    {
        public static AudioClip GenerateWindLoop(int intensity = 1, int seed = 22222)
        {
            System.Random rng = new System.Random(seed);
            
            // Pink noise for wind
            float duration = 3.0f;
            float[] noise = GenerateNoise(duration, 0.3f * intensity);
            
            // Low-pass filter simulation (attenuate highs)
            for (int i = 0; i < noise.Length; i++)
            {
                float t = (float)i / noise.Length;
                // Modulate amplitude for gusts
                float gust = 0.5f + 0.5f * Mathf.Sin(t * 2f * Mathf.PI * rng.Next(1, 4));
                noise[i] *= gust;
            }
            
            return CreateClip(noise, "ambient_wind");
        }

        public static AudioClip GenerateInsectChirp(int seed = 33333)
        {
            System.Random rng = new System.Random(seed);
            
            float duration = 2.0f;
            float[] samples = GenerateSilence(duration);
            
            // Random chirps
            int numChirps = rng.Next(5, 12);
            for (int c = 0; c < numChirps; c++)
            {
                int startSample = rng.Next(0, samples.Length - 1000);
                float freq = rng.Next(2000, 5000);
                
                for (int i = 0; i < 500 && startSample + i < samples.Length; i++)
                {
                    float t = (float)i / 44100f;
                    float env = Mathf.Sin(t * Mathf.PI * 2f);
                    samples[startSample + i] += 0.1f * Mathf.Sin(2f * Mathf.PI * freq * t) * env;
                }
            }
            
            return CreateClip(samples, "ambient_insects");
        }

        public static AudioClip GenerateWaterDrip(int seed = 44444)
        {
            System.Random rng = new System.Random(seed);
            
            float duration = 4.0f;
            float[] samples = GenerateSilence(duration);
            
            // Occasional drips
            int numDrips = rng.Next(3, 7);
            for (int d = 0; d < numDrips; d++)
            {
                int startSample = rng.Next(0, samples.Length - 2000);
                
                // High frequency ping with decay
                for (int i = 0; i < 2000 && startSample + i < samples.Length; i++)
                {
                    float t = (float)i / 44100f;
                    float env = Mathf.Exp(-t * 10f);
                    samples[startSample + i] += 0.3f * Mathf.Sin(2f * Mathf.PI * 3000f * t) * env;
                }
            }
            
            return CreateClip(samples, "ambient_drip");
        }

        public static AudioClip GenerateMachineHum(int seed = 55555)
        {
            // Low frequency drone
            float[] tone1 = GenerateTone(50f, 3.0f, 0.4f);
            float[] tone2 = GenerateTone(60f, 3.0f, 0.3f);
            
            float[] mixed = new float[tone1.Length];
            for (int i = 0; i < tone1.Length; i++)
            {
                mixed[i] = tone1[i] + tone2[i] * 0.5f;
            }
            
            return CreateClip(mixed, "ambient_machine");
        }

        public override AudioClip Generate(string name, params object[] parameters)
        {
            if (parameters.Length > 0 && parameters[0] is string type)
            {
                return type switch
                {
                    "wind" => GenerateWindLoop(parameters.Length > 1 ? (int)parameters[1] : 1),
                    "insects" => GenerateInsectChirp(),
                    "drip" => GenerateWaterDrip(),
                    "machine" => GenerateMachineHum(),
                    _ => GenerateWindLoop(1)
                };
            }
            return GenerateWindLoop(1);
        }
    }
}
