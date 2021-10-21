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
            switch (stageData[x, y])
            {
                case TileType.Blank:
                    stageData[x, y] = TileType.Wall;
                    break;
                case TileType.Wall:
                    stageData[x, y] = TileType.Stop;
                    break;
                case TileType.Stop:
                    stageData[x, y] = TileType.PortalBlue;
                    break;
                case TileType.PortalBlue:
                    stageData[x, y] = TileType.PortalOrange;
                    break;
                case TileType.PortalOrange:
                    stageData[x, y] = TileType.Push;
                    break;
                case TileType.Push:
                    stageData[x, y] = TileType.Corner;
                    break;
                case TileType.Corner:
                    stageData[x, y] = TileType.Blank;
                    break;
            }
        }
    }
}