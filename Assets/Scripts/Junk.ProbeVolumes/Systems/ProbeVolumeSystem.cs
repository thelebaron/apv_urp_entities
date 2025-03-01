using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Junk.ProbeVolumes
{
    [DisableAutoCreation]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    public partial struct ProbeVolumeSystem : ISystem
    {        
        private EntityQuery query;
        private      EntityQuery cleanupQuery;
        
        public void OnCreate(ref SystemState state)
        {
            query = SystemAPI.QueryBuilder()
                .WithAll<ProbeVolumeData>()
                .WithDisabled<ProbeVolumeState>()
                .WithNone<ReferenceCleanup>()
                .Build();
            
            cleanupQuery = SystemAPI.QueryBuilder()
                .WithNone<ProbeVolumeData>()
                .WithNone<ProbeVolumeState>()
                .WithAll<ReferenceCleanup>()
                .Build();
        }

        public void OnUpdate(ref SystemState state)
        {
            var entities = query.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                var probeData = SystemAPI.GetComponent<ProbeVolumeData>(entity);
                
                var go      = new GameObject
                {
                    name = "ECS ProbeVolume"
                };
                go.SetActive(false);
                go.hideFlags = HideFlags.DontSave;
                
                // Add the component normally.
                var probeVolume = go.AddComponent<ProbeVolume>();
                probeData.SetProbeVolume(probeVolume);
                go.AddComponent<ProbeCompanionCleanup>();
                
                
                var cleanupComponent = new ReferenceCleanup
                {
                    Reference = go
                };
                state.EntityManager.AddComponentObject(entity, cleanupComponent);
                go.SetActive(true);
                state.EntityManager.SetComponentEnabled<ProbeVolumeState>(entity, true);
            }
            
            var cleanupEntities = cleanupQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in cleanupEntities)
            {
                var managed = SystemAPI.ManagedAPI.GetComponent<ReferenceCleanup>(entity);
                if (managed.Reference != null)
                {
                    Object.DestroyImmediate(managed.Reference);
                }
                state.EntityManager.RemoveComponent<ReferenceCleanup>(entity);
            }
        
        }
    }
    /*
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem | WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.Default)]
    public partial class ProbePerSceneRepatch : SystemBase
    {
        protected override void OnUpdate()
        {
            foreach (var (data, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PerSceneGUID>().WithAll<ProbeVolumePerSceneData>().WithEntityAccess())
            {
                var managed     = SystemAPI.ManagedAPI.GetComponent<ProbeVolumePerSceneData>(entity);
                var guidData   = SystemAPI.ManagedAPI.GetComponent<PerSceneGUID>(entity);
                
                if (managed == null)
                {
                    Debug.Log("No ProbeVolumePerSceneData found");
                }
                if (guidData == null)
                {
                    Debug.Log("No GUID data found for ProbeVolumePerSceneData");
                    continue;
                }
                if(managed.sceneGUID != guidData.SceneGUID)
                    managed.sceneGUID = guidData.SceneGUID;
            }
        }
    }*/
    
    public class ReferenceCleanup : ICleanupComponentData, IDisposable, ICloneable
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
            return new ReferenceCleanup { Reference = this.Reference };
        }
    }
}