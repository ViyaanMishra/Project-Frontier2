using UnityEngine;
using Frontier.MeshGen.Audio;

namespace Frontier.MeshGen.Audio
{
    /// <summary>
    /// Procedural music stem generation for dynamic soundtrack layers
    /// </summary>
    public static class MusicStems : AudioSynthBase
    {
        public static AudioClip GenerateAmbientPad(float duration = 10.0f)
        {
            float[] samples = GenerateSilence(duration);
            
            // Simple chord progression: Am - F - C - G
            float[][] chords = new float[][]
            {
                new float[] { 220f, 261.6f, 329.6f }, // Am
                new float[] { 174.6f, 220f, 261.6f }, // F
                new float[] { 261.6f, 329.6f, 392f },  // C
                new float[] { 196f, 246.9f, 329.6f }   // G
            };
            
            float chordDuration = duration / chords.Length;
            
            for (int c = 0; c < chords.Length; c++)
            {
                for (int i = 0; i < chordDuration * 44100 && c * (int)(chordDuration * 44100) + i < samples.Length; i++)
                {
                    int idx = c * (int)(chordDuration * 44100) + i;
                    float t = (float)i / 44100f;
                    float env = Mathf.Sin(t * Mathf.PI / chordDuration);
                    
                    foreach (float freq in chords[c])
                    {
                        samples[idx] += 0.15f * Mathf.Sin(2f * Mathf.PI * freq * t) * env;
                    }
                }
            }
            
            return CreateClip(samples, "music_ambient_pad");
        }

        public static AudioClip GenerateDroneLow(float duration = 8.0f)
        {
            float[] samples = GenerateSilence(duration);
            
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / 44100f;
                
                // Deep drone with slow modulation
                float drone = Mathf.Sin(2f * Mathf.PI * 55f * t);
                drone += Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.5f;
                
                // Slow amplitude modulation
                float mod = 0.7f + 0.3f * Mathf.Sin(t * 0.5f);
                
                samples[i] = drone * mod * 0.4f;
            }
            
            return CreateClip(samples, "music_drone_low");
        }

        public static AudioClip GenerateRhythmicPulse(float duration = 4.0f, float bpm = 100f)
        {
            float[] samples = GenerateSilence(duration);
            float beatDuration = 60f / bpm;
            int beatSamples = Mathf.FloorToInt(beatDuration * 44100);
            
            for (int b = 0; b < duration / beatDuration; b++)
            {
                for (int i = 0; i < beatSamples && b * beatSamples + i < samples.Length; i++)
                {
                    int idx = b * beatSamples + i;
                    float t = (float)i / 44100f;
                    float env = Mathf.Exp(-t * 10f);
                    
                    // Soft kick-like sound
                    samples[idx] = 0.5f * Mathf.Sin(2f * Mathf.PI * 80f * t) * env;
                }
            }
            
            return CreateClip(samples, $"music_pulse_{bpm}bpm");
        }

        public static AudioClip GenerateMelodicArpeggio(float duration = 6.0f)
        {
            float[] samples = GenerateSilence(duration);
            
            // Simple arpeggio pattern
            float[] notes = { 329.6f, 392f, 493.9f, 659.3f, 493.9f, 392f }; // E4, G4, B4, E5, B4, G4
            float noteDuration = duration / notes.Length;
            int noteSamples = Mathf.FloorToInt(noteDuration * 44100);
            
            for (int n = 0; n < notes.Length; n++)
            {
                for (int i = 0; i < noteSamples && n * noteSamples + i < samples.Length; i++)
                {
                    int idx = n * noteSamples + i;
                    float t = (float)i / 44100f;
                    float env = Mathf.Sin(t * Mathf.PI / noteDuration);
                    
                    samples[idx] = 0.3f * Mathf.Sin(2f * Mathf.PI * notes[n] * t) * env;
                }
            }
            
            return CreateClip(samples, "music_arpeggio");
        }

        public static AudioClip GenerateTensionLayer(float duration = 5.0f)
        {
            float[] samples = GenerateSilence(duration);
            System.Random rng = new System.Random();
            
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / 44100f;
                
                // Dissonant intervals
                float tone1 = Mathf.Sin(2f * Mathf.PI * 200f * t);
                float tone2 = Mathf.Sin(2f * Mathf.PI * 213f * t) * 0.5f; // Tritone-ish
                
                // Random high-pitched accents
                if (rng.NextDouble() < 0.05)
                {
                    tone1 += Mathf.Sin(2f * Mathf.PI * 1500f * t) * 0.3f;
                }
                
                samples[i] = (tone1 + tone2) * 0.3f;
            }
            
            return CreateClip(samples, "music_tension");
        }

        public override AudioClip Generate(string name, params object[] parameters)
        {
            if (parameters.Length > 0 && parameters[0] is string type)
            {
                return type switch
                {
                    "pad" => GenerateAmbientPad(),
                    "drone" => GenerateDroneLow(),
                    "pulse" => GenerateRhythmicPulse(parameters.Length > 1 ? (float)parameters[1] : 100f),
                    "arpeggio" => GenerateMelodicArpeggio(),
                    "tension" => GenerateTensionLayer(),
                    _ => GenerateAmbientPad()
                };
            }
            return GenerateAmbientPad();
        }
    }
}
