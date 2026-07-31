Shader "Frontier/URP/Character/Skin"
{
    Properties
    {
        _SkinColor ("Skin Color", Color) = (0.8, 0.6, 0.5, 1)
        _DiffuseMap ("Diffuse Map", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 2)) = 0.5
        
        _SubsurfaceColor ("Subsurface Color", Color) = (0.9, 0.5, 0.4, 1)
        _SubsurfaceStrength ("Subsurface Strength", Range(0, 1)) = 0.7
        _SubsurfaceScale ("Subsurface Scale", Range(0, 5)) = 1.5
        
        _RoughnessMap ("Roughness Map", 2D) = "white" {}
        _Roughness ("Roughness", Range(0, 1)) = 0.5
        
        _SpecularTint ("Specular Tint", Color) = (0.8, 0.6, 0.5, 1)
        _SpecularStrength ("Specular Strength", Range(0, 2)) = 0.5
        
        _DetailMap ("Detail Map (Pores)", 2D) = "gray" {}
        _DetailScale ("Detail Scale", Float) = 50
        _DetailStrength ("Detail Strength", Range(0, 1)) = 0.3
        
        _BlushColor ("Blush Color", Color) = (0.9, 0.4, 0.4, 0.3)
        _BlushMask ("Blush Mask", 2D) = "black" {}
        _BlushStrength ("Blush Strength", Range(0, 1)) = 0.3
        
        _FrecklesMask ("Freckles Mask", 2D) = "black" {}
        _FrecklesColor ("Freckles Color", Color) = (0.5, 0.35, 0.25, 1)
        _FrecklesStrength ("Freckles Strength", Range(0, 1)) = 0.2
        
        _AO ("Ambient Occlusion", 2D) = "white" {}
        
        _RimLightColor ("Rim Light Color", Color) = (1, 0.8, 0.7, 0.5)
        _RimPower ("Rim Power", Range(0.1, 5)) = 2
    }
    
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }
        LOD 400
        
        Pass
        {
            Name "CharacterSkin"
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
                UNITY_FOG_COORDS(5)
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            TEXTURE2D(_DiffuseMap); SAMPLER(sampler_DiffuseMap);
            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
            TEXTURE2D(_RoughnessMap); SAMPLER(sampler_RoughnessMap);
            TEXTURE2D(_DetailMap); SAMPLER(sampler_DetailMap);
            TEXTURE2D(_BlushMask); SAMPLER(sampler_BlushMask);
            TEXTURE2D(_FrecklesMask); SAMPLER(sampler_FrecklesMask);
            TEXTURE2D(_AO); SAMPLER(sampler_AO);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _SkinColor;
                float4 _SubsurfaceColor;
                float4 _SpecularTint;
                float4 _BlushColor;
                float4 _FrecklesColor;
                float4 _RimLightColor;
                float _NormalStrength, _SubsurfaceStrength, _SubsurfaceScale;
                float _Roughness, _SpecularStrength, _DetailScale, _DetailStrength;
                float _BlushStrength, _FrecklesStrength, _RimPower;
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
                
                UNITY_TRANSFER_FOG(output, output.positionCS);
                
                return output;
            }
            
            // Subsurface scattering approximation (cheap screen-space style)
            half3 CalculateSubsurface(half3 albedo, half3 lightColor, half NdotL, half3 viewDir, half3 normalWS)
            {
                // Simple wrap lighting for SSS effect
                half wrappedNdotL = NdotL * 0.5 + 0.5;
                half sssTerm = pow(saturate(dot(viewDir, -normalize(GetMainLight().direction))), 2);
                sssTerm *= _SubsurfaceStrength;
                
                half3 sssColor = _SubsurfaceColor.rgb * albedo * sssTerm;
                return sssColor * wrappedNdotL;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                half3 viewDir = normalize(GetCameraPositionWS() - input.positionWS);
                half3 normalWS = normalize(input.normalWS);
                
                // Sample diffuse/albedo
                half3 albedo = SAMPLE_TEXTURE2D(_DiffuseMap, sampler_DiffuseMap, input.uv).rgb * _SkinColor.rgb;
                
                // Normal mapping
                #ifdef _NORMALMAP
                    half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv));
                    half3x3 tangentToWorld = half3x3(input.tangentWS, input.bitangentWS, input.normalWS);
                    normalWS = normalize(mul(tangentToWorld, normalTS));
                    normalWS = normalize(lerp(input.normalWS, normalWS, _NormalStrength));
                #endif
                
                // Detail map for pores/fine skin detail
                half detail = SAMPLE_TEXTURE2D(_DetailMap, sampler_DetailMap, input.uv * _DetailScale).r;
                half detailModulate = lerp(1, detail, _DetailStrength);
                albedo *= detailModulate;
                
                // Roughness
                half roughness = SAMPLE_TEXTURE2D(_RoughnessMap, sampler_RoughnessMap, input.uv).r * _Roughness;
                
                // Freckles
                half freckles = SAMPLE_TEXTURE2D(_FrecklesMask, sampler_FrecklesMask, input.uv).r;
                half3 freckleColor = lerp(albedo, _FrecklesColor.rgb, freckles * _FrecklesStrength);
                albedo = lerp(albedo, freckleColor, freckles * _FrecklesStrength);
                
                // Blush
                half blush = SAMPLE_TEXTURE2D(_BlushMask, sampler_BlushMask, input.uv).r;
                half3 blushedAlbedo = lerp(albedo, albedo * _BlushColor.rgb + _BlushColor.rgb * 0.2, blush * _BlushStrength);
                albedo = lerp(albedo, blushedAlbedo, blush * _BlushStrength);
                
                // Ambient Occlusion
                half ao = SAMPLE_TEXTURE2D(_AO, sampler_AO, input.uv).r;
                
                // Lighting
                Light mainLight = GetMainLight();
                half3 attenuatedLighting = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
                
                // Diffuse with soft skin shading
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half softNdotL = NdotL * 0.7 + 0.3; // Softer transition for skin
                half3 diffuseColor = albedo * mainLight.color * softNdotL * attenuatedLighting;
                
                // Specular (skin has layered specular)
                half3 halfDir = SafeNormalize(mainLight.direction + viewDir);
                half NdotH = saturate(dot(normalWS, halfDir));
                
                // Primary specular (oily layer)
                half specPrimary = pow(NdotH, (1.0 - roughness) * 256) * _SpecularStrength;
                half3 specColor = _SpecularTint.rgb * specPrimary * attenuatedLighting;
                
                // Secondary specular (deeper layer - broader)
                half specSecondary = pow(NdotH, (1.0 - roughness) * 64) * _SpecularStrength * 0.5;
                specColor += _SpecularTint.rgb * specSecondary * attenuatedLighting * 0.5;
                
                // Subsurface scattering
                half3 subsurface = CalculateSubsurface(albedo, mainLight.color, NdotL, viewDir, normalWS);
                subsurface *= attenuatedLighting;
                
                // Rim lighting
                half rimDot = 1.0 - saturate(dot(viewDir, normalWS));
                half rim = pow(rimDot, _RimPower) * _RimLightColor.a;
                half3 rimColor = _RimLightColor.rgb * rim;
                
                // Combine all components
                half3 color = diffuseColor + specColor + subsurface + rimColor;
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
