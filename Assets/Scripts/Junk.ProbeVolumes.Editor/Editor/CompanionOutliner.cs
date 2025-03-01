#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.Reflection;

namespace Junk.ProbeVolumes
{
    public class CompanionOutliner : EditorWindow
    {
        Vector2 scrollPos;
        GUIStyle leftAlignedButtonStyle;
        int selectedTab = 0;
        readonly string[] tabNames = new string[] { "Companion Scene", "Live Conversion" };

        [MenuItem("Window/Companion Outliner")]
        public static void ShowWindow()
        {
            GetWindow<CompanionOutliner>("Companion Outliner");
        }

        private void OnEnable()
        {
            leftAlignedButtonStyle = new GUIStyle(EditorStyles.toolbarButton)
            {
                alignment = TextAnchor.MiddleLeft
            };
        }

        private void OnGUI()
        {
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);

            GameObject[] objectsToDisplay = selectedTab == 0 
                ? GetCompanionRootObjects() 
                : GetLiveConversionRootObjects();

            if (objectsToDisplay.Length == 0)
            {
                EditorGUILayout.LabelField("No objects found in the selected scene.");
            }
            else
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
                foreach (GameObject obj in objectsToDisplay)
                {
                    if (obj == null)
                        continue;

                    GUIContent content = EditorGUIUtility.ObjectContent(obj, typeof(GameObject));
                    content.text = " " + obj.name;
                    if (GUILayout.Button(content, leftAlignedButtonStyle))
                    {
                        Selection.activeGameObject = obj;
                    }
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private static GameObject[] GetCompanionRootObjects()
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("Unity.Entities.CompanionGameObjectUtility", false))
                .FirstOrDefault(t => t != null);

            if (type == null)
            {
                Debug.Log("CompanionGameObjectUtility type not found in loaded assemblies.");
                return Array.Empty<GameObject>();
            }

            FieldInfo companionField = type.GetField("_companionScene", BindingFlags.Static | BindingFlags.NonPublic);
            if (companionField == null)
            {
                Debug.Log("CompanionGameObjectUtility companionField not found");
                return Array.Empty<GameObject>();
            }

            Scene companionScene = (Scene)companionField.GetValue(null);
            if (!(companionScene.IsValid() && companionScene.isLoaded))
                return Array.Empty<GameObject>();

            return companionScene.GetRootGameObjects();
        }

        private static GameObject[] GetLiveConversionRootObjects()
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("Unity.Entities.CompanionGameObjectUtility", false))
                .FirstOrDefault(t => t != null);

            if (type == null)
            {
                Debug.Log("CompanionGameObjectUtility type not found in loaded assemblies.");
                return Array.Empty<GameObject>();
            }

            FieldInfo liveConversionField = type.GetField("_companionSceneLiveConversion", BindingFlags.Static | BindingFlags.NonPublic);
            if (liveConversionField == null)
            {
                Debug.Log("CompanionGameObjectUtility liveConversionField not found");
                return Array.Empty<GameObject>();
            }

            Scene liveConversionScene = (Scene)liveConversionField.GetValue(null);
            if (!(liveConversionScene.IsValid() && liveConversionScene.isLoaded))
                return Array.Empty<GameObject>();

            return liveConversionScene.GetRootGameObjects();
        }
    }
}
#endif
