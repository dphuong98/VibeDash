
using UnityEngine;
public interface ILevelLoader: IBasicObject
{
    TilePrefabPack Pack { get; }
    Transform LevelRoot { get; }

    Level GetLevel();
    void LoadLevel(LevelData levelData); 
}

public class LevelLoader : MonoBehaviour, ILevelLoader
{
    [SerializeField] private TilePrefabPack pack;
    [SerializeField] private Transform levelRoot;

    public TilePrefabPack Pack => pack;
    public Transform LevelRoot => levelRoot;
    
    
    public void Setup()
    {
        // TODO: Load player prefs environment pack
        GetLevel().Setup();
    }

    public void CleanUp()
    {
        if (!LevelRoot) return;
        
        foreach (Transform child in LevelRoot)
            Destroy(child.gameObject);
    }

    public Level GetLevel()
    {
        return LevelRoot.GetComponent<Level>();
    }
    
    public void LoadLevel(LevelData levelData)
    {
        if (levelData.StagePositions.Count == 0)
        {
            Debug.LogError("Level contains no stage"); return;
        }
        
        GetLevel().SetLevelData(levelData);
        var levelGrid = LevelRoot.GetComponent<Grid>();
        
        //Place stages
        foreach (var stage in levelData.StagePositions)
        {
            var stagePos = new Vector3(stage.Value.x, 0, stage.Value.y);
            LoadStage(levelGrid, stagePos, stage.Key);
        }
        
        //Place bridges
        foreach (var bridge in levelData.Bridges)
        {
            for (var i = 0; i < bridge.bridgeParts.Count; i++)
            {
                if (i == 0 || i == bridge.bridgeParts.Count - 1) continue;
                var worldPos = levelGrid.GetCellCenterWorld(bridge.bridgeParts[i]);
                worldPos.y = 0;
                
                //TODO place directional bridge
                PlaceTile(worldPos, TileType.Bridge, LevelRoot);
            }
        }
    }

    private void LoadStage(Grid levelGrid, Vector3 position, StageData stageData)
    {
        for (var y = 0; y < stageData.Size.y; y++)
        {
            for (var x = 0; x < stageData.Size.x; x++)
            {
                var tile = stageData[x, y];
                var gridPos = new Vector2Int(x, y);
                var worldPos = position + levelGrid.GetCellCenterWorld(gridPos);
                worldPos.y = 0;
                
                if (stageData.TileDirections.TryGetValue(gridPos, out var direction))
                {
                    PlaceDirectionalTile(worldPos, direction, tile, LevelRoot);
                    continue;
                }
                
                PlaceTile(worldPos, tile, LevelRoot);
            }
        }
    }

    private void PlaceTile(Vector3 position, TileType tileType, Transform parentTransform)
    {
        GameObject prefab = null;
        
        switch (tileType)
        {
            case TileType.Wall:
                prefab = Pack.WallPrefab;
                break;
            case TileType.Entrance: case TileType.Exit: case TileType.Road:
                prefab = Pack.RoadPrefab;
                break;
            case TileType.Stop:
                prefab = Pack.StopPrefab;
                break;
            case TileType.PortalBlue:
                prefab = Pack.PortalBluePrefab;
                break;
            case TileType.PortalOrange:
                prefab = Pack.PortalOrangePrefab;
                break;
            case TileType.Blank:
                prefab = Pack.BlankPrefab;
                break;
            case TileType.Bridge:
                prefab = Pack.BridgePrefab;
                break;
        }
        
        if (!prefab) return;
        Instantiate(prefab, position, Quaternion.identity, parentTransform);
    }

    private void PlaceDirectionalTile(Vector3 position, Vector2Int direction, TileType tileType, Transform parentTransform)
    {
        GameObject prefab = null;
        
        switch (tileType)
        {
            case TileType.Push:
                prefab = Pack.PushPrefab;
                break;
            case TileType.Corner:
                prefab = Pack.CornerPrefab;
                break;
        }
        
        if (!prefab) return;
        
        var rotation = Quaternion.Euler(new Vector3
            {y = Vector3.Angle(Vector3.up, new Vector3(direction.x, direction.y, 0))});
        
        Instantiate(prefab, position, rotation, parentTransform);
    }
}
