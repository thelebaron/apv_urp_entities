using System.Linq;
using Unity.Entities;
using Unity.Entities.Conversion;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;

namespace Junk.Probes.Authoring
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class AddAdaptiveProbeVolumeToCompanionComponentSupportedTypes
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            
        }
        static AddAdaptiveProbeVolumeToCompanionComponentSupportedTypes()
        {
            CompanionComponentSupportedTypes.Types = CompanionComponentSupportedTypes
                .Types
                .Concat(new ComponentType[]
                {
                    typeof(ProbeVolume),
                    typeof(ProbeVolumePerSceneData),
                })
                .ToArray();
        }
    }
    #if UNITY_EDITOR
    class ProbeVolumeCompanionBaker : Baker<ProbeVolume>
    {
        public override void Bake(ProbeVolume authoring)
        {
            // Setting companions to Dynamic
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, authoring);
        }
    }
    
    class ProbeVolumePerSceneDataCompanionBaker : Baker<ProbeVolumePerSceneData>
    {
        public override void Bake(ProbeVolumePerSceneData authoring)
        {
            // Setting companions to Dynamic
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, authoring);
        }
    }
    #endif
}