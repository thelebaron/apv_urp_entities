using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public class ProbeVolumePerSceneDataBaker : Baker<ProbeVolumePerSceneData>
{
    public override void Bake(ProbeVolumePerSceneData authoring)
    {
        return;
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        
        AddComponentObject(entity, new ProbePerSceneComponentData
        {
            SceneGUID = authoring.sceneGUID,
            BakingSet = authoring.bakingSet
        });
        //AddComponentObject(entity, authoring.gameObject);
        AddComponent<ProbePerSceneComponentState>(entity);
        SetComponentEnabled<ProbePerSceneComponentState>(entity, false);
    }
}
public class ProbePerSceneComponentData : IComponentData, IDisposable, ICloneable
{
    public string                                     SceneGUID;
    public UnityEngine.Rendering.ProbeVolumeBakingSet BakingSet;
    public int                                       Stage;
    
    public void Dispose()
    {
        
    }

    public object Clone()
    {
        return new ProbePerSceneComponentData { SceneGUID = SceneGUID, BakingSet = BakingSet };
    }
}

public struct ProbePerSceneComponentState : IComponentData, IEnableableComponent
{
    public int State;
}

public class ProbeReferenceCleanup : ICleanupComponentData, IDisposable, ICloneable
{
    public GameObject Reference;

    public void Dispose()
    {
    #if UNITY_EDITOR
        Object.DestroyImmediate(Reference);
    #else
        Object.Destroy(Reference);
    #endif
    }

    public object Clone()
    {
        return new ProbeReferenceCleanup { Reference = this.Reference };
    }
}