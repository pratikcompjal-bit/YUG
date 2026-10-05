using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Original procedural architectural kit. Public position arguments are world-space bases.</summary>
public static class CholaKit
{
    public static Material stone, darkStone, trim, bronze, foliage, water, earth, plaster;
    static Material bark, shadow, flame, leafLight;
    static Mesh cubeMesh, sphereMesh;
    static readonly Dictionary<string, Mesh> rounds = new Dictionary<string, Mesh>();
    static readonly Dictionary<int, Mesh> fronds = new Dictionary<int, Mesh>();
    static bool initialized;

    public static void Init()
    {
        if (initialized && stone != null) return;
        initialized = true;
        Shader shader = Resources.Load<Shader>("WeatheredStone");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Diffuse");
        Texture2D granite = Granite(false);
        Texture2D normal = Granite(true);
        stone = Mat("Weathered Chola granite", new Color(.46f, .435f, .355f), .13f, 0, shader);
        darkStone = Mat("Patinated recess stone", new Color(.24f, .27f, .235f), .10f, 0, shader);
        trim = Mat("Worn carved stone edges", new Color(.60f, .56f, .43f), .18f, 0, shader);
        trim.SetFloat("_Masonry",0);
        plaster = Mat("Weathered limewash", new Color(.53f, .49f, .38f), .08f, 0, shader);
        foreach (Material m in new [] { stone, darkStone, trim, plaster })
        {
            m.mainTexture = granite;
            m.mainTextureScale = new Vector2(2, 2);
            if (m.HasProperty("_BumpMap")) { m.SetTexture("_BumpMap", normal); m.SetFloat("_BumpScale", .40f); m.EnableKeyword("_NORMALMAP"); }
        }
        shader = Shader.Find("Standard");
        bronze = Mat("Aged temple bronze", new Color(.35f, .24f, .105f), .46f, .72f, shader);
        foliage = Mat("Palm deep green", new Color(.13f, .26f, .115f), .10f, 0, shader);
        leafLight = Mat("Palm sunlit leaf", new Color(.27f, .37f, .16f), .08f, 0, shader);
        bark = Mat("Palm bark", new Color(.29f, .24f, .16f), .05f, 0, shader);
        water = Mat("Temple tank jade water", new Color(.035f, .14f, .115f), .84f, .20f, Resources.Load<Shader>("TempleWater"));
        earth = Mat("Damp laterite courtyard", new Color(.23f, .245f, .17f), .05f, 0, Resources.Load<Shader>("WeatheredStone"));
        earth.SetFloat("_Masonry",0); earth.SetFloat("_Moss",.75f);
        earth.mainTexture = granite; earth.mainTextureScale = new Vector2(14, 14);
        shadow = Mat("Niche darkness", new Color(.085f, .071f, .054f), .02f, 0, shader);
        flame = Mat("Oil flame warm emission", new Color(1f, .47f, .075f), .1f, 0, shader);
        flame.EnableKeyword("_EMISSION"); flame.SetColor("_EmissionColor", new Color(1f, .38f, .035f) * 4.0f);
        cubeMesh = MakeCube(); sphereMesh = MakeSphere(12, 8);
    }

