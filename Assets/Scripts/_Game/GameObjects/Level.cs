﻿
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Level : MonoBehaviour
{
    private TilePrefabPack prefabPack;
    
    private LevelData levelData;
    private List<Portal> portals = new List<Portal>();
    private TileDirection tileDirections = new TileDirection();
    
    public Grid LevelGrid { get; private set; }

    public LevelData LevelData => levelData;
    public List<Portal> PortalPairs => new List<Portal>(portals);
    public TileDirection TileDirections => new TileDirection(tileDirections);
    
    public void SetLevel(LevelData levelData, Grid levelGrid, TilePrefabPack prefabPack)
    {
        this.levelData = levelData;
        this.LevelGrid = levelGrid;
        this.prefabPack = prefabPack;
        
        //Combine portal and tile direction data from all stages
        foreach (var stagePos in levelData.StagePositions)
        {
            foreach (var portal in stagePos.Key.PortalPairs)
            {
                portals.Add(new Portal(portal.Blue + stagePos.Value.ToVector2Int(), portal.Orange + stagePos.Value.ToVector2Int()));
            }
        }

        foreach (var stagePos in levelData.StagePositions)
        {
            foreach (var tileDirection in stagePos.Key.TileDirections)
            {
                var gridPos = tileDirection.Key + stagePos.Value.ToVector2Int();
                tileDirections.Add(gridPos, tileDirection.Value);
            }
        }
    }
    
    public TileType GetTileType(Vector3Int gridPos)
    {
        var position = LevelGrid.GetCellCenterWorld(gridPos);

        if (Physics.Raycast(position + Vector3.up, Vector3.down, out var hitInfo, Mathf.Infinity,
                1 << LevelLoader.tileLayerMask) &&
            hitInfo.transform.GetComponent<Tile>() is var tileComponent &&
            tileComponent != null)
        {
            return tileComponent.TileType;
        }

        return TileType.Air;
    }

    public Bridge GetBridge(Vector3Int gridPos)
    {
        var frontBridge = levelData.Bridges.Where(s => s.bridgeParts[1] == gridPos);
        if (frontBridge.Any()) return frontBridge.First();

        var backBridge = levelData.Bridges.Where(s => s.bridgeParts[s.bridgeParts.Count - 2] == gridPos);
        if (backBridge.Any()) return backBridge.First().ReverseBridge();
        
        return null;
    }
}
