using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(ProbeVolume))]
public class ManagedData : MonoBehaviour
{
 
}



public struct ProbeVolumeState : IComponentData, IEnableableComponent
{
    
}

public struct ProbeVolumeData : IComponentData
{
    public   ProbeVolume.Mode mode;
    public   Vector3          size;
    public   bool             overrideRendererFilters;
    public   float            minRendererVolumeSize;
    public   LayerMask        objectLayerMask;
    public   int              lowestSubdivLevelOverride;
    public   int              highestSubdivLevelOverride;
    public   bool             overridesSubdivLevels;
    internal bool             mightNeedRebaking;
    internal Matrix4x4        cachedTransform;
    internal int              cachedHashCode;
    public   bool             fillEmptySpaces;
    
    public ProbeVolumeData (ProbeVolume volume)
    {
        mode                     = volume.mode;
        size                     = volume.size;
        overrideRendererFilters  = volume.overrideRendererFilters;
        minRendererVolumeSize    = volume.minRendererVolumeSize;
        objectLayerMask          = volume.objectLayerMask;
        lowestSubdivLevelOverride = volume.lowestSubdivLevelOverride;
        highestSubdivLevelOverride = volume.highestSubdivLevelOverride;
        overridesSubdivLevels    = volume.overridesSubdivLevels;
        mightNeedRebaking        = false;
        cachedTransform          = Matrix4x4.identity;
        cachedHashCode           = 0;
        fillEmptySpaces          = false;
        
        // Set internal 'serializedBakingSet' using reflection.
        var bakingSetField = typeof(UnityEngine.Rendering.ProbeVolumePerSceneData).GetField("mightNeedRebaking", BindingFlags.Instance | BindingFlags.NonPublic);
        if (bakingSetField != null)
            mightNeedRebaking = (bool) bakingSetField.GetValue(volume);
        var cachedTransformField = typeof(UnityEngine.Rendering.ProbeVolumePerSceneData).GetField("cachedTransform", BindingFlags.Instance | BindingFlags.NonPublic);
        if (cachedTransformField != null)
            cachedTransform = (Matrix4x4) cachedTransformField.GetValue(volume);
        var cachedHashCodeField = typeof(UnityEngine.Rendering.ProbeVolumePerSceneData).GetField("cachedHashCode", BindingFlags.Instance | BindingFlags.NonPublic);
        if (cachedHashCodeField != null)
            cachedHashCode = (int) cachedHashCodeField.GetValue(volume);
        var fillEmptySpacesField = typeof(UnityEngine.Rendering.ProbeVolumePerSceneData).GetField("fillEmptySpaces", BindingFlags.Instance | BindingFlags.NonPublic);
        if (fillEmptySpacesField != null)
            fillEmptySpaces = (bool) fillEmptySpacesField.GetValue(volume);
    }
    
    public void SetProbeVolume(ProbeVolume target)
    {
        target.mode                     = mode;
        target.size                     = size;
        target.overrideRendererFilters  = overrideRendererFilters;
        target.minRendererVolumeSize    = minRendererVolumeSize;
        target.objectLayerMask          = objectLayerMask;
        target.lowestSubdivLevelOverride = lowestSubdivLevelOverride;
        target.highestSubdivLevelOverride = highestSubdivLevelOverride;
        target.overridesSubdivLevels    = overridesSubdivLevels;
        //target.mightNeedRebaking        = mightNeedRebaking;
        //target.cachedTransform          = cachedTransform;
        //target.cachedHashCode           = cachedHashCode;
        target.fillEmptySpaces          = fillEmptySpaces;
        
        var bakingSetField = typeof(UnityEngine.Rendering.ProbeVolumePerSceneData).GetField("mightNeedRebaking", BindingFlags.Instance | BindingFlags.NonPublic);
        if (bakingSetField != null)
            bakingSetField.SetValue(target, mightNeedRebaking);
        var cachedTransformField = typeof(UnityEngine.Rendering.ProbeVolumePerSceneData).GetField("cachedTransform", BindingFlags.Instance | BindingFlags.NonPublic);
        if (cachedTransformField != null)
            cachedTransformField.SetValue(target, cachedTransform);
        var cachedHashCodeField = typeof(UnityEngine.Rendering.ProbeVolumePerSceneData).GetField("cachedHashCode", BindingFlags.Instance | BindingFlags.NonPublic);
        if (cachedHashCodeField != null)
            cachedHashCodeField.SetValue(target, cachedHashCode);
    }
    
}

public class ProbeVolumeBakingSetData : IComponentData
{
    public int                                        Stage;
    public UnityEngine.Rendering.ProbeVolumeBakingSet BakingSet;
}

public class ProbeVolumeBaker : Baker<ManagedData>
{
    public override void Bake(ManagedData authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        
        if(authoring.GetComponent<ProbeVolume>() == null)
            return;
        
        AddComponent<ProbeVolumeState>(entity);
        SetComponentEnabled<ProbeVolumeState>(entity, false);

        var probeData = new ProbeVolumeData(authoring.GetComponent<ProbeVolume>());
        AddComponent(entity, probeData);

    }
}
