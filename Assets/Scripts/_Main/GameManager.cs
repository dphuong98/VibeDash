using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    [FormerlySerializedAs("DebugLevel")] public LevelData debugLevelData;
    private static LevelData autoloadLevelData;

    private Level level;
    private GameObject playerObject;
    private PlayerController playerController;

    private void Start()
    {
        if (autoloadLevelData)
        {
            level = new LevelLoader().LoadLevel(autoloadLevelData);
            var levelGrid = level.GetComponent<Grid>();
            playerObject = new PlayerLoader().LoadPlayerObject(levelGrid, autoloadLevelData);
            playerController = new PlayerController(playerObject.transform, level);
        }
    }

    public static void SetAutoloadLevel(LevelData levelData)
    {
        autoloadLevelData = levelData;
    }

    [ContextMenu("Load Debug")]
    public void LoadDebug()
    {
        autoloadLevelData = debugLevelData;
        EditorApplication.isPlaying = true;
    }
}
