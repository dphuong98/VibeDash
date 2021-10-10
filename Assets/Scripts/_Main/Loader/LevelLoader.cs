﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private static Level AutoloadLevel;
    [SerializeField] private Level DebugLoadLevel;
    
    [SerializeField] private TilePrefabPack PrefabPack;
    [SerializeField] private Transform levelObject;

    private Grid levelGrid;

    private void Awake()
    {
        levelGrid = levelObject.gameObject.AddComponent<Grid>();
        levelGrid.cellSwizzle = GridLayout.CellSwizzle.XZY;
    }

    private void Start()
    {
        if (AutoloadLevel) LoadLevel(AutoloadLevel);
    }

    [ContextMenu("LoadDebug")]
    public void LoadLevel()
    {
        foreach (var stage in DebugLoadLevel.StagePositions)
        {
            var stagePos = new Vector3(stage.Value.x, 0, stage.Value.y);
            LoadStage(stagePos, stage.Key);
        }
    }

    public static void SetAutoloadLevel(Level level)
    {
        AutoloadLevel = level;
    }

    public void LoadLevel(Level level)
    {
        foreach (var stage in level.StagePositions)
        {
            var stagePos = new Vector3(stage.Value.x, 0, stage.Value.y);
            LoadStage(stagePos, stage.Key);
        }
    }

    private void LoadStage(Vector3 position, Stage stage)
    {
        for (var y = 0; y < stage.Size.y; y++)
        {
            for (var x = 0; x < stage.Size.x; x++)
            {
                var tile = stage[x, y];
                var gridPos = new Vector2Int(x, y);

                if (stage.TileDirections.TryGetValue(gridPos, out var direction))
                {
                    continue;
                }
                
                PlaceTile(position + levelGrid.GetCellCenterWorld(gridPos), tile);
            }
        }
    }

    private void PlaceTile(Vector3 position, TileType tile)
    {
        GameObject prefab = null;
        
        switch (tile)
        {
            case TileType.Wall:
                prefab = PrefabPack.WallPrefab;
                break;
            case TileType.Entrance: case TileType.Exit: case TileType.Road:
                prefab = PrefabPack.RoadPrefab;
                break;
            case TileType.Stop:
                prefab = PrefabPack.StopPrefab;
                break;
            case TileType.PortalBlue:
                prefab = PrefabPack.PortalBluePrefab;
                break;
            case TileType.PortalOrange:
                prefab = PrefabPack.PortalOrangePrefab;
                break;
            default: case TileType.Air:
                break;
        }
        
        if (prefab) Instantiate(prefab, position, Quaternion.identity, levelObject);
    }

    private void PlaceDirectionalTile(Vector3 position, Vector2Int direction, TileType tile)
    {
        switch (tile)
        {
            case TileType.Push:
                break;
            case TileType.Corner:
                break;
        }
    }
}
