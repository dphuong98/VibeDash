using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public interface IGame : IBasicObject
{
    Transform FinishLine { get; }
    IInputController InputController { get; }
    ILevelLoader LevelLoader { get; }
    IPlayer Player { get; }
    
    UnityEvent OnWin { get; }
    UnityEvent OnLose { get; }
    
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

    public LevelData DebugLevelData;

    [SerializeField] private Transform finishLine;
    [SerializeField] private InputController inputController;
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private Player player;

    public Transform FinishLine { get; private set; }
    public IInputController InputController { get; private set; }
    public ILevelLoader LevelLoader { get; private set; }
    public IPlayer Player { get; private set; }
    
    public UnityEvent OnWin { get; }
    public UnityEvent OnLose { get; }

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
        FinishLine = finishLine;
        LevelLoader = levelLoader;
        InputController = inputController;
        Player = player;

        LevelLoader.Setup();
        InputController.Setup();
        Player.Setup();
        
        //Load player progression
    }
    
    public void CleanUp()
    {
        LevelLoader.CleanUp();
        InputController.CleanUp();
        Player.CleanUp();

        AutoloadLevelData = null;
    }

    public void Play()
    {
        Load();

        //Set state or sth
    }

    private void Load()
    {
        //TODO: Add player progression current level
        if (AutoloadLevelData != null)
            LevelLoader.LoadLevel(AutoloadLevelData);
        else if (DebugLevelData != null)
            LevelLoader.LoadLevel(DebugLevelData);
        else return;
        
        //Place Player and FinishLine
        var level = LevelLoader.GetLevel();
        Player.Root.position = level.GetStartingLinePos();
        var finishLinePos = level.GetFinishLinePos();
        if (finishLinePos == null)
        {
            Debug.LogError("No empty space near last stage exit is found");
            return;
        }
        FinishLine.position = (Vector3) finishLinePos;
    }

    public void Restart()
    {
        //throw new NotImplementedException();
    }

    private void OnApplicationQuit()
    {
        CleanUp();
    }
}
