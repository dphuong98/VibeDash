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

public class StageDesigner : EditorWindow
{
    private Vector2Int stageSize;
    private Object selectedStage;


    private string stageFolder = "Assets/Resources/ScriptableObjects/Stages";
    private Vector2 labelSize = new Vector2(100f, 0);

    [MenuItem("Extra/StageDesigner")]
    static void Init()
    {
        GetWindow(typeof(StageDesigner));
    }

    private void OnGUI()
    {
        GUILayout.Label("Stage");
        GUILayout.BeginVertical();
        
        GUILayout.BeginHorizontal();
        selectedStage = EditorGUILayout.ObjectField(selectedStage, typeof(StageScriptable), true);
        
        if (GUILayout.Button("New"))
        {
            NewStage();
        }
        if (GUILayout.Button("Delete"))
        {
            DeleteStage(selectedStage as StageScriptable);
        }
        GUILayout.EndHorizontal();

        if (selectedStage == null) return;

        var stage = selectedStage as StageScriptable;
        GUILayout.BeginHorizontal();
        GUILayout.Label("Entrance: ", GUILayout.Width(labelSize.x));
        stage.Entrance.x = EditorGUILayout.IntField(stage.Entrance.x);
        GUILayout.Label(" x ");
        stage.Entrance.y = EditorGUILayout.IntField(stage.Entrance.y);
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Exit: ", GUILayout.Width(labelSize.x));
        stage.Exit.x = EditorGUILayout.IntField(stage.Exit.x);
        GUILayout.Label(" x ");
        stage.Exit.y = EditorGUILayout.IntField(stage.Exit.y);
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("StageSize: ", GUILayout.Width(labelSize.x));
        stageSize.x = EditorGUILayout.IntField(stage.StageSize.x);
        GUILayout.Label(" x ");
        stageSize.y = EditorGUILayout.IntField(stage.StageSize.y);
        GUILayout.EndHorizontal();

        if (StageSizeChanged(stage, stageSize))
            stage.StageSize = stageSize;
        
        for (var y = 0; y < stage.StageSize.y; y++)
        {
            GUILayout.BeginHorizontal();
            for (var x = 0; x < stage.StageSize.x; x++)
            {
                stage.tileData[x,y] = (TileType)EditorGUILayout.EnumPopup(stage.tileData[x,y]);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }

    private void DeleteStage(StageScriptable stage)
    {
        if (stage == null) return;
        
        AssetDatabase.DeleteAsset(stageFolder + "/" + stage.name + ".asset");
        AssetDatabase.SaveAssets();
        //TODO Add last stage when deleted
    }

    private void NewStage()
    {
        var newStageName = GetNewName();
        selectedStage = ScriptableObject.CreateInstance<StageScriptable>();
        AssetDatabase.CreateAsset(selectedStage, stageFolder + "/" + newStageName + ".asset");
        AssetDatabase.SaveAssets();
    }

    private string GetNewName()
    {
        var number = 0;
        var prefix = "Stage";
        while (File.Exists(stageFolder + "/" + prefix + number + ".asset"))
        {
            number++;
        }

        return prefix + number;
    }
    private bool StageSizeChanged(StageScriptable lastStage, Vector2Int currentStageSize)
    {
        return lastStage.tileData.GetLength(0) != currentStageSize.x || lastStage.tileData.GetLength(1) != currentStageSize.y;
    }
}
