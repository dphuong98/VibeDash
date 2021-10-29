﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using _Main.Game.Interfaces;
using Packages.Rider.Editor.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

public enum GameState
{
    Win,
    Lose
}

public interface IGame : IBasicObject, SimpleStateMachine<GameState>
{
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

    [SerializeField] private LevelData debugLevelData;

    [SerializeField] private Transform finishLine;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private Player player;

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

    private void Update()
    {
        DebugUI.Instance.AddText("GameState: " + CurrentState);
    }

    public void Setup()
    {
        CameraController.Setup();
        LevelLoader.Setup();
        Player.Setup();

        Player.OnPlayerWin.AddListener(GameWin);
        Player.OnPlayerFell.AddListener(GameLost);

        //Load player progression
    }
    
    public void CleanUp()
    {
        CameraController.CleanUp();
        LevelLoader.CleanUp();
        Player.CleanUp();

        AutoloadLevelData = null;
    }

    public void Play()
    {
        Load();
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
    }

    private void GameLost()
    {
        Debug.Log("Lose");
    }
}
