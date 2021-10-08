using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;

public class UpdateScriptableObject : MonoBehaviour
{
    [ContextMenu("UpdateAll")]
    private void UpdateAll()
    {
        var stagePaths = Directory.GetFiles(Application.dataPath + "/Resources/Data/Stages", "*.asset", SearchOption.AllDirectories);
        foreach(string stagePath in stagePaths)
        {
            string assetPath = FileUtil.GetProjectRelativePath(stagePath);
            var stage = AssetDatabase.LoadAssetAtPath<Stage>(assetPath);
            stage.GenerateSolution();
            
            var asset = ScriptableObject.CreateInstance<Stage>();
            asset.Init();
            asset.CopyFrom(stage);
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
        }
    }
}