using System;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using Unity.Scenes;
using Unity.Scenes.Editor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Junk.ProbeVolumes.Editor
{
    public static  class CompanionManager
    {
        const HideFlags CompanionFlags =
            HideFlags.HideInHierarchy       |
            HideFlags.DontSaveInBuild       |
            HideFlags.DontUnloadUnusedAsset |
            HideFlags.NotEditable;
        
        internal static void SetCompanionFlags(GameObject gameObject)
        {
            var companionFlags = CompanionFlags;
            //if (EditorSceneManager.GetPreviewScenesVisibleInHierarchy())
            {
                //companionFlags &= ~HideFlags.HideInHierarchy;
            }

            gameObject.hideFlags = companionFlags;
        }

        static void CreateCompanionScene()
        {
            
        }
        internal static void MoveAllSubsceneGameObjectsToCompanionScene(SubScene subScene)
        {
            if (GetCompanionScene(out var companionScene))
            {
                if (companionScene == default)
                {
                    Debug.Log($"Companion scene: isLoaded {companionScene.isLoaded} name {companionScene.name}");
                }
                
                var loadableScenes = SubSceneInspectorUtility.GetLoadableScenes(new[] { subScene });
                if (loadableScenes.Length <= 0)
                    return;
                
                var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                foreach (var loadableScene in loadableScenes)
                {
                    var sceneSectionData = entityManager.GetComponentData<SceneSectionData>(loadableScene.Scene);
                    var sceneTagQuery    = entityManager.CreateEntityQuery(typeof(SceneTag), typeof(CompanionLink));
                    var sceneTagEntities = sceneTagQuery.ToEntityArray(Allocator.Temp);
                    
                    foreach (var entity in sceneTagEntities)
                    {
                        var sceneTag = entityManager.GetSharedComponent<SceneTag>(entity);
                        if (sceneTag.SceneEntity == loadableScene.Scene)
                        {
                            var companionLink = entityManager.GetComponentData<CompanionLink>(entity);
                            
                            SceneManager.MoveGameObjectToScene(companionLink.Companion, companionScene);
                        }
                    }
                    sceneTagQuery.Dispose();
                    sceneTagEntities.Dispose();
                }
            }
        }

        internal static void MoveToCompanionScene(GameObject gameObject)
        {
            if (GetCompanionScene(out var companionScene))
            {
                SceneManager.MoveGameObjectToScene(gameObject, companionScene);
            }
        }
        internal static void RecreateCompanionScenes()
        {
            //Unity.Entities.CompanionGameObjectUtility.CreateCompanionScenes();
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("Unity.Entities.CompanionGameObjectUtility", false))
                .FirstOrDefault(t => t != null);

            MethodInfo method = type.GetMethod("CreateCompanionScenes", BindingFlags.Static | BindingFlags.NonPublic);
            if (method == null)
            {
                Debug.LogError("CreateCompanionScenes method not found.");
                return;
            }

            method.Invoke(null, null);
        }
        public static bool GetCompanionSceneLiveConversion(out Scene scene)
        {
            scene = default;
            
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("Unity.Entities.CompanionGameObjectUtility", false))
                .FirstOrDefault(t => t != null);

            if (type == null)
            {
                Debug.Log("CompanionGameObjectUtility type not found in loaded assemblies.");
                return false;
            }

            FieldInfo liveConversionField = type.GetField("_companionSceneLiveConversion", BindingFlags.Static | BindingFlags.NonPublic);
            if (liveConversionField == null)
            {
                Debug.Log("CompanionGameObjectUtility liveConversionField not found");
                return false;
            }
            scene = (Scene)liveConversionField.GetValue(null);
            return true;
        }
        
        public static bool GetCompanionScene(out Scene scene)
        {
            scene = default;
            
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("Unity.Entities.CompanionGameObjectUtility", false))
                .FirstOrDefault(t => t != null);

            if (type == null)
            {
                Debug.Log("CompanionGameObjectUtility type not found in loaded assemblies.");
                return false;
            }

            FieldInfo field = type.GetField("_companionScene", BindingFlags.Static | BindingFlags.NonPublic);
            if (field == null)
            {
                Debug.Log("CompanionGameObjectUtility _companionScene not found");
                return false;
            }
            scene = (Scene)field.GetValue(null);
            return true;
        }
    }
}