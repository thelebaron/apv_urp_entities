using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Junk.Probes
{
    [AddComponentMenu("")]
    public class ProbeCompanionCleanup : MonoBehaviour
    {
        
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