#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Junk.Probes;
using Unity.Scenes;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Junk.Probes
{
    public class SubsceneBakerWindow : EditorWindow
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
        static         string            openSceneGuid;

        [MenuItem("Window/Subscene Baker")]
        public static void ShowWindow()
        {
            GetWindow<SubsceneBakerWindow>("Subscene Baker");
            var scene = SceneManager.GetActiveScene();
            openSceneGuid = scene.GetGuid();
        }

        public static void StartBake(SceneAsset scene)
        {
            if (isBaking)
                return;
            isBaking       = true;
            hasStartedBake = false;
            bakeResult     = false;

            originalScenePath = SceneManager.GetActiveScene().path;
            StoreSubscene();

            targetScene = scene;
            SubsceneBakerWindow window = GetWindow<SubsceneBakerWindow>("Scene Baker");
            window.Show();

            string targetScenePath = AssetDatabase.GetAssetPath(targetScene);
            EditorSceneManager.OpenScene(targetScenePath, OpenSceneMode.Single);
            EditorSceneManager.SaveOpenScenes();
            window.CleanNullProbeInstances();
            window.CloseCompanionPreviewScenes();

            // Delay bake start by 1 second.
            startBakeTime            =  EditorApplication.timeSinceStartup + 1.0;
            progressId               =  Progress.Start("Baking Lightmaps", "Preparing light bake...");
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
                float estimatedBakeTime = 30f;
                float progress          = Mathf.Clamp01((float)((currentTime - startBakeTime) / estimatedBakeTime));
                EditorApplication.delayCall += () => { Progress.Report(progressId, progress, $"Baking Lightmaps... {Mathf.RoundToInt(progress * 100)}%"); };
            }
            else if (!hasStartedBake)
            {
                EditorApplication.delayCall += () => { Progress.Report(progressId, 0f, "Waiting to start bake..."); };
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

                isBaking                 =  false;
                EditorApplication.update -= StartBake;
                //GetWindow<SubsceneBakerWindow>()?.Close();
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
            if (Selection.activeGameObject != null)
            {
                var selectedGameObject = Selection.activeGameObject;
                var subscene           = selectedGameObject.GetComponent<SubScene>();
                if (subscene != null)
                {
                    if (GUILayout.Button("Bake subscene lighting"))
                    {
                        SubsceneBakerWindow.StartBake(subscene.SceneAsset);
                    }
                }
            }


            GUILayout.Label("Scene Baker is running:", EditorStyles.boldLabel);
            GUILayout.Label(hasStartedBake ? (Lightmapping.isRunning ? "Baking in progress..." : "Bake complete.") : "Waiting to start bake...");

            if (GUILayout.Button("Clean Null Probe Instances"))
            {
                CleanNullProbeInstances();
                Debug.Log("Cleaned null probe instances.");
            }

            if (GUILayout.Button("Clear ALL ProbeVolumePerSceneData"))
            {
                var objs = Object.FindObjectsByType<ProbeVolumePerSceneData>(FindObjectsSortMode.None);
                for (int i = objs.Length - 1; i >= 0; i--)
                {
                    var obj = objs[i];
                    Object.DestroyImmediate(obj.gameObject, true);
                }
            }


            /*
            // note if scene is changed this doesnt work
            if (probeVolumes != null)
            {
                // remove null entries
                probeVolumes = probeVolumes.Where(probe => probe != null).ToList();
                // remove mismatched guids
                probeVolumes = probeVolumes.Where(probe => openSceneGuid.Equals(probe.gameObject.scene.GetGuid())).ToList();


                var correctScene = probeVolumes.Count > 1
                    ? probeVolumes.All(probe => openSceneGuid.Equals(probe.gameObject.scene.GetGuid()))
                    : probeVolumes.Any(probe => openSceneGuid.Equals(probe.gameObject.scene.GetGuid())); // line 148 error here

                if (!correctScene)
                    GUILayout.Label("Mismatched scene guids for probe volumes.");
                else
                {
                    GUILayout.Label($"Probe Volumes in the current scene: {probeVolumes.Count}", EditorStyles.boldLabel);
                    foreach (var probe in probeVolumes)
                    {
                        GUILayout.Label(probe.name);
                    }
                }
            }
            else
            {
                GUILayout.Label("No Probe Volumes found in the current scene.");
            }*/
        }

        void CleanNullProbeInstances()
        {
            probeVolumes.Clear();
            var probeVolumeType    = typeof(ProbeVolume);
            var instancesFieldInfo = probeVolumeType.GetField("instances", BindingFlags.NonPublic | BindingFlags.Static);
            if (instancesFieldInfo == null)
                return;

            var probeVolume = FindObjectsByType<ProbeVolume>(FindObjectsSortMode.None);
            var instances   = instancesFieldInfo.GetValue(probeVolume) as IList<ProbeVolume>;
            if (instances == null)
                return;

            for (int i = instances.Count - 1; i >= 0; i--)
            {
                if (instances[i] == null)
                {
                    instances.RemoveAt(i);
                    continue;
                }

                var currentScene = SceneManager.GetActiveScene();
                var probe        = instances[i];

                if (probe.gameObject.scene != currentScene)
                {
                    instances.RemoveAt(i);
                    continue;
                }

                probeVolumes.Add(instances[i]);
            }


            if (ProbeReferenceVolume.instance.perSceneDataList != null)
            {
                var fullPerSceneDataList = ProbeReferenceVolume.instance.perSceneDataList;
                // Create a copy so modifications don't interfere with iteration.
                var listCopy = new List<ProbeVolumePerSceneData>(fullPerSceneDataList);
                foreach (var perScene in listCopy)
                {
                    if (perScene == null)
                    {
                        fullPerSceneDataList.Remove(perScene);
                        continue;
                    }

                    // Use string.IsNullOrEmpty to check for null or empty scene name.
                    string sceneName = perScene.gameObject.scene.name;
                    if (string.IsNullOrEmpty(sceneName))
                    {
                        //Debug.Log($"Removing perScene with null or empty scene name. List count before removal: {fullPerSceneDataList.Count}");
                        Object.DestroyImmediate(perScene.gameObject);
                        fullPerSceneDataList.Remove(perScene);
                        continue;
                    }

                    if (sceneName.Equals("CompanionScene"))
                    {
                        //Debug.Log($"Removing CompanionScene perScene. List count before removal: {fullPerSceneDataList.Count}");
                        Object.DestroyImmediate(perScene.gameObject);
                        fullPerSceneDataList.Remove(perScene);
                    }
                }

            }
        }

        private void CloseCompanionPreviewScenes()
        {
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

            MethodInfo method = companionType.GetMethod("AssemblyReloadEventsOnbeforeAssemblyReload", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
            {
                Debug.LogError("Method 'AssemblyReloadEventsOnbeforeAssemblyReload' not found.");
                return;
            }

            method.Invoke(null, null);
        }
    }
}

#endif