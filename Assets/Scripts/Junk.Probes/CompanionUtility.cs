
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine.SceneManagement;
    using System;
    using System.Linq;
    using System.Reflection;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using Object = UnityEngine.Object;

    namespace Junk.Probes
{
    public class CompanionUtility
    {
        [MenuItem("Tools/CompanionUtility/Check scene status")]
        private static void CheckSceneStatus()
        {
            var scenes = SceneManager.sceneCount;
            var previewSceneCount = EditorSceneManager.previewSceneCount;
            Debug.Log($"Found {SceneManager.sceneCount} scenes and {previewSceneCount} preview scenes. CompanionScene loaded: {IsCompanionSceneLoaded()}, CompanionLiveScene loaded: {IsCompanionSceneLiveConversionLoaded()}");
            
            var currentScene = SceneManager.GetActiveScene();
            var rootObjects  = currentScene.GetRootGameObjects();
            //Debug.Log($" rootObjects.Length {rootObjects.Length}       sf   "      );
            //Debug.Log($"currentScene.path {currentScene.path}" );

            foreach (var rootObject in rootObjects)
            {
                //Debug.Log($" rootObject.name {rootObject.name}");

            }
        }
        
        [MenuItem("Tools/CompanionUtility/Check objects")]
        private static void CheckObjects()
        {
            var currentScene = SceneManager.GetActiveScene();
            var rootObjects  = currentScene.GetRootGameObjects();
            Debug.Log($" rootObjects.Length {rootObjects.Length} ");
            //Debug.Log($"currentScene.path {currentScene.path}" );

            foreach (var rootObject in rootObjects)
            {
                Debug.Log($" rootObject.name {rootObject.name} flags:{ rootObject.gameObject.hideFlags}");
                
                //if(rootObject.gameObject.hideFlags)
                //Object.DestroyImmediate(rootObject);
                //HideInHierarchy, NotEditable, DontSaveInBuild, DontUnloadUnusedAsset
            }
            
            
        }
        
        private static bool IsCompanionSceneLoaded()
        {
            // Search all loaded assemblies for the type.
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("Unity.Entities.CompanionGameObjectUtility", false))
                .FirstOrDefault(t => t != null);

            if (type == null)
            {
                Debug.Log("CompanionGameObjectUtility type not found in loaded assemblies.");
                return false;
            }

            FieldInfo companionField      = type.GetField("_companionScene", BindingFlags.Static               | BindingFlags.NonPublic);
            if (companionField == null)
            {
                Debug.Log($"CompanionGameObjectUtility companionField not found");
                return false;
            }

            Scene companionScene      = (Scene)companionField.GetValue(null);

            bool companionLoaded      = companionScene.IsValid()      && companionScene.isLoaded;

            return companionLoaded;
        }

        private static bool IsCompanionSceneLiveConversionLoaded()
        {
            // Search all loaded assemblies for the type.
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("Unity.Entities.CompanionGameObjectUtility", false))
                .FirstOrDefault(t => t != null);

            if (type == null)
            {
                Debug.Log("CompanionGameObjectUtility type not found in loaded assemblies.");
                return false;
            }

            FieldInfo liveConversionField = type.GetField("_companionSceneLiveConversion", BindingFlags.Static | BindingFlags.NonPublic);
            if (liveConversionField == null)
                return false;

            Scene liveConversionScene = (Scene)liveConversionField.GetValue(null);

            bool liveConversionLoaded = liveConversionScene.IsValid() && liveConversionScene.isLoaded;

            return liveConversionLoaded;
        }
    }


    /// <summary>
    /// This is to cleanup any go created by a system for hybrid use. Unsure how actual cleanup should happen on worldshutdown
    /// </summary>
    [InitializeOnLoad]
    public static class CompanionCleanupUtility
    {
        
        static CompanionCleanupUtility()
        {
            AssemblyReloadEvents.beforeAssemblyReload += CleanupTempObjects;
            
        }

        [MenuItem("Tools/CompanionUtility/FORCE DELETE ORPHANED")]
        private static void CleanupTempObjects()
        {
            var currentScene = SceneManager.GetActiveScene();
            var rootObjects = currentScene.GetRootGameObjects();

            foreach (var rootObject in rootObjects)
            {
                //Debug.Log($" rootObject.name {rootObject.name}");
                if (rootObject.GetComponent<ProbeCompanionCleanup>())
                {
                    //Debug.Log($" Got probe {rootObject.name}");
                    Object.DestroyImmediate(rootObject);
                    continue;
                }
            }
            // errors in the same loop just get it again and do second check
            rootObjects = currentScene.GetRootGameObjects();
            foreach (var rootObject in rootObjects)
            {
                //HideInHierarchy, NotEditable, DontSaveInBuild, DontUnloadUnusedAsset
                const HideFlags combinedFlags = HideFlags.HideInHierarchy | HideFlags.NotEditable | HideFlags.DontSaveInBuild | HideFlags.DontUnloadUnusedAsset;
                if (rootObject.gameObject.hideFlags == combinedFlags)
                {
                    Debug.Log($"found {rootObject.name} for cleanup");
                    //Object.DestroyImmediate(rootObject);
                    continue;
                }
            }
            
            // doesnt work i guess not same scene
            //var found = Object.FindObjectsByType<ProbeCompanionCleanup>(FindObjectsSortMode.None);
            //Debug.Log($"found {found.Length} probes");
            // Replace "YourTempObjectType" with your component type, or use a tag or other identifier
            //foreach (var obj in Object.FindObjectsByType<ProbeCompanionCleanup>(FindObjectsSortMode.None))
            {
                //Debug.Log(obj.gameObject.name);
                //Object.DestroyImmediate(obj.gameObject);
            }
        }
        
        [MenuItem("Tools/CompanionUtility/FORCE DELETE COMPANIONS")]
        private static void CleanupCompanionObjects()
        {
            // Search all loaded assemblies for the type.
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("Unity.Entities.CompanionGameObjectUtility", false))
                .FirstOrDefault(t => t != null);

            if (type == null)
            {
                Debug.Log("CompanionGameObjectUtility type not found in loaded assemblies.");
                return;
            }

            FieldInfo companionField      = type.GetField("_companionScene", BindingFlags.Static               | BindingFlags.NonPublic);
            if (companionField == null)
            {
                Debug.Log("companionField not found");
                return;
            }

            Scene companionScene      = (Scene)companionField.GetValue(null);

            var rootObjects = companionScene.GetRootGameObjects();
            Debug.Log($"Found {rootObjects.Length} companion objects in companionScene");
            foreach (var rootObject in rootObjects)
            {
                Object.DestroyImmediate(rootObject);
            }
        }
    }
}
#endif