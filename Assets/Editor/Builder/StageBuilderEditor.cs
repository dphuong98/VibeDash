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
public class StageBuilderEditor : BuilderEditor<StageData>
{
    private StageBuilder stageBuilder;
    
    //InspectorGUI members
    private bool expandDropdown;
    private readonly Vector2 expandButtonSize = new Vector2(30, 30);

    private new void OnEnable()
    {
        base.OnEnable();
        stageBuilder = builder as StageBuilder;
    }

    //TODO what does #if UNITY_EDITOR purpose
    public override void OnInspectorGUI()
    {
        #region Info
            GUILayout.Label("Stage Info", EditorStyles.boldLabel);
            GUILayout.Label("Stage size: (" + stageBuilder.EditingStageData.Size.x + ", " + stageBuilder.EditingStageData.Size.y + ")");

            if (0 <= stageBuilder.selectedTile.x && stageBuilder.selectedTile.x < stageBuilder.Cols &&
                0 <= stageBuilder.selectedTile.y && stageBuilder.selectedTile.y < stageBuilder.Rows)
            {
                GUILayout.Label("Selected Tile: (" + (stageBuilder.selectedTile.x+1) + ", " + (stageBuilder.selectedTile.y+1) + ")");
            }
            else GUILayout.Label("Selected Tile: (  ,  )");
            
            GUILayout.BeginHorizontal();
            stageBuilder.viewSolution = GUILayout.Toggle(stageBuilder.viewSolution, "View solution (turn off if lag)");
            GUILayout.FlexibleSpace();
            stageBuilder.movingSolution = GUILayout.Toggle(stageBuilder.movingSolution, "Moving path");
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Speed: ");
            StageRenderer.solutionSpeed = EditorGUILayout.FloatField(StageRenderer.solutionSpeed, GUILayout.MaxWidth(64));
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
            GUILayout.Label("Maximum points: " + Pathfinding.CountUniqueTiles(stageBuilder.EditingStageData.Solution));
        #endregion

        #region File
            GUILayoutExt.HorizontalSeparator();
            GUILayout.Label("File", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Loaded Stage: ", stageBuilder.LoadedStageData, typeof(StageData), true);
            GUI.enabled = true;
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("New"))
                {
                    NewItem();
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

    public override void SaveAs()
    {
        var defaultName = "";

        if (!stageBuilder.EditingStageData.IsOnBorder(stageBuilder.EditingStageData.GetEntrance()))
        {
            defaultName += "En_";
        }

        var relativeSize = stageBuilder.EditingStageData.Size.x + stageBuilder.EditingStageData.Size.y;
        if (relativeSize > 20) defaultName += "Large";
        else if (relativeSize > 15) defaultName += "Medium";
        else defaultName += "Small";
        
        var rx = new Regex(@"(\d+)");
        var d = new DirectoryInfo(StageBuilder.DefaultFolder);
        var number = 0;
        if (d.GetFiles(defaultName+"??.asset") is var fileInfos && fileInfos.Length != 0)
        {
            number = fileInfos.Select(s => rx.Match(s.Name)).Where(s => s.Success).Max(s =>
            {
                int.TryParse(s.Value, out var num);
                return num;
            });
        }
        
        var path = EditorUtility.SaveFilePanel("Save As", StageBuilder.DefaultFolder, defaultName+(number+1), "asset");
        builder.Save(FileUtil.GetProjectRelativePath(path));
    }
}
