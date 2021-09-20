using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

[ExecuteInEditMode]
public class LevelBuilder : Builder<Level>
{
    //Path
    private const string levelFolder = "Assets/Resources/Data/Levels";

    private MiniStage miniStagePrefab;

    public Level LoadedLevel => LoadedItem;
    public Level EditingLevel => EditingItem;

    public List<MiniStage> stages;
    
    private void OnEnable()
    {
        SceneView.duringSceneGui += DrawSceneGUI;
        Init();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DrawSceneGUI;
    }

    private void Init()
    {
        miniStagePrefab = Resources.Load<MiniStage>("Prefabs/Editor/MiniStage");
        
        base.Init(levelFolder);
    }
    
    private void DrawSceneGUI(SceneView sceneview)
    {
        if (EditingLevel == null) return;

        //Render and handle bridge connections

        //Other GUI option
    }

    private void Update()
    {
        stages.RemoveAll(s => s == null);
    }

    public void ImportStage()
    {
        //TODO Refactor this into file loader class
        var path = EditorUtility.OpenFilePanel("Open", StageBuilder.StageFolder, "asset");
        if (string.IsNullOrEmpty(path)) return;
        
        try
        {
            var stage = AssetDatabase.LoadAssetAtPath<Stage>(UnityEditor.FileUtil.GetProjectRelativePath(path));
            if (stage == null)
            {
                Debug.LogErrorFormat("Cannot load {0} asset at {1}", "Stage", path);
                return;
            }
            
            var miniStage = Instantiate(miniStagePrefab, Vector3.zero, Quaternion.identity, transform);
            miniStage.SetStage(stage);
            miniStage.name = Path.GetFileNameWithoutExtension(path);
            stages.Add(miniStage);
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("Exception when open asset {0} {1} {2}", path, ex.Message, ex.StackTrace);
        }
    }

    protected override void OnReload()
    {
        
    }
}
