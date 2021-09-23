using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(LevelBuilder))]
public class LevelBuilderEditor : BuilderEditor<Level>
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
            EditorGUILayout.ObjectField("Loaded Level: ", levelBuilder.LoadedLevel, typeof(Level), true);
            EditorGUILayout.ObjectField("Editing Level: ", levelBuilder.EditingLevel, typeof(Level), true);
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

    public override void NewItem()
    {
        levelBuilder.NewItem();
    }

    public override void Open()
    {
        var path = EditorUtility.OpenFilePanel("Open", LevelBuilder.DefaultFolder, "asset");
        levelBuilder.Open(FileUtil.GetProjectRelativePath(path));
    }

    public override void Save()
    {
        if (!levelBuilder.Save())
            SaveAs();
    }

    public override void SaveAs()
    {
        var rx = new Regex(@"(\d+)");
        var d = new DirectoryInfo(LevelBuilder.DefaultFolder);
        var number = 0;
        if (d.GetFiles(LevelBuilder.DefaultName+"?.asset") is var fileInfos && fileInfos.Count() != 0)
        {
            number = fileInfos.Select(s => rx.Match(s.Name)).Where(s => s.Success).Max(s =>
            {
                int.TryParse(s.Value, out var num);
                return num;
            });
        }
        
        var path = EditorUtility.SaveFilePanel("Save As", LevelBuilder.DefaultFolder, LevelBuilder.DefaultName+(number+1), "asset");
        levelBuilder.Save(FileUtil.GetProjectRelativePath(path));
    }

    private void ImportStage()
    {
        var path = EditorUtility.OpenFilePanel("Open", StageBuilder.StageFolder, "asset");
        levelBuilder.ImportStage(FileUtil.GetProjectRelativePath(path));
    }
}
