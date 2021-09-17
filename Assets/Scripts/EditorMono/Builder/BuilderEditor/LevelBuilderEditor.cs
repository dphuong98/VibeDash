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

    private StageBuilder stageBuilderPrefab;
    
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
                    levelBuilder.NewItem();
                }
                
                if (GUILayout.Button("Open"))
                {
                    levelBuilder.Open();
                }

                if (GUILayout.Button("Save"))
                {
                    levelBuilder.Save();
                }

                if (GUILayout.Button("Save As"))
                {
                    levelBuilder.SaveAs();
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

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("NewStage"))
            {
                levelBuilder.NewStage();
            }
            
            if (GUILayout.Button("ImportStage"))
            {
                
            }
            
            if (GUILayout.Button("RemoveStage"))
            {
                
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
}
