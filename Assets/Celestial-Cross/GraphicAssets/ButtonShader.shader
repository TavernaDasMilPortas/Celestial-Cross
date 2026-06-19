Shader "UI/SpriteWithPattern"
{
    Properties
    {
        _MainTex ("Sprite Shape", 2D) = "white" {}
        _PatternTex ("Pattern Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _PatternScale ("Pattern Scale", Float) = 5
        
        // Propriedades obrigatórias para UI Masking
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        [HideInInspector] _ClipRect ("Clip Rect", Vector) = (-32767, -32767, 32767, 32767)
        [HideInInspector] _UIMaskSoftnessX ("UI Mask Softness X", Float) = 0
        [HideInInspector] _UIMaskSoftnessY ("UI Mask Softness Y", Float) = 0
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
        ColorMask [_ColorMask]

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _PatternTex;

            float4 _MainTex_ST;
            float _PatternScale;
            fixed4 _Color;
            float4 _ClipRect;

            v2f vert (appdata v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv,_MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 shape = tex2D(_MainTex, i.uv);

                float2 patternUV = i.uv * _PatternScale;
                fixed4 pattern = tex2D(_PatternTex, patternUV);

                fixed4 final = pattern * shape * _Color * i.color;
                
                // Aplica a máscara do RectMask2D
                final.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);

                return final;
            }
            ENDCG
        }
    }
}