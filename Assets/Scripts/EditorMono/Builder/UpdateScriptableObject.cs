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
            var stage = AssetDatabase.LoadAssetAtPath<StageData>(assetPath);
            
            UpdateStage(ref stage);
            
            var asset = ScriptableObject.CreateInstance<StageData>();
            asset.Init();
            asset.CopyFrom(stage);
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
        }
    }

    private void UpdateStage(ref StageData stageData)
    {
        for (var x = 0; x < stageData.Size.x; x++)
        for (var y = 0; y < stageData.Size.y; y++)
        {
            if (stageData[x, y] == TileType.Air) stageData[x, y] = TileType.Entrance;
        }
    }
}