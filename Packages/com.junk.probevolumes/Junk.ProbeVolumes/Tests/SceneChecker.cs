#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Junk.ProbeVolumes
{
    [AddComponentMenu("")]
    public class SceneChecker : MonoBehaviour
    {
        public List<ProbeVolume> probeVolumes;

        [ContextMenu("Check Scenes")]
        void CheckScenesList()
        {
            var scenes = SceneManager.loadedSceneCount;

            for (int i = 0; i < scenes; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                Debug.Log($"Scene {i}: {scene.name}");
            }

            var setups = EditorSceneManager.GetSceneManagerSetup();
            for (int i = 0; i < setups.Length; i++)
            {
                Debug.Log($"Editor Scene {i}: path {setups[i].path} and subscene {setups[i].isSubScene}");

            }

            //var probeInstances = ProbeVolume.instances;
            CleanNullProbeInstances();
            CloseCompanionPreviewScenes();
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

            Debug.Log($"ProbeVolume Instances count: {instances.Count}");

            // Iterate backwards to safely remove null entries.
            for (int i = instances.Count - 1; i >= 0; i--)
            {
                if (instances[i] == null)
                {
                    Debug.Log($"Probe is null.");
                    instances.RemoveAt(i);
                    continue;
                }

                var probe = instances[i];
                if (probe.gameObject.scene != gameObject.scene)
                {
                    Debug.Log($"Probe {probe.name} is not in the current scene.");
                    instances.RemoveAt(i);
                    continue;
                }

                probeVolumes.Add(instances[i]);

            }
        }

        void CloseCompanionPreviewScenes()
        {
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
        }
    }
}
#endif
