Shader "UI/ScrollHoleMask"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture (Fundo Animado)", 2D) = "white" {}
        _MaskTex ("Static Mask (Black = Hole)", 2D) = "white" {}
        
        _DetailTex ("Textura Adicional (Ex: Papel)", 2D) = "white" {}
        _DetailColor ("Cor da Textura Adicional", Color) = (1,1,1,1)

        _ScrollSpeedX ("Scroll Speed X", Float) = 0.1
        _ScrollSpeedY ("Scroll Speed Y", Float) = 0.1
        
        _Color ("Tint", Color) = (1,1,1,1)

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
                float2 staticUV : TEXCOORD0;
                float2 scrollUV : TEXCOORD1;
                float4 worldPosition : TEXCOORD8;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;
            sampler2D _DetailTex;
            
            fixed4 _DetailColor;
            fixed4 _Color;
            float4 _TextureSampleAdd;
            float4 _ClipRect;
            
            float _ScrollSpeedX;
            float _ScrollSpeedY;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                
                // UV para a máscara estática e textura estática
                OUT.staticUV = v.texcoord;
                
                // UV que move no tempo para o fundo
                OUT.scrollUV = v.texcoord + float2(_Time.y * _ScrollSpeedX, _Time.y * _ScrollSpeedY);
                
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Lendo o fundo animado
                half4 color = (tex2D(_MainTex, IN.scrollUV) + _TextureSampleAdd) * IN.color;
                
                // Lendo a textura adicional por cima (Multiplicação para misturar bem com texturas brancas)
                half4 detailColor = tex2D(_DetailTex, IN.staticUV) * _DetailColor;
                color.rgb *= detailColor.rgb;
                
                // Lendo o canal A da textura de máscara estática para furar tudo
                half4 mask = tex2D(_MaskTex, IN.staticUV);
                color.a *= mask.a;
                
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
