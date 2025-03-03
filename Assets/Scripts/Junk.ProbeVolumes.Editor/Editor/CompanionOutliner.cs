#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.Reflection;
using Object = UnityEngine.Object;

namespace Junk.ProbeVolumes
{
    public class CompanionOutliner : EditorWindow
    {
        Vector2 scrollPos;
        GUIStyle leftAlignedButtonStyle;
        int selectedTab = 0;
        readonly string[] tabNames = new string[] { "Companion Scene", "Live Conversion", "Main Scene" };

        [MenuItem("Window/Companion Outliner")]
        public static void ShowWindow()
        {
            GetWindow<CompanionOutliner>("Companion Outliner");
        }

        private void OnEnable()
        {
            var editorStylesType = typeof(EditorStyles);
            var currentField     = editorStylesType.GetField("s_Current", BindingFlags.Static | BindingFlags.NonPublic);
            var currentValue     = currentField?.GetValue(null);

            if (currentValue != null)
            {
                leftAlignedButtonStyle = new GUIStyle(EditorStyles.toolbarButton)
                {
                    alignment = TextAnchor.MiddleLeft
                };
            }
            else
                leftAlignedButtonStyle = new GUIStyle { alignment = TextAnchor.MiddleLeft };
        }

        private void OnGUI()
        {
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);

            GameObject[] objectsToDisplay = selectedTab switch
            {
                0 => GetCompanionRootObjects(),
                1 => GetLiveConversionRootObjects(),
                2 => GetMainSceneHiddenRootObjects(),
                _ => Array.Empty<GameObject>()
            };

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
            if (objectsToDisplay.Length == 0)
            {
                EditorGUILayout.LabelField("No objects found in the selected scene.");
            }
            else
            {
                foreach (GameObject obj in objectsToDisplay)
                {
                    if (obj == null)
                        continue;

                    string guid = GetSceneGUID(obj.scene);
                    string displayText = $"{obj.name} : {guid}";

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(displayText, leftAlignedButtonStyle))
                    {
                        Selection.activeGameObject = obj;
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        Object.DestroyImmediate(obj);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();

            // Bottom button to destroy all gameobjects in the current list
            if (objectsToDisplay.Length > 0)
            {
                if (GUILayout.Button("Destroy All in Current Tab"))
                {
                    foreach (GameObject obj in objectsToDisplay.ToArray())
                    {
                        if (obj != null)
                            Object.DestroyImmediate(obj);
                    }
                }
            }
        }

        private static string GetSceneGUID(Scene scene)
        {
            if (scene.IsValid() && !string.IsNullOrEmpty(scene.path))
            {
                string guid = AssetDatabase.AssetPathToGUID(scene.path);
                return string.IsNullOrEmpty(guid) ? "N/A" : guid;
            }
            return "N/A";
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

        private static GameObject[] GetMainSceneHiddenRootObjects()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            if (!(currentScene.IsValid() && currentScene.isLoaded))
                return Array.Empty<GameObject>();

            GameObject[] rootObjects = currentScene.GetRootGameObjects();
            return rootObjects.Where(obj => obj.hideFlags != HideFlags.None).ToArray();
        }
    }
}
#endif
