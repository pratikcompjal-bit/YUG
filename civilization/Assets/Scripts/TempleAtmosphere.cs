using System.Collections.Generic;
using UnityEngine;

/// <summary>Authored architectural framing, garden beds, weather and environmental life.</summary>
public class TempleAtmosphere : MonoBehaviour
{
    readonly List<Transform> birds=new List<Transform>();
    Material plants,fernLight,paving;
    static Material particleMaterial;
    public static Material SoftParticleMaterial()
    {
        if(particleMaterial!=null)return particleMaterial;
        var texture=new Texture2D(64,64,TextureFormat.RGBA32,false);
        for(int y=0;y<64;y++)for(int x=0;x<64;x++){float r=Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f))/31.5f;texture.SetPixel(x,y,new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-r),2)));}
        texture.Apply();particleMaterial=new Material(Shader.Find("Particles/Standard Unlit"));particleMaterial.mainTexture=texture;
        particleMaterial.SetFloat("_Mode",2);particleMaterial.SetInt("_SrcBlend",(int)UnityEngine.Rendering.BlendMode.SrcAlpha);particleMaterial.SetInt("_DstBlend",(int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);particleMaterial.SetInt("_ZWrite",0);particleMaterial.EnableKeyword("_ALPHABLEND_ON");particleMaterial.renderQueue=3000;
        return particleMaterial;
    }

    public void Build()
    {
        paving=new Material(CholaKit.stone);paving.name="Moss-edged granite paving";paving.color=new Color(.36f,.38f,.33f);paving.SetFloat("_Moss",.4f);
        plants=new Material(CholaKit.foliage);plants.color=new Color(.075f,.18f,.085f);
        fernLight=new Material(plants);fernLight.color=new Color(.18f,.27f,.10f);
        // Forecourt narrows the approach; doors and balconies frame the first major reveal.
        foreach(int side in new[]{-1,1})
        {
            Wall(new Vector3(side*8,0,-8),new Vector3(1.2f,5.4f,20));
            for(int row=0;row<4;row++)CholaKit.Pillar(new Vector3(side*7.2f,0,-15+row*5),5.6f,transform);
            Box("Approach shelter",new Vector3(side*10.5f,5.6f,-8),new Vector3(8,.5f,21),CholaKit.darkStone);
            Box("Shelter cornice",new Vector3(side*10.5f,5.35f,-8),new Vector3(8.4f,.20f,21.4f),CholaKit.trim);
            // Terraced planted strips and a route to a secluded shrine court.
            Box("Courtyard paving",new Vector3(side*11,.005f,16),new Vector3(13,.05f,29),paving,false);
            Box("Raised garden edging",new Vector3(side*25.5f,0,24),new Vector3(1,.55f,58),CholaKit.darkStone);
            for(int z=0;z<6;z++)
            {
                int step=z%2;
                FernBed(new Vector3(side*(23.8f-step),.04f,-5+z*11),4.0f,13,z+side*20);
                FernBed(new Vector3(side*12.8f,.07f,25+z*5),1.8f,5,z+side*40);
            }
            // Enclosed mandapa side aisles, with small openings catching warm light.
            for(int z=24;z<=39;z+=5)
            {
                Wall(new Vector3(side*10,.9f,z),new Vector3(.9f,3.6f,3.7f));
                Box("Mandapa stone screen sill",new Vector3(side*10,4.5f,z),new Vector3(1.1f,.16f,4.3f),CholaKit.trim);
            }
            Box("Mandapa upper frieze",new Vector3(side*10,5.5f,32),new Vector3(1,1.0f,21),CholaKit.stone);
            Box("Courtyard parapet base",new Vector3(side*31.2f,0,28),new Vector3(.5f,.65f,91),CholaKit.darkStone);
            for(int z=-12;z<72;z+=7)
            {
                Box("Enclosure pilaster",new Vector3(side*31,0,z),new Vector3(.9f,3.9f,1),CholaKit.stone);
                Box("Pilaster cap",new Vector3(side*31,3.9f,z),new Vector3(1.4f,.3f,1.5f),CholaKit.trim);
            }
        }
        // Paving close to the camera has joints at a consistent world scale.
        Box("Processional paving",new Vector3(0,.036f,0),new Vector3(6.8f,.015f,37),paving,false);
        Box("Mandapa laid stone floor",new Vector3(0,.901f,32),new Vector3(20.7f,.015f,20.7f),paving,false);
        // Low railings frame the tank, rather than a featureless blue rectangle.
        for(int i=0;i<9;i++)foreach(float z in new[]{-2.0f,14.0f})
        {
            Box("Tank baluster",new Vector3(-27.5f+i*1.35f,.65f,z),new Vector3(.16f,.68f,.16f),CholaKit.darkStone,false);
        }
        foreach(float z in new[]{-2.0f,14.0f})Box("Tank rail",new Vector3(-22,1.33f,z),new Vector3(12,.13f,.24f),CholaKit.trim,false);
        // Lotus pads, petals, and inset courtyard rosettes provide small-scale detail.
        for(int i=0;i<15;i++)
        {
            Vector3 p=new Vector3(-26+(i*1.93f)%8,.14f,1+(i*3.13f)%10);
            var pad=Primitive("Lotus leaf",PrimitiveType.Cylinder,p,new Vector3(.5f,.015f,.4f),plants);
            if(i%3==0)for(int n=0;n<7;n++)
            {
                float a=n*Mathf.PI*2/7;var petal=Primitive("Tank lotus petal",PrimitiveType.Sphere,p+new Vector3(Mathf.Cos(a)*.10f,.05f,Mathf.Sin(a)*.10f),new Vector3(.1f,.11f,.2f),CholaKit.trim);petal.transform.rotation=Quaternion.Euler(-28,n*360/7,0);
            }
        }
        // An optional sheltered niche is reachable behind the eastern shrine.
        Wall(new Vector3(24,0,36),new Vector3(8,3.8f,1));
        CholaKit.Lamp(new Vector3(23,0,34),transform);CholaKit.Lamp(new Vector3(25,0,34),transform);
        FernBed(new Vector3(27,0,33),2,9,114);
        // A dark inner sanctuary becomes reachable after opening the real stone doors.
        Box("Sanctuary polished floor",new Vector3(0,1.761f,54),new Vector3(16,.03f,16),paving,false);
        Box("Inner bronze dais",new Vector3(0,1.79f,56),new Vector3(3.4f,.6f,3.4f),CholaKit.darkStone);
        Primitive("Sacred stone lingam",PrimitiveType.Capsule,new Vector3(0,3.05f,56),new Vector3(1.15f,.83f,1.15f),CholaKit.darkStone);
        foreach(int s in new[]{-1,1}){CholaKit.Lamp(new Vector3(s*2.6f,1.79f,54),transform);CholaKit.Lamp(new Vector3(s*4,1.79f,58),transform);}
        var sanctuary=new GameObject("Sanctuary shaft of light");sanctuary.transform.SetParent(transform);sanctuary.transform.position=new Vector3(0,7.7f,54);sanctuary.transform.rotation=Quaternion.Euler(75,0,0);
        var glow=sanctuary.AddComponent<Light>();glow.type=LightType.Spot;glow.color=new Color(1,.68f,.31f);glow.intensity=5;glow.range=17;glow.spotAngle=57;glow.shadows=LightShadows.Soft;
        // Terrain and distant palms remove the empty horizon beyond the enclosure.
        Mesh terrain=new Mesh();var verts=new List<Vector3>();var tris=new List<int>();
        const int steps=64;
        for(int ring=0;ring<2;ring++)for(int i=0;i<=steps;i++)
        {
            float a=i*Mathf.PI*2/steps,r=ring==0?79:145;
            verts.Add(new Vector3(Mathf.Cos(a)*r,ring==0?-1:5+Mathf.PerlinNoise(i*.22f,8)*19,25+Mathf.Sin(a)*r));
        }
        for(int i=0;i<steps;i++){int j=i+steps+1;tris.AddRange(new[]{i,j,i+1,i+1,j,j+1});}
        terrain.SetVertices(verts);terrain.SetTriangles(tris,0);terrain.RecalculateNormals();
        var hills=new GameObject("Distant tropical ridges");hills.transform.SetParent(transform);hills.AddComponent<MeshFilter>().sharedMesh=terrain;hills.AddComponent<MeshRenderer>().sharedMaterial=plants;
        for(int i=0;i<28;i++){float a=i*Mathf.PI*2/28;CholaKit.Palm(new Vector3(Mathf.Cos(a)*52,0,25+Mathf.Sin(a)*70),11+i%5,transform);}
        AddDust(); AddBirds(); AddAmbientSound();
    }

    void Wall(Vector3 p,Vector3 size)
    {
        Box("Coursed granite wall",p,size,CholaKit.stone);
        Box("Wall foot moulding",p,new Vector3(size.x+.17f,.22f,size.z+.17f),CholaKit.darkStone);
        Box("Wall cornice",p+Vector3.up*(size.y-.14f),new Vector3(size.x+.25f,.21f,size.z+.25f),CholaKit.trim);
    }
    GameObject Box(string n,Vector3 p,Vector3 s,Material m,bool collision=true){return CholaKit.Box(n,p,s,m,transform,collision);}
    GameObject Primitive(string n,PrimitiveType type,Vector3 p,Vector3 s,Material m)
    {
        var go=GameObject.CreatePrimitive(type);go.name=n;go.transform.SetParent(transform);go.transform.position=p;go.transform.localScale=s;go.GetComponent<Renderer>().sharedMaterial=m;var c=go.GetComponent<Collider>();c.enabled=false;Destroy(c);return go;
    }
    void FernBed(Vector3 p,float radius,int count,int seed)
    {
        var rng=new System.Random(seed);var v=new List<Vector3>();var t=new List<int>();
        for(int plant=0;plant<count;plant++)
        {
            Vector3 baseP=p+new Vector3(((float)rng.NextDouble()-.5f)*radius*2,0,((float)rng.NextDouble()-.5f)*radius*2);
            for(int leaf=0;leaf<9;leaf++)
            {
                float a=leaf*Mathf.PI*2/9+(float)rng.NextDouble(),length=.55f+(float)rng.NextDouble()*.8f;
                Vector3 d=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)),side=new Vector3(-d.z,0,d.x);
                for(int row=0;row<5;row++)
                {
                    float k=row/5f,q=(row+1)/5f;Vector3 c=baseP+d*length*k+Vector3.up*(Mathf.Sin(k*Mathf.PI)*.45f+ k*.15f);
                    Vector3 tip=baseP+d*length*q+Vector3.up*(Mathf.Sin(q*Mathf.PI)*.45f+q*.15f);
                    float width=Mathf.Sin((k+.07f)*Mathf.PI)*.11f;int idx=v.Count;
                    v.Add(c-side*width);v.Add(c+side*width);v.Add(tip);
                    t.AddRange(new[]{idx,idx+1,idx+2,idx+2,idx+1,idx});
                }
            }
        }
        Mesh mesh=new Mesh();mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();
        var go=new GameObject("Clustered tropical ferns");go.transform.SetParent(transform);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=seed%2==0?plants:fernLight;
    }
    void AddDust()
    {
        var go=new GameObject("Drifting humid motes");go.transform.SetParent(transform);go.transform.position=new Vector3(0,3,29);var ps=go.AddComponent<ParticleSystem>();
        var main=ps.main;main.startLifetime=12;main.startSpeed=.09f;main.startSize=.035f;main.startColor=new Color(1,.83f,.49f,.34f);main.maxParticles=500;main.simulationSpace=ParticleSystemSimulationSpace.World;
        var emission=ps.emission;emission.rateOverTime=25;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(24,6,42);
        var noise=ps.noise;noise.enabled=true;noise.strength=.12f;noise.frequency=.15f;
        go.GetComponent<ParticleSystemRenderer>().material=SoftParticleMaterial();
    }
    void AddBirds()
    {
        for(int i=0;i<7;i++)
        {
            var go=new GameObject("Distant circling bird");go.transform.SetParent(transform);var mesh=new Mesh();mesh.vertices=new[]{new Vector3(-.6f,0,0),new Vector3(0,0,.16f),new Vector3(0,-.12f,-.14f),new Vector3(.6f,0,0)};mesh.triangles=new[]{0,1,2,2,1,3,2,1,0,3,1,2};mesh.RecalculateNormals();go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=CholaKit.darkStone;birds.Add(go.transform);
        }
    }
    void AddAmbientSound()
    {
        var audio=gameObject.AddComponent<AudioSource>();audio.loop=true;audio.volume=.17f;
        int count=22050*12;float[] samples=new float[count];var rng=new System.Random(409);float filtered=0;
        for(int i=0;i<count;i++)
        {
            float time=i/22050f;filtered=Mathf.Lerp(filtered,(float)rng.NextDouble()*2-1,.035f);
            float chirp=Mathf.Pow(Mathf.Max(0,Mathf.Sin(time*2.1f)),18)*Mathf.Sin(time*(3700+180*Mathf.Sin(time*40)));
            samples[i]=filtered*.38f+chirp*.035f;
        }
        var clip=AudioClip.Create("Original courtyard wind and insects",count,1,22050,false);clip.SetData(samples,0);audio.clip=clip;audio.Play();
    }
    void Update()
    {
        for(int i=0;i<birds.Count;i++){float a=Time.time*.065f+i*.85f;birds[i].position=new Vector3(Mathf.Cos(a)*(23+i),27+i*.7f,48+Mathf.Sin(a)*18);birds[i].rotation=Quaternion.Euler(0,-a*Mathf.Rad2Deg,Mathf.Sin(Time.time*4+i)*12);}
    }
}
