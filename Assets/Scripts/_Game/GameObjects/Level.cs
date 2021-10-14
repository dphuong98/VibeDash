
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Level : MonoBehaviour
{
    private LevelData levelData;
    private List<Portal> portals = new List<Portal>();
    private TileDirection tileDirections = new TileDirection();

    public Grid LevelGrid { get; private set; }

    public List<Portal> Portals => new List<Portal>(portals);
    public TileDirection TileDirections => new TileDirection(tileDirections);
    
    public void SetLevel(LevelData levelData, Grid levelGrid)
    {
        this.levelData = levelData;
        this.LevelGrid = levelGrid;
        
        //Combine portal and tile direction data from all stages
        foreach (var stagePos in levelData.StagePositions)
        {
            foreach (var portal in stagePos.Key.PortalPairs)
            {
                portals.Add(new Portal(portal.Blue + stagePos.Value, portal.Orange + stagePos.Value));
            }
        }

        foreach (var stagePos in levelData.StagePositions)
        {
            foreach (var tileDirection in stagePos.Key.TileDirections)
            {
                var gridPos = tileDirection.Key + stagePos.Value;
                tileDirections.Add(gridPos, tileDirection.Value);
            }
        }
    }
    
    public TileType GetTileType(Vector3Int gridPos)
    {
        var position = LevelGrid.GetCellCenterWorld(gridPos);

        if (Physics.Raycast(position + Vector3.up, Vector3.down, out var hitInfo, Mathf.Infinity) &&
            hitInfo.transform.GetComponent<Tile>() is var tileComponent &&
            tileComponent != null)
        {
            return tileComponent.TileType;
        }

        return TileType.Air;
    }
}
