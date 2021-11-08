
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
    
    Vector3 GetStartingLinePos();
    Vector3? GetFinishLinePos();
    
    ITile GetTile(Vector3Int gridPos);
    ITile GetBridgeTile(Vector3 worldPos);
    Bridge  GetBridge(Vector3Int gridPos);
    Vector3Int GetTileDirection(Vector3Int gridPos);
    Vector3Int? GetOtherPortal(Vector3Int gridPos);
    
    void SetLevelData(LevelData levelData);
}

public class Level : MonoBehaviour, ILevel
{
    [SerializeField] private LayerMask tileLayerMask;
    [SerializeField] private Grid levelGrid;

    public LevelData LevelData { get; private set; }
    public Grid LevelGrid => levelGrid;
    
    private List<Portal> portals;
    private TileDirection tileDirections;
    
    
    public void Setup()
    {
        portals = new List<Portal>();
        tileDirections = new TileDirection();
    }

    public void CleanUp()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }

    public void SetLevelData(LevelData levelData)
    {
        LevelData = levelData;
        
        //Merge TileDirections, Portals
        foreach (var stagePos in LevelData.StagePositions)
        {
            foreach (var portal in stagePos.Key.PortalPairs)
            {
                portals.Add(new Portal(portal.Blue + stagePos.Value.ToVector2Int(), portal.Orange + stagePos.Value.ToVector2Int()));
            }
        }

        foreach (var stagePos in LevelData.StagePositions)
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
        //TODO raycast all, its not broken atm so dont fix
        var position = levelGrid.GetCellCenterWorld(gridPos);
        
        if (Physics.Raycast(position + Vector3.up, Vector3.down, out var hitInfo, Mathf.Infinity, tileLayerMask))
        {
            return hitInfo.transform.GetComponent<ITile>();
        }

        return null;
    }

    public ITile GetBridgeTile(Vector3 worldPos)
    {
        var raycastHits = Physics.RaycastAll(worldPos + Vector3.up, Vector3.down);

        var tileHit = raycastHits.Where(s => s.transform.GetComponent<ITile>() is var tile &&
                                             tile != null && tile.TileType == TileType.Blank);
        return tileHit.Any() ? tileHit.First().transform.GetComponent<ITile>() : null;
    }

    public Bridge GetBridge(Vector3Int gridPos)
    {
        var bridge = LevelData.Bridges.FirstOrDefault(s => s.BridgeParts[1] == gridPos);
        if (bridge != null) return new Bridge(bridge);
        
        bridge = LevelData.Bridges.FirstOrDefault(s => s.BridgeParts[s.BridgeParts.Count - 2] == gridPos);
        if (bridge != null) return bridge.ReverseBridge();

        return null;
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
