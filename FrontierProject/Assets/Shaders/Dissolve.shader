Shader "Frontier/LowPoly/Dissolve"
{
    Properties
    {
        _Color ("Base Color", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _DissolveEdgeWidth ("Edge Width", Range(0, 0.5)) = 0.1
        _EdgeColor ("Edge Color", Color) = (1, 0.5, 0, 1)
        _EdgeEmission ("Edge Emission", Range(0, 2)) = 1
        _NoiseScale ("Noise Scale", Range(0.1, 10)) = 3
        _DissolveSpeed ("Dissolve Speed", Range(0, 5)) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" "RenderPipeline"="UniversalPipeline" }
        
        LOD 100
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            Blend [_SrcBlend] [_DstBlend]
            ZWrite On
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float fogFactor : TEXCOORD3;
                float4 color : COLOR;
                float noiseValue : TEXCOORD4;
            };
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half _DissolveAmount;
                half _DissolveEdgeWidth;
                half4 _EdgeColor;
                half _EdgeEmission;
                half _NoiseScale;
                half _DissolveSpeed;
                half _Cutoff;
            CBUFFER_END
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            // Pseudo-random noise function
            float hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }
            
            float noise(float2 x)
            {
                float2 i = floor(x);
                float2 f = frac(x);
                
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
                output.positionWS = vertexInput.positionWS;
                output.color = input.color * _Color;
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                
                // Calculate noise value for dissolve
                float time = _Time.y * _DissolveSpeed;
                float2 noiseUV = input.uv * _NoiseScale + time;
                output.noiseValue = noise(noiseUV);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 col = texColor * input.color;
                
                // Dissolve calculation
                float dissolveThreshold = _DissolveAmount;
                float edgeStart = dissolveThreshold - _DissolveEdgeWidth;
                float noiseVal = input.noiseValue;
                
                // Clip dissolved areas
                if (noiseVal < dissolveThreshold)
                    discard;
                
                // Edge glow for dissolving areas
                half3 finalColor = col.rgb;
                
                if (noiseVal < dissolveThreshold + _DissolveEdgeWidth && noiseVal >= edgeStart)
                {
                    float edgeFactor = 1.0 - saturate((noiseVal - edgeStart) / _DissolveEdgeWidth);
                    half3 edgeGlow = _EdgeColor.rgb * _EdgeEmission * edgeFactor;
                    finalColor = lerp(finalColor, edgeGlow, edgeFactor * 0.8);
                }
                
                // Lighting
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float3 normal = input.normalWS;
                
                float NdotL = saturate(dot(normal, lightDir));
                float3 diffuse = mainLight.color * mainLight.distanceAttenuation * NdotL;
                half3 ambient = SampleSH(input.normalWS);
                
                finalColor = finalColor * (diffuse + ambient);
                finalColor = MixFog(finalColor, input.fogFactor);
                
                return half4(finalColor, col.a);
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float noiseValue : TEXCOORD0;
            };
            
            CBUFFER_START(UnityPerMaterial)
                half _DissolveAmount;
                half _NoiseScale;
                half _DissolveSpeed;
            CBUFFER_END
            
            float hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }
            
            float noise(float2 x)
            {
                float2 i = floor(x);
                float2 f = frac(x);
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }
            
            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                float2 noiseUV = input.uv * _NoiseScale + _Time.y * _DissolveSpeed;
                output.noiseValue = noise(noiseUV);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                
                return output;
            }
            
            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                if (input.noiseValue < _DissolveAmount)
                    discard;
                return 0;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
