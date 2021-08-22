using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Testspam))]
public class TestspamEditor : Editor
{
    public Object stageData;
    public Transform levelTransform;
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        stageData = EditorGUILayout.ObjectField(stageData, typeof(StageScriptable), true);

        if (GUILayout.Button("LoadStage"))
        {
            LevelLoader.LoadStageAt(levelTransform, stageData as StageScriptable, Vector3.zero);
        }
    }
}
