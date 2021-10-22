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

    private Level levelComponent;
    private GameObject finishTriggerObject;
    private GameObject playerObject;

    private void Start()
    {
        ExecuteLoadCommands();
    }

    private void Update()
    {
        var stateMachine = GetComponent<Animator>();
        var text = "GameState: " + (stateMachine.GetBool("FinishTriggered") ? "Win" :
                                     (stateMachine.GetBool("PlayerFellOffMap") ? "Lose" : "Playing"))
                                 + "\n";
        DebugUI.Instance.AddText(text);
    }

    private void ExecuteLoadCommands()
    {
        if (AutoloadLevelData != null)
        {
            LoadGameplay(AutoloadLevelData);
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
        levelComponent = LevelLoader.LoadLevel(levelData);
        if (levelComponent == null) return;
        
        finishTriggerObject = MiscLoader.LoadFinishTrigger(levelComponent);
        playerObject = MiscLoader.LoadPlayerObject(levelComponent);
    }

    public void Restart()
    {
        UnloadGameplay();
        ExecuteLoadCommands();
    }

    private void UnloadGameplay()
    {
        if (playerObject) Destroy(playerObject);
        if (finishTriggerObject) Destroy(finishTriggerObject);
        
        if (levelComponent) Destroy(levelComponent.gameObject);
    }

    public bool PlayerTriggeredFinish()
    {
        return finishTriggerObject.GetComponent<PlayerDetector>().PlayerEntered;
    }

    public bool PlayerFellOffMap()
    {
        var playerGridPos = levelComponent.LevelGrid.WorldToCell(playerObject.transform.position);
        return levelComponent.GetTileType(playerGridPos) == TileType.Air;
    }

    private void OnApplicationQuit()
    {
        AutoloadLevelData = null;
    }
}
