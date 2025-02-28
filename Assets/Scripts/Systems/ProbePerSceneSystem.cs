using System.Collections.Generic;
using System.Reflection;
using Unity.Assertions;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;

namespace DefaultNamespace
{
    //[DisableAutoCreation]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    public partial struct ProbePerSceneSystem : ISystem
    {
        private EntityQuery      query;
        private EntityQuery      cleanupQuery;
        
        public void OnCreate(ref SystemState state)
        {
            query = SystemAPI.QueryBuilder()
                .WithAll<ProbePerSceneComponentData>()
                .WithDisabled<ProbePerSceneComponentState>()
                .WithNone<ProbeReferenceCleanup>()
                .Build();
            
            cleanupQuery = SystemAPI.QueryBuilder()
                .WithNone<ProbePerSceneComponentData>()
                .WithNone<ProbePerSceneComponentState>()
                .WithAll<ProbeReferenceCleanup>()
                .Build();
        }

        public void OnUpdate(ref SystemState state)
        {
            var entities = query.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                var managed = SystemAPI.ManagedAPI.GetComponent<ProbePerSceneComponentData>(entity);
                var go      = new GameObject
                {
                    name      = "ECS ProbeVolumePerSceneData",
                    hideFlags = HideFlags.DontSave
                };
                
                // TRES Important: if gameobject is not deactivated, onenable is called for any script added to it
                // which causes initialization before we have a chance to set the serializedBakingSet field.
                go.SetActive(false);
                
                // Add the component normally.
                var probeVolumePerSceneData = go.AddComponent<UnityEngine.Rendering.ProbeVolumePerSceneData>();
                
                
                // Set internal 'serializedBakingSet' using reflection.
                var bakingSetField = typeof(UnityEngine.Rendering.ProbeVolumePerSceneData).GetField("serializedBakingSet", BindingFlags.Instance | BindingFlags.NonPublic);
                if (bakingSetField != null)
                {
                    bakingSetField.SetValue(probeVolumePerSceneData, managed.BakingSet);
                }
                else
                {
                    Debug.LogError("Failed to set serializedBakingSet field");
                }

                // Set internal 'sceneGUID' using reflection.
                var sceneGUIDField = typeof(UnityEngine.Rendering.ProbeVolumePerSceneData)
                    .GetField("sceneGUID", BindingFlags.Instance | BindingFlags.NonPublic);
                if (sceneGUIDField != null)
                {
                    sceneGUIDField.SetValue(probeVolumePerSceneData, managed.SceneGUID);
                }

                var cleanupComponent = new ProbeReferenceCleanup
                {
                    Reference = go
                };
                go.SetActive(true);
                state.EntityManager.AddComponentObject(entity, cleanupComponent);
                state.EntityManager.SetComponentEnabled<ProbePerSceneComponentState>(entity, true);
                
                ProbeReferenceVolume.instance.SetActiveBakingSet(null);
              
                Debug.Log($"Set active baking set:go");
            }

            foreach (var (probePerScene, entity) in SystemAPI.Query<RefRO<ProbePerSceneComponentState>>().WithAll<ProbeReferenceCleanup>().WithAll<ProbePerSceneComponentData>().WithEntityAccess())
            {
                if (ProbeReferenceVolume.instance.currentBakingSet ==null)
                {
                    var managed = SystemAPI.ManagedAPI.GetComponent<ProbePerSceneComponentData>(entity);
                    ProbeReferenceVolume.instance.SetActiveBakingSet(managed.BakingSet);
                    Debug.Log($"Set active baking set: {managed.BakingSet}");
                }
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

        }
    }
}