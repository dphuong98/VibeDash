using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class LevelBuilder : MonoBehaviour
{
    //Path
    private const string levelFolder = "Assets/Resources/Data/Levels";
    public static string LevelFolder => levelFolder;

    public Level LoadedLevel { get; private set; }
    public Level EditingLevel { get; private set; }
    
    private void OnEnable()
    {
        SceneView.duringSceneGui += DrawSceneGUI;

        if (EditingLevel == null)
        {
            NewLevel();
            //CreateVisualization();
        }
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DrawSceneGUI;
    }
    
    private void DrawSceneGUI(SceneView sceneview)
    {
        if (EditingLevel == null) return;

        //Render and handle bridge connections
        
        //Other GUI option
    }
    
    public void NewLevel()
    {
        LoadedLevel = null;
        EditingLevel = Level.CreateLevel();
        AssetDatabase.CreateAsset(EditingLevel, Path.Combine(levelFolder, "_tmp_.asset"));
        AssetDatabase.SaveAssets();
        //CreateVisualization();
    }
    
    public bool Open(string path)
    {
        try
        {
            var asset = AssetDatabase.LoadAssetAtPath<Level>(path);
            if (asset == null)
            {
                Debug.LogErrorFormat("Cannot load level asset at {0}", path);
                return false;
            }
            LoadedLevel = asset;
            EditingLevel.CopyFrom(asset);
            //CreateVisualization();
            
            gameObject.name = Path.GetFileNameWithoutExtension(path);
            Debug.LogFormat("Opened level from {0}", path);
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("Exception when open asset {0} {1} {2}", path, ex.Message, ex.StackTrace);
        }

        return true;
    }
    
    public void Save()
    {
        if (LoadedLevel == null)
        {
            Debug.LogError("Loaded Stage is NULL");
            return;
        }

        LoadedLevel.CopyFrom(EditingLevel);

        EditorUtility.SetDirty(LoadedLevel);
        EditorUtility.SetDirty(EditingLevel);
        AssetDatabase.SaveAssets();
        
        Debug.LogFormat("Saved level to {0}", LoadedLevel);
    }
    
    public void SaveAs(string path)
    {
        try
        {
            var asset = Level.CreateLevel();
            asset.CopyFrom(EditingLevel);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            LoadedLevel = asset;

            gameObject.name = Path.GetFileNameWithoutExtension(path);
            Debug.LogFormat("Saved level to {0}", path);
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("{0} {1}", ex.Message, ex.StackTrace);
        }
    }
    
    public void Reload()
    {
        if (LoadedLevel != null)
        {
            EditingLevel.CopyFrom(LoadedLevel);
            EditorUtility.SetDirty(EditingLevel);
            //CreateVisualization();
            Debug.Log("Reloaded level");
        }
    }
}
