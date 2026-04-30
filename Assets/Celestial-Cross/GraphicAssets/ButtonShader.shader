Shader "UI/SpriteWithPattern"
{
    Properties
    {
        _MainTex ("Sprite Shape", 2D) = "white" {}
        _PatternTex ("Pattern Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _PatternScale ("Pattern Scale", Float) = 5
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
            };

            sampler2D _MainTex;
            sampler2D _PatternTex;

            float4 _MainTex_ST;
            float _PatternScale;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
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

                return final;
            }
            ENDCG
        }
    }
}