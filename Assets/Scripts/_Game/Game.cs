using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class Game : MonoBehaviour
{
    public static LevelData AutoloadLevelData
    {
        get
        {
            var path = System.IO.Path.Combine(LevelBuilder.LevelFolder, "_autoload_.asset");
            return AssetDatabase.LoadAssetAtPath<LevelData>(path);
        }
        set
        {
            var path = System.IO.Path.Combine(LevelBuilder.LevelFolder, "_autoload_.asset");
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

    public LevelData DebugLevelData;

    private Level level;
    private GameObject playerObject;

    private void Start()
    {
        if (AutoloadLevelData != null)
        {
            LoadGameplay(AutoloadLevelData);
            AutoloadLevelData = null;
            return;
        }

        if (DebugLevelData != null)
        {
            LoadGameplay(DebugLevelData);
            return;
        }
    }

    private void LoadGameplay(LevelData levelData)
    {
        level = LevelLoader.LoadLevel(levelData);
        if (level == null) return;
        
        playerObject = PlayerLoader.LoadPlayerObject(level);
        playerObject.GetComponent<Player>().Level = level;
    }

}
