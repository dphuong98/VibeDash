using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.TerrainAPI;
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
    private static Vector2Int lastestBridgePart;
    private static Bridge editingBridge;
    
    private List<MiniStage> miniStages = new List<MiniStage>();
    private List<Bridge> bridges = new List<Bridge>();
    
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
        miniStagePrefab = Resources.Load<GameObject>("Prefabs/Editor/MiniStage");
        
        if (miniStages == null) miniStages = new List<MiniStage>();
        if (bridges == null) bridges = new List<Bridge>();
        
        base.Init(levelFolder);
    }
    
    private void DrawSceneGUI(SceneView sceneView)
    {
        if (EditingLevel == null) return;

        DrawBuilderFocusButton();
        HandleClick(sceneView);

        //Render and handle bridge connections
        DrawBridge();
        DrawBridgeBuilder();
        HandleBridgeBuilding(sceneView);

        //Other GUI option
    }

    private void Update()
    {
        ApplyChanges();
    }

    private void DrawBuilderFocusButton()
    {
        Handles.BeginGUI();
             
        var rect = new Rect(10, 400, 100, 50);
        if (GUI.Button(rect, "Level Builder"))
        {
            Selection.activeGameObject = gameObject;
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
        for (var i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
        
        miniStages = new List<MiniStage>();
        bridges = new List<Bridge>();
    }

    private void PopulateScene()
    {
        foreach (var stage in EditingLevel.StagePositions)
        {
            var position = stage.Key;
            ImportStage(AssetDatabase.GetAssetPath(stage.Value), position);
        }

        bridges = new List<Bridge>(EditingLevel.Bridges);
    }

    private void ApplyChanges()
    {
        miniStages.RemoveAll(s => s == null);
        
        var stageData =
            miniStages.ToDictionary(s => s.GetPosition(), s => s.Stage);
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
        
        //Start bridge building
        if (Event.current.button == 0)
        {
            if (RaycastMiniStage(out var miniStage) &&
                miniStage.TileSelected(out var gridPos))
            {
                var stage = miniStage.Stage;
                var mousePos = sceneView.SceneViewToWorld();
                
                if (0 <= gridPos.x && gridPos.x < stage.Size.x && 0 <= gridPos.y && gridPos.y < stage.Size.y)
                {
                    if (stage[gridPos.x, gridPos.y] == TileType.Exit)
                    {
                        editingBridge = new Bridge(Pathfinding.CountUniqueTiles(miniStage.Stage.Solution));
                        var mouseGridPos3 = grid.WorldToCell(mousePos);
                        editingBridge.bridgeParts.Add(new Vector2Int(mouseGridPos3.x, mouseGridPos3.y));
                        return;
                    }
                    
                    if (editingBridge != null && editingBridge.bridgeParts.Count > editingBridge.MaxLength)
                    {
                        editingBridge = null;
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
        if (editingBridge == null)
        {
            //Render exit blinking
            
            return;
        }
        
        if (editingBridge.bridgeParts.Count > editingBridge.MaxLength)
            Handles.color = Color.red;
        else Handles.color = Color.green;
        
        //Render entrance for bridge end
        

        //Render editing bridge
        for (int i = 0; i < editingBridge.bridgeParts.Count - 1; i++)
        {
            Handles.DrawLine(grid.GetCellCenterWorld(editingBridge.bridgeParts[i]), (grid.GetCellCenterWorld(editingBridge.bridgeParts[i+1])));
        }
    }

    private void DrawBridge()
    {
        Handles.color = Color.yellow;
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
