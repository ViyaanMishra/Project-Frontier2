using UnityEngine;
using System.Collections.Generic;

namespace AAA.LowPoly.Graphics
{
    /// <summary>
    /// Advanced LOD (Level of Detail) system with smooth transitions and HLOD support
    /// Provides AAA-quality automatic detail management based on distance and screen size
    /// </summary>
    [ExecuteInEditMode]
    public class AdvancedLODSystem : MonoBehaviour
    {
        [System.Serializable]
        public class LODGroup
        {
            public GameObject[] meshes;
            public float[] screenRelativeTransitionHeights;
            public float[] fadeTransitionWidth = new float[] { 0.1f, 0.1f, 0.1f };
            public bool useCrossFade = true;
            public bool animateTransitions = true;
            public float transitionDuration = 0.3f;
        }

        [Header("LOD Settings")]
        [Tooltip("Enable automatic LOD switching")]
        public bool autoLODEnabled = true;
        
        [Tooltip("Use screen-relative transitions instead of distance-based")]
        public bool useScreenRelative = true;
        
        [Tooltip("Base transition distance when not using screen-relative")]
        public float baseTransitionDistance = 50f;
        
        [Tooltip("Quality multiplier for LOD selection (higher = more detailed)")]
        [Range(0.5f, 2f)]
        public float qualityMultiplier = 1f;

        [Header("HLOD (Hierarchical LOD)")]
        [Tooltip("Enable HLOD for distant objects")]
        public bool enableHLOD = true;
        
        [Tooltip("Distance at which HLOD takes over")]
        public float hlodDistance = 200f;
        
        [Tooltip("HLOD mesh to use at extreme distances")]
        public GameObject hlodMesh;
        
        [Tooltip("Billboard texture for extreme distances")]
        public Texture2D billboardTexture;
        
        [Tooltip("Material to use for billboard rendering")]
        public Material billboardMaterial;

        [Header("Smooth Transitions")]
        [Tooltip("Enable smooth alpha fading between LODs")]
        public bool smoothTransitions = true;
        
        [Tooltip("Duration of LOD transition animations")]
        public float transitionDuration = 0.3f;
        
        [Tooltip("Use dithering for smoother transitions")]
        public bool useDithering = true;
        
        [Tooltip("Dithering pattern scale")]
        public float ditherScale = 0.1f;

        [Header("Performance")]
        [Tooltip("Update frequency in frames (1 = every frame)")]
        [Range(1, 60)]
        public int updateFrequency = 2;
        
        [Tooltip("Use occlusion culling to skip hidden objects")]
        public bool useOcclusionCulling = true;
        
        [Tooltip("Minimum render size in pixels")]
        public float minRenderSize = 10f;

        [Header("Animation LOD")]
        [Tooltip("Reduce animation updates at distance")]
        public bool animateLOD = true;
        
        [Tooltip("Animation update distances")]
        public float[] animationLODDistances = new float[] { 20f, 50f, 100f };
        
        [Tooltip("Animation update rates (1 = full speed)")]
        public float[] animationUpdateRates = new float[] { 1f, 0.5f, 0.25f, 0.1f };

        [Header("Debug")]
        public bool showDebugInfo = false;
        public bool visualizeLODBounds = false;

        private LODGroup[] lodGroups;
        private MeshRenderer[] meshRenderers;
        private SkinnedMeshRenderer[] skinnedMeshRenderers;
        private Camera mainCamera;
        private int frameCounter = 0;
        private int currentLOD = 0;
        private float[] transitionAlphas;
        private Dictionary<MeshRenderer, Material[]> originalMaterials = new Dictionary<MeshRenderer, Material[]>();
        private Dictionary<MeshRenderer, Material[]> fadeMaterials = new Dictionary<MeshRenderer, Material[]>();

        // Shader properties for dithering
        private static readonly int DitherAlphaID = Shader.PropertyToID("_DitherAlpha");
        private static readonly int TransitionAlphaID = Shader.PropertyToID("_TransitionAlpha");

        void Awake()
        {
            Initialize();
        }

        void OnEnable()
        {
            Initialize();
        }

