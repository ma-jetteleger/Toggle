Shader "Unlit/QuadrantBackground"
{
    Properties
    {
        _ColorTL ("Top Left Color", Color) = (1, 0, 0, 1)
        _ColorTR ("Top Right Color", Color) = (0, 1, 0, 1)
        _ColorBL ("Bottom Left Color", Color) = (0, 0, 1, 1)
        _ColorBR ("Bottom Right Color", Color) = (1, 1, 0, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Background" }
        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off
            Blend Off

            HLSLPROGRAM
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

            fixed4 _ColorTL;
            fixed4 _ColorTR;
            fixed4 _ColorBL;
            fixed4 _ColorBR;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                bool isTop = uv.y > 0.5;
                bool isRight = uv.x > 0.5;

                if (isTop && !isRight) return _ColorTL;
                if (isTop && isRight)  return _ColorTR;
                if (!isTop && !isRight) return _ColorBL;
                return _ColorBR;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
