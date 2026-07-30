Shader \"Frontier/ToonOutline\" {
    Properties { _Color(\"Color\", Color) = (1,1,1,1) _OutlineWidth(\"Outline Width\", Float) = 0.02 }
    SubShader {
        Pass {
            Name \"OUTLINE\"
            Cull Front
            CGPROGRAM #pragma vertex vert #pragma fragment frag #include \"UnityCG.cginc\"
            float _OutlineWidth;
            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            struct v2f { float4 pos : SV_POSITION; };
            v2f vert(appdata v) {
                v2f o;
                v.vertex.xyz += v.normal * _OutlineWidth;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            fixed4 frag(v2f i) : SV_Target { return fixed4(0,0,0,1); }
            ENDCG
        }
        Pass {
            Name \"MAIN\"
            CGPROGRAM #pragma vertex vert #pragma fragment frag #include \"UnityCG.cginc\"
            fixed4 _Color;
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; fixed4 col : COLOR; };
            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.col = _Color;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target { return i.col; }
            ENDCG
        }
    }
}
