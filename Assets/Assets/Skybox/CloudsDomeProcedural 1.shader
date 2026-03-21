Shader "CustomRenderTexture/CloudsDomeProcedural"
{
	Properties
    {
        _Color ("Tint", Color) = (1,1,1,1)
        _CloudScale ("Cloud Scale", Range(0.1, 10)) = 2
        _CloudThreshold ("Cloud Threshold", Range(0, 1)) = 0.45
        _Softness ("Softness", Range(0.001, 0.5)) = 0.18
        _Offset ("Offset", Vector) = (0,0,0,0)
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
            float _CloudScale;
            float _CloudThreshold;
            float _Softness;
            float4 _Offset;

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float3 objDir : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(a, b, u.x) +
                       (c - a) * u.y * (1.0 - u.x) +
                       (d - b) * u.x * u.y;
            }

            float fbm(float2 p)
            {
                float v = 0.0;
                float a = 0.5;

                v += noise(p) * a; p *= 2.02; a *= 0.5;
                v += noise(p) * a; p *= 2.03; a *= 0.5;
                v += noise(p) * a; p *= 2.01; a *= 0.5;
                v += noise(p) * a;

                return v;
            }

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.objDir = normalize(v.vertex.xyz);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // Convert sphere direction to stable spherical coordinates
                float3 d = normalize(i.objDir);

                float u = atan2(d.x, d.z) / (2.0 * UNITY_PI) + 0.5;
                float v = d.y * 0.5 + 0.5;

                float2 p = float2(u, v) * _CloudScale + _Offset.xy;

                float n = fbm(p);

                float alpha = smoothstep(_CloudThreshold - _Softness, _CloudThreshold + _Softness, n);

                fixed4 col = _Color;
                col.a *= alpha;

                return col;
            }
            ENDCG
        }
    }
}
