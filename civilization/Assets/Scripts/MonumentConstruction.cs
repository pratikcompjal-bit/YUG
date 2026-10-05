using System.Collections;
using UnityEngine;

/// <summary>Three recovered architectural assemblies physically rebuild a ceremonial torana.</summary>
public class MonumentConstruction : MonoBehaviour
{
    public bool Moving { get; private set; }
    public bool DoorOpen { get; private set; }
    readonly Transform[] pieces = new Transform[3];
    readonly Vector3[] seated = {new Vector3(0,1.12f,37),new Vector3(0,5.32f,37),new Vector3(0,6.05f,37)};
    readonly Transform[] doors = new Transform[2];
    Vector3[] sources, stations;
    ParticleSystem dust;
    AudioSource rumble;
    int collected, placed;
    public void CancelMotion(){Moving=false;}

    public void Initialize(Vector3[] sourcePoints, Vector3[] workPoints)
    {
        sources=sourcePoints;stations=workPoints;
        CholaKit.Box("Broken ceremonial foundation",new Vector3(0,.9f,37),new Vector3(5.7f,.22f,2.8f),CholaKit.darkStone,transform);
        for(int i=0;i<3;i++){var root=new GameObject(new[]{"Recovered paired pillar shafts","Recovered lotus lintel","Recovered bronze crown"}[i]);root.transform.SetParent(transform);pieces[i]=root.transform;}
        foreach(int s in new[]{-1,1}) CholaKit.Pillar(new Vector3(s*1.8f,0,0),4.2f,pieces[0]);
        CholaKit.Box("Lintel load-bearing beam",Vector3.zero,new Vector3(5,.40f,1.2f),CholaKit.stone,pieces[1]);
        CholaKit.Box("Lotus cornice",Vector3.up*.4f,new Vector3(5.6f,.22f,1.5f),CholaKit.trim,pieces[1]);
        for(int i=-3;i<=3;i++) Rosette(new Vector3(i*.66f,.23f,-.63f),.20f,pieces[1]);
        for(int i=0;i<4;i++) Part("Crown lotus tier",PrimitiveType.Cylinder,new Vector3(0,.07f+i*.12f,0),new Vector3(1.1f-i*.2f,.07f,1.1f-i*.2f),CholaKit.bronze,pieces[2]);
        Part("Kalasha vessel",PrimitiveType.Sphere,new Vector3(0,.65f,0),new Vector3(.7f,.85f,.7f),CholaKit.bronze,pieces[2]);
        Part("Kalasha spire",PrimitiveType.Cylinder,new Vector3(0,1.25f,0),new Vector3(.1f,.4f,.1f),CholaKit.bronze,pieces[2]);
        foreach(Transform piece in pieces) foreach(Collider c in piece.GetComponentsInChildren<Collider>()) c.enabled=false;
        // A damaged matching monument and the low relief provide a visual construction diagram.
        CholaKit.Pillar(new Vector3(7,.9f,39),2.1f,transform);
        CholaKit.Box("Fallen matching beam",new Vector3(8,.9f,37),new Vector3(1,.6f,3.4f),CholaKit.stone,transform);
        var relief=new GameObject("Completed monument relief");relief.transform.SetParent(transform);relief.transform.position=new Vector3(-4,.45f,3.7f);
        foreach(int s in new[]{-1,1}) CholaKit.Box("Relief upright",new Vector3(-4+s*.37f,.45f,3.72f),new Vector3(.12f,.74f,.09f),CholaKit.trim,transform,false);
        CholaKit.Box("Relief lintel",new Vector3(-4,1.19f,3.72f),new Vector3(.95f,.12f,.09f),CholaKit.trim,transform,false);
        Part("Relief crown",PrimitiveType.Sphere,new Vector3(-4,1.46f,3.7f),new Vector3(.15f,.24f,.08f),CholaKit.bronze,transform);
        // New sanctuary floor is reached by an uninterrupted sequence of shallow steps.
        for(int i=0;i<5;i++) CholaKit.Box("Sanctuary approach step",new Vector3(0,.9f,42.4f+i*.40f),new Vector3(4.2f,.172f*(i+1),.42f),CholaKit.trim,transform);
        CholaKit.Box("Sanctuary threshold landing",new Vector3(0,.9f,44.3f),new Vector3(4.2f,.86f,.65f),CholaKit.trim,transform);
        for(int i=0;i<2;i++)
        {
            int sign=i==0?-1:1;
            doors[i]=CholaKit.Box("Massive moving sanctuary door",new Vector3(sign*1.18f,1.76f,44.7f),new Vector3(2.35f,4.65f,.7f),CholaKit.darkStone,transform).transform;
            for(int row=0;row<3;row++)
            {
                var panel=CholaKit.Box("Door inset relief",new Vector3(sign*1.18f,2.0f+row*1.38f,44.30f),new Vector3(1.85f,1.13f,.15f),CholaKit.stone,transform,false);
                panel.transform.SetParent(doors[i],true);
                Rosette(new Vector3(sign*1.18f,2.57f+row*1.38f,44.19f),.33f,doors[i],true);
            }
        }
        var dustGo=new GameObject("Settling granite dust");dustGo.transform.SetParent(transform);dust=dustGo.AddComponent<ParticleSystem>();
        dust.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var main=dust.main;main.startLifetime=2.8f;main.startSpeed=.55f;main.startSize=.4f;main.startColor=new Color(.65f,.57f,.40f,.28f);main.maxParticles=300;main.simulationSpace=ParticleSystemSimulationSpace.World;
        var emission=dust.emission;emission.enabled=false;var shape=dust.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(4,.5f,2);
        var renderer=dust.GetComponent<ParticleSystemRenderer>();renderer.material=TempleAtmosphere.SoftParticleMaterial();
        rumble=gameObject.AddComponent<AudioSource>();var clip=AudioClip.Create("Sliding stone and bronze resonance",44100,1,22050,false);float[] data=new float[44100];
        var random=new System.Random(59);for(int i=0;i<data.Length;i++){float t=i/22050f;data[i]=(float)((random.NextDouble()-.5)*.10+Mathf.Sin(t*190)*.055)*Mathf.Sin(Mathf.PI*i/data.Length);}
        clip.SetData(data,0);rumble.clip=clip;rumble.volume=.7f;
        SetState(0,0,false);
    }

