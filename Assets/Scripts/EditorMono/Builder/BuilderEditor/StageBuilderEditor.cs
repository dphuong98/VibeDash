using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(StageBuilder))]
public class StageBuilderEditor : Editor
{
    private StageBuilder stageBuilder;
    
    //InspectorGUI members
    private bool expandDropdown;
    private readonly Vector2 expandButtonSize = new Vector2(30, 30);

    private void OnEnable()
    {
        stageBuilder = target as StageBuilder;
    }

    //TODO what does #if UNITY_EDITOR purpose
    public override void OnInspectorGUI()
    {
        #region Info
            GUILayout.Label("Stage Info", EditorStyles.boldLabel);
            GUILayout.Label("Stage size: (" + stageBuilder.EditingStage.Size.x + ", " + stageBuilder.EditingStage.Size.y + ")");
            
            if (0 <= stageBuilder.SelectedTile.x && stageBuilder.SelectedTile.x < stageBuilder.Cols &&
                0 <= stageBuilder.SelectedTile.y && stageBuilder.SelectedTile.y < stageBuilder.Rows)
            {
                GUILayout.Label("Selected Tile: (" + (stageBuilder.SelectedTile.x+1) + ", " + (stageBuilder.SelectedTile.y+1) + ")");
            }
            else GUILayout.Label("Selected Tile: (  ,  )");
            
            GUILayout.BeginHorizontal();
            stageBuilder.SolutionMode = GUILayout.Toggle(stageBuilder.SolutionMode, "View solution");
            GUILayout.FlexibleSpace();
            stageBuilder.MovingSolution = GUILayout.Toggle(stageBuilder.MovingSolution, "Moving path");
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Speed: ");
            stageBuilder.SolutionSpeed = EditorGUILayout.IntField(stageBuilder.SolutionSpeed, GUILayout.MaxWidth(64));
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
        #endregion

        #region File
            GUILayoutExt.HorizontalSeparator();
            GUILayout.Label("File", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Loaded Stage: ", stageBuilder.LoadedStage, typeof(Stage), true);
            GUI.enabled = true;
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("New"))
                {
                    stageBuilder.NewItem();
                }
                
                if (GUILayout.Button("Open"))
                {
                    stageBuilder.Open();
                }

                if (GUILayout.Button("Save"))
                {
                    stageBuilder.Save();
                }

                if (GUILayout.Button("Save As"))
                {
                    stageBuilder.SaveAs();
                }

                if (GUILayout.Button("Reload"))
                {
                    stageBuilder.Reload();
                }
            }
            GUILayout.EndHorizontal();
        #endregion

        #region LevelTools
            GUILayoutExt.HorizontalSeparator();
            GUILayout.Label("Stage Tools", EditorStyles.boldLabel);
            expandDropdown = EditorGUILayout.Foldout(expandDropdown, "Expand & Shrink");
            if (expandDropdown)
            {
                EditorGUILayout.BeginHorizontal();

                // Expand
                EditorGUILayout.BeginVertical();
                {
                    
                    
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("↑", GUILayout.Width(expandButtonSize.x), GUILayout.Height(expandButtonSize.y)))
                        {
                            stageBuilder.ExpandTop();
                        }
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();


                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        //GUILayout.Button();
                        if (GUILayout.Button("←", GUILayout.Width(expandButtonSize.x), GUILayout.Height(expandButtonSize.y)))
                        {
                            stageBuilder.ExpandLeft();
                        }
                        if (GUILayout.Button("", GUILayout.Width(expandButtonSize.x), GUILayout.Height(expandButtonSize.y)))
                        {

                        }
                        if (GUILayout.Button("→", GUILayout.Width(expandButtonSize.x), GUILayout.Height(expandButtonSize.y)))
                        {
                            stageBuilder.ExpandRight();
                        }
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("↓", GUILayout.Width(expandButtonSize.x), GUILayout.Height(expandButtonSize.y)))
                        {
                            stageBuilder.ExpandBottom();
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
                        if (GUILayout.Button("↓", GUILayout.Width(expandButtonSize.x), GUILayout.Height(expandButtonSize.y)))
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
                        if (GUILayout.Button("→", GUILayout.Width(expandButtonSize.x), GUILayout.Height(expandButtonSize.y)))
                        {
                            stageBuilder.CollapseLeft();
                        }
                        if (GUILayout.Button("", GUILayout.Width(expandButtonSize.x), GUILayout.Height(expandButtonSize.y)))
                        {

                        }
                        if (GUILayout.Button("←", GUILayout.Width(expandButtonSize.x), GUILayout.Height(expandButtonSize.y)))
                        {
                            stageBuilder.CollapseRight();
                        }
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("↑", GUILayout.Width(expandButtonSize.x), GUILayout.Height(expandButtonSize.y)))
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
        #endregion

        //End OnInspectorGUI
    }
}
