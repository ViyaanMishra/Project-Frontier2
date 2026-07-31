# Project Frontier - AAA Low Poly Asset System

## Overview
This project now includes a **procedural AAA-quality low poly asset generation system** that automatically replaces blocky placeholder geometry with true-to-shape, high-quality meshes.

## What's Included

### 🎨 High-Quality Materials (Pre-created)
Located in `Assets/GeneratedAssets/Materials/`:
- **M_ForestGreen** - Rich forest foliage material
- **M_WoodBark** - Natural wood texture for trees and barrels
- **M_StoneGray** - Realistic stone material for rocks
- **M_DirtBrown** - Earth/dirt ground material
- **M_MetalSilver** - Polished metallic surface
- **M_PlasticWhite** - Clean plastic/ceramic look
- **M_WaterBlue** - Transparent water with proper alpha blending
- **M_GlassBlue** - Clear glass with transparency

### 🔧 Procedural Mesh Generation (Editor Script)
The `ProceduralAssetGenerator.cs` script automatically generates optimized low-poly meshes:

**Generated Meshes:**
- **SM_Rock_01** - Icosphere-based rock with natural noise variation (not a cube!)
- **SM_Tree_Trunk_01** - Cylindrical trunk with slight organic irregularity
- **SM_Tree_Leaves_01** - Cone-shaped foliage cluster
- **SM_Barrel_01** - Bulged cylinder barrel shape (true barrel form)
- **SM_Character_Body** - Smooth capsule character body

### 🚀 Runtime Asset Replacement
The `AssetReplacer.cs` component can be attached to any GameObject to automatically upgrade it to AAA quality at runtime.

## How It Works

### Automatic Generation on Import
When you import this project into Unity:
1. The `[InitializeOnLoadMethod]` in `ProceduralAssetGenerator` detects missing assets
2. Automatically generates all meshes and materials
3. Assets are saved to `Assets/GeneratedAssets/`

### Manual Generation (Optional)
If you want to regenerate assets:
1. Open Unity Editor
2. Go to **Tools → Project Frontier → Generate AAA Assets**
3. Click "Generate All Assets"

### Using AssetReplacer Component
Attach to any GameObject:
```csharp
// In Unity Inspector:
// 1. Add Component → AssetReplacer
// 2. Select Asset Type (Rock, Tree, Barrel, Character, Custom)
// 3. Enable randomization options for variety
```

## Asset Quality Features

### No More Blocky Placeholders!
✅ **Rocks**: Smooth icospheres with procedural noise for natural variation  
✅ **Trees**: Proper cylindrical trunks + cone foliage (not cubes)  
✅ **Barrels**: Bulged cylinder shape true to real barrels  
✅ **Characters**: Smooth capsule geometry  
✅ **Materials**: PBR-lite setup with proper smoothness/metallic values  

### Optimization
- Low polygon counts (performance-friendly)
- Proper UV coordinates ready for textures
- Recalculated normals for correct lighting
- Tangent data for normal mapping support

## File Structure
```
Assets/
├── GeneratedAssets/
│   ├── Materials/          # Pre-created URP materials
│   │   ├── M_ForestGreen.mat
│   │   ├── M_WoodBark.mat
│   │   ├── M_StoneGray.mat
│   │   ├── M_DirtBrown.mat
│   │   ├── M_MetalSilver.mat
│   │   ├── M_PlasticWhite.mat
│   │   ├── M_WaterBlue.mat
│   │   └── M_GlassBlue.mat
│   ├── Meshes/             # Generated at import/build time
│   │   ├── SM_Rock_01.asset
│   │   ├── SM_Tree_Trunk_01.asset
│   │   ├── SM_Tree_Leaves_01.asset
│   │   ├── SM_Barrel_01.asset
│   │   └── SM_Character_Body.asset
│   └── Prefabs/            # Ready-to-use prefabs (optional)
├── Scripts/
│   ├── Editor/
│   │   ├── ProceduralAssetGenerator.cs    # Mesh/material generator
│   │   └── ProjectFrontier.Editor.asmdef
│   └── Runtime/
│       ├── AssetReplacer.cs               # Runtime replacement component
│       └── ProjectFrontier.Runtime.asmdef
└── Scenes/
    └── MainGame.unity
```

## Usage Examples

### Creating a Rock Formation
1. Create empty GameObject or use existing cube
2. Add Component → `AssetReplacer`
3. Set Asset Type to **Rock**
4. Enable **Randomize Scale** (0.8 to 1.5 for variety)
5. Enable **Randomize Rotation**
6. Duplicate multiple times for natural rock field

### Creating a Forest
1. Create parent GameObject "Tree"
2. Add `AssetReplacer` component
3. Set Asset Type to **Tree**
4. Adjust scale range (0.8 to 1.2)
5. Duplicate across your terrain

### Creating Props
Use the same system for barrels, crates, and other props - all will have proper non-blocky geometry.

## Technical Details

### Geometry Algorithms
- **Icosphere Generation**: Subdivided icosahedron for smooth spheres
- **Cylinder Generation**: Parametric cylinder with configurable segments
- **Noise Application**: Per-vertex displacement for organic variation
- **Bulge Mapping**: Sinusoidal displacement for barrel shapes

### Material Properties
All materials use **Universal Render Pipeline/Lit** shader with:
- Albedo color (tintable)
- Smoothness (0.2 - 0.95 range)
- Metallic (0.0 - 0.9 range)
- Optional transparency (water, glass)

## Performance
- Meshes generated once at editor time (not runtime)
- Low poly counts: 200-800 triangles per asset
- Materials use simple URP Lit shader (mobile-friendly)
- Instancing-ready for batch rendering

## Troubleshooting

### Assets Not Generating?
1. Check Console for errors
2. Ensure URP is installed (Package Manager)
3. Manually run: Tools → Project Frontier → Generate AAA Assets

### Materials Pink?
- URP not configured - ensure Universal Render Pipeline Asset exists
- Shader not found - reinstall URP package

### Meshes Not Appearing?
- Check if generation completed (Console log: "AAA Low Poly Assets generated successfully!")
- Verify mesh files exist in `Assets/GeneratedAssets/Meshes/`

---

**Enjoy your AAA-quality low poly game world! No more blocky placeholders!** 🎮✨
