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

    private void OnDisable()
    {
        
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
                    NewStage();
                }
                
                if (GUILayout.Button("Open"))
                {
                    Open();
                }

                if (GUILayout.Button("Save"))
                {
                    Save();
                }

                if (GUILayout.Button("Save As"))
                {
                    SaveAs();
                }

                if (GUILayout.Button("Reload"))
                {
                    Reload();
                }
            }
            GUILayout.EndHorizontal();
        #endregion

        #region LevelTools
            GUILayoutExt.HorizontalSeparator();
            GUILayout.Label("Level Tools", EditorStyles.boldLabel);
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

    public void NewStage()
    {
        stageBuilder.NewStage();
    }

    public void Open()
    {
        var path = EditorUtility.OpenFilePanel("Open", StageBuilder.StageFolder, "asset");
        if (!string.IsNullOrEmpty(path))
        {
            if (stageBuilder.Open(UnityEditor.FileUtil.GetProjectRelativePath(path)))
            {
                Debug.LogFormat("Opened {0}", path);
            }
        }
    }

    public void Save()
    {
        if (stageBuilder.LoadedStage != null)
            stageBuilder.Save();
        else
            SaveAs();
    }

    public void SaveAs()
    {
        var rx = new Regex(@"(\d+)");
        var d = new DirectoryInfo(StageBuilder.StageFolder);
        var number = 0;
        if (d.GetFiles("Stage?.asset") is var fileInfos && fileInfos.Count() != 0)
        {
            number = fileInfos.Select(s => rx.Match(s.Name)).Where(s => s.Success).Max(s =>
            {
                int.TryParse(s.Value, out var num);
                return num;
            });
        }
        
        var path = EditorUtility.SaveFilePanel("Save As", StageBuilder.StageFolder, "Stage"+(number+1), "asset");
        if (!string.IsNullOrEmpty(path))
        {
            stageBuilder.SaveAs(UnityEditor.FileUtil.GetProjectRelativePath(path));
        }
    }

    public void Reload()
    {
        stageBuilder.Reload();
    }
}
