Shader "Frontier/URP/Environment/Lava"
{
    Properties
    {
        _LavaColor ("Lava Color", Color) = (1, 0.4, 0.1, 1)
        _CoolColor ("Cool/Crust Color", Color) = (0.2, 0.1, 0.1, 1)
        _GlowColor ("Glow Color", Color) = (1, 0.6, 0.2, 1)
        
        _NoiseMap ("Flow Noise", 2D) = "white" {}
        _NoiseSpeed ("Noise Speed", Vector) = (0.1, 0.05, 0, 0)
        _NoiseScale ("Noise Scale", Float) = 2
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.8
        
        _FlowSpeed ("Flow Speed", Range(0, 2)) = 0.3
        _FlowDirection ("Flow Direction", Vector) = (1, 0, 0, 0)
        
        _CrustPattern ("Crust Pattern", 2D) = "gray" {}
        _CrustScale ("Crust Scale", Float) = 5
        _CrustThreshold ("Crust Threshold", Range(0, 1)) = 0.5
        _CrustColor ("Crust Color", Color) = (0.15, 0.1, 0.1, 1)
        
        _BubbleFrequency ("Bubble Frequency", Range(0, 5)) = 1
        _BubbleSize ("Bubble Size", Range(0.01, 0.5)) = 0.05
        _BubbleIntensity ("Bubble Intensity", Range(0, 1)) = 0.5
        
        _EmissionStrength ("Emission Strength", Range(0, 10)) = 5
        _EmissionFlicker ("Emission Flicker", Range(0, 1)) = 0.3
        
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 2)) = 0.5
        
        _HeatDistortion ("Heat Distortion", Range(0, 0.1)) = 0.02
    }
    
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }
        LOD 400
        
        Pass
        {
            Name "Lava"
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
            
            TEXTURE2D(_NoiseMap); SAMPLER(sampler_NoiseMap);
            TEXTURE2D(_CrustPattern); SAMPLER(sampler_CrustPattern);
            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _LavaColor;
                float4 _CoolColor;
                float4 _GlowColor;
                float4 _CrustColor;
                float2 _NoiseSpeed, _FlowDirection;
                float _NoiseScale, _NoiseStrength, _FlowSpeed;
                float _CrustScale, _CrustThreshold;
                float _BubbleFrequency, _BubbleSize, _BubbleIntensity;
                float _EmissionStrength, _EmissionFlicker;
                float _NormalStrength, _HeatDistortion;
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
            
            // Animated flow noise
            half CalculateFlowNoise(float2 uv, float time)
            {
                float2 scrolledUV = uv * _NoiseScale + _NoiseSpeed * time + _FlowDirection * time * _FlowSpeed;
                
                half noise = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, scrolledUV).r;
                noise += SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, scrolledUV * 2.3).r * 0.5;
                noise += SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, scrolledUV * 4.7).r * 0.25;
                
                return noise / 1.75;
            }
            
            // Crust formation pattern
            half CalculateCrust(float2 uv, float time)
            {
                float2 crustUV = uv * _CrustScale;
                half crust = SAMPLE_TEXTURE2D(_CrustPattern, sampler_CrustPattern, crustUV).r;
                
                // Animate crust edges
                crust += sin(uv.x * 20 + time) * 0.1;
                crust += cos(uv.y * 15 - time * 0.7) * 0.1;
                
                return saturate(crust);
            }
            
            // Bubble effect
            half CalculateBubbles(float2 uv, float time)
            {
                float bubbleTime = time * _BubbleFrequency;
                float2 bubbleUV = uv * (1.0 / _BubbleSize);
                
                half bubbles = sin(bubbleUV.x * 6.28 + bubbleTime) * 
                              sin(bubbleUV.y * 6.28 + bubbleTime * 0.7);
                bubbles = pow(saturate(bubbles), 3);
                
                return bubbles * _BubbleIntensity;
            }
            
            // Emission flicker
            half CalculateFlicker(float time)
            {
                half flicker = sin(time * 7.3) * 0.5 + 0.5;
                flicker += sin(time * 13.7) * 0.25;
                flicker += sin(time * 5.1) * 0.125;
                return lerp(1, flicker / 0.875, _EmissionFlicker);
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float time = _Time.y;
                half3 viewDir = normalize(GetCameraPositionWS() - input.positionWS);
                half3 normalWS = normalize(input.normalWS);
                
                // Flow noise for lava movement
                half flowNoise = CalculateFlowNoise(input.uv, time);
                flowNoise = lerp(0.5, flowNoise, _NoiseStrength);
                
                // Crust pattern
                half crust = CalculateCrust(input.uv, time);
                half crustMask = step(_CrustThreshold, crust);
                
                // Bubbles
                half bubbles = CalculateBubbles(input.uv, time);
                
                // Emission flicker
                half flicker = CalculateFlicker(time);
                
                // Normal mapping
                #ifdef _NORMALMAP
                    half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv));
                    half3x3 tangentToWorld = half3x3(input.tangentWS, input.bitangentWS, input.normalWS);
                    normalWS = normalize(mul(tangentToWorld, normalTS));
                    normalWS = normalize(lerp(input.normalWS, normalWS, _NormalStrength));
                #endif
                
                // Base lava color modulated by noise
                half3 lavaBase = lerp(_CoolColor.rgb, _LavaColor.rgb, flowNoise);
                
                // Apply crust
                half3 withCrust = lerp(lavaBase, _CrustColor.rgb, crustMask * 0.8);
                
                // Add glow in cracks (inverse of crust)
                half crackMask = 1.0 - crustMask;
                crackMask *= flowNoise;
                half3 glow = _GlowColor.rgb * crackMask * flicker;
                
                // Add bubbles
                half3 bubbleGlow = _GlowColor.rgb * bubbles * 0.5;
                
                // Combine colors
                half3 finalColor = withCrust + glow + bubbleGlow;
                
                // Strong emission
                finalColor *= _EmissionStrength;
                
                // Lighting (minimal since lava is self-illuminated)
                Light mainLight = GetMainLight();
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half3 ambientTerm = SampleSH(normalWS) * withCrust * 0.3;
                finalColor += withCrust * mainLight.color * NdotL * 0.2 + ambientTerm;
                
                UNITY_APPLY_FOG(finalColor, input.fogCoord);
                
                return half4(finalColor, 1);
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
