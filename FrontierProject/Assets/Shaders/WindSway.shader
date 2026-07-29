Shader "Frontier/LowPoly/WindSway"
{
    Properties
    {
        _Color ("Base Color", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _WindStrength ("Wind Strength", Range(0, 1)) = 0.5
        _WindSpeed ("Wind Speed", Range(0, 2)) = 1.0
        _SwayFrequency ("Sway Frequency", Range(0.1, 5)) = 1.0
        _VertexOffset ("Vertex Offset", Range(0, 1)) = 0.3
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
        [Toggle] _AlphaClip ("Alpha Clip", Float) = 0
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }
        
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
            #pragma multi_compile _ _ALPHATEST_ON
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
            };
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half _WindStrength;
                half _WindSpeed;
                half _SwayFrequency;
                half _VertexOffset;
                half _Cutoff;
            CBUFFER_END
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            // Simplex noise for wind animation
            float3 simplexNoiseDeriv(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                float2 uv = (i.xy + f.xy) + i.z;
                return f;
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                float3 pos = input.positionOS.xyz;
                float height = pos.y;
                float offset = height * _VertexOffset;
                
                // Wind sway animation
                float time = _Time.y * _WindSpeed;
                float wind = sin(time * _SwayFrequency + pos.x * 0.5) * cos(time * 0.7 + pos.z * 0.3);
                wind *= _WindStrength * offset;
                
                pos.x += wind;
                pos.z += wind * 0.5;
                
                input.positionOS.xyz = pos;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
                output.positionWS = vertexInput.positionWS;
                output.color = input.color * _Color;
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 col = texColor * input.color;
                
                #ifdef _ALPHATEST_ON
                    clip(col.a - _Cutoff);
                #endif
                
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float3 normal = input.normalWS;
                
                // Flat shading with slight normal variation
                float NdotL = saturate(dot(normal, lightDir));
                float3 diffuse = mainLight.color * mainLight.distanceAttenuation * NdotL;
                
                // Ambient term
                half3 ambient = SampleSH(input.normalWS);
                
                half3 finalColor = col.rgb * (diffuse + ambient);
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
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };
            
            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                float3 positionOS = input.positionOS.xyz;
                
                // Apply minimal wind for shadows
                float time = _Time.y * _WindSpeed;
                float wind = sin(time * _SwayFrequency) * _WindStrength * 0.1;
                positionOS.x += wind * positionOS.y;
                
                input.positionOS.xyz = positionOS;
                output.positionCS = TransformObjectToHClip(positionOS);
                return output;
            }
            
            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
