Shader "UI/StarrySky"
{
    Properties
    {
        [Header(Background)]
        _BgColor ("Background Color", Color) = (0.05, 0.05, 0.15, 1.0)
        
        [Header(Star Settings)]
        [PerRendererData] _MainTex ("Star Shape (UI Image)", 2D) = "white" {}
        _StarColor ("Star Color", Color) = (1.0, 1.0, 0.8, 1.0)
        
        [Header(Grid and Density)]
        _Density ("Star Density (Grid Size)", Range(1, 100)) = 20.0
        _Probability ("Star Probability", Range(0, 1)) = 0.5
        _AspectRatio ("Aspect Ratio (Width / Height)", Float) = 1.0
        
        [Header(Size Settings)]
        _MinSize ("Min Star Size", Range(0.01, 1)) = 0.1
        _MaxSize ("Max Star Size", Range(0.01, 1)) = 0.7
        
        [Header(Twinkle Settings)]
        _TwinkleSpeed ("Twinkle Speed", Range(0, 10)) = 2.0
        _TwinkleIntensity ("Twinkle Min Brightness", Range(0, 1)) = 0.2

        // --- Configurações necessárias para UI ---
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        [HideInInspector] _ClipRect ("Clip Rect", Vector) = (-32767, -32767, 32767, 32767)
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

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            float4 _BgColor;
            float4 _StarColor;
            sampler2D _MainTex;
            float _Density;
            float _Probability;
            float _AspectRatio;
            float _MinSize;
            float _MaxSize;
            float _TwinkleSpeed;
            float _TwinkleIntensity;
            
            float4 _ClipRect;

            float2 random2(float2 p)
            {
                return frac(sin(float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)))) * 43758.5453);
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 st = IN.uv;
                st.x *= max(0.001, _AspectRatio);
                st *= max(1.0, _Density);
                
                float2 id = floor(st); 
                float2 f = frac(st);   
                
                fixed4 col = _BgColor;

                // Um offset de tempo fixo por célula para que não pisquem todas juntas
                float timeOffset = random2(id).x * 100.0;
                float speed = max(0.2, _TwinkleSpeed);
                
                // Tempo contínuo
                float t = _Time.y * speed + timeOffset;
                
                // Dividimos a vida de uma estrela em "ciclos"
                float cycle = frac(t * 0.5); 
                float cycleIndex = floor(t * 0.5);
                
                // MÁGICA: A semente aleatória muda a cada ciclo!
                // Isso faz com que toda vez que a estrela apagar, ela renasça em um lugar DIFERENTE,
                // criando um efeito contínuo de novas estrelas surgindo na tela.
                float2 rand = random2(id + cycleIndex * 12.34);
                
                if (rand.x <= _Probability) 
                {
                    float2 offset = float2(0.5, 0.5) + (rand * 0.3 - 0.15); 
                    float2 pos = f - offset;
                    
                    float size = lerp(_MinSize, _MaxSize, rand.y);
                    float2 starUV = (pos / size) + 0.5;
                    float isInside = step(0.0, starUV.x) * step(starUV.x, 1.0) * step(0.0, starUV.y) * step(starUV.y, 1.0);
                    
                    // Curva de brilho perfeita (Seno): vai de 0 -> 1 -> 0
                    float baseTwinkle = sin(cycle * 3.14159265);
                    
                    // Aplica a intensidade mínima que o usuário pediu no meio da vida útil
                    float twinkle = lerp(_TwinkleIntensity, 1.0, baseTwinkle);
                    
                    // Força a opacidade a ir para ZERO absoluto nas bordas do ciclo
                    // para que a estrela mude de posição sem dar um "pulo" visual
                    twinkle *= smoothstep(0.0, 0.1, cycle) * smoothstep(1.0, 0.9, cycle);

                    fixed4 texColor = tex2D(_MainTex, starUV);
                    float shapeMask = texColor.a * max(texColor.r, max(texColor.g, texColor.b));
                    
                    float finalMask = shapeMask * isInside * twinkle;
                    fixed4 finalStarColor = _StarColor * finalMask;
                    
                    col.rgb += finalStarColor.rgb;
                    col.a = max(_BgColor.a, finalMask);
                }
                
                col *= IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
                
                #ifdef UNITY_UI_ALPHACLIP
                clip (col.a - 0.001);
                #endif
                
                return col;
            }
            ENDCG
        }
    }
}
