Shader "Project/Environment/WaterFoamRing"
{
    Properties
    {
        _FoamColor("Foam Color", Color) = (0.96, 0.99, 1, 0.95)
        _PoolColor("Pool Color", Color) = (0.03, 0.2, 0.27, 0.5)
        _Center("Impact Center (XZ)", Vector) = (0, 0, 0, 0)
        _Down("Downstream Direction (XZ)", Vector) = (0, -1, 0, 0)
        _CoreWidth("Core Half Width", Float) = 5
        _CoreLength("Core Half Length", Float) = 3.5
        _CoreShift("Core Shift Downstream", Float) = 1.5
        _TrailLength("Trail Length", Float) = 14
        _TrailSpeed("Trail Speed", Float) = 2.2
        _RingSpeed("Ring Speed", Float) = 0.7
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+1" "RenderPipeline" = "UniversalPipeline" }

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
                float4 _FoamColor;
                float4 _PoolColor;
                float4 _Center;
                float4 _Down;
                float _CoreWidth;
                float _CoreLength;
                float _CoreShift;
                float _TrailLength;
                float _TrailSpeed;
                float _RingSpeed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 positionXZ : TEXCOORD0;
                float fogFactor : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = pos.positionCS;
                output.positionXZ = pos.positionWS.xz;
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

            float Fbm(float2 p)
            {
                return ValueNoise(p) * 0.55 + ValueNoise(p * 2.1 + 5.3) * 0.3 + ValueNoise(p * 4.3 + 1.7) * 0.15;
            }

            // Foam of a plunge pool in world space. along grows downstream, across is the sideways offset from the
            // impact point. The core is a broken-up churning ellipse, the trail is a set of streaks stretched along
            // the flow that thin out downstream, and weak rings travel outward over a darker pool of deep water.
            half4 frag(Varyings input) : SV_Target
            {
                float t = _Time.y;
                float2 p = input.positionXZ - _Center.xy;
                float2 down = normalize(_Down.xy);
                float2 side = float2(-down.y, down.x);
                float along = dot(p, down);
                float across = dot(p, side);

                float churn = Fbm(p * 0.9 + float2(t * 0.45, -t * 0.6));
                float fine = ValueNoise(p * 3.1 - float2(t * 0.9, t * 0.7));

                float2 coreSpace = float2(across / _CoreWidth, (along - _CoreShift) / _CoreLength);
                float core = smoothstep(1.0, 0.25, length(coreSpace) + (churn - 0.5) * 0.9);
                core = saturate(core * (0.75 + fine * 0.5));

                float downstream = max(along, 0.0);
                float trailFade = exp(-downstream / (_TrailLength * 0.45)) * smoothstep(-2.0, 1.5, along);
                float lateralWidth = _CoreWidth * (0.85 + downstream * 0.025);
                float lateral = smoothstep(1.0, 0.25, abs(across) / lateralWidth);
                float streakNoise = ValueNoise(float2(across * 1.7, (along - t * _TrailSpeed) * 0.32));
                float streak = smoothstep(0.38, 0.78, streakNoise + (churn - 0.5) * 0.35);
                float trail = trailFade * lateral * streak * 0.9;

                float r = length(p);
                float ringWave = pow(saturate(sin((r * 0.9 - t * _RingSpeed) * 6.2831853) * 0.5 + 0.5), 4.0);
                float rings = ringWave * exp(-r / 7.0) * smoothstep(_CoreWidth * 0.5, _CoreWidth * 1.0, r) * 0.4;

                float foam = saturate(core + trail + rings);

                float2 poolSpace = float2(across / (_CoreWidth * 1.35), (along - _CoreShift * 1.4) / (_CoreLength * 2.4));
                float pool = smoothstep(1.0, 0.35, length(poolSpace) + (churn - 0.5) * 0.3);
                float poolAlpha = pool * _PoolColor.a;

                float3 foamColor = lerp(_FoamColor.rgb * 0.82, _FoamColor.rgb, saturate(churn * 1.4));
                float3 color = lerp(_PoolColor.rgb, foamColor, foam);
                float alpha = saturate(poolAlpha * (1.0 - foam) + foam * _FoamColor.a);

                color = MixFog(color, input.fogFactor);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
