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
    
    public List<MiniStage> stages;
    
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
        
        base.Init(levelFolder);
    }
    
    private void DrawSceneGUI(SceneView sceneView)
    {
        if (EditingLevel == null) return;

        HandleClick(sceneView);

        //Render and handle bridge connections
        RenderBridgeBuilder();
        HandleBridgeBuilding(sceneView);

        //Other GUI option
    }

    private void Update()
    {
        stages.RemoveAll(s => s == null);
    }

    public void ImportStage()
    {
        //TODO Refactor this into file loader class
        var path = EditorUtility.OpenFilePanel("Open", StageBuilder.StageFolder, "asset");
        if (string.IsNullOrEmpty(path)) return;
        
        try
        {
            var stage = AssetDatabase.LoadAssetAtPath<Stage>(UnityEditor.FileUtil.GetProjectRelativePath(path));
            if (stage == null)
            {
                Debug.LogErrorFormat("Cannot load {0} asset at {1}", "Stage", path);
                return;
            }
            
            var miniStage = Instantiate(miniStagePrefab, Vector3.zero, Quaternion.identity, transform);
            miniStage.GetComponentInChildren<MiniStage>().SetStage(stage);
            miniStage.name = Path.GetFileNameWithoutExtension(path);
            stages.Add(miniStage.GetComponentInChildren<MiniStage>());
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("Exception when open asset {0} {1} {2}", path, ex.Message, ex.StackTrace);
        }
    }

    private void HandleClick(SceneView sceneView)
    {
        if (Event.current.type != EventType.MouseDown ||
            Event.current.modifiers != EventModifiers.None ||
            Event.current.button != 0) return;
        
        //Function that return any ministage it hits
        //Function that check grid hit on that stage
        if (RaycastMiniStage(out var miniStage) &&
            miniStage.TileSelected(out var gridPos))
        {
            var stage = miniStage.Stage;
            var mousePos = sceneView.SceneViewToWorld();

            if (0 <= gridPos.x && gridPos.x < stage.Size.x && 0 <= gridPos.y && gridPos.y < stage.Size.y)
            {
                if (editingBridge == null && stage[gridPos.x, gridPos.y] == TileType.Exit)
                {
                    editingBridge = new Bridge(Pathfinding.GetMaximumUniqueTile(miniStage.Stage));
                    var mouseGridPos3 = grid.WorldToCell(mousePos);
                    editingBridge.bridgeParts.Add(new Vector2Int(mouseGridPos3.x, mouseGridPos3.y));
                    Event.current.Use();
                    return;
                }
            
                if (editingBridge != null && stage[gridPos.x, gridPos.y] == TileType.Entrance)
                {
                    //End bridge and append to scriptable
                    var mouseGridPos3 = grid.WorldToCell(mousePos);
                    editingBridge.bridgeParts.Add(new Vector2Int(mouseGridPos3.x, mouseGridPos3.y));
                    
                    
                    Event.current.Use();
                    return;
                }
            }
        }
        
        editingBridge = null;
        Event.current.Use();
        
        //Click on objectives
        
        //Click on non-objectives
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

    private void RenderBridgeBuilder()
    {
        if (editingBridge == null) return;

        //Render current bridge
        for (int i = 0; i < editingBridge.bridgeParts.Count - 1; i++)
        {
            Handles.DrawLine(grid.GetCellCenterWorld(editingBridge.bridgeParts[i]), (grid.GetCellCenterWorld(editingBridge.bridgeParts[i+1])));
        }
    }

    private void HandleBridgeBuilding(SceneView sceneView)
    {
        //Get current grid tile relative to bridge start
        if (editingBridge == null) return;
        
        Debug.Log("Bridge building: " + (editingBridge != null));
        Debug.Log("Bridge length: " + (editingBridge?.bridgeParts.Count));
        
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

    protected override void OnReload()
    {
        
    }
}
