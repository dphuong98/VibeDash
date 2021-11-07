
using System.Linq;
using PathCreation;
using UnityEngine;
public interface ILevelLoader: IBasicObject
{
    TilePrefabPack Pack { get; set; }
    Transform LevelRoot { get; }

    ILevel GetLevel();
    void LoadLevel(LevelData levelData); 
}

public class LevelLoader : MonoBehaviour, ILevelLoader
{
    [SerializeField] private Transform levelRoot;
    [SerializeField] private PathCreator pathCreator;
    [SerializeField] private GameObject bridgeEntryPrefab;

    private const float bridgeSpacing = 0.9f;
    
    public TilePrefabPack Pack { get; set; }
    public Transform LevelRoot => levelRoot;
    
    
    public void Setup()
    {
        // TODO: Load player prefs environment pack
        GetLevel().Setup();
    }

    public void CleanUp()
    {
        GetLevel().CleanUp();
    }

    public ILevel GetLevel()
    {
        return LevelRoot.GetComponent<ILevel>();
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
            var stageWorldPos = levelGrid.CellToWorld(stage.Value);
            LoadStage(levelGrid, stageWorldPos, stage.Key);
        }
        
        //Place bridges
        foreach (var bridge in levelData.Bridges)
        {
            var worldPos = levelGrid.GetCellCenterWorld(bridge.BridgeParts[1]);
            Instantiate(bridgeEntryPrefab, worldPos, Quaternion.identity, LevelRoot);
            
            worldPos = levelGrid.GetCellCenterWorld(bridge.BridgeParts[bridge.BridgeParts.Count - 2]);
            Instantiate(bridgeEntryPrefab, worldPos, Quaternion.identity, LevelRoot);
            
            var worldPath = bridge.BridgeParts.Select(s => levelGrid.GetCellCenterWorld(s)).ToList();
            var bezierPath = new BezierPath(worldPath);
            pathCreator.bezierPath = bezierPath;

            var bridgeDistance = bridgeSpacing;
            while (bridgeDistance < pathCreator.path.length)
            {
                PlaceBridgeModel(pathCreator.path.GetPointAtDistance(bridgeDistance, EndOfPathInstruction.Stop),
                    pathCreator.path.GetRotationAtDistance(bridgeDistance, EndOfPathInstruction.Stop),
                    LevelRoot);
                bridgeDistance += bridgeSpacing;
            }
        }
    }

    private void PlaceBridgeModel(Vector3 position, Quaternion rotation, Transform parentTransform)
    {
        position = new Vector3(position.x, parentTransform.position.y, position.z);
        rotation.eulerAngles = new Vector3(rotation.eulerAngles.x, rotation.eulerAngles.y, 0);
        Instantiate(Pack.BridgePrefab, position, rotation, parentTransform);
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
                worldPos.y = position.y;
                
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
