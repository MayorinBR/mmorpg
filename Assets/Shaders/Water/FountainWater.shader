Shader "Project/Environment/FountainWater"
{
    Properties
    {
        _ShallowColor("Shallow Color", Color) = (0.62, 0.86, 0.97, 0.85)
        _DeepColor("Deep Color", Color) = (0.25, 0.58, 0.86, 0.92)
        _FoamColor("Foam Color", Color) = (1, 1, 1, 1)
        _FlowMode("Flow Mode (0 surface, 1 falling sheet)", Range(0, 1)) = 0
        _PatternScale("Pattern Scale", Float) = 1.4
        _Speed("Speed", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

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
                float4 _ShallowColor;
                float4 _DeepColor;
                float4 _FoamColor;
                float _FlowMode;
                float _PatternScale;
                float _Speed;
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
                float3 positionWS : TEXCOORD1;
                float4 color : COLOR;
                float fogFactor : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = pos.positionCS;
                output.positionWS = pos.positionWS;
                output.uv = input.uv;
                output.color = input.color;
                output.fogFactor = ComputeFogFactor(pos.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float t = _Time.y * _Speed;
                float3 color;
                float alpha;

                if (_FlowMode > 0.5)
                {
                    float u = input.uv.x;
                    float scroll = input.uv.y * 1.6 - t * 2.4;
                    float a = sin(u * 6.2831853 + sin(scroll * 1.7) * 1.6) * 0.5 + 0.5;
                    float b = sin(u * 3.1415927 - scroll * 0.9 + 1.7) * 0.5 + 0.5;
                    float streak = smoothstep(0.55, 1.0, a * 0.6 + b * 0.5);
                    float gate = smoothstep(0.05, 0.45, sin(u * 4.712389 + sin(scroll * 0.6) * 0.8 + 0.6) * 0.5 + 0.5);
                    color = lerp(_ShallowColor.rgb, _FoamColor.rgb, streak * 0.7);
                    color = lerp(color, _FoamColor.rgb, input.color.g);
                    alpha = _ShallowColor.a * input.color.a * lerp(0.35, 1.0, gate) * (0.7 + 0.3 * streak);
                    alpha = saturate(alpha + input.color.g * 0.45);
                }
                else
                {
                    float2 p = input.positionWS.xz * _PatternScale;
                    float r = sin(p.x * 2.7 + t * 1.3) * sin(p.y * 2.3 - t * 1.1) + sin((p.x + p.y) * 1.9 + t * 0.8) * 0.6;
                    r = saturate(r * 0.3 + 0.5);
                    color = lerp(_DeepColor.rgb, _ShallowColor.rgb, r);
                    color += smoothstep(0.78, 0.95, r) * 0.25;
                    alpha = lerp(_DeepColor.a, _ShallowColor.a, r);
                }

                color = MixFog(color, input.fogFactor);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
