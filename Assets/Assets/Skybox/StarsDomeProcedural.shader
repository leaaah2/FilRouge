Shader "CustomRenderTexture/StarsDomeProcedural"
{
Properties
    {
        _Color ("Tint", Color) = (1,1,1,1)
        _Density ("Star Density", Range(50, 2000)) = 500
        _Threshold ("Star Threshold", Range(0.8, 0.9999)) = 0.985
        _Brightness ("Brightness", Range(0, 5)) = 1.5
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Front
        ZWrite Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _Density;
            float _Threshold;
            float _Brightness;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float2 uv = i.uv * _Density;
                float2 cell = floor(uv);
                float2 local = frac(uv) - 0.5;

                float rnd = hash21(cell);
                float starMask = step(_Threshold, rnd);

                float2 starPos = float2(hash21(cell + 1.23), hash21(cell + 4.56)) - 0.5;
                float d = length(local - starPos * 0.7);

                float star = starMask * smoothstep(0.16, 0.02, d) * _Brightness;

                fixed4 col = _Color;
                star = saturate(star * 0.8);
			col.rgb *= star;
			col.a *= star;

                return col;
            }
            ENDCG
        }
    }
}
