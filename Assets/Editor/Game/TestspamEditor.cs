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
    }
}
