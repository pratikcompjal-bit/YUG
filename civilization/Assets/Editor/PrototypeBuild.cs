using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PrototypeBuild
{
    [MenuItem("Prototype/Create Scene and Build Windows")]
    public static void CreateAndBuild()
    {
        string root = Directory.GetParent(Application.dataPath).FullName;
        Directory.CreateDirectory(Path.Combine(root, "Assets/Scenes"));
        Directory.CreateDirectory(Path.Combine(root, "Build"));
        Directory.CreateDirectory(Path.Combine(root, "work"));
        PlayerSettings.companyName = "Temple Sketches";
        PlayerSettings.productName = "YUG Civilization";
        PlayerSettings.defaultScreenWidth = 1600;
        PlayerSettings.defaultScreenHeight = 900;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = true;
        PlayerSettings.forceSingleInstance = true;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { UnityEngine.Rendering.GraphicsDeviceType.Direct3D11 });
        PlayerSettings.usePlayerLog = false;
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
        PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Standalone, ManagedStrippingLevel.Disabled);
        QualitySettings.SetQualityLevel(QualitySettings.names.Length - 1, true);
        QualitySettings.shadows = ShadowQuality.All;
        QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
        QualitySettings.shadowProjection = ShadowProjection.StableFit;
        QualitySettings.shadowCascades = 2;
        QualitySettings.shadowDistance = 85f;
        QualitySettings.antiAliasing = 4;
        QualitySettings.vSyncCount = 1;
        // Runtime geometry has no scene material references for the shader collector.
        var graphics = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
        var included = graphics.FindProperty("m_AlwaysIncludedShaders");
        foreach (string shaderName in new[] { "Standard", "Unlit/Color", "Hidden/TempleLens", "Particles/Standard Unlit", "Sprites/Default", "Skybox/Procedural", "Unlit/Transparent" })
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null) continue;
            bool found = false;
            for (int i = 0; i < included.arraySize; i++)
                if (included.GetArrayElementAtIndex(i).objectReferenceValue == shader) found = true;
            if (!found) { included.InsertArrayElementAtIndex(included.arraySize); included.GetArrayElementAtIndex(included.arraySize - 1).objectReferenceValue = shader; }
        }
        graphics.ApplyModifiedPropertiesWithoutUndo();
        if (true)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject rootObject = new GameObject("THE STONE REMEMBERS — World");
            rootObject.AddComponent<TempleWorld>();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Temple.unity");
        }
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Temple.unity", true) };
        AssetDatabase.SaveAssets();
        BuildReport result = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] { "Assets/Scenes/Temple.unity" },
            locationPathName = Path.Combine(root, "Build/YUG Civilization.exe"),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });
        string report = "Result: " + result.summary.result + "\nErrors: " + result.summary.totalErrors
            + "\nWarnings: " + result.summary.totalWarnings + "\nBytes: " + result.summary.totalSize
            + "\nDuration: " + result.summary.totalTime;
        File.WriteAllText(Path.Combine(root, "work/build-result.txt"), report);
        Debug.Log(report);
        if (result.summary.result != BuildResult.Succeeded)
            throw new Exception("Native build failed: " + result.summary.result);
    }
}
