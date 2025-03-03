#if UNITY_EDITOR
using Junk.ProbeVolumes.Hybrid;
using Unity.Entities;
using Unity.Scenes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

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

            if (GUILayout.Button("Dispose World"))
            {
                World.DisposeAllWorlds();
            }

            if (GUILayout.Button("ClosePreviewScene"))
            {
                if (CompanionManager.GetCompanionScene(out var companionScene))
                    EditorSceneManager.ClosePreviewScene(companionScene);
                if (CompanionManager.GetCompanionSceneLiveConversion(out var companionSceneLiveConversion))
                    EditorSceneManager.ClosePreviewScene(companionSceneLiveConversion);
            }
            if (GUILayout.Button("unload Scene"))
            {
                
                var subScene   = target as LightmappedSubscene;
                if (subScene == null)
                    return;
                SubsceneManager.UnloadSubscene(subScene.GetComponent<SubScene>());
            }


            if (GUILayout.Button("Clear World"))
            {
                SubsceneManager.ClearAllWorlds();
            }

            if (GUILayout.Button("MoveAllSubsceneGameObjectsToCompanionScenes"))
            {

                var lightmappedSubscene = target as LightmappedSubscene;
                var subScene            = lightmappedSubscene?.GetComponent<SubScene>();
                CompanionManager.RecreateCompanionScenes();
                CompanionManager.MoveAllSubsceneGameObjectsToCompanionScene(subScene);
            }
            //base.OnInspectorGUI();
        }
    }
}

#endif