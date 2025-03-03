#if UNITY_EDITOR

using Junk.ProbeVolumes.Editor.Editor;
using Junk.ProbeVolumes.Hybrid;
using Unity.Entities;
using Unity.Scenes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Junk.ProbeVolumes.Editor
{
    [CustomEditor(typeof(LightmappedSubscene))]
    public class SubSceneBakerInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Bake"))
            {
                if(target is not LightmappedSubscene)
                    return;
                
                var lightmappedSubscene = target as LightmappedSubscene;
                var subScene   = lightmappedSubscene?.GetComponent<SubScene>();
                if (subScene == null)
                    return;
#if UNITY_EDITOR
                SubsceneBakerWindow.StartBake(subScene);
#endif
            }

            if (GUILayout.Button("Clear World"))
            {
                World.DisposeAllWorlds();
            }

            if (GUILayout.Button("unload Scene"))
            {
                
                var subScene   = target as LightmappedSubscene;
                if (subScene == null)
                    return;
                SubsceneManager.UnloadSubscene(subScene.GetComponent<SubScene>());
            }

            if (GUILayout.Button("close companion Scenes"))
            {
                if (CompanionManager.GetCompanionScene(out var companionScene))
                    EditorSceneManager.CloseScene(companionScene, true);
                if (CompanionManager.GetCompanionSceneLiveConversion(out var companionSceneLiveConversion))
                    EditorSceneManager.CloseScene(companionSceneLiveConversion, true);
            }
            //base.OnInspectorGUI();
        }
    }
}

#endif