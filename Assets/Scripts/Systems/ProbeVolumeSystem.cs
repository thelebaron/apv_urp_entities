using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace DefaultNamespace
{

    [DisableAutoCreation]
    //[WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    public partial struct ProbeVolumeSystem : ISystem
    {        
        private EntityQuery query;
        private      EntityQuery cleanupQuery;
        
        public void OnCreate(ref SystemState state)
        {
            query = SystemAPI.QueryBuilder()
                .WithAll<ProbeVolumeData>()
                .WithDisabled<ProbeVolumeState>()
                .WithNone<ProbeReferenceCleanup>()
                .Build();
            
            cleanupQuery = SystemAPI.QueryBuilder()
                .WithNone<ProbeVolumeData>()
                .WithNone<ProbeVolumeState>()
                .WithAll<ProbeReferenceCleanup>()
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
                    name = "ProbeVolume"
                };
                go.hideFlags = HideFlags.DontSave;
                
                // Add the component normally.
                var probeVolume = go.AddComponent<ProbeVolume>();
                probeData.SetProbeVolume(probeVolume);
                
                state.EntityManager.SetComponentEnabled<ProbeVolumeState>(entity, true);
                
                var cleanupComponent = new ProbeReferenceCleanup
                {
                    Reference = go
                };
                state.EntityManager.AddComponentObject(entity, cleanupComponent);
            }
            
            var cleanupEntities = cleanupQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in cleanupEntities)
            {
                var managed = SystemAPI.ManagedAPI.GetComponent<ProbeReferenceCleanup>(entity);
                if (managed.Reference != null)
                {
                    Object.DestroyImmediate(managed.Reference);
                }
                state.EntityManager.RemoveComponent<ProbeReferenceCleanup>(entity);
            }
            /*
            foreach (var (data, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<ProbeVolumeBakingSet>().WithDisabled<ProbeVolumeState>().WithEntityAccess())
            {
                var managed     = SystemAPI.ManagedAPI.GetComponent<ProbeVolumeBakingSet>(entity);

                if (managed.Stage == 0)
                {
                    ProbeReferenceVolume.instance.SetActiveBakingSet(null);
                    managed.Stage = 1;
                    //return;
                }

                if (managed.Stage == 1 && ProbeReferenceVolume.instance.currentBakingSet==null)
                {
                    ProbeReferenceVolume.instance.SetActiveBakingSet(managed.BakingSet);
                    SystemAPI.SetComponentEnabled<ProbeVolumeState>(entity, true);
                    managed.Stage = 0;
                }
            }*/
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
}