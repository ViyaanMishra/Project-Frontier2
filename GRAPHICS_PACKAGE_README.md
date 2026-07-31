# AAA Low Poly Graphics Package

A comprehensive collection of high-quality shaders, scripts, and VFX systems for creating AAA-quality low poly games in Unity.

## 📁 Package Structure

```
Assets/
├── Shaders/
│   ├── TerrainPBR.shader          # Advanced terrain shader with tessellation
│   ├── StylizedCharacter.shader   # Character shader with cel-shading
│   ├── VFX_ParticleMaster.shader  # Master particle shader
│   └── Water_Advanced.shader      # Photorealistic water shader
├── Materials/                      # Pre-configured materials
├── Textures/                       # Texture assets
├── Prefabs/                        # Ready-to-use prefabs
├── Models/                         # 3D models
├── Animations/                     # Animation clips
├── VFX/                            # Visual effects
└── Scripts/
    ├── Graphics/
    │   └── AdvancedLODSystem.cs   # LOD management system
    ├── Rendering/                  # Rendering utilities
    ├── VFX/
    │   └── AdvancedVFXManager.cs  # VFX management system
    └── PostProcessing/             # Post-processing effects
```

## 🎨 Shaders

### 1. TerrainPBR Shader (`AAA/LowPoly/TerrainPBR`)

**Features:**
- **Parallax Occlusion Mapping (POM)** - Realistic depth perception
- **Tessellation** - Dynamic geometry detail based on distance
- **Triplanar Mapping** - Seamless texture projection on complex terrain
- **Detail Maps** - High-frequency surface details
- **Dynamic Snow Accumulation** - Altitude and normal-based snow
- **Wetness Effects** - Dynamic wetness from rain
- **Rim Lighting** - Stylized edge highlighting
- **Ambient Occlusion** - Contact shadows

**Key Properties:**
```
_MainTex, _NormalMap, _HeightMap - Base textures
_TessellationDistance - LOD control
_ParallaxOffset - Depth intensity
_SnowThreshold, _SnowColor - Snow settings
_Wetness, _WetColor - Wet surface effect
_RimColor, _RimPower - Rim lighting
```

**Usage:**
1. Create material with `AAA/LowPoly/TerrainPBR` shader
2. Assign albedo, normal, and height maps
3. Adjust tessellation distance for performance
4. Enable triplanar mapping for seamless terrain

---

### 2. StylizedCharacter Shader (`AAA/LowPoly/StylizedCharacter`)

**Features:**
- **Cel/Toon Shading** - Stylized non-photorealistic rendering
- **Outline Pass** - Configurable character outlines
- **Ramp Textures** - Custom shading gradients
- **Subsurface Scattering** - Skin translucency approximation
- **Anisotropic Highlights** - Hair/fur specular
- **Dissolve Effect** - Teleportation/death effects
- **Holographic Mode** - Sci-fi hologram effect
- **Damage & Dirt Overlays** - Wear and tear
- **Fresnel & Rim Lighting** - Edge highlights
- **Vertex Color Support** - Paint details directly on mesh

**Key Properties:**
```
_Color, _MainTex, _NormalMap - Base appearance
_RampTexture - Cel shading gradient
_OutlineColor, _OutlineWidth - Outline settings
_RimColor, _RimPower - Rim lighting
_SSSColor, _SSSDistance - Subsurface scattering
_DissolveThreshold - Dissolve effect control
_HolographicIntensity - Hologram effect
_DamageMap, _DamageIntensity - Damage overlay
```

**Usage:**
1. Create material with `AAA/LowPoly/StylizedCharacter` shader
2. Assign character textures
3. Configure outline width for cartoon style
4. Use vertex colors for additional detail
5. Animate dissolve threshold for effects

---

### 3. VFX ParticleMaster Shader (`AAA/LowPoly/VFX/ParticleMaster`)

**Features:**
- **Sprite Sheet Animation** - Animated particle textures
- **Soft Particles** - Depth-based fading
- **Color Over Lifetime** - Gradient color animation
- **Size & Rotation Over Lifetime** - Dynamic particle transforms
- **Noise Distortion** - Procedural particle deformation
- **Turbulence** - Swirling motion effects
- **Sparkle/Scintillation** - Random bright flashes
- **Depth Fade** - Intersection glow effects
- **Flowmap Support** - Directed particle movement
- **Dual Pass Rendering** - Additive glow pass

**Key Properties:**
```
_MainTex, _Color - Base particle appearance
_SizeOverLife - 4-point size curve
_ColorOverLifeStart/Mid/End - Color stages
_EmissionStrength, _EmissionColor - Glow settings
_NoiseStrength, _NoiseScale - Distortion
_SoftParticleFade - Depth fade distance
_SparkleIntensity - Twinkle effect
_FlowMap, _FlowStrength - Flow direction
```

**Usage:**
1. Create particle system with custom material
2. Assign `AAA/LowPoly/VFX/ParticleMaster` shader
3. Configure sprite sheet animation frames
4. Enable soft particles for quality
5. Use flowmaps for rivers/smoke trails

---

### 4. AdvancedWater Shader (`AAA/LowPoly/Water/AdvancedWater`)

**Features:**
- **Gerstner Waves** - Physically-based wave simulation
- **Dual Normal Maps** - Detailed surface ripples
- **Foam Generation** - Shoreline and crest foam
- **Caustics** - Underwater light patterns
- **Screen-Space Refraction** - Realistic light bending
- **Fresnel Reflections** - Angle-based reflectivity
- **Depth-Based Absorption** - Light attenuation in water
- **Flowmaps** - River current simulation
- **Iridescence** - Oil slick rainbow effects
- **Tessellation** - Wave geometry detail
- **Underwater Fog** - Submerged view effects

