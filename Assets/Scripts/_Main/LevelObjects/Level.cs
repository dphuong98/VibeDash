
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public interface ILevel: IBasicObject
{
    LevelData LevelData { get; }

    ITile GetTile(Vector3Int gridPos);
    void SetLevelData(LevelData levelData);
}

public class Level : MonoBehaviour, ILevel
{
    [SerializeField] private Grid levelGrid;
    [SerializeField] private LayerMask tileLayerMask;
    
    public LevelData LevelData { get; private set; }

    public ITile GetTile(Vector3Int gridPos)
    {
        var position = levelGrid.GetCellCenterWorld(gridPos);

        if (Physics.Raycast(position + Vector3.up, Vector3.down, out var hitInfo,
                1 << tileLayerMask))
        {
            return hitInfo.transform.GetComponent<ITile>();
        }

        return null;
    }

    public void SetLevelData(LevelData levelData)
    {
        LevelData = levelData;
        
        //Merge TileDirections, Portals
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
            if (GetTile(gridPos + scoutDirection) == null)
            {
                return gridPos + scoutDirection;
            }
            scoutDirection.RotateClockwise();
        } while (scoutDirection != Vector3Int.up);

        return null;
    }

    public void Setup()
    {
        
    }

    public void CleanUp()
    {
        
    }
}
