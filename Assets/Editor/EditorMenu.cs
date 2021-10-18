using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EditorMenu
{
    private const string StageBuilderScenePath = "Assets/Scenes/StageBuilder.unity";
    private const string LevelBuilderScenePath = "Assets/Scenes/LevelBuilder.unity";
    private const string GameplayScenePath = "Assets/Scenes/Gameplay.unity";

    [MenuItem("VibeDash/StageBuilder")]
    private static void OpenStageEditor()
    {
        if (Application.isPlaying)
        {
            return;
        }
        
        //EditorApplication.ExecuteMenuItem("Tools/ProGrids/Close ProGrids");
        EditorSceneManager.OpenScene(StageBuilderScenePath);
        //EditorApplication.ExecuteMenuItem("Tools/ProGrids/ProGrids Window");
    }
    
    [MenuItem("VibeDash/StageBuilder", true)]
    private static bool OpenStageEditorCondition()
    {
        return SceneManager.GetActiveScene().path != StageBuilderScenePath;
    }
    
    [MenuItem("VibeDash/LevelBuilder")]
    private static void OpenLevelEditor()
    {
        if (Application.isPlaying)
        {
            return;
        }
        
        //EditorApplication.ExecuteMenuItem("Tools/ProGrids/Close ProGrids");
        EditorSceneManager.OpenScene(LevelBuilderScenePath);
        //EditorApplication.ExecuteMenuItem("Tools/ProGrids/ProGrids Window");
    }
    
    [MenuItem("VibeDash/LevelBuilder", true)]
    private static bool OpenLevelEditorCondition()
    {
        return SceneManager.GetActiveScene().path != LevelBuilderScenePath;
    }
    
    [MenuItem("VibeDash/Gameplay")]
    private static void OpenGameplay()
    {
        if (Application.isPlaying)
        {
            return;
        }

        //EditorApplication.ExecuteMenuItem("Tools/ProGrids/Close ProGrids");
        EditorSceneManager.OpenScene(GameplayScenePath);
        //EditorApplication.ExecuteMenuItem("Tools/ProGrids/ProGrids Window");
    }
    
    [MenuItem("VibeDash/Gameplay", true)]
    private static bool OpenGameplayCondition()
    {
        return SceneManager.GetActiveScene().path != GameplayScenePath;
    }
}
