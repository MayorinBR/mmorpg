Shader "Project/Environment/PoolWater"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0.05, 0.28, 0.5, 0.86)
        _StreakColor("Streak Color", Color) = (0.78, 0.93, 1, 1)
        _Down("Flow Direction (XZ)", Vector) = (0, -1, 0, 0)
        _TileSize("Pattern Tile Size (m)", Float) = 2.5
        _FlowSpeed("Flow Speed (m/s)", Float) = 0.9
        _StreakAmount("Streak Amount", Range(0, 1)) = 0.35
        _ShoreFade("Shore Fade Height (m)", Float) = 0.35
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _StreakColor;
                float4 _Down;
                float _TileSize;
                float _FlowSpeed;
                float _StreakAmount;
                float _ShoreFade;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float fogFactor : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = pos.positionCS;
                output.positionWS = pos.positionWS;
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

            float FlowPattern(float2 p, float2 dir)
            {
                float2 perp = float2(-dir.y, dir.x);
                float2 q = float2(dot(p, dir) * 0.45, dot(p, perp) * 1.6);
                return ValueNoise(q) * 0.65 + ValueNoise(q * 2.3 + 7.1) * 0.35;
            }

            // Same look as the river, with one fixed flow direction. The water is faded where the terrain is less than
            // _ShoreFade meters below it, so the shoreline follows the real ground instead of the mesh cells.
            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.positionCS.xy / _ScaledScreenParams.xy;
                float rawDepth = SampleSceneDepth(uv);
                float3 groundWS = ComputeWorldSpacePosition(uv, rawDepth, UNITY_MATRIX_I_VP);
                float shore = saturate((input.positionWS.y - groundWS.y) / max(_ShoreFade, 0.001));

                float2 dir = normalize(_Down.xy);
                const float cycle = 2.0;
                float phase0 = frac(_Time.y / cycle);
                float phase1 = frac(phase0 + 0.5);
                float weight0 = 1.0 - abs(1.0 - 2.0 * phase0);
                float2 baseUv = input.positionWS.xz / _TileSize;
                float n0 = FlowPattern(baseUv - dir * (_FlowSpeed * cycle * phase0 / _TileSize), dir);
                float n1 = FlowPattern(baseUv - dir * (_FlowSpeed * cycle * phase1 / _TileSize) + float2(0.37, 0.61), dir);
                float n = lerp(n1, n0, weight0);

                float streak = smoothstep(0.55, 0.8, n) * _StreakAmount;
                float3 color = lerp(_BaseColor.rgb, _StreakColor.rgb, saturate(streak));
                float alpha = saturate(_BaseColor.a + streak * 0.1) * shore;

                color = MixFog(color, input.fogFactor);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
