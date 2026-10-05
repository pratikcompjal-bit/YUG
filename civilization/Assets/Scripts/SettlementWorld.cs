using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A fictional playable village. Resource rules live here, not in its visual actors.
public class SettlementWorld : MonoBehaviour
{
    [Serializable] public class State { public int version=1,grain,wood,stone,coins=3; public bool met,irrigation,granary,complete; }
    public State state=new State();
    public class Site { public string id,title; public Vector3 point; public float availableAt; public GameObject visual; }
    public List<Site> sites=new List<Site>();
    public TempleWorld world; public string savePath; public bool Journal;
    Material lime,wood,roof,clay,path,cloth,grass;
    Transform staticRoot; GameObject repairedCanal,filledGranary;
    Site nearby; string notice=""; float noticeUntil; GUIStyle title,text,small,button; Texture2D panel;
    bool high=true; bool verify; float measuredFps;
    public Vector3 StewardPoint=new Vector3(-4,0,-78),CanalPoint=new Vector3(-41,0,-48),GranaryPoint=new Vector3(21,0,-38),MarketPoint=new Vector3(8,0,-55);

    void Start()
    {
        world=GetComponent<TempleWorld>(); verify=Array.IndexOf(Environment.GetCommandLineArgs(),"-verifySettlement")>=0;
        savePath=Path.Combine(Path.GetDirectoryName(world.SavePath),world.Verifying?"settlement-test.json":"settlement.json");
        staticRoot=new GameObject("Settlement static architecture").transform;
        lime=Material("Sun-worn lime plaster",new Color(.62f,.57f,.44f));
        wood=Material("Dark palmyra timber",new Color(.20f,.14f,.085f));
        roof=Material("Palm-leaf thatch",new Color(.34f,.29f,.14f));
        clay=Material("Fired clay",new Color(.46f,.24f,.13f));
        path=Material("Packed village earth",new Color(.39f,.35f,.23f));
        cloth=Material("Indigo awning",new Color(.17f,.25f,.27f));
        grass=Material("Rice blades",new Color(.30f,.39f,.16f));
        BuildVillage();BuildFields();BuildSites();BuildPeople();BuildLandscape();
        QualitySettings.shadowDistance=110;QualitySettings.shadowCascades=4;QualitySettings.antiAliasing=4;
        Camera.main.farClipPlane=360;
        StaticBatchingUtility.Combine(staticRoot.gameObject);
        if(verify)gameObject.AddComponent<SettlementVerification>();
    }
    Material Material(string name,Color color)
    {
        var m=new Material(CholaKit.plaster);m.name=name;m.color=color;m.SetFloat("_Masonry",0);m.SetFloat("_Moss",.12f);return m;
    }
    GameObject Box(string name,Vector3 p,Vector3 scale,Material m,bool solid=true,Transform parent=null)
    { return CholaKit.Box(name,p,scale,m,parent==null?staticRoot:parent,solid); }
    void BuildVillage()
    {
        Box("Processional road",new Vector3(0,.012f,-59),new Vector3(10,.025f,86),path,false);
        foreach(float z in new[]{-42f,-67f,-92f})Box("Cross street",new Vector3(0,.011f,z),new Vector3(98,.024f,5),path,false);
        for(int side=-1;side<=1;side+=2)for(int row=0;row<3;row++)for(int col=0;col<2;col++)
            House(new Vector3(side*(19+col*22),0,-29-row*25),row*2+col,side);
        // Roofed market stalls leave the central processional road clear.
        foreach(int side in new[]{-1,1})for(int i=0;i<3;i++)
        {
            Vector3 p=new Vector3(side*10,0,-49-i*5);
            foreach(float x in new[]{-1.4f,1.4f})foreach(float z in new[]{-1.4f,1.4f})Box("Market timber post",p+new Vector3(x,0,z),new Vector3(.13f,2.8f,.13f),wood);
            var awning=Box("Woven market canopy",p+Vector3.up*2.7f,new Vector3(3.8f,.07f,3.8f),i%2==0?cloth:roof,false);awning.transform.Rotate(0,0,side*5);
            Box("Market counter",p+new Vector3(0,0,.4f),new Vector3(2.8f,.8f,1.2f),wood);
            for(int n=0;n<5;n++)Pot(p+new Vector3(-1+n*.45f,.82f,.4f),.25f,i==0?clay:CholaKit.bronze);
        }
        // Granary sits beside the market; the service point remains in the street.
        filledGranary=new GameObject("Stocked grain baskets");
        for(int i=0;i<6;i++)Pot(new Vector3(21+(i%3)*.75f,.1f,-34-(i/3)*.7f),.45f,roof,filledGranary.transform);
        filledGranary.SetActive(false);
        for(int i=0;i<3;i++)Box("Irrigation sluice timber",new Vector3(-43+i*.5f,.1f,-48),new Vector3(.3f,.8f,3),wood);
        repairedCanal=new GameObject("Restored irrigation water");
        Box("Channel water",new Vector3(-47,.03f,-55),new Vector3(2,.06f,34),CholaKit.water,false,repairedCanal.transform);repairedCanal.SetActive(false);
        // Low banks and a shallow irrigation channel, with a walkable crossing.
        foreach(float x in new[]{-48.3f,-45.7f})Box("Irrigation bank",new Vector3(x,0,-55),new Vector3(.45f,.27f,34),CholaKit.darkStone);
        Box("Canal footbridge",new Vector3(-47,.28f,-47),new Vector3(4,.15f,3),wood);
        // Well on the western square.
        for(int i=0;i<16;i++){float a=i*Mathf.PI/8;var b=Box("Well rim",new Vector3(-15+Mathf.Cos(a)*1.5f,0,-89+Mathf.Sin(a)*1.5f),new Vector3(.7f,.8f,.5f),CholaKit.stone);b.transform.Rotate(0,-a*Mathf.Rad2Deg,0);}
        foreach(float x in new[]{-17f,-13f})Box("Well frame",new Vector3(x,0,-89),new Vector3(.2f,3.2f,.2f),wood);
        Box("Well crossbeam",new Vector3(-15,3,-89),new Vector3(4.5f,.25f,.25f),wood);
    }
    void House(Vector3 p,int variation,int side)
    {
        float w=7+variation%2,d=8;
        Box("House stone plinth",p,new Vector3(w+.7f,.22f,d+.7f),CholaKit.darkStone);
        Box("Limewashed rear wall",p+new Vector3(0,.22f,3.8f),new Vector3(w,3.1f,.35f),lime);
        foreach(float x in new[]{-w/2,w/2})Box("Limewashed side wall",p+new Vector3(x,.22f,0),new Vector3(.35f,3.1f,d),lime);
        foreach(int s in new[]{-1,1})Box("Doorway wall",p+new Vector3(s*(w+1.5f)/4,.22f,-3.8f),new Vector3((w-1.5f)/2,3.1f,.35f),lime);
        Box("Door lintel",p+new Vector3(0,2.5f,-3.8f),new Vector3(1.8f,.55f,.5f),wood);
        Box("House door",p+new Vector3(.4f,.22f,-3.9f),new Vector3(.8f,2.25f,.12f),wood);
        Roof(p+Vector3.up*3.3f,w+1.3f,d+1.5f,2.1f);
        foreach(float x in new[]{-2.8f,2.8f})Box("Veranda post",p+new Vector3(x,0,-5.5f),new Vector3(.14f,2.4f,.14f),wood);
        var verandah=Box("Shaded veranda",p+new Vector3(0,2.4f,-4.7f),new Vector3(w+1,.12f,3),roof,false);verandah.transform.Rotate(-13,0,0);
        for(int i=0;i<2;i++)Pot(p+new Vector3(-2+i*.8f,.25f,-4.5f),.4f,clay);
        for(int k=0;k<3;k++)Box("Timber window slat",p+new Vector3(w/2+.19f,1.3f+k*.2f,.4f),new Vector3(.05f,.08f,1.1f),wood,false);
    }
    void Roof(Vector3 p,float width,float depth,float height)
    {
        var go=new GameObject("Layered palm-leaf roof");go.transform.SetParent(staticRoot);go.transform.position=p;
        var mesh=new Mesh();mesh.vertices=new[]{new Vector3(-width/2,0,-depth/2),new Vector3(width/2,0,-depth/2),new Vector3(0,height,-depth/2),new Vector3(-width/2,0,depth/2),new Vector3(width/2,0,depth/2),new Vector3(0,height,depth/2)};
        mesh.triangles=new[]{0,2,1,3,4,5,0,3,5,0,5,2,2,5,4,2,4,1};mesh.RecalculateNormals();
        go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=roof;
        for(int side=-1;side<=1;side+=2)for(int i=0;i<7;i++){float t=(i+.3f)/7;Box("Thatch binding",p+new Vector3(side*width*.5f*t,height*(1-t),0),new Vector3(.065f,.06f,depth+.1f),wood,false);}
    }
    void Pot(Vector3 p,float size,Material mat,Transform parent=null)
    {
        var pot=GameObject.CreatePrimitive(PrimitiveType.Sphere);pot.name="Handmade storage vessel";pot.transform.SetParent(parent==null?staticRoot:parent);pot.transform.position=p+Vector3.up*size*.6f;pot.transform.localScale=new Vector3(size,size*1.2f,size);pot.GetComponent<Renderer>().sharedMaterial=mat;Destroy(pot.GetComponent<Collider>());
    }
    void BuildFields()
    {
        for(int patch=0;patch<4;patch++)
        {
            Vector3 p=new Vector3(-65+(patch%2)*10,0,-40-(patch/2)*16);
            Box("Flooded paddy plot",p+Vector3.up*.025f,new Vector3(8,.02f,12),CholaKit.water,false);
            foreach(float dx in new[]{-4.3f,4.3f})Box("Paddy earth bund",p+new Vector3(dx,0,0),new Vector3(.5f,.22f,12.7f),path);
            GameObject crop=new GameObject("Rice patch "+patch);var combine=new List<CombineInstance>();
            var blade=new Mesh();blade.vertices=new[]{new Vector3(-.045f,0,0),new Vector3(.045f,0,0),new Vector3(.10f,.62f,0),new Vector3(0,0,-.045f),new Vector3(0,0,.045f),new Vector3(0,.64f,.10f)};blade.triangles=new[]{0,1,2,2,1,0,3,4,5,5,4,3};blade.RecalculateNormals();
            for(int x=0;x<14;x++)for(int z=0;z<20;z++)combine.Add(new CombineInstance{mesh=blade,transform=Matrix4x4.TRS(p+new Vector3(-3.5f+x*.53f,.05f,-5.4f+z*.55f),Quaternion.Euler(0,(x*19+z*13)%180,0),Vector3.one)});
            var mesh=new Mesh();mesh.CombineMeshes(combine.ToArray());crop.AddComponent<MeshFilter>().sharedMesh=mesh;crop.AddComponent<MeshRenderer>().sharedMaterial=grass;
            sites.Add(new Site{id="rice"+patch,title="Harvest rice (+3 grain)",point=p+new Vector3(0,0,5.7f),visual=crop});
        }
        for(int i=0;i<4;i++)
        {
            Vector3 p=new Vector3(49,0,-49-i*7);
            for(int n=0;n<3;n++)Box("Timber bundle",p+new Vector3(n*.25f,.03f,0),new Vector3(.2f,.22f,2),wood);
            sites.Add(new Site{id="wood"+i,title="Gather fallen timber (+2 wood)",point=p+Vector3.right*1.2f});
            Vector3 q=new Vector3(47,0,-91+i*4);
            for(int n=0;n<3;n++)Box("Quarry stone",q+new Vector3(n*.35f,0,0),new Vector3(.3f,.4f,.5f),CholaKit.stone);
            sites.Add(new Site{id="stone"+i,title="Collect dressed stone (+2 stone)",point=q+Vector3.left*1.5f});
        }
    }
    void BuildSites()
    {
        sites.Add(new Site{id="steward",title="Speak with the village steward",point=StewardPoint});
        sites.Add(new Site{id="canal",title="Repair irrigation (4 wood, 4 stone)",point=CanalPoint});
        sites.Add(new Site{id="granary",title="Stock granary (9 grain)",point=GranaryPoint});
        sites.Add(new Site{id="market",title="Trade 3 grain for 2 coins",point=MarketPoint});
        sites.Add(new Site{id="toolsmith",title="Buy 2 stone for 2 coins",point=new Vector3(-8,0,-55)});
        foreach(var site in sites){var marker=new GameObject("Village marker "+site.id);marker.transform.position=site.point+Vector3.up*2.5f;var label=marker.AddComponent<TextMesh>();label.text=site.id.StartsWith("rice")?"RICE FIELD":site.id.StartsWith("wood")?"TIMBER":site.id.StartsWith("stone")?"STONE":site.id.ToUpperInvariant();label.fontSize=36;label.characterSize=.075f;label.anchor=TextAnchor.MiddleCenter;label.color=new Color(.94f,.85f,.61f);marker.AddComponent<SettlementMarker>();}
    }
    void BuildPeople()
    {
        SpawnCitizen(StewardPoint+Vector3.left,StewardPoint+Vector3.left,0);
        for(int i=0;i<12;i++)
        {
            float x=(i%2==0?-1:1)*(5.8f+(i%3)*.3f);float z=-35-(i/2)*10;
            SpawnCitizen(new Vector3(x,0,z),new Vector3(x,0,Mathf.Max(-95,z-16)),i+1);
        }
    }
    void SpawnCitizen(Vector3 a,Vector3 b,int index)
    {
        var person=Instantiate(world.pilgrim.AvatarRoot.gameObject);person.name=index==0?"Village steward":"Resident "+index;person.transform.position=a;person.transform.localScale=Vector3.one*(.92f+(index%4)*.035f);
        foreach(var c in person.GetComponentsInChildren<Collider>())Destroy(c);
        Color[] colors={new Color(.52f,.43f,.28f),new Color(.20f,.29f,.27f),new Color(.51f,.24f,.16f),new Color(.65f,.61f,.45f)};
        foreach(var r in person.GetComponentsInChildren<Renderer>())if(r.name.Contains("dhoti")||r.name.Contains("Sash")){var m=new Material(r.sharedMaterial);m.color=colors[index%4];r.sharedMaterial=m;}
        var agent=person.AddComponent<SettlementResident>();agent.a=a;agent.b=b;agent.world=world;agent.phase=index*.7f;
    }
    void BuildLandscape()
    {
        for(int i=0;i<34;i++){float x=(i%2==0?-1:1)*(75+(i%5)*4);float z=-112+(i/2)*12;CholaKit.Palm(new Vector3(x,0,z),8+i%5,staticRoot);}
        for(int i=0;i<6;i++)CholaKit.Palm(new Vector3(i%2==0?-27:28,0,-95+i*12),9+i%3,staticRoot);
        Box("River beyond fields",new Vector3(-91,.015f,-27),new Vector3(15,.04f,230),CholaKit.water,false);
        // Walkable world limits and riverbank: no invisible fall out of the map.
        foreach(float x in new[]{-81f,101f})Box("Village boundary bank",new Vector3(x,0,-22),new Vector3(1.4f,1.2f,267),CholaKit.earth);
        foreach(float z in new[]{-120f,120f})Box("Village boundary bank",new Vector3(10,0,z),new Vector3(184,1.2f,1.4f),CholaKit.earth);
    }
    void Update()
    {
        if(world==null||!world.Playing)return;
        measuredFps=Mathf.Lerp(measuredFps,1/Mathf.Max(.001f,Time.unscaledDeltaTime),.02f);
        if(Input.GetKeyDown(KeyCode.Tab)&&!world.Building){Journal=!Journal;world.SetPause(Journal);}
        if(Journal&&!world.Paused)Journal=false;
        if(Input.GetKeyDown(KeyCode.G)){high=!high;QualitySettings.shadowDistance=high?110:50;QualitySettings.shadowCascades=high?4:2;QualitySettings.antiAliasing=high?4:0;Say(high?"High graphics":"Performance graphics");}
        if(world.Paused||world.Building)return;
        nearby=Nearest();if(Input.GetKeyDown(KeyCode.F))Interact();
        foreach(var s in sites)if(s.visual!=null&&!s.visual.activeSelf&&Time.time>=s.availableAt)s.visual.SetActive(true);
        if(state.irrigation&&state.granary&&world.progress.restored&&!state.complete){state.complete=true;SaveSettlement();Say("The settlement prospers. Irrigation, granary and sanctuary restored.",12);}
    }
    Site Nearest(){Site found=null;float best=3;foreach(var s in sites){float d=Vector3.Distance(world.pilgrim.transform.position,s.point);if(d<best){best=d;found=s;}}return found;}
    public bool Interact()
    {
        if(!world.Playing||world.Paused||world.Building)return false;
        var site=Nearest();if(site==null)return false;
        if(Time.time<site.availableAt){Say("This resource is recovering. Try another patch or return later.");return false;}
        if(site.id=="steward"){state.met=true;Say("Steward: Repair the sluice with 4 wood and 4 stone, then bring 9 grain to the granary. Our temple also needs restoration.",10);}
        else if(site.id.StartsWith("rice")){state.grain+=state.irrigation?5:3;site.availableAt=Time.time+75;if(site.visual!=null)site.visual.SetActive(false);Say("Rice harvested. The patch regrows in 75 seconds.");}
        else if(site.id.StartsWith("wood")){state.wood+=2;site.availableAt=Time.time+90;Say("Two timber bundles collected.");}
        else if(site.id.StartsWith("stone")){state.stone+=2;site.availableAt=Time.time+90;Say("Two dressed stone blocks collected.");}
        else if(site.id=="canal"){
            if(state.irrigation){Say("The sluice is already repaired.");return false;}
            if(!state.met||state.wood<4||state.stone<4){Say("Meet the steward, then bring 4 wood and 4 stone.");return false;}
            state.wood-=4;state.stone-=4;state.irrigation=true;repairedCanal.SetActive(true);Say("Irrigation restored. Each rice harvest now yields 5 grain.");
        } else if(site.id=="granary"){
            if(state.granary){Say("The granary is already stocked.");return false;}
            if(!state.irrigation||state.grain<9){Say("Repair irrigation first, then bring 9 grain.");return false;}
            state.grain-=9;state.granary=true;state.coins+=6;filledGranary.SetActive(true);Say("Granary stocked. The village awards 6 coins. Restore the temple sanctuary to complete the settlement.",9);
        } else if(site.id=="market") {if(state.grain<3){Say("The merchant needs 3 grain.");return false;}state.grain-=3;state.coins+=2;Say("Traded 3 grain for 2 coins.");}
        else if(site.id=="toolsmith") {if(state.coins<2){Say("Two coins are needed.");return false;}state.coins-=2;state.stone+=2;Say("Bought 2 stone for 2 coins.");}
        SaveSettlement();return true;
    }
    public void ResetSettlement(){state=new State();foreach(var s in sites){s.availableAt=0;if(s.visual!=null)s.visual.SetActive(true);}Refresh();SaveSettlement();}
    public void LoadSettlement(){try{if(File.Exists(savePath)){var loaded=JsonUtility.FromJson<State>(File.ReadAllText(savePath));if(loaded!=null&&loaded.version==1&&loaded.grain>=0&&loaded.wood>=0&&loaded.stone>=0&&loaded.coins>=0)state=loaded;}Refresh();}catch(Exception e){Say("Village save could not be read.");Debug.LogWarning(e.Message);}}
    public void SaveSettlement(){try{File.WriteAllText(savePath+".tmp",JsonUtility.ToJson(state,true));if(File.Exists(savePath))File.Replace(savePath+".tmp",savePath,null);else File.Move(savePath+".tmp",savePath);}catch(Exception e){Say("Village progress could not be saved.");Debug.LogWarning(e.Message);}}
    void Refresh(){if(repairedCanal!=null)repairedCanal.SetActive(state.irrigation);if(filledGranary!=null)filledGranary.SetActive(state.granary);}
    void Say(string s,float duration=5){notice=s;noticeUntil=Time.unscaledTime+duration;}
    public string Objective(){return state.complete?"Settlement restored":!state.met?"Meet the steward beside the southern road":!state.irrigation?"Repair irrigation: 4 wood + 4 stone":!state.granary?"Deliver 9 grain to the village granary":"Restore the temple monument and open its sanctuary";}
    void OnGUI()
    {
        if(world==null||!world.Playing)return;
        if(title==null){title=new GUIStyle(GUI.skin.label){fontSize=24,fontStyle=FontStyle.Bold};text=new GUIStyle(GUI.skin.label){fontSize=17,wordWrap=true};small=new GUIStyle(text){fontSize=14};button=new GUIStyle(GUI.skin.button){fontSize=17};panel=new Texture2D(1,1);panel.SetPixel(0,0,new Color(.07f,.10f,.08f,.93f));panel.Apply();}
        float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)/2,(Screen.height-720*scale)/2),Quaternion.identity,Vector3.one*scale);
        if(world.Paused&&!Journal)return;
        GUI.DrawTexture(new Rect(28,137,425,72),panel);GUI.Label(new Rect(43,145,400,25),"Grain "+state.grain+"   Timber "+state.wood+"   Stone "+state.stone+"   Coins "+state.coins,text);GUI.Label(new Rect(43,175,400,25),"F  Village action    Tab  Journal    G  Graphics",small);
        if(world.pilgrim.transform.position.z<-18){GUI.DrawTexture(new Rect(28,25,530,96),panel);GUI.Label(new Rect(43,34,500,30),"YUG  /  KAVERI SETTLEMENT",title);GUI.Label(new Rect(43,72,490,44),Objective(),text);}
        GUI.Label(new Rect(1030,76,220,30),(high?"High":"Performance")+" · "+Mathf.RoundToInt(measuredFps)+" fps",small);
        if(nearby!=null&&!world.Paused){GUI.DrawTexture(new Rect(300,565,680,55),panel);GUI.Label(new Rect(321,579,640,36),"[F]  "+(nearby.id.StartsWith("rice")&&state.irrigation?"Harvest rice (+5 grain)":nearby.title),text);}
        if(Time.unscaledTime<noticeUntil){GUI.DrawTexture(new Rect(230,628,820,77),panel);GUI.Label(new Rect(248,640,786,65),notice,text);}
        if(Journal){GUI.DrawTexture(new Rect(245,100,790,510),panel);GUI.Label(new Rect(275,125,710,40),"The village record",title);GUI.Label(new Rect(275,183,710,325),
          "1. Steward — beside the southern road\n2. Timber grove — east of the market\n3. Stone yard — southeast corner\n4. Irrigation sluice — western fields\n5. Rice plots — west, beyond the channel\n6. Granary — northeast of the market\n7. Temple — follow the central road north\n\n"+Objective()+"\n\nWASD move · Shift run · Space jump\nRight mouse / arrows camera · F village actions · E temple actions\nA fictional Chola-era settlement, not a surveyed reconstruction.",text);if(GUI.Button(new Rect(275,548,710,40),"Return to settlement  [Tab]",button)){Journal=false;world.SetPause(false);}}
    }
}

