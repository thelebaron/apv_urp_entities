
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine.SceneManagement;
#endif
    using UnityEngine;

namespace Junk.Probes
{
    public class CompanionUtility
    {
        
    }
    

#if UNITY_EDITOR
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

        private static void CleanupTempObjects()
        {
            //Debug.Log("Cleanup temp objects");
            var currentScene = SceneManager.GetActiveScene();
            var rootObjects = currentScene.GetRootGameObjects();
            //Debug.Log($" rootObjects.Length {rootObjects.Length}       sf   "      );
            //Debug.Log($"currentScene.path {currentScene.path}" );

            foreach (var rootObject in rootObjects)
            {
                //Debug.Log($" rootObject.name {rootObject.name}");
                if (rootObject.GetComponent<ProbeCompanionCleanup>())
                {
                    //Debug.Log($" Got probe {rootObject.name}");
                    Object.DestroyImmediate(rootObject);
                }
            }
            
            var found = Object.FindObjectsByType<ProbeCompanionCleanup>(FindObjectsSortMode.None);
            Debug.Log($"found {found.Length} probes");
            // Replace "YourTempObjectType" with your component type, or use a tag or other identifier
            foreach (var obj in Object.FindObjectsByType<ProbeCompanionCleanup>(FindObjectsSortMode.None))
            {
                Debug.Log(obj.gameObject.name);
                Object.DestroyImmediate(obj.gameObject);
            }
        }
    }
#endif

}