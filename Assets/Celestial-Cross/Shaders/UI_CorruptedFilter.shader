Shader "UI/CorruptedFilter"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint (Aplica Transparência)", Color) = (1,1,1,1)
        
        [Enum(Burn Edge, 0, TV Retuning, 1)] _PurificationMode ("Purification Style", Float) = 0
        
        _GlitchIntensity ("Glitch Intensity", Range(0, 5)) = 1.0
        _StaticIntensity ("Static Amount", Range(0, 1)) = 0.8
        _StaticGrainSize ("Static Grain Size", Range(1.0, 50.0)) = 5.0
        
        _RedBlots ("Red Blots Amount", Range(0, 1)) = 0.3
        _BlackBlots ("Black Blots Amount", Range(0, 1)) = 0.2
        
        _GlitchSpeed ("Glitch Speed", Range(0, 50)) = 20.0
        
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0.0
        _BurnEdge ("Burn Edge Size", Range(0, 0.5)) = 0.15
        
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
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
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _PurificationMode;
            
            float _GlitchIntensity;
            float _StaticIntensity;
            float _StaticGrainSize;
            float _RedBlots;
            float _BlackBlots;
            float _GlitchSpeed;
            float _DissolveAmount;
            float _BurnEdge;

            // Função de ruído clássica
            float rand(float2 n) { 
                return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
            }

            // Ruído suave
            float noise(float2 p){
                float2 ip = floor(p);
                float2 u = frac(p);
                u = u*u*(3.0-2.0*u);
                
                float res = lerp(
                    lerp(rand(ip),rand(ip+float2(1.0,0.0)),u.x),
                    lerp(rand(ip+float2(0.0,1.0)),rand(ip+float2(1.0,1.0)),u.x),u.y);
                return res*res;
            }

            // Rotação de UV para cortes angulados
            float2 rotateUV(float2 uv, float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                uv -= 0.5;
                uv = float2(c * uv.x - s * uv.y, s * uv.x + c * uv.y);
                uv += 0.5;
                return uv;
            }

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float time = _Time.y * _GlitchSpeed;

                // Variáveis locais para permitir modificação durante a "Sintonização"
                float currentGlitch = _GlitchIntensity;
                float currentStatic = _StaticIntensity;
                float currentRed = _RedBlots;
                float currentBlack = _BlackBlots;

                if (_PurificationMode == 1.0) // TV Retuning
                {
                    float remaining = 1.0 - _DissolveAmount;
                    currentGlitch *= remaining;
                    currentStatic *= remaining;
                    currentRed *= remaining;
                    currentBlack *= remaining;
                    
                    // Barra rolante horizontal de dessincronização V-Sync quando está resintonizando
                    if (_DissolveAmount > 0.0)
                    {
                        float vsync = step(0.85, sin(uv.y * 15.0 + time * 30.0));
                        uv.x += vsync * _DissolveAmount * 0.15 * rand(float2(time, uv.y));
                    }
                }

                // 1. CORTES E DESLOCAMENTOS DE MUITOS ÂNGULOS (Shattered / Chaotic)
                float2 uvRot1 = rotateUV(uv, 0.5);
                float2 uvRot2 = rotateUV(uv, -0.8);
                float2 uvRot3 = rotateUV(uv, 1.2);
                float2 uvRot4 = rotateUV(uv, -0.3);
                
                float lineNoise1 = rand(float2(floor(uvRot1.y * 15.0), floor(time * 2.0)));
                float lineNoise2 = rand(float2(floor(uvRot2.x * 20.0), floor(time * 3.0)));
                float lineNoise3 = rand(float2(floor(uvRot3.y * 10.0), floor(time * 4.0)));
                float lineNoise4 = rand(float2(floor(uvRot4.x * 25.0), floor(time * 1.5)));

                float isCut1 = step(0.85, lineNoise1);
                float isCut2 = step(0.90, lineNoise2);
                float isCut3 = step(0.92, lineNoise3);
                float isCut4 = step(0.88, lineNoise4);

                // Em vez de sin(time) que faz um movimento suave (wobble), 
                // usamos um random baseado em blocos de tempo para dar "snaps" violentos (glitch real)
                float glitchSnapX = rand(float2(floor(time * 15.0), 1.0)) * 2.0 - 1.0;
                float glitchSnapY = rand(float2(floor(time * 15.0), 2.0)) * 2.0 - 1.0;

                uv.x += (isCut1 * 0.05 + isCut3 * -0.07) * currentGlitch * glitchSnapX;
                uv.y -= (isCut2 * 0.08 + isCut4 * -0.04) * currentGlitch * glitchSnapY;

                // Ler textura original
                fixed4 texColor = tex2D(_MainTex, uv);

                // 2. ESTÁTICA DE TV COLORIDA E AJUSTÁVEL
                float gridRes = 2000.0 / _StaticGrainSize;
                float2 grainUV = floor(uv * gridRes) / gridRes;

                float staticR = rand(grainUV * time * 0.1);
                float staticG = rand(grainUV * time * 0.13);
                float staticB = rand(grainUV * time * 0.17);
                fixed3 tvStatic = fixed3(staticR, staticG, staticB);
                
                float staticLuma = dot(tvStatic, fixed3(0.299, 0.587, 0.114));
                fixed3 finalStatic = lerp(fixed3(staticLuma, staticLuma, staticLuma), tvStatic, 0.5);

                // 3. COMBINAÇÃO DRAMÁTICA DE CORES E MANCHAS
                fixed3 corruptedColor = lerp(texColor.rgb * finalStatic, finalStatic, currentStatic);
                
                float blockNoise = noise(uv * 5.0 + time);
                float redThreshold = 1.0 - currentRed;
                float blackThreshold = currentBlack;

                if (blockNoise > redThreshold) corruptedColor *= fixed3(0.8, 0.1, 0.1);
                if (blockNoise < blackThreshold) corruptedColor *= 0.1;

                // Scanlines inclinadas
                float scanlines = sin((uv.x + uv.y) * 150.0 + time * 15.0);
                corruptedColor += scanlines * 0.05 * currentGlitch;

                fixed4 finalColor = fixed4(corruptedColor, texColor.a);
                finalColor *= IN.color;

                // 4. PURIFICAÇÃO
                if (_PurificationMode == 0.0) // Burn Edge (Dissolve)
                {
                    float dissolveNoise = noise(uv * 8.0 - time * 0.5);
                    float threshold = _DissolveAmount * 1.5; 
                    
                    if (threshold > 0.0)
                    {
                        if (dissolveNoise < threshold)
                        {
                            finalColor.a = 0.0;
                        }
                        else if (dissolveNoise < threshold + _BurnEdge)
                        {
                            float edgeThickness = (threshold + _BurnEdge - dissolveNoise) / _BurnEdge; 
                            float edgeGlitch = step(0.2, rand(float2(uv.y * 50.0, time)));
                            finalColor.rgb = lerp(finalColor.rgb, fixed3(2.0, 2.0, 2.0), edgeThickness * edgeGlitch); 
                            finalColor.a = IN.color.a; 
                        }
                    }
                    finalColor.a *= saturate(1.0 - (_DissolveAmount * 1.2 - 0.2));
                }
                else // TV Retuning
                {
                    // Pequeno flash branco de "Sinal Encontrado" no final
                    float flash = smoothstep(0.7, 0.9, _DissolveAmount) * smoothstep(1.0, 0.8, _DissolveAmount);
                    finalColor.rgb += flash * fixed3(1.5, 1.5, 1.5);
                    
                    // Fade out clássico
                    finalColor.a *= saturate(1.0 - _DissolveAmount);
                }

                return finalColor;
            }
            ENDCG
        }
    }
}
