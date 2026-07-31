Shader "Frontier/URP/VFX/ParticleVolumetric"
{
    Properties
    {
        _MainTex ("Particle Texture", 2D) = "white" {}
        _NoiseMap ("3D Noise Map", 3D) = "white" {}
        
        _TintColor ("Tint Color", Color) = (1, 1, 1, 1)
        _EmissionColor ("Emission Color", Color) = (1, 0.5, 0, 1)
        
        _Softness ("Softness Factor", Range(0, 1)) = 0.5
        _InvFade ("Soft Particles Factor", Range(0, 5)) = 1
        
        _NoiseScale ("Noise Scale", Float) = 1
        _NoiseSpeed ("Noise Speed", Vector) = (0.1, 0.1, 0.1, 0)
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.5
        
        _DistortionStrength ("Distortion Strength", Range(0, 1)) = 0.2
        _DistortionSpeed ("Distortion Speed", Float) = 0.5
        
        _FresnelPower ("Fresnel Power", Range(0.1, 10)) = 2
        _FresnelColor ("Fresnel Color", Color) = (0.5, 0.8, 1, 1)
        
        _SubsurfaceScattering ("SSS Strength", Range(0, 1)) = 0.3
        _ScatterWidth ("Scatter Width", Range(0, 1)) = 0.5
        
        _ScrollSpeed ("UV Scroll Speed", Vector) = (0, 0.1, 0, 0)
        _RotationSpeed ("Rotation Speed", Range(-1, 1)) = 0
        
        _DepthFadeStart ("Depth Fade Start", Float) = 0.1
        _DepthFadeEnd ("Depth Fade End", Float) = 1
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline" 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }
        
        LOD 400
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        
        Pass
        {
            Name "ParticleVolumetric"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float4 color : TEXCOORD2;
                float depth : TEXCOORD3;
                float fogCoord : TEXCOORD4;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE3D(_NoiseMap); SAMPLER(sampler_NoiseMap);
            TEXTURE2D(_CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _TintColor;
                float4 _EmissionColor;
                float4 _FresnelColor;
                float2 _ScrollSpeed;
                float _Softness, _InvFade;
                float _NoiseScale, _NoiseStrength;
                float4 _NoiseSpeed;
                float _DistortionStrength, _DistortionSpeed;
                float _FresnelPower;
                float _SubsurfaceScattering, _ScatterWidth;
                float _RotationSpeed;
                float _DepthFadeStart, _DepthFadeEnd;
            CBUFFER_END
            
            // 3D noise sampling
            half SampleNoise3D(float3 pos)
            {
                pos = pos * _NoiseScale + _NoiseSpeed.xyz * _Time.y;
                
                half noise = SAMPLE_TEXTURE3D(_NoiseMap, sampler_NoiseMap, pos).r;
                noise += SAMPLE_TEXTURE3D(_NoiseMap, sampler_NoiseMap, pos * 2.1).r * 0.5;
                noise += SAMPLE_TEXTURE3D(_NoiseMap, sampler_NoiseMap, pos * 4.3).r * 0.25;
                
                return noise / 1.75;
            }
            
            // Soft particle depth fade
            half SoftParticles(float sceneDepth, float particleDepth)
            {
                float linearSceneDepth = LinearEyeDepth(sceneDepth, _ZBufferParams);
                float linearParticleDepth = LinearEyeDepth(particleDepth, _ZBufferParams);
                return saturate(_InvFade * (linearSceneDepth - linearParticleDepth));
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                // Rotate UVs
                float angle = _Time.y * _RotationSpeed * 6.28318;
                float s = sin(angle);
                float c = cos(angle);
                float2 centeredUV = input.uv - 0.5;
                float2 rotatedUV = float2(centeredUV.x * c - centeredUV.y * s, centeredUV.x * s + centeredUV.y * c) + 0.5;
                
                // Scroll UVs
                float2 scrolledUV = rotatedUV * _MainTex_ST.xy + _MainTex_ST.zw + _ScrollSpeed * _Time.y;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.uv = scrolledUV;
                output.color = input.color;
                output.depth = vertexInput.positionCS.z;
                output.fogCoord = ComputeFogIntensity(vertexInput.positionCS.z);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float3 viewDir = normalize(GetCameraPositionWS() - input.positionWS);
                float3 normalWS = float3(0, 0, 1); // Billboard particles
                
                // Sample main texture
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                // Apply 3D noise for volumetric appearance
                half noise = SampleNoise3D(input.positionWS);
                noise = lerp(0.5, noise, _NoiseStrength);
                texColor *= noise;
                
                // Distortion effect
                float2 distortionUV = input.uv + float2(_Time.y * _DistortionSpeed, 0);
                half distortion = SampleNoise3D(input.positionWS * 0.5) * _DistortionStrength;
                texColor *= 1 + distortion;
                
                // Fresnel rim lighting
                half fresnel = pow(1.0 - saturate(dot(viewDir, normalWS)), _FresnelPower);
                half3 fresnelTerm = _FresnelColor.rgb * fresnel * _FresnelColor.a;
                
                // Subsurface scattering approximation
                half sss = pow(saturate(dot(viewDir, -GetMainLight().direction)), 1.0 / _ScatterWidth);
                half3 sssTerm = _TintColor.rgb * sss * _SubsurfaceScattering;
                
                // Emission
                half3 emission = _EmissionColor.rgb * _EmissionColor.a * texColor.r;
                
                // Combine colors
                half3 finalColor = texColor.rgb * _TintColor.rgb * input.color.rgb;
                finalColor += fresnelTerm + sssTerm + emission;
                
                // Depth-based fade
                #if defined(_DEPTH_TEXTURE)
                float sceneDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, input.positionCS.xy / input.positionCS.w);
                half depthFade = SoftParticles(sceneDepth, input.depth);
                depthFade = smoothstep(_DepthFadeStart, _DepthFadeEnd, depthFade);
                finalColor *= depthFade;
                #endif
                
                // Apply softness
                half alpha = texColor.a * input.color.a * _TintColor.a;
                alpha *= smoothstep(0, _Softness, texColor.r);
                
                // Fog
                finalColor = MixFog(finalColor, input.fogCoord);
                
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/InternalErrorShader"
}
