Shader "Hidden/TempleLens"
{
    Properties { _MainTex ("Image", 2D) = "white" {} }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            sampler2D _CameraDepthNormalsTexture;
            float4 _MainTex_TexelSize;
            float4 frag(v2f_img i) : SV_Target
            {
                float3 col = tex2D(_MainTex, i.uv).rgb;
                float3 glow = 0;
                for (int k = 0; k < 8; k++)
                {
                    float a = k * .7853982;
                    float2 offset = float2(cos(a), sin(a)) * _MainTex_TexelSize.xy * 7;
                    float3 tap = tex2D(_MainTex, i.uv + offset).rgb;
                    glow += max(0, tap - 1.15);
                }
                col += glow * .016;
                float depth;float3 normal;
                DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture,i.uv),depth,normal);
                float occlusion=0;
                for(int s=0;s<12;s++)
                {
                    float a=s*2.39996;
                    float2 offset=float2(cos(a),sin(a))*_MainTex_TexelSize.xy*(3+s*1.2);
                    float other;float3 n;
                    DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture,i.uv+offset),other,n);
                    float diff=(depth-other)*_ProjectionParams.z;
                    occlusion+=smoothstep(.035,.15,diff)*(1-smoothstep(.15,1.5,diff));
                }
                col*=1-occlusion*.035;
                // Gentle shoulder preserves bright sandstone and the cool river.
                col = col / (1 + max(0, col - .7) * .28);
                float2 p = (i.uv - .5) * 2;
                float vignette = 1 - .20 * smoothstep(.35, 1.35, dot(p, p));
                return float4(max(0, col * vignette), 1);
            }
            ENDCG
        }
    }
}
