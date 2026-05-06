Shader "UI/PulsingGlow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (0,1,1,1)

        _GlowSize ("Glow Size", Float) = 0.005

        _PulseSpeed ("Pulse Speed", Float) = 2
        _PulseStrength ("Pulse Strength", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
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

            sampler2D _MainTex;

            float4 _GlowColor;

            float _GlowSize;
            float _PulseSpeed;
            float _PulseStrength;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                float glow = 0;

                glow += tex2D(_MainTex, i.uv + float2(_GlowSize,0)).a;
                glow += tex2D(_MainTex, i.uv + float2(-_GlowSize,0)).a;
                glow += tex2D(_MainTex, i.uv + float2(0,_GlowSize)).a;
                glow += tex2D(_MainTex, i.uv + float2(0,-_GlowSize)).a;

                glow = saturate(glow);

                float pulse =
                    (sin(_Time.y * _PulseSpeed) * 0.5 + 0.5);

                pulse *= _PulseStrength;

                fixed4 glowColor =
                    _GlowColor * glow * pulse;

                return col + glowColor * (1 - col.a);
            }

            ENDCG
        }
    }
}