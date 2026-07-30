using UnityEngine;
using Frontier.MeshGen.Audio;

namespace Frontier.MeshGen.Audio
{
    /// <summary>
    /// Procedural weapon sound generation (gunshots, reloads, impacts)
    /// </summary>
    public static class WeaponSounds : AudioSynthBase
    {
        public static AudioClip GenerateGunshot(string weaponType, int seed = 12345)
        {
            System.Random rng = new System.Random(seed);
            
            // Base noise burst for explosion
            float[] noise = GenerateNoise(0.3f, 0.8f);
            
            // Add tonal component for caliber feel
            float freq = weaponType switch
            {
                "pistol" => 150f,
                "rifle" => 120f,
                "shotgun" => 80f,
                "heavy" => 60f,
                _ => 100f
            };
            
            float[] tone = GenerateTone(freq, 0.15f, 0.5f);
            
            // Mix
            float[] mixed = new float[noise.Length];
            for (int i = 0; i < Mathf.Min(noise.Length, tone.Length); i++)
            {
                mixed[i] = noise[i] * 0.7f + (i < tone.Length ? tone[i] * 0.3f : 0);
            }
            
            // Apply sharp envelope
            mixed = ApplyEnvelope(mixed, 0.01f, 0.05f, 0.3f, 0.1f);
            
            return CreateClip(mixed, $" gunshot_{weaponType}");
        }

        public static AudioClip GenerateReloadClick(int seed = 54321)
        {
            System.Random rng = new System.Random(seed);
            
            // Short high-frequency click
            float[] click = GenerateTone(2000f, 0.05f, 0.6f);
            click = ApplyEnvelope(click, 0.005f, 0.01f, 0.5f, 0.02f);
            
            return CreateClip(click, "reload_click");
        }

        public static AudioClip GenerateMeleeSwing(int seed = 11111)
        {
            // Whoosh sound using filtered noise
            float[] noise = GenerateNoise(0.4f, 0.4f);
            
            // Frequency modulation simulation
            for (int i = 0; i < noise.Length; i++)
            {
                float t = (float)i / noise.Length;
                noise[i] *= Mathf.Sin(t * Mathf.PI) * 0.8f + 0.2f;
            }
            
            return CreateClip(noise, "melee_swing");
        }

        public static AudioClip GenerateExplosion(float size = 1.0f, int seed = 99999)
        {
            // Long noise burst with low frequency rumble
            float duration = 0.5f * size;
            float[] noise = GenerateNoise(duration, 0.9f);
            float[] rumble = GenerateTone(40f, duration, 0.6f);
            
            float[] mixed = new float[noise.Length];
            for (int i = 0; i < noise.Length; i++)
            {
                mixed[i] = noise[i] * 0.6f + rumble[i] * 0.4f;
            }
            
            mixed = ApplyEnvelope(mixed, 0.02f, 0.1f, 0.5f, 0.3f * size);
            
            return CreateClip(mixed, "explosion");
        }

        public override AudioClip Generate(string name, params object[] parameters)
        {
            if (parameters.Length > 0 && parameters[0] is string type)
            {
                return type switch
                {
                    "gunshot" => GenerateGunshot(parameters.Length > 1 ? parameters[1].ToString() : "rifle"),
                    "reload" => GenerateReloadClick(),
                    "swing" => GenerateMeleeSwing(),
                    "explosion" => GenerateExplosion(parameters.Length > 1 ? (float)parameters[1] : 1.0f),
                    _ => GenerateGunshot("rifle")
                };
            }
            return GenerateGunshot("rifle");
        }
    }
}
