Shader "AAA/LowPoly/VFX/ParticleMaster"
{
    Properties
    {
        _MainTex ("Particle Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        
        // Particle animation
        _FrameOverTime ("Frames Over Time", Range(1, 64)) = 8
        _CycleCount ("Cycle Count", Range(1, 10)) = 1
        _UVBlend ("UV Blend", Range(0, 1)) = 0.5
        
        // Size and rotation
        _SizeOverLife ("Size Over Life Curve", Vector) = (1, 1, 1, 1)
        _RotationSpeed ("Rotation Speed", Range(-360, 360)) = 0
        _StartRotation ("Start Rotation", Range(0, 360)) = 0
        
        // Color over life
        _ColorOverLifeStart ("Color Start", Color) = (1,1,1,1)
        _ColorOverLifeMid ("Color Mid", Color) = (1,1,1,1)
        _ColorOverLifeEnd ("Color End", Color) = (1,1,1,1)
        
        // Emission/glow
        _EmissionStrength ("Emission Strength", Range(0, 5)) = 1.0
        _EmissionColor ("Emission Color", Color) = (1, 0.5, 0, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 3)) = 1.5
        
        // Distortion
        _DistortionStrength ("Distortion Strength", Range(0, 1)) = 0.0
        _DistortionSpeed ("Distortion Speed", Range(0, 10)) = 1.0
        _DistortionScale ("Distortion Scale", Range(0.1, 10)) = 1.0
        
        // Soft particles
        _SoftParticleFade ("Soft Particle Fade", Range(0, 2)) = 0.5
        _CameraFade ("Camera Fade", Range(0, 10)) = 2.0
        
        // Noise
        _NoiseTexture ("Noise Texture", 2D) = "gray" {}
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.0
        _NoiseSpeed ("Noise Speed", Vector) = (0.1, 0.1, 0, 0)
        _NoiseScale ("Noise Scale", Range(0.1, 10)) = 1.0
        
        // Rim/Fresnel
        _RimPower ("Rim Power", Range(0.5, 8)) = 3.0
        _RimIntensity ("Rim Intensity", Range(0, 5)) = 1.0
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        
        // Depth fade color
        _DepthFadeColor ("Depth Fade Color", Color) = (1, 0.5, 0, 1)
        _DepthFadeStart ("Depth Fade Start", Range(0, 10)) = 1.0
        _DepthFadeEnd ("Depth Fade End", Range(0, 10)) = 5.0
        
        // Turbulence
        _TurbulenceStrength ("Turbulence Strength", Range(0, 2)) = 0.0
        _TurbulenceFrequency ("Turbulence Frequency", Range(0.1, 10)) = 1.0
        _TurbulenceSpeed ("Turbulence Speed", Range(0, 5)) = 1.0
        
        // Sparkle/scintillation
        _SparkleIntensity ("Sparkle Intensity", Range(0, 2)) = 0.0
        _SparkleFrequency ("Sparkle Frequency", Range(0.1, 20)) = 5.0
        _SparkleSize ("Sparkle Size", Range(0.01, 1)) = 0.1
        
        // Dissolve/burn
        _DissolveEdgeColor ("Dissolve Edge Color", Color) = (1, 0.5, 0, 1)
        _DissolveEdgeWidth ("Dissolve Edge Width", Range(0, 1)) = 0.1
        _DissolveThreshold ("Dissolve Threshold", Range(0, 1)) = 0
        
        // Flowmap
        _FlowMap ("Flow Map", 2D) = "gray" {}
        _FlowStrength ("Flow Strength", Range(0, 2)) = 0.0
        _FlowSpeed ("Flow Speed", Range(-5, 5)) = 0.5
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "PreviewType"="Plane" "IgnoreProjector"="True" }
        LOD 400
        
        Cull Off
        Lighting Off
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            Name "MAIN"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.0
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            sampler2D _NoiseTexture;
            sampler2D _FlowMap;
            sampler2D_float _CameraDepthTexture;
            
            float4 _MainTex_ST;
            fixed4 _Color;
            
            half _FrameOverTime;
            half _CycleCount;
            half _UVBlend;
            
            float4 _SizeOverLife;
            half _RotationSpeed;
            half _StartRotation;
            
            fixed4 _ColorOverLifeStart;
            fixed4 _ColorOverLifeMid;
            fixed4 _ColorOverLifeEnd;
            
            half _EmissionStrength;
            fixed4 _EmissionColor;
            half _GlowIntensity;
            
            half _DistortionStrength;
            half _DistortionSpeed;
            half _DistortionScale;
            
            half _SoftParticleFade;
            half _CameraFade;
            
            half _NoiseStrength;
            float4 _NoiseSpeed;
            half _NoiseScale;
            
            half _RimPower;
            half _RimIntensity;
            fixed4 _RimColor;
            
            fixed4 _DepthFadeColor;
            half _DepthFadeStart;
            half _DepthFadeEnd;
            
            half _TurbulenceStrength;
            half _TurbulenceFrequency;
            half _TurbulenceSpeed;
            
            half _SparkleIntensity;
            half _SparkleFrequency;
            half _SparkleSize;
            
            fixed4 _DissolveEdgeColor;
            half _DissolveEdgeWidth;
            half _DissolveThreshold;
            
            half _FlowStrength;
            half _FlowSpeed;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float4 tangent : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float3 worldPos : TEXCOORD1;
                float4 params : TEXCOORD2;  // x: lifetime, y: size, z: rotation, w: frame
                float3 viewDir : TEXCOORD3;
                float depth : TEXCOORD4;
                UNITY_FOG_COORDS(5)
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            // Pseudo-random function
            float rand(float2 co)
            {
                return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
            }
            
            // Noise function
            float noise(float2 uv)
            {
                return tex2Dlod(_NoiseTexture, float4(uv, 0, 0)).r;
            }
            
            // Turbulence function
            float turbulence(float2 uv, float time)
            {
                float value = 0.0;
                float amplitude = 1.0;
                float frequency = 1.0;
                
                for(int i = 0; i < 4; i++)
                {
                    value += amplitude * noise(uv * frequency + time);
                    amplitude *= 0.5;
                    frequency *= 2.0;
                }
                
                return value;
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                // Calculate particle parameters from vertex color
                float lifetime = v.color.r;  // 0-1 lifetime
                float startSize = v.color.g;
                float rotation = v.color.b * 360;
                float frame = v.color.a * _FrameOverTime;
                
                // Animate size over lifetime
                float sizeMultiplier = _SizeOverLife.x * (1-lifetime) + _SizeOverLife.y * lifetime;
                sizeMultiplier = lerp(sizeMultiplier, _SizeOverLife.z, smoothstep(0.3, 0.7, lifetime));
                sizeMultiplier = lerp(sizeMultiplier, _SizeOverLife.w, lifetime);
                
                // Animate rotation
                rotation += _Time.y * _RotationSpeed * lifetime;
                rotation += _StartRotation;
                
                // Animate UVs for sprite sheet
                float framesX = sqrt(_FrameOverTime);
                float framesY = _FrameOverTime / framesX;
                float currentFrame = fmod(frame + _Time.y * _CycleCount, _FrameOverTime);
                float frameX = fmod(currentFrame, framesX) / framesX;
                float frameY = floor(currentFrame / framesX) / framesY;
                
                // Apply flowmap distortion
                float2 flowUV = v.uv;
                if(_FlowStrength > 0)
                {
                    fixed4 flow = tex2Dlod(_FlowMap, float4(flowUV, 0, 0));
                    flowUV += flow.xy * _FlowStrength * _FlowSpeed * _Time.y;
                }
                
                // Apply noise distortion
                float2 noiseUV = v.uv * _NoiseScale + _Time.y * _NoiseSpeed.xy;
                if(_NoiseStrength > 0)
                {
                    float noiseVal = noise(noiseUV);
                    flowUV += (noiseVal - 0.5) * _NoiseStrength;
                }
                
                // Apply turbulence
                if(_TurbulenceStrength > 0)
                {
                    float turb = turbulence(v.uv * _TurbulenceFrequency, _Time.y * _TurbulenceSpeed);
                    v.vertex.xy += (turb - 0.5) * _TurbulenceStrength;
                }
                
                // Rotate vertex
                float rad = radians(rotation);
                float2x4 rot = float2x4(cos(rad), -sin(rad), sin(rad), cos(rad));
                float2 rotatedVertex = mul(rot, v.vertex.xy);
                
                // Scale vertex
                v.vertex.xy = rotatedVertex * sizeMultiplier * startSize;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(flowUV, _MainTex);
                o.uv += float2(frameX, frameY);
                o.color = v.color;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.params = float4(lifetime, sizeMultiplier, rotation, currentFrame);
                o.viewDir = normalize(_WorldSpaceCameraPos - o.worldPos);
                
                // Depth for soft particles
                o.depth = COMPUTE_DEPTH_01;
                
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // Sample main texture
                fixed4 mainTex = tex2D(_MainTex, i.uv);
                
                // Apply noise scrolling
                if(_NoiseStrength > 0)
                {
                    float2 noiseUV = i.uv * _NoiseScale + _Time.y * _NoiseSpeed.xy;
                    float noiseVal = noise(noiseUV);
                    mainTex.rgb = lerp(mainTex.rgb, mainTex.rgb * noiseVal, _NoiseStrength);
                }
                
                // Color over lifetime
                fixed4 colorOverLife = lerp(_ColorOverLifeStart, _ColorOverLifeMid, i.params.x * 2);
                colorOverLife = lerp(colorOverLife, _ColorOverLifeEnd, saturate(i.params.x * 2 - 1));
                
                // Base color
                fixed4 col = mainTex * _Color * i.color * colorOverLife;
                
                // Soft particles
                if(_SoftParticleFade > 0 && _CameraDepthTexture != 0)
                {
                    float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.depth)));
                    float partZ = i.depth;
                    float fade = saturate(_SoftParticleFade * (sceneZ - partZ));
                    col.a *= lerp(1, fade, _SoftParticleFade);
                }
                
                // Camera fade
                if(_CameraFade > 0)
                {
                    float distToCamera = length(_WorldSpaceCameraPos - i.worldPos);
                    float cameraFade = smoothstep(0, _CameraFade, distToCamera);
                    col.a *= cameraFade;
                }
                
                // Depth fade
                if(_DepthFadeEnd > _DepthFadeStart)
                {
                    float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.depth)));
                    float partZ = i.depth;
                    float depthDiff = sceneZ - partZ;
                    float depthFade = smoothstep(_DepthFadeStart, _DepthFadeEnd, depthDiff);
                    col.rgb = lerp(_DepthFadeColor.rgb, col.rgb, depthFade);
                }
                
                // Rim lighting
                float rim = 1.0 - saturate(dot(normalize(i.viewDir), float3(0,0,1)));
                rim = pow(rim, _RimPower) * _RimIntensity;
                col.rgb += _RimColor.rgb * rim * col.a;
                
                // Emission/glow
                col.rgb += _EmissionColor.rgb * mainTex.rgb * _EmissionStrength * _GlowIntensity;
                
                // Sparkle effect
                if(_SparkleIntensity > 0)
                {
                    float sparkleNoise = rand(i.uv + _Time.y * _SparkleFrequency);
                    float sparkle = smoothstep(1 - _SparkleSize, 1, sparkleNoise);
                    col.rgb += sparkle * _SparkleIntensity;
                }
                
                // Dissolve edge
                if(_DissolveThreshold > 0)
                {
                    float dissolveEdge = smoothstep(_DissolveThreshold - _DissolveEdgeWidth, _DissolveThreshold, mainTex.a);
                    col.rgb += _DissolveEdgeColor.rgb * dissolveEdge * (1 - mainTex.a);
                }
                
                // Distortion (screen space)
                if(_DistortionStrength > 0)
                {
                    float2 distortionUV = i.uv * _DistortionScale + _Time.y * _DistortionSpeed;
                    float distortion = noise(distortionUV) * _DistortionStrength;
                    // Note: Full screen distortion requires grab pass
                }
                
                // Fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                return col;
            }
            ENDCG
        }
        
        // Additive pass for glow effects
        Pass
        {
            Name "ADDITIVE"
            
            Tags { "LightMode" = "Always" }
            
            Cull Off
            Lighting Off
            ZWrite Off
            Blend One One
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_add
            #pragma target 4.0
            
            #include "UnityCG.cginc"
            
            // Reuse properties from above
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _EmissionColor;
            half _EmissionStrength;
            half _GlowIntensity;
            half _RimPower;
            half _RimIntensity;
            fixed4 _RimColor;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float3 viewDir : TEXCOORD1;
            };
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                o.viewDir = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex).xyz);
                return o;
            }
            
            fixed4 frag_add(v2f i) : SV_Target
            {
                fixed4 mainTex = tex2D(_MainTex, i.uv);
                fixed4 col = mainTex * _Color * i.color;
                
                // Strong emission for additive pass
                col.rgb *= _EmissionStrength * _GlowIntensity * 2;
                
                // Rim for additive
                float rim = 1.0 - saturate(dot(normalize(i.viewDir), float3(0,0,1)));
                rim = pow(rim, _RimPower) * _RimIntensity;
                col.rgb += _RimColor.rgb * rim;
                
                return col;
            }
            ENDCG
        }
    }
    
    FallBack "Particles/Standard Unlit"
}
