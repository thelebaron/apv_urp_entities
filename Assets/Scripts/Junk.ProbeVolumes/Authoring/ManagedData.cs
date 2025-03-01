using System;
using System.Collections.Generic;
using System.Reflection;
using Junk.ProbeVolumes;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;

namespace Junk.ProbeVolumes
{
    
    [RequireComponent(typeof(ProbeVolume))]
    public class ManagedData : MonoBehaviour
    {
 
    }




    public class ProbeVolumeBaker : Baker<ManagedData>
    {
        public override void Bake(ManagedData authoring)
        {
            return;
            var entity = GetEntity(TransformUsageFlags.Dynamic);
        
            if(authoring.GetComponent<ProbeVolume>() == null)
                return;
        
            AddComponent<ProbeVolumeState>(entity);
            SetComponentEnabled<ProbeVolumeState>(entity, false);

            var probeData = new ProbeVolumeData(authoring.GetComponent<ProbeVolume>());
            AddComponent(entity, probeData);

        }
    }

}