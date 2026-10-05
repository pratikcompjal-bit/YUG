Shader "Skybox/YUG Daylight"
{
    Properties
    {
        _ZenithColor ("High sky", Color) = (.36, .50, .57, 1)
        _HorizonColor ("Horizon", Color) = (.70, .67, .53, 1)
        _EarthHaze ("Distant earth haze", Color) = (.56, .57, .47, 1)
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _ZenithColor, _HorizonColor, _EarthHaze;
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 position : SV_POSITION; float3 direction : TEXCOORD0; };
            v2f vert(appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.direction = v.vertex.xyz;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float y = normalize(i.direction).y;
                float upper = smoothstep(0, .75, y);
                float lower = smoothstep(0, .28, -y);
                fixed3 color = lerp(_HorizonColor.rgb, _ZenithColor.rgb, upper);
                color = lerp(color, _EarthHaze.rgb, lower);
                return fixed4(color, 1);
            }
            ENDCG
        }
    }
}
