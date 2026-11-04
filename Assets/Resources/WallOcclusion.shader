Shader "SunNight/Wall Occlusion"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}
        _MaskTex("Mask", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        [HideInInspector] _Color("Tint", Color) = (1,1,1,1)
        [HideInInspector] _ZWrite("ZWrite", Float) = 0
        [HideInInspector] _FadeArea("Center XY, radii ZW", Vector) = (0,0,1,1)
        [HideInInspector] _FadeAmount("Fade", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite [_ZWrite]
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"
            struct Attributes { COMMON_2D_INPUTS half4 color : COLOR; };
            struct Varyings { COMMON_2D_LIT_OUTPUTS half4 color : COLOR; float2 worldXY : TEXCOORD4; };
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Lit2DCommon.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float4 _FadeArea;
                float _FadeAmount;
            CBUFFER_END
            Varyings Vert(Attributes v)
            {
                SetUpSpriteInstanceProperties();
                v.positionOS = UnityFlipSprite(v.positionOS, unity_SpriteProps.xy);
                Varyings o = CommonLitVertex(v);
                o.color = v.color * _Color * unity_SpriteColor;
                o.worldXY = TransformObjectToWorld(v.positionOS).xy;
                return o;
            }
            half4 Frag(Varyings i) : SV_Target
            {
                float distanceToPlayer = length((i.worldXY - _FadeArea.xy) / _FadeArea.zw);
                i.color.a *= 1 - _FadeAmount * (1 - smoothstep(0.45, 1.0, distanceToPlayer));
                return CommonLitFragment(i, i.color);
            }
            ENDHLSL
        }
    }
}
