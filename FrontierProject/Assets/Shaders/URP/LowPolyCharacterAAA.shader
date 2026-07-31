Shader "Frontier/URP/Character/LowPolyCharacterAAA"
{
    Properties
    {
        [Header(Main Texture)]
        _MainTex ("Albedo", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _MaskMap ("Mask Map (R=AO G=Roughness B=Metal A=Detail)", 2D) = "black" {}
        
        [Header(Color Tinting)]
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _SecondaryColor ("Secondary Color", Color) = (0.5, 0.5, 0.5, 1)
        _TertiaryColor ("Tertiary Color", Color) = (0.3, 0.3, 0.3, 1)
        _ColorBlendMask ("Color Blend Mask", 2D) = "white" {}
        
        [Header(PBR Settings)]
        _Metallic ("Metallic", Range(0, 1)) = 0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        _AOStrength ("AO Strength", Range(0, 2)) = 1
        
        [Header(Subsurface Scattering)]
        _SSSColor ("SSS Color", Color) = (1, 0.5, 0.4, 1)
        _SSSScale ("SSS Scale", Range(0, 1)) = 0.3
        _SSSPower ("SSS Power", Range(0.1, 10)) = 2
        
        [Header(Rim Lighting)]
        _RimColor ("Rim Color", Color) = (0.5, 0.8, 1, 1)
        _RimPower ("Rim Power", Range(0.1, 10)) = 3
        _RimIntensity ("Rim Intensity", Range(0, 2)) = 0.5
        
        [Header(Emission)]
        _EmissionMap ("Emission Map", 2D) = "black" {}
        _EmissionColor ("Emission Color", Color) = (1, 1, 1, 1)
        _EmissionIntensity ("Emission Intensity", Range(0, 10)) = 1
        
        [Header(Detail Mapping)]
        _DetailMap ("Detail Normal", 2D) = "bump" {}
        _DetailScale ("Detail Scale", Range(0, 10)) = 5
        _DetailStrength ("Detail Strength", Range(0, 2)) = 0.5
        
        [Header(Cloth/Fabric)]
        _ClothToggle ("Enable Cloth Shading", Float) = 0
        _FabricWeave ("Fabric Weave Pattern", 2D) = "white" {}
        _Fuzziness ("Fuzz Factor", Range(0, 1)) = 0.2
        
        [Header(Wetness/Blood)]
        _WetnessMask ("Wetness Mask", 2D) = "black" {}
        _WetnessLevel ("Wetness Level", Range(0, 1)) = 0
        _BloodMask ("Blood/Damage Mask", 2D) = "black" {}
        _BloodColor ("Blood Color", Color) = (0.6, 0.1, 0.1, 1)
        
        [Header(Fresnel Outline)]
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.02
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _Outline Fresnel ("Outline Fresnel Power", Range(0.1, 10)) = 2
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline" 
            "RenderType" = "Opaque" 
            "Queue" = "Geometry"
        }
        
        LOD 500
        
        // Main forward pass
        Pass
        {
            Name "CharacterAAA"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.0
            
            #pragma multi_compile _ _NORMALMAP
            #pragma multi_compile _ _PARALLAXMAP
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float4 color : COLOR;
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
                float4 color : TEXCOORD5;
                float fogCoord : TEXCOORD6;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
            TEXTURE2D(_MaskMap); SAMPLER(sampler_MaskMap);
            TEXTURE2D(_ColorBlendMask); SAMPLER(sampler_ColorBlendMask);
            TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap);
            TEXTURE2D(_DetailMap); SAMPLER(sampler_DetailMap);
            TEXTURE2D(_FabricWeave); SAMPLER(sampler_FabricWeave);
            TEXTURE2D(_WetnessMask); SAMPLER(sampler_WetnessMask);
            TEXTURE2D(_BloodMask); SAMPLER(sampler_BloodMask);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BaseColor, _SecondaryColor, _TertiaryColor;
                float4 _SSSColor, _RimColor, _EmissionColor, _BloodColor, _OutlineColor;
                float _Metallic, _Smoothness, _AOStrength;
                float _SSSScale, _SSSPower;
                float _RimPower, _RimIntensity;
                float _EmissionIntensity;
                float _DetailScale, _DetailStrength;
                float _ClothToggle, _Fuzziness;
                float _WetnessLevel;
                float _OutlineWidth, _OutlineFresnel;
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
                output.normalWS = NormalizeNormalPerPixel(normalInput.normalWS);
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                output.fogCoord = ComputeFogIntensity(vertexInput.positionCS.z);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float3 viewDir = normalize(GetCameraPositionWS() - input.positionWS);
                float3 normalWS = input.normalWS;
                
                // Sample main textures
                half4 albedoMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv));
                half4 maskMap = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, input.uv);
                
                // Color blending based on mask
                half4 colorMask = SAMPLE_TEXTURE2D(_ColorBlendMask, sampler_ColorBlendMask, input.uv);
                half3 finalAlbedo = lerp(_BaseColor.rgb, _SecondaryColor.rgb, colorMask.r);
                finalAlbedo = lerp(finalAlbedo, _TertiaryColor.rgb, colorMask.g);
                finalAlbedo *= albedoMap.rgb * input.color.rgb;
                
                // Detail mapping
                half3 detailNormal = UnpackNormal(SAMPLE_TEXTURE2D(_DetailMap, sampler_DetailMap, input.uv * _DetailScale));
                normalTS = lerp(normalTS, detailNormal, _DetailStrength);
                
                // Transform normal to world space
                float3x3 tangentToWorld = float3x3(input.tangentWS, input.bitangentWS, input.normalWS);
                normalWS = normalize(mul(tangentToWorld, normalTS));
                
                // PBR properties from masks
                half metallic = _Metallic * maskMap.b;
                half smoothness = _Smoothness * maskMap.g;
                half ao = lerp(1, maskMap.r, _AOStrength);
                
                // Wetness effect
                half wetness = SAMPLE_TEXTURE2D(_WetnessMask, sampler_WetnessMask, input.uv).r * _WetnessLevel;
                smoothness = lerp(smoothness, 0.9, wetness);
                finalAlbedo *= lerp(1, 0.7, wetness);
                
                // Blood/damage overlay
                half bloodMask = SAMPLE_TEXTURE2D(_BloodMask, sampler_BloodMask, input.uv).r;
                finalAlbedo = lerp(finalAlbedo, _BloodColor.rgb, bloodMask);
                
                // Cloth shading
                half clothTerm = 0;
                if (_ClothToggle > 0.5)
                {
                    half fabricPattern = SAMPLE_TEXTURE2D(_FabricWeave, sampler_FabricWeave, input.uv * 50).r;
                    clothTerm = fabricPattern * _Fuzziness;
                    finalAlbedo *= 1 - clothTerm * 0.3;
                }
                
                // Subsurface scattering approximation
                Light mainLight = GetMainLight();
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half sssTerm = pow(saturate(dot(viewDir, -mainLight.direction)), _SSSPower);
                half3 sss = _SSSColor.rgb * sssTerm * _SSSScale * NdotL;
                
                // Rim lighting
                half rimDot = 1.0 - saturate(dot(viewDir, normalWS));
                half rimTerm = pow(rimDot, _RimPower);
                half3 rim = _RimColor.rgb * rimTerm * _RimIntensity;
                
                // Emission
                half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb;
                emission *= _EmissionColor.rgb * _EmissionIntensity;
                
                // PBR Lighting calculation
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = viewDir;
                inputData.shadowAttenuation = 1;
                inputData.normalizedScreenSpaceUV = input.positionCS.xy / _ScreenSize.xy;
                
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = finalAlbedo * ao;
                surfaceData.normalWS = normalWS;
                surfaceData.metallic = metallic;
                surfaceData.smoothness = smoothness;
                surfaceData.occlusion = ao;
                surfaceData.alpha = 1;
                
                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                
                // Add special effects
                color.rgb += sss + rim + emission;
                
                // Apply fog
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
        
        // Outline pass (for stylized look)
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "UniversalForward" }
            
            Cull Front
            ZWrite Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float fogCoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth, _OutlineFresnel;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                // Expand vertices along normal for outline
                float3 expandedPos = input.positionOS.xyz + input.normalOS * _OutlineWidth;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(expandedPos);
                output.positionCS = vertexInput.positionCS;
                output.fogCoord = ComputeFogIntensity(vertexInput.positionCS.z);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                half4 color = _OutlineColor;
                color.rgb = MixFog(color.rgb, input.fogCoord);
                
                return color;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/InternalErrorShader"
}
