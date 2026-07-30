Shader "Frontier/Advanced/StylizedToonOutline"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.02
        _RimColor ("Rim Light Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", Range(0.5, 8)) = 3.0
        _ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.3
        _ShadowColor ("Shadow Color", Color) = (0.5, 0.5, 0.6, 1)
        _SpecularColor ("Specular Color", Color) = (1,1,1,1)
        _Shininess ("Shininess", Range(0.01, 1)) = 0.3
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        // Outline Pass (Back faces, expanded)
        Pass
        {
            Name "Outline"
            Cull Front
            ZWrite On
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                float _OutlineWidth;
                float4 _OutlineColor;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                // Expand vertex along normal for outline
                float3 expandedPos = input.positionOS.xyz + input.normalOS * _OutlineWidth;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(expandedPos);
                output.positionCS = vertexInput.positionCS;
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }

        // Main Toon Shading Pass
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _RimColor;
                float _RimPower;
                float _ShadowThreshold;
                float4 _ShadowColor;
                float4 _SpecularColor;
                float _Shininess;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = vertexInput.positionCS;
                output.normalWS = normalize(normalInput.normalWS);
                output.positionWS = vertexInput.positionWS;
                output.uv = input.uv;
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);
                half3 normalWS = normalize(input.normalWS);
                half3 viewDir = normalize(GetCameraPositionWS() - input.positionWS);

                // Diffuse with toon ramp
                half NdotL = saturate(dot(normalWS, lightDir));
                half diffuseIntensity = NdotL > _ShadowThreshold ? 1.0 : 0.4;
                half3 diffuse = _BaseColor.rgb * mainLight.color * diffuseIntensity * mainLight.distanceAttenuation;

                // Shadow color blend
                if (NdotL <= _ShadowThreshold)
                    diffuse = lerp(diffuse, _ShadowColor.rgb * _BaseColor.rgb * mainLight.color, 0.5);

                // Specular (sharp toon highlight)
                half3 halfDir = normalize(lightDir + viewDir);
                half NdotH = saturate(dot(normalWS, halfDir));
                half specularIntensity = step(0.95 - _Shininess * 0.5, NdotH);
                half3 specular = _SpecularColor.rgb * mainLight.color * specularIntensity * mainLight.distanceAttenuation;

                // Rim lighting
                half NdotV = saturate(dot(normalWS, viewDir));
                half rimFactor = 1.0 - NdotV;
                rimFactor = pow(rimFactor, _RimPower);
                half3 rim = _RimColor.rgb * rimFactor;

                // Ambient
                half3 ambient = _BaseColor.rgb * 0.1;

                half3 finalColor = ambient + diffuse + specular + rim;
                
                return half4(finalColor, _BaseColor.a);
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
