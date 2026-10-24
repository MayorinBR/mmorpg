Shader "Project/Environment/DungeonPainted"
{
    Properties
    {
        [MainTexture] _BaseMap("Palette", 2D) = "white" {}
        _CategoryMap("Category Map", 2D) = "black" {}
        _PatternArray("Pattern Array", 2DArray) = "" {}
        _TileSize("Tile Size (m)", Float) = 1
        _PatternGain("Pattern Gain", Range(0.5, 1.5)) = 1.15
        _PatternStrength("Pattern Strength", Range(0, 1)) = 1
        _GroundGradientHeight("Ground Gradient Height", Float) = 1.2
        _GroundGradientDarkness("Ground Gradient Darkness", Range(0, 1)) = 0.12
        _EmissionStrength("Emission Strength", Range(0, 4)) = 1.6
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_BaseMap);
        TEXTURE2D(_CategoryMap);
        TEXTURE2D_ARRAY(_PatternArray);
        SAMPLER(sampler_BaseMap);
        SAMPLER(sampler_PatternArray);
        SamplerState sampler_point_clamp;

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float _TileSize;
            float _PatternGain;
            float _PatternStrength;
            float _GroundGradientHeight;
            float _GroundGradientDarkness;
            float _EmissionStrength;
        CBUFFER_END

        static const float kSliceScale[8] = { 3.0, 2.8, 2.0, 4.0, 1.5, 2.0, 5.0, 4.0 };
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float heightOS : TEXCOORD3;
                float fogFactor : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = pos.positionCS;
                output.positionWS = pos.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.heightOS = input.positionOS.y;
                output.fogFactor = ComputeFogFactor(pos.positionCS.z);
                return output;
            }

            float SamplePattern(float3 positionWS, float3 normalWS, float slice)
            {
                float size = max(_TileSize, 0.01) * kSliceScale[(int)slice];
                float3 w = pow(abs(normalWS), 4.0);
                w /= max(w.x + w.y + w.z, 1e-4);
                float3 p = positionWS / size;
                float x = SAMPLE_TEXTURE2D_ARRAY(_PatternArray, sampler_PatternArray, p.zy, slice).r;
                float y = SAMPLE_TEXTURE2D_ARRAY(_PatternArray, sampler_PatternArray, p.xz, slice).r;
                float z = SAMPLE_TEXTURE2D_ARRAY(_PatternArray, sampler_PatternArray, p.xy, slice).r;
                return x * w.x + y * w.y + z * w.z;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float3 normalWS = normalize(input.normalWS);

                half3 palette = SAMPLE_TEXTURE2D(_BaseMap, sampler_point_clamp, input.uv).rgb;
                half4 category = SAMPLE_TEXTURE2D(_CategoryMap, sampler_point_clamp, input.uv);
                float slice = floor(category.r * 255.0 + 0.5);
                float pattern = SamplePattern(input.positionWS, normalWS, slice);
                pattern = lerp(1.0, saturate(pattern * _PatternGain), _PatternStrength);
                float ground = 1.0 - _GroundGradientDarkness * (1.0 - smoothstep(0.0, _GroundGradientHeight, input.heightOS));

                SurfaceData surface = (SurfaceData)0;
                surface.albedo = palette * pattern * ground;
                surface.emission = palette * category.g * _EmissionStrength;
                surface.alpha = 1.0;
                surface.smoothness = 0.0;
                surface.occlusion = 1.0;
                surface.normalTS = half3(0, 0, 1);

                InputData data = (InputData)0;
                data.positionWS = input.positionWS;
                data.normalWS = normalWS;
                data.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                data.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                data.fogCoord = input.fogFactor;
                data.bakedGI = SampleSH(normalWS);
                data.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                data.shadowMask = half4(1, 1, 1, 1);

                half4 color = UniversalFragmentPBR(data, surface);
                color.rgb = MixFog(color.rgb, input.fogFactor);
                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirectionWS = normalize(_LightPosition - positionWS);
            #else
                float3 lightDirectionWS = _LightDirection;
            #endif
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
            #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #else
                positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #endif
                output.positionCS = positionCS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            struct Attributes { float4 positionOS : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half frag(Varyings input) : SV_Target { return input.positionCS.z; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float3 normalWS : TEXCOORD0; };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return half4(NormalizeNormalPerPixel(input.normalWS), 0.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
