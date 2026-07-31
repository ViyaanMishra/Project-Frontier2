Shader "AAA/LowPoly/TerrainPBR"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _BumpScale ("Normal Scale", Range(0,2)) = 1.0
        _AOStrength ("AO Strength", Range(0,2)) = 1.0
        _HeightMap ("Height Map", 2D) = "gray" {}
        _HeightScale ("Height Scale", Range(0,0.1)) = 0.02
        _ParallaxOffset ("Parallax Offset", Range(0,0.08)) = 0.04
        _DetailMask ("Detail Mask", 2D) = "white" {}
        _DetailAlbedoMap ("Detail Albedo x2", 2D) = "gray" {}
        _DetailNormalMap ("Detail Normal x2", 2D) = "bump" {}
        _DetailNormalScale ("Detail Normal Scale", Range(0,2)) = 0.5
        _OcclusionStrength ("Occlusion Strength", Range(0,1)) = 1.0
        
        // Triplanar mapping
        _TriplanarBlendSharpness ("Triplanar Sharpness", Range(1,10)) = 4.0
        _UseTriplanar ("Use Triplanar Mapping", Float) = 0.0
        
        // Tessellation
        _TessellationUniform ("Tessellation Uniform", Range(1,64)) = 16
        _TessellationDistance ("Tessellation Distance", Range(10,500)) = 100
        _TessellationHeight ("Tessellation Height Factor", Range(0,1)) = 0.5
        
        // Grass/vegetation blending
        _GrassBlend ("Grass Blend", Range(0,1)) = 0.0
        _GrassColor ("Grass Color", Color) = (0.3, 0.6, 0.2, 1)
        
        // Snow
        _SnowColor ("Snow Color", Color) = (0.95, 0.95, 1.0, 1)
        _SnowThreshold ("Snow Threshold", Range(-1,1)) = 0.5
        _SnowSoftness ("Snow Softness", Range(0.01,1)) = 0.2
        
        // Wetness
        _Wetness ("Wetness", Range(0,1)) = 0.0
        _WetColor ("Wet Color", Color) = (0.1, 0.1, 0.15, 1)
        
        // Rim lighting
        _RimColor ("Rim Color", Color) = (0.5, 0.7, 1.0, 1)
        _RimPower ("Rim Power", Range(0.5,8)) = 3.0
        _RimIntensity ("Rim Intensity", Range(0,2)) = 0.5
        
        // Ambient occlusion texture
        _AOTex ("Ambient Occlusion", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-100" }
        LOD 600
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert tessellate:tessDistance
        #pragma target 4.5
        #pragma multi_compile_fog
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup
        
        #include "UnityCG.cginc"
        #include "Lighting.cginc"
        #include "UnityStandardUtils.cginc"
        #include "UnityInstancing.cginc"
        
        sampler2D _MainTex;
        sampler2D _NormalMap;
        sampler2D _HeightMap;
        sampler2D _DetailMask;
        sampler2D _DetailAlbedoMap;
        sampler2D _DetailNormalMap;
        sampler2D _AOTex;
        
        float4 _MainTex_ST;
        float4 _DetailAlbedoMap_ST;
        float4 _DetailNormalMap_ST;
        
        half _Glossiness;
        half _Metallic;
        half _BumpScale;
        half _AOStrength;
        half _HeightScale;
        half _ParallaxOffset;
        half _DetailNormalScale;
        half _OcclusionStrength;
        half _TriplanarBlendSharpness;
        half _UseTriplanar;
        
        half _TessellationUniform;
        half _TessellationDistance;
        half _TessellationHeight;
        
        half _GrassBlend;
        fixed4 _GrassColor;
        
        fixed4 _SnowColor;
        half _SnowThreshold;
        half _SnowSoftness;
        
        half _Wetness;
        fixed4 _WetColor;
        
        fixed4 _RimColor;
        half _RimPower;
        half _RimIntensity;
        
        struct Input
        {
            float2 uv_MainTex;
            float2 uv_DetailMask;
            float2 uv_DetailAlbedoMap;
            float2 uv_DetailNormalMap;
            float2 uv_AOTex;
            float3 viewDir;
            float3 worldPos;
            float3 worldNormal;
            INTERNAL_DATA
            float3 normal;
            fixed4 color : COLOR;
        };
        
        UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
        UNITY_INSTANCING_BUFFER_END(Props)
        
        void setup()
        {
            UNITY_INITIALIZE_OUTPUT(Props, Props);
            Props._Color = float4(1,1,1,1);
        }
        
        // Tessellation function
        float4 tessDistance(Input v0, Input v1, Input v2)
        {
            const float minDist = _TessellationDistance * 0.5;
            const float maxDist = _TessellationDistance;
            
            float3 pp0 = UnityObjectToViewPos(v0.vertex);
            float3 pp1 = UnityObjectToViewPos(v1.vertex);
            float3 pp2 = UnityObjectToViewPos(v2.vertex);
            
            float d0 = distance(_WorldSpaceCameraPos, v0.worldPos);
            float d1 = distance(_WorldSpaceCameraPos, v1.worldPos);
            float d2 = distance(_WorldSpaceCameraPos, v2.worldPos);
            
            float f = 1.0 - saturate((min(d0, min(d1, d2)) - minDist) / (maxDist - minDist));
            return _TessellationUniform + f * (_TessellationUniform * 3);
        }
        
        // Triplanar sampling helper
        fixed4 SampleTriplanar(sampler2D tex, float3 worldPos, float3 worldNormal, float4 texST)
        {
            float3 blendWeights = pow(abs(worldNormal), _TriplanarBlendSharpness);
            blendWeights /= (blendWeights.x + blendWeights.y + blendWeights.z + 0.0001);
            
            float2 uvX = worldPos.zy * texST.xy + texST.zw;
            float2 uvY = worldPos.xz * texST.xy + texST.zw;
            float2 uvZ = worldPos.xy * texST.xy + texST.zw;
            
            fixed4 colX = tex2D(tex, uvX);
            fixed4 colY = tex2D(tex, uvY);
            fixed4 colZ = tex2D(tex, uvZ);
            
            return colX * blendWeights.x + colY * blendWeights.y + colZ * blendWeights.z;
        }
        
        // Parallax occlusion mapping
        float2 ParallaxOcclusionMapping(float2 uv, float3 viewDir, float heightScale)
        {
            const int minLayers = 8;
            const int maxLayers = 32;
            
            float numLayers = lerp(minLayers, maxLayers, abs(dot(viewDir, float3(0,0,1))));
            float layerDepth = 1.0 / numLayers;
            float currentLayerDepth = 0.0;
            float P = viewDir.z * heightScale;
            float deltaUVs = P / numLayers;
            
            float2 currentUV = uv;
            float currentDepthMapValue = 0.0;
            
            for(int i = 0; i < maxLayers; i++)
            {
                if(currentLayerDepth >= currentDepthMapValue)
                    break;
                    
                currentUV -= deltaUVs;
                currentDepthMapValue = tex2Dlod(_HeightMap, float4(currentUV, 0, 0)).r;
                currentLayerDepth += layerDepth;
            }
            
            float2 prevUV = currentUV + deltaUVs;
            float prevDepthMapValue = tex2Dlod(_HeightMap, float4(prevUV, 0, 0)).r;
            
            float depth = currentLayerDepth - layerDepth;
            float depth2 = currentDepthMapValue;
            float nearDepth = depth - currentLayerDepth;
            float farDepth = depth2 - currentLayerDepth;
            
            float surfaceDist = nearDepth / (nearDepth - farDepth);
            return prevUV + deltaUVs * surfaceDist;
        }
        
        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
            o.uv_DetailMask = v.texcoord;
            o.uv_DetailAlbedoMap = TRANSFORM_TEX(v.texcoord, _DetailAlbedoMap);
            o.uv_DetailNormalMap = TRANSFORM_TEX(v.texcoord, _DetailNormalMap);
            o.uv_AOTex = v.texcoord;
            o.color = v.color;
            
            // Vertex displacement based on height map
            float height = tex2Dlod(_HeightMap, float4(o.uv_MainTex, 0, 0)).r;
            v.vertex.xyz += v.normal * height * _HeightScale * _TessellationHeight;
        }
        
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 mainCol;
            fixed3 normalVal;
            
            if(_UseTriplanar > 0.5)
            {
                // Triplanar mapping
                float3 worldPos = IN.worldPos;
                float3 worldNormal = WorldNormalVector(IN, float3(0,0,1));
                
                mainCol = SampleTriplanar(_MainTex, worldPos, worldNormal, _MainTex_ST);
                normalVal = UnpackNormal(SampleTriplanar(_NormalMap, worldPos, worldNormal, float4(1,1,0,0)));
                
                // Apply parallax occlusion mapping with triplanar
                float2 pomUV = ParallaxOcclusionMapping(IN.uv_MainTex, IN.viewDir, _ParallaxOffset);
                mainCol = SampleTriplanar(_MainTex, worldPos + pomUV, worldNormal, _MainTex_ST);
            }
            else
            {
                // Standard UV mapping with POM
                float2 pomUV = ParallaxOcclusionMapping(IN.uv_MainTex, IN.viewDir, _ParallaxOffset);
                mainCol = tex2D(_MainTex, pomUV);
                normalVal = UnpackNormal(tex2D(_NormalMap, pomUV));
            }
            
            // Detail maps
            fixed detailMask = tex2D(_DetailMask, IN.uv_DetailMask).g;
            fixed4 detailAlbedo = tex2D(_DetailAlbedoMap, IN.uv_DetailAlbedoMap);
            fixed3 detailNormal = UnpackNormal(tex2D(_DetailNormalMap, IN.uv_DetailNormalMap));
            
            // Blend detail with main
            mainCol.rgb = lerp(mainCol.rgb, mainCol.rgb * detailAlbedo.rgb * 2, detailMask);
            normalVal = lerp(normalVal, detailNormal, detailMask * _DetailNormalScale);
            
            // Ambient occlusion
            half ao = tex2D(_AOTex, IN.uv_AOTex).r;
            ao = lerp(1, ao, _OcclusionStrength);
            
            // Snow accumulation based on normal and height
            float snowFactor = smoothstep(_SnowThreshold, _SnowThreshold + _SnowSoftness, IN.worldNormal.y);
            snowFactor *= smoothstep(0.3, 0.8, tex2D(_HeightMap, IN.uv_MainTex).r);
            mainCol.rgb = lerp(mainCol.rgb, _SnowColor.rgb, snowFactor);
            
            // Wetness effect
            mainCol.rgb = lerp(mainCol.rgb, mainCol.rgb * (1 - _WetColor.rgb), _Wetness);
            _Glossiness = lerp(_Glossiness, 0.8, _Wetness);
            
            // Grass blending
            fixed3 grassBlend = _GrassColor.rgb * _GrassBlend;
            mainCol.rgb = lerp(mainCol.rgb, grassBlend, _GrassBlend);
            
            // Rim lighting calculation
            float3 viewDir = normalize(IN.viewDir);
            float3 N = normalize(normalVal);
            float rim = 1.0 - saturate(dot(viewDir, N));
            rim = pow(rim, _RimPower) * _RimIntensity;
            
            o.Albedo = mainCol.rgb * IN.color.rgb;
            o.Normal = normalize(normalVal) * _BumpScale;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Occlusion = ao;
            o.Alpha = mainCol.a;
            
            // Add rim to emission for stylized look
            o.Emission = _RimColor.rgb * rim;
        }
        ENDCG
    }
    FallBack "Standard"
    CustomEditor "TerrainPBREditor"
}
