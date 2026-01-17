Shader "Custom/UnlitWhiteOutline"
{
    Properties
    {
        _Color ("Fill Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.05)) = 0.01
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        // --- Outline pass (draw "inflated" mesh in black) ---
        Pass
        {
            Name "OUTLINE"
            Cull Front
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _OutlineWidth;
            fixed4 _OutlineColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;

                // UnityObjectToViewPos returns float3
                float3 viewPos = UnityObjectToViewPos(v.vertex);

                // Transform normal into view space
                float3 viewNormal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));

                // Extrude in view space
                viewPos += viewNormal * _OutlineWidth;

                // Project to clip space (needs float4)
                o.pos = mul(UNITY_MATRIX_P, float4(viewPos, 1.0));

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }

        // --- Fill pass (unlit white, no shading) ---
        Pass
        {
            Name "FILL"
            Cull Back
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _Color; // totally flat
            }
            ENDCG
        }
    }

    Fallback Off
}
