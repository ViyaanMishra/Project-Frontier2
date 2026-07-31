Shader "Frontier/URP/Hologram/Projection"
{
    Properties
    {
        _HologramColor ("Hologram Color", Color) = (0.2, 0.8, 1, 0.5)
        _ScrollSpeed ("Scroll Speed", Range(0, 2)) = 0.5
        _ScanlineSpeed ("Scanline Speed", Range(0, 5)) = 1
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.3
        
        _GridSize ("Grid Size", Range(0.01, 1)) = 0.1
        _GridThickness ("Grid Thickness", Range(0.001, 0.1)) = 0.01
        _GridColor ("Grid Color", Color) = (0.4, 0.9, 1, 0.8)
        
        _FresnelPower ("Fresnel Power", Range(0.1, 10)) = 3
        _FresnelColor ("Fresnel Color", Color) = (0.5, 0.9, 1, 1)
        
        _NoiseMap ("Noise Map", 2D) = "white" {}
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.2
        _NoiseSpeed ("Noise Speed", Vector) = (0.3, 0.2, 0, 0)
        
        _FlickerSpeed ("Flicker Speed", Range(0, 10)) = 3
        _FlickerAmount ("Flicker Amount", Range(0, 1)) = 0.15
        
        _EdgeFade ("Edge Fade", Range(0, 1)) = 0.2
        _AlphaCutoff ("Alpha Cutoff", Range(0, 1)) = 0.05
        
        _EmissionStrength ("Emission Strength", Range(0, 5)) = 2
    }
    
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent+10" }
        LOD 400
        
        Pass
        {
            Name "HologramProjection"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            
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
                float3 tangentWS : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
                float2 uv : TEXCOORD4;
                float3 viewDir : TEXCOORD5;
                UNITY_FOG_COORDS(6)
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            TEXTURE2D(_NoiseMap); SAMPLER(sampler_NoiseMap);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _HologramColor;
                float4 _GridColor;
                float4 _FresnelColor;
                float _ScrollSpeed, _ScanlineSpeed, _ScanlineIntensity;
                float _GridSize, _GridThickness, _FresnelPower;
                float2 _NoiseSpeed;
                float _NoiseStrength, _FlickerSpeed, _FlickerAmount;
                float _EdgeFade, _AlphaCutoff, _EmissionStrength;
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
                output.viewDir = normalize(GetCameraPositionWS() - vertexInput.positionWS);
                
                UNITY_TRANSFER_FOG(output, output.positionCS);
                
                return output;
            }
            
            // Procedural grid pattern
            half CalculateGrid(float2 uv, float gridSize, float thickness)
            {
                float2 gridUV = uv / gridSize;
                float2 gridLine = abs(frac(gridUV) - 0.5);
                half gridX = smoothstep(thickness, 0, gridLine.x);
                half gridY = smoothstep(thickness, 0, gridLine.y);
                return max(gridX, gridY);
            }
            
            // Scanline effect
            half CalculateScanlines(float2 uv, float time, float speed)
            {
                half scanline = sin(uv.y * 100 + time * speed * 6.28) * 0.5 + 0.5;
                scanline = pow(scanline, 3) * 0.5;
                return scanline;
            }
            
            // Hologram flicker
            half CalculateFlicker(float time)
            {
                half flicker = sin(time * _FlickerSpeed * 6.28) * 0.5 + 0.5;
                flicker += sin(time * _FlickerSpeed * 3.7) * 0.25;
                flicker += sin(time * _FlickerSpeed * 9.1) * 0.125;
                return saturate(flicker * _FlickerAmount + (1 - _FlickerAmount));
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
                
                // Scrolling UV for noise
                float2 scrolledUV = input.uv + _NoiseSpeed * time * _ScrollSpeed;
                
                // Sample noise
                half noise = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, scrolledUV).r;
                noise = lerp(noise, 1, _NoiseStrength);
                
                // Grid pattern
                half grid = CalculateGrid(input.uv, _GridSize, _GridThickness);
                
                // Scanlines
                half scanlines = CalculateScanlines(input.uv, time, _ScanlineSpeed);
                
                // Flicker
                half flicker = CalculateFlicker(time);
                
                // Fresnel rim
                half fresnel = CalculateFresnel(viewDir, normalWS, _FresnelPower);
                
                // Base hologram color with noise modulation
                half3 baseColor = _HologramColor.rgb * noise * flicker;
                
                // Add grid
                half3 gridCol = _GridColor.rgb * grid * _GridColor.a;
                
                // Add scanlines
                half3 scanlineCol = _HologramColor.rgb * scanlines * _ScanlineIntensity;
                
                // Fresnel rim color
                half3 fresnelCol = _FresnelColor.rgb * fresnel * _FresnelColor.a;
                
                // Edge fade based on UV bounds
                half edgeFadeX = smoothstep(0, _EdgeFade, input.uv.x) * smoothstep(1, 1-_EdgeFade, input.uv.x);
                half edgeFadeY = smoothstep(0, _EdgeFade, input.uv.y) * smoothstep(1, 1-_EdgeFade, input.uv.y);
                half edgeFade = min(edgeFadeX, edgeFadeY);
                
                // Combine all components
                half3 finalColor = baseColor + gridCol + scanlineCol + fresnelCol;
                finalColor *= edgeFade;
                finalColor *= _EmissionStrength;
                
                // Alpha calculation
                half alpha = _HologramColor.a * flicker;
                alpha += grid * _GridColor.a * 0.5;
                alpha += fresnel * _FresnelColor.a * 0.3;
                alpha = saturate(alpha);
                
                // Alpha cutoff for performance
                clip(alpha - _AlphaCutoff);
                
                // Add simple lighting contribution
                Light mainLight = GetMainLight();
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                finalColor += mainLight.color * NdotL * 0.2;
                
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
