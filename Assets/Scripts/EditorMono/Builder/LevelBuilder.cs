using System;
using System.Collections.Generic;
using System.Linq;
using PathCreation;
using UnityEditor;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

[ExecuteInEditMode]
public class LevelBuilder : Builder<LevelData>
{
    //Path
    private const string levelFolder = "Assets/Resources/Data/Levels";
    public static string LevelFolder => levelFolder;

    private GameObject miniStagePrefab;

    public LevelData LoadedLevelData => LoadedItem;
    public LevelData EditingLevelData => EditingItem;

    //Members
    private int maxBridgeLength;
    private BridgeBuilder bridgeBuilder;
    private Grid levelGrid;
    private Bridge editingBridge;
    
    private List<MiniStage> miniStages = new List<MiniStage>();
    private List<Bridge> bridges = new List<Bridge>();
    
    private void OnEnable()
    {
        levelGrid = GetComponentInChildren<Grid>();
        SceneView.duringSceneGui += DrawSceneGUI;
        Init();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DrawSceneGUI;
    }

    private void Init()
    {
        miniStagePrefab = Resources.Load<GameObject>("Prefabs/Editor/MiniStage");

        if (miniStages == null) miniStages = new List<MiniStage>();
        if (bridges == null) bridges = new List<Bridge>();

        bridgeBuilder = GetComponentInChildren<BridgeBuilder>(true);
        base.Init(levelFolder);
    }
    
    private void DrawSceneGUI(SceneView sceneView)
    {
        if (EditingLevelData == null) return;

        DrawFocusButton();
        HandleClick(sceneView);

        foreach (var miniStage in miniStages)
        {
            StageRenderer.SetStage(miniStage.StageData, levelGrid, levelGrid.WorldToCell(miniStage.transform.position));
            StageRenderer.DrawTileIcons();
        }

        //Render and handle bridge connections
        DrawBridge();
        DrawBridgeBuilder();

        //Other GUI option
    }

    private void Update()
    {
        ApplyChanges();
    }

    private void DrawFocusButton()
    {
        Handles.BeginGUI();

        if (HandlesExt.DrawButton(10, 400, 100, 50, "LevelBuilder"))
        {
            UnityEditorWindowHelper.GetWindow(WindowType.Inspector);
            Selection.activeGameObject = gameObject;
            SceneView.lastActiveSceneView.rotation = Quaternion.Euler(0,0,0);
            SceneView.FrameLastActiveSceneView();
            SceneView.lastActiveSceneView.orthographic = true;
        }

        Handles.EndGUI();
    }

    public override void NewItem()
    {
        WipeScene();

        base.NewItem();
    }
    
    public override bool Open(string path)
    {
        WipeScene();
        
        if (!base.Open(path)) return false;

        PopulateScene();
        
        return true;
    }

    public override void Reload()
    {
        WipeScene();
        base.Reload();
        PopulateScene();
    }

    private void WipeScene()
    {
        var miniStageFolder = transform.FindInChildren("MiniStages");

        if (miniStageFolder == null)
        {
            miniStageFolder = new GameObject("MiniStages").transform;
            miniStageFolder.parent = transform;
        }
        
        for (var i = miniStageFolder.childCount - 1; i >= 0; i--)
            DestroyImmediate(miniStageFolder.GetChild(i).gameObject);
        
        miniStages = new List<MiniStage>();
        bridges = new List<Bridge>();
    }

    private void PopulateScene()
    {
        foreach (var stage in EditingLevelData.StagePositions)
        {
            var position = stage.Value;
            //TODO remove redundant get asset path
            ImportStage(AssetDatabase.GetAssetPath(stage.Key), position);
        }

        bridges = new List<Bridge>(EditingLevelData.Bridges);
    }

