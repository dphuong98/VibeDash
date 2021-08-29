using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Testspam))]
public class TestspamEditor : Editor
{
    public Stage stageData;
    public Grid parentGrid;
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        stageData = (Stage)EditorGUILayout.ObjectField(stageData, typeof(Stage), true);
        parentGrid = (Grid)EditorGUILayout.ObjectField(parentGrid, typeof(Transform), true);

        if (GUILayout.Button("LoadLevel"))
        {
            LevelLoader.LoadLevelAt(parentGrid, stageData, new Vector2Int());
        }
    }
}
