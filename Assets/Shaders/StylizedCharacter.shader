Shader "AAA/LowPoly/StylizedCharacter"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _EmissionMap ("Emission", 2D) = "black" {}
        _OcclusionMap ("Occlusion", 2D) = "white" {}
        _DetailMask ("Detail Mask", 2D) = "white" {}
        _DetailAlbedoMap ("Detail Albedo x2", 2D) = "gray" {}
        _DetailNormalMap ("Detail Normal x2", 2D) = "bump" {}
        
        _Color ("Base Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _BumpScale ("Normal Scale", Range(0,2)) = 1.0
        _OcclusionStrength ("Occlusion Strength", Range(0,1)) = 1.0
        
        // Stylized shading
        _RampTexture ("Shading Ramp", 2D) = "white" {}
        _ShadowColor ("Shadow Color", Color) = (0.4, 0.4, 0.5, 1)
        _HighlightColor ("Highlight Color", Color) = (1, 0.9, 0.8, 1)
        _ShadowSoftness ("Shadow Softness", Range(0.01, 1)) = 0.3
        _CelShadingSteps ("Cel Steps", Range(2, 8)) = 3
        
        // Rim lighting
        _RimColor ("Rim Color", Color) = (0.5, 0.7, 1.0, 1)
        _RimPower ("Rim Power", Range(0.5, 8)) = 3.0
        _RimIntensity ("Rim Intensity", Range(0, 2)) = 1.0
        
        // Fresnel
        _FresnelColor ("Fresnel Color", Color) = (0.3, 0.5, 0.8, 1)
        _FresnelPower ("Fresnel Power", Range(0.1, 5)) = 2.0
        _FresnelIntensity ("Fresnel Intensity", Range(0, 2)) = 0.5
        
        // Subsurface scattering approximation
        _SSSColor ("SSS Color", Color) = (1, 0.6, 0.4, 1)
        _SSSDistance ("SSS Distance", Range(0, 1)) = 0.3
        _SSSIntensity ("SSS Intensity", Range(0, 2)) = 0.5
        
        // Outline
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.005
        
        // Anisotropy (for hair/fur effect)
        _AnisotropyDirection ("Anisotropy Direction", Vector) = (0, 1, 0, 0)
        _AnisotropyStrength ("Anisotropy Strength", Range(0, 1)) = 0.0
        _AnisotropyShift ("Anisotropy Shift", Range(-1, 1)) = 0.0
        
        // Dissolve effect
        _DissolveMap ("Dissolve Map", 2D) = "white" {}
        _DissolveThreshold ("Dissolve Threshold", Range(0, 1)) = 1
        _DissolveEdgeColor ("Dissolve Edge Color", Color) = (1, 0.5, 0, 1)
        _DissolveEdgeWidth ("Dissolve Edge Width", Range(0, 0.1)) = 0.05
        
        // Holographic effect
        _HolographicIntensity ("Holographic Intensity", Range(0, 1)) = 0
        _HolographicSpeed ("Holographic Speed", Range(0, 10)) = 1
        _HolographicColor ("Holographic Color", Color) = (0, 1, 1, 1)
        
        // Damage/wear
        _DamageMap ("Damage Map", 2D) = "white" {}
        _DamageIntensity ("Damage Intensity", Range(0, 1)) = 0
        _DamageColor ("Damage Color", Color) = (0.3, 0.1, 0.1, 1)
        
        // Dirt/grime accumulation
        _DirtMap ("Dirt Map", 2D) = "white" {}
        _DirtColor ("Dirt Color", Color) = (0.2, 0.15, 0.1, 1)
        _DirtIntensity ("Dirt Intensity", Range(0, 1)) = 0
        
        // Vertex colors usage
        _UseVertexColors ("Use Vertex Colors", Float) = 1.0
        _VertexColorBlend ("Vertex Color Blend", Range(0, 1)) = 1.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 500
        
        // Outline pass
        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode" = "Always" }
            
            Cull Front
            ZWrite On
            
            CGPROGRAM
            #pragma vertex vert_outline
            #pragma fragment frag_outline
            #pragma target 3.5
            
            #include "UnityCG.cginc"
            
            float _OutlineWidth;
            fixed4 _OutlineColor;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
            };
            
            v2f vert_outline(appdata v)
            {
                v2f o;
                float3 viewPos = UnityObjectToViewPos(v.vertex);
                float3 viewNormal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));
                float outlineExpand = _OutlineWidth * (1.0 - dot(viewNormal, float3(0,0,1)) * 0.5);
                
                v.vertex.xyz += v.normal * outlineExpand;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = _OutlineColor;
                return o;
            }
            
            fixed4 frag_outline(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
        
        // Main render pass
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow keepalpha
        #pragma target 4.5
        #pragma multi_compile_fog
        #pragma multi_compile_instancing
        
        #include "UnityCG.cginc"
        #include "Lighting.cginc"
        #include "UnityStandardUtils.cginc"
        
        sampler2D _MainTex;
        sampler2D _NormalMap;
        sampler2D _EmissionMap;
        sampler2D _OcclusionMap;
        sampler2D _DetailMask;
        sampler2D _DetailAlbedoMap;
        sampler2D _DetailNormalMap;
        sampler2D _RampTexture;
        sampler2D _DissolveMap;
        sampler2D _DamageMap;
        sampler2D _DirtMap;
        
        float4 _MainTex_ST;
        float4 _DetailAlbedoMap_ST;
        float4 _DetailNormalMap_ST;
        
        fixed4 _Color;
        half _Glossiness;
        half _Metallic;
        half _BumpScale;
        half _OcclusionStrength;
        
        fixed4 _ShadowColor;
        fixed4 _HighlightColor;
        half _ShadowSoftness;
        half _CelShadingSteps;
        
        fixed4 _RimColor;
        half _RimPower;
        half _RimIntensity;
        
        fixed4 _FresnelColor;
        half _FresnelPower;
        half _FresnelIntensity;
        
        fixed4 _SSSColor;
        half _SSSDistance;
        half _SSSIntensity;
        
        half _DissolveThreshold;
        fixed4 _DissolveEdgeColor;
        half _DissolveEdgeWidth;
        
        half _HolographicIntensity;
        half _HolographicSpeed;
        fixed4 _HolographicColor;
        
        half _DamageIntensity;
        fixed4 _DamageColor;
        
        half _DirtIntensity;
        fixed4 _DirtColor;
        
        half _UseVertexColors;
        half _VertexColorBlend;
        
        float4 _AnisotropyDirection;
        half _AnisotropyStrength;
        half _AnisotropyShift;
        
        struct Input
        {
            float2 uv_MainTex;
            float2 uv_DetailMask;
            float2 uv_DetailAlbedoMap;
            float2 uv_DetailNormalMap;
            float2 uv_DissolveMap;
            float2 uv_DamageMap;
            float2 uv_DirtMap;
            float3 viewDir;
            float3 worldPos;
            float3 worldNormal;
            INTERNAL_DATA
            fixed4 color : COLOR;
            UNITY_FOG_COORDS(1)
        };
        
        // Cel/Toon shading function
        fixed3 CelShade(fixed3 color, float NdotL, int steps)
        {
            float band = 1.0 / steps;
            float quantized = floor(NdotL * steps) * band;
            quantized = saturate(quantized);
            
            // Use ramp texture if available
            fixed3 rampColor = tex2D(_RampTexture, float2(quantized, 0.5)).rgb;
            return color * lerp(fixed3(quantized, quantized, quantized), rampColor, 0.5);
        }
        
        // Subsurface scattering approximation
        fixed3 CalculateSSS(fixed3 color, float3 worldPos, float3 viewDir, float3 lightDir)
        {
            float3 halfVec = normalize(lightDir + viewDir);
            float SSS = pow(saturate(dot(halfVec, -worldNormal)), _SSSDistance * 10);
            return color * _SSSColor.rgb * SSS * _SSSIntensity;
        }
        
        // Anisotropic highlight
        fixed3 CalculateAnisotropy(float3 viewDir, float3 normal, float3 tangent)
        {
            float3 halfVec = normalize(_WorldSpaceLightPos0.xyz + viewDir);
            float3 bitangent = cross(normal, tangent);
            
            float3 anisotropicHalfVec = normalize(halfVec - dot(halfVec, tangent) * tangent);
            float NdotAH = saturate(dot(normal, anisotropicHalfVec));
            
            float aniso = pow(1.0 - NdotAH, 256 * _AnisotropyStrength);
            aniso *= (1.0 + _AnisotropyShift);
            
            return _HighlightColor.rgb * aniso;
        }
        
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Base color with vertex colors
            fixed4 mainCol = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            
            if(_UseVertexColors > 0.5)
            {
                mainCol.rgb = lerp(mainCol.rgb, mainCol.rgb * IN.color.rgb, _VertexColorBlend);
            }
            
            // Detail maps
            fixed detailMask = tex2D(_DetailMask, IN.uv_DetailMask).g;
            fixed4 detailAlbedo = tex2D(_DetailAlbedoMap, IN.uv_DetailAlbedoMap);
            fixed3 detailNormal = UnpackNormal(tex2D(_DetailNormalMap, IN.uv_DetailNormalMap));
            
            mainCol.rgb = lerp(mainCol.rgb, mainCol.rgb * detailAlbedo.rgb * 2, detailMask);
            
            // Normal mapping
            fixed3 normalVal = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));
            normalVal = lerp(normalVal, detailNormal, detailMask);
            o.Normal = normalize(normalVal) * _BumpScale;
            
            // Occlusion
            half ao = tex2D(_OcclusionMap, IN.uv_MainTex).r;
            ao = lerp(1, ao, _OcclusionStrength);
            
            // Damage overlay
            fixed4 damage = tex2D(_DamageMap, IN.uv_DamageMap);
            mainCol.rgb = lerp(mainCol.rgb, _DamageColor.rgb, damage.r * _DamageIntensity);
            
            // Dirt overlay
            fixed4 dirt = tex2D(_DirtMap, IN.uv_DirtMap);
            mainCol.rgb = lerp(mainCol.rgb, _DirtColor.rgb, dirt.r * _DirtIntensity);
            
            // Lighting calculation for cel shading
            float3 worldNormal = WorldNormalVector(IN, o.Normal);
            float NdotL = saturate(dot(worldNormal, _WorldSpaceLightPos0.xyz));
            
            // Apply cel/toon shading
            fixed3 shadedColor = CelShade(mainCol.rgb, NdotL, (int)_CelShadingSteps);
            
            // Add shadow color tint
            shadedColor = lerp(shadedColor * _ShadowColor.rgb, shadedColor, NdotL);
            
            // Rim lighting
            float3 viewDir = normalize(IN.viewDir);
            float rim = 1.0 - saturate(dot(viewDir, worldNormal));
            rim = pow(rim, _RimPower) * _RimIntensity;
            
            // Fresnel effect
            float fresnel = pow(1.0 - saturate(dot(viewDir, worldNormal)), _FresnelPower);
            fresnel *= _FresnelIntensity;
            
            // Subsurface scattering
            fixed3 sss = CalculateSSS(mainCol.rgb, IN.worldPos, viewDir, _WorldSpaceLightPos0.xyz);
            
            // Anisotropic highlights
            fixed3 aniso = CalculateAnisotropy(viewDir, worldNormal, _AnisotropyDirection.xyz);
            
            // Holographic effect
            fixed3 holoColor = fixed3(0,0,0);
            if(_HolographicIntensity > 0)
            {
                float holoPattern = sin(IN.worldPos.y * 10 + _Time.y * _HolographicSpeed);
                holoPattern = smoothstep(0.3, 0.7, holoPattern);
                holoColor = _HolographicColor.rgb * holoPattern * _HolographicIntensity;
            }
            
            // Dissolve effect
            clip(tex2D(_DissolveMap, IN.uv_DissolveMap).r - _DissolveThreshold + 0.001);
            
            // Emission
            fixed4 emission = tex2D(_EmissionMap, IN.uv_MainTex);
            
            o.Albedo = shadedColor;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Occlusion = ao;
            o.Alpha = mainCol.a;
            o.Emission = emission.rgb + (_RimColor.rgb * rim) + (_FresnelColor.rgb * fresnel) + sss + aniso + holoColor;
            
            UNITY_APPLY_FOG(IN.fogCoord, o.Albedo);
        }
        ENDCG
    }
    
    // Fallback for older hardware
    FallBack "Standard"
    CustomEditor "StylizedCharacterEditor"
}
