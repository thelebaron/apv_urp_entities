﻿using Unity.Scenes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Editor
{
    [CustomEditor(typeof(SubSceneBaker))]
    public class SubSceneBakerInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Bake"))
            {
                var component = target as SubSceneBaker;
                var subScene   = component.GetComponent<SubScene>();
#if UNITY_EDITOR
                SceneBakerWindow.StartBake(subScene.SceneAsset);
#endif
            }

            base.OnInspectorGUI();
        }
    }
}