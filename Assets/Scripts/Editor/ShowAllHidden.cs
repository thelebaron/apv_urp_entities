﻿using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class ShowAllHidden
    {
        [MenuItem("Tools/Show All Hidden")]
        public static void Show()
        {
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (go.hideFlags == HideFlags.HideInHierarchy)
                {
                    go.hideFlags = HideFlags.None;
                }
            }
        }
    }
}