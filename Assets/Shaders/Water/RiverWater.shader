Shader "Project/Environment/RiverWater"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0.05, 0.28, 0.5, 0.86)
        _StreakColor("Streak Color", Color) = (0.78, 0.93, 1, 1)
        [NoScaleOffset] _FlowMap("Flow Map (RG direction, B speed, A water)", 2D) = "gray" {}
        _FlowArea("Flow Area (x0, z0, sizeX, sizeZ)", Vector) = (-100, -100, 200, 200)
        _TileSize("Pattern Tile Size (m)", Float) = 2.5
        _FlowSpeed("Flow Speed (m/s)", Float) = 1.6
        _MinSpeed("Minimum Speed Factor", Range(0, 1)) = 0.3
        _StreakAmount("Streak Amount", Range(0, 1)) = 0.4
        _FoamAmount("Rapids Foam Amount", Range(0, 1)) = 0.7
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
            Cull Back

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_FlowMap);
            SAMPLER(sampler_FlowMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _StreakColor;
                float4 _FlowArea;
                float _TileSize;
                float _FlowSpeed;
                float _MinSpeed;
                float _StreakAmount;
                float _FoamAmount;
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

            // Noise stretched along the flow direction so the highlights read as streaks of moving water.
            float FlowPattern(float2 p, float2 dir)
            {
                float2 perp = float2(-dir.y, dir.x);
                float2 q = float2(dot(p, dir) * 0.45, dot(p, perp) * 1.6);
                return ValueNoise(q) * 0.65 + ValueNoise(q * 2.3 + 7.1) * 0.35;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 xz = input.positionWS.xz;
                float2 flowUv = (xz - _FlowArea.xy) / _FlowArea.zw;
                half4 flow = SAMPLE_TEXTURE2D(_FlowMap, sampler_FlowMap, flowUv);

                float2 raw = flow.rg * 2.0 - 1.0;
                float strength = saturate(length(raw));
                float2 dir = strength > 1e-3 ? normalize(raw) : float2(1, 0);
                float speed01 = flow.b;
                float speed = _FlowSpeed * lerp(_MinSpeed, 1.0, speed01) * strength;

                // Two layers advected with a half cycle offset and cross faded, so the reset of each layer is hidden.
                const float cycle = 2.0;
                float phase0 = frac(_Time.y / cycle);
                float phase1 = frac(phase0 + 0.5);
                float weight0 = 1.0 - abs(1.0 - 2.0 * phase0);

                float2 baseUv = xz / _TileSize;
                float n0 = FlowPattern(baseUv - dir * (speed * cycle * phase0 / _TileSize), dir);
                float n1 = FlowPattern(baseUv - dir * (speed * cycle * phase1 / _TileSize) + float2(0.37, 0.61), dir);
                float n = lerp(n1, n0, weight0);

                float streak = smoothstep(0.55, 0.8, n) * _StreakAmount * lerp(0.5, 1.0, speed01) * strength;
                float foam = smoothstep(0.6, 0.85, n) * smoothstep(0.4, 0.9, speed01) * _FoamAmount * strength;

                float3 color = lerp(_BaseColor.rgb, _StreakColor.rgb, saturate(streak + foam));
                float alpha = saturate(_BaseColor.a + foam * (1.0 - _BaseColor.a) + streak * 0.1);

                color = MixFog(color, input.fogFactor);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
