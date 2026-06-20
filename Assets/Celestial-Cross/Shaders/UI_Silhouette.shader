Shader "UI/Silhouette"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _SilhouetteColor ("Silhouette Color", Color) = (0.05, 0.02, 0.08, 1)
        _EdgeGlow ("Edge Glow Intensity", Range(0, 5)) = 0
        _EdgeGlowColor ("Edge Glow Color", Color) = (1, 0.8, 0, 1)
        _RevealProgress ("Reveal Progress", Range(0, 1)) = 0
        _AlphaThreshold ("Alpha Threshold", Range(0, 1)) = 0.05

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            fixed4 _SilhouetteColor;
            float _EdgeGlow;
            fixed4 _EdgeGlowColor;
            float _RevealProgress;
            float _AlphaThreshold;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = v.texcoord;

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                // Discard transparent pixels
                if (color.a < _AlphaThreshold)
                {
                    discard;
                }

                // If fully revealed, output original color
                if (_RevealProgress >= 1.0)
                {
                    return color;
                }

                // 4-tap sampling for edge detection
                float2 uv = IN.texcoord;
                float2 tx = _MainTex_TexelSize.xy;
                
                half alphaUp = tex2D(_MainTex, uv + float2(0, tx.y)).a;
                half alphaDown = tex2D(_MainTex, uv + float2(0, -tx.y)).a;
                half alphaLeft = tex2D(_MainTex, uv + float2(-tx.x, 0)).a;
                half alphaRight = tex2D(_MainTex, uv + float2(tx.x, 0)).a;

                bool isEdge = (alphaUp < _AlphaThreshold) || (alphaDown < _AlphaThreshold) || 
                              (alphaLeft < _AlphaThreshold) || (alphaRight < _AlphaThreshold);

                fixed4 finalColor = _SilhouetteColor;

                if (isEdge && _EdgeGlow > 0)
                {
                    // Add glow to edges
                    finalColor.rgb += _EdgeGlowColor.rgb * _EdgeGlow;
                }

                // Mix silhouette with original color based on reveal progress
                finalColor = lerp(finalColor, color, _RevealProgress);
                
                // Keep the original alpha for anti-aliased edges
                finalColor.a = color.a;

                return finalColor;
            }
        ENDCG
        }
    }
}
