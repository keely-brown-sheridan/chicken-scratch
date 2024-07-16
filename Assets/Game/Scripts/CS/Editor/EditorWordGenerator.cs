using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorWordGenerator : EditorWindow
{
    [MenuItem("GameData/Create New Words")]
    public static void ShowWindow()
    {
        GetWindow(typeof(EditorWordGenerator));
    }


    private void OnGUI()
    {
        GUILayout.Label("Create New Word", EditorStyles.boldLabel);
    }
}
