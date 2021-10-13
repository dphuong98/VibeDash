using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class LevelLoader
{
    private LayerMask tileLayerMask;
    public TilePrefabPack PrefabPack;

    public LevelLoader()
    {
        PrefabPack = Resources.Load<TilePrefabPack>("Prefabs/StageTilePacks/DefaultPack/DefaultPack");
        tileLayerMask = LayerMask.NameToLayer("Tiles");
    }
    
    public Level LoadLevel(LevelData levelData)
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
        
        foreach (var stage in levelData.StagePositions)
        {
            var stagePos = new Vector3(stage.Value.x, 0, stage.Value.y);
            LoadStage(levelGrid, stagePos, stage.Key);
        }

        return levelComponent;
    }

    private void LoadStage(Grid levelGrid, Vector3 position, StageData stageData)
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

    private void PlaceTile(Vector3 position, TileType tile, Transform parentTransform)
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
            case TileType.Blank:
                prefab = PrefabPack.BlankPrefab;
                break;
            default: case TileType.Air:
                break;
        }

        if (prefab)
        {
            var tileObject = Object.Instantiate(prefab, position, Quaternion.identity, parentTransform);
            tileObject.layer = tileLayerMask;
            tileObject.AddComponent<Tile>().TileType = tile;
        }
    }

    private void PlaceDirectionalTile(Vector3 position, Vector2Int direction, TileType tile, Transform parentTransform)
    {
        GameObject prefab = null;
        
        switch (tile)
        {
            case TileType.Push:
                prefab = PrefabPack.PushPrefab;
                break;
            case TileType.Corner:
                prefab = PrefabPack.CornerPrefab;
                break;
        }

        var rotation = Quaternion.Euler(new Vector3
            {y = Vector3.Angle(Vector3.up, new Vector3(direction.x, direction.y, 0))});
        if (prefab)
        {
            var tileObject = Object.Instantiate(prefab, position, rotation, parentTransform);
            tileObject.layer = tileLayerMask;
            tileObject.AddComponent<Tile>().TileType = tile;
        }
    }
}
