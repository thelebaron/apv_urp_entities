using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DefaultNamespace
{
    [ExecuteAlways]
    public class ProbePerSceneCompanion : MonoBehaviour
    {
        void OnUpdate()
        {
            // test delete invalid guid
            if(gameObject.scene.GetGuid().Equals("00000000000000000000000000000000"))
            {
                Debug.LogError($"ProbeVolumePerSceneData: Scene {gameObject.scene.name} has invalid GUID. Cannot RegisterPerSceneData.");
                return;
            }
        }
        

    }
    
    public static class SceneExtensions
    {
        static PropertyInfo s_SceneGUID = typeof(Scene).GetProperty("guid", BindingFlags.NonPublic | BindingFlags.Instance);
        public static string GetGuid(this Scene scene)
        {
            Debug.Assert(s_SceneGUID != null, "Reflection for scene GUID failed");
            return (string)s_SceneGUID.GetValue(scene);
        }
    }
}