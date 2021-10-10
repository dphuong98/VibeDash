using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

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

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        EditorSceneManager.OpenScene(StageBuilderScenePath);
    }
    
    [MenuItem("VibeDash/StageBuilder", true)]
    private static bool OpenStageEditorCondition()
    {
        return EditorSceneManager.GetActiveScene().path != StageBuilderScenePath;
    }
    
    [MenuItem("VibeDash/LevelBuilder")]
    private static void OpenLevelEditor()
    {
        if (Application.isPlaying)
        {
            return;
        }

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        EditorSceneManager.OpenScene(LevelBuilderScenePath);
    }
    
    [MenuItem("VibeDash/LevelBuilder", true)]
    private static bool OpenLevelEditorCondition()
    {
        return EditorSceneManager.GetActiveScene().path != LevelBuilderScenePath;
    }
    
    [MenuItem("VibeDash/Gameplay")]
    private static void OpenGameplay()
    {
        if (Application.isPlaying)
        {
            return;
        }

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        EditorSceneManager.OpenScene(GameplayScenePath);
    }
    
    [MenuItem("VibeDash/Gameplay", true)]
    private static bool OpenGameplayCondition()
    {
        return EditorSceneManager.GetActiveScene().path != GameplayScenePath;
    }
}
