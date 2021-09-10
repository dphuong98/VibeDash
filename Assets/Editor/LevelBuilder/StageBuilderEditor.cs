using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(StageBuilder))]
public class StageBuilderEditor : Editor
{
    //LevelBuilder members
    private static readonly string SaveFolder = "Assets/Resources/Levels";
    private static readonly string GameScenePath = "Assets/Scenes/Gameplay.unity";
    private StageBuilder stageBuilder;
    
    //InspectorGUI members
    private bool expandDropdown;
    private Vector2 expandButtonSize = new Vector2(30, 30);

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
        #region LevelInfo
            GUILayout.Label("Level Info", EditorStyles.boldLabel);
            GUILayout.Label("Level size: (" + stageBuilder.EditingStage.Size.x + ", " + stageBuilder.EditingStage.Size.y + ")");
            if (0 <= stageBuilder.SelectedTile.x && stageBuilder.SelectedTile.x < stageBuilder.Cols &&
                0 <= stageBuilder.SelectedTile.y && stageBuilder.SelectedTile.y < stageBuilder.Rows)
            {
                GUILayout.Label("Selected Tile: (" + (stageBuilder.SelectedTile.x+1) + ", " + (stageBuilder.SelectedTile.y+1) + ")");
            }
            else GUILayout.Label("Selected Tile: (0, 0)");

            stageBuilder.GraphMode = GUILayout.Toggle(stageBuilder.GraphMode, "GraphMode");
            stageBuilder.CheatMode = GUILayout.Toggle(stageBuilder.CheatMode, "CheatMode");
        #endregion

        #region File
            GUILayoutExt.HorizontalSeparator();
            GUILayout.Label("File", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Loaded Level: ", stageBuilder.LoadedStage, typeof(Stage), true);
            GUI.enabled = true;
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("New"))
                {
                    stageBuilder.NewStage();
                }
                
                if (GUILayout.Button("Open"))
                {
                    var path = EditorUtility.OpenFilePanel("Open", SaveFolder, "asset");
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (stageBuilder.Open(FileUtil.GetProjectRelativePath(path)))
                        {
                            Debug.LogFormat("Opened {0}", path);
                        }
                    }
                }

                if (GUILayout.Button("Save"))
                {
                    stageBuilder.Save();
                }

                if (GUILayout.Button("Save As"))
                {
                    var path = EditorUtility.SaveFilePanel("Save As", SaveFolder, "Level", "asset");
                    if (!string.IsNullOrEmpty(path))
                    {
                        stageBuilder.SaveAs(FileUtil.GetProjectRelativePath(path));
                    }
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
                            stageBuilder.ExpandBottom();
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

        #region Play
            if (GUILayout.Button("Play"))
            {
                var level = (target as StageBuilder).EditingStage;
                
                if (Application.isPlaying)
                {
                    return;
                }

                LevelLoader.CurrentStage = level;
                AssetDatabase.SaveAssets();
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                EditorSceneManager.OpenScene(GameScenePath);
                EditorApplication.isPlaying = true;
            }
        #endregion
        
        //End OnInspectorGUI
    }
}
