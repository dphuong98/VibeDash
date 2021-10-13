using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(LevelBuilder))]
public class LevelBuilderEditor : BuilderEditor<LevelData>
{
    private const string GameScenePath = "Assets/Scenes/Gameplay.unity";
    private LevelBuilder levelBuilder;

    private new void OnEnable()
    {
        base.OnEnable();
        levelBuilder = target as LevelBuilder;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        #region Info
            GUILayout.Label("Level Info", EditorStyles.boldLabel);
            
        #endregion

        #region File
            GUILayoutExt.HorizontalSeparator();
            GUILayout.Label("File", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Loaded Level: ", levelBuilder.LoadedLevelData, typeof(LevelData), true);
            EditorGUILayout.ObjectField("Editing Level: ", levelBuilder.EditingLevelData, typeof(LevelData), true);
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
            GUILayout.Label("Level Tools", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("ImportStage"))
            {
                ImportStage();
            }
            GUILayout.EndHorizontal();
        #endregion
        
        #region Play
        if (GUILayout.Button("Play"))
        {
            var level = levelBuilder.EditingLevelData;
            
            if (Application.isPlaying) return;

            GameManager.AutoloadLevelData = levelBuilder.EditingLevelData;
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(GameScenePath);
            EditorApplication.isPlaying = true;
        }
        #endregion
        
        //End OnInspectorGUI
    }

    public override void NewItem()
    {
        levelBuilder.NewItem();
    }

    public override void Open()
    {
        var path = EditorUtility.OpenFilePanel("Open", LevelBuilder.DefaultFolder, "asset");
        levelBuilder.Open(FileUtil.GetProjectRelativePath(path));
    }

    public override void Reload()
    {
        levelBuilder.Reload();
    }

    private void ImportStage()
    {
        var path = EditorUtility.OpenFilePanel("Open", StageBuilder.StageFolder, "asset");
        levelBuilder.ImportStage(FileUtil.GetProjectRelativePath(path));
    }
}
