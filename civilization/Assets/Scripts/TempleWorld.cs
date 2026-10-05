using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Scene composition and the short restoration journey, using the existing art kit.</summary>
public class TempleWorld : MonoBehaviour
{
    [Serializable] public class Journey
    {
        public int version = 2;
        public bool clue;
        public int fragments;
        public int sequence;
        public bool restored;
    }

    public Journey progress = new Journey();
    public PilgrimController pilgrim;
    public bool Playing { get; private set; }
    public bool Paused { get; private set; }
    public bool Verifying { get; private set; }
    public MonumentConstruction construction;
    public bool Building { get; private set; }
    public string SavePath { get; private set; }
    public readonly Vector3[] fragmentPoints = { new Vector3(-18, 0, 7), new Vector3(18, 0, 22), new Vector3(-7, .9f, 31) };
    public readonly Vector3[] sealPoints = { new Vector3(-4, .9f, 34), new Vector3(0, .9f, 34), new Vector3(4, .9f, 34) };
    public readonly Vector3 cluePoint = new Vector3(-4, 0, 4);
    public readonly Vector3 altarPoint = new Vector3(0, .9f, 40);
    public static readonly Vector3 StartPoint = new Vector3(0, .1f, -83);
    readonly string[] fragmentNames = { "Paired pillar shafts", "Lotus-carved lintel", "Bronze ceremonial crown" };
    readonly string[] sealNames = { "PILLARS", "LINTEL", "CROWN" };
    readonly int[] order = { 0, 1, 2 };
    readonly GameObject[] fragments = new GameObject[3];
    readonly Renderer[] seals = new Renderer[3];
    readonly List<TextMesh> worldLabels = new List<TextMesh>();
    Light sun;
    Material activeSeal, idleSeal;
    AudioSource sound;
    AudioClip chime;
    GameObject restoredCrown;
    string message = "", nearest = "";
    float messageUntil;
    bool showClue, ending, confirmReset;
    GUIStyle heading, body, small, button, eyebrow;
    Texture2D panel;
    int nearby = -1;
    const string Inscription = "Two upright shafts carry the weight.\nThe lotus beam bridges their shoulders.\nThe bronze crown rests above the lotus.\n\nRecover the pieces from the tank, eastern shrine\nand mandapa. Rebuild the ceremonial monument\nfrom its foundations upward.";

