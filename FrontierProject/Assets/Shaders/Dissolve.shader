Shader "Frontier/Dissolve\" {
    Properties { _Color(\"Color\", Color) = (1,1,1,1) _DissolveAmount(\"Dissolve\", Range(0,1)) = 0 }
    SubShader {
        Tags { \"RenderType\"=\"TransparentCutout\" }
        Pass {
            CGPROGRAM #pragma vertex vert #pragma fragment frag #include \"UnityCG.cginc\"
            fixed4 _Color; float _DissolveAmount;
            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            struct v2f { float4 pos : SV_POSITION; fixed4 col : COLOR; float3 worldPos : TEXCOORD0; };
            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.col = _Color;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target {
                if (i.worldPos.y < _DissolveAmount) discard;
                return i.col;
            }
            ENDCG
        }
    }
}
