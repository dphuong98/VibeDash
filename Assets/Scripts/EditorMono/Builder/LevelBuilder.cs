using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

[ExecuteInEditMode]
public class LevelBuilder : Builder<Level>
{
    //Path
    private const string levelFolder = "Assets/Resources/Data/Levels";

    private GameObject miniStagePrefab;

    public Level LoadedLevel => LoadedItem;
    public Level EditingLevel => EditingItem;

    //Members
    private static Grid grid;
    private static Vector2Int lastestBridgePart;
    private static Bridge editingBridge;
    
    [SerializeField]
    private List<MiniStage> miniStages;
    [SerializeField]
    private List<Bridge> bridges;
    
    private void OnEnable()
    {
        SceneView.duringSceneGui += DrawSceneGUI;
        Init();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DrawSceneGUI;
    }

    private void Init()
    {
        grid = GetComponent<Grid>();
        miniStagePrefab = Resources.Load<GameObject>("Prefabs/Editor/MiniStage");
        
        miniStages = new List<MiniStage>();
        bridges = new List<Bridge>();
        
        base.Init(levelFolder);
    }
    
    private void DrawSceneGUI(SceneView sceneView)
    {
        if (EditingLevel == null) return;

        HandleClick(sceneView);

        //Render and handle bridge connections
        DrawBridge();
        DrawBridgeBuilder();
        HandleBridgeBuilding(sceneView);

        //Other GUI option
    }

    public override void NewItem()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
        
        miniStages = new List<MiniStage>();
        bridges = new List<Bridge>();

        base.NewItem();
    }
    
    public override bool Open(string path)
    {
        NewItem();
        
        if (!base.Open(path)) return false;

        foreach (var stage in EditingLevel.Stages)
        {
            var gridPos = new Vector3Int(stage.Value.x, stage.Value.y, 0);
            var position = grid.CellToWorld(gridPos);
            ImportStage(AssetDatabase.GetAssetPath(stage.Key), position);
        }

        bridges = new List<Bridge>(EditingLevel.Bridges);
        
        return true;
    }

    public override bool Save()
    {
        ApplyChanges();
        return base.Save();
    }

    public override bool Save(string path)
    {
        ApplyChanges();
        return base.Save(path);
    }

    private void ApplyChanges()
    {
        miniStages.RemoveAll(s => s == null);
        
        var stageData =
            miniStages.ToDictionary(s => s.Stage, s => grid.WorldToCell(s.GetPosition()).ToVector2Int());
        EditingLevel.Import(stageData, bridges);
    }

    public void ImportStage(string path, Vector3 position = default)
    {
        if (string.IsNullOrEmpty(path)) return;
        
        try
        {
            var stage = AssetDatabase.LoadAssetAtPath<Stage>(path);
            if (stage == null)
            {
                Debug.LogErrorFormat("Cannot load {0} asset at {1}", "Stage", path);
                return;
            }

            if (miniStages.Any(s => s.Stage == stage))
            {
                Debug.LogError("A level cannot contain the same stage twice");
                return;
            }
            
            var miniStageObject = Instantiate(miniStagePrefab, position, Quaternion.identity, transform);
            var miniStage = miniStageObject.GetComponentInChildren<MiniStage>();
            miniStage.SetStage(stage);
            miniStageObject.name = Path.GetFileNameWithoutExtension(path);
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
                
        if (RaycastMiniStage(out var miniStage) &&
            miniStage.TileSelected(out var gridPos))
        {
            var stage = miniStage.Stage;
            var mousePos = sceneView.SceneViewToWorld();

            //Left mouse clicked
            if (Event.current.button == 0)
            {
                if (0 <= gridPos.x && gridPos.x < stage.Size.x && 0 <= gridPos.y && gridPos.y < stage.Size.y)
                {
                    if (editingBridge == null && stage[gridPos.x, gridPos.y] == TileType.Exit)
                    {
                        editingBridge = new Bridge(Pathfinding.GetMaximumUniqueTile(miniStage.Stage));
                        var mouseGridPos3 = grid.WorldToCell(mousePos);
                        editingBridge.bridgeParts.Add(new Vector2Int(mouseGridPos3.x, mouseGridPos3.y));
                        return;
                    }
            
                    if (editingBridge != null && stage[gridPos.x, gridPos.y] == TileType.Entrance)
                    {
                        //End bridge and append to scriptable
                        var mouseGridPos3 = grid.WorldToCell(mousePos);
                        editingBridge.bridgeParts.Add(new Vector2Int(mouseGridPos3.x, mouseGridPos3.y));
                    
                        bridges.Add(editingBridge);
                        editingBridge = null;
                        return;
                    }
                }
            }

            if (Event.current.button == 1)
            {
                
            }
        }

        if (Event.current.button == 0)
        {
            editingBridge = null;
        }
    }

    private bool RaycastMiniStage(out MiniStage miniStage)
    {
        var worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        if (Physics.Raycast(worldRay, out var hitInfo))
        {
            if (hitInfo.collider.gameObject.GetComponent<MiniStage>() != null)
            {
                miniStage = hitInfo.collider.gameObject.GetComponent<MiniStage>();
                return true;
            }
        }

        miniStage = null;
        return false;
    }

    private void DrawBridgeBuilder()
    {
        if (editingBridge == null) return;

        //Render current bridge
        for (int i = 0; i < editingBridge.bridgeParts.Count - 1; i++)
        {
            Handles.DrawLine(grid.GetCellCenterWorld(editingBridge.bridgeParts[i]), (grid.GetCellCenterWorld(editingBridge.bridgeParts[i+1])));
        }
    }

    private void DrawBridge()
    {
        foreach (var bridge in bridges)
        {
            for (int i = 0; i < bridge.bridgeParts.Count - 1; i++)
            {
                Handles.DrawLine(grid.GetCellCenterWorld(bridge.bridgeParts[i]), (grid.GetCellCenterWorld(bridge.bridgeParts[i+1])));
            }
        }
    }

    private void HandleBridgeBuilding(SceneView sceneView)
    {
        //Get current grid tile relative to bridge start
        if (editingBridge == null) return;

        var mousePos = sceneView.SceneViewToWorld();
        var mouseGridPos3 = grid.WorldToCell(mousePos);
        var mouseGridPos = new Vector2Int(mouseGridPos3.x, mouseGridPos3.y);

        if (editingBridge.bridgeParts.Count >= 2 && mouseGridPos == editingBridge.bridgeParts[editingBridge.bridgeParts.Count - 2])
        {
            //Delete if backtrack
            editingBridge.bridgeParts.RemoveAt(editingBridge.bridgeParts.Count - 1);
            return;
        }
        
        if (mouseGridPos != editingBridge.bridgeParts.Last()) editingBridge.bridgeParts.Add(mouseGridPos);
    }
}
