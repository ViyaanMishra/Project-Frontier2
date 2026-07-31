# AAA Low Poly Graphics Package

A comprehensive collection of high-quality shaders, animation systems, and VFX components for Unity URP (Universal Render Pipeline). Designed to deliver AAA-quality visuals with optimized low-poly aesthetics.

## Features Overview

### 🎨 Shaders

#### Terrain & Environment
- **LowPolyTerrainAAA.shader** - Advanced terrain shader with:
  - 8-layer splatmap blending with height-based transitions
  - Triplanar mapping for seamless UV projection
  - Parallax occlusion mapping for depth
  - Slope and altitude tinting
  - Wetness and ambient occlusion maps
  - Tessellation support for geometric detail
  - Microsurface detail enhancement

- **OceanWater.shader** - Premium water shader featuring:
  - Vertex displacement waves
  - Dual normal map scrolling
  - Depth-based color fading
  - Foam generation at wave peaks
  - Fresnel-based rim lighting
  - Caustics simulation
  - Refraction and transparency

- **VolumetricFog.shader** - Atmospheric fog with:
  - 3D noise-based volumetric variation
  - Height-based density falloff
  - Henyey-Greenstein light scattering
  - Sun direction-based god rays
  - Depth fade for distant atmospherics

#### Characters & Objects
- **LowPolyCharacterAAA.shader** - Character shader including:
  - Full PBR workflow with mask maps
  - Subsurface scattering approximation
  - Dynamic rim lighting
  - Cloth/fabric shading with weave patterns
  - Wetness and blood/damage overlays
  - Detail normal mapping
  - Fresnel outline pass for stylized look

#### VFX & Particles
- **ParticleVolumetric.shader** - Volumetric particle system with:
  - 3D noise integration for volume
  - Soft particle depth fading
  - Fresnel rim effects
  - Subsurface scattering
  - UV rotation and scrolling
  - Distortion effects
  - Emission and tint controls

### 🎭 Animation Systems

#### ProceduralAnimation.cs
Advanced procedural animation controller featuring:
- **Inverse Kinematics (IK)**
  - Hand targeting for object interaction
  - Head tracking with smooth interpolation
  - Body rotation based on movement direction
  
- **Procedural Walking**
  - Foot IK for uneven terrain
  - Step height and speed customization
  - Ground snapping via raycast
  
- **Secondary Motion**
  - Spring-physics based bone movement
  - Configurable stiffness and damping
  - Perfect for hair, clothing, accessories
  
- **Breathing Animation**
  - Procedural chest movement
  - Configurable rate and amplitude
  
- **Arm Swinging**
  - Velocity-based arm motion
  - Natural walking animation enhancement

### ✨ Visual Effects

#### VFXManager.cs
Centralized VFX control system providing:
- **Dynamic Lighting**
  - Perlin noise-based light flickering
  - Temporary light creation for explosions
  - Light intensity modulation
  
- **Atmospheric Effects**
  - God rays intensity control
  - Atmospheric scattering parameters
  - Dynamic vignette based on game state
  
- **Weather VFX**
  - Rain, snow, and dust storm particle systems
  - Intensity-based emission control
  - Seamless weather transitions
  
- **Combat VFX**
  - Muzzle flash instantiation
  - Impact spark effects
  - Explosion triggers with screen shake
  - Post-process flash effects
  
- **Screen Space Effects**
  - Motion blur based on velocity
  - Chromatic aberration for anomalies
  - Film grain for damage states
  - Dynamic bloom adjustment

#### DynamicPostProcessManager.cs
Context-aware post-processing controller:
- **Biome-Based Color Grading**
  - Wasteland, forest, arctic, anomaly presets
  - Automatic saturation and contrast adjustment
  
- **Weather Integration**
  - Rain/storm vignette and desaturation
  - Fog atmospheric adjustments
  - Clear sky bloom enhancement
  
- **Time of Day System**
  - Sunrise/sunset color tones
  - Night exposure compensation
  - Smooth transitions between periods

## Installation

