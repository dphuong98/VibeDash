using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Tilemaps;
using UnityEngine;
using System.Linq;
using UnityEngine.Windows;
using UnityEngine.WSA;
using Object = UnityEngine.Object;

public class LevelDesigner : EditorWindow
{
    private readonly Vector2 buttonSize = new Vector2(50,50);
    private string levelFolder = "Assets/Resources/ScriptableObjects/Levels";
    private Object selectedLevel;
    
    [MenuItem("Extra/LevelDesigner")]
    static void Init()
    {
        GetWindow(typeof(LevelDesigner));
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();

        // Level
        GUILayout.Label("Level");
        GUILayout.BeginHorizontal();
        selectedLevel = EditorGUILayout.ObjectField(selectedLevel, typeof(LevelScriptable), true);
        
        if (GUILayout.Button("New"))
        {
            NewLevel();
        }
        if (GUILayout.Button("Delete"))
        {
            DeleteLevel(selectedLevel as LevelScriptable);
        }
        GUILayout.EndHorizontal();

        if (selectedLevel == null) return;

        var level = selectedLevel as LevelScriptable;
        
        GUILayout.Label("Stages");
        
        
        level.stages.RemoveAll(item => item == null);
        for (var i = 0; i < level.stages.Count; i++)
        {
            level.stages[i] = (StageScriptable) EditorGUILayout.ObjectField(level.stages[i], typeof(StageScriptable), true);
        }

        Object addStage = null;
        addStage  = EditorGUILayout.ObjectField(addStage, typeof(StageScriptable), true);
        if (addStage != null) level.stages.Add(addStage as StageScriptable);
 
        GUILayout.EndVertical();
    }

    private void NewLevel()
    {
        var newLevelName = GetNewName();
        selectedLevel = ScriptableObject.CreateInstance<LevelScriptable>();
        AssetDatabase.CreateAsset(selectedLevel, levelFolder + "/" + newLevelName + ".asset");
        AssetDatabase.SaveAssets();
    }

    private void DeleteLevel(LevelScriptable level)
    {
        if (level == null) return;
        
        AssetDatabase.DeleteAsset(levelFolder + "/" + level.name + ".asset");
        AssetDatabase.SaveAssets();
    }

    private string GetNewName()
    {
        var number = 0;
        var prefix = "Level";
        while (File.Exists(levelFolder + "/" + prefix + number))
        {
            number++;
        }

        return prefix + number;
    }
}
