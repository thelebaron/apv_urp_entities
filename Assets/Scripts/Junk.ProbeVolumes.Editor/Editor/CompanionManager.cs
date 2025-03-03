using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Junk.ProbeVolumes.Editor.Editor
{
    public static  class CompanionManager
    {
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

            Debug.Log("got liveConversionScene");
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

            Debug.Log("got companionScene");
            scene = (Scene)field.GetValue(null);
            return true;
        }
    }
}