Shader "Custom/URP/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineThickness ("Outline Thickness", Range(0, 10)) = 0
        _FlashColor ("Flash Color", Color) = (1,1,1,0)
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _Color;
            float4 _RendererColor;
            float4 _OutlineColor;
            float _OutlineThickness;
            float4 _FlashColor;

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color * _Color * _RendererColor;

                #ifdef PIXELSNAP_ON
                output.positionCS = floor(output.positionCS * _ScreenParams.xy) / _ScreenParams.xy;
                #endif

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float4 c = tex2D(_MainTex, input.uv) * input.color;
                
                // Outline Logic
                float outlineAlpha = 0;
                if (_OutlineThickness > 0 && c.a <= 0.05)
                {
                    float2 up = float2(0, _MainTex_TexelSize.y) * _OutlineThickness;
                    float2 right = float2(_MainTex_TexelSize.x, 0) * _OutlineThickness;
                    
                    outlineAlpha += tex2D(_MainTex, input.uv + up).a;
                    outlineAlpha += tex2D(_MainTex, input.uv - up).a;
                    outlineAlpha += tex2D(_MainTex, input.uv + right).a;
                    outlineAlpha += tex2D(_MainTex, input.uv - right).a;
                    outlineAlpha += tex2D(_MainTex, input.uv + up + right).a;
                    outlineAlpha += tex2D(_MainTex, input.uv - up - right).a;
                    outlineAlpha += tex2D(_MainTex, input.uv + up - right).a;
                    outlineAlpha += tex2D(_MainTex, input.uv - up + right).a;
                    
                    outlineAlpha = saturate(outlineAlpha);
                    if (outlineAlpha > 0)
                    {
                        // Se não tem alfa no centro e tem outline ao redor
                        return _OutlineColor * outlineAlpha;
                    }
                }
                
                // Flash Logic
                if (c.a > 0 && _FlashColor.a > 0)
                {
                    c.rgb = lerp(c.rgb, _FlashColor.rgb, _FlashColor.a);
                }

                c.rgb *= c.a; // Premultiply alpha for Blend One OneMinusSrcAlpha
                return c;
            }
            ENDHLSL
        }
    }
}
