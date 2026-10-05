using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>Opt-in native integration run. Uses the real motor and proximity interactions.</summary>
public class TempleVerification : MonoBehaviour
{
    [Serializable] class Check { public string name; public bool passed; public string detail; }
    [Serializable] class Report
    {
        public string timestamp;
        public string platform;
        public bool passed;
        public int frames;
        public float averageFps;
        public List<Check> checks = new List<Check>();
        public List<string> runtimeErrors = new List<string>();
    }
    TempleWorld world;
    Report report = new Report();
    string output;
    int frameCount;
    float elapsed;

    void Awake() { Application.logMessageReceived += OnLog; }
    void OnDestroy() { Application.logMessageReceived -= OnLog; }
    void OnLog(string message, string stack, LogType type)
    { if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert) report.runtimeErrors.Add(message + "\n" + stack); }
    void Update() { frameCount++; elapsed += Time.unscaledDeltaTime; }
    void CheckResult(string name, bool passed, string detail = "")
    { report.checks.Add(new Check { name = name, passed = passed, detail = detail }); Debug.Log("VERIFY " + (passed ? "PASS " : "FAIL ") + name + " " + detail); }

    IEnumerator Start()
    {
        world=GetComponent<TempleWorld>();
        output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../work"));
        Directory.CreateDirectory(Path.Combine(output,"Captures"));
        yield return new WaitForSeconds(2);
        yield return Capture("construction-01-title");
        world.BeginNew();var player=world.pilgrim;var village=GetComponent<SettlementWorld>();
        foreach(string id in new[]{"steward","wood0","wood1","stone0","stone1","canal","rice0","rice1","granary"}){
            player.Teleport(village.sites.Find(s=>s.id==id).point+Vector3.up*.1f);yield return new WaitForSeconds(.15f);
            CheckResult("Settlement prerequisite "+id,village.Interact());
        }
        CheckResult("Village awaits temple restoration",village.state.irrigation&&village.state.granary&&!village.state.complete);
        player.Teleport(new Vector3(0,.1f,-15));player.useMoveOverride=true;
        yield return new WaitForSeconds(.5f);
        CheckResult("Fresh construction journey",world.progress.sequence==0&&!world.progress.restored);
        yield return Walk(new Vector3(0,0,-4));
        yield return Capture("construction-02-approach");
        yield return Walk(new Vector3(-4,0,2));
        CheckResult("Read construction relief",world.InteractNearest()&&world.progress.clue);
        world.CloseClue();
        yield return Walk(new Vector3(-14,0,5));
        CheckResult("Recover pillar shafts by tank",world.InteractNearest()&&world.progress.fragments==1);
        yield return Capture("construction-03-tank");
        yield return Walk(new Vector3(-11,0,16));
        yield return Walk(new Vector3(15,0,16));
        yield return Walk(new Vector3(18,0,20));
        CheckResult("Recover carved lintel",world.InteractNearest()&&world.progress.fragments==3);
        yield return Walk(new Vector3(15,0,17));
        yield return Walk(new Vector3(0,0,17));
        yield return Walk(new Vector3(0,.9f,25));
        CheckResult("Mandapa stairs traversable",player.IsGrounded&&player.transform.position.y>.85f);
        yield return Walk(new Vector3(-7,.9f,29));
        CheckResult("Recover bronze crown",world.InteractNearest()&&world.progress.fragments==7);
        yield return Capture("construction-04-ruined-monument");
        yield return Walk(new Vector3(0,.9f,32));
        CheckResult("Unsupported lintel rejected",!world.InteractNearest()&&world.progress.sequence==0);
        yield return Walk(new Vector3(-4,.9f,32));world.InteractNearest();
        CheckResult("Placement animates before committing",world.Building&&world.progress.sequence==0);
        yield return new WaitForSeconds(2.6f);
        CheckResult("Pillars visibly seated",world.progress.sequence==1&&!world.Building);
        yield return Capture("construction-05-pillars");
        yield return Walk(new Vector3(4,.9f,32));
        CheckResult("Crown cannot float above missing lintel",!world.InteractNearest()&&world.progress.sequence==1);
        yield return Walk(new Vector3(0,.9f,32));world.InteractNearest();yield return new WaitForSeconds(2.6f);
        CheckResult("Lintel bridges pillars",world.progress.sequence==2);
        yield return Capture("construction-06-lintel");
        yield return Walk(new Vector3(4,.9f,32));world.InteractNearest();yield return new WaitForSeconds(2.6f);
        CheckResult("Crown completes monument",world.progress.sequence==3);
        yield return Capture("construction-07-complete");
        RaycastHit hit;
        CheckResult("Sanctuary doors physically closed",Physics.Raycast(new Vector3(1,3,43),Vector3.forward,out hit,3)&&hit.collider.name.Contains("door"));
        yield return Walk(new Vector3(5.5f,.9f,32));yield return Walk(new Vector3(5.5f,.9f,39));yield return Walk(new Vector3(0,.9f,38));
        CheckResult("Rebuilt monument releases mechanism",world.InteractNearest());
        yield return new WaitForSeconds(3);yield return Capture("construction-08-opening");
        yield return new WaitForSeconds(3);
        CheckResult("Door opens and control returns",world.progress.restored&&world.construction.DoorOpen&&player.inputEnabled);
        CheckResult("Village and temple complete the settlement",village.state.complete);
        yield return Walk(new Vector3(2.8f,.9f,41.4f));yield return Walk(new Vector3(0,.9f,41.4f));
        yield return Walk(new Vector3(0,1.76f,49));
        CheckResult("Player can enter opened sanctuary",player.transform.position.z>48&&player.IsGrounded);
        yield return Capture("construction-09-sanctuary");
        CheckResult("Save restores assembled monument, open doors and completed settlement",world.ContinueJourney()&&world.progress.sequence==3&&world.construction.DoorOpen&&village.state.complete);
        world.BeginNew();CheckResult("New journey resets structure and closes doors",world.progress.sequence==0&&!world.construction.DoorOpen);
        CheckResult("No runtime errors",report.runtimeErrors.Count==0,report.runtimeErrors.Count.ToString());
        report.timestamp=DateTime.UtcNow.ToString("O");report.platform=Application.platform.ToString();
        report.frames=frameCount;report.averageFps=frameCount/Mathf.Max(1,elapsed);report.passed=report.checks.TrueForAll(c=>c.passed);
        File.WriteAllText(Path.Combine(output,"construction-verification.json"),JsonUtility.ToJson(report,true));
        Debug.Log("VERIFY COMPLETE "+report.passed);Application.Quit(report.passed?0:1);
    }
    IEnumerator Walk(Vector3 target)
    {
        float deadline = Time.realtimeSinceStartup + 25;
        Vector3 delta;
        do
        {
            delta = target - world.pilgrim.transform.position; delta.y = 0;
            if (delta.magnitude < .24f) break;
            world.pilgrim.MoveInputOverride = delta.normalized * Mathf.Clamp01(delta.magnitude * 2);
            yield return null;
        } while (Time.realtimeSinceStartup < deadline);
        world.pilgrim.MoveInputOverride = Vector3.zero;
        yield return new WaitForSeconds(.15f);
        if (delta.magnitude >= .35f) CheckResult("Route reaches " + target, false, "Stopped at " + world.pilgrim.transform.position);
    }

    IEnumerator Capture(string name)
    {
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(output, "Captures", name + ".png"));
        yield return null;
    }
}