1. Copy the following folders into your Unity project's `Assets` directory:
   - `FrontierProject/Assets/Shaders/URP/`
   - `FrontierProject/Assets/Scripts/Graphics/`

2. Ensure you have Unity's Universal Render Pipeline (URP) installed via Package Manager.

3. For best results, configure your URP Asset with:
   - Depth Texture: Enabled
   - Opaque Texture: Enabled
   - MSAA: 4x or higher
   - Render Scale: 1.0

## Usage Examples

### Applying the Terrain Shader
```csharp
// Create material with AAA terrain shader
Material terrainMat = new Material(Shader.Find("Frontier/URP/Environment/LowPolyTerrainAAA"));

// Assign textures
terrainMat.SetTexture("_SplatMap", splatMapTexture);
terrainMat.SetTexture("_TerrainHeight", heightMapTexture);

// Configure layer tiling
terrainMat.SetVector("_Layer0Tiling", new Vector4(30, 30, 0, 0));
```

### Setting Up Procedural Animation
```csharp
// Add to character GameObject
ProceduralAnimation procAnim = gameObject.AddComponent<ProceduralAnimation>();

// Configure IK targets
procAnim.lookTarget = cameraTransform;
procAnim.leftHandTarget = weaponLeftHand;
procAnim.rightHandTarget = weaponRightHand;

// Enable secondary motion for hair/clothing
procAnim.enableSecondaryMotion = true;
procAnim.secondaryBones = hairBones;
```

### Using VFX Manager
```csharp
// Get VFX manager instance
VFXManager vfx = FindObjectOfType<VFXManager>();

// Trigger explosion
vfx.TriggerExplosionVFX(explosionPosition, magnitude: 5f);

// Change weather
vfx.SetWeatherType("rain", intensity: 0.8f);

// Enable god rays
vfx.EnableGodRays(true);
```

## Performance Considerations

- **Shader LOD**: All shaders include LOD directives for automatic quality scaling
- **Instancing**: Supported on all shaders for draw call optimization
- **Parallax Mapping**: Can be disabled on lower-end devices via keyword stripping
- **Particle Count**: Volumetric particles capped at 10,000 by default (adjustable)
- **Tessellation**: Optional feature - disable for mobile platforms

## Customization Guide

### Creating New Materials
1. Right-click in Project window → Create → Material
2. Select the appropriate shader from `Frontier/URP/` category
3. Configure properties in Inspector
4. Drag onto GameObject

### Adding Custom VFX
1. Extend `VFXManager.cs` with new particle system references
2. Add public methods for triggering custom effects
3. Integrate with game events (damage, pickups, etc.)

### Modifying Shaders
All shaders are well-commented and use standard URP includes. Key modification points:
- `CBUFFER_START`: Add new properties
- `frag()`: Modify pixel-level rendering
- `vert()`: Adjust vertex manipulation

## Technical Specifications

- **Unity Version**: 2021.3 LTS or newer recommended
- **Render Pipeline**: Universal Render Pipeline (URP) 12.x+
- **Shader Model**: 3.5 minimum, 4.5 recommended for full features
- **Platform Support**: PC, Console, Mobile (with quality adjustments)

## File Structure

```
FrontierProject/Assets/
├── Shaders/
│   ├── URP/
│   │   ├── LowPolyTerrainAAA.shader
│   │   ├── LowPolyCharacterAAA.shader
│   │   ├── OceanWater.shader
│   │   ├── VolumetricFog.shader
│   │   └── ParticleVolumetric.shader
│   └── Expanded/
│       └── (additional shaders)
└── Scripts/
    └── Graphics/
        ├── DynamicPostProcessManager.cs
        ├── VFX/
        │   └── VFXManager.cs
        └── Animations/
            └── ProceduralAnimation.cs
```

## License

This graphics package is provided as part of the Frontier project. See project license for usage terms.

## Support & Documentation

For additional documentation, troubleshooting, or feature requests, please refer to the main project repository.

---

**Version**: 1.0.0  
**Last Updated**: 2024  
**Compatibility**: Unity 2021.3+ with URP
