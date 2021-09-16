using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(LevelBuilder))]
public class LevelBuilderEditor : Editor
{
    private const string GameScenePath = "Assets/Scenes/Gameplay.unity";
    private LevelBuilder levelBuilder;
    
    private void OnEnable()
    {
        levelBuilder = target as LevelBuilder;
    }

    private void OnDisable()
    {
        
    }

    public override void OnInspectorGUI()
    {
        #region Info
            GUILayout.Label("Level Info", EditorStyles.boldLabel);
            
        #endregion

        #region File
            GUILayoutExt.HorizontalSeparator();
            GUILayout.Label("File", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Loaded Stage: ", levelBuilder.LoadedLevel, typeof(Stage), true);
            GUI.enabled = true;
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("New"))
                {
                    NewLevel();
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
        
        #region Play
        if (GUILayout.Button("Play"))
        {
            /*var level = (target as StageBuilder).EditingStage;
                
            if (Application.isPlaying)
            {
                return;
            }

            LevelLoader.CurrentStage = level;
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            EditorSceneManager.OpenScene(GameScenePath);
            EditorApplication.isPlaying = true;*/
        }
        #endregion
        
        //End OnInspectorGUI
    }
    
    public void NewLevel()
    {
        levelBuilder.NewLevel();
    }

    public void Open()
    {
        var path = EditorUtility.OpenFilePanel("Open", LevelBuilder.LevelFolder, "asset");
        if (!string.IsNullOrEmpty(path))
        {
            if (levelBuilder.Open(UnityEditor.FileUtil.GetProjectRelativePath(path)))
            {
                Debug.LogFormat("Opened {0}", path);
            }
        }
    }

    public void Save()
    {
        if (levelBuilder.LoadedLevel != null)
            levelBuilder.Save();
        else
            SaveAs();
    }

    public void SaveAs()
    {
        var rx = new Regex(@"(\d+)");
        var d = new DirectoryInfo(LevelBuilder.LevelFolder);
        var number = 0;
        if (d.GetFiles("Level?.asset") is var fileInfos && fileInfos.Count() != 0)
        {
            number = fileInfos.Select(s => rx.Match(s.Name)).Where(s => s.Success).Max(s =>
            {
                int.TryParse(s.Value, out var num);
                return num;
            });
        }

        var path = EditorUtility.SaveFilePanel("Save As", LevelBuilder.LevelFolder, "Level"+(number+1), "asset");
        if (!string.IsNullOrEmpty(path))
        {
            levelBuilder.SaveAs(UnityEditor.FileUtil.GetProjectRelativePath(path));
        }
    }

    public void Reload()
    {
        levelBuilder.Reload();
    }
}
