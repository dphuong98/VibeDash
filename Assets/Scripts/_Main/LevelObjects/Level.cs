
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

    public LevelData LevelData => levelData;
    public List<Portal> PortalPairs => new List<Portal>(portals);
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
    
    /// <summary>
    /// Get special tile
    /// </summary>
    /// <param name="gridPos"></param>
    /// <returns></returns>
    public TileType GetTileType(Vector3Int gridPos)
    {
        var tileType = TileType.Air;
        var position = LevelGrid.GetCellCenterWorld(gridPos);

        var tileTypes = Physics.RaycastAll(position + Vector3.up, Vector3.down,
            1 << LevelLoader.tileLayerMask)
            .Where(s => s.transform.GetComponent<Tile>() != null)
            .Select(s => s.transform.GetComponent<Tile>().TileType);
        
        foreach (var type in tileTypes)
        {
            if ((int)type > (int)tileType) tileType = type;
        }
        return tileType;
    }

    public bool HasTile(Vector3Int gridPos, TileType tileType)
    {
        var position = LevelGrid.GetCellCenterWorld(gridPos);

        var tileTypes = Physics.RaycastAll(position + Vector3.up, Vector3.down,
                1 << LevelLoader.tileLayerMask)
            .Where(s => s.transform.GetComponent<Tile>() != null)
            .Select(s => s.transform.GetComponent<Tile>().TileType);
        return tileTypes.Contains(tileType);
    }
    
    public Bridge GetBridge(Vector3Int gridPos, Vector3Int direction)
    {
        //TODO support overlapping of bridges
        var bridge = levelData.Bridges.FirstOrDefault(s => s.bridgeParts.Contains(gridPos));
        if (bridge != null)
        {
            var currentIndex = bridge.bridgeParts.IndexOf(gridPos);
            var previousGridPos = bridge.bridgeParts[currentIndex - 1];
            var previousDirection = gridPos - previousGridPos;
            if (previousDirection == direction)
            {
                var newBridge = bridge.bridgeParts.GetRange(currentIndex, bridge.bridgeParts.Count - currentIndex);
                return new Bridge(0, newBridge);
            }
            else
            {
                //Reverse bridge
                var newBridge = bridge.bridgeParts.GetRange(0, currentIndex);
                newBridge.Reverse();
                return new Bridge(0, newBridge);
            }
        }

        return null;
    }

    public Vector3Int? GetEmptyNeighbor(Vector3Int gridPos)
    {
        var scoutDirection = Vector3Int.up;
        do
        {
            if (GetTileType(gridPos + scoutDirection) == TileType.Air)
            {
                return gridPos + scoutDirection;
            }
            scoutDirection.RotateClockwise();
        } while (scoutDirection != Vector3Int.up);

        return null;
    }
}
