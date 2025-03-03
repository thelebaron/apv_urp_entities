using System;
using System.Reflection;
using Unity.Entities;
using Unity.Scenes;
using Unity.Scenes.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Junk.ProbeVolumes.Editor.Editor
{
    public class SubsceneManager
    {
        public static void UnloadSubscene(SubScene subScene)
        {
            Debug.Log("UnloadSubscene");
            var loadableScenes = SubSceneInspectorUtility.GetLoadableScenes(new[] { subScene });

            if (loadableScenes.Length <= 0)
                return;

            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            foreach (var loadableScene in loadableScenes)
            {
                Unload(entityManager, subScene, loadableScene);
            }
        }


        public void RemoveSceneFromHierarchy(Scene scene)
        {
            if (scene.IsValid())
            {
                EditorSceneManager.CloseScene(scene, false); // 'false' does not save modifications
            }
        }

        
        void load(EntityManager entityManager, SubScene subscene, bool hasLoadableSections, SubSceneInspectorUtility.LoadableScene loadableScene)
        {
            entityManager.AddComponentData(loadableScene.Scene, new RequestSceneLoaded());
        }
        
        private static void Unload(EntityManager entityManager, SubScene subscene, SubSceneInspectorUtility.LoadableScene loadableScene)
        {
            // if we unload section 0, we also need to unload the entire rest
            entityManager.RemoveComponent<RequestSceneLoaded>(loadableScene.Scene);
            EditorUpdateUtility.EditModeQueuePlayerLoopUpdate();
        }
        

        public void ClearAllWorlds()
        {
            // @TODO: TEMP for debugging
            if (GUILayout.Button("ClearWorld"))
            {
                World.DisposeAllWorlds();
                DefaultWorldInitialization.Initialize("Default World", !Application.isPlaying);

                var scenes = Object.FindObjectsByType<SubScene>(FindObjectsSortMode.None);
                foreach (var scene in scenes)
                {
                    var oldEnabled = scene.enabled;
                    scene.enabled = false;
                    scene.enabled = oldEnabled;
                }

               // EditorUpdateUtility.EditModeQueuePlayerLoopUpdate();
            }
        }
    }
}