    private void ApplyChanges()
    {
        if (EditingLevelData == null) return;
        
        miniStages.RemoveAll(s => s == null);

        var stageData =
            miniStages.ToDictionary(s => s.StageData, s => GetGridPosition(s.transform.position));

        for (var i = bridges.Count - 1; i >= 0; i--)
        {
            var bridgeStart = bridges[i].BridgeParts.First();
            if (GetTileType(GetMiniStage(bridgeStart), bridgeStart) != TileType.Exit)
            {
                bridges.RemoveAt(i);
                return;
            }

            var bridgeEnd = bridges[i].BridgeParts.Last();
            if (GetTileType(GetMiniStage(bridgeEnd), bridgeEnd) != TileType.Entrance)
            {
                bridges.RemoveAt(i);
                return;
            }

        }
        
        EditingLevelData.Import(stageData, bridges);
    }

    public void ImportStage(string path, Vector3Int gridPos = default)
    {
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            var stage = AssetDatabase.LoadAssetAtPath<StageData>(path);
            if (stage == null)
            {
                Debug.LogErrorFormat("Cannot load {0} asset at {1}", "Stage", path);
                return;
            }

            if (stage.IsEntranceStage() && miniStages.Exists(s => s.StageData.IsEntranceStage()))
            {
                Debug.LogError("There can only be one entrance stage where entrance tile is in the middle of the stage.");
                return;
            }

            if (miniStages.Any(s => s.StageData == stage))
            {
                Debug.LogError("A level cannot contain the same stage twice");
                return;
            }

            if (gridPos == default && miniStages.Any())
            {
                var highestStageTop = miniStages.Max(s => s.transform.position.y + s.GetComponent<MeshFilter>().sharedMesh.bounds.size.y);
                gridPos = new Vector3Int { y = (int)Math.Round(highestStageTop + 2) };
            }

            var worldPos = levelGrid.CellToWorld(gridPos);
            var miniStageObject = Instantiate(miniStagePrefab, worldPos, Quaternion.identity, transform.FindInChildren("MiniStages"));
            var miniStage = miniStageObject.GetComponentInChildren<MiniStage>();
            miniStage.SetStage(stage);
            miniStageObject.name = System.IO.Path.GetFileNameWithoutExtension(path);
            miniStages.Add(miniStage);
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("Exception when open asset {0} {1} {2}", path, ex.Message, ex.StackTrace);
        }
    }

    private void HandleClick(SceneView sceneView)
    {
        if (Event.current.type != EventType.MouseDown ||
            Event.current.modifiers != EventModifiers.None)
            return;
        
        //Handle left mouse click
        if (Event.current.button == 0)
        {
            var mousePos = sceneView.SceneViewToWorld();
            var mouseGridPos = GetGridPosition(mousePos);

            var miniStage = GetMiniStage(mouseGridPos);
            var gridTileType = GetTileType(miniStage, mouseGridPos);
            
            //Start bridge building
            if (gridTileType == TileType.Exit)
            {
                var nearestNeighbor = GetEmptyNeighbor(mouseGridPos);
                if (nearestNeighbor != null)
                {
                    maxBridgeLength = Pathfinding.CountUniqueTiles(miniStage.StageData.Solution);
                    editingBridge = new Bridge(new List<Vector3Int>{mouseGridPos, (Vector3Int)nearestNeighbor});
                    StartBridgeBuilding(editingBridge.BridgeParts);
                }
                return;
            }

            //Finish building bridge
            if (IsBuildingBridge() &&
                gridTileType == TileType.Entrance &&
                bridgeBuilder.GetMaxTile() <= maxBridgeLength)
            {
                var nearestNeighbor = GetEmptyNeighbor(mouseGridPos);
                if (nearestNeighbor != null)
                {
                    editingBridge.BridgeParts.Add((Vector3Int)nearestNeighbor);
                    editingBridge.BridgeParts.Add(mouseGridPos);
                    bridges.Add(editingBridge);
                    StopBridgeBuilding();
                }
                return;
            }

            if (IsBuildingBridge())
            {
                //Cancel bridge building if clicked on stage
                if (miniStage != null)
                {
                    StopBridgeBuilding();
                }
                else
                {
                    editingBridge.BridgeParts.Add(mouseGridPos);
                    UpdateBridge(editingBridge.BridgeParts);
                }
            }
        }

        //Handle right mouse click
        if (Event.current.button == 1)
        {
            if (editingBridge == null) return;
            
            if (editingBridge.BridgeParts.Count > 2)
            {
                editingBridge.BridgeParts.RemoveAt(editingBridge.BridgeParts.Count - 1);
                UpdateBridge(editingBridge.BridgeParts);
                return;
            }
            
            StopBridgeBuilding();
        }
    }

    private void StartBridgeBuilding(List<Vector3Int> bridgeParts)
    {
        bridgeBuilder.gameObject.SetActive(true);
        UpdateBridge(bridgeParts);
    }

    private void UpdateBridge(List<Vector3Int> bridgeParts)
    {
        bridgeBuilder.SetBridge(bridgeParts.Select(s => levelGrid.GetCellCenterWorld(s)));
        Debug.Log(bridgeBuilder.GetMaxTile());
    }

    private void StopBridgeBuilding()
    {
        editingBridge = null;
        bridgeBuilder.gameObject.SetActive(false);
    }

    private void DrawBridge()
    {
        // Handles.color = Color.yellow;
        // foreach (var bridge in bridges)
        // {
        //     for (int i = 0; i < bridge.bridgeParts.Count - 1; i++)
        //     {
        //         Handles.DrawLine(GetWorldPosition(bridge.bridgeParts[i]), GetWorldPosition(bridge.bridgeParts[i+1]));
        //     }
        // }
    }
    
    private void DrawBridgeBuilder()
    {
        if (!IsBuildingBridge())
        {
            //Render exit blinking
            return;
        }
        
        //Render entrance for bridge end
        
        var remainingBridgeLength = maxBridgeLength - bridgeBuilder.GetMaxTile();
        HandlesExt.DrawText(GetWorldPosition(editingBridge.BridgeParts.Last()), "" + remainingBridgeLength, 150, Color.white);
        
        //Render editing bridge
        bridgeBuilder.SetBezierColor(remainingBridgeLength < 0 ? Color.red : Color.green);
    }

    private TileType GetTileType(MiniStage miniStage, Vector3Int gridPos)
    {
        if (miniStage == null) return TileType.Air;
        
        var miniStageGridPos = GetGridPosition(miniStage.transform.position);
        var relativeGridPos = gridPos - miniStageGridPos;
        if (relativeGridPos.x < 0 || relativeGridPos.y < 0) return TileType.Air;
        
        return miniStage.StageData[relativeGridPos.x, relativeGridPos.y];
    }
    
    private MiniStage GetMiniStage(Vector3Int gridPos)
    {
        foreach (var miniStage in miniStages)
        {
            var miniStageGridPos = GetGridPosition(miniStage.transform.position);
            var relativeGridPos = gridPos - miniStageGridPos;
            if (miniStage.StageData.IsValidTile(relativeGridPos.ToVector2Int())) return miniStage;
        }

        return null;
    }

    private bool IsBuildingBridge()
    {
        return editingBridge != null &&
               editingBridge.BridgeParts != null &&
               editingBridge.BridgeParts.Count > 0;
    }

    private Vector3Int? GetEmptyNeighbor(Vector3Int gridPos)
    {
        var scoutDirection = Vector3Int.up;
        do
        {
            var scoutGridPos = gridPos + scoutDirection;
            if (GetTileType(GetMiniStage(scoutGridPos), scoutGridPos) == TileType.Air) return gridPos + scoutDirection;
            scoutDirection = scoutDirection.RotateClockwise();
        } while (scoutDirection != Vector3Int.up);

        return null;
    }
    
    private Vector3Int GetGridPosition(Vector3 worldPos)
    {
        return levelGrid.WorldToCell(worldPos);
    }

    private Vector3 GetWorldPosition(Vector3Int gridPos)
    {
        return levelGrid.GetCellCenterWorld(gridPos);
    }
}
