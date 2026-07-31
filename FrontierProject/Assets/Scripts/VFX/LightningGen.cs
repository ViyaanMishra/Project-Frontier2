using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Frontier.VFX
{
    /// <summary>
    /// Advanced lightning generator with branching bolts, electrical arcs, and EMP effects
    /// </summary>
    public class LightningGen : ComponentSystem
    {
        [System.Serializable]
        public struct LightningBolt
        {
            public Vector3 startPosition;
            public Vector3 endPosition;
            public NativeList<Vector3> segments;
            public NativeList<float> segmentWidths;
            public float lifetime;
            public float age;
            public float intensity;
            public LightningType boltType;
            public Color coreColor;
            public Color outerColor;
            public bool isActive;
        }
        
        [System.Serializable]
        public struct ElectricalArc
        {
            public Entity sourceEntity;
            public Entity targetEntity;
            public Vector3 startPos;
            public Vector3 endPos;
            public float voltage;
            public float amperage;
            public float duration;
            public float age;
            public ArcBehavior behavior;
            public bool isGrounded;
            public NativeList<Vector3> arcPoints;
        }
        
        public enum LightningType { CloudToGround, GroundToCloud, InCloud, Ball, Chain, Forked }
        public enum ArcBehavior { Static, Oscillating, Pulsing, Random, Tracking }
        
        private NativeList<LightningBolt> _activeBolts;
        private NativeList<ElectricalArc> _activeArcs;
        private AnimationCurve _boltFlickerCurve;
        private Gradient _lightningColorGradient;
        
        protected override void OnCreate()
        {
            _activeBolts = new NativeList<LightningBolt>(Allocator.Persistent);
            _activeArcs = new NativeList<ElectricalArc>(Allocator.Persistent);
            InitializeCurves();
        }
        
        protected override void OnDestroy()
        {
            for (int i = 0; i < _activeBolts.Length; i++)
            {
                var bolt = _activeBolts[i];
                if (bolt.segments.IsCreated) bolt.segments.Dispose();
                if (bolt.segmentWidths.IsCreated) bolt.segmentWidths.Dispose();
            }
            
            for (int i = 0; i < _activeArcs.Length; i++)
            {
                var arc = _activeArcs[i];
                if (arc.arcPoints.IsCreated) arc.arcPoints.Dispose();
            }
            
            _activeBolts.Dispose();
            _activeArcs.Dispose();
        }
        
        private void InitializeCurves()
        {
            _boltFlickerCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.05f, 0.3f),
                new Keyframe(0.1f, 0.8f),
                new Keyframe(0.15f, 0.2f),
                new Keyframe(0.2f, 0.9f),
                new Keyframe(0.25f, 0.1f),
                new Keyframe(0.3f, 0.7f),
                new Keyframe(0.4f, 0.4f),
                new Keyframe(0.5f, 0.6f),
                new Keyframe(0.6f, 0.3f),
                new Keyframe(0.7f, 0.5f),
                new Keyframe(0.8f, 0.2f),
                new Keyframe(0.9f, 0.4f),
                new Keyframe(1f, 0f)
            );
            
            _lightningColorGradient = new Gradient();
            var colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(new Color(1f, 1f, 1f), 0f),
                new GradientColorKey(new Color(0.8f, 0.9f, 1f), 0.3f),
                new GradientColorKey(new Color(0.5f, 0.7f, 1f), 0.6f),
                new GradientColorKey(new Color(0.3f, 0.5f, 0.8f), 1f)
            };
            var alphaKeys = new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            };
            _lightningColorGradient.SetKeys(colorKeys, alphaKeys);
        }
        
        public int CreateLightningBolt(Vector3 start, Vector3 end, LightningType type, 
                                      float intensity = 1f, float lifetime = 0.5f)
        {
            var bolt = new LightningBolt
            {
                startPosition = start,
                endPosition = end,
                segments = new NativeList<Vector3>(Allocator.Temp),
                segmentWidths = new NativeList<float>(Allocator.Temp),
                lifetime = lifetime,
                age = 0f,
                intensity = intensity,
                boltType = type,
                coreColor = new Color(1f, 1f, 1f, 1f),
                outerColor = new Color(0.5f, 0.7f, 1f, 0.8f),
                isActive = true
            };
            
            GenerateBoltSegments(ref bolt, type);
            
            _activeBolts.Add(bolt);
            return _activeBolts.Length - 1;
        }
        
        private void GenerateBoltSegments(ref LightningBolt bolt, LightningType type)
        {
            Vector3 direction = bolt.endPosition - bolt.startPosition;
            float distance = direction.magnitude;
            int segmentCount = Mathf.RoundToInt(distance * 2f);
            float segmentLength = distance / segmentCount;
            
            bolt.segments.Add(bolt.startPosition);
            bolt.segmentWidths.Add(bolt.intensity * 0.5f);
            
            Vector3 currentPos = bolt.startPosition;
            
            for (int i = 1; i < segmentCount; i++)
            {
                float t = i / (float)segmentCount;
                Vector3 basePos = Vector3.Lerp(bolt.startPosition, bolt.endPosition, t);
                
                // Add randomness based on type
                float deviation = GetDeviationForType(type, t);
                Vector3 offset = new Vector3(
                    UnityEngine.Random.Range(-deviation, deviation),
                    UnityEngine.Random.Range(-deviation, deviation),
                    UnityEngine.Random.Range(-deviation, deviation)
                );
                
                currentPos = basePos + offset;
                bolt.segments.Add(currentPos);
                
                // Width varies along bolt
                float width = bolt.intensity * (1f - t) * UnityEngine.Random.Range(0.5f, 1f);
                bolt.segmentWidths.Add(width);
            }
            
            bolt.segments.Add(bolt.endPosition);
            bolt.segmentWidths.Add(0f);
            
            // Add branches for certain types
            if (type == LightningType.Forked || type == LightningType.Chain)
            {
                AddBranches(ref bolt, segmentCount);
            }
        }
        
        private float GetDeviationForType(LightningType type, float t)
        {
            switch (type)
            {
                case LightningType.CloudToGround:
                    return Mathf.Sin(t * math.PI) * 2f;
                case LightningType.GroundToCloud:
                    return Mathf.Sin(t * math.PI) * 1.5f;
                case LightningType.Forked:
                    return Mathf.Sin(t * math.PI * 3f) * 3f;
                case LightningType.Chain:
                    return UnityEngine.Random.Range(0.5f, 2f);
                case LightningType.Ball:
                    return 0.5f;
                default:
                    return 1f;
            }
        }
        
        private void AddBranches(ref LightningBolt bolt, int mainSegmentCount)
        {
            int branchCount = UnityEngine.Random.Range(2, 5);
            
            for (int b = 0; b < branchCount; b++)
            {
                int branchStartIndex = UnityEngine.Random.Range(1, mainSegmentCount - 2);
                Vector3 branchStart = bolt.segments[branchStartIndex];
                
                Vector3 branchDir = new Vector3(
                    UnityEngine.Random.Range(-1f, 1f),
                    UnityEngine.Random.Range(-0.5f, 0.5f),
                    UnityEngine.Random.Range(-1f, 1f)
                ).normalized;
                
                int branchSegments = UnityEngine.Random.Range(3, 8);
                float branchLength = UnityEngine.Random.Range(1f, 3f);
                
                for (int i = 0; i < branchSegments; i++)
                {
                    float bt = i / (float)branchSegments;
                    Vector3 branchPos = branchStart + branchDir * branchLength * bt;
                    branchPos += new Vector3(
                        UnityEngine.Random.Range(-0.5f, 0.5f),
                        UnityEngine.Random.Range(-0.5f, 0.5f),
                        UnityEngine.Random.Range(-0.5f, 0.5f)
                    );
                    
                    bolt.segments.Add(branchPos);
                    bolt.segmentWidths.Add(bolt.intensity * 0.3f * (1f - bt));
                }
            }
        }
        
        public int CreateElectricalArc(Entity source, Entity target, Vector3 startPos, Vector3 endPos,
                                       float voltage, float amperage, ArcBehavior behavior = ArcBehavior.Oscillating)
        {
            var arc = new ElectricalArc
            {
                sourceEntity = source,
                targetEntity = target,
                startPos = startPos,
                endPos = endPos,
                voltage = voltage,
                amperage = amperage,
                duration = voltage / 1000f, // Higher voltage = longer duration
                age = 0f,
                behavior = behavior,
                isGrounded = false,
                arcPoints = new NativeList<Vector3>(Allocator.Temp)
            };
            
            GenerateArcPoints(ref arc);
            
            _activeArcs.Add(arc);
            return _activeArcs.Length - 1;
        }
        
        private void GenerateArcPoints(ref ElectricalArc arc)
        {
            Vector3 direction = arc.endPos - arc.startPos;
            float distance = direction.magnitude;
            int pointCount = Mathf.RoundToInt(distance * 3f);
            
            arc.arcPoints.Add(arc.startPos);
            
            for (int i = 1; i < pointCount; i++)
            {
                float t = i / (float)pointCount;
                Vector3 basePos = Vector3.Lerp(arc.startPos, arc.endPos, t);
                
                float deviation = Mathf.Sin(t * math.PI) * (distance * 0.1f);
                Vector3 offset = new Vector3(
                    UnityEngine.Random.Range(-deviation, deviation),
                    UnityEngine.Random.Range(-deviation, deviation),
                    UnityEngine.Random.Range(-deviation, deviation)
                );
                
                arc.arcPoints.Add(basePos + offset);
            }
            
            arc.arcPoints.Add(arc.endPos);
        }
        
        public void UpdateBolt(int index, float deltaTime)
        {
            if (index < 0 || index >= _activeBolts.Length) return;
            
            var bolt = _activeBolts[index];
            if (!bolt.isActive) return;
            
            bolt.age += deltaTime;
            
            if (bolt.age >= bolt.lifetime)
            {
                bolt.isActive = false;
                _activeBolts[index] = bolt;
                return;
            }
            
            // Flicker effect
            float flickerValue = _boltFlickerCurve.Evaluate(bolt.age / bolt.lifetime);
            bolt.coreColor.a = flickerValue;
            bolt.outerColor.a = flickerValue * 0.7f;
            
            _activeBolts[index] = bolt;
        }
        
        public void UpdateArc(int index, float deltaTime)
        {
            if (index < 0 || index >= _activeArcs.Length) return;
            
            var arc = _activeArcs[index];
            if (arc.age >= arc.duration)
            {
                _activeArcs.RemoveAt(index);
                return;
            }
            
            arc.age += deltaTime;
            
            // Update arc points based on behavior
            switch (arc.behavior)
            {
                case ArcBehavior.Oscillating:
                    OscillateArc(ref arc, deltaTime);
                    break;
                case ArcBehavior.Pulsing:
                    PulseArc(ref arc, deltaTime);
                    break;
                case ArcBehavior.Random:
                    RandomizeArc(ref arc, deltaTime);
                    break;
                case ArcBehavior.Tracking:
                    TrackTarget(ref arc, deltaTime);
                    break;
            }
            
            _activeArcs[index] = arc;
        }
        
        private void OscillateArc(ref ElectricalArc arc, float deltaTime)
        {
            for (int i = 1; i < arc.arcPoints.Length - 1; i++)
            {
                var point = arc.arcPoints[i];
                float offset = Mathf.Sin(arc.age * 50f + i) * 0.2f;
                point += Vector3.right * offset;
                arc.arcPoints[i] = point;
            }
        }
        
        private void PulseArc(ref ElectricalArc arc, float deltaTime)
        {
            float pulse = (Mathf.Sin(arc.age * 30f) + 1f) * 0.5f;
            arc.voltage = arc.voltage * (0.5f + pulse * 0.5f);
        }
        
        private void RandomizeArc(ref ElectricalArc arc, float deltaTime)
        {
            if (UnityEngine.Random.value > 0.3f) return;
            
            for (int i = 1; i < arc.arcPoints.Length - 1; i++)
            {
                var point = arc.arcPoints[i];
                point += new Vector3(
                    UnityEngine.Random.Range(-0.3f, 0.3f),
                    UnityEngine.Random.Range(-0.3f, 0.3f),
                    UnityEngine.Random.Range(-0.3f, 0.3f)
                );
                arc.arcPoints[i] = point;
            }
        }
        
        private void TrackTarget(ref ElectricalArc arc, float deltaTime)
        {
            if (arc.targetEntity != Entity.Null)
            {
                // Would get target position from ECS world
                arc.endPos = arc.endPos + Vector3.up * Mathf.Sin(arc.age * 5f) * 0.5f;
                GenerateArcPoints(ref arc);
            }
        }
        
        public float CalculateDamage(int arcIndex, float distance)
        {
            if (index < 0 || index >= _activeArcs.Length) return 0f;
            
            var arc = _activeArcs[arcIndex];
            
            if (distance > 2f) return 0f; // Only damage at very close range
            
            float baseDamage = arc.voltage * arc.amperage * 0.001f;
            float distanceMod = 1f - (distance / 2f);
            
            return baseDamage * distanceMod;
        }
        
        public void DestroyBolt(int index)
        {
            if (index < 0 || index >= _activeBolts.Length) return;
            
            var bolt = _activeBolts[index];
            if (bolt.segments.IsCreated) bolt.segments.Dispose();
            if (bolt.segmentWidths.IsCreated) bolt.segmentWidths.Dispose();
            
            _activeBolts.RemoveAt(index);
        }
    }
}
