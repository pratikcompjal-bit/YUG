Shader "Temple/WeatheredStone"
{
    Properties
    {
        _Color("Stone",Color)=(.42,.39,.32,1)
        _MainTex("Granite",2D)="white"{}
        _Masonry("Courses: wall 1, paving 2",Float)=1
        _Moss("Patina",Range(0,1))=.3
        _Glossiness("Smoothness",Range(0,1))=.13
    }
    SubShader
    {
        Tags {"RenderType"="Opaque"}
        LOD 300
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        sampler2D _MainTex;
        fixed4 _Color;
        float _Masonry,_Moss,_Glossiness;
        struct Input {float3 worldPos; float3 worldNormal; INTERNAL_DATA};
        float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
        float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
        void surf(Input IN,inout SurfaceOutputStandard o)
        {
            float3 p=IN.worldPos;
            float3 w=pow(abs(WorldNormalVector(IN,float3(0,0,1))),5);w/=max(.001,w.x+w.y+w.z);
            float tex=tex2D(_MainTex,p.zy*.75).r*w.x+tex2D(_MainTex,p.xz*.75).r*w.y+tex2D(_MainTex,p.xy*.75).r*w.z;
            float2 uv=w.x>w.z?p.zy:p.xy;
            if(w.y>.6) uv=p.xz;
            float2 grid=uv/float2(1.8,.68);
            if(w.y>.6) grid=uv/1.35;
            grid.x+=floor(grid.y)*.5;
            float2 edge=min(frac(grid),1-frac(grid));
            float joint=1-smoothstep(.008,.027,min(edge.x,edge.y));
            joint*=step(.5,_Masonry);
            float n=noise(uv*1.1)*.6+noise(uv*4.7)*.4;
            float stains=smoothstep(.35,.8,n)*_Moss;
            float variation=lerp(.86,1.1,hash(floor(grid)));
            float3 stone=_Color.rgb*tex*variation*(.91+n*.16);
            stone=lerp(stone,stone*float3(.56,.72,.40),stains);
            stone*=1-joint*.37;
            o.Albedo=stone;
            o.Smoothness=_Glossiness;
            o.Occlusion=1-joint*.38;
            // Fine grain is world mapped, so long walls and paving never stretch the texture.
            float fine=noise(uv*48);
            o.Normal=normalize(float3(ddx(fine)*.14,ddy(fine)*.14,1));
        }
        ENDCG
    }
    Fallback "Diffuse"
}
