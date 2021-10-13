
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Level : MonoBehaviour
{
    private LevelData levelData;
    private Grid levelGrid;
    private List<Portal> portals = new List<Portal>();
    private TileDirection tileDirections = new TileDirection();

    public Grid LevelGrid => levelGrid;
    public List<Portal> Portals => new List<Portal>(portals);
    
    public void SetLevel(LevelData levelData, Grid levelGrid)
    {
        this.levelData = levelData;
        this.levelGrid = levelGrid;
        foreach (var stageData in levelData.StagePositions.Keys)
        {
            if (stageData.PortalPairs.Count != 0) portals.AddRange(stageData.PortalPairs);
        }

        foreach (var tileDirection in levelData.StagePositions.Keys.SelectMany(stageData => stageData.TileDirections))
        {
            tileDirections.AddUnique(tileDirection.Key, tileDirection.Value);
        }
    }
    
    public TileType GetTileType(Vector3Int gridPos)
    {
        var position = levelGrid.GetCellCenterWorld(gridPos);

        if (Physics.Raycast(position + Vector3.up, Vector3.down, out var hitInfo, Mathf.Infinity) &&
            hitInfo.transform.GetComponent<Tile>() is var tileComponent &&
            tileComponent != null)
        {
            return tileComponent.TileType;
        }

        return TileType.Air;
    }
}
