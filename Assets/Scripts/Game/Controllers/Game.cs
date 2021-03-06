using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using _Main.Game.Interfaces;
using Packages.Rider.Editor.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

public enum GameState
{
    Preplay,
    Playing,
    Win,
    Lose
}

public interface IGame : IBasicObject, SimpleStateMachine<GameState>
{
    TilePrefabPack Pack { get; }
    Transform FinishLine { get; }
    ICameraController CameraController { get; }
    ILevelLoader LevelLoader { get; }
    IPlayer Player { get; }

    void Play();
    void Restart();
}

public class Game : MonoBehaviour, IGame
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
    
    [Header("Debug")]
    [SerializeField] private LevelData debugLevelData;

    [Space]
    [Header("Game assets")]
    [SerializeField] private GameObject preplayUI;
    [SerializeField] private GameObject winWindow;
    [SerializeField] private GameObject loseWindow;
    [SerializeField] private TilePrefabPack pack;
    
    [Space]
    [Header("Game interface members")]
    [SerializeField] private Transform finishLine;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private Player player;

    public TilePrefabPack Pack => pack;
    public Transform FinishLine => finishLine;
    public ICameraController CameraController => cameraController;
    public ILevelLoader LevelLoader => levelLoader;
    public IPlayer Player => player;
    
    public GameState CurrentState { get; private set; }


    private void Awake()
    {
        Setup();
    }

    private void Start()
    {
        Play();
    }

    public void Setup()
    {
        CameraController.Setup();
        
        LevelLoader.Pack = Pack;
        LevelLoader.Setup();
        
        Player.TileStack.RoadPrefab = Pack.RoadPrefab;
        Player.OnInput.AddListener(DisablePreplayUI);
        Player.OnPlayerWin.AddListener(GameWin);
        Player.OnPlayerFell.AddListener(GameLost);
        Player.Setup();
        
        CurrentState = GameState.Preplay;
        preplayUI.SetActive(true);

        //Load player progression
    }
    
    public void CleanUp()
    {
        CameraController.CleanUp();
        LevelLoader.CleanUp();
        Player.CleanUp();

        AutoloadLevelData = null;
    }

    [ContextMenu("Reset Progression")]
    public void ResetProgression()
    {
        PlayerPrefs.SetInt(SaveDataKeys.CurrentLevel, 0);
    }

    public void Play()
    {
        Load();
        
        Player.Play();
    }

    private void Load()
    {
        var loaded = false;

#if UNITY_EDITOR
        if (AutoloadLevelData != null)
        {
            LevelLoader.LoadLevel(AutoloadLevelData);
            loaded = true;
        }
        else if (debugLevelData != null)
        {
            LevelLoader.LoadLevel(debugLevelData);
            loaded = true;
        }
#endif
        
        if (!loaded)
        {
            var levelNumber = PlayerPrefs.GetInt(SaveDataKeys.CurrentLevel);
            var levelDatas = Resources.LoadAll<LevelData>(ResourcePaths.LevelDataFolder);
            if (levelDatas.Length <= levelNumber)
            {
                Debug.LogError("LevelData not found");
                return;
            }
            
            LevelLoader.LoadLevel(levelDatas[levelNumber]);
        }
        
        //Place Player and FinishLine
        var level = LevelLoader.GetLevel();
        Player.Root.position = level.GetStartingLinePos();
        if (level.GetFinishLinePos() == null)
        {
            Debug.LogError("No empty space near last stage exit is found");
            return;
        }

        var finishLinePos = (Vector3) level.GetFinishLinePos();
        FinishLine.position = finishLinePos;
    }

    public void Restart()
    {
        CleanUp();
        Setup();
        Play();
    }

    private void OnApplicationQuit()
    {
        CleanUp();
    }

    private void GameWin()
    {
        Debug.Log("Win");
        CurrentState = GameState.Win;
        PlayerPrefs.SetInt(SaveDataKeys.CurrentLevel, PlayerPrefs.GetInt(SaveDataKeys.CurrentLevel)+1);
        winWindow.SetActive(true);
    }

    private void GameLost()
    {
        Debug.Log("Lose");
        CurrentState = GameState.Lose;
        loseWindow.SetActive(true);
    }
    
    private void DisablePreplayUI()
    {
        if (CurrentState != GameState.Preplay) return;

        CurrentState = GameState.Playing;
        preplayUI.SetActive(false);
    }

    public void UpdateSelectedAssets()
    {
        if (PlayerPrefs.GetString(SaveDataKeys.PlayerModelName) != Player.Root.name) Restart();
    }
}
