Shader "Frontier/WindSway" {
    Properties { _Color(\"Color\", Color) = (1,1,1,1) _WindSpeed(\"Wind Speed\", Float) = 1.0 }
    SubShader {
        Tags { \"RenderType\"=\"Opaque\" }
        Pass {
            CGPROGRAM #pragma vertex vert #pragma fragment frag #include \"UnityCG.cginc\"
            fixed4 _Color; float _WindSpeed;
            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            struct v2f { float4 pos : SV_POSITION; fixed4 col : COLOR; };
            v2f vert(appdata v) {
                v2f o;
                float sway = sin(_Time.y * _WindSpeed + v.vertex.x) * 0.1;
                v.vertex.x += sway;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.col = _Color;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target { return i.col; }
            ENDCG
        }
    }
}
