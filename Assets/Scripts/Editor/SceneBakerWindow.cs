using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class SceneBakerWindow : EditorWindow
{
    private        List<ProbeVolume> probeVolumes = new List<ProbeVolume>();
    private static SceneAsset        targetScene;
    private static string            originalScenePath;
    private static double            startBakeTime;
    private static bool              hasStartedBake;
    private static bool              isBaking;
    private static bool              bakeResult;
    private static string            selectedGameObjectName;
    private static int               selectedGameObjectHash;
    private static int               progressId;

    public static void StartBake(SceneAsset scene)
    {
        if (isBaking)
            return;
        isBaking = true;
        hasStartedBake = false;
        bakeResult = false;

        originalScenePath = SceneManager.GetActiveScene().path;
        StoreSubscene();

        targetScene = scene;
        SceneBakerWindow window = GetWindow<SceneBakerWindow>("Scene Baker");
        window.Show();

        string targetScenePath = AssetDatabase.GetAssetPath(targetScene);
        EditorSceneManager.OpenScene(targetScenePath, OpenSceneMode.Single);
        EditorSceneManager.SaveOpenScenes();
        window.CleanNullProbeInstances();
        window.CloseCompanionPreviewScenes();

        // Delay bake start by 1 second.
        startBakeTime = EditorApplication.timeSinceStartup + 1.0;
        progressId = Progress.Start("Baking Lightmaps", "Preparing light bake...");
        EditorApplication.update += StartBake;
    }

    private static void StoreSubscene()
    {
        if (Selection.activeGameObject != null)
        {
            selectedGameObjectName = Selection.activeGameObject.name;
            selectedGameObjectHash = Selection.activeGameObject.GetHashCode();
        }
        else
        {
            selectedGameObjectName = "";
        }
    }

    private static async void StartBake()
    {
        double currentTime = EditorApplication.timeSinceStartup;

        if (!hasStartedBake && currentTime >= startBakeTime)
        {
            bakeResult     = Lightmapping.BakeAsync();
            hasStartedBake = true;
        }

        if (hasStartedBake && Lightmapping.isRunning)
        {
            // Simulate progress update over an estimated bake time.
            float estimatedBakeTime = 30f;
            float progress = Mathf.Clamp01((float)((currentTime - startBakeTime) / estimatedBakeTime));
            EditorApplication.delayCall += () =>
            {
                Progress.Report(progressId, progress, $"Baking Lightmaps... {Mathf.RoundToInt(progress * 100)}%");
            };
        }
        else if (!hasStartedBake)
        {
            EditorApplication.delayCall += () =>
            {
                Progress.Report(progressId, 0f, "Waiting to start bake...");
            };
        }

        if (hasStartedBake && !Lightmapping.isRunning)
        {
            EditorApplication.delayCall += () =>
            {
                Progress.Report(progressId, 1f, "Lightmap bake complete");
                Progress.Finish(progressId);
            };

            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
            SelectSubscene();
            
            // somehow this breaks other runtime stuff
            //ProbeReferenceVolume.instance.SetActiveBakingSet(null);

            isBaking = false;
            EditorApplication.update -= StartBake;
            GetWindow<SceneBakerWindow>()?.Close();
        }
    }

    private static void SelectSubscene()
    {
        if (!string.IsNullOrEmpty(selectedGameObjectName))
        {
            var obj = GameObject.Find(selectedGameObjectName);
            if (obj != null)
            {
                Selection.activeGameObject = obj;
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Scene Baker is running...", EditorStyles.boldLabel);
        GUILayout.Label(hasStartedBake
            ? (Lightmapping.isRunning ? "Baking in progress..." : "Bake complete.")
            : "Waiting to start bake...");
        
        // probeVolumes
        if (probeVolumes.Count > 0)
        {
            GUILayout.Label($"Probe Volumes in the current scene: {probeVolumes.Count}", EditorStyles.boldLabel);
            foreach (var probe in probeVolumes)
            {
                GUILayout.Label(probe.name);
            }
        }
        else
        {
            GUILayout.Label("No Probe Volumes found in the current scene.");
        }
    }
    
    void CleanNullProbeInstances()
    {
        probeVolumes.Clear();
        // Get the type of ProbeVolume.
        var probeVolumeType = typeof(ProbeVolume);

        // Retrieve the non-public static field "instances".
        var instancesFieldInfo = probeVolumeType.GetField("instances", BindingFlags.NonPublic | BindingFlags.Static);
        if (instancesFieldInfo == null)
            return;

        var probeVolume = FindObjectsByType<ProbeVolume>(FindObjectsSortMode.None);
        
        // Assume "instances" is a collection (e.g., List<ProbeVolume>).
        var instances = instancesFieldInfo.GetValue(probeVolume) as IList<ProbeVolume>;
        if (instances == null)
            return;

        //Debug.Log($"ProbeVolume Instances count: {instances.Count}");
        
        // Iterate backwards to safely remove null entries.
        for (int i = instances.Count - 1; i >= 0; i--)
        {
            if (instances[i] == null)
            {
                //Debug.Log($"Probe is null.");
                instances.RemoveAt(i);
                continue;
            }
            
            // currentScene
            var currentScene = SceneManager.GetActiveScene();
            var probe = instances[i];
            
            if(probe.gameObject.scene != currentScene)
            {
                //Debug.Log($"Probe {probe.name} is not in the current scene.");
                instances.RemoveAt(i);
                continue;
            }
            probeVolumes.Add(instances[i]);
        }
    }
    
    void CloseCompanionPreviewScenes()
    {
#if UNITY_EDITOR
        // Search through all loaded assemblies for the internal type.
        Type companionType = null;
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            companionType = assembly.GetType("Unity.Entities.CompanionGameObjectUtility");
            if (companionType != null)
                break;
        }
        if (companionType == null)
        {
            Debug.LogError("Type 'Unity.Entities.CompanionGameObjectUtility' not found.");
            return;
        }

        // Retrieve the private static method.
        MethodInfo method = companionType.GetMethod("AssemblyReloadEventsOnbeforeAssemblyReload", BindingFlags.NonPublic | BindingFlags.Static);
        if (method == null)
        {
            Debug.LogError("Method 'AssemblyReloadEventsOnbeforeAssemblyReload' not found.");
            return;
        }

        // Invoke the internal method.
        method.Invoke(null, null);
#endif
    }
}