    static Material Mat(string name, Color color, float smooth, float metallic, Shader shader)
    {
        Material m = new Material(shader); m.name = name; m.color = color;
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smooth);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
        return m;
    }

    static Texture2D Granite(bool normal)
    {
        const int n = 256;
        float[] h = new float[n * n]; Color[] pixels = new Color[n * n];
        for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
        {
            uint hash = (uint)(x * 374761393 + y * 668265263 + 104729);
            hash = (hash ^ (hash >> 13)) * 1274126177;
            float grit = (hash & 1023) / 1023f;
            float coarse = Mathf.PerlinNoise(x * .033f + 18.2f, y * .033f + 9.1f);
            float fine = Mathf.PerlinNoise(x * .19f + 7.4f, y * .19f + 3.8f);
            h[y * n + x] = Mathf.Clamp01(coarse * .55f + fine * .22f + grit * .23f);
        }
        for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
        {
            float v = h[y * n + x];
            if (normal)
            {
                float dx = h[y * n + (x + 1) % n] - h[y * n + (x + n - 1) % n];
                float dy = h[((y + 1) % n) * n + x] - h[((y + n - 1) % n) * n + x];
                Vector3 q = new Vector3(-dx * 1.15f, -dy * 1.15f, 1).normalized;
                pixels[y * n + x] = new Color(q.x * .5f + .5f, q.y * .5f + .5f, q.z * .5f + .5f, 1);
            }
            else
            {
                float fleck = v > .68f ? .11f : (v < .29f ? -.12f : 0);
                float c = .76f + v * .30f + fleck;
                pixels[y * n + x] = new Color(c, c * .985f, c * .955f, 1);
            }
        }
        Texture2D tex = new Texture2D(n, n, TextureFormat.RGBA32, true, normal);
        tex.name = normal ? "Procedural granite normal 256" : "Procedural granite albedo 256";
        tex.wrapMode = TextureWrapMode.Repeat; tex.filterMode = FilterMode.Trilinear; tex.anisoLevel = 4;
        tex.SetPixels(pixels); tex.Apply(true, false); return tex;
    }

    public static GameObject Box(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent = null, bool collider = true)
    {
        Init();
        GameObject go = Shape(name, cubeMesh, mat, parent);
        go.transform.position = pos + Vector3.up * scale.y * .5f;
        go.transform.rotation = Quaternion.identity; go.transform.localScale = scale;
        if (collider) go.AddComponent<BoxCollider>();
        return go;
    }

    static GameObject Root(string name, Vector3 pos, Transform parent)
    {
        GameObject go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.position = pos; return go;
    }
    static GameObject Shape(string name, Mesh mesh, Material mat, Transform parent)
    {
        GameObject go = new GameObject(name); go.transform.SetParent(parent, false);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer renderer = go.AddComponent<MeshRenderer>(); renderer.sharedMaterial = mat;
        renderer.shadowCastingMode = ShadowCastingMode.On; renderer.receiveShadows = true; return go;
    }
    static GameObject Block(string name, Vector3 bottom, Vector3 scale, Material mat, Transform parent)
    {
        GameObject go = Shape(name, cubeMesh, mat, parent);
        go.transform.localPosition = bottom + Vector3.up * scale.y * .5f; go.transform.localScale = scale; return go;
    }
    static GameObject Oval(string name, Vector3 center, Vector3 radii, Material mat, Transform parent)
    {
        GameObject go = Shape(name, sphereMesh, mat, parent); go.transform.localPosition = center; go.transform.localScale = radii; return go;
    }
    static GameObject Round(string name, Vector3 bottom, float height, float radius, float topRadius, int sides, Material mat, Transform parent)
    {
        string key = sides + ":" + Mathf.RoundToInt(topRadius / radius * 1000);
        Mesh mesh;
        if (!rounds.TryGetValue(key, out mesh)) { mesh = MakeRound(sides, topRadius / radius); rounds.Add(key, mesh); }
        GameObject go = Shape(name, mesh, mat, parent); go.transform.localPosition = bottom;
        go.transform.localScale = new Vector3(radius, height, radius); return go;
    }
    static GameObject Limb(string name, Vector3 a, Vector3 b, float radius, float endRadius, Material mat, Transform parent, int sides = 8)
    {
        GameObject go = Round(name, a, (b - a).magnitude, radius, endRadius, sides, mat, parent);
        go.transform.localRotation = Quaternion.FromToRotation(Vector3.up, b - a); return go;
    }
    static void CollisionBox(Transform root, string name, Vector3 bottom, Vector3 size)
    {
        GameObject g = new GameObject(name); g.transform.SetParent(root, false); g.transform.localPosition = bottom + Vector3.up * size.y * .5f;
        BoxCollider c = g.AddComponent<BoxCollider>(); c.size = size;
    }

    public static GameObject Pillar(Vector3 pos, float height, Transform parent = null)
    {
        Init(); GameObject root = Root("Carved octagonal stone pillar", pos, parent); Transform p = root.transform;
        float r = height * .067f; float y = 0;
        Block("Square foot", new Vector3(0,y,0), new Vector3(r*3.5f,height*.045f,r*3.5f), darkStone,p); y+=height*.045f;
        Block("Bevelled plinth", new Vector3(0,y,0), new Vector3(r*3.0f,height*.052f,r*3.0f),trim,p); y+=height*.052f;
        Block("Square pedestal",new Vector3(0,y,0),new Vector3(r*2.35f,height*.16f,r*2.35f),stone,p); y+=height*.16f;
        Round("Lower lotus collar",new Vector3(0,y,0),height*.045f,r*1.44f,r*1.13f,12,trim,p); y+=height*.045f;
        Round("Octagonal shaft",new Vector3(0,y,0),height*.43f,r,r*.89f,8,stone,p); y+=height*.43f;
        for(int i=0;i<3;i++) Round("Carved neck ring",new Vector3(0,y+i*height*.026f,0),height*.015f,r*1.13f,r*1.13f,12,trim,p);
        y+=height*.075f;
        Round("Lotus capital",new Vector3(0,y,0),height*.065f,r*.95f,r*1.70f,12,stone,p);
        for(int i=0;i<8;i++)
        {
            float a=i*Mathf.PI/4; GameObject petal=Oval("Lotus petal",new Vector3(Mathf.Cos(a)*r*1.15f,y+height*.038f,Mathf.Sin(a)*r*1.15f),new Vector3(r*.35f,height*.045f,r*.62f),trim,p);
            petal.transform.localRotation=Quaternion.Euler(0,-i*45,0);
        }
        y=height*.89f;
        Block("Cross corbel east west",new Vector3(0,y,0),new Vector3(r*4.8f,height*.055f,r*1.8f),stone,p);
        Block("Cross corbel north south",new Vector3(0,y,0),new Vector3(r*1.8f,height*.055f,r*4.8f),stone,p);
        for(int side=-1;side<=1;side+=2)
        {
            Oval("Scroll end",new Vector3(side*r*1.85f,y,r*.0f),new Vector3(r*.48f,r*.42f,r*.82f),trim,p);
            Oval("Scroll end",new Vector3(0,y,side*r*1.85f),new Vector3(r*.82f,r*.42f,r*.48f),trim,p);
        }
        Block("Abacus",new Vector3(0,height*.945f,0),new Vector3(r*4.7f,height*.055f,r*4.7f),trim,p);
        Combine(p,"Pillar"); CollisionBox(p,"Pillar collision",Vector3.zero,new Vector3(r*2.25f,height,r*2.25f)); return root;
    }

    public static GameObject Vimana(Vector3 pos, float width = 20, float height = 31, Transform parent = null)
    {
        Init(); GameObject root=Root("Nine tier Chola vimana",pos,parent); Transform p=root.transform;
        float baseH=height*.055f, sanctuaryTop=height*.30f, tierH=height*.50f/9f;
        Block("Upana foundation",Vector3.zero,new Vector3(width*1.12f,baseH*.28f,width*1.12f),darkStone,p);
        Block("Jagati plinth",new Vector3(0,baseH*.28f,0),new Vector3(width*1.08f,baseH*.30f,width*1.08f),stone,p);
        Block("Kumuda moulding",new Vector3(0,baseH*.58f,0),new Vector3(width*1.10f,baseH*.18f,width*1.10f),trim,p);
        Block("Sanctuary base course",new Vector3(0,baseH*.76f,0),new Vector3(width*1.03f,baseH*.24f,width*1.03f),darkStone,p);
        Block("Sanctuary west wall",new Vector3(-width*.445f,baseH,0),new Vector3(width*.07f,sanctuaryTop-baseH,width*.96f),stone,p);
        Block("Sanctuary east wall",new Vector3(width*.445f,baseH,0),new Vector3(width*.07f,sanctuaryTop-baseH,width*.96f),stone,p);
        Block("Sanctuary rear wall",new Vector3(0,baseH,-width*.445f),new Vector3(width*.96f,sanctuaryTop-baseH,width*.07f),stone,p);
        foreach(int side in new[]{-1,1}) Block("Sanctuary portal cheek",new Vector3(side*width*.30f,baseH,width*.445f),new Vector3(width*.36f,sanctuaryTop-baseH,width*.07f),stone,p);
        Block("Sanctuary portal header",new Vector3(0,baseH+4.7f,width*.445f),new Vector3(width*.26f,sanctuaryTop-baseH-4.7f,width*.07f),stone,p);
        for(int s=0;s<4;s++)
        {
            Quaternion q=Quaternion.Euler(0,s*90,0);
            for(int i=-2;i<=2;i++)
            {
                if(s==0 && i==0) continue;
                Vector3 v=q*new Vector3(i*width*.174f,baseH+height*.037f,width*.484f);
                Niche(p,v,q,width*.126f,(sanctuaryTop-baseH)*.76f,true);
            }
            for(int i=-3;i<=3;i++)
            {
                if(s==0 && i==0)continue;
                Vector3 v=q*new Vector3(i*width*.147f,baseH,width*.485f);
                GameObject pilaster=Block("Wall pilaster",v,new Vector3(width*.021f,sanctuaryTop-baseH,width*.025f),trim,p); pilaster.transform.localRotation=q;
            }
        }
        // The front portal is visually deep; the playable entrance chamber is placed by scene composition.
        // The central portal remains open for the animated stone doors and playable inner chamber.
        Block("Sanctum cornice",new Vector3(0,sanctuaryTop-.25f,0),new Vector3(width*1.025f,.42f,width*1.025f),trim,p);
        for(int tier=0;tier<9;tier++)
        {
            float t=tier/8f,w=width*Mathf.Lerp(.94f,.29f,t),y=sanctuaryTop+tier*tierH;
            Block("Storey "+(tier+1)+" wall",new Vector3(0,y,0),new Vector3(w,tierH*.77f,w),stone,p);
            Block("Projecting cornice",new Vector3(0,y+tierH*.72f,0),new Vector3(w+width*.038f,tierH*.19f,w+width*.038f),trim,p);
            Block("Shadow under eave",new Vector3(0,y+tierH*.68f,0),new Vector3(w+width*.022f,tierH*.065f,w+width*.022f),darkStone,p);
            Block("Upper terrace moulding",new Vector3(0,y+tierH*.91f,0),new Vector3(w+width*.016f,tierH*.075f,w+width*.016f),stone,p);
            int count=Mathf.Max(1,5-tier/2); float spacing=w/(count+1);
            for(int side=0;side<4;side++)
            {
                Quaternion q=Quaternion.Euler(0,side*90,0);
                for(int i=0;i<count;i++)
                {
                    float x=(i-(count-1)*.5f)*spacing;
                    Niche(p,q*new Vector3(x,y+tierH*.10f,w*.506f),q,Mathf.Min(spacing*.61f,tierH*.66f),tierH*.60f,tier<4);
                }
            }
            if(tier<8)
            {
                float nextW=width*Mathf.Lerp(.94f,.29f,(tier+1)/8f);
                float corner=w*.41f;
                for(int sx=-1;sx<=1;sx+=2) for(int sz=-1;sz<=1;sz+=2)
                    MiniShrine(p,new Vector3(sx*corner,y+tierH*.89f,sz*corner),Mathf.Min((w-nextW)*.65f,tierH*.76f),tierH*.66f);
            }
        }
        float summit=height*.80f, domeR=width*.145f;
        Round("Octagonal griva drum",new Vector3(0,summit,0),height*.046f,domeR*.90f,domeR*.90f,8,stone,p);
        Round("Dome lower rim",new Vector3(0,summit+height*.039f,0),height*.013f,domeR*1.14f,domeR*1.07f,16,trim,p);
        GameObject dome=Shape("Octagonal stone shikhara dome",MakeDome(8,6),stone,p);
        dome.transform.localPosition=new Vector3(0,height*.846f,0); dome.transform.localScale=new Vector3(domeR,height*.105f,domeR);
        Round("Dome crown ring",new Vector3(0,height*.943f,0),height*.009f,domeR*.22f,domeR*.18f,12,trim,p);
        Kalasha(p,new Vector3(0,height*.951f,0),height*.049f);
        Combine(p,"Vimana static");
        CollisionBox(p,"Sanctuary foundation",Vector3.zero,new Vector3(width*.96f,baseH,width*.96f));
        foreach(int side in new[]{-1,1}) {
            CollisionBox(p,"Sanctuary side",new Vector3(side*width*.445f,baseH,0),new Vector3(width*.07f,sanctuaryTop-baseH,width*.96f));
            CollisionBox(p,"Sanctuary front",new Vector3(side*width*.30f,baseH,width*.445f),new Vector3(width*.36f,sanctuaryTop-baseH,width*.07f));
        }
        CollisionBox(p,"Sanctuary back",new Vector3(0,baseH,-width*.445f),new Vector3(width*.96f,sanctuaryTop-baseH,width*.07f));
        for(int i=0;i<9;i++) { float w=width*Mathf.Lerp(.94f,.29f,i/8f); CollisionBox(p,"Tier collision",new Vector3(0,sanctuaryTop+i*tierH,0),new Vector3(w,tierH,w)); }
        return root;
    }

    static void Niche(Transform parent,Vector3 pos,Quaternion rotation,float width,float height,bool sculpture)
    {
        GameObject group=new GameObject("Carved deity niche"); group.transform.SetParent(parent,false); group.transform.localPosition=pos; group.transform.localRotation=rotation; Transform p=group.transform;
        Block("Recess",new Vector3(0,0,.012f),new Vector3(width*.66f,height*.81f,.045f),shadow,p);
        float r=width*.09f;
        Round("Left niche column",new Vector3(-width*.43f,0,.09f),height*.72f,r,r*.87f,8,trim,p);
        Round("Right niche column",new Vector3(width*.43f,0,.09f),height*.72f,r,r*.87f,8,trim,p);
        Block("Niche foot",new Vector3(0,0,.07f),new Vector3(width*1.03f,height*.09f,width*.28f),trim,p);
        Block("Niche lintel",new Vector3(0,height*.72f,.06f),new Vector3(width*1.12f,height*.09f,width*.30f),trim,p);
        Oval("Kudu arch crest",new Vector3(0,height*.83f,.08f),new Vector3(width*.38f,height*.18f,width*.14f),stone,p);
        if(sculpture)
        {
            Oval("Sculpture torso",new Vector3(0,height*.40f,.10f),new Vector3(width*.14f,height*.21f,width*.12f),darkStone,p);
            Oval("Sculpture head",new Vector3(0,height*.64f,.105f),new Vector3(width*.12f,height*.092f,width*.13f),trim,p);
            Block("Sculpture folded drape",new Vector3(0,height*.11f,.08f),new Vector3(width*.28f,height*.22f,width*.19f),stone,p);
        }
    }
    static void MiniShrine(Transform p,Vector3 pos,float width,float height)
    {
        Block("Terrace miniature shrine",pos,new Vector3(width*.84f,height*.52f,width*.84f),stone,p);
        Block("Miniature cornice",pos+Vector3.up*height*.48f,new Vector3(width,height*.12f,width),trim,p);
        GameObject dome=Shape("Miniature octagonal roof",MakeDome(8,4),stone,p); dome.transform.localPosition=pos+Vector3.up*height*.60f; dome.transform.localScale=new Vector3(width*.47f,height*.31f,width*.47f);
        Round("Miniature finial",pos+Vector3.up*height*.90f,height*.14f,width*.07f,width*.025f,8,trim,p);
    }
    static void Kalasha(Transform p,Vector3 pos,float height)
    {
        Round("Kalasha lotus foot",pos,height*.15f,height*.25f,height*.16f,12,bronze,p);
        Oval("Kalasha vessel",pos+Vector3.up*height*.40f,new Vector3(height*.22f,height*.28f,height*.22f),bronze,p);
        Round("Kalasha neck",pos+Vector3.up*height*.62f,height*.17f,height*.08f,height*.10f,12,bronze,p);
        Oval("Kalasha cap",pos+Vector3.up*height*.80f,new Vector3(height*.14f,height*.10f,height*.14f),bronze,p);
        Round("Kalasha tip",pos+Vector3.up*height*.86f,height*.14f,height*.055f,.002f,8,bronze,p);
    }

    public static GameObject Nandi(Vector3 pos,float scale=1,Transform parent=null)
    {
        Init(); GameObject root=Root("Reclining Nandi guardian",pos,parent); Transform p=root.transform;
        Block("Nandi rectangular stone dais",Vector3.zero,new Vector3(3.25f,.27f,4.35f),darkStone,p);
        Block("Nandi dais carved edge",new Vector3(0,.27f,0),new Vector3(3.40f,.11f,4.48f),trim,p);
        Oval("Reclining bull body",new Vector3(0,.94f,-.25f),new Vector3(1.02f,.62f,1.39f),darkStone,p);
        Oval("Bull shoulder hump",new Vector3(0,1.50f,.44f),new Vector3(.68f,.70f,.71f),stone,p);
        Oval("Upright bull neck",new Vector3(0,1.49f,1.01f),new Vector3(.57f,.74f,.60f),darkStone,p);
        Oval("Bull forehead",new Vector3(0,1.98f,1.35f),new Vector3(.58f,.52f,.59f),stone,p);
        Oval("Broad bull muzzle",new Vector3(0,1.72f,1.85f),new Vector3(.50f,.30f,.43f),darkStone,p);
        for(int side=-1;side<=1;side+=2)
        {
            Oval("Folded front leg",new Vector3(side*.76f,.55f,1.02f),new Vector3(.30f,.23f,.81f),stone,p);
            Oval("Front cloven hoof",new Vector3(side*.78f,.49f,1.62f),new Vector3(.27f,.17f,.26f),darkStone,p);
            Oval("Folded hind leg",new Vector3(side*.86f,.55f,-.94f),new Vector3(.34f,.25f,.73f),stone,p);
            Oval("Hind knee",new Vector3(side*1.02f,.68f,-1.12f),new Vector3(.25f,.28f,.34f),darkStone,p);
            GameObject ear=Oval("Bull ear",new Vector3(side*.64f,2.03f,1.36f),new Vector3(.39f,.13f,.21f),darkStone,p); ear.transform.localRotation=Quaternion.Euler(0,side*22,side*16);
            Vector3 h0=new Vector3(side*.38f,2.32f,1.27f),h1=new Vector3(side*.54f,2.69f,1.15f),h2=new Vector3(side*.43f,2.94f,1.12f);
            Limb("Curved bull horn base",h0,h1,.13f,.075f,trim,p); Limb("Curved bull horn tip",h1,h2,.078f,.007f,trim,p);
            Oval("Bull eye",new Vector3(side*.47f,2.06f,1.67f),new Vector3(.055f,.053f,.042f),shadow,p);
            Oval("Nostril",new Vector3(side*.22f,1.76f,2.19f),new Vector3(.057f,.046f,.027f),shadow,p);
        }
        // A beaded neck garland and pendant distinguish the guardian sculpture.
        for(int i=0;i<15;i++)
        {
            float a=Mathf.PI*2*i/15f; Oval("Carved neck garland bead",new Vector3(Mathf.Cos(a)*.595f,1.54f+Mathf.Sin(a)*.44f,1.30f),new Vector3(.09f,.09f,.09f),trim,p);
        }
        Oval("Bronze bell pendant",new Vector3(0,1.13f,1.81f),new Vector3(.14f,.18f,.11f),bronze,p);
        Limb("Tail along haunch",new Vector3(.72f,.95f,-1.24f),new Vector3(1.02f,.48f,-1.68f),.085f,.055f,darkStone,p);
        Combine(p,"Nandi"); CollisionBox(p,"Nandi dais collision",Vector3.zero,new Vector3(3.40f,.65f,4.48f));
        CollisionBox(p,"Nandi body collision",new Vector3(0,.38f,0),new Vector3(1.8f,2.05f,3.4f)); p.localScale=Vector3.one*scale; return root;
    }

    public static GameObject Palm(Vector3 pos,float height=10,Transform parent=null)
    {
        Init(); GameObject root=Root("Curved coconut palm",pos,parent); Transform p=root.transform;
        int seed=Mathf.Abs(Mathf.RoundToInt(pos.x*71+pos.z*137+height*31)); System.Random rng=new System.Random(seed);
        float lean=height*(.09f+(float)rng.NextDouble()*.045f); Vector3 last=Vector3.zero;
        const int count=13;
        for(int i=0;i<count;i++)
        {
            float t=(i+1f)/count; Vector3 next=new Vector3(lean*t*t,height*t,.2f*Mathf.Sin(t*2));
            Limb("Ringed palm trunk",last,next,Mathf.Lerp(height*.031f,height*.020f,i/(float)count),Mathf.Lerp(height*.030f,height*.019f,t),bark,p,9);
            Vector3 band=Vector3.Lerp(last,next,.86f); Limb("Trunk growth ring",band,Vector3.Lerp(last,next,.98f),height*Mathf.Lerp(.032f,.021f,t),height*Mathf.Lerp(.032f,.021f,t),darkStone,p,9); last=next;
        }
        Oval("Palm crown",last,new Vector3(height*.065f,height*.077f,height*.065f),foliage,p);
        for(int i=0;i<11;i++)
        {
            Mesh mesh; int variation=i%3;
            if(!fronds.TryGetValue(variation,out mesh)){mesh=MakeFrond(variation);fronds.Add(variation,mesh);}
            GameObject leaf=Shape("Sweeping palm frond",mesh,i%3==0?leafLight:foliage,p);
            leaf.transform.localPosition=last; leaf.transform.localRotation=Quaternion.Euler(i%2==0?-3:8,i*360f/11f+(float)rng.NextDouble()*13,0);
            float length=height*(.39f+(float)rng.NextDouble()*.12f); leaf.transform.localScale=Vector3.one*length;
        }
        for(int i=0;i<5;i++){float a=i*Mathf.PI*2/5;Oval("Coconut",last+new Vector3(Mathf.Cos(a)*.33f,-.20f,Mathf.Sin(a)*.33f),new Vector3(.17f,.21f,.17f),bark,p);}
        Combine(p,"Palm");
        CapsuleCollider col=root.AddComponent<CapsuleCollider>(); col.center=new Vector3(lean*.4f,height*.46f,0);col.height=height*.92f;col.radius=height*.030f;
        return root;
    }

    public static GameObject Shrine(Vector3 pos,float scale=1,Transform parent=null)
    {
        Init(); GameObject root=Root("Small courtyard shrine",pos,parent);Transform p=root.transform;
        Block("Shrine plinth",Vector3.zero,new Vector3(4.3f,.38f,4.3f),darkStone,p);
        Block("Shrine moulding",new Vector3(0,.38f,0),new Vector3(4.5f,.16f,4.5f),trim,p);
        Block("Shrine sanctuary",new Vector3(0,.54f,0),new Vector3(3.65f,3.75f,3.65f),stone,p);
        Niche(p,new Vector3(0,.62f,1.85f),Quaternion.identity,1.65f,2.9f,false);
        for(int side=1;side<4;side++)Niche(p,Quaternion.Euler(0,side*90,0)*new Vector3(0,.92f,1.85f),Quaternion.Euler(0,side*90,0),1.40f,2.2f,true);
        for(int i=0;i<3;i++)
        {
            float w=4.15f-i*.72f,y=4.2f+i*.61f;
            Block("Shrine stepped cornice",new Vector3(0,y,0),new Vector3(w,.16f,w),trim,p);
            Block("Shrine upper course",new Vector3(0,y+.16f,0),new Vector3(w-.36f,.47f,w-.36f),stone,p);
        }
        GameObject roof=Shape("Shrine stone dome",MakeDome(8,5),stone,p);roof.transform.localPosition=new Vector3(0,6.02f,0);roof.transform.localScale=new Vector3(1.20f,1.12f,1.20f);
        Kalasha(p,new Vector3(0,7.08f,0),.75f);Combine(p,"Courtyard shrine");CollisionBox(p,"Shrine collision",Vector3.zero,new Vector3(4.3f,4.3f,4.3f));p.localScale=Vector3.one*scale;return root;
    }

    public static GameObject Lamp(Vector3 pos,Transform parent=null)
    {
        Init();GameObject root=Root("Bronze standing oil lamp",pos,parent);Transform p=root.transform;
        Round("Lamp base",Vector3.zero,.12f,.36f,.29f,16,bronze,p);
        Round("Lamp tapered foot",new Vector3(0,.12f,0),.22f,.24f,.10f,16,bronze,p);
        Round("Lamp stem",new Vector3(0,.34f,0),1.12f,.063f,.047f,12,bronze,p);
        for(int i=0;i<3;i++)Oval("Turned bronze ring",new Vector3(0,.47f+i*.34f,0),new Vector3(.105f,.052f,.105f),bronze,p);
        Round("Oil bowl",new Vector3(0,1.43f,0),.13f,.12f,.39f,16,bronze,p);
        Round("Oil bowl rim",new Vector3(0,1.55f,0),.032f,.41f,.41f,16,bronze,p);
        Oval("Oil",new Vector3(0,1.57f,0),new Vector3(.33f,.017f,.33f),shadow,p);
        Combine(p,"Bronze lamp");
        for(int i=0;i<4;i++)
        {
            float a=i*Mathf.PI*.5f;Vector3 v=new Vector3(Mathf.Cos(a)*.29f,1.63f,Mathf.Sin(a)*.29f);
            Oval("Oil flame",v,new Vector3(.032f,.13f,.032f),flame,p);
        }
        GameObject lightGo=new GameObject("Warm oil light");lightGo.transform.SetParent(p,false);lightGo.transform.localPosition=new Vector3(0,1.85f,0);
        Light l=lightGo.AddComponent<Light>();l.type=LightType.Point;l.color=new Color(1f,.55f,.22f);l.intensity=1.8f;l.range=5.5f;l.shadows=LightShadows.None;
        return root;
    }

    /// <summary>Combines generated static decoration into one renderer per shared material.</summary>
    static void Combine(Transform root,string label)
    {
        MeshFilter[] filters=root.GetComponentsInChildren<MeshFilter>();
        Dictionary<Material,List<CombineInstance>> groups=new Dictionary<Material,List<CombineInstance>>();
        List<GameObject> oldChildren=new List<GameObject>();foreach(Transform child in root)oldChildren.Add(child.gameObject);
        Matrix4x4 toLocal=root.worldToLocalMatrix;
        foreach(MeshFilter f in filters)
        {
            MeshRenderer r=f.GetComponent<MeshRenderer>();if(r==null||f.sharedMesh==null)continue;
            Material m=r.sharedMaterial;List<CombineInstance> list;if(!groups.TryGetValue(m,out list)){list=new List<CombineInstance>();groups.Add(m,list);}
            CombineInstance ci=new CombineInstance();ci.mesh=f.sharedMesh;ci.transform=toLocal*f.transform.localToWorldMatrix;list.Add(ci);
        }
        foreach(GameObject old in oldChildren)old.SetActive(false);
        foreach(KeyValuePair<Material,List<CombineInstance>> pair in groups)
        {
            Mesh mesh=new Mesh();mesh.name=label+" / "+pair.Key.name;mesh.indexFormat=IndexFormat.UInt32;
            mesh.CombineMeshes(pair.Value.ToArray(),true,true,false);mesh.RecalculateBounds();
            Shape(mesh.name,mesh,pair.Key,root);
        }
        foreach(GameObject old in oldChildren)UnityEngine.Object.Destroy(old);
    }

    static Mesh MakeCube()
    {
        var v=new List<Vector3>();var uv=new List<Vector2>();var tr=new List<int>();
        Vector3[] axes={Vector3.right,Vector3.up,Vector3.forward};const float inner=.478f;
        System.Action<Vector3[]> face=points=>{
            Vector3 center=Vector3.zero;foreach(Vector3 point in points)center+=point;center/=points.Length;
            if(Vector3.Dot(Vector3.Cross(points[1]-points[0],points[2]-points[0]),center)<0)Array.Reverse(points);
            int index=v.Count;for(int i=0;i<points.Length;i++){v.Add(points[i]);uv.Add(new Vector2(i==1||i==2?1:0,i>=2?1:0));}
            for(int i=1;i<points.Length-1;i++)tr.AddRange(new[]{index,index+i,index+i+1});
        };
        for(int axis=0;axis<3;axis++)foreach(int sign in new[]{-1,1}){
            Vector3 n=axes[axis]*sign*.5f,u=axes[(axis+1)%3]*inner,w=axes[(axis+2)%3]*inner;
            face(new[]{n-u-w,n+u-w,n+u+w,n-u+w});
        }
        for(int edge=0;edge<3;edge++)foreach(int s in new[]{-1,1})foreach(int q in new[]{-1,1}){
            Vector3 a=axes[(edge+1)%3]*s,b=axes[(edge+2)%3]*q,d=axes[edge]*inner;
            face(new[]{a*.5f+b*inner-d,a*.5f+b*inner+d,a*inner+b*.5f+d,a*inner+b*.5f-d});
        }
        foreach(int x in new[]{-1,1})foreach(int y in new[]{-1,1})foreach(int z in new[]{-1,1})
            face(new[]{new Vector3(x*.5f,y*inner,z*inner),new Vector3(x*inner,y*.5f,z*inner),new Vector3(x*inner,y*inner,z*.5f)});
        Mesh m=new Mesh();m.name="Bevelled dressed granite module";m.SetVertices(v);m.SetUVs(0,uv);m.SetTriangles(tr,0);m.RecalculateNormals();m.RecalculateTangents();return m;
    }
    static Mesh MakeSphere(int sides,int rows)
    {
        List<Vector3> v=new List<Vector3>();List<Vector2> uv=new List<Vector2>();List<int> t=new List<int>();
        for(int y=0;y<=rows;y++){float phi=Mathf.PI*y/rows;for(int x=0;x<=sides;x++){float a=Mathf.PI*2*x/sides;v.Add(new Vector3(Mathf.Sin(phi)*Mathf.Cos(a),Mathf.Cos(phi),Mathf.Sin(phi)*Mathf.Sin(a)));uv.Add(new Vector2((float)x/sides,(float)y/rows));}}
        for(int y=0;y<rows;y++)for(int x=0;x<sides;x++){int a=y*(sides+1)+x,b=a+sides+1;t.Add(a);t.Add(a+1);t.Add(b);t.Add(a+1);t.Add(b+1);t.Add(b);}
        Mesh m=new Mesh();m.name="Shared carved ellipsoid";m.SetVertices(v);m.SetUVs(0,uv);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateTangents();return m;
    }
    static Mesh MakeRound(int sides,float taper)
    {
        List<Vector3> v=new List<Vector3>();List<Vector2> uv=new List<Vector2>();List<int> tr=new List<int>();
        for(int i=0;i<sides;i++)
        {
            float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;int k=v.Count;
            v.Add(new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)));v.Add(new Vector3(Mathf.Cos(a)*taper,1,Mathf.Sin(a)*taper));v.Add(new Vector3(Mathf.Cos(b)*taper,1,Mathf.Sin(b)*taper));v.Add(new Vector3(Mathf.Cos(b),0,Mathf.Sin(b)));
            uv.Add(new Vector2((float)i/sides,0));uv.Add(new Vector2((float)i/sides,1));uv.Add(new Vector2((float)(i+1)/sides,1));uv.Add(new Vector2((float)(i+1)/sides,0));
            tr.Add(k);tr.Add(k+1);tr.Add(k+2);tr.Add(k);tr.Add(k+2);tr.Add(k+3);
            k=v.Count;v.Add(Vector3.zero);v.Add(new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)));v.Add(new Vector3(Mathf.Cos(b),0,Mathf.Sin(b)));uv.Add(Vector2.zero);uv.Add(Vector2.zero);uv.Add(Vector2.one);tr.Add(k);tr.Add(k+1);tr.Add(k+2);
            k=v.Count;v.Add(Vector3.up);v.Add(new Vector3(Mathf.Cos(b)*taper,1,Mathf.Sin(b)*taper));v.Add(new Vector3(Mathf.Cos(a)*taper,1,Mathf.Sin(a)*taper));uv.Add(Vector2.zero);uv.Add(Vector2.zero);uv.Add(Vector2.one);tr.Add(k);tr.Add(k+1);tr.Add(k+2);
        }
        Mesh m=new Mesh();m.name="Shared "+sides+" sided stone profile";m.SetVertices(v);m.SetUVs(0,uv);m.SetTriangles(tr,0);m.RecalculateNormals();m.RecalculateTangents();return m;
    }
    static readonly Dictionary<string,Mesh> domeMeshes=new Dictionary<string,Mesh>();
    static Mesh MakeDome(int sides,int rows)
    {
        string key=sides+":"+rows;Mesh cached;if(domeMeshes.TryGetValue(key,out cached))return cached;
        List<Vector3> v=new List<Vector3>();List<Vector2> uv=new List<Vector2>();List<int> tr=new List<int>();
        for(int j=0;j<=rows;j++)
        {
            float t=(float)j/rows;float r=Mathf.Cos(t*Mathf.PI*.5f);float y=Mathf.Sin(t*Mathf.PI*.5f);
            for(int i=0;i<=sides;i++){float a=(i+.5f)*Mathf.PI*2/sides;v.Add(new Vector3(Mathf.Cos(a)*r,y,Mathf.Sin(a)*r));uv.Add(new Vector2((float)i/sides,t));}
        }
        for(int j=0;j<rows;j++)for(int i=0;i<sides;i++){int a=j*(sides+1)+i,b=a+sides+1;tr.Add(a);tr.Add(b);tr.Add(a+1);tr.Add(a+1);tr.Add(b);tr.Add(b+1);}
        Mesh mesh=new Mesh();mesh.name="Octagonal hemispherical shikhara";mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(tr,0);mesh.RecalculateNormals();mesh.RecalculateTangents();domeMeshes[key]=mesh;return mesh;
    }
    static Mesh MakeFrond(int variation)
    {
        // Curved feather leaves have individual tapered leaflets and explicitly doubled faces.
        List<Vector3> v=new List<Vector3>();List<Vector2> uv=new List<Vector2>();List<int> tr=new List<int>();
        for(int i=0;i<18;i++)
        {
            float t=.07f+i*.049f;float next=t+.075f;
            float width=Mathf.Sin(t*Mathf.PI)*(.19f+variation*.018f);
            Vector3 center=new Vector3(0,FrondY(t,variation),t),tip=new Vector3(0,FrondY(next,variation),next);
            for(int side=-1;side<=1;side+=2)
            {
                Vector3 outer=new Vector3(side*width,FrondY(t+.13f,variation)-width*.28f,t+.13f);
                Vector3 rear=new Vector3(side*width*.91f,FrondY(t+.17f,variation)-width*.33f,t+.17f);
                int k=v.Count;v.Add(center);v.Add(outer);v.Add(rear);v.Add(tip);uv.Add(new Vector2(.5f,t));uv.Add(new Vector2(side<0?0:1,t));uv.Add(new Vector2(side<0?0:1,next));uv.Add(new Vector2(.5f,next));
                tr.Add(k);tr.Add(k+1);tr.Add(k+2);tr.Add(k);tr.Add(k+2);tr.Add(k+3);
                // Duplicate back vertices so leaf tops retain proper lighting normals.
                int back=v.Count;v.Add(center);v.Add(outer);v.Add(rear);v.Add(tip);uv.Add(new Vector2(.5f,t));uv.Add(new Vector2(0,t));uv.Add(new Vector2(1,next));uv.Add(new Vector2(.5f,next));
                tr.Add(back+2);tr.Add(back+1);tr.Add(back);tr.Add(back+3);tr.Add(back+2);tr.Add(back);
            }
        }
        Mesh m=new Mesh();m.name="Shared curved palm frond "+variation;m.SetVertices(v);m.SetUVs(0,uv);m.SetTriangles(tr,0);m.RecalculateNormals();m.RecalculateTangents();return m;
    }
    static float FrondY(float t,int variation){return Mathf.Sin(t*Mathf.PI)*.15f-t*t*(.24f+variation*.04f);}
}
