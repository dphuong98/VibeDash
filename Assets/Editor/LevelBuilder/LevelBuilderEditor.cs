using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelBuilder))]
public class LevelBuilderEditor : Editor
{
    //LevelBuilder members
    private LevelBuilder _levelBuilder;
    
    //InspectorGUI members
    private bool dimensionExpaned;
    private Vector2 dimensionButtonSize = new Vector2(30, 30);

    private void OnEnable()
    {
        _levelBuilder = target as LevelBuilder;
    }

    private void OnDisable()
    {
        
    }

    //TODO what does #if UNITY_EDITOR purpose
    public override void OnInspectorGUI()
    {
        //Stage data
        GUILayout.Label("Level Info", EditorStyles.boldLabel);
        GUILayout.Label("Level size: (" + _levelBuilder.CurrentLevel.Size.x + ", " + _levelBuilder.CurrentLevel.Size.y + ")");
        
        GUILayoutExt.HorizontalSeparator();
        GUILayout.Label("File", EditorStyles.boldLabel);
        GUI.enabled = false;
        EditorGUILayout.ObjectField("Targeted Level: ", _levelBuilder.CurrentLevel, typeof(Level), true);
        GUI.enabled = true;
        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("New"))
            {
                _levelBuilder.NewStage();
            }
        }
        GUILayout.EndHorizontal();

        GUILayoutExt.HorizontalSeparator();
        GUILayout.Label("Level Tools", EditorStyles.boldLabel);
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
                        _levelBuilder.ExpandBottom();
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
                        _levelBuilder.ExpandLeft();
                    }
                    if (GUILayout.Button("", GUILayout.Width(dimensionButtonSize.x), GUILayout.Height(dimensionButtonSize.y)))
                    {

                    }
                    if (GUILayout.Button("→", GUILayout.Width(dimensionButtonSize.x), GUILayout.Height(dimensionButtonSize.y)))
                    {
                        _levelBuilder.ExpandRight();
                    }
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("↓", GUILayout.Width(dimensionButtonSize.x), GUILayout.Height(dimensionButtonSize.y)))
                    {
                        _levelBuilder.ExpandTop();
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
                        _levelBuilder.CollapseTop();
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
                        _levelBuilder.CollapseLeft();
                    }
                    if (GUILayout.Button("", GUILayout.Width(dimensionButtonSize.x), GUILayout.Height(dimensionButtonSize.y)))
                    {

                    }
                    if (GUILayout.Button("←", GUILayout.Width(dimensionButtonSize.x), GUILayout.Height(dimensionButtonSize.y)))
                    {
                        _levelBuilder.CollapseRight();
                    }
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("↑", GUILayout.Width(dimensionButtonSize.x), GUILayout.Height(dimensionButtonSize.y)))
                    {
                        _levelBuilder.CollapseBottom();
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
