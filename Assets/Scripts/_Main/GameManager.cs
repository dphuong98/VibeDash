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
    private CameraController cameraController;

    private void Start()
    {
        if (autoloadLevelData)
        {
            LoadGameplay(autoloadLevelData);
        }
    }

    private void LoadGameplay(LevelData levelData)
    {
        level = new LevelLoader().LoadLevel(levelData);
        var levelGrid = level.GetComponent<Grid>();
        playerObject = new PlayerLoader().LoadPlayerObject(levelGrid, levelData);
        playerController = new PlayerController(playerObject.transform, level);
        playerObject.AddComponent<CameraController>();
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
