using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettlementVerification : MonoBehaviour
{
    [Serializable] class Report { public bool passed;public List<string> checks=new List<string>();public List<string> errors=new List<string>();public float meanFps; }
    Report report=new Report();SettlementWorld village;TempleWorld world;string folder;int frames;float elapsed;
    void Awake(){Application.logMessageReceived+=Log;}
    void OnDestroy(){Application.logMessageReceived-=Log;}
    void Log(string m,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(m);}
    void Update(){frames++;elapsed+=Time.unscaledDeltaTime;}
    void Check(string name,bool ok){report.checks.Add((ok?"PASS ":"FAIL ")+name);Debug.Log(report.checks[report.checks.Count-1]);}
    IEnumerator Start()
    {
        village=GetComponent<SettlementWorld>();world=GetComponent<TempleWorld>();folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../work"));Directory.CreateDirectory(folder);
        yield return new WaitForSeconds(2);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"settlement-title.png"));yield return new WaitForSeconds(.3f);
        world.BeginNew();yield return new WaitForSeconds(.5f);
        Check("new settlement has separate test save",village.savePath.EndsWith("settlement-test.json")&&village.state.grain==0);
        Check("out of range interaction rejected",!village.Interact());
        yield return Visit(village.CanalPoint);Check("repair cannot spend missing resources",!village.Interact()&&!village.state.irrigation&&village.state.wood==0);
        yield return Visit(village.StewardPoint);Check("steward starts mission",village.Interact()&&village.state.met);
        foreach(string id in new[]{"wood0","wood1","stone0","stone1"}){yield return Visit(village.sites.Find(s=>s.id==id).point);Check("gather "+id,village.Interact());Check("resource cooldown "+id,!village.Interact());}
        yield return Visit(village.CanalPoint);Check("sluice consumes materials and repairs",village.Interact()&&village.state.irrigation&&village.state.wood==0&&village.state.stone==0);Check("repair cannot be purchased twice",!village.Interact());
        foreach(string id in new[]{"rice0","rice1","rice2"}){yield return Visit(village.sites.Find(s=>s.id==id).point);Check("irrigated harvest "+id,village.Interact());}
        Check("irrigation improves yield",village.state.grain==15);
        yield return Visit(village.MarketPoint);Check("market trades without negative balances",village.Interact()&&village.state.grain==12&&village.state.coins==5);
        yield return Visit(village.GranaryPoint);Check("granary consumes grain and rewards coins",village.Interact()&&village.state.granary&&village.state.grain==3&&village.state.coins==11);
        village.SaveSettlement();village.state=new SettlementWorld.State();village.LoadSettlement();Check("settlement save reload",village.state.irrigation&&village.state.granary&&village.state.coins==11);
        // Walk the actual controller along the main street and through the temple gateway.
        world.pilgrim.Teleport(new Vector3(0,.1f,-80));world.pilgrim.useMoveOverride=true;
        float deadline=Time.time+35;while(world.pilgrim.transform.position.z<-3&&Time.time<deadline){world.pilgrim.MoveInputOverride=Vector3.forward;yield return null;}world.pilgrim.MoveInputOverride=Vector3.zero;world.pilgrim.useMoveOverride=false;
        Check("street and temple gateway traversable "+world.pilgrim.transform.position,world.pilgrim.transform.position.z>-4);
        // Cinematic overview for visual inspection of authored world and material rendering.
        world.pilgrim.SetCinematic(new Vector3(53,30,-102),new Vector3(0,7,-32));yield return new WaitForSeconds(2);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"settlement-overview.png"));yield return new WaitForSeconds(.5f);
        world.pilgrim.ClearCinematic();yield return Visit(new Vector3(0,.1f,-65));world.pilgrim.CameraYaw=0;world.pilgrim.CameraPitch=16;yield return new WaitForSeconds(2);ScreenCapture.CaptureScreenshot(Path.Combine(folder,"settlement-street.png"));yield return new WaitForSeconds(.5f);
        world.SetPause(true);Vector3 before=world.pilgrim.transform.position;yield return new WaitForSecondsRealtime(.4f);Check("pause freezes movement",Vector3.Distance(before,world.pilgrim.transform.position)<.02f);world.SetPause(false);
        report.meanFps=frames/Mathf.Max(.01f,elapsed);report.passed=report.errors.Count==0&&!report.checks.Exists(s=>s.StartsWith("FAIL"));File.WriteAllText(Path.Combine(folder,"settlement-verification.json"),JsonUtility.ToJson(report,true));Application.Quit(report.passed?0:1);
    }
    IEnumerator Visit(Vector3 p){world.pilgrim.Teleport(p+Vector3.up*.1f);yield return new WaitForSeconds(.12f);}
}