    public void SetState(int recovered,int built,bool opened)
    {
        collected=recovered;placed=built;
        if(!Moving) for(int i=0;i<3;i++)
        {
            bool seatedNow=i<built,found=(recovered&(1<<i))!=0;
            pieces[i].position=seatedNow?seated[i]:found?stations[i]+Vector3.up*1.1f:sources[i]+Vector3.up*.7f;
            pieces[i].rotation=seatedNow||found?Quaternion.identity:Quaternion.Euler(0,i*37,i==0?90:0);
            pieces[i].localScale=Vector3.one*(seatedNow?1:found?.24f:i==0?.45f:.6f);
            foreach(Collider c in pieces[i].GetComponentsInChildren<Collider>()) c.enabled=seatedNow;
        }
        DoorOpen=opened;
        for(int i=0;i<2;i++) doors[i].position=new Vector3((i==0?-1:1)*(opened?3.7f:1.18f),4.085f,44.7f);
    }

    public IEnumerator Place(int index)
    {
        Moving=true;Transform piece=pieces[index];Vector3 from=piece.position;Vector3 scale=piece.localScale;
        rumble.Play();
        for(float t=0;t<1;t+=Time.deltaTime/2.2f)
        {
            float eased=t*t*(3-2*t);piece.position=Vector3.Lerp(from,seated[index],eased)+Vector3.up*Mathf.Sin(t*Mathf.PI)*1.2f;
            piece.localScale=Vector3.Lerp(scale,Vector3.one,eased);piece.rotation=Quaternion.Slerp(piece.rotation,Quaternion.identity,eased);yield return null;
        }
        piece.position=seated[index];piece.localScale=Vector3.one;Moving=false;placed=index+1;
        foreach(Collider c in piece.GetComponentsInChildren<Collider>())c.enabled=true;
        Puff(seated[index]);
    }

    public IEnumerator OpenDoor()
    {
        rumble.Play();Puff(new Vector3(0,5.5f,44.5f));
        for(float t=0;t<1;t+=Time.deltaTime/4)
        {
            float x=Mathf.Lerp(1.18f,3.7f,Mathf.SmoothStep(0,1,t));
            for(int i=0;i<2;i++)doors[i].position=new Vector3((i==0?-1:1)*x,4.085f,44.7f);
            yield return null;
        }
        DoorOpen=true;
        for(int i=0;i<2;i++)doors[i].position=new Vector3((i==0?-1:1)*3.7f,4.085f,44.7f);
    }
    public void Puff(Vector3 position){dust.transform.position=position;dust.Emit(80);}

    static GameObject Part(string name,PrimitiveType type,Vector3 pos,Vector3 scale,Material material,Transform parent)
    {
        var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=pos;go.transform.localScale=scale;
        var c=go.GetComponent<Collider>();c.enabled=false;Destroy(c);go.GetComponent<Renderer>().sharedMaterial=material;return go;
    }
    static void Rosette(Vector3 p,float radius,Transform parent,bool world=false)
    {
        for(int i=0;i<8;i++)
        {
            float a=i*Mathf.PI/4;var petal=Part("Carved lotus petal",PrimitiveType.Sphere,p+new Vector3(Mathf.Sin(a),Mathf.Cos(a),0)*radius*.55f,new Vector3(radius*.35f,radius,.07f),CholaKit.trim,parent);
            if(world){petal.transform.position=p+new Vector3(Mathf.Sin(a),Mathf.Cos(a),0)*radius*.55f;Vector3 s=parent.lossyScale;petal.transform.localScale=new Vector3(radius*.35f/s.x,radius/s.y,.07f/s.z);}
            petal.transform.rotation=Quaternion.Euler(0,0,-i*45);
        }
    }
}
