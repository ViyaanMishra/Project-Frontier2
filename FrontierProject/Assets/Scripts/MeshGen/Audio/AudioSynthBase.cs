using UnityEngine;

namespace Frontier.MeshGen.Audio
{
    /// <summary>
    /// Base class for procedural audio synthesis using AudioClip sample generation
    /// </summary>
    public abstract class AudioSynthBase
    {
        protected float[] GenerateSilence(float duration, int sampleRate = 44100)
        {
            int samples = Mathf.FloorToInt(duration * sampleRate);
            return new float[samples];
        }

        protected float[] GenerateTone(float frequency, float duration, float amplitude = 0.5f, int sampleRate = 44100)
        {
            int samples = Mathf.FloorToInt(duration * sampleRate);
            float[] data = new float[samples];
            
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                data[i] = amplitude * Mathf.Sin(2f * Mathf.PI * frequency * t);
            }
            
            return data;
        }

        protected float[] GenerateNoise(float duration, float amplitude = 0.5f, int sampleRate = 44100)
        {
            int samples = Mathf.FloorToInt(duration * sampleRate);
            float[] data = new float[samples];
            
            System.Random rng = new System.Random();
            for (int i = 0; i < samples; i++)
            {
                data[i] = amplitude * ((float)rng.NextDouble() * 2f - 1f);
            }
            
            return data;
        }

        protected float[] ApplyEnvelope(float[] samples, float attack, float decay, float sustain, float release, int sampleRate = 44100)
        {
            float[] result = new float[samples.Length];
            int attackSamples = Mathf.FloorToInt(attack * sampleRate);
            int decaySamples = Mathf.FloorToInt(decay * sampleRate);
            int releaseSamples = Mathf.FloorToInt(release * sampleRate);
            int sustainStart = attackSamples + decaySamples;
            int sustainEnd = samples.Length - releaseSamples;
            
            for (int i = 0; i < samples.Length; i++)
            {
                float env;
                if (i < attackSamples)
                    env = (float)i / attackSamples;
                else if (i < sustainStart)
                    env = 1f - (1f - sustain) * ((float)(i - attackSamples) / decaySamples);
                else if (i < sustainEnd)
                    env = sustain;
                else
                    env = sustain * (1f - (float)(i - sustainEnd) / releaseSamples);
                
                result[i] = samples[i] * env;
            }
            
            return result;
        }

        protected AudioClip CreateClip(float[] samples, string name, int sampleRate = 44100)
        {
            AudioClip clip = AudioClip.Create(name, samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public abstract AudioClip Generate(string name, params object[] parameters);
    }
}
