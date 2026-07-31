Shader "Frontier/URP/Terrain/Splatmap"
{
    Properties
    {
        _TerrainLayers ("Terrain Layers", Range(0, 4)) = 4
        
        // Layer 0 - Grass
        _Layer0Tex ("Layer 0 (Grass)", 2D) = "white" {}
        _Layer0Normal ("Layer 0 Normal", 2D) = "bump" {}
        _Layer0Tiling ("Layer 0 Tiling", Vector) = (1, 1, 0, 0)
        
        // Layer 1 - Dirt/Rock
        _Layer1Tex ("Layer 1 (Dirt)", 2D) = "white" {}
        _Layer1Normal ("Layer 1 Normal", 2D) = "bump" {}
        _Layer1Tiling ("Layer 1 Tiling", Vector) = (1, 1, 0, 0)
        
        // Layer 2 - Stone
        _Layer2Tex ("Layer 2 (Stone)", 2D) = "white" {}
        _Layer2Normal ("Layer 2 Normal", 2D) = "bump" {}
        _Layer2Tiling ("Layer 2 Tiling", Vector) = (1, 1, 0, 0)
        
        // Layer 3 - Snow
        _Layer3Tex ("Layer 3 (Snow)", 2D) = "white" {}
        _Layer3Normal ("Layer 3 Normal", 2D) = "bump" {}
        _Layer3Tiling ("Layer 3 Tiling", Vector) = (1, 1, 0, 0)
        
        // Splatmap
        _SplatMap ("Splat Map (RGBA)", 2D) = "white" {}
        
        // Terrain settings
        _TerrainHeight ("Terrain Height Map", 2D) = "white" {}
        _HeightBlend ("Height Blend", Range(0, 1)) = 0.1
        _NormalStrength ("Normal Strength", Range(0, 2)) = 1
        _GlobalTiling ("Global Tiling", Float) = 50
    }
    
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry-100" }
        LOD 300
        
        Pass
        {
            Name "TerrainSplatmap"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            
            #pragma multi_compile _ _NORMALMAP
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
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
                UNITY_FOG_COORDS(6)
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            // Textures
            TEXTURE2D(_Layer0Tex); SAMPLER(sampler_Layer0Tex);
            TEXTURE2D(_Layer0Normal); SAMPLER(sampler_Layer0Normal);
            TEXTURE2D(_Layer1Tex); SAMPLER(sampler_Layer1Tex);
            TEXTURE2D(_Layer1Normal); SAMPLER(sampler_Layer1Normal);
            TEXTURE2D(_Layer2Tex); SAMPLER(sampler_Layer2Tex);
            TEXTURE2D(_Layer2Normal); SAMPLER(sampler_Layer2Normal);
            TEXTURE2D(_Layer3Tex); SAMPLER(sampler_Layer3Tex);
            TEXTURE2D(_Layer3Normal); SAMPLER(sampler_Layer3Normal);
            TEXTURE2D(_SplatMap); SAMPLER(sampler_SplatMap);
            TEXTURE2D(_TerrainHeight); SAMPLER(sampler_TerrainHeight);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _Layer0Tex_ST, _Layer1Tex_ST, _Layer2Tex_ST, _Layer3Tex_ST;
                float4 _Layer0Tiling, _Layer1Tiling, _Layer2Tiling, _Layer3Tiling;
                float _HeightBlend, _NormalStrength, _GlobalTiling;
            CBUFFER_END
            
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
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;
                output.uv = input.uv * _GlobalTiling;
                
                // Calculate splat UVs based on world position
                float2 worldUV = input.positionOS.xz / _GlobalTiling;
                output.splatUV = float4(worldUV, worldUV);
                
                UNITY_TRANSFER_FOG(output, output.positionCS);
                
                return output;
            }
            
            half3 GetLayerColor(TEXTURE2D_PARAM(tex, samplerTex), float2 uv, float4 tiling)
            {
                return SAMPLE_TEXTURE2D(tex, samplerTex, uv * tiling.xy + tiling.zw).rgb;
            }
            
            half3 GetLayerNormal(TEXTURE2D_PARAM(normalTex, samplerNormal), float2 uv, float4 tiling)
            {
                half3 normal = UnpackNormal(SAMPLE_TEXTURE2D(normalTex, samplerNormal, uv * tiling.xy + tiling.zw));
                return normal * _NormalStrength;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                // Sample splatmap
                half4 splat = SAMPLE_TEXTURE2D(_SplatMap, sampler_SplatMap, input.uv);
                half height = SAMPLE_TEXTURE2D(_TerrainHeight, sampler_TerrainHeight, input.uv).r;
                
                // Height-based blending
                half layerWeights[4];
                layerWeights[0] = saturate(1.0 - height / _HeightBlend);
                layerWeights[1] = saturate((height - 0.3) / _HeightBlend) * saturate(1.0 - (height - 0.3) / _HeightBlend);
                layerWeights[2] = saturate((height - 0.6) / _HeightBlend) * saturate(1.0 - (height - 0.6) / _HeightBlend);
                layerWeights[3] = saturate((height - 0.85) / _HeightBlend);
                
                // Combine with splatmap weights
                layerWeights[0] *= (1.0 - splat.r - splat.g - splat.b);
                layerWeights[1] *= splat.r;
                layerWeights[2] *= splat.g;
                layerWeights[3] *= splat.b;
                
                // Normalize weights
                half totalWeight = layerWeights[0] + layerWeights[1] + layerWeights[2] + layerWeights[3];
                if (totalWeight > 0)
                {
                    layerWeights[0] /= totalWeight;
                    layerWeights[1] /= totalWeight;
                    layerWeights[2] /= totalWeight;
                    layerWeights[3] /= totalWeight;
                }
                
                // Sample layers
                half3 col0 = GetLayerColor(_Layer0Tex, input.uv, _Layer0Tiling);
                half3 col1 = GetLayerColor(_Layer1Tex, input.uv, _Layer1Tiling);
                half3 col2 = GetLayerColor(_Layer2Tex, input.uv, _Layer2Tiling);
                half3 col3 = GetLayerColor(_Layer3Tex, input.uv, _Layer3Tiling);
                
                half3 albedo = col0 * layerWeights[0] + col1 * layerWeights[1] + 
                              col2 * layerWeights[2] + col3 * layerWeights[3];
                
                // Normals
                half3 norm0 = GetLayerNormal(_Layer0Normal, input.uv, _Layer0Tiling);
                half3 norm1 = GetLayerNormal(_Layer1Normal, input.uv, _Layer1Tiling);
                half3 norm2 = GetLayerNormal(_Layer2Normal, input.uv, _Layer2Tiling);
                half3 norm3 = GetLayerNormal(_Layer3Normal, input.uv, _Layer3Tiling);
                
                half3 normalWS = normalize(norm0 * layerWeights[0] + norm1 * layerWeights[1] + 
                                          norm2 * layerWeights[2] + norm3 * layerWeights[3]);
                
                // Transform to tangent space
                half3x3 tangentToWorld = half3x3(input.tangentWS, input.bitangentWS, input.normalWS);
                half3 normalTS = mul(tangentToWorld, normalWS);
                
                // Lighting
                Light mainLight = GetMainLight();
                half3 attenuatedLighting = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
                half3 diffuseTerm = DiffuseTerm(normalTS, mainLight.direction);
                half3 color = albedo * mainLight.color * diffuseTerm * attenuatedLighting;
                
                // Ambient
                half3 ambientTerm = SampleSH(normalWS) * albedo;
                color += ambientTerm;
                
                UNITY_APPLY_FOG(color, input.fogCoord);
                
                return half4(color, 1);
            }
            ENDHLSL
        }
        
        // Shadow pass
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            ENDHLSL
        }
    }
    
    FallBack "Hidden/InternalErrorShader"
}