        void Initialize()
        {
            mainCamera = Camera.main;
            
            meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
            skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
            lodGroups = GetComponentsInChildren<LODGroup>(true);

            // Cache original materials
            foreach (var renderer in meshRenderers)
            {
                if (!originalMaterials.ContainsKey(renderer))
                {
                    originalMaterials[renderer] = renderer.sharedMaterials;
                }
            }

            // Initialize transition alphas
            transitionAlphas = new float[lodGroups.Length];
            for (int i = 0; i < transitionAlphas.Length; i++)
            {
                transitionAlphas[i] = 1f;
            }
        }

        void LateUpdate()
        {
            if (!autoLODEnabled || mainCamera == null)
                return;

            frameCounter++;
            if (frameCounter % updateFrequency != 0)
                return;

            UpdateLODs();
        }

        void UpdateLODs()
        {
            Vector3 cameraPosition = mainCamera.transform.position;
            float distance = Vector3.Distance(cameraPosition, transform.position);

            // Check occlusion
            if (useOcclusionCulling && !IsVisible())
            {
                SetLODActive(false);
                return;
            }
            else
            {
                SetLODActive(true);
            }

            // Calculate appropriate LOD level
            int targetLOD = CalculateLODLevel(distance);
            
            if (enableHLOD && distance > hlodDistance)
            {
                ShowHLOD();
                return;
            }

            // Smooth transition between LODs
            if (smoothTransitions && targetLOD != currentLOD)
            {
                StartCoroutine(TransitionLOD(currentLOD, targetLOD));
            }
            else
            {
                SetLOD(targetLOD);
            }

            // Update animation LOD
            if (animateLOD)
            {
                UpdateAnimationLOD(distance);
            }

            currentLOD = targetLOD;

            if (showDebugInfo)
            {
                DebugLODInfo(distance, targetLOD);
            }
        }

        int CalculateLODLevel(float distance)
        {
            if (useScreenRelative)
            {
                // Calculate screen-relative size
                Bounds bounds = GetBounds();
                float screenSize = CalculateScreenSize(bounds);
                
                // Determine LOD based on screen size
                if (screenSize > 0.5f * qualityMultiplier)
                    return 0;
                else if (screenSize > 0.25f * qualityMultiplier)
                    return 1;
                else if (screenSize > 0.125f * qualityMultiplier)
                    return 2;
                else
                    return 3;
            }
            else
            {
                // Distance-based LOD
                float adjustedDistance = distance / qualityMultiplier;
                
                if (adjustedDistance < baseTransitionDistance * 0.3f)
                    return 0;
                else if (adjustedDistance < baseTransitionDistance * 0.6f)
                    return 1;
                else if (adjustedDistance < baseTransitionDistance)
                    return 2;
                else
                    return 3;
            }
        }

        float CalculateScreenSize(Bounds bounds)
        {
            if (mainCamera == null)
                return 0f;

            // Calculate bounding sphere screen size
            float distance = Vector3.Distance(mainCamera.transform.position, bounds.center);
            float radius = bounds.extents.magnitude;
            
            // Screen space size approximation
            float fovRad = mainCamera.fieldOfView * Mathf.Deg2Rad;
            float screenHeightAtDistance = 2f * Mathf.Tan(fovRad * 0.5f) * distance;
            float screenWidthAtDistance = screenHeightAtDistance * mainCamera.aspect;
            
            float screenRatioX = (radius * 2f) / screenWidthAtDistance;
            float screenRatioY = (radius * 2f) / screenHeightAtDistance;
            
            return Mathf.Max(screenRatioX, screenRatioY);
        }

        Bounds GetBounds()
        {
            Bounds totalBounds = new Bounds(transform.position, Vector3.zero);
            
            foreach (var renderer in meshRenderers)
            {
                if (renderer.enabled)
                {
                    totalBounds.Encapsulate(renderer.bounds);
                }
            }
            
            foreach (var renderer in skinnedMeshRenderers)
            {
                if (renderer.enabled)
                {
                    totalBounds.Encapsulate(renderer.bounds);
                }
            }
            
            return totalBounds;
        }

        System.Collections.IEnumerator TransitionLOD(int fromLOD, int toLOD)
        {
            float elapsed = 0f;
            
            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / transitionDuration;
                float alpha = Mathf.SmoothStep(0, 1, t);
                
                // Apply dithering or alpha fade
                if (useDithering)
                {
                    ApplyDitheringTransition(alpha);
                }
                else
                {
                    ApplyAlphaTransition(fromLOD, toLOD, alpha);
                }
                
                yield return null;
            }
            
