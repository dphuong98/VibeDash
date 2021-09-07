using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[CustomEditor(typeof(Testspam))]
public class TestspamEditor : Editor
{
    [FormerlySerializedAs("stageData")] public Stage stageData;
    public Grid parentGrid;
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
