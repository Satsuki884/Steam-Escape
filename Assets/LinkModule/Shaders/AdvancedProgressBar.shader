Shader "Unlit/AdvancedProgressBar"
{
    Properties
    {
        [HideInInspector] _MainTex("Texture", 2D) = "white" {}
        [HideInInspector] _MainColor("Main Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _StartColor("Start Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _EndColor("End Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _BackColor("Back Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _Gradient("Gradient", Range(0, 1)) = 0
        [HideInInspector] _Roundness("Roundness", Range(0, 1)) = 0.5
        [HideInInspector] _BorderSize("Border Size", Range(0, 1)) = 0.2
        [HideInInspector] _FillAmount("Fill Amount", Range(0, 1)) = 0
        [HideInInspector] _Size("Size", Vector) = (1, 1, 1, 1)
        [HideInInspector] _GlowColor("Glow Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _GlowSize("Glow Size", Range(0, 0.5)) = 0.05
        [HideInInspector] _GlowPower("Glow Power", Range(1, 10)) = 4
        [HideInInspector] _TrailSpeed("Trail Speed", Range(0.1, 5)) = 1
        [HideInInspector] _TrailStrength("Trail Strength", Range(0, 1)) = 0.3
        [HideInInspector] _TrailFrequency("Trail Frequency", Range(1, 10)) = 5
        [HideInInspector] _TrailWidth("Trail Width", Range(0.01, 0.5)) = 0.1
        [HideInInspector] _TrailCount("Trail Count", Range(1, 6)) = 3
        [HideInInspector] _TrailFadeStart("Trail Fade Start", Range(0.7, 1)) = 0.9
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainColor, _StartColor, _EndColor, _BackColor;
            float4 _GlowColor;
            float _Gradient, _Roundness, _BorderSize, _FillAmount;
            float _GlowSize, _GlowPower;
            float _TrailSpeed, _TrailStrength, _TrailFrequency, _TrailWidth;
            float _TrailCount, _TrailFadeStart, _HighlightPower;
            float4 _Size;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float CalculateDistance(float2 position, float roundness, float2 halfSize)
            {
                float2 edge = abs(position) - (halfSize - roundness);
                float outDistance = length(max(edge, 0));
                float insDistance = min(max(edge.x, edge.y), 0);
                return outDistance + insDistance - roundness;
            }

            float ApplyAA(float dist)
            {
                float f = fwidth(dist) * 0.5;
                return smoothstep(f, -f, dist);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 size = _Size.xy;
                float uvPos;
                float axisHalfSize;
                if (size.x < size.y)
                {
                    uvPos = uv.y;
                    axisHalfSize = size.x * 0.5;
                }
                else
                {
                    uvPos = uv.x;
                    axisHalfSize = size.y * 0.5;
                }

                float frameDistance = CalculateDistance((uv - 0.5) * size, _Roundness * axisHalfSize, size * 0.5);
                float frameMask = ApplyAA(frameDistance);
                float borderDistance = frameDistance + _BorderSize * axisHalfSize;
                float borderMask = ApplyAA(borderDistance);

                float fillMask = step(uvPos, _FillAmount);
                float glowEdge = smoothstep(_FillAmount - _GlowSize, _FillAmount, uvPos);
                float glowFactor = pow(glowEdge, _GlowPower);
                float4 glow = _GlowColor * glowFactor * fillMask;

                float trail = 0;
                for (int n = 0; n < int(_TrailCount); n++) {
                    float t = frac(uv.x * _TrailFrequency + _Time.y * _TrailSpeed + n * 0.3);
                    float pulse = smoothstep(_TrailWidth, 0.0, abs(t - 0.5));
                    trail += pulse;
                }

                float isInGlowZone = smoothstep(_FillAmount - _GlowSize, _FillAmount, uvPos);
                float fadeOut = smoothstep(1.0, _TrailFadeStart, uv.x);
                trail *= _TrailStrength * isInGlowZone * fadeOut;
                float4 trailColor = _GlowColor * trail;

                float4 fill = lerp(_StartColor, _EndColor, uvPos * _Gradient + _FillAmount * (1 - _Gradient));
                fill.rgb = lerp(fill.rgb, _GlowColor.rgb, glowFactor * 0.5);

                float4 fillColor = float4(fill.rgb + glow.rgb + trailColor.rgb, fill.a * borderMask * fillMask);
                float4 backColor = float4(_BackColor.rgb, _BackColor.a * frameMask);
                float4 mainColor = tex2D(_MainTex, uv) * _MainColor;
                float4 color = lerp(backColor, fillColor, max(fillColor.a, (_BorderSize <= 0 && fillMask) * fill.a));
                float alpha = max(backColor.a, fillColor.a);

                return mainColor * float4(color.rgb, alpha);
            }
            ENDCG
        }
    }
}