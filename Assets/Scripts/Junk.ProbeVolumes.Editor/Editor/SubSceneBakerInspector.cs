#if UNITY_EDITOR

using Junk.ProbeVolumes.Hybrid;
using Unity.Scenes;
using UnityEditor;
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
                SubsceneBakerWindow.StartBake(subScene.SceneAsset);
#endif
            }

            //base.OnInspectorGUI();
        }
    }
}

#endif