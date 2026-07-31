Shader "Frontier/URP/Fabric/Cloth"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.5, 0.5, 0.5, 1)
        _MainTex ("Fabric Pattern", 2D) = "white" {}
        _PatternTiling ("Pattern Tiling", Float) = 10
        
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 2)) = 0.8
        
        _WeaveMap ("Weave Pattern", 2D) = "gray" {}
        _WeaveScale ("Weave Scale", Range(0.1, 5)) = 1
        _WeaveIntensity ("Weave Intensity", Range(0, 1)) = 0.3
        
        _Fuzziness ("Fuzziness/Hair", Range(0, 1)) = 0.2
        _FuzzColor ("Fuzz Color", Color) = (0.6, 0.6, 0.6, 1)
        
        _WrinkleMap ("Wrinkle Map", 2D) = "black" {}
        _WrinkleStrength ("Wrinkle Strength", Range(0, 1)) = 0.4
        
        _Sheen ("Sheen", Range(0, 1)) = 0.3
        _SheenColor ("Sheen Color", Color) = (0.8, 0.8, 0.9, 1)
        
        _Thickness ("Thickness", Range(0, 1)) = 0.5
        _SubsurfaceColor ("Subsurface Color", Color) = (0.8, 0.6, 0.5, 1)
        
        _AO ("Ambient Occlusion", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }
        LOD 350
        
        Pass
        {
            Name "FabricCloth"
            Tags { "LightMode" = "UniversalForward" }
            
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
                UNITY_FOG_COORDS(5)
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
            TEXTURE2D(_WeaveMap); SAMPLER(sampler_WeaveMap);
            TEXTURE2D(_WrinkleMap); SAMPLER(sampler_WrinkleMap);
            TEXTURE2D(_AO); SAMPLER(sampler_AO);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _FuzzColor;
                float4 _SheenColor;
                float4 _SubsurfaceColor;
                float _PatternTiling, _WeaveScale, _WeaveIntensity;
                float _NormalStrength, _Fuzziness, _WrinkleStrength;
                float _Sheen, _Thickness;
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
                output.uv = input.uv * _PatternTiling;
                
                UNITY_TRANSFER_FOG(output, output.positionCS);
                
                return output;
            }
            
            // Procedural weave pattern
            half CalculateWeave(float2 uv, float scale)
            {
                float2 wovenUV = uv * scale;
                half thread1 = sin(wovenUV.x * 3.14159 * 2) * 0.5 + 0.5;
                half thread2 = sin(wovenUV.y * 3.14159 * 2) * 0.5 + 0.5;
                half weave = max(thread1, thread2);
                return weave;
            }
            
            // Fuzz/hair effect based on view angle and light
            half3 CalculateFuzz(half3 viewDir, half3 normalWS, half3 lightDir)
            {
                half rimFactor = pow(1.0 - saturate(dot(viewDir, normalWS)), 3);
                half lightRim = saturate(dot(lightDir, normalize(cross(normalWS, cross(viewDir, normalWS)))));
                return _FuzzColor.rgb * rimFactor * lightRim * _Fuzziness;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                half3 viewDir = normalize(GetCameraPositionWS() - input.positionWS);
                half3 normalWS = normalize(input.normalWS);
                
                // Sample base texture
                half3 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).rgb * _BaseColor.rgb;
                
                // Normal mapping
                #ifdef _NORMALMAP
                    half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv));
                    half3x3 tangentToWorld = half3x3(input.tangentWS, input.bitangentWS, input.normalWS);
                    normalWS = normalize(mul(tangentToWorld, normalTS));
                    normalWS = normalize(lerp(input.normalWS, normalWS, _NormalStrength));
                #endif
                
                // Weave pattern
                half weaveSample = SAMPLE_TEXTURE2D(_WeaveMap, sampler_WeaveMap, input.uv * _WeaveScale).r;
                half proceduralWeave = CalculateWeave(input.uv, _WeaveScale);
                half combinedWeave = lerp(proceduralWeave, weaveSample, 0.5) * _WeaveIntensity;
                albedo *= lerp(half3(1, 1, 1), half3(1.1, 1.05, 1), combinedWeave);
                
                // Wrinkles (affect both color and normals)
                half wrinkles = SAMPLE_TEXTURE2D(_WrinkleMap, sampler_WrinkleMap, input.uv).r;
                albedo *= lerp(half3(1, 1, 1), half3(0.7, 0.7, 0.75), wrinkles * _WrinkleStrength);
                
                // Ambient Occlusion
                half ao = SAMPLE_TEXTURE2D(_AO, sampler_AO, input.uv).r;
                
                // Lighting
                Light mainLight = GetMainLight();
                half3 attenuatedLighting = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
                
                // Diffuse with cloth-like falloff (softer than standard Lambert)
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half diffuseTerm = NdotL * 0.5 + 0.5; // Softer cloth shading
                half3 diffuseColor = albedo * mainLight.color * diffuseTerm * attenuatedLighting;
                
                // Sheen (velvet-like effect at grazing angles)
                half sheenFactor = pow(1.0 - saturate(dot(viewDir, normalWS)), 2);
                half3 sheenColor = _SheenColor.rgb * sheenFactor * _Sheen;
                
                // Fuzz effect
                half3 fuzz = CalculateFuzz(viewDir, normalWS, mainLight.direction);
                
                // Subsurface scattering approximation (light passing through fabric)
                half sssMask = 1.0 - saturate(dot(viewDir, -mainLight.direction));
                sssMask = pow(sssMask, 3) * _Thickness;
                half3 subsurface = _SubsurfaceColor.rgb * sssMask * attenuatedLighting;
                
                // Combine all components
                half3 color = diffuseColor + sheenColor + fuzz + subsurface;
                color *= ao;
                
                // Add ambient
                half3 ambientTerm = SampleSH(normalWS) * albedo * 0.6;
                color += ambientTerm;
                
                UNITY_APPLY_FOG(color, input.fogCoord);
                
                return half4(color, 1);
            }
            ENDHLSL
        }
        
        // Shadow pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            ENDHLSL
        }
    }
    
    FallBack "Hidden/InternalErrorShader"
}