    void Awake()
    {
        Verifying = (Array.IndexOf(Environment.GetCommandLineArgs(), "-verifyTemple") >= 0 || Array.IndexOf(Environment.GetCommandLineArgs(), "-verifySettlement") >= 0);
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        // Editor saves also live in the project's Build folder; verification never touches player progress.
        if (Application.isEditor) root = Path.Combine(root, "Build");
        SavePath = Path.Combine(root, Verifying ? "construction-test-save.json" : "construction-journey.json");
        CholaKit.Init();
        BuildLightAndCamera();
        BuildTemple();
        gameObject.AddComponent<TempleAtmosphere>().Build();
        BuildJourneyObjects();
        construction=gameObject.AddComponent<MonumentConstruction>();construction.Initialize(fragmentPoints,sealPoints);
        GameObject player = new GameObject("Pilgrim");
        player.transform.position = StartPoint;
        pilgrim = player.AddComponent<PilgrimController>();
        pilgrim.viewCamera = Camera.main;
        pilgrim.inputEnabled = false;
        pilgrim.CameraPitch = 23;
        pilgrim.SetCinematic(new Vector3(48, 25, -82), new Vector3(0, 7, -26));
        sound = gameObject.AddComponent<AudioSource>();
        chime = AudioClip.Create("Original bronze resonance", 22050, 1, 22050, false);
        float[] samples = new float[22050];
        for (int i = 0; i < samples.Length; i++)
        {
            float t = i / 22050f;
            samples[i] = .19f * Mathf.Exp(-5 * t) * (Mathf.Sin(t * 2 * Mathf.PI * 440) + .3f * Mathf.Sin(t * 2 * Mathf.PI * 1174));
        }
        chime.SetData(samples, 0);
        gameObject.AddComponent<SettlementWorld>();
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "-verifyTemple") >= 0) gameObject.AddComponent<TempleVerification>();
    }

    void BuildLightAndCamera()
    {
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(.32f, .43f, .45f);
        RenderSettings.ambientEquatorColor = new Color(.27f, .31f, .28f);
        RenderSettings.ambientGroundColor = new Color(.12f, .14f, .12f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(.42f, .53f, .49f);
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = .0045f;
        var lightObject = new GameObject("Late afternoon sun");
        sun = lightObject.AddComponent<Light>(); sun.type = LightType.Directional;
        sun.color = new Color(1, .90f, .72f); sun.intensity = 1.25f;
        sun.shadows = LightShadows.Soft; sun.shadowBias = .035f;
        lightObject.transform.rotation = Quaternion.Euler(32, -42, 0);
        var skyShader = Shader.Find("Skybox/Procedural");
        if (skyShader != null)
        {
            var sky = new Material(skyShader);
            sky.SetColor("_SkyTint", new Color(.58f, .65f, .64f));
            sky.SetColor("_GroundColor", new Color(.48f, .40f, .27f));
            sky.SetFloat("_AtmosphereThickness", 1.4f);sky.SetFloat("_Exposure",.7f); RenderSettings.skybox = sky;
        }
        RenderSettings.sun = sun;
        GameObject cameraObject = new GameObject("Pilgrim camera"); cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>(); camera.fieldOfView = 55;
        camera.nearClipPlane = .08f; camera.farClipPlane = 360;
        cameraObject.AddComponent<AudioListener>(); cameraObject.AddComponent<TempleLens>();
        Application.targetFrameRate = 60;
    }

    GameObject Box(string name, Vector3 bottom, Vector3 size, Material material, bool collide = true)
    { return CholaKit.Box(name, bottom, size, material, transform, collide); }

    void BuildTemple()
    {
        Box("Earth", new Vector3(0, -1, 25), new Vector3(220, 1, 310), CholaKit.earth);
        Box("Axial processional stone path", new Vector3(0, 0, 0), new Vector3(7, .035f, 38), CholaKit.stone);
        // Walled courtyard with an open southern gateway and generous circulation around each landmark.
        Box("West enclosure", new Vector3(-32, 0, 28), new Vector3(1.4f, 3.5f, 92), CholaKit.plaster);
        Box("East enclosure", new Vector3(32, 0, 28), new Vector3(1.4f, 3.5f, 92), CholaKit.plaster);
        Box("Rear enclosure", new Vector3(0, 0, 74), new Vector3(65, 3.5f, 1.4f), CholaKit.plaster);
        for (int side = -1; side <= 1; side += 2)
        {
            Box("Gateway wing", new Vector3(side * 19, 0, -18), new Vector3(26, 3.5f, 1.4f), CholaKit.plaster);
            CholaKit.Pillar(new Vector3(side * 5, 0, -18), 7, transform);
            Box("Gate cap", new Vector3(side * 5, 7, -18), new Vector3(3, .5f, 3), CholaKit.trim);
        }
        Box("Entrance lintel", new Vector3(0, 7.5f, -18), new Vector3(13, .6f, 3), CholaKit.stone);
        // Broad 18 cm rises are within the existing controller's step height.
        for (int i = 0; i < 5; i++)
            Box("Mandapa approach step " + (i + 1), new Vector3(0, 0, 18.5f + i * .7f), new Vector3(12, (i + 1) * .18f, .72f), CholaKit.trim);
        Box("Mandapa platform", new Vector3(0, 0, 32), new Vector3(21, .9f, 21), CholaKit.stone);
        for (int side = -1; side <= 1; side += 2)
        {
            for (int row = 0; row < 4; row++) CholaKit.Pillar(new Vector3(side * 8.6f, .9f, 24 + row * 5), 5.5f, transform);
            Box("Mandapa architrave", new Vector3(side * 8.6f, 6.4f, 31.5f), new Vector3(2.5f, .55f, 20), CholaKit.trim);
        }
        // Open central clerestory preserves the view of the dominant vimana from the approach.
        Box("West roof", new Vector3(-7, 6.95f, 32), new Vector3(7, .45f, 21), CholaKit.stone);
        Box("East roof", new Vector3(7, 6.95f, 32), new Vector3(7, .45f, 21), CholaKit.stone);
        Box("Mandapa front lintel", new Vector3(0, 6.4f, 23.5f), new Vector3(20, .55f, 2), CholaKit.trim);
        GameObject tower = CholaKit.Vimana(new Vector3(0, 0, 54), 20, 32, transform);
        tower.transform.rotation = Quaternion.Euler(0, 180, 0);
        CholaKit.Nandi(new Vector3(0, 0, 12), .75f, transform);
        CholaKit.Shrine(new Vector3(22, 0, 26), .8f, transform);
        CholaKit.Shrine(new Vector3(-22, 0, 45), .9f, transform);
        Box("Temple tank water", new Vector3(-22, .06f, 6), new Vector3(11, .05f, 14), CholaKit.water, false);
        // A raised tank lip prevents walking onto the decorative water surface.
        foreach (float x in new[] { -28f, -16f }) Box("Tank stone edge", new Vector3(x, 0, 6), new Vector3(.8f, .65f, 16), CholaKit.trim);
        foreach (float z in new[] { -2f, 14f }) Box("Tank stone edge", new Vector3(-22, 0, z), new Vector3(12.8f, .65f, .8f), CholaKit.trim);
        // Place the river memory beside the accessible eastern bank.
        fragmentPoints[0] = new Vector3(-14, 0, 7);
        for (int side = -1; side <= 1; side += 2)
            for (int i = 0; i < 6; i++) CholaKit.Palm(new Vector3(side * (27 - i % 2 * 2), 0, -10 + i * 14), 9 + i % 3, transform);
        for (int side = -1; side <= 1; side += 2)
            foreach (float z in new[] { 22f, 32f, 40f }) CholaKit.Lamp(new Vector3(side * 5.8f, .9f, z), transform);
        restoredCrown = new GameObject("Restored sanctuary lamps"); restoredCrown.transform.SetParent(transform);
        for (int i = -2; i <= 2; i++) CholaKit.Lamp(new Vector3(i * 2.2f, .9f, 41), restoredCrown.transform);
        restoredCrown.SetActive(false);
    }

    void BuildJourneyObjects()
    {
        idleSeal = new Material(CholaKit.bronze);
        activeSeal = new Material(CholaKit.bronze); activeSeal.color = new Color(1, .69f, .22f);
        activeSeal.EnableKeyword("_EMISSION"); activeSeal.SetColor("_EmissionColor", new Color(1, .40f, .05f) * 1.8f);
        Box("Inscribed stone", cluePoint, new Vector3(1.5f, 1.65f, .4f), CholaKit.darkStone);
        Label("STONE MEMORY", cluePoint + new Vector3(0, 2, -.1f), .14f);
        for (int i = 0; i < 3; i++)
        {
            Box("Memory pedestal", fragmentPoints[i], new Vector3(.9f, .7f, .9f), CholaKit.darkStone);
            fragments[i] = new GameObject("Recovery state / " + fragmentNames[i]); fragments[i].transform.SetParent(transform);
            Label(new[] { "PILLAR SHAFTS", "CARVED LINTEL", "BRONZE CROWN" }[i], fragmentPoints[i] + Vector3.up * 2.1f, .14f);
            Box(sealNames[i] + " pedestal", sealPoints[i], new Vector3(1.4f, 1, 1.4f), CholaKit.darkStone);
            seals[i] = Box(sealNames[i] + " seal", sealPoints[i] + Vector3.up, new Vector3(1.1f, .14f, 1.1f), idleSeal, false).GetComponent<Renderer>();
            Label(sealNames[i], sealPoints[i] + new Vector3(0, 1.7f, -.2f), .18f);
        }
        Box("Door mechanism", altarPoint, new Vector3(1.15f, .8f, .8f), CholaKit.bronze);
        Label("SANCTUARY MECHANISM", altarPoint + Vector3.up * 1.6f, .15f);
    }

    void Label(string text, Vector3 position, float size)
    {
        var go = new GameObject(text); go.transform.SetParent(transform); go.transform.position = position;
        var label = go.AddComponent<TextMesh>(); label.text = text; label.fontSize = 64;
        label.characterSize = size * .28f; label.anchor = TextAnchor.MiddleCenter; label.color = new Color(1, .86f, .55f);
        worldLabels.Add(label);
    }

    void Update()
    {
        foreach (TextMesh label in worldLabels)
        {
            label.GetComponent<Renderer>().enabled = Playing && !ending && Vector3.Distance(pilgrim.transform.position, label.transform.position) < 10;
            label.transform.rotation = pilgrim.viewCamera.transform.rotation;
        }
        if (!Playing) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (showClue) { showClue = false; SetPause(false); }
            else if (!ending) SetPause(!Paused);
        }
        if (Input.GetKeyDown(KeyCode.J) && !ending) { showClue = !showClue; SetPause(showClue); }
        if (Paused || ending || Building) return;
        FindNearby();
        if (Input.GetKeyDown(KeyCode.E)) InteractNearest();
    }

    void FindNearby()
    {
        nearby = -1; nearest = ""; float best = 2.55f;
        Consider(0, cluePoint, "Read the stone inscription", ref best);
        for (int i = 0; i < 3; i++)
        {
            if ((progress.fragments & (1 << i)) == 0) Consider(i + 1, fragmentPoints[i], "Recover " + fragmentNames[i], ref best);
            if (i >= progress.sequence && !progress.restored)
                Consider(i + 4, sealPoints[i], "Place " + sealNames[i] + " on the monument", ref best);
        }
        Consider(7, altarPoint, progress.restored ? "View the restored monument" : "Open the sanctuary mechanism", ref best);
    }

    void Consider(int id, Vector3 point, string prompt, ref float best)
    {
        float distance = Vector3.Distance(pilgrim.transform.position + Vector3.up * .6f, point + Vector3.up * .6f);
        if (distance < best) { best = distance; nearby = id; nearest = prompt; }
    }

    // Both keyboard interaction and the native integration test use this proximity-checked path.
    public bool InteractNearest()
    {
        if (!Playing || Paused || ending || Building) return false;
        FindNearby();
        if (nearby < 0) return false;
        if (nearby == 0)
        {
            progress.clue = true; showClue = true; Save(); SetPause(true); return true;
        }
        if (nearby <= 3)
        {
            int i = nearby - 1;
            progress.fragments |= 1 << i;
            Say(fragmentNames[i] + " recovered. The piece is now at the monument worksite.");
            RefreshProgress(); Save(); Ring(1 + i * .15f); return true;
        }
        if (nearby <= 6)
        {
            int i = nearby - 4;
            if (!progress.clue) { Say("The relief beside the entrance shows how this monument was built."); return false; }
            if ((progress.fragments & (1 << i)) == 0) { Say("Recover this architectural piece before placing it."); return false; }
            if (i == order[progress.sequence])
            { StartCoroutine(PlaceComponent(i)); return true; }
            Say(i==1?"The lintel needs both upright pillars beneath it.":"The crown belongs above the carved lotus lintel.");Ring(.65f);return false;
        }
        if (progress.sequence != 3) { Say("Rebuild the monument before the sanctuary can open."); return false; }
        StartCoroutine(FinalView()); return true;
    }

    IEnumerator PlaceComponent(int index)
    {
        Building=true;pilgrim.inputEnabled=false;nearest="";
        Say("Placing "+fragmentNames[index]+"…");
        yield return construction.Place(index);
        progress.sequence++;Building=false;pilgrim.inputEnabled=!Paused;
        RefreshProgress();Save();Ring(1+progress.sequence*.18f);
        Say(progress.sequence==3?"The monument stands again. Release the bronze sanctuary mechanism.":fragmentNames[index]+" seated. The structure can carry the next piece.");
    }

    public void BeginNew()
    {
        StopAllCoroutines(); progress = new Journey(); Playing = true; ending = false; Building=false; showClue = false; confirmReset = false;
        construction.CancelMotion();
        pilgrim.ClearCinematic(); pilgrim.Teleport(StartPoint); pilgrim.checkpoint = StartPoint;
        pilgrim.CameraYaw = 0; pilgrim.CameraPitch = 23;
        GetComponent<SettlementWorld>()?.ResetSettlement();
        SetPause(false); RefreshProgress(); Save(); Say("Meet the steward beside the road. Press F to speak; Tab opens your village record.");
    }

    public bool ContinueJourney()
    {
        try
        {
            Journey loaded = JsonUtility.FromJson<Journey>(File.ReadAllText(SavePath));
            if (loaded == null || loaded.version != 2 || loaded.fragments < 0 || loaded.fragments > 7 || loaded.sequence < 0 || loaded.sequence > 3
                || (loaded.sequence > 0 && (!loaded.clue || (loaded.fragments & ((1 << loaded.sequence)-1)) != ((1 << loaded.sequence)-1))) || (loaded.restored && loaded.sequence != 3))
                throw new InvalidDataException("Invalid journey data");
            GetComponent<SettlementWorld>()?.LoadSettlement();
            progress = loaded; Playing = true; ending = false; showClue = false;
            pilgrim.ClearCinematic(); pilgrim.Teleport(StartPoint); SetPause(false); RefreshProgress();
            Say("Your recovered pieces and reconstructed monument are preserved."); return true;
        }
        catch (Exception ex) when (ex is IOException || ex is ArgumentException || ex is UnauthorizedAccessException)
        { Say("This save could not be read. Start a new journey to replace it."); return false; }
    }

    public void CloseClue() { showClue = false; SetPause(false); }
    public void SetPause(bool pause) { Paused = pause; Time.timeScale = pause ? 0 : 1; pilgrim.inputEnabled = Playing && !pause && !ending && !Building; }
    public void ReturnToEntrance() { pilgrim.Teleport(StartPoint); Say("Returned to the entrance. Your recovered pieces are safe."); }
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
            string temporary = SavePath + ".tmp";
            File.WriteAllText(temporary, JsonUtility.ToJson(progress, true));
            if (File.Exists(SavePath)) File.Replace(temporary, SavePath, null);
            else File.Move(temporary, SavePath);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        { Say("Progress could not be saved. Check that the Build folder is writable."); Debug.LogWarning(ex.Message); }
    }

    void RefreshProgress()
    {
        for (int i = 0; i < 3; i++)
        {
            fragments[i].SetActive((progress.fragments & (1 << i)) == 0);
            bool awake = false; for (int j = 0; j < progress.sequence; j++) if (order[j] == i) awake = true;
            seals[i].sharedMaterial = awake ? activeSeal : idleSeal;
        }
        restoredCrown.SetActive(progress.restored);
        sun.intensity = progress.restored ? 1.4f : 1.25f;
        if(construction!=null)construction.SetState(progress.fragments,progress.sequence,progress.restored);
    }

    IEnumerator FinalView()
    {
        ending = true; pilgrim.inputEnabled = false; Ring(1.6f);
        pilgrim.SetCinematic(new Vector3(8, 5.4f, 34), new Vector3(0, 3.7f, 46));
        if(!progress.restored)yield return construction.OpenDoor();
        progress.restored=true;RefreshProgress();Save();
        yield return new WaitForSeconds(1.4f);
        Say("The sanctuary is open. Walk through the parted stone doors.", 10);
        ending = false; pilgrim.ClearCinematic(); pilgrim.inputEnabled = true;
    }

    void Ring(float pitch) { sound.pitch = pitch; sound.PlayOneShot(chime); }
    void Say(string text, float seconds = 6) { message = text; messageUntil = Time.unscaledTime + seconds; }
    public int MemoryCount { get { return ((progress.fragments & 1) != 0 ? 1 : 0) + ((progress.fragments & 2) != 0 ? 1 : 0) + ((progress.fragments & 4) != 0 ? 1 : 0); } }

    void InitGui()
    {
        if (heading != null) return;
        panel = new Texture2D(1, 1); panel.SetPixel(0, 0, new Color(.045f, .065f, .06f, .92f)); panel.Apply();
        heading = new GUIStyle(GUI.skin.label) { fontSize = 39, wordWrap = true, fontStyle = FontStyle.Bold }; heading.normal.textColor = new Color(1, .90f, .68f);
        body = new GUIStyle(GUI.skin.label) { fontSize = 20, wordWrap = true }; body.normal.textColor = new Color(.92f, .92f, .83f);
        small = new GUIStyle(body) { fontSize = 16 };
        eyebrow = new GUIStyle(body) { fontSize = 14, fontStyle = FontStyle.Bold }; eyebrow.normal.textColor = new Color(.83f, .65f, .35f);
        button = new GUIStyle(GUI.skin.button) { fontSize = 19, padding = new RectOffset(16, 16, 10, 10) };
    }

    void OnGUI()
    {
        if(GetComponent<SettlementWorld>()?.Journal==true)return;
        InitGui();
        float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) / 2, (Screen.height - 720 * scale) / 2, 0), Quaternion.identity, Vector3.one * scale);
        if (!Playing || (Paused && !showClue))
        {
            GUI.DrawTexture(new Rect(48, 55, 525, 607), panel);
            GUI.Label(new Rect(80, 81, 455, 28), "A CHOLA-INSPIRED JOURNEY  /  c. 1024 CE", eyebrow);
            GUI.Label(new Rect(80, 118, 455, 112), Paused ? "The courtyard waits" : "YUG\nThe Kaveri Settlement", heading);
            GUI.Label(new Rect(80, 237, 451, 72), "Meet the steward. Restore the irrigation channel and granary. Rebuild the temple monument.", body);
            if (confirmReset)
            {
                GUI.Label(new Rect(80, 323, 445, 64), "Start again? This replaces your saved journey.", body);
                if (GUI.Button(new Rect(80, 398, 215, 47), "Start again", button)) BeginNew();
                if (GUI.Button(new Rect(307, 398, 215, 47), "Keep journey", button)) confirmReset = false;
            }
            else
            {
                if (Playing && GUI.Button(new Rect(80, 328, 442, 47), "Return to the courtyard", button)) SetPause(false);
                if (!Playing && File.Exists(SavePath) && GUI.Button(new Rect(80, 328, 442, 47), "Continue journey", button)) ContinueJourney();
                if (GUI.Button(new Rect(80, 385, 442, 47), "New settlement", button)) { if (File.Exists(SavePath)) confirmReset = true; else BeginNew(); }
                if (Playing && GUI.Button(new Rect(80, 442, 215, 42), "Back to entrance", button)) { ReturnToEntrance(); SetPause(false); }
                if (GUI.Button(new Rect(Playing ? 307 : 80, 442, Playing ? 215 : 442, 42), "Quit", button)) { Time.timeScale = 1; Application.Quit(); }
            }
            GUI.Label(new Rect(80, 510, 453, 105), "WASD  Move     Shift  Run     Space  Jump\nArrows / right-drag  Camera     E  Interact\nJ  Inscription     Esc  Pause\nF  Village actions    Tab  Journal    G  Graphics", small);
            GUI.Label(new Rect(80, 625, 450, 23), "FICTIONAL TEMPLE  ·  ORIGINAL PROCEDURAL ART", eyebrow);
        }
        else if (showClue)
        {
            GUI.DrawTexture(new Rect(310, 150, 660, 400), panel);
            GUI.Label(new Rect(350, 178, 580, 50), "Words held in stone", heading);
            GUI.Label(new Rect(350, 255, 580, 195), progress.clue ? Inscription : "Find the inscribed stone on the left of the approach to learn the order of the seals.", body);
            if (GUI.Button(new Rect(350, 470, 580, 45), "Return  [Esc]", button)) CloseClue();
        }
        else
        {
            if (pilgrim.transform.position.z > -18) { GUI.DrawTexture(new Rect(28, 25, 560, 100), panel);
            GUI.Label(new Rect(48, 39, 520, 26), "THE STONE REMEMBERS", eyebrow);
            string objective = progress.restored ? "Sanctuary open · Enter through the stone doorway" : !progress.clue ? "Inspect the relief beside the approach" : progress.sequence==3 ? "Monument rebuilt · Release the sanctuary mechanism" : "Recovered " + MemoryCount + "/3 · Built " + progress.sequence + "/3   [J: construction clue]";
            GUI.Label(new Rect(48, 73, 520, 44), objective, small); }
            GUI.Label(new Rect(850, 32, 400, 28), "E  Interact    J  Inscription    Esc  Pause", small);
            if (nearest != "" && !ending) { GUI.DrawTexture(new Rect(300, 568, 680, 50), panel); GUI.Label(new Rect(322, 579, 640, 35), "[E]  " + nearest, body); }
            if (ending) { GUI.DrawTexture(new Rect(300, 465, 680, 90), panel); GUI.Label(new Rect(333, 485, 620, 60), "Stone bears stone. The sanctuary opens.", body); }
        }
        if (message != "" && Time.unscaledTime < messageUntil)
        { GUI.DrawTexture(new Rect(250, 639, 780, 58), panel); GUI.Label(new Rect(270, 651, 740, 40), message, small); }
    }

    void OnDestroy() { Time.timeScale = 1; if (chime != null) Destroy(chime); }
}
