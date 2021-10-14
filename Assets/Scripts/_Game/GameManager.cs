using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static LevelData AutoloadLevelData
    {
        get
        {
            var path = Path.Combine(LevelBuilder.LevelFolder, "_autoload_.asset");
            return AssetDatabase.LoadAssetAtPath<LevelData>(path);
        }
        set
        {
            var path = Path.Combine(LevelBuilder.LevelFolder, "_autoload_.asset");
            if (value == null)
            {
                AssetDatabase.DeleteAsset(path);
                return;
            }
            
            var c = AutoloadLevelData;
            if (c == null)
            {
                var asset = ScriptableObject.CreateInstance<LevelData>();
                asset.CopyFrom(value);
                AssetDatabase.CreateAsset(asset, path);
            }
            else
            {
                c.CopyFrom(value);
            }

            AssetDatabase.SaveAssets();
        }
    }

    private Level level;
    private GameObject playerObject;
    private PlayerController playerController;
    private CameraController cameraController;

    private void Start()
    {
        if (AutoloadLevelData != null)
        {
            LoadGameplay(AutoloadLevelData);
            AutoloadLevelData = null;
        }
    }

    private void LoadGameplay(LevelData levelData)
    {
        level = new LevelLoader().LoadLevel(levelData);
        if (level == null) return;
        
        var levelGrid = level.gameObject.GetComponent<Grid>();
        playerObject = new PlayerLoader().LoadPlayerObject(levelGrid, levelData);
        playerController = new PlayerController(playerObject.transform, level);
        playerObject.AddComponent<CameraController>();
    }

}
