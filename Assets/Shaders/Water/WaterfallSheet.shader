Shader "Project/Environment/WaterfallSheet"
{
    Properties
    {
        _Color("Water Color", Color) = (0.72, 0.88, 0.98, 0.9)
        _FoamColor("Foam Color", Color) = (1, 1, 1, 1)
        _CrossFrequency("Cross Frequency (cells across)", Float) = 9
        _AlongFrequency("Along Frequency (cells per second of fall)", Float) = 3.2
        _Scroll("Scroll (seconds of fall per second)", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+2" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _FoamColor;
                float _CrossFrequency;
                float _AlongFrequency;
                float _Scroll;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float fogFactor : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = pos.positionCS;
                output.uv = input.uv;
                output.color = input.color;
                output.fogFactor = ComputeFogFactor(pos.positionCS.z);
                return output;
            }

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = Hash21(i);
                float b = Hash21(i + float2(1, 0));
                float c = Hash21(i + float2(0, 1));
                float d = Hash21(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // uv.x runs across the sheet and uv.y is the time of fall in seconds. Features scroll along uv.y, so they
            // move at the real speed of the water: slow near the lip and fast near the base, where they also stretch.
            half4 frag(Varyings input) : SV_Target
            {
                float t = _Time.y * _Scroll;
                float wobble = (ValueNoise(float2(input.uv.y * 1.7 - t * 0.8, 3.1)) - 0.5) * 0.8;

                float2 p0 = float2(input.uv.x * _CrossFrequency + wobble, (input.uv.y - t) * _AlongFrequency);
                float2 p1 = float2(input.uv.x * _CrossFrequency * 2.1 + 4.0, (input.uv.y - t * 1.35) * _AlongFrequency * 1.7);
                float n = ValueNoise(p0) * 0.6 + ValueNoise(p1) * 0.4;

                float streak = smoothstep(0.5, 0.85, n);
                float edge = min(input.uv.x, 1.0 - input.uv.x) + (n - 0.5) * 0.12;
                float edgeFade = smoothstep(0.02, 0.22, edge);

                float aeration = input.color.g;
                float3 color = lerp(_Color.rgb, _FoamColor.rgb, saturate(streak * 0.8 + aeration * 0.45));
                float alpha = _Color.a * edgeFade * input.color.a * lerp(0.5, 1.0, n);

                color = MixFog(color, input.fogFactor);
                return half4(color, saturate(alpha));
            }
            ENDHLSL
        }
    }
    FallBack Off
}