public class SettlementResident : MonoBehaviour
{
    public Vector3 a,b;public TempleWorld world;public float phase;
    Transform left,right,armL,armR;float travel;bool reverse;
    void Start(){foreach(var t in GetComponentsInChildren<Transform>()){if(t.name=="Left hip")left=t;if(t.name=="Right hip")right=t;if(t.name=="Left shoulder")armL=t;if(t.name=="Right shoulder")armR=t;}}
    void Update(){if(world==null||world.Paused)return;Vector3 target=reverse?a:b;bool walk=Vector3.Distance(a,b)>.1f&&Vector3.Distance(transform.position,world.pilgrim.transform.position)>2;
        if(walk){transform.position=Vector3.MoveTowards(transform.position,target,Time.deltaTime*.7f);Vector3 d=target-transform.position;if(d.sqrMagnitude>.02f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(d),Time.deltaTime*3);if(d.magnitude<.1f)reverse=!reverse;travel+=Time.deltaTime*4;}
        float angle=walk?Mathf.Sin(travel+phase)*22:0;if(left)left.localRotation=Quaternion.Euler(angle,0,0);if(right)right.localRotation=Quaternion.Euler(-angle,0,0);if(armL)armL.localRotation=Quaternion.Euler(-angle*.6f,0,0);if(armR)armR.localRotation=Quaternion.Euler(angle*.6f,0,0);
    }
}
public class SettlementMarker : MonoBehaviour
{
    void LateUpdate(){var camera=Camera.main;if(camera==null)return;transform.rotation=camera.transform.rotation;var r=GetComponent<Renderer>();r.enabled=Vector3.Distance(camera.transform.position,transform.position)<27;}
}
