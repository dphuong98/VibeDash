using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class LevelLoader
{
    public static int tileLayerMask = LayerMask.NameToLayer("Tiles");

    public static GameObject FinishTriggerPrefab = Resources.Load<GameObject>("Prefabs/Game/FinishTrigger");
    public static TilePrefabPack PrefabPack = Resources.Load<TilePrefabPack>("Prefabs/StageTilePacks/DefaultPack/DefaultPack");

    public static Level LoadLevel(LevelData levelData)
    {
        if (levelData.StagePositions.Count == 0)
        {
            Debug.LogError("Level contains no stage"); return null;
        }

        var levelObject = new GameObject {name = "LevelObject"};
        var levelGrid = levelObject.AddComponent<Grid>();
        var levelComponent = levelObject.AddComponent<Level>();
        levelComponent.SetLevel(levelData, levelGrid);
        levelGrid.cellSwizzle = GridLayout.CellSwizzle.XZY;
        
        //Place stages
        foreach (var stage in levelData.StagePositions)
        {
            var stagePos = new Vector3(stage.Value.x, 0, stage.Value.y);
            LoadStage(levelGrid, stagePos, stage.Key);
        }

        //Place bridge
        foreach (var bridge in levelData.Bridges)
        {
            for (var i = 0; i < bridge.bridgeParts.Count; i++)
            {
                if (i == 0 || i == bridge.bridgeParts.Count - 1) continue;
                var part = bridge.bridgeParts[i];
                //TODO place rectangle bridge
                PlaceTile(levelGrid.GetCellCenterWorld(part), TileType.Bridge, levelObject.transform);
            }
        }
        
        //Place finish trigger
        var lastStagePosition = levelData.StagePositions.Last();
        var exitGridPos = lastStagePosition.Value + lastStagePosition.Key.GetExit().ToVector3Int();
        var emptyNeighbor = levelComponent.GetEmptyNeighbor(exitGridPos);
        if (emptyNeighbor == null)
        {
            Debug.LogError("There is no empty spot next to last stage's exit. Level loading is halted.");
        }

        Object.Instantiate(FinishTriggerPrefab, levelGrid.GetCellCenterWorld((Vector3Int) emptyNeighbor), Quaternion.identity, levelObject.transform);
        
        return levelComponent;
    }

    private static void LoadStage(Grid levelGrid, Vector3 position, StageData stageData)
    {
        for (var y = 0; y < stageData.Size.y; y++)
        {
            for (var x = 0; x < stageData.Size.x; x++)
            {
                var tile = stageData[x, y];
                var gridPos = new Vector2Int(x, y);

                if (stageData.TileDirections.TryGetValue(gridPos, out var direction))
                {
                    PlaceDirectionalTile(position + levelGrid.GetCellCenterWorld(gridPos), direction, tile, levelGrid.transform);
                    continue;
                }
                
                PlaceTile(position + levelGrid.GetCellCenterWorld(gridPos), tile, levelGrid.transform);
            }
        }
    }

    public static GameObject PlaceTile(Vector3 position, TileType tileType, Transform parentTransform)
    {
        switch (tileType)
        {
            case TileType.Wall:
                return SpawnTile(tileType, PrefabPack.WallPrefab, position, parentTransform);
            case TileType.Entrance: case TileType.Exit: case TileType.Road:
                return SpawnTile(TileType.Road, PrefabPack.RoadPrefab, position, parentTransform);
            case TileType.Stop:
                SpawnTile(TileType.Blank, PrefabPack.BlankPrefab, position, parentTransform);
                return SpawnTile(tileType, PrefabPack.StopPrefab, position, parentTransform);
            case TileType.PortalBlue:
                return SpawnTile(tileType, PrefabPack.PortalBluePrefab, position, parentTransform);
            case TileType.PortalOrange:
                return SpawnTile(tileType, PrefabPack.PortalOrangePrefab, position, parentTransform);
            case TileType.Blank:
                return SpawnTile(tileType, PrefabPack.BlankPrefab, position, parentTransform);
            case TileType.Bridge:
                return SpawnTile(tileType, PrefabPack.BridgePrefab, position, parentTransform);
        }

        return null;
    }

    private static GameObject SpawnTile(TileType tileType, GameObject prefab, Vector3 position, Transform parentTransform)
    {
        if (!prefab) return null;
        
        var tileObject = Object.Instantiate(prefab, position, Quaternion.identity, parentTransform);
        tileObject.AddComponent<Tile>().TileType = tileType;
        tileObject.layer = tileLayerMask;

        return tileObject;
    }

    private static GameObject PlaceDirectionalTile(Vector3 position, Vector2Int direction, TileType tileType, Transform parentTransform)
    {
        switch (tileType)
        {
            case TileType.Push:
                SpawnTile(TileType.Blank, PrefabPack.BlankPrefab, position, parentTransform);
                return SpawnDirectionalTile(tileType, PrefabPack.PushPrefab, position, direction, parentTransform);
            case TileType.Corner:
                SpawnTile(TileType.Road, PrefabPack.RoadPrefab, position, parentTransform);
                return SpawnDirectionalTile(tileType, PrefabPack.CornerPrefab, position, direction, 
                    parentTransform);
        }

        return null;
    }

    public static GameObject SpawnDirectionalTile(TileType tileType, GameObject prefab, Vector3 position, Vector2Int direction, Transform parentTransform)
    {
        if (!prefab) return null;
        
        var rotation = Quaternion.Euler(new Vector3
            {y = Vector3.Angle(Vector3.up, new Vector3(direction.x, direction.y, 0))});
        
        var tileObject = Object.Instantiate(prefab, position, rotation, parentTransform);
        tileObject.AddComponent<Tile>().TileType = tileType;
        tileObject.layer = tileLayerMask;

        return tileObject;
    }
}
