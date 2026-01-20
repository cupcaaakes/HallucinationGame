Shader "Unlit/BlurGlitchGrabPass"
{
    Properties
    {
        _Opacity("Opacity", Range(0,1)) = 1

        _BlurStrength("Blur Strength", Range(0,1)) = 0.8
        _BlurRadius("Blur Radius (pixels)", Range(0,10)) = 3

        _GlitchStrength("Glitch Strength (pixels)", Range(0,80)) = 20
        _GlitchSpeed("Glitch Speed", Range(0,20)) = 4
        _BandCount("Glitch Bands", Range(1,200)) = 35
        _BandChance("Band Chance", Range(0,1)) = 0.35

        _Chromatic("Chromatic Offset (pixels)", Range(0,10)) = 2
        _Noise("Noise Jitter (pixels)", Range(0,10)) = 1

        // ---------------- NEW ----------------
        _BandSizeJitter("Band Size Randomness", Range(0,2)) = 0.65
        _BandSizeSpeed("Band Size Change Speed", Range(0,20)) = 0

        _GlitchEnable("Glitch Enable (0=Never,1=Yes)", Range(0,1)) = 1
        _TimedGlitch("Timed Glitch Mode (0=Always,1=Bursts)", Range(0,1)) = 0

        _GlitchMinInterval("Glitch Min Interval (sec)", Range(0,30)) = 3
        _GlitchMaxInterval("Glitch Max Interval (sec)", Range(0.1,60)) = 7
        _GlitchDuration("Glitch Burst Duration (sec)", Range(0.05,5)) = 0.35
        _GlitchEventChance("Burst Chance per Window", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent+50" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        GrabPass { "_GrabTexture" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _GrabTexture;
            float4 _GrabTexture_TexelSize;

            float _Opacity;
            float _BlurStrength;
            float _BlurRadius;

            float _GlitchStrength;
            float _GlitchSpeed;
            float _BandCount;
            float _BandChance;

            float _Chromatic;
            float _Noise;

            // NEW
            float _BandSizeJitter;
            float _BandSizeSpeed;

            float _GlitchEnable;
            float _TimedGlitch;

            float _GlitchMinInterval;
            float _GlitchMaxInterval;
            float _GlitchDuration;
            float _GlitchEventChance;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 grabPos : TEXCOORD0;
            };

            // cheap deterministic random
            float Hash(float n)
            {
                return frac(sin(n) * 43758.5453123);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.pos);
                return o;
            }

            fixed4 SampleGrab(float2 uv)
            {
                return tex2D(_GrabTexture, uv);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // normalized screen uv
                float2 uv = i.grabPos.xy / i.grabPos.w;
                float2 texel = _GrabTexture_TexelSize.xy;

                // ---- random time step ----
                float stepTime = floor(_Time.y * max(0.01, _GlitchSpeed));

                // ============================================================
                // NEW: Global "glitch comes in bursts" gate (or always-on)
                // ============================================================
                float maxInterval = max(_GlitchMaxInterval, 0.001);
                float winId = floor(_Time.y / maxInterval);
                float localT = _Time.y - winId * maxInterval;

                float dur = clamp(_GlitchDuration, 0.01, maxInterval);
                float startMin = min(_GlitchMinInterval, maxInterval - dur);
                float startRange = max(0.0, (maxInterval - dur) - startMin);

                float startT = startMin + Hash(winId * 17.11 + 3.77) * startRange;
                float roll = Hash(winId * 91.37 + 5.13);

                float eventOn =
                    step(roll, _GlitchEventChance) *
                    step(startT, localT) *
                    step(localT, startT + dur);

                // 0 = always, 1 = bursts
                float glitchGate = lerp(1.0, eventOn, saturate(_TimedGlitch)) * saturate(_GlitchEnable);

                // ============================================================
                // NEW: Random band sizes
                // ============================================================
                float bandBase = uv.y * max(1.0, _BandCount);

                // base band id (before warping)
                float baseBandId = floor(bandBase);

                // slowly-changing band randomness (optional)
                float bandTime = floor(_Time.y * max(0.01, _BandSizeSpeed));

                // warp amount in "band units"
                float warp = (Hash(baseBandId * 78.233 + bandTime * 9.17) - 0.5) * _BandSizeJitter;

                // warped band id -> creates thicker/thinner bands
                float band = floor(bandBase + warp);

                // ---- banded glitch mask ----
                float rBand = Hash(band * 78.233 + stepTime * 3.17);
                float bandOn = step(_BandChance, rBand);

                // APPLY global burst gate
                bandOn *= glitchGate;

                // ---- horizontal glitch shift in pixels -> uv ----
                float glitchPixels = (rBand - 0.5) * _GlitchStrength * bandOn;
                uv.x += glitchPixels * texel.x;

                // ---- random jitter ----
                float jitter = (Hash(stepTime * 91.7 + band * 12.3) - 0.5) * _Noise * bandOn;
                uv += float2(jitter * texel.x, 0);

                // ---- blur taps (5 tap cross blur) ----
                float2 o = texel * _BlurRadius;

                fixed4 c0 = SampleGrab(uv);
                fixed4 c1 = SampleGrab(uv + float2( o.x, 0));
                fixed4 c2 = SampleGrab(uv + float2(-o.x, 0));
                fixed4 c3 = SampleGrab(uv + float2(0,  o.y));
                fixed4 c4 = SampleGrab(uv + float2(0, -o.y));

                fixed4 blurred = (c0 * 4 + c1 + c2 + c3 + c4) / 8;

                fixed4 baseCol = lerp(c0, blurred, _BlurStrength);

                // ---- chromatic glitch (RGB split) ----
                float2 ch = texel * _Chromatic * bandOn;

                fixed4 cr = SampleGrab(uv + ch);
                fixed4 cb = SampleGrab(uv - ch);

                baseCol.r = cr.r;
                baseCol.b = cb.b;

                baseCol.a = _Opacity;
                return baseCol;
            }
            ENDCG
        }
    }
}
