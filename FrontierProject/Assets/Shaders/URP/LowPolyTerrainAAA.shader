Shader "Frontier/URP/Environment/LowPolyTerrainAAA"
{
    Properties
    {
        [Header(Terrain Layers)]
        _TerrainLayers ("Active Layers", Range(0, 8)) = 4
        
        // Layer 0 - Grass
        _Layer0Tex ("Layer 0 (Grass)", 2D) = "white" {}
        _Layer0Normal ("Layer 0 Normal", 2D) = "bump" {}
        _Layer0Height ("Layer 0 Height", 2D) = "black" {}
        _Layer0Tiling ("Layer 0 Tiling", Vector) = (30, 30, 0, 0)
        _Layer0Color ("Layer 0 Tint", Color) = (1, 1, 1, 1)
        
        // Layer 1 - Dirt/Rock
        _Layer1Tex ("Layer 1 (Dirt)", 2D) = "white" {}
        _Layer1Normal ("Layer 1 Normal", 2D) = "bump" {}
        _Layer1Height ("Layer 1 Height", 2D) = "black" {}
        _Layer1Tiling ("Layer 1 Tiling", Vector) = (25, 25, 0, 0)
        _Layer1Color ("Layer 1 Tint", Color) = (1, 1, 1, 1)
        
        // Layer 2 - Stone
        _Layer2Tex ("Layer 2 (Stone)", 2D) = "white" {}
        _Layer2Normal ("Layer 2 Normal", 2D) = "bump" {}
        _Layer2Height ("Layer 2 Height", 2D) = "black" {}
        _Layer2Tiling ("Layer 2 Tiling", Vector) = (20, 20, 0, 0)
        _Layer2Color ("Layer 2 Tint", Color) = (1, 1, 1, 1)
        
        // Layer 3 - Snow
        _Layer3Tex ("Layer 3 (Snow)", 2D) = "white" {}
        _Layer3Normal ("Layer 3 Normal", 2D) = "bump" {}
        _Layer3Height ("Layer 3 Height", 2D) = "black" {}
        _Layer3Tiling ("Layer 3 Tiling", Vector) = (35, 35, 0, 0)
        _Layer3Color ("Layer 3 Tint", Color) = (1, 1, 1, 1)
        
        // Layer 4 - Sand
        _Layer4Tex ("Layer 4 (Sand)", 2D) = "white" {}
        _Layer4Normal ("Layer 4 Normal", 2D) = "bump" {}
        _Layer4Height ("Layer 4 Height", 2D) = "black" {}
        _Layer4Tiling ("Layer 4 Tiling", Vector) = (30, 30, 0, 0)
        _Layer4Color ("Layer 4 Tint", Color) = (1, 1, 1, 1)
        
        // Layer 5 - Mud
        _Layer5Tex ("Layer 5 (Mud)", 2D) = "white" {}
        _Layer5Normal ("Layer 5 Normal", 2D) = "bump" {}
        _Layer5Height ("Layer 5 Height", 2D) = "black" {}
        _Layer5Tiling ("Layer 5 Tiling", Vector) = (25, 25, 0, 0)
        _Layer5Color ("Layer 5 Tint", Color) = (1, 1, 1, 1)
        
        // Layer 6 - Gravel
        _Layer6Tex ("Layer 6 (Gravel)", 2D) = "white" {}
        _Layer6Normal ("Layer 6 Normal", 2D) = "bump" {}
        _Layer6Height ("Layer 6 Height", 2D) = "black" {}
        _Layer6Tiling ("Layer 6 Tiling", Vector) = (20, 20, 0, 0)
        _Layer6Color ("Layer 6 Tint", Color) = (1, 1, 1, 1)
        
        // Layer 7 - Cliff
        _Layer7Tex ("Layer 7 (Cliff)", 2D) = "white" {}
        _Layer7Normal ("Layer 7 Normal", 2D) = "bump" {}
        _Layer7Height ("Layer 7 Height", 2D) = "black" {}
        _Layer7Tiling ("Layer 7 Tiling", Vector) = (15, 15, 0, 0)
        _Layer7Color ("Layer 7 Tint", Color) = (1, 1, 1, 1)
        
        // Splatmap
        _SplatMap ("Splat Map (RGBA)", 2D) = "white" {}
        _SplatMap2 ("Splat Map 2 (RGBA)", 2D) = "black" {}
        
        // Terrain height and blending
        _TerrainHeight ("Terrain Height Map", 2D) = "white" {}
        _TerrainNormals ("Terrain Normals", 2D) = "bump" {}
        _HeightBlend ("Height Blend", Range(0.01, 0.5)) = 0.05
        _NormalStrength ("Normal Strength", Range(0, 3)) = 1.5
        _NormalScale ("Normal Scale", Float) = 1
        _GlobalTiling ("Global Tiling", Float) = 100
        
        // Triplanar settings
        _TriplanarBlend ("Triplanar Blend Sharpness", Range(0.5, 10)) = 4
        _TriplanarEnable ("Enable Triplanar", Float) = 0
        
        // Microsurface detail
        _MicroDetail ("Micro Detail Strength", Range(0, 1)) = 0.3
        _MicroDetailScale ("Micro Detail Scale", Range(0.1, 10)) = 2
        
        // Slope tinting
        _SlopeTintEnable ("Enable Slope Tinting", Float) = 1
        _SlopeSteepColor ("Steep Slope Color", Color) = (0.5, 0.45, 0.4, 1)
        _SlopeBlend ("Slope Blend Power", Range(0.1, 10)) = 3
        
        // Altitude tinting
        _AltitudeTintEnable ("Enable Altitude Tinting", Float) = 1
        _AltitudeLowColor ("Low Altitude Color", Color) = (0.4, 0.35, 0.3, 1)
        _AltitudeHighColor ("High Altitude Color", Color) = (0.9, 0.95, 1, 1)
        _AltitudeBlendStart ("Altitude Blend Start", Range(0, 1)) = 0.3
        _AltitudeBlendEnd ("Altitude Blend End", Range(0, 1)) = 0.8
        
        // Wetness
        _WetnessMap ("Wetness Map", 2D) = "black" {}
        _WetnessStrength ("Wetness Strength", Range(0, 1)) = 0
        _WetnessDarkening ("Wetness Darkening", Range(0, 0.5)) = 0.3
        _WetnessSpecular ("Wetness Specular", Range(0, 1)) = 0.6
        
        // Ambient occlusion
        _AOMap ("Ambient Occlusion Map", 2D) = "white" {}
        _AOStrength ("AO Strength", Range(0, 2)) = 1
        
        // Parallax
        _ParallaxStrength ("Parallax Strength", Range(0, 0.1)) = 0.02
        _ParallaxSamples ("Parallax Samples", Range(4, 64)) = 32
        
        // Tessellation
        _TessellationFactor ("Tessellation Factor", Range(1, 64)) = 1
        _TessellationDistance ("Tessellation Distance", Range(10, 500)) = 100
    }
    
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry-100" }
        LOD 600
        
        // Main forward pass
        Pass
        {
            Name "TerrainAAA"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            
            #pragma multi_compile _ _NORMALMAP
            #pragma multi_compile _ _PARALLAXMAP
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 tangentWS : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
                float2 uv : TEXCOORD4;
                float4 splatUV : TEXCOORD5;
                float3 viewDirTS : TEXCOORD6;
                float fogCoord : TEXCOORD7;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            // All texture declarations
            TEXTURE2D(_Layer0Tex); SAMPLER(sampler_Layer0Tex);
            TEXTURE2D(_Layer0Normal); SAMPLER(sampler_Layer0Normal);
            TEXTURE2D(_Layer0Height); SAMPLER(sampler_Layer0Height);
            TEXTURE2D(_Layer1Tex); SAMPLER(sampler_Layer1Tex);
            TEXTURE2D(_Layer1Normal); SAMPLER(sampler_Layer1Normal);
            TEXTURE2D(_Layer1Height); SAMPLER(sampler_Layer1Height);
            TEXTURE2D(_Layer2Tex); SAMPLER(sampler_Layer2Tex);
            TEXTURE2D(_Layer2Normal); SAMPLER(sampler_Layer2Normal);
            TEXTURE2D(_Layer2Height); SAMPLER(sampler_Layer2Height);
            TEXTURE2D(_Layer3Tex); SAMPLER(sampler_Layer3Tex);
            TEXTURE2D(_Layer3Normal); SAMPLER(sampler_Layer3Normal);
            TEXTURE2D(_Layer3Height); SAMPLER(sampler_Layer3Height);
            TEXTURE2D(_Layer4Tex); SAMPLER(sampler_Layer4Tex);
            TEXTURE2D(_Layer4Normal); SAMPLER(sampler_Layer4Normal);
            TEXTURE2D(_Layer4Height); SAMPLER(sampler_Layer4Height);
            TEXTURE2D(_Layer5Tex); SAMPLER(sampler_Layer5Tex);
            TEXTURE2D(_Layer5Normal); SAMPLER(sampler_Layer5Normal);
            TEXTURE2D(_Layer5Height); SAMPLER(sampler_Layer5Height);
            TEXTURE2D(_Layer6Tex); SAMPLER(sampler_Layer6Tex);
            TEXTURE2D(_Layer6Normal); SAMPLER(sampler_Layer6Normal);
            TEXTURE2D(_Layer6Height); SAMPLER(sampler_Layer6Height);
            TEXTURE2D(_Layer7Tex); SAMPLER(sampler_Layer7Tex);
            TEXTURE2D(_Layer7Normal); SAMPLER(sampler_Layer7Normal);
            TEXTURE2D(_Layer7Height); SAMPLER(sampler_Layer7Height);
            TEXTURE2D(_SplatMap); SAMPLER(sampler_SplatMap);
            TEXTURE2D(_SplatMap2); SAMPLER(sampler_SplatMap2);
            TEXTURE2D(_TerrainHeight); SAMPLER(sampler_TerrainHeight);
            TEXTURE2D(_TerrainNormals); SAMPLER(sampler_TerrainNormals);
            TEXTURE2D(_WetnessMap); SAMPLER(sampler_WetnessMap);
            TEXTURE2D(_AOMap); SAMPLER(sampler_AOMap);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _Layer0Tex_ST, _Layer1Tex_ST, _Layer2Tex_ST, _Layer3Tex_ST;
                float4 _Layer4Tex_ST, _Layer5Tex_ST, _Layer6Tex_ST, _Layer7Tex_ST;
                float4 _Layer0Tiling, _Layer1Tiling, _Layer2Tiling, _Layer3Tiling;
                float4 _Layer4Tiling, _Layer5Tiling, _Layer6Tiling, _Layer7Tiling;
                float4 _Layer0Color, _Layer1Color, _Layer2Color, _Layer3Color;
                float4 _Layer4Color, _Layer5Color, _Layer6Color, _Layer7Color;
                float4 _SlopeSteepColor, _AltitudeLowColor, _AltitudeHighColor;
                float4 _WetnessMap_ST;
                float _HeightBlend, _NormalStrength, _NormalScale, _GlobalTiling;
                float _TriplanarBlend, _TriplanarEnable;
                float _MicroDetail, _MicroDetailScale;
                float _SlopeTintEnable, _SlopeBlend;
                float _AltitudeTintEnable, _AltitudeBlendStart, _AltitudeBlendEnd;
                float _WetnessStrength, _WetnessDarkening, _WetnessSpecular;
                float _AOStrength, _ParallaxStrength, _ParallaxSamples;
                float _TessellationFactor, _TessellationDistance;
            CBUFFER_END
            
            // Triplanar sampling
            half3 SampleTriplanar(TEXTURE2D_PARAM(tex, samplerTex), float3 posWS, float3 normalWS, float4 tiling)
            {
                float3 blendWeights = pow(abs(normalWS), _TriplanarBlend);
                blendWeights /= (blendWeights.x + blendWeights.y + blendWeights.z + 0.0001);
                
                float2 uvX = posWS.yz * tiling.xy + tiling.zw;
                float2 uvY = posWS.xz * tiling.xy + tiling.zw;
                float2 uvZ = posWS.xy * tiling.xy + tiling.zw;
                
                half3 colX = SAMPLE_TEXTURE2D(tex, samplerTex, uvX).rgb;
                half3 colY = SAMPLE_TEXTURE2D(tex, samplerTex, uvY).rgb;
                half3 colZ = SAMPLE_TEXTURE2D(tex, samplerTex, uvZ).rgb;
                
                return colX * blendWeights.x + colY * blendWeights.y + colZ * blendWeights.z;
            }
            
            half3 GetLayerAlbedo(int layerIdx, float2 uv, float3 posWS, float3 normalWS)
            {
                float4 tiling;
                TEXTURE2D tex; SAMPLER samp;
                
                if (layerIdx == 0) { tex = _Layer0Tex; samp = sampler_Layer0Tex; tiling = _Layer0Tiling; }
                else if (layerIdx == 1) { tex = _Layer1Tex; samp = sampler_Layer1Tex; tiling = _Layer1Tiling; }
                else if (layerIdx == 2) { tex = _Layer2Tex; samp = sampler_Layer2Tex; tiling = _Layer2Tiling; }
                else if (layerIdx == 3) { tex = _Layer3Tex; samp = sampler_Layer3Tex; tiling = _Layer3Tiling; }
                else if (layerIdx == 4) { tex = _Layer4Tex; samp = sampler_Layer4Tex; tiling = _Layer4Tiling; }
                else if (layerIdx == 5) { tex = _Layer5Tex; samp = sampler_Layer5Tex; tiling = _Layer5Tiling; }
                else if (layerIdx == 6) { tex = _Layer6Tex; samp = sampler_Layer6Tex; tiling = _Layer6Tiling; }
                else { tex = _Layer7Tex; samp = sampler_Layer7Tex; tiling = _Layer7Tiling; }
                
                if (_TriplanarEnable > 0.5)
                    return SampleTriplanar(tex, samp, posWS, normalWS, tiling);
                else
                    return SAMPLE_TEXTURE2D(tex, samp, uv * tiling.xy + tiling.zw).rgb;
            }
            
            half3 GetLayerNormal(int layerIdx, float2 uv, float3 posWS, float3 normalWS)
            {
                float4 tiling;
                TEXTURE2D tex; SAMPLER samp;
                
                if (layerIdx == 0) { tex = _Layer0Normal; samp = sampler_Layer0Normal; tiling = _Layer0Tiling; }
                else if (layerIdx == 1) { tex = _Layer1Normal; samp = sampler_Layer1Normal; tiling = _Layer1Tiling; }
                else if (layerIdx == 2) { tex = _Layer2Normal; samp = sampler_Layer2Normal; tiling = _Layer2Tiling; }
                else if (layerIdx == 3) { tex = _Layer3Normal; samp = sampler_Layer3Normal; tiling = _Layer3Tiling; }
                else if (layerIdx == 4) { tex = _Layer4Normal; samp = sampler_Layer4Normal; tiling = _Layer4Tiling; }
                else if (layerIdx == 5) { tex = _Layer5Normal; samp = sampler_Layer5Normal; tiling = _Layer5Tiling; }
                else if (layerIdx == 6) { tex = _Layer6Normal; samp = sampler_Layer6Normal; tiling = _Layer6Tiling; }
                else { tex = _Layer7Normal; samp = sampler_Layer7Normal; tiling = _Layer7Tiling; }
                
                half3 norm;
                if (_TriplanarEnable > 0.5)
                    norm = SampleTriplanar(tex, samp, posWS, normalWS, tiling);
                else
                    norm = UnpackNormal(SAMPLE_TEXTURE2D(tex, samp, uv * tiling.xy + tiling.zw));
                
                return norm * _NormalStrength;
            }
            
            // Parallax occlusion mapping
            float2 ParallaxMapping(float2 uv, float3 viewDirTS, float heightMapSample)
            {
                float2 deltaView = viewDirTS.xy / max(viewDirTS.z, 0.0001);
                float2 deltaUV = deltaView * _ParallaxStrength;
                
                float2 currentUV = uv;
                float currentHeight = heightMapSample;
                
                [loop]
                for (int i = 0; i < (int)_ParallaxSamples; i++)
                {
                    if (currentHeight <= 0) break;
                    currentUV -= deltaUV / (float)_ParallaxSamples;
                    currentHeight -= 1.0 / (float)_ParallaxSamples;
                }
                
                return currentUV;
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = NormalizeNormalPerPixel(normalInput.normalWS);
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;
                output.uv = input.uv * _GlobalTiling;
                
                // Calculate splat UVs
                float2 worldUV = input.positionOS.xz / _GlobalTiling;
                output.splatUV = float4(worldUV, worldUV);
                
                // View direction in tangent space for parallax
                float3x3 tangentToWorld = float3x3(output.tangentWS, output.bitangentWS, output.normalWS);
                float3x3 worldToTangent = transpose(tangentToWorld);
                output.viewDirTS = mul(worldToTangent, GetCameraPositionWS() - output.positionWS);
                
                output.fogCoord = ComputeFogIntensity(vertexInput.positionCS.z);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float3 viewDirWS = normalize(GetCameraPositionWS() - input.positionWS);
                float3 normalWS = input.normalWS;
                float2 uv = input.uv;
                
                // Parallax mapping
                #ifdef _PARALLAXMAP
                float heightSample = SAMPLE_TEXTURE2D(_Layer0Height, sampler_Layer0Height, uv).r;
                uv = ParallaxMapping(uv, normalize(input.viewDirTS), heightSample);
                #endif
                
                // Sample splatmaps
                half4 splat = SAMPLE_TEXTURE2D(_SplatMap, sampler_SplatMap, input.splatUV.xy);
                half4 splat2 = SAMPLE_TEXTURE2D(_SplatMap2, sampler_SplatMap2, input.splatUV.zw);
                half terrainHeight = SAMPLE_TEXTURE2D(_TerrainHeight, sampler_TerrainHeight, input.splatUV.xy).r;
                
                // Height-based blending for 8 layers
                half layerWeights[8];
                
                // First 4 layers from splatmap
                layerWeights[0] = saturate(1.0 - terrainHeight / _HeightBlend) * (1.0 - splat.r - splat.g - splat.b);
                layerWeights[1] = splat.r;
                layerWeights[2] = splat.g;
                layerWeights[3] = splat.b;
                
                // Next 4 layers from second splatmap
                layerWeights[4] = splat2.r;
                layerWeights[5] = splat2.g;
                layerWeights[6] = splat2.b;
                layerWeights[7] = saturate((terrainHeight - 0.9) / _HeightBlend);
                
                // Normalize weights
                half totalWeight = 0;
                for (int i = 0; i < 8; i++) totalWeight += layerWeights[i];
                if (totalWeight > 0.0001)
                {
                    for (int i = 0; i < 8; i++) layerWeights[i] /= totalWeight;
                }
                
                // Sample and blend albedo
                half3 albedo = 0;
                half3 normalTangent = 0;
                
                for (int i = 0; i < 8; i++)
                {
                    if (layerWeights[i] > 0.001)
                    {
                        albedo += GetLayerAlbedo(i, uv, input.positionWS, normalWS) * layerWeights[i];
                        normalTangent += GetLayerNormal(i, uv, input.positionWS, normalWS) * layerWeights[i];
                    }
                }
                
                // Microsurface detail
                half microDetail = SAMPLE_TEXTURE2D(_Layer0Tex, sampler_Layer0Tex, uv * _MicroDetailScale).r;
                albedo *= lerp(1, microDetail, _MicroDetail);
                
                // Slope tinting
                if (_SlopeTintEnable > 0.5)
                {
                    half slope = 1.0 - dot(normalWS, float3(0, 1, 0));
                    half slopeMask = pow(slope, _SlopeBlend);
                    albedo = lerp(albedo, _SlopeSteepColor.rgb, slopeMask * 0.5);
                }
                
                // Altitude tinting
                if (_AltitudeTintEnable > 0.5)
                {
                    half altitudeMask = smoothstep(_AltitudeBlendStart, _AltitudeBlendEnd, terrainHeight);
                    half3 altitudeColor = lerp(_AltitudeLowColor.rgb, _AltitudeHighColor.rgb, altitudeMask);
                    albedo = lerp(albedo, altitudeColor, 0.3);
                }
                
                // Wetness
                half wetness = SAMPLE_TEXTURE2D(_WetnessMap, sampler_WetnessMap, input.splatUV.xy).r * _WetnessStrength;
                albedo *= lerp(1, 1 - _WetnessDarkening, wetness);
                
                // Ambient occlusion
                half ao = SAMPLE_TEXTURE2D(_AOMap, sampler_AOMap, input.splatUV.xy).r;
                ao = lerp(1, ao, _AOStrength);
                
                // Transform normal to world space
                float3x3 tangentToWorld = float3x3(input.tangentWS, input.bitangentWS, input.normalWS);
                normalWS = normalize(mul(tangentToWorld, normalize(normalTangent)));
                
                // PBR Lighting
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = viewDirWS;
                inputData.shadowAttenuation = 1;
                inputData.normalizedScreenSpaceUV = input.positionCS.xy / _ScreenSize.xy;
                
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo * ao;
                surfaceData.normalWS = normalWS;
                surfaceData.metallic = 0;
                surfaceData.smoothness = lerp(0.3, 0.6, wetness * _WetnessSpecular);
                surfaceData.occlusion = ao;
                surfaceData.alpha = 1;
                
                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                
                // Fog
                color.rgb = MixFog(color.rgb, input.fogCoord);
                
                return color;
            }
            ENDHLSL
        }
        
        // Shadow caster pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            ENDHLSL
        }
        
        // Depth prepass
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            
            ZWrite On
            ColorMask 0
            
            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }
    
    CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.TerrainShader"
    FallBack "Hidden/InternalErrorShader"
}
