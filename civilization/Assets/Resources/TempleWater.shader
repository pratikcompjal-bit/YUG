Shader "Temple/LivingWater"
{
 Properties {_Color("Deep jade",Color)=(.035,.15,.13,1)}
 SubShader
 {
  Tags {"RenderType"="Opaque"}
  CGPROGRAM
  #pragma surface surf Standard fullforwardshadows
  #pragma target 3.0
  fixed4 _Color;
  struct Input {float3 worldPos;float3 viewDir;};
  void surf(Input IN,inout SurfaceOutputStandard o)
  {
   float2 p=IN.worldPos.xz;
   float a=sin(p.x*3.7+p.y*2.9+_Time.y*.8),b=sin(p.y*6.1-p.x*1.6+_Time.y*1.2);
   o.Normal=normalize(float3(a*.025,b*.018,1));
   float f=pow(1-saturate(dot(normalize(IN.viewDir),o.Normal)),3);
   o.Albedo=lerp(_Color.rgb,float3(.25,.37,.33),f*.65);
   o.Smoothness=.62;o.Metallic=.04;
   o.Emission=float3(.025,.05,.043)*pow(saturate(a*b),12);
  }
  ENDCG
 }
 Fallback "Standard"
}
