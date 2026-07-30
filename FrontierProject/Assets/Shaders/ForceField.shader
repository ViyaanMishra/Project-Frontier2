Shader \"Frontier/ForceField\" {
    Properties { _Color(\"Color\", Color) = (0,1,1,0.5) _GridSize(\"Grid Size\", Float) = 0.5 }
    SubShader {
        Tags { \"RenderType\"=\"Transparent\" }
        Pass {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM #pragma vertex vert #pragma fragment frag #include \"UnityCG.cginc\"
            fixed4 _Color; float _GridSize;
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float3 worldPos : TEXCOORD0; };
            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target {
                float grid = fmod(i.worldPos.x, _GridSize) < 0.05 || fmod(i.worldPos.y, _GridSize) < 0.05;
                return grid ? _Color : fixed4(0,0,0,0);
            }
            ENDCG
        }
    }
}
