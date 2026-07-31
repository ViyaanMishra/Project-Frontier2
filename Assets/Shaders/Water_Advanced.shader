Shader "AAA/LowPoly/Water/AdvancedWater"
{
    Properties
    {
        _Color ("Water Color", Color) = (0.0, 0.3, 0.5, 0.8)
        _DeepColor ("Deep Water Color", Color) = (0.0, 0.1, 0.2, 1.0)
        _ShallowColor ("Shallow Water Color", Color) = (0.0, 0.5, 0.7, 0.9)
        
        // Surface waves
        _WaveSpeed ("Wave Speed", Vector) = (0.5, 0.5, 0, 0)
        _WaveHeight ("Wave Height", Range(0, 2)) = 0.5
        _WaveTiling ("Wave Tiling", Range(0.1, 10)) = 1.0
        _WaveSteepness ("Wave Steepness", Range(0, 1)) = 0.3
        
        // Normal maps for surface detail
        _NormalMap1 ("Normal Map 1", 2D) = "bump" {}
        _NormalMap2 ("Normal Map 2", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 2)) = 1.0
        _NormalSpeed ("Normal Map Speed", Vector) = (0.02, 0.02, 0, 0)
        
        // Foam
        _FoamTexture ("Foam Texture", 2D) = "white" {}
        _FoamThreshold ("Foam Threshold", Range(0, 1)) = 0.5
        _FoamScale ("Foam Scale", Range(0.1, 10)) = 2.0
        _FoamSpeed ("Foam Speed", Range(0, 2)) = 0.5
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 0.8)
        _FoamDarkening ("Foam Darkening", Range(0, 1)) = 0.3
        
        // Caustics
        _CausticsTexture ("Caustics Texture", 2D) = "white" {}
        _CausticsStrength ("Caustics Strength", Range(0, 2)) = 1.0
        _CausticsSpeed ("Caustics Speed", Vector) = (0.05, 0.05, 0, 0)
        _CausticsScale ("Caustics Scale", Range(0.1, 10)) = 1.0
        _CausticsDepth ("Caustics Depth Falloff", Range(0, 10)) = 3.0
        
        // Reflection and refraction
        _ReflectionStrength ("Reflection Strength", Range(0, 1)) = 0.8
        _RefractionStrength ("Refraction Strength", Range(0, 1)) = 0.5
        _FresnelPower ("Fresnel Power", Range(0.5, 8)) = 3.0
        _FresnelBias ("Fresnel Bias", Range(0, 1)) = 0.1
        
        // Transparency and absorption
        _Transparency ("Transparency", Range(0, 1)) = 0.7
        _AbsorptionColor ("Absorption Color", Color) = (0.0, 0.3, 0.4, 1)
        _AbsorptionDistance ("Absorption Distance", Range(0, 50)) = 10.0
        
        // Specular
        _SpecularColor ("Specular Color", Color) = (1, 1, 1, 1)
        _SpecularStrength ("Specular Strength", Range(0, 2)) = 1.5
        _Glossiness ("Glossiness", Range(0, 1)) = 0.85
        
        // Depth-based effects
        _DepthGradient ("Depth Gradient", Range(0, 1)) = 0.5
        _MaxDepth ("Max Depth", Range(0, 100)) = 20.0
        
        // Shoreline
        _ShorelineFoamWidth ("Shoreline Foam Width", Range(0, 10)) = 2.0
        _ShorelineFoamIntensity ("Shoreline Foam Intensity", Range(0, 2)) = 1.0
        
        // Distortion
        _DistortionStrength ("Distortion Strength", Range(0, 1)) = 0.1
        _DistortionScale ("Distortion Scale", Range(0.1, 10)) = 1.0
        
        // Underwater fog
        _UnderwaterFogColor ("Underwater Fog Color", Color) = (0.0, 0.3, 0.5, 1)
        _UnderwaterFogDensity ("Underwater Fog Density", Range(0, 1)) = 0.3
        
        // Tessellation
        _TessellationDistance ("Tessellation Distance", Range(10, 500)) = 100
        _TessellationUniform ("Tessellation Uniform", Range(1, 64)) = 16
        
        // Flow map for rivers
        _FlowMap ("Flow Map", 2D) = "gray" {}
        _FlowStrength ("Flow Strength", Range(0, 2)) = 0.0
        _FlowSpeed ("Flow Speed", Range(-5, 5)) = 0.5
        
        // Ripples from intersections
        _RippleTexture ("Ripple Texture", 2D) = "black" {}
        _RippleStrength ("Ripple Strength", Range(0, 1)) = 0.5
        _RippleSpeed ("Ripple Speed", Range(0.1, 5)) = 1.0
        _RippleScale ("Ripple Scale", Range(0.1, 10)) = 1.0
        
        // Oil slick / iridescence
        _IridescenceStrength ("Iridescence Strength", Range(0, 1)) = 0.0
        _IridescenceScale ("Iridescence Scale", Range(0.1, 10)) = 1.0
        
        // Shadow receiving
        _ShadowStrength ("Shadow Strength", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
        LOD 600
        
        GrabPass { "_GrabTexture" }
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade vertex:vert tessellate:tessDistance
        #pragma target 4.5
        #pragma multi_compile_fog
        #pragma multi_compile_instancing
        
        #include "UnityCG.cginc"
        #include "Lighting.cginc"
        #include "UnityStandardUtils.cginc"
        
        sampler2D _GrabTexture;
        sampler2D _NormalMap1;
        sampler2D _NormalMap2;
        sampler2D _FoamTexture;
        sampler2D _CausticsTexture;
        sampler2D _FlowMap;
        sampler2D _RippleTexture;
        sampler2D_float _CameraDepthTexture;
        
        float4 _GrabTexture_TexelSize;
        
        fixed4 _Color;
        fixed4 _DeepColor;
        fixed4 _ShallowColor;
        
        float2 _WaveSpeed;
        half _WaveHeight;
        half _WaveTiling;
        half _WaveSteepness;
        
        half _NormalStrength;
        float2 _NormalSpeed;
        
        half _FoamThreshold;
        half _FoamScale;
        half _FoamSpeed;
        fixed4 _FoamColor;
        half _FoamDarkening;
        
        half _CausticsStrength;
        float2 _CausticsSpeed;
        half _CausticsScale;
        half _CausticsDepth;
        
        half _ReflectionStrength;
        half _RefractionStrength;
        half _FresnelPower;
        half _FresnelBias;
        
        half _Transparency;
        fixed4 _AbsorptionColor;
        half _AbsorptionDistance;
        
        fixed4 _SpecularColor;
        half _SpecularStrength;
        half _Glossiness;
        
        half _DepthGradient;
        half _MaxDepth;
        
        half _ShorelineFoamWidth;
        half _ShorelineFoamIntensity;
        
        half _DistortionStrength;
        half _DistortionScale;
        
        fixed4 _UnderwaterFogColor;
        half _UnderwaterFogDensity;
        
        half _TessellationDistance;
        half _TessellationUniform;
        
        half _FlowStrength;
        half _FlowSpeed;
        
        half _RippleStrength;
        half _RippleSpeed;
        half _RippleScale;
        
        half _IridescenceStrength;
        half _IridescenceScale;
        
        half _ShadowStrength;
        
        struct Input
        {
            float2 uv_NormalMap1;
            float2 uv_NormalMap2;
            float2 uv_FoamTexture;
            float2 uv_CausticsTexture;
            float2 uv_FlowMap;
            float4 grabPos;
            float3 viewDir;
            float3 worldPos;
            float3 worldNormal;
            INTERNAL_DATA
            UNITY_FOG_COORDS(1)
        };
        
        // Tessellation function
        float4 tessDistance(Input v0, Input v1, Input v2)
        {
            const float minDist = _TessellationDistance * 0.5;
            const float maxDist = _TessellationDistance;
            
            float d0 = distance(_WorldSpaceCameraPos, v0.worldPos);
            float d1 = distance(_WorldSpaceCameraPos, v1.worldPos);
            float d2 = distance(_WorldSpaceCameraPos, v2.worldPos);
            
            float f = 1.0 - saturate((min(d0, min(d1, d2)) - minDist) / (maxDist - minDist));
            return _TessellationUniform + f * (_TessellationUniform * 2);
        }
        
        // Gerstner wave function
        float3 GerstnerWave(float4 wave, float3 p, inout float3 tangent, inout float3 binormal)
        {
            float steepness = wave.w;
            float wavelength = wave.z;
            float k = 2 * UNITY_PI / wavelength;
            float c = sqrt(9.8 / k);
            float2 d = normalize(wave.xy);
            float f = k * (dot(p.xz, d) - c * _Time.y);
            float a = steepness / k;
            
            tangent += float3(
                d.x * -(steepness * sin(f)),
                0,
                d.y * -(steepness * sin(f))
            );
            
            binormal += float3(
                d.y * -(steepness * cos(f)),
                0,
                -d.x * -(steepness * cos(f))
            );
            
            return float3(
                d.x * (a * cos(f)),
                a * sin(f),
                d.y * (a * cos(f))
            );
        }
        
        // Calculate water displacement
        float3 CalculateWaterDisplacement(float2 uv, float time)
        {
            float3 displacement = float3(0, 0, 0);
            
            // Multiple wave layers
            for(int i = 0; i < 4; i++)
            {
                float frequency = pow(2, i);
                float amplitude = _WaveHeight / frequency;
                float speed = _WaveSpeed.x / frequency;
                
                displacement.y += sin(uv.x * _WaveTiling * frequency + time * speed) * amplitude;
                displacement.y += sin(uv.y * _WaveTiling * frequency + time * speed * 0.8) * amplitude;
            }
            
            return displacement;
        }
        
        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            
            float time = _Time.y;
            float3 tangent = float3(1, 0, 0);
            float3 binormal = float3(0, 0, 1);
            
            // Apply Gerstner waves
            float3 displacement = CalculateWaterDisplacement(v.texcoord.xy, time);
            v.vertex.xyz += displacement;
            
            // Update normals based on wave displacement
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            o.worldPos = worldPos;
        }
        
        // Foam calculation
        fixed CalculateFoam(float2 uv, float depth, float time)
        {
            float foamUV = uv.x * _FoamScale + time * _FoamSpeed;
            float foamNoise = tex2D(_FoamTexture, float2(foamUV, uv.y * _FoamScale)).r;
            foamNoise += tex2D(_FoamTexture, float2(uv.x * _FoamScale, foamUV)).r;
            foamNoise *= 0.5;
            
            float foam = smoothstep(_FoamThreshold, _FoamThreshold + 0.2, foamNoise);
            
            // Depth-based foam (shallow water)
            float depthFoam = smoothstep(2, 0, depth);
            foam = max(foam, depthFoam);
            
            return foam;
        }
        
        // Caustics calculation
        fixed3 CalculateCaustics(float2 uv, float depth, float time)
        {
            float2 causticsUV = uv * _CausticsScale + time * _CausticsSpeed;
            fixed caustics1 = tex2D(_CausticsTexture, causticsUV).r;
            fixed caustics2 = tex2D(_CausticsTexture, causticsUV + float2(10, 10)).r;
            
            fixed3 caustics = (caustics1 + caustics2) * 0.5;
            caustics *= smoothstep(_CausticsDepth, 0, depth);
            caustics *= _CausticsStrength;
            
            return caustics;
        }
        
        // Iridescence effect
        fixed3 CalculateIridescence(float3 viewDir, float3 normal)
        {
            if(_IridescenceStrength <= 0) return fixed3(0, 0, 0);
            
            float fresnel = pow(1.0 - saturate(dot(viewDir, normal)), 5);
            float iridescencePhase = fresnel * _IridescenceScale;
            
            fixed3 iridescenceColor;
            iridescenceColor.r = sin(iridescencePhase * 6.28) * 0.5 + 0.5;
            iridescenceColor.g = sin(iridescencePhase * 6.28 + 2.09) * 0.5 + 0.5;
            iridescenceColor.b = sin(iridescencePhase * 6.28 + 4.18) * 0.5 + 0.5;
            
            return iridescenceColor * _IridescenceStrength;
        }
        
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float time = _Time.y;
            
            // Animated normal maps
            float2 nmUV1 = IN.uv_NormalMap1 + time * _NormalSpeed;
            float2 nmUV2 = IN.uv_NormalMap2 - time * _NormalSpeed * 0.5;
            
            fixed3 normal1 = UnpackNormal(tex2D(_NormalMap1, nmUV1));
            fixed3 normal2 = UnpackNormal(tex2D(_NormalMap2, nmUV2));
            fixed3 normalBlend = lerp(normal1, normal2, 0.5);
            
            // Flow map influence
            if(_FlowStrength > 0)
            {
                fixed4 flow = tex2D(_FlowMap, IN.uv_FlowMap);
                normalBlend.xy += flow.xy * _FlowStrength * _FlowSpeed;
            }
            
            o.Normal = normalize(normalBlend) * _NormalStrength;
            
            float3 worldNormal = WorldNormalVector(IN, o.Normal);
            float3 viewDir = normalize(IN.viewDir);
            
            // Fresnel
            float fresnel = _FresnelBias + (1 - _FresnelBias) * pow(1.0 - saturate(dot(viewDir, worldNormal)), _FresnelPower);
            
            // Depth calculation for shallow/deep water blending
            float depth = 0;
            if(_CameraDepthTexture != 0)
            {
                float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(IN.grabPos)));
                float partZ = IN.grabPos.z;
                depth = sceneZ - partZ;
            }
            
            // Depth-based color
            float depthFactor = saturate(depth / _MaxDepth);
            fixed3 waterColor = lerp(_ShallowColor.rgb, _DeepColor.rgb, depthFactor);
            waterColor = lerp(waterColor, _Color.rgb, _DepthGradient);
            
            // Foam
            fixed foam = CalculateFoam(IN.uv_FoamTexture, depth, time);
            waterColor = lerp(waterColor, _FoamColor.rgb * (1 - _FoamDarkening), foam * _ShorelineFoamIntensity);
            
            // Caustics
            fixed3 caustics = CalculateCaustics(IN.uv_CausticsTexture, depth, time);
            waterColor += caustics;
            
            // Iridescence
            fixed3 iridescence = CalculateIridescence(viewDir, worldNormal);
            waterColor += iridescence;
            
            // Specular
            float3 halfVec = normalize(_WorldSpaceLightPos0.xyz + viewDir);
            float NdotH = saturate(dot(worldNormal, halfVec));
            float specular = pow(NdotH, 100 * _Glossiness) * _SpecularStrength;
            
            // Grab pass for refraction
            float2 distortionOffset = normalBlend.xy * _DistortionStrength * _DistortionScale;
            float4 grabUV = IN.grabPos;
            grabUV.xy += distortionOffset * grabUV.w;
            
            fixed4 refractedColor = tex2Dproj(_GrabTexture, grabUV);
            
            // Absorption
            fixed3 absorbedColor = refractedColor.rgb * exp(-depth * _AbsorptionColor.rgb / _AbsorptionDistance);
            
            // Blend reflection and refraction with fresnel
            fixed3 finalColor = lerp(absorbedColor, waterColor, fresnel * _ReflectionStrength);
            finalColor = lerp(finalColor, waterColor, _RefractionStrength);
            
            // Add specular
            finalColor += _SpecularColor.rgb * specular;
            
            // Shadow attenuation
            #if defined(SHADOWS_SCREEN)
            fixed shadow = tex2D(_ShadowMask, IN.uv_NormalMap1).r;
            finalColor *= lerp(1, shadow, _ShadowStrength);
            #endif
            
            o.Albedo = finalColor;
            o.Metallic = 0.0;
            o.Smoothness = _Glossiness;
            o.Alpha = _Transparency * (1 - foam * 0.3);
            o.Emission = caustics * 0.5;
            
            UNITY_APPLY_FOG(IN.fogCoord, o.Albedo);
        }
        ENDCG
        
        // Second grab pass for proper transparency
        GrabPass { "_GrabTexture2" }
    }
    
    FallBack "Transparent/Diffuse"
    CustomEditor "AdvancedWaterEditor"
}
