using System.Reflection;
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
        private EntityQuery query;
        private EntityQuery cleanupQuery;
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
                    name = "ProbePerSceneComponentState"
                };
                go.hideFlags = HideFlags.DontSave;

                if (managed.BakingSet == null)
                {
                    Debug.Log($"BakingSet is null for entity: {entity} with SceneGUID: {managed.SceneGUID}");
                }

                { // DEBUG CRAP
                    //var perScene = Object.FindObjectsByType<UnityEngine.Rendering.ProbeVolumePerSceneData>(FindObjectsSortMode.None);
                    //Debug.Log($"Existing ProbeVolumePerSceneData count: {perScene.Length}");
                    
                    //var probeVolumeReference = ProbeReferenceVolume.instance.
                }

                Debug.Log($"BakingSet {managed.BakingSet} for entity: {entity} with SceneGUID: {managed.SceneGUID}");
                
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
                state.EntityManager.AddComponentObject(entity, cleanupComponent);
                state.EntityManager.SetComponentEnabled<ProbePerSceneComponentState>(entity, true);
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