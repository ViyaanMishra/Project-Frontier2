Shader \"Frontier/HologramUI\" {
    Properties { _Color(\"Color\", Color) = (0,1,0,0.8) _ScanSpeed(\"Scan Speed\", Float) = 2.0 }
    SubShader {
        Tags { \"RenderType\"=\"Transparent\" }
        Pass {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM #pragma vertex vert #pragma fragment frag #include \"UnityCG.cginc\"
            fixed4 _Color; float _ScanSpeed;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };
            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target {
                float scan = sin(i.uv.y * 50 + _Time.y * _ScanSpeed) * 0.5 + 0.5;
                return fixed4(_Color.rgb, _Color.a * scan);
            }
            ENDCG
        }
    }
}
