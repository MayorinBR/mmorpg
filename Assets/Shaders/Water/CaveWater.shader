Shader "Project/Environment/CaveWater"
{
    Properties
    {
        _DeepColor("Deep Color", Color) = (0.02, 0.15, 0.22, 1)
        _ShallowColor("Shallow Color", Color) = (0.2, 0.55, 0.65, 1)
        _FoamColor("Foam Color", Color) = (0.85, 0.95, 1, 1)
        _FlowSpeed("Flow Speed", Float) = 2.6
        _MinLight("Minimum Light", Range(0, 1)) = 0.35
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Off

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _DeepColor;
                float4 _ShallowColor;
                float4 _FoamColor;
                float _FlowSpeed;
                float _MinLight;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float light : TEXCOORD1;
                float fogFactor : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = pos.positionCS;
                output.positionWS = pos.positionWS;
                output.light = input.color.r;
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

            // Water that fills the cave and runs toward the lip, which is the -Z direction. The noise is stretched
            // along the flow and scrolls with time; vertex color red is how much light reaches that spot, so the
            // back of the cave fades into deep water.
            half4 frag(Varyings input) : SV_Target
            {
                float t = _Time.y;
                float2 w = float2(input.positionWS.x + input.positionWS.y * 0.7, input.positionWS.z);

                float broad = ValueNoise(float2(w.x * 0.45, (w.y + t * _FlowSpeed) * 0.2));
                float fine = ValueNoise(float2(w.x * 1.5 + 7.1, (w.y + t * _FlowSpeed * 1.6) * 0.4));
                float streak = smoothstep(0.55, 0.85, broad * 0.6 + fine * 0.5);

                float3 water = lerp(_DeepColor.rgb, _ShallowColor.rgb, broad);
                float3 color = lerp(water, _FoamColor.rgb, streak * 0.7);
                color *= lerp(_MinLight, 1.0, saturate(input.light));

                color = MixFog(color, input.fogFactor);
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
