
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public interface ILevel: IBasicObject
{
    LevelData LevelData { get; }
    Grid LevelGrid { get; }
    
    ITile GetTile(Vector3Int gridPos);
    Bridge GetBridge(Vector3Int gridPos, Vector3Int direction);
    Vector3Int GetTileDirection(Vector3Int gridPos);
    Vector3Int? GetOtherPortal(Vector3Int gridPos);
    void SetLevelData(LevelData levelData);
}

public class Level : MonoBehaviour, ILevel
{
    [SerializeField] private Grid levelGrid;
    [SerializeField] private LayerMask tileLayerMask;
    
    public LevelData LevelData { get; private set; }
    public Grid LevelGrid { get; private set; }

    private List<Portal> portals;
    private TileDirection tileDirections;
    
    
    public void Setup()
    {
        LevelGrid = levelGrid;

        portals = new List<Portal>();
        tileDirections = new TileDirection();
    }

    public void CleanUp()
    {
        
    }

    public void SetLevelData(LevelData levelData)
    {
        LevelData = levelData;
        
        //Merge TileDirections, Portals
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
    
    public ITile GetTile(Vector3Int gridPos)
    {
        var position = levelGrid.GetCellCenterWorld(gridPos);

        if (Physics.Raycast(position + Vector3.up, Vector3.down, out var hitInfo, tileLayerMask))
        {
            return hitInfo.transform.GetComponent<ITile>();
        }

        return null;
    }

    public Bridge GetBridge(Vector3Int gridPos, Vector3Int direction)
    {
        //TODO support overlapping of bridges
        var bridge = LevelData.Bridges.FirstOrDefault(s => s.bridgeParts.Contains(gridPos));
        if (bridge == null) return null;
        
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
            var newBridge = bridge.bridgeParts.GetRange(0, currentIndex + 1);
            newBridge.Reverse();
            return new Bridge(0, newBridge);
        }
    }

    public Vector3Int GetTileDirection(Vector3Int gridPos)
    {
        return tileDirections[gridPos.ToVector2Int()].ToVector3Int();
    }
    
    public Vector3Int? GetOtherPortal(Vector3Int gridPos)
    {
        var portalBlue = portals.Where(s => s.Blue == gridPos.ToVector2Int());
        if (portalBlue.Any()) return portalBlue.First().Orange.ToVector3Int();
        
        var portalOrange = portals.Where(s => s.Orange == gridPos.ToVector2Int());
        if (portalOrange.Any()) return portalOrange.First().Blue.ToVector3Int();

        return null;
    }

    public Vector3 GetStartingLinePos()
    {
        var entranceStage = LevelData.StagePositions.First();
        var firstEntranceGridPos = entranceStage.Key.GetEntrance();
        return levelGrid.GetCellCenterWorld(entranceStage.Value + firstEntranceGridPos.ToVector3Int());
    }

    public Vector3? GetFinishLinePos()
    {
        var lastStagePosition = LevelData.StagePositions.Last();
        var exitGridPos = lastStagePosition.Value + lastStagePosition.Key.GetExit().ToVector3Int();
        var emptyNeighbor = GetEmptyNeighbor(exitGridPos);
        if (emptyNeighbor != null) return levelGrid.GetCellCenterWorld((Vector3Int) emptyNeighbor);
        return null;
    }

    private Vector3Int? GetEmptyNeighbor(Vector3Int gridPos)
    {
        var scoutDirection = Vector3Int.up;
        do
        {
            if (GetTile(gridPos + scoutDirection) == null) return gridPos + scoutDirection;
            scoutDirection = scoutDirection.RotateClockwise();
        } while (scoutDirection != Vector3Int.up);

        return null;
    }
}
