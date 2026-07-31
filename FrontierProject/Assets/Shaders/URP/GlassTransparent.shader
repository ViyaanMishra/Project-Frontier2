Shader "Frontier/URP/Glass/Transparent"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.95, 0.98, 1, 0.3)
        _Transmission ("Transmission", Range(0, 1)) = 0.9
        _Refraction ("Refraction Index", Range(0.9, 1.5)) = 1.05
        _RefractionDistortion ("Refraction Distortion", Range(0, 0.1)) = 0.02
        
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 2)) = 0.5
        
        _TintMap ("Tint Map", 2D) = "white" {}
        _TintStrength ("Tint Strength", Range(0, 1)) = 0.3
        
        _FresnelPower ("Fresnel Power", Range(0.1, 10)) = 2.5
        _FresnelColor ("Fresnel Color", Color) = (0.5, 0.7, 1, 1)
        
        _ScratchMap ("Scratch/Dirt Map", 2D) = "black" {}
        _ScratchStrength ("Scratch Strength", Range(0, 1)) = 0.15
        
        _ThicknessMap ("Thickness Map", 2D) = "white" {}
        _Thickness ("Thickness", Range(0, 1)) = 0.1
        
        _ReflectionBlur ("Reflection Blur", Range(0, 1)) = 0.1
    }
    
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 400
        
        Pass
        {
            Name "GlassTransparent"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            
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
                float4 screenPos : TEXCOORD5;
                UNITY_FOG_COORDS(6)
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
            TEXTURE2D(_TintMap); SAMPLER(sampler_TintMap);
            TEXTURE2D(_ScratchMap); SAMPLER(sampler_ScratchMap);
            TEXTURE2D(_ThicknessMap); SAMPLER(sampler_ThicknessMap);
            TEXTURE2D(_CameraColorTexture); SAMPLER(sampler_CameraColorTexture);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _FresnelColor;
                float _Transmission, _Refraction, _RefractionDistortion;
                float _NormalStrength, _TintStrength, _FresnelPower;
                float _ScratchStrength, _Thickness, _ReflectionBlur;
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
                output.screenPos = ComputeScreenPos(output.positionCS);
                
                UNITY_TRANSFER_FOG(output, output.positionCS);
                
                return output;
            }
            
            half CalculateFresnel(half3 viewDir, half3 normalWS, half power)
            {
                half cosAngle = saturate(dot(viewDir, normalWS));
                half fresnel = pow(1.0 - cosAngle, power);
                return fresnel;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                half3 viewDir = normalize(GetCameraPositionWS() - input.positionWS);
                half3 normalWS = normalize(input.normalWS);
                
                // Normal mapping
                #ifdef _NORMALMAP
                    half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv));
                    half3x3 tangentToWorld = half3x3(input.tangentWS, input.bitangentWS, input.normalWS);
                    normalWS = normalize(mul(tangentToWorld, normalTS));
                    normalWS = normalize(lerp(input.normalWS, normalWS, _NormalStrength));
                #endif
                
                // Refraction distortion
                half2 refractionOffset = normalWS.xy * _RefractionDistortion;
                float2 refractionUV = input.screenPos.xy / input.screenPos.w + refractionOffset;
                
                // Sample background with refraction
                half3 refractedColor = SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, refractionUV).rgb;
                
                // Base transmission
                half3 baseColor = _BaseColor.rgb * _Transmission;
                
                // Tint
                half tint = SAMPLE_TEXTURE2D(_TintMap, sampler_TintMap, input.uv).r;
                half3 tintedColor = lerp(refractedColor, refractedColor * _BaseColor.rgb, tint * _TintStrength);
                
                // Thickness-based absorption
                half thickness = SAMPLE_TEXTURE2D(_ThicknessMap, sampler_ThicknessMap, input.uv).r;
                half3 absorption = exp(-thickness * _Thickness * half3(0.1, 0.15, 0.2));
                tintedColor *= absorption;
                
                // Fresnel reflection
                half fresnel = CalculateFresnel(viewDir, normalWS, _FresnelPower);
                half3 fresnelReflection = _FresnelColor.rgb * fresnel;
                
                // Scratches/dirt affecting transparency
                half scratches = SAMPLE_TEXTURE2D(_ScratchMap, sampler_ScratchMap, input.uv).r;
                half alpha = _BaseColor.a * (1.0 - scratches * _ScratchStrength * 0.5);
                
                // Combine refracted color with fresnel reflection
                half3 finalColor = lerp(tintedColor, fresnelReflection, fresnel);
                
                // Add specular highlight
                Light mainLight = GetMainLight();
                half3 halfDir = SafeNormalize(mainLight.direction + viewDir);
                half NdotH = saturate(dot(normalWS, halfDir));
                half specularTerm = pow(NdotH, 256) * (1.0 - fresnel);
                half3 specular = half3(1, 1, 1) * specularTerm * mainLight.distanceAttenuation;
                finalColor += specular;
                
                UNITY_APPLY_FOG(finalColor, input.fogCoord);
                
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
        
        // Depth only pass for proper sorting
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
