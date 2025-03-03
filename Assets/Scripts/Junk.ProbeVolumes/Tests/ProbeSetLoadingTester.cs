#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering;

namespace Junk.ProbeVolumes
{
    [ExecuteAlways]
    [AddComponentMenu("")]
    internal class ProbeSetLoadingTester : MonoBehaviour
    {
        public UnityEngine.Rendering.ProbeVolumeBakingSet set;
        public UnityEngine.Rendering.ProbeVolumeBakingSet loadedSet;

        void OnEnable()
        {
            if (set == null)
                return;

            ProbeReferenceVolume.instance.SetActiveBakingSet(set);

            loadedSet = ProbeReferenceVolume.instance.currentBakingSet;
        }
    }
}
#endif