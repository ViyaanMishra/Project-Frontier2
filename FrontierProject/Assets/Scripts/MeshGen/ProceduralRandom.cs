using UnityEngine;

namespace Frontier.MeshGen
{
    /// <summary>
    /// Seeded random number generator for deterministic procedural generation
    /// </summary>
    public class ProceduralRandom
    {
        private System.Random rng;
        private int seed;
        
        public ProceduralRandom(int seed)
        {
            this.seed = seed;
            this.rng = new System.Random(seed);
        }
        
        /// <summary>
        /// Random float between 0 and 1
        /// </summary>
        public float Value()
        {
            return (float)rng.NextDouble();
        }
        
        /// <summary>
        /// Random float in range [min, max]
        /// </summary>
        public float Range(float min, float max)
        {
            return Mathf.Lerp(min, max, Value());
        }
        
        /// <summary>
        /// Random integer in range [min, max)
        /// </summary>
        public int Range(int min, int max)
        {
            return rng.Next(min, max);
        }
        
        /// <summary>
        /// Random Vector3 with optional magnitude range
        /// </summary>
        public Vector3 Vector3(float minMag = 0f, float maxMag = 1f)
        {
            return new Vector3(
                Range(-1, 1),
                Range(-1, 1),
                Range(-1, 1)
            ).normalized * Range(minMag, maxMag);
        }
        
        /// <summary>
        /// Random color with optional hue range
        /// </summary>
        public Color Color(float hueMin = 0f, float hueMax = 1f, float satMin = 0.3f, float satMax = 0.8f, float valMin = 0.5f, float valMax = 1f)
        {
            float h = Range(hueMin, hueMax);
            float s = Range(satMin, satMax);
            float v = Range(valMin, valMax);
            return Color.HSVToRGB(h, s, v);
        }
        
        /// <summary>
        /// Pick random element from array
        /// </summary>
        public T Pick<T>(T[] array)
        {
            if (array == null || array.Length == 0) return default(T);
            return array[rng.Next(array.Length)];
        }
        
        /// <summary>
        /// Random rotation
        /// </summary>
        public Quaternion Rotation()
        {
            return Quaternion.Euler(
                Range(0, 360),
                Range(0, 360),
                Range(0, 360)
            );
        }
        
        /// <summary>
        /// Random point on unit sphere
        /// </summary>
        public Vector3 OnUnitSphere()
        {
            float theta = Range(0, Mathf.PI * 2);
            float phi = Mathf.Acos(2 * Value() - 1);
            float x = Mathf.Sin(phi) * Mathf.Cos(theta);
            float y = Mathf.Sin(phi) * Mathf.Sin(theta);
            float z = Mathf.Cos(phi);
            return new Vector3(x, y, z);
        }
        
        /// <summary>
        /// Gaussian distributed random value
        /// </summary>
        public float Gaussian(float mean = 0, float stdDev = 1)
        {
            double u1 = rng.NextDouble();
            double u2 = rng.NextDouble();
            double randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log((float)u1)) * Mathf.Sin(2.0f * Mathf.PI * (float)u2);
            return mean + stdDev * (float)randStdNormal;
        }
        
        /// <summary>
        /// Chance check (returns true if value < chance)
        /// </summary>
        public bool Chance(float chance)
        {
            return Value() < chance;
        }
        
        /// <summary>
        /// Reset with new seed
        /// </summary>
        public void Reseed(int newSeed)
        {
            this.seed = newSeed;
            this.rng = new System.Random(newSeed);
        }
        
        public int Seed => seed;
    }
}
