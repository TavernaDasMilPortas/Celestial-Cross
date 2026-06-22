Shader "UI/TutorialSpotlight"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0,0,0,0.7)
        
        _HoleCenter ("Hole Center", Vector) = (0.5, 0.5, 0, 0)
        _HoleSize ("Hole Size", Vector) = (0.2, 0.2, 0, 0)
        _Feather ("Feather", Range(0, 0.1)) = 0.01
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
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
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                half4 color     : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            half4 _Color;
            float4 _HoleCenter; // xy = center in screen pixels, zw = unused
            float4 _HoleSize;   // xy = width/height in pixels
            float _Feather;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // Calcula distância para o centro do buraco em coordenadas de pixel (simplificado para círculo)
                // i.vertex.xy está em coordenadas de pixel na tela (Screen Space)
                float2 pixelPos = i.vertex.xy;
                float2 center = _HoleCenter.xy;
                
                // Formato retangular arredondado ou circular
                float2 d = abs(pixelPos - center) - _HoleSize.xy * 0.5;
                float dist = length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
                
                float mask = smoothstep(0, _Feather * 100.0, dist);
                
                half4 color = i.color;
                color.a *= mask;
                
                return color;
            }
            ENDHLSL
        }
    }
}
