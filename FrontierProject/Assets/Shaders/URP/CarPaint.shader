Shader "Frontier/URP/Vehicle/CarPaint"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.2, 0.2, 0.2, 1)
        _MetallicFlakes ("Metallic Flakes Texture", 2D) = "white" {}
        _FlakeDensity ("Flake Density", Range(0, 1)) = 0.5
        _FlakeScale ("Flake Scale", Range(0.1, 5)) = 1
        _FlakeIntensity ("Flake Intensity", Range(0, 2)) = 1
        
        _ClearCoat ("Clear Coat Strength", Range(0, 1)) = 0.8
        _ClearCoatSmoothness ("Clear Coat Smoothness", Range(0, 1)) = 0.95
        
        _DirtMap ("Dirt/Grime Map", 2D) = "black" {}
        _DirtStrength ("Dirt Strength", Range(0, 1)) = 0.3
        _ScratchMap ("Scratch Map", 2D) = "black" {}
        _ScratchStrength ("Scratch Strength", Range(0, 1)) = 0.2
        
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 2)) = 1
        
        _RustMask ("Rust Mask", 2D) = "black" {}
        _RustColor ("Rust Color", Color) = (0.6, 0.3, 0.1, 1)
        _RustStrength ("Rust Strength", Range(0, 1)) = 0
        
        _EdgeWear ("Edge Wear", Range(0, 1)) = 0.1
        _AO ("Ambient Occlusion", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }
        LOD 400
        
        Pass
        {
            Name "CarPaintForward"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            
            #pragma multi_compile _ _NORMALMAP
            #pragma multi_compile _ _METALLICSPECGLOSSMAP
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/BRDF.hlsl"
            
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
                float4 screenPos : TEXCOORD5;
                UNITY_FOG_COORDS(6)
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            TEXTURE2D(_MetallicFlakes); SAMPLER(sampler_MetallicFlakes);
            TEXTURE2D(_DirtMap); SAMPLER(sampler_DirtMap);
            TEXTURE2D(_ScratchMap); SAMPLER(sampler_ScratchMap);
            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
            TEXTURE2D(_RustMask); SAMPLER(sampler_RustMask);
            TEXTURE2D(_AO); SAMPLER(sampler_AO);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _RustColor;
                float _FlakeDensity, _FlakeScale, _FlakeIntensity;
                float _ClearCoat, _ClearCoatSmoothness;
                float _DirtStrength, _ScratchStrength, _NormalStrength;
                float _RustStrength, _EdgeWear;
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
                output.uv = input.uv;
                output.screenPos = ComputeScreenPos(output.positionCS);
                
                UNITY_TRANSFER_FOG(output, output.positionCS);
                
                return output;
            }
            
            half3 CalculateMetallicFlakes(float2 uv, half3 viewDir, half3 normalWS)
            {
                half3 flakeUV = SAMPLE_TEXTURE2D(_MetallicFlakes, sampler_MetallicFlakes, uv * _FlakeScale).rgb;
                half flakeMask = dot(flakeUV, half3(0.33, 0.33, 0.34));
                
                // Animate flakes based on view angle
                half sparkle = pow(saturate(dot(viewDir, reflect(half3(0, 1, 0), normalWS))), 128);
                sparkle *= flakeMask * _FlakeDensity;
                
                return half3(1, 1, 0.9) * sparkle * _FlakeIntensity;
            }
            
            half CalculateEdgeWear(float3 positionWS, half3 normalWS)
            {
                // Simple edge detection based on normal curvature
                half edgeFactor = 1.0 - saturate(abs(normalWS.y));
                return edgeFactor * _EdgeWear;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                half3 viewDir = normalize(GetCameraPositionWS() - input.positionWS);
                half3 normalWS = normalize(input.normalWS);
                
                // Sample textures
                half3 albedo = _BaseColor.rgb;
                half smoothness = _ClearCoatSmoothness;
                half metallic = 0.9;
                
                // Metallic flakes
                half3 flakes = CalculateMetallicFlakes(input.uv, viewDir, normalWS);
                albedo += flakes;
                
                // Dirt/Grime
                half dirt = SAMPLE_TEXTURE2D(_DirtMap, sampler_DirtMap, input.uv).r;
                half3 dirtColor = lerp(albedo, half3(0.3, 0.25, 0.2), dirt * _DirtStrength);
                albedo = lerp(albedo, dirtColor, dirt * _DirtStrength);
                smoothness *= (1.0 - dirt * _DirtStrength * 0.5);
                
                // Scratches
                half scratches = SAMPLE_TEXTURE2D(_ScratchMap, sampler_ScratchMap, input.uv).r;
                smoothness *= (1.0 - scratches * _ScratchStrength * 0.3);
                
                // Rust
                half rust = SAMPLE_TEXTURE2D(_RustMask, sampler_RustMask, input.uv).r;
                albedo = lerp(albedo, _RustColor.rgb, rust * _RustStrength);
                metallic *= (1.0 - rust * _RustStrength);
                smoothness *= (1.0 - rust * _RustStrength * 0.5);
                
                // Edge wear
                half edgeWear = CalculateEdgeWear(input.positionWS, normalWS);
                albedo = lerp(albedo, half3(0.15, 0.15, 0.15), edgeWear);
                smoothness *= (1.0 - edgeWear * 0.5);
                
                // Ambient Occlusion
                half ao = SAMPLE_TEXTURE2D(_AO, sampler_AO, input.uv).r;
                
                // Normal mapping
                #ifdef _NORMALMAP
                    half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv));
                    half3x3 tangentToWorld = half3x3(input.tangentWS, input.bitangentWS, input.normalWS);
                    normalWS = normalize(mul(tangentToWorld, normalTS));
                    normalWS = normalize(lerp(input.normalWS, normalWS, _NormalStrength));
                #endif
                
                // Lighting
                Light mainLight = GetMainLight();
                half3 attenuatedLighting = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
                
                // BRDF for clear coat
                InputData inputData;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = viewDir;
                inputData.shadowAttenuation = attenuatedLighting;
                inputData.fogCoord = input.fogCoord;
                
                half3 brdfColor = albedo * mainLight.color * DiffuseTerm(normalWS, mainLight.direction);
                brdfColor *= attenuatedLighting;
                
                // Specular (clear coat)
                half3 halfDir = SafeNormalize(mainLight.direction + viewDir);
                half NdotH = saturate(dot(normalWS, halfDir));
                half specularTerm = Pow4(NdotH) * _ClearCoat;
                half3 specular = half3(1, 1, 1) * specularTerm * attenuatedLighting;
                
                half3 color = brdfColor + specular;
                color *= ao;
                
                // Add ambient
                half3 ambientTerm = SampleSH(normalWS) * albedo * 0.5;
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
