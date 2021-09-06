using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(LevelBuilder))]
public class LevelBuilderEditor : Editor
{
    //LevelBuilder members
    private static readonly string SaveFolder = "Assets/Resources/Levels";
    private static readonly string GameScenePath = "Assets/Scenes/Gameplay.unity";
    private LevelBuilder levelBuilder;
    
    //InspectorGUI members
    private bool dimensionExpaned;
    private Vector2 dimensionButtonSize = new Vector2(30, 30);

    private void OnEnable()
    {
        levelBuilder = target as LevelBuilder;
    }

    private void OnDisable()
    {
        
    }

    //TODO what does #if UNITY_EDITOR purpose
    public override void OnInspectorGUI()
    {
        #region LevelInfo
            GUILayout.Label("Level Info", EditorStyles.boldLabel);
            GUILayout.Label("Level size: (" + levelBuilder.EditingLevel.Size.x + ", " + levelBuilder.EditingLevel.Size.y + ")");
            if (0 <= levelBuilder.SelectedTile.x && levelBuilder.SelectedTile.x < levelBuilder.Cols &&
                0 <= levelBuilder.SelectedTile.y && levelBuilder.SelectedTile.y < levelBuilder.Rows)
            {
                GUILayout.Label("Selected Tile: (" + (levelBuilder.SelectedTile.x+1) + ", " + (levelBuilder.SelectedTile.y+1) + ")");
            }
            else GUILayout.Label("Selected Tile: (0, 0)");
        #endregion

        #region File
            GUILayoutExt.HorizontalSeparator();
            GUILayout.Label("File", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Loaded Level: ", levelBuilder.LoadedLevel, typeof(Level), true);
            GUI.enabled = true;
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("New"))
                {
                    levelBuilder.NewLevel();
                }
                
                if (GUILayout.Button("Open"))
                {
                    var path = EditorUtility.OpenFilePanel("Open", SaveFolder, "asset");
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (levelBuilder.Open(FileUtil.GetProjectRelativePath(path)))
                        {
                            Debug.LogFormat("Opened {0}", path);
                        }
                    }
                }

                if (GUILayout.Button("Save"))
                {
                    levelBuilder.Save();
                }

                if (GUILayout.Button("Save As"))
                {
                    var path = EditorUtility.SaveFilePanel("Save As", SaveFolder, "Level", "asset");
                    if (!string.IsNullOrEmpty(path))
                    {
                        levelBuilder.SaveAs(FileUtil.GetProjectRelativePath(path));
                    }
                }

                if (GUILayout.Button("Reload"))
                {
                    levelBuilder.Reload();
                }
            }
            GUILayout.EndHorizontal();
        #endregion

        #region LevelTools
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
                            levelBuilder.ExpandBottom();
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
                            levelBuilder.ExpandLeft();
                        }
                        if (GUILayout.Button("", GUILayout.Width(dimensionButtonSize.x), GUILayout.Height(dimensionButtonSize.y)))
                        {

                        }
                        if (GUILayout.Button("→", GUILayout.Width(dimensionButtonSize.x), GUILayout.Height(dimensionButtonSize.y)))
                        {
                            levelBuilder.ExpandRight();
                        }
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("↓", GUILayout.Width(dimensionButtonSize.x), GUILayout.Height(dimensionButtonSize.y)))
                        {
                            levelBuilder.ExpandTop();
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
                            levelBuilder.CollapseTop();
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
                            levelBuilder.CollapseLeft();
                        }
                        if (GUILayout.Button("", GUILayout.Width(dimensionButtonSize.x), GUILayout.Height(dimensionButtonSize.y)))
                        {

                        }
                        if (GUILayout.Button("←", GUILayout.Width(dimensionButtonSize.x), GUILayout.Height(dimensionButtonSize.y)))
                        {
                            levelBuilder.CollapseRight();
                        }
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("↑", GUILayout.Width(dimensionButtonSize.x), GUILayout.Height(dimensionButtonSize.y)))
                        {
                            levelBuilder.CollapseBottom();
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
                var level = (target as LevelBuilder).EditingLevel;
                
                if (Application.isPlaying || !level.IsClearable())
                {
                    Debug.Log("Level is not clearable!");
                    return;
                }

                LevelLoader.CurrentLevel = level;
                AssetDatabase.SaveAssets();
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                EditorSceneManager.OpenScene(GameScenePath);
                EditorApplication.isPlaying = true;
            }
        #endregion
        
        //End OnInspectorGUI
    }
}
