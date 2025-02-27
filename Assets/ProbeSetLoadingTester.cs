using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class ProbeSetLoadingTester : MonoBehaviour
{
    public UnityEngine.Rendering.ProbeVolumeBakingSet set;
    public UnityEngine.Rendering.ProbeVolumeBakingSet loadedSet;
    void OnEnable()
    {
        if (set == null)
        {
            return;
        }
        
        ProbeReferenceVolume.instance.SetActiveBakingSet(set);
        
        loadedSet = ProbeReferenceVolume.instance.currentBakingSet;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
