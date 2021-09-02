using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StageBuilder))]
public class StageBuilderEditor : Editor
{
    //LevelBuilder members
    private StageBuilder stageBuilder;
    
    //InspectorGUI members
    private bool dimensionExpaned;
    private Vector2 dimensionButtonSize = new Vector2(30, 30);

    private void OnEnable()
    {
        stageBuilder = target as StageBuilder;
    }

    private void OnDisable()
    {
        
    }

    //TODO what does #if UNITY_EDITOR purpose
    public override void OnInspectorGUI()
    {
        //Stage data
        GUILayout.Label("Stage Info");
        GUILayout.Label("Stage size: (" + stageBuilder.CurrentStage.Size.x + ", " + stageBuilder.CurrentStage.Size.y + ")");
        
        GUILayout.Label("File");
        
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Targeted Stage: ", stageBuilder.CurrentStage, typeof(Stage), true);
        GUI.enabled = true;
        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("New"))
            {
                stageBuilder.NewStage();
            }
        }
        GUILayout.EndHorizontal();

        dimensionExpaned = EditorGUILayout.Foldout(dimensionExpaned, "Expand & Shrink");

        if (dimensionExpaned)
        {
            EditorGUILayout.BeginHorizontal();

            // Expand
            EditorGUILayout.BeginVertical();
            {
                
                
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("↑", GUILayout.Width(dimensionButtonSize.x), GUILayout.Height(dimensionButtonSize.y)))
                    {
                        stageBuilder.ExpandBottom();
                    }
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    //GUILayout.Button();
                    if (GUILayout.Button("←", GUILayout.Width(dimensionButtonSize.x), GUILayout.Height(dimensionButtonSize.y)))
                    {
                        stageBuilder.ExpandLeft();
                    }
                    if (GUILayout.Button("", GUILayout.Width(dimensionButtonSize.x), GUILayout.Height(dimensionButtonSize.y)))
                    {

                    }
                    if (GUILayout.Button("→", GUILayout.Width(dimensionButtonSize.x), GUILayout.Height(dimensionButtonSize.y)))
                    {
                        stageBuilder.ExpandRight();
                    }
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("↓", GUILayout.Width(dimensionButtonSize.x), GUILayout.Height(dimensionButtonSize.y)))
                    {
                        stageBuilder.ExpandTop();
                    }
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();


            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical();
            // Shrink
            {
                
                
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("↓", GUILayout.Width(dimensionButtonSize.x), GUILayout.Height(dimensionButtonSize.y)))
                    {
                        stageBuilder.CollapseTop();
                    }
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    //GUILayout.Button();
                    if (GUILayout.Button("→", GUILayout.Width(dimensionButtonSize.x), GUILayout.Height(dimensionButtonSize.y)))
                    {
                        stageBuilder.CollapseLeft();
                    }
                    if (GUILayout.Button("", GUILayout.Width(dimensionButtonSize.x), GUILayout.Height(dimensionButtonSize.y)))
                    {

                    }
                    if (GUILayout.Button("←", GUILayout.Width(dimensionButtonSize.x), GUILayout.Height(dimensionButtonSize.y)))
                    {
                        stageBuilder.CollapseRight();
                    }
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("↑", GUILayout.Width(dimensionButtonSize.x), GUILayout.Height(dimensionButtonSize.y)))
                    {
                        stageBuilder.CollapseBottom();
                    }
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();
                
                
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }
        // End dimension foldout
        
        //End OnInspectorGUI
    }
}
