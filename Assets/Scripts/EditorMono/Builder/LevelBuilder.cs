using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

[ExecuteInEditMode]
public class LevelBuilder : Builder<Level>
{
    //Path
    private const string levelFolder = "Assets/Resources/Data/Levels";
    public static string LevelFolder => levelFolder;
    
    private StageBuilder stageBuilderPrefab;

    //public Stage SelectedStage;
    
    public Level LoadedLevel { get; private set; }
    public Level EditingLevel { get; private set; }
    
    private void OnEnable()
    {
        SceneView.duringSceneGui += DrawSceneGUI;
        Init();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DrawSceneGUI;
    }

    public void Init()
    {
        base.Init(levelFolder);
        if (EditingLevel == null) NewItem();
        stageBuilderPrefab = Resources.Load<StageBuilder>("Prefabs/Editor/StageBuilder");
    }
    
    private void DrawSceneGUI(SceneView sceneview)
    {
        if (EditingLevel == null) return;

        //Render and handle bridge connections
        
        //Other GUI option
    }

    public void NewStage()
    {
        var newStageBuilder = Instantiate(stageBuilderPrefab, new Vector3(0, 0, 0), Quaternion.identity, transform);
        if (!newStageBuilder.SaveAs())
        {
            DestroyImmediate(newStageBuilder.gameObject);
        }
    }

    public void ImportStage()
    {
        var newStageBuilder = Instantiate(stageBuilderPrefab, new Vector3(0, 0, 0), Quaternion.identity, transform);
        if (!newStageBuilder.Open())
        {
            DestroyImmediate(newStageBuilder.gameObject);
        }
    }

    protected override void OnReload()
    {
        
    }
}
