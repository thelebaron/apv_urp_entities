#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Junk.ProbeVolumes
{
    [ExecuteAlways]
    public class SceneProbeSetManualLoadingTester : MonoBehaviour
    {
        public UnityEngine.Rendering.ProbeVolumeBakingSet set;
        public bool                                       probeDebug;

        public UnityEngine.Rendering.ProbeVolumeBakingSet currentLoadedSet;

        [ContextMenu("Unload")]
        void Unload()
        {
            if (ProbeReferenceVolume.instance == null)
            {
                Debug.LogError("ProbeReferenceVolume.instance is null");
                return;
            }

            ProbeReferenceVolume.instance.SetActiveBakingSet(null);
        }

        [ContextMenu("Load")]
        void Load()
        {
            if (set == null)
            {
                return;
            }

            ProbeReferenceVolume.instance.SetActiveBakingSet(set);
        }


        [ContextMenu("Log Set")]
        void LogSet()
        {
            Debug.Log($"Current set is {ProbeReferenceVolume.instance.currentBakingSet}");
        }

        private void Update()
        {
            if (ProbeReferenceVolume.instance == null)
            {
                Debug.Log("ProbeReferenceVolume.instance is null");
                return;
            }

            currentLoadedSet = ProbeReferenceVolume.instance.currentBakingSet;

            if (Keyboard.current.uKey.wasPressedThisFrame)
            {
                Unload();
            }

            if (Keyboard.current.lKey.wasPressedThisFrame)
            {
                Load();
            }
            /*
            #if UNITY_EDITOR
            if (!probeDebug)
            {
                return;
            }
            // Get the internal property "probeVolumeDebug" (it's nonpublic)
            var debugProp = typeof(ProbeReferenceVolume).GetProperty("probeVolumeDebug", BindingFlags.Instance | BindingFlags.NonPublic);

            if (debugProp != null)
            {
                object probeVolumeDebug = debugProp.GetValue(ProbeReferenceVolume.instance);
                if (probeVolumeDebug == null)
                {
                    Debug.LogError("probeVolumeDebug is null");
                    return;
                }

                // Get the field "drawProbes" from the probeVolumeDebug object
                FieldInfo drawProbesField = probeVolumeDebug.GetType().GetField("drawProbes", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if (drawProbesField != null)
                {
                    // Toggle the value
                    bool currentValue = (bool)drawProbesField.GetValue(probeVolumeDebug);
                    drawProbesField.SetValue(probeVolumeDebug, probeDebug);
                    Debug.Log("Toggled drawProbes to: " + probeDebug);
                }
            }
            #endif*/
        }
    }
}
#endif