Shader "Frontier/URP/FX/ForceField"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.2, 0.5, 1, 0.3)
        _EdgeColor ("Edge Color", Color) = (0.4, 0.7, 1, 0.8)
        
        _NoiseMap ("Noise Map", 2D) = "white" {}
        _NoiseSpeed ("Noise Speed", Vector) = (0.2, 0.1, 0, 0)
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.5
        
        _GridSize ("Grid Size", Range(0.01, 1)) = 0.1
        _GridThickness ("Grid Thickness", Range(0.001, 0.1)) = 0.005
        _GridColor ("Grid Color", Color) = (0.5, 0.8, 1, 0.6)
        
        _FresnelPower ("Fresnel Power", Range(0.1, 10)) = 2.5
        _FresnelColor ("Fresnel Color", Color) = (0.6, 0.9, 1, 1)
        
        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 1
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.3
        
        _ImpactRadius ("Impact Radius", Float) = 0.5
        _ImpactStrength ("Impact Strength", Range(0, 2)) = 1
        _ImpactDecay ("Impact Decay", Range(0, 5)) = 2
        
        _EmissionStrength ("Emission Strength", Range(0, 5)) = 2
        _AlphaCutoff ("Alpha Cutoff", Range(0, 1)) = 0.01
    }
    
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent+5" }
        LOD 400
        
        Pass
        {
            Name "ForceField"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Front
            
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
                float3 viewDir : TEXCOORD3;
                UNITY_FOG_COORDS(4)
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            TEXTURE2D(_NoiseMap); SAMPLER(sampler_NoiseMap);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _EdgeColor;
                float4 _GridColor;
                float4 _FresnelColor;
                float2 _NoiseSpeed;
                float _NoiseStrength, _GridSize, _GridThickness;
                float _FresnelPower, _PulseSpeed, _PulseAmount;
                float _ImpactRadius, _ImpactStrength, _ImpactDecay;
                float _EmissionStrength, _AlphaCutoff;
                
                // Impact animation parameters (set via material properties)
                float _ImpactTime;
                float3 _ImpactPosition;
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
                output.viewDir = normalize(GetCameraPositionWS() - vertexInput.positionWS);
                
                UNITY_TRANSFER_FOG(output, output.positionCS);
                
                return output;
            }
            
            // Procedural hexagonal grid pattern
            half CalculateHexGrid(float2 uv, float size, float thickness)
            {
                float2 hexUV = uv / size;
                float q = floor(hexUV.x);
                float r = floor(hexUV.y);
                float s = floor(-hexUV.x - hexUV.y);
                
                float a = abs(hexUV.x - q);
                float b = abs(hexUV.y - r);
                float c = abs(hexUV.x + hexUV.y + q + r);
                
                half dist = max(max(a, b), c);
                half line = smoothstep(thickness, 0, dist);
                
                return line;
            }
            
            // Energy noise animation
            half CalculateEnergyNoise(float2 uv, float time)
            {
                float2 scrolledUV = uv + _NoiseSpeed * time;
                half noise = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, scrolledUV).r;
                noise += SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, scrolledUV * 2.3).r * 0.5;
                noise += SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, scrolledUV * 4.7).r * 0.25;
                return noise / 1.75;
            }
            
            // Pulse effect
            half CalculatePulse(float time)
            {
                half pulse = sin(time * _PulseSpeed * 6.28) * 0.5 + 0.5;
                return lerp(1, pulse, _PulseAmount);
            }
            
            // Impact ripple effect
            half CalculateImpact(float3 positionWS, float time)
            {
                if (_ImpactTime <= 0) return 0;
                
                float age = time - _ImpactTime;
                if (age < 0 || age > 1) return 0;
                
                float dist = distance(positionWS, _ImpactPosition);
                float ripplePos = age * _ImpactRadius * 2;
                float rippleWidth = 0.1 + age * 0.2;
                
                half ripple = exp(-pow((dist - ripplePos) / rippleWidth, 2));
                ripple *= (1.0 - age) * _ImpactStrength;
                
                return ripple;
            }
            
            half CalculateFresnel(half3 viewDir, half3 normalWS, half power)
            {
                half cosAngle = saturate(dot(viewDir, normalWS));
                return pow(1.0 - cosAngle, power);
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float time = _Time.y;
                half3 viewDir = normalize(input.viewDir);
                half3 normalWS = normalize(input.normalWS);
                
                // Energy noise
                half noise = CalculateEnergyNoise(input.uv, time);
                noise = lerp(noise, 1, _NoiseStrength);
                
                // Hex grid
                half grid = CalculateHexGrid(input.uv, _GridSize, _GridThickness);
                
                // Pulse
                half pulse = CalculatePulse(time);
                
                // Impact ripple
                half impact = CalculateImpact(input.positionWS, time);
                
                // Fresnel rim
                half fresnel = CalculateFresnel(viewDir, normalWS, _FresnelPower);
                
                // Base color with noise modulation
                half3 baseCol = _BaseColor.rgb * noise * pulse;
                
                // Grid overlay
                half3 gridCol = _GridColor.rgb * grid * _GridColor.a;
                
                // Fresnel rim color
                half3 fresnelCol = _FresnelColor.rgb * fresnel * _FresnelColor.a;
                
                // Edge highlight (stronger at mesh edges based on normal)
                half edgeMask = 1.0 - saturate(abs(dot(normalWS, half3(0, 1, 0))));
                half3 edgeCol = _EdgeColor.rgb * edgeMask * _EdgeColor.a * 0.5;
                
                // Impact flash
                half3 impactCol = half3(1, 0.8, 0.5) * impact;
                
                // Combine all components
                half3 finalColor = baseCol + gridCol + fresnelCol + edgeCol + impactCol;
                finalColor *= _EmissionStrength;
                
                // Alpha calculation
                half alpha = _BaseColor.a * noise;
                alpha += grid * _GridColor.a * 0.5;
                alpha += fresnel * _FresnelColor.a * 0.3;
                alpha += impact * 0.8;
                alpha = saturate(alpha);
                
                // Alpha cutoff
                clip(alpha - _AlphaCutoff);
                
                // Simple lighting contribution
                Light mainLight = GetMainLight();
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                finalColor += mainLight.color * NdotL * 0.1;
                
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