**Key Properties:**
```
_Color, _DeepColor, _ShallowColor - Water colors
_WaveSpeed, _WaveHeight, _WaveTiling - Wave settings
_NormalMap1/2, _NormalStrength - Surface detail
_FoamTexture, _FoamThreshold - Foam generation
_CausticsTexture, _CausticsStrength - Caustic patterns
_ReflectionStrength, _RefractionStrength - Optical effects
_FresnelPower, _FresnelBias - Reflection angle
_AbsorptionColor, _AbsorptionDistance - Light absorption
_FlowMap, _FlowStrength - Current direction
_IridescenceStrength - Rainbow sheen
```

**Usage:**
1. Create water plane with material
2. Assign `AAA/LowPoly/Water/AdvancedWater` shader
3. Configure wave parameters for desired sea state
4. Add normal maps for surface detail
5. Enable caustics for underwater scenes
6. Use flowmaps for rivers

---

## 💻 Scripts

### AdvancedLODSystem.cs

**Purpose:** Automatic level-of-detail management with smooth transitions

**Features:**
- Screen-relative LOD calculation
- Hierarchical LOD (HLOD) support
- Smooth dithering transitions
- Animation LOD (reduced update rate at distance)
- Occlusion culling integration
- Auto quality adjustment
- Billboard rendering for distant objects

**Usage:**
```csharp
// Attach to any GameObject with multiple LOD meshes
AdvancedLODSystem lod = gameObject.AddComponent<AdvancedLODSystem>();
lod.qualityMultiplier = 1.5f;  // Increase detail
lod.smoothTransitions = true;   // Enable fade transitions
lod.enableHLOD = true;          // Enable distant HLOD

// Force specific LOD
lod.ForceLOD(0);  // Highest quality
lod.ResetLOD();   // Return to automatic
```

**Inspector Settings:**
- `Quality Multiplier` - Global detail level (0.5-2.0)
- `Base Transition Distance` - LOD switch distances
- `Transition Duration` - Fade animation time
- `Update Frequency` - Performance optimization
- `Enable HLOD` - Distant object simplification

---

### AdvancedVFXManager.cs

**Purpose:** Centralized VFX system with weather integration and performance optimization

**Features:**
- Weather effect integration (rain, snow, fog)
- Dynamic particle lighting
- GPU instancing for performance
- Soft particle blending
- Auto quality adjustment based on FPS
- Collision particle spawning
- Preset system for different scenarios

**Usage:**
```csharp
// Access singleton
AdvancedVFXManager vfx = AdvancedVFXManager.Instance;

// Control weather
vfx.SetWeatherIntensity(0.8f);  // Heavy weather

// Play specific effects
vfx.PlayEffect("Explosion");
vfx.StopEffect("Fire");

// Load preset
vfx.LoadPreset(2);  // Combat preset
```

**Inspector Settings:**
- `Weather Intensity` - Global weather strength
- `Max Dynamic Lights` - Performance limit
- `Soft Particle Fade` - Quality setting
- `Target FPS` - Auto-adjustment target
- `Min/Max Particle Count` - Quality bounds

---

## 🎯 Best Practices

### Performance Optimization

1. **LOD System**
   - Use screen-relative LOD for consistent quality
   - Enable HLOD for large scenes
   - Set appropriate transition distances

2. **Shaders**
   - Disable tessellation on mobile/VR
   - Reduce parallax steps for performance
   - Use detail maps sparingly

3. **VFX**
   - Enable GPU instancing
   - Limit dynamic lights
   - Use auto quality adjustment
   - Cap particle counts

### Quality Settings

**Ultra:**
- Tessellation: Enabled
- Parallax Steps: 32
- Soft Particles: Enabled
- Dynamic Lights: 8+
- Max Particles: 10000

**High:**
- Tessellation: Enabled (reduced)
- Parallax Steps: 16
- Soft Particles: Enabled
- Dynamic Lights: 4
- Max Particles: 5000

**Medium:**
- Tessellation: Disabled
- Parallax Steps: 8
- Soft Particles: Basic
- Dynamic Lights: 2
- Max Particles: 2000

**Low:**
- Tessellation: Disabled
- Parallax: Disabled
- Soft Particles: Disabled
- Dynamic Lights: 0
- Max Particles: 500

---

## 🔧 Integration Guide

### Setting Up a New Scene

1. **Import Package** - Copy all folders to Assets/
2. **Configure Camera** - Enable depth texture for soft particles
3. **Add Managers** - Create empty GameObjects for LOD and VFX managers
4. **Setup Lighting** - Configure directional light with shadows
5. **Add Post-Processing** - Apply bloom, color grading, ambient occlusion

### Creating Materials

1. Right-click in Project window → Create → Material
2. Select appropriate shader from `AAA/LowPoly/` path
3. Assign textures and configure properties
4. Save as prefab for reuse

### Optimizing for Target Platform

**PC/Console:**
- Enable all features
- High tessellation
- Full post-processing stack

**Mobile:**
- Disable tessellation
- Reduce parallax steps
- Limit dynamic lights
- Reduce particle counts

**VR:**
- Fixed tessellation
- Reduced draw calls
- Stereo instancing
- Performance priority

---

## 📋 Requirements

- Unity 2020.3 LTS or newer
- Shader Model 4.5+ for full features
- DirectX 11 / OpenGL 4.4 / Metal
- 4GB+ RAM recommended

---

## 📄 License

This package is provided as-is for educational and commercial use.

---

## 🤝 Support

For issues, questions, or contributions, please refer to the project repository.

---

**Version:** 1.0.0  
**Last Updated:** 2024  
**Author:** AAA Low Poly Graphics Team
