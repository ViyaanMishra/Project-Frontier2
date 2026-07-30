Shader "Frontier/Advanced/LowPolyWater"
{
    Properties
    {
        _Color ("Water Color", Color) = (0.0, 0.4, 0.6, 0.8)
        _DeepColor ("Deep Water Color", Color) = (0.0, 0.1, 0.3, 1.0)
        _DepthFade ("Depth Fade", Range(0, 10)) = 3.0
        _WaveSpeed ("Wave Speed", Vector) = (0.5, 0.5, 0.5, 0.5)
        _WaveHeight ("Wave Height", Range(0, 2)) = 0.2
        _FoamColor ("Foam Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _FoamThreshold ("Foam Threshold", Range(0, 1)) = 0.5
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 2)) = 1.0
        _Shininess ("Shininess", Range(0, 1)) = 0.3
        _Transparency ("Transparency", Range(0, 1)) = 0.7
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 200
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float4 screenPos : TEXCOORD4;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _DeepColor;
                float _DepthFade;
                float4 _WaveSpeed;
                float _WaveHeight;
                float4 _FoamColor;
                float _FoamThreshold;
                float _NormalStrength;
                float _Shininess;
                float _Transparency;
            CBUFFER_END

            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                // Simple wave animation in vertex shader
                float time = _Time.y;
                float wave = sin(input.uv.x * 10.0 + time * _WaveSpeed.x) * cos(input.uv.y * 10.0 + time * _WaveSpeed.y) * _WaveHeight;
                input.positionOS.y += wave;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.uv = input.uv;
                output.normalWS = normalize(normalInput.normalWS);
                output.tangentWS = normalInput.tangentWS;
                output.screenPos = ComputeScreenPos(output.positionCS);
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Sample normal map
                half4 normalTex = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv);
                half3 normalTS = UnpackNormal(normalTex);
                half3 normalWS = normalize(mul((half3x3)GetWorldToTangentMatrix(input), normalTS));

                // Depth-based color fading
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, input.screenPos.xy / input.screenPos.w);
                depth = LinearEyeDepth(depth, _ZBufferParams);
                float depthDiff = depth - input.positionWS.z;
                float depthFactor = saturate(depthDiff / _DepthFade);
                
                half3 waterColor = lerp(_DeepColor.rgb, _Color.rgb, depthFactor);

                // Simple lighting
                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);
                half NdotL = saturate(dot(normalWS, lightDir));
                
                // Specular
                half3 viewDir = normalize(GetCameraPositionWS() - input.positionWS);
                half3 halfDir = normalize(lightDir + viewDir);
                half NdotH = saturate(dot(normalWS, halfDir));
                half specular = pow(NdotH, 128.0 * _Shininess) * mainLight.distanceAttenuation;

                // Foam simulation (simplified)
                half foam = step(_FoamThreshold, normalTex.g);
                
                half3 finalColor = waterColor * NdotL * mainLight.color;
                finalColor += specular * mainLight.color;
                finalColor = lerp(finalColor, _FoamColor.rgb, foam * 0.3);

                return half4(finalColor, _Transparency * _Color.a);
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
