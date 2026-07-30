using UnityEngine;
using Frontier.MeshGen.Audio;

namespace Frontier.MeshGen.Audio
{
    /// <summary>
    /// Procedural machine and industrial sounds (generators, conveyors, tools)
    /// </summary>
    public static class MachineSounds : AudioSynthBase
    {
        public static AudioClip GenerateGeneratorHum()
        {
            float duration = 3.0f;
            float[] samples = GenerateSilence(duration);
            
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / 44100f;
                
                // Base drone with harmonics
                float hum = Mathf.Sin(2f * Mathf.PI * 50f * t);
                hum += Mathf.Sin(2f * Mathf.PI * 100f * t) * 0.5f;
                hum += Mathf.Sin(2f * Mathf.PI * 150f * t) * 0.3f;
                
                // Irregular mechanical noise
                float noise = ((float)new System.Random().NextDouble() * 2f - 1f) * 0.1f;
                
                samples[i] = (hum + noise) * 0.4f;
            }
            
            return CreateClip(samples, "machine_generator");
        }

        public static AudioClip GenerateConveyorLoop()
        {
            float duration = 2.0f;
            float[] samples = GenerateSilence(duration);
            
            System.Random rng = new System.Random(789);
            
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / 44100f;
                
                // Rhythmic mechanical motion
                float rhythm = Mathf.Sin(t * 10f * Mathf.PI) * 0.3f;
                
                // Add clicks and rattles
                if (rng.NextDouble() < 0.1)
                    rhythm += ((float)rng.NextDouble() * 2f - 1f) * 0.2f;
                
                samples[i] = rhythm * 0.5f;
            }
            
            return CreateClip(samples, "machine_conveyor");
        }

        public static AudioClip GenerateHammerHit()
        {
            float duration = 0.15f;
            float[] samples = GenerateSilence(duration);
            
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / 44100f;
                float env = Mathf.Exp(-t * 40f);
                
                // Metallic impact
                samples[i] = 0.6f * Mathf.Sin(2f * Mathf.PI * 1000f * t) * env;
                samples[i] += 0.3f * Mathf.Sin(2f * Mathf.PI * 2000f * t) * env;
            }
            
            return CreateClip(samples, "machine_hammer");
        }

        public static AudioClip GenerateDrillLoop()
        {
            float duration = 1.5f;
            float[] samples = GenerateSilence(duration);
            
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / 44100f;
                
                // High frequency drilling sound
                float drill = Mathf.Sin(2f * Mathf.PI * 800f * t);
                drill += ((float)new System.Random().NextDouble() * 2f - 1f) * 0.3f;
                
                samples[i] = drill * 0.5f;
            }
            
            return CreateClip(samples, "machine_drill");
        }

        public static AudioClip GenerateWeldingSpark()
        {
            float duration = 0.5f;
            float[] samples = GenerateSilence(duration);
            
            System.Random rng = new System.Random(321);
            
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / 44100f;
                
                // Crackling sparks
                if (rng.NextDouble() < 0.3)
                {
                    float spark = Mathf.Sin(2f * Mathf.PI * 5000f * t) * 0.2f;
                    samples[i] = spark;
                }
                
                // Continuous arc hum
                samples[i] += Mathf.Sin(2f * Mathf.PI * 200f * t) * 0.1f;
            }
            
            return CreateClip(samples, "machine_welding");
        }

        public override AudioClip Generate(string name, params object[] parameters)
        {
            if (parameters.Length > 0 && parameters[0] is string type)
            {
                return type switch
                {
                    "generator" => GenerateGeneratorHum(),
                    "conveyor" => GenerateConveyorLoop(),
                    "hammer" => GenerateHammerHit(),
                    "drill" => GenerateDrillLoop(),
                    "welding" => GenerateWeldingSpark(),
                    _ => GenerateGeneratorHum()
                };
            }
            return GenerateGeneratorHum();
        }
    }
}
