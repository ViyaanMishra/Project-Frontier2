Shader "Frontier/URP/Environment/Water/Ocean"
{
    Properties
    {
        _WaterColor ("Water Color", Color) = (0.1, 0.4, 0.6, 0.9)
        _DeepColor ("Deep Water Color", Color) = (0.05, 0.15, 0.3, 1)
        _FoamColor ("Foam Color", Color) = (0.8, 0.9, 1, 1)
        
        _NormalMap1 ("Normal Map 1", 2D) = "bump" {}
        _NormalMap2 ("Normal Map 2", 2D) = "bump" {}
        _NormalSpeed1 ("Normal Speed 1", Vector) = (0.02, 0.02, 0, 0)
        _NormalSpeed2 ("Normal Speed 2", Vector) = (-0.03, -0.01, 0, 0)
        _NormalStrength ("Normal Strength", Range(0, 2)) = 0.5
        
        _WaveHeight ("Wave Height", Range(0, 2)) = 0.5
        _WaveFrequency ("Wave Frequency", Range(0, 10)) = 1
        _WaveSpeed ("Wave Speed", Range(0, 5)) = 0.5
        
        _FoamThreshold ("Foam Threshold", Range(0, 1)) = 0.7
        _FoamScale ("Foam Scale", Range(0, 10)) = 5
        _FoamNoise ("Foam Noise", 2D) = "white" {}
        
        _Refraction ("Refraction Strength", Range(0, 0.5)) = 0.05
        _FresnelPower ("Fresnel Power", Range(0.1, 10)) = 3
        _FresnelColor ("Fresnel Color", Color) = (0.5, 0.8, 1, 0.8)
        
        _SpecularIntensity ("Specular Intensity", Range(0, 2)) = 1.5
        _Smoothness ("Smoothness", Range(0, 1)) = 0.85
        
        _DepthFade ("Depth Fade Start", Float) = 5
        _MaxDepth ("Max Depth", Float) = 20
        
        _CausticsStrength ("Caustics Strength", Range(0, 1)) = 0.3
        _CausticsSpeed ("Caustics Speed", Range(0, 2)) = 0.5
    }
    
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 500
        
        Pass
        {
            Name "OceanWater"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
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
                float depth : TEXCOORD6;
                UNITY_FOG_COORDS(7)
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            TEXTURE2D(_NormalMap1); SAMPLER(sampler_NormalMap1);
            TEXTURE2D(_NormalMap2); SAMPLER(sampler_NormalMap2);
            TEXTURE2D(_FoamNoise); SAMPLER(sampler_FoamNoise);
            TEXTURE2D(_CameraColorTexture); SAMPLER(sampler_CameraColorTexture);
            TEXTURE2D(_CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _WaterColor;
                float4 _DeepColor;
                float4 _FoamColor;
                float4 _FresnelColor;
                float2 _NormalSpeed1, _NormalSpeed2;
                float _NormalStrength, _WaveHeight, _WaveFrequency, _WaveSpeed;
                float _FoamThreshold, _FoamScale, _Refraction, _FresnelPower;
                float _SpecularIntensity, _Smoothness;
                float _DepthFade, _MaxDepth, _CausticsStrength, _CausticsSpeed;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                // Vertex displacement for waves
                float time = _Time.y * _WaveSpeed;
                float wave = sin(input.positionOS.x * _WaveFrequency + time) * 
                            cos(input.positionOS.z * _WaveFrequency * 0.7 + time * 0.8);
                wave += sin(input.positionOS.z * _WaveFrequency * 0.5 - time * 0.6) * 0.5;
                input.positionOS.y += wave * _WaveHeight;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;
                output.uv = input.uv;
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.depth = vertexInput.positionCS.z;
                
                UNITY_TRANSFER_FOG(output, output.positionCS);
                
                return output;
            }
            
            half CalculateFoam(float2 uv, float time)
            {
                float2 foamUV = uv * _FoamScale;
                half foam = SAMPLE_TEXTURE2D(_FoamNoise, sampler_FoamNoise, foamUV + float2(time * 0.1, 0)).r;
                foam += SAMPLE_TEXTURE2D(_FoamNoise, sampler_FoamNoise, foamUV * 1.5 - float2(time * 0.15, 0)).r * 0.5;
                return foam / 1.5;
            }
            
            half CalculateCaustics(float2 uv, float time)
            {
                float2 causticUV = uv * 10;
                half caustic = sin(causticUV.x * 3.14 + time * _CausticsSpeed) * 
                              sin(causticUV.y * 3.14 + time * _CausticsSpeed * 0.7);
                return saturate(caustic) * _CausticsStrength;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float time = _Time.y * _WaveSpeed;
                half3 viewDir = normalize(GetCameraPositionWS() - input.positionWS);
                half3 normalWS = normalize(input.normalWS);
                
                // Sample and combine two normal maps
                half2 scroll1 = _NormalSpeed1 * time * 10;
                half2 scroll2 = _NormalSpeed2 * time * 10;
                
                half3 normal1 = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap1, sampler_NormalMap1, input.uv + scroll1));
                half3 normal2 = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap2, sampler_NormalMap2, input.uv + scroll2));
                half3 combinedNormal = normalize(normal1 + normal2);
                
                half3x3 tangentToWorld = half3x3(input.tangentWS, input.bitangentWS, input.normalWS);
                normalWS = normalize(mul(tangentToWorld, combinedNormal));
                normalWS = normalize(lerp(input.normalWS, normalWS, _NormalStrength));
                
                // Depth-based color
                float sceneDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, input.screenPos.xy / input.screenPos.w);
                float linearDepth = LinearEyeDepth(sceneDepth, _ZBufferParams);
                float waterDepth = max(0, linearDepth - input.depth);
                half depthFactor = saturate(waterDepth / _MaxDepth);
                
                // Water color blending based on depth
                half3 waterColor = lerp(_WaterColor.rgb, _DeepColor.rgb, depthFactor);
                
                // Foam at wave peaks and edges
                half foam = CalculateFoam(input.uv, time);
                half foamMask = step(_FoamThreshold, foam) * (1.0 - depthFactor);
                waterColor = lerp(waterColor, _FoamColor.rgb, foamMask * _FoamColor.a);
                
                // Fresnel
                half fresnel = pow(1.0 - saturate(dot(viewDir, normalWS)), _FresnelPower);
                half3 fresnelColor = _FresnelColor.rgb * fresnel * _FresnelColor.a;
                
                // Specular highlights
                Light mainLight = GetMainLight();
                half3 halfDir = SafeNormalize(mainLight.direction + viewDir);
                half NdotH = saturate(dot(normalWS, halfDir));
                half specularTerm = pow(NdotH, 128 * _Smoothness) * _SpecularIntensity;
                half3 specular = half3(1, 1, 1) * specularTerm * mainLight.distanceAttenuation;
                
                // Caustics
                half caustics = CalculateCaustics(input.uv, time);
                waterColor += caustics * half3(0.5, 0.7, 0.8);
                
                // Refraction (screen offset)
                half2 refractionOffset = normalWS.xy * _Refraction;
                half3 refractedColor = SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, 
                                                        input.screenPos.xy / input.screenPos.w + refractionOffset).rgb;
                
                // Combine water with refracted background
                half alpha = _WaterColor.a * (1.0 - foamMask * 0.5);
                half3 finalColor = lerp(refractedColor, waterColor, alpha);
                finalColor += fresnelColor + specular;
                
                // Depth fade for shoreline
                half depthFade = smoothstep(0, _DepthFade, waterDepth);
                finalColor *= depthFade;
                
                UNITY_APPLY_FOG(finalColor, input.fogCoord);
                
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
        
        // Depth pass
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
    
    FallBack "Hidden/InternalErrorShader"
}
