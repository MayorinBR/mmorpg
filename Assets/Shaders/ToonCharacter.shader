Shader "Project/Characters/ToonCharacter"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        [NoScaleOffset] _SkinMask ("Skin Mask", 2D) = "black" {}
        _SkinColor ("Skin Color", Color) = (0.94, 0.82, 0.78, 1)
        _ShadowColor ("Shadow Color", Color) = (0.62, 0.58, 0.72, 1)
        _ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.5
        _ShadowSoftness ("Shadow Softness", Range(0.001, 0.5)) = 0.02
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower ("Rim Power", Range(0.5, 8)) = 3
        _RimStrength ("Rim Strength", Range(0, 1)) = 0.25
        _OutlineColor ("Outline Color", Color) = (0.12, 0.08, 0.1, 1)
        _OutlineWidth ("Outline Width (pixels)", Range(0, 6)) = 1.5

        [NoScaleOffset] _EyeArray ("Eye Layers", 2DArray) = "" {}
        [NoScaleOffset] _EyeIrisArray ("Eye Iris Mask Layers", 2DArray) = "" {}
        _EyeLayout ("Eye Layout (scale xy, offset xy)", Vector) = (0.52513, 0.52513, 0.23577, 0.24833)
        _EyeOffset ("Eye UV Offset", Vector) = (0, 0, 0, 0)
        _EyeFrame ("Eye Layer Index", Float) = 0
        _IrisColor ("Iris Color", Color) = (0.45, 0.28, 0.15, 1)
        [NoScaleOffset] _MouthArray ("Mouth Layers", 2DArray) = "" {}
        _MouthLayout ("Mouth Layout (scale xy, offset xy)", Vector) = (0.52513, 0.52513, 0.23743, 0.39948)
        _MouthOffset ("Mouth UV Offset", Vector) = (0, 0, 0, 0)
        _MouthFrame ("Mouth Layer Index", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        TEXTURE2D(_SkinMask);
        SAMPLER(sampler_SkinMask);
        TEXTURE2D_ARRAY(_EyeArray);
        SAMPLER(sampler_EyeArray);
        TEXTURE2D_ARRAY(_EyeIrisArray);
        SAMPLER(sampler_EyeIrisArray);
        TEXTURE2D_ARRAY(_MouthArray);
        SAMPLER(sampler_MouthArray);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _SkinColor;
            half4 _ShadowColor;
            half _ShadowThreshold;
            half _ShadowSoftness;
            half4 _RimColor;
            half _RimPower;
            half _RimStrength;
            half4 _OutlineColor;
            half _OutlineWidth;
            float4 _EyeLayout;
            float4 _EyeOffset;
            float _EyeFrame;
            half4 _IrisColor;
            float4 _MouthLayout;
            float4 _MouthOffset;
            float _MouthFrame;
        CBUFFER_END

        // Maps the face UV (UV1) into a face layer. A layer is a whole sprite canvas; the layout
        // (scale xy, offset xy) says which part of the canvas covers the face region, so sprites
        // can be authored at their natural size. Returns 0 outside the face region or the canvas.
        half FaceLayerUv(float4 layout, float2 faceUv, out float2 layerUv)
        {
            float2 uv = faceUv * layout.xy + layout.zw;
            float2 inFace = step(0.0, faceUv) * step(faceUv, 1.0);
            float2 inCanvas = step(0.0, uv) * step(uv, 1.0);
            layerUv = clamp(uv, 0.001, 0.999);
            return inFace.x * inFace.y * inCanvas.x * inCanvas.y;
        }

        // Draws one layer of the mouth array over the base color.
        half3 ApplyMouthLayer(half3 color, float2 faceUv, half mask)
        {
            float2 uv;
            half inside = FaceLayerUv(_MouthLayout, faceUv + _MouthOffset.xy, uv);
            half4 texel = SAMPLE_TEXTURE2D_ARRAY(_MouthArray, sampler_MouthArray, uv, floor(_MouthFrame + 0.5));
            half alpha = texel.a * inside * smoothstep(0.5h, 1.0h, mask);
            return lerp(color, texel.rgb, alpha);
        }

        // Draws one layer of the eye array. The layer holds a white sclera and black line art;
        // the iris mask layer marks the pixels that take _IrisColor, so the sclera stays white
        // for any iris color.
        half3 ApplyEyeLayer(half3 color, float2 faceUv, half mask)
        {
            float2 uv;
            half inside = FaceLayerUv(_EyeLayout, faceUv + _EyeOffset.xy, uv);
            float layer = floor(_EyeFrame + 0.5);
            half4 texel = SAMPLE_TEXTURE2D_ARRAY(_EyeArray, sampler_EyeArray, uv, layer);
            half iris = SAMPLE_TEXTURE2D_ARRAY(_EyeIrisArray, sampler_EyeIrisArray, uv, layer).r;
            half alpha = texel.a * inside * smoothstep(0.5h, 1.0h, mask);
            return lerp(color, texel.rgb * lerp(half3(1, 1, 1), _IrisColor.rgb, iris), alpha);
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float2 faceUv : TEXCOORD1;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float2 faceUv : TEXCOORD3;
                half faceMask : TEXCOORD4;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = position.positionCS;
                output.positionWS = position.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.faceUv = input.faceUv;
                output.faceMask = saturate(input.color.g - input.color.r - input.color.b) * input.color.a;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half3 normalWS = normalize(input.normalWS);
                half3 viewDirWS = normalize(GetWorldSpaceViewDir(input.positionWS));
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                half skinMask = SAMPLE_TEXTURE2D(_SkinMask, sampler_SkinMask, input.uv).r;
                albedo.rgb = lerp(albedo.rgb, _SkinColor.rgb, skinMask);
                albedo.rgb = ApplyEyeLayer(albedo.rgb, input.faceUv, input.faceMask);
                albedo.rgb = ApplyMouthLayer(albedo.rgb, input.faceUv, input.faceMask);

                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half halfLambert = dot(normalWS, mainLight.direction) * 0.5h + 0.5h;
                half band = smoothstep(_ShadowThreshold - _ShadowSoftness, _ShadowThreshold + _ShadowSoftness, halfLambert);
                band *= mainLight.shadowAttenuation;

                half3 lit = albedo.rgb;
                half3 shaded = albedo.rgb * _ShadowColor.rgb;
                half3 color = lerp(shaded, lit, band) * mainLight.color;

                half rim = pow(1.0h - saturate(dot(normalWS, viewDirWS)), _RimPower);
                color += _RimColor.rgb * rim * _RimStrength * band;

                return half4(color, 1.0h);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float4 positionCS = TransformObjectToHClip(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float2 normalCS = mul((float3x3)UNITY_MATRIX_VP, normalWS).xy;
                float2 direction = normalCS / max(length(normalCS), 1e-4);
                positionCS.xy += direction * (_OutlineWidth * 2.0 / _ScreenParams.xy) * positionCS.w;
                output.positionCS = positionCS;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return _OutlineColor;
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
            Cull Back

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            float4 Vert(Attributes input) : SV_POSITION
            {
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
                return positionCS;
            }

            half4 Frag(float4 positionCS : SV_POSITION) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R
            Cull Back

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag

            float4 Vert(float4 positionOS : POSITION) : SV_POSITION
            {
                return TransformObjectToHClip(positionOS.xyz);
            }

            half4 Frag(float4 positionCS : SV_POSITION) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }

    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}
