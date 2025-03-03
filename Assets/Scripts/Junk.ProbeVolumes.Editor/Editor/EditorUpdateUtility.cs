using UnityEditor;
using UnityEngine;

namespace Junk.ProbeVolumes.Editor
{
#if UNITY_EDITOR
    // copied from Unity.Scenes.EditorUpdateUtility
    internal static class EditorUpdateUtility
    {
        public static bool DidRequest = false;
        public static void EditModeQueuePlayerLoopUpdate()
        {
            if (!Application.isPlaying && !DidRequest)
            {
                DidRequest = true;
                EditorApplication.QueuePlayerLoopUpdate();
                EditorApplication.update += EditorUpdate;
            }
        }

        static void EditorUpdate()
        {
            DidRequest               =  false;
            EditorApplication.update -= EditorUpdate;
            EditorApplication.QueuePlayerLoopUpdate();
        }
    }
#endif
}