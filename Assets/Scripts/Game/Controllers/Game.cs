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
    ICameraController CameraController { get; }
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

    [SerializeField] private LevelData debugLevelData;

    [SerializeField] private Transform finishLine;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private Player player;

    public Transform FinishLine => finishLine;
    public ICameraController CameraController => cameraController;
    public ILevelLoader LevelLoader => levelLoader;
    public IPlayer Player => player;
    
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
        LevelLoader.Setup();
        Player.Setup();
        
        //Load player progression
    }
    
    public void CleanUp()
    {
        LevelLoader.CleanUp();
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
        else if (debugLevelData != null)
            LevelLoader.LoadLevel(debugLevelData);
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