            SetLOD(toLOD);
            ClearTransitionEffects();
        }

        void ApplyDitheringTransition(float alpha)
        {
            foreach (var renderer in meshRenderers)
            {
                if (renderer == null) continue;
                
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material != null)
                    {
                        material.SetFloat(DitherAlphaID, alpha);
                    }
                }
            }
        }

        void ApplyAlphaTransition(int fromLOD, int toLOD, float alpha)
        {
            // Implementation for alpha-based cross-fading
            // Requires transparent materials or custom shader support
        }

        void ClearTransitionEffects()
        {
            foreach (var renderer in meshRenderers)
            {
                if (renderer == null) continue;
                
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material != null)
                    {
                        material.SetFloat(DitherAlphaID, 1f);
                        material.SetFloat(TransitionAlphaID, 1f);
                    }
                }
            }
        }

        void SetLOD(int lodLevel)
        {
            // Enable/disable LOD groups
            for (int i = 0; i < lodGroups.Length; i++)
            {
                if (lodGroups[i] != null && lodGroups[i].meshes != null)
                {
                    for (int j = 0; j < lodGroups[i].meshes.Length; j++)
                    {
                        if (lodGroups[i].meshes[j] != null)
                        {
                            lodGroups[i].meshes[j].SetActive(j <= lodLevel);
                        }
                    }
                }
            }
        }

        void SetLODActive(bool active)
        {
            foreach (var renderer in meshRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = active;
                }
            }
            
            foreach (var renderer in skinnedMeshRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = active;
                }
            }
        }

        void ShowHLOD()
        {
            // Hide regular meshes
            SetLODActive(false);
            
            // Show HLOD mesh if available
            if (hlodMesh != null)
            {
                hlodMesh.SetActive(true);
            }
            
            // Could implement billboard rendering here
        }

        void UpdateAnimationLOD(float distance)
        {
            int animationLODLevel = 0;
            
            for (int i = 0; i < animationLODDistances.Length; i++)
            {
                if (distance > animationLODDistances[i])
                {
                    animationLODLevel = i + 1;
                }
            }
            
            animationLODLevel = Mathf.Min(animationLODLevel, animationUpdateRates.Length - 1);
            
            float updateRate = animationUpdateRates[animationLODLevel];
            
            // Apply to animators
            var animators = GetComponentsInChildren<Animator>(true);
            foreach (var animator in animators)
            {
                if (animator != null)
                {
                    animator.updateMode = AnimatorUpdateMode.Normal;
                    animator.cullingMode = AnimatorCullingMode.CullCompletely;
                    
                    // Note: Actual animation rate limiting requires custom implementation
                }
            }
        }

        bool IsVisible()
        {
            // Basic visibility check
            // In production, integrate with Unity's occlusion culling system
            return true;
        }

        void DebugLODInfo(float distance, int lodLevel)
        {
            string debugText = $"LOD: {lodLevel}\nDistance: {distance:F1}m\n";
            
            if (useScreenRelative)
            {
                Bounds bounds = GetBounds();
                float screenSize = CalculateScreenSize(bounds);
                debugText += $"Screen Size: {screenSize:P2}\n";
            }
            
            debugText += $"Quality Mult: {qualityMultiplier:F2}";
            
            // Could add GUIText or Debug.Log here
        }

        void OnDrawGizmosSelected()
        {
            if (!visualizeLODBounds)
                return;
            
            Bounds bounds = GetBounds();
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
            
            // Draw LOD distance spheres
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, baseTransitionDistance * 0.3f);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, baseTransitionDistance * 0.6f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, baseTransitionDistance);
            
            if (enableHLOD)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(transform.position, hlodDistance);
            }
        }

        // Public API
        public void ForceLOD(int lodLevel)
        {
            autoLODEnabled = false;
            SetLOD(lodLevel);
        }

        public void ResetLOD()
        {
            autoLODEnabled = true;
            currentLOD = 0;
        }

        public int GetCurrentLOD()
        {
            return currentLOD;
        }

        public void SetQualityMultiplier(float multiplier)
        {
            qualityMultiplier = Mathf.Clamp(multiplier, 0.5f, 2f);
        }
    }
}
