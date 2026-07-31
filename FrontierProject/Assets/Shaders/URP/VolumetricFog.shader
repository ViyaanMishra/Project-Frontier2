Shader "Frontier/URP/Environment/VolumetricFog"
{
    Properties
    {
        _FogColor ("Fog Color", Color) = (0.6, 0.7, 0.8, 0.5)
        _FogDensity ("Fog Density", Range(0, 0.5)) = 0.02
        
        _NoiseMap3D ("3D Noise Map", 3D) = "white" {}
        _NoiseScale ("Noise Scale", Vector) = (1, 1, 1, 1)
        _NoiseSpeed ("Noise Speed", Vector) = (0.1, 0.05, 0.02, 0)
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.5
        
        _HeightFalloff ("Height Falloff", Range(0, 1)) = 0.1
        _HeightOffset ("Height Offset", Float) = 0
        
        _LightScattering ("Light Scattering", Range(0, 1)) = 0.5
        _ScatteringAnisotropy ("Scattering Anisotropy", Range(-0.9, 0.9)) = 0.6
        
        _SunColor ("Sun Color", Color) = (1, 0.95, 0.8, 1)
        _SunDirection ("Sun Direction", Vector) = (0, -1, 0.5, 0)
        
        _DepthFadeStart ("Depth Fade Start", Float) = 10
        _DepthFadeEnd ("Depth Fade End", Float) = 100
        
        _EmissionStrength ("Emission Strength", Range(0, 2)) = 0.5
    }
    
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent-10" }
        LOD 300
        
        Pass
        {
            Name "VolumetricFog"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
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
                float3 positionWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float depth : TEXCOORD2;
                UNITY_FOG_COORDS(3)
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            TEXTURE3D(_NoiseMap3D); SAMPLER(sampler_NoiseMap3D);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _FogColor;
                float4 _SunColor;
                float4 _SunDirection;
                float4 _NoiseScale, _NoiseSpeed;
                float _FogDensity, _NoiseStrength;
                float _HeightFalloff, _HeightOffset;
                float _LightScattering, _ScatteringAnisotropy;
                float _DepthFadeStart, _DepthFadeEnd, _EmissionStrength;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.uv = input.uv;
                output.depth = vertexInput.positionCS.z;
                
                UNITY_TRANSFER_FOG(output, output.positionCS);
                
                return output;
            }
            
            // 3D Noise sampling with trilinear interpolation approximation
            half SampleNoise3D(float3 pos)
            {
                pos = pos * _NoiseScale.xyz + _NoiseSpeed.xyz * _Time.y;
                
                // Sample at integer coordinates and interpolate
                float3 basePos = floor(pos);
                float3 fracPos = frac(pos);
                
                half noise = SAMPLE_TEXTURE3D(_NoiseMap3D, sampler_NoiseMap3D, pos).r;
                
                // Add layered detail
                noise += SAMPLE_TEXTURE3D(_NoiseMap3D, sampler_NoiseMap3D, pos * 2.1).r * 0.5;
                noise += SAMPLE_TEXTURE3D(_NoiseMap3D, sampler_NoiseMap3D, pos * 4.3).r * 0.25;
                
                return noise / 1.75;
            }
            
            // Henyey-Greenstein phase function for light scattering
            half CalculateHGPhase(half cosAngle, half g)
            {
                half numerator = 1.0 - g * g;
                half denominator = pow(1.0 + g * g - 2.0 * g * cosAngle, 1.5);
                return (3.0 / (8.0 * 3.14159)) * (numerator / denominator);
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                half3 viewDir = normalize(GetCameraPositionWS() - input.positionWS);
                
                // Height-based density falloff
                half heightFactor = exp(-(input.positionWS.y + _HeightOffset) * _HeightFalloff);
                heightFactor = saturate(heightFactor);
                
                // 3D noise for volumetric variation
                half noise = SampleNoise3D(input.positionWS);
                noise = lerp(0.5, noise, _NoiseStrength);
                
                // Base fog density
                half density = _FogDensity * heightFactor * noise;
                
                // Distance-based accumulation
                half distFromCamera = length(input.positionWS - GetCameraPositionWS());
                half accumulatedDensity = 1.0 - exp(-density * distFromCamera);
                accumulatedDensity = saturate(accumulatedDensity);
                
                // Light scattering
                half3 sunDir = normalize(_SunDirection.xyz);
                half cosAngle = dot(viewDir, sunDir);
                half phaseFunction = CalculateHGPhase(cosAngle, _ScatteringAnisotropy);
                half3 scatteredLight = _SunColor.rgb * phaseFunction * _LightScattering;
                
                // Combine fog color with scattered light
                half3 fogWithScattering = _FogColor.rgb + scatteredLight;
                half3 finalColor = fogWithScattering * accumulatedDensity;
                
                // Depth fade for distant fog
                half depthFade = smoothstep(_DepthFadeStart, _DepthFadeEnd, distFromCamera);
                finalColor *= depthFade;
                
                // Emission
                finalColor *= _EmissionStrength;
                
                // Alpha based on accumulated density
                half alpha = accumulatedDensity * _FogColor.a;
                
                UNITY_APPLY_FOG_COLOR(finalColor, input.fogCoord, half4(0, 0, 0, 0));
                
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
