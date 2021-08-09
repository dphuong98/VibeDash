using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.TerrainAPI;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.XR;

[CustomEditor(typeof(SceneCameraController))]
public class SceneCameraSelector : Editor
{
    private int cameraNumber;
    
    private float buttonWidth = 40f;
    private float buttonHeight = 20f;
    
    public void OnEnable()
    {
        SceneView.duringSceneGui += DrawButton;
    }

    private void DrawButton(SceneView sceneview)
    {
        var sceneCameraController = (SceneCameraController)target;
        
        Handles.BeginGUI();

        GUILayout.BeginVertical();
        GUILayout.Space(200f);

        var buttonLabel = sceneCameraController.GetViewMode() == SceneCameraController.ViewMode.Isometric ? "Iso" : "Per";
        if (GUILayout.Button(buttonLabel, GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
        {
            sceneCameraController.ToggleViewMode();
        }
        GUILayout.EndVertical();

        Handles.EndGUI();
    }
}