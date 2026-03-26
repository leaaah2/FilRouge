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

            float hash31(float3 p)
            {
                p = frac(p * 0.1031);
                p += dot(p, p.yzx + 33.33);
                return frac((p.x + p.y) * p.z);
            }

            float noise3(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);

                float n000 = hash31(i + float3(0,0,0));
                float n100 = hash31(i + float3(1,0,0));
                float n010 = hash31(i + float3(0,1,0));
                float n110 = hash31(i + float3(1,1,0));
                float n001 = hash31(i + float3(0,0,1));
                float n101 = hash31(i + float3(1,0,1));
                float n011 = hash31(i + float3(0,1,1));
                float n111 = hash31(i + float3(1,1,1));

                float3 u = f * f * (3.0 - 2.0 * f);

                float nx00 = lerp(n000, n100, u.x);
                float nx10 = lerp(n010, n110, u.x);
                float nx01 = lerp(n001, n101, u.x);
                float nx11 = lerp(n011, n111, u.x);

                float nxy0 = lerp(nx00, nx10, u.y);
                float nxy1 = lerp(nx01, nx11, u.y);

                return lerp(nxy0, nxy1, u.z);
            }

            float fbm3(float3 p)
            {
                float v = 0.0;
                float a = 0.5;

                v += noise3(p) * a; p *= 2.02; a *= 0.5;
                v += noise3(p) * a; p *= 2.03; a *= 0.5;
                v += noise3(p) * a; p *= 2.01; a *= 0.5;
                v += noise3(p) * a;

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

                float3 d = normalize(i.objDir);

                // Seamless on a sphere
                float3 p = d * _CloudScale + _Offset.xyz;

                float n = fbm3(p);

                float alpha = smoothstep(
                    _CloudThreshold - _Softness,
                    _CloudThreshold + _Softness,
                    n
                );

                fixed4 col = _Color;
                col.a *= alpha;
                return col;
            }
            ENDCG
        }
    }
}