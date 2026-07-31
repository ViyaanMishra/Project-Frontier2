Shader "Frontier/URP/Weapon/EnergyBlade"
{
    Properties
    {
        _CoreColor ("Core Color", Color) = (0.2, 0.8, 1, 1)
        _EdgeColor ("Edge Color", Color) = (0.8, 0.95, 1, 1)
        _GlowColor ("Glow Color", Color) = (0.5, 0.9, 1, 1)
        
        _CoreIntensity ("Core Intensity", Range(0, 2)) = 1.5
        _EdgeWidth ("Edge Width", Range(0, 0.5)) = 0.05
        _GlowIntensity ("Glow Intensity", Range(0, 3)) = 2
        
        _NoiseMap ("Energy Noise", 2D) = "white" {}
        _NoiseSpeed ("Noise Speed", Vector) = (0.5, 0.3, 0, 0)
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.5
        
        _FlickerSpeed ("Flicker Speed", Range(0, 10)) = 2
        _FlickerAmount ("Flicker Amount", Range(0, 1)) = 0.3
        
        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 1
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.2
        
        _DamageGlow ("Damage Glow", Range(0, 1)) = 0
        _DamageColor ("Damage Color", Color) = (1, 0.3, 0.1, 1)
        
        _EmissionBoost ("Emission Boost", Range(0, 5)) = 2
    }
    
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent+1" }
        LOD 400
        
        Pass
        {
            Name "EnergyBlade"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend One One
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
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float depth : TEXCOORD3;
                UNITY_FOG_COORDS(4)
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            TEXTURE2D(_NoiseMap); SAMPLER(sampler_NoiseMap);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _CoreColor;
                float4 _EdgeColor;
                float4 _GlowColor;
                float4 _DamageColor;
                float _CoreIntensity, _EdgeWidth, _GlowIntensity;
                float2 _NoiseSpeed;
                float _NoiseStrength, _FlickerSpeed, _FlickerAmount;
                float _PulseSpeed, _PulseAmount, _DamageGlow, _EmissionBoost;
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
                output.uv = input.uv;
                output.depth = vertexInput.positionCS.z;
                
                UNITY_TRANSFER_FOG(output, output.positionCS);
                
                return output;
            }
            
            // Perlin-like noise for energy effect
            half CalculateEnergyNoise(float2 uv, float time)
            {
                float2 scrolledUV = uv + _NoiseSpeed * time;
                half noise = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, scrolledUV).r;
                noise += SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, scrolledUV * 2.1).r * 0.5;
                noise += SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, scrolledUV * 4.3).r * 0.25;
                return noise / 1.75;
            }
            
            half CalculateFlicker(float time)
            {
                half flicker = sin(time * _FlickerSpeed * 6.28) * 0.5 + 0.5;
                flicker += sin(time * _FlickerSpeed * 2.3) * 0.25;
                flicker += sin(time * _FlickerSpeed * 5.7) * 0.125;
                return saturate(flicker * _FlickerAmount + (1 - _FlickerAmount));
            }
            
            half CalculatePulse(float time)
            {
                return sin(time * _PulseSpeed * 6.28) * 0.5 * _PulseAmount + 1;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float time = _Time.y;
                half3 viewDir = normalize(GetCameraPositionWS() - input.positionWS);
                half3 normalWS = normalize(input.normalWS);
                
                // Distance from center (assumes blade geometry)
                half distFromCenter = abs(input.uv.x - 0.5) * 2;
                
                // Energy noise animation
                half noise = CalculateEnergyNoise(input.uv, time);
                noise = lerp(noise, 1, _NoiseStrength);
                
                // Flicker and pulse
                half flicker = CalculateFlicker(time);
                half pulse = CalculatePulse(time);
                
                // Core (brightest center)
                half coreMask = 1.0 - smoothstep(0, 0.3, distFromCenter);
                coreMask *= noise;
                half3 core = _CoreColor.rgb * coreMask * _CoreIntensity * flicker * pulse;
                
                // Edge glow (fades toward edges)
                half edgeMask = smoothstep(0, _EdgeWidth, distFromCenter);
                edgeMask *= 1.0 - smoothstep(0.8, 1, distFromCenter);
                edgeMask *= noise * 0.8 + 0.2;
                half3 edge = _EdgeColor.rgb * edgeMask * _GlowIntensity * flicker;
                
                // Outer glow
                half glowMask = 1.0 - smoothstep(0, 1, distFromCenter);
                glowMask = pow(glowMask, 0.5);
                half3 glow = _GlowColor.rgb * glowMask * _GlowIntensity * 0.5;
                
                // Damage hotspots
                half3 damage = half3(0, 0, 0);
                if (_DamageGlow > 0)
                {
                    half damageNoise = CalculateEnergyNoise(input.uv * 3, time * 2);
                    half damageMask = step(0.7, damageNoise) * _DamageGlow;
                    damage = _DamageColor.rgb * damageMask * 2;
                }
                
                // Combine all layers
                half3 finalColor = core + edge + glow + damage;
                finalColor *= _EmissionBoost;
                
                // Alpha based on intensity
                half alpha = saturate(dot(finalColor, half3(0.33, 0.33, 0.34)));
                alpha = saturate(alpha + _GlowIntensity * 0.2);
                
                // Fresnel-like rim for extra pop
                half rim = pow(1.0 - saturate(dot(viewDir, normalWS)), 2);
                finalColor += _EdgeColor.rgb * rim * 0.3;
                
                UNITY_APPLY_FOG_COLOR(finalColor, input.fogCoord, half4(0, 0, 0, 0));
                
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
        
        // Opaque pass for depth
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
