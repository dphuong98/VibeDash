using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class MiniStage : MonoBehaviour
{
    public int maxPoints;
    public StageData StageData { get; private set; }
    private Grid grid;
    private MeshFilter meshFilter;

    public Vector3 GetNearestCellCenter(Vector3 position)
    {
        return grid.GetCellCenterWorld(grid.WorldToCell(position));
    }

    public void SetStage(StageData stageData)
    {
        StageData = stageData;
        maxPoints = Pathfinding.CountUniqueTiles(stageData.Solution);
        CreateBackgroundMesh();
    }

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
        grid = GetComponentInChildren<Grid>();
        meshFilter = GetComponent<MeshFilter>();
    }

    private void DrawSceneGUI(SceneView sceneview)
    {
        if (StageData == null) return;
        
        StageRenderer.SetStage(StageData, grid);
        StageRenderer.DrawTileIcons();
        
        DrawMaxPoints();
        HandleClick();
    }

    private void DrawMaxPoints()
    {
        var topLeft = transform.position + new Vector3(-meshFilter.sharedMesh.bounds.size.x / 2, meshFilter.sharedMesh.bounds.size.y / 2, 0);
        HandlesExt.DrawText(topLeft, "MaxPoints: " + maxPoints, 150);
    }
    
    private void HandleClick()
    {
        if (Selection.activeGameObject == this.gameObject &&
            Event.current.type == EventType.MouseDown &&
            Event.current.modifiers == EventModifiers.None)
        {
            if (Event.current.button == 1)
            {
                SceneView.RepaintAll();
                TileMenu().ShowAsContext();
            }
        }

    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
    
    public bool TileSelected(out Vector2Int gridPos)
    {
        var worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        if (Physics.Raycast(worldRay, out var hitInfo))
        {
            if (hitInfo.collider.gameObject == this.gameObject)
            {
                var gridPos3 = grid.WorldToCell(hitInfo.point);
                gridPos = new Vector2Int(gridPos3.x, gridPos3.y);
                
                return true;
            }
        }

        gridPos = Vector2Int.zero;
        return false;
    }
    
    public bool TileSelected(Vector3 position, out Vector2Int gridPos)
    {
        if (Physics.Raycast(position + Vector3.back * 45 , Vector3.forward, out var hitInfo))
        {
            if (hitInfo.collider.gameObject == this.gameObject)
            {
                var gridPos3 = grid.WorldToCell(hitInfo.point);
                gridPos = new Vector2Int(gridPos3.x, gridPos3.y);
                
                return true;
            }
        }

        gridPos = Vector2Int.zero;
        return false;
    }
    
    private GenericMenu TileMenu()
    {
        GenericMenu menu = new GenericMenu();
        
        menu.AddItem(new GUIContent("Change stage"), false, ChangeStage);
        menu.AddItem(new GUIContent("Edit in StageBuilder"), false, OpenInStageBuilder, StageData);
        menu.AddItem(new GUIContent("Remove stage"), false, RemoveSelf);
        
        return menu;
    }

    private void ChangeStage()
    {
        //TODO Refactor this into file loader class
        var path = EditorUtility.OpenFilePanel("Open", StageBuilder.DefaultFolder, "asset");
        if (string.IsNullOrEmpty(path)) return;
        
        try
        {
            var stage = AssetDatabase.LoadAssetAtPath<StageData>(UnityEditor.FileUtil.GetProjectRelativePath(path));
            if (stage == null)
            {
                Debug.LogErrorFormat("Cannot load {0} asset at {1}", "Stage", path);
                return;
            }

            SetStage(stage);
            gameObject.name = Path.GetFileNameWithoutExtension(path);
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("Exception when open asset {0} {1} {2}", path, ex.Message, ex.StackTrace);
        }
    }

    private void OpenInStageBuilder(object stage)
    {
        var stageObject = stage as StageData;
        StageBuilderScene.loadStageUponEnable = AssetDatabase.GetAssetPath(stageObject);
        EditorApplication.ExecuteMenuItem("VibeDash/StageBuilder");
    }
    
    private void RemoveSelf()
    {
        DestroyImmediate(gameObject);
    }

    [ContextMenu("CreateMesh")]
    private void CreateBackgroundMesh()
    {
        GetComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh = MeshGenerator.Quad(StageData.Size.x + 2, StageData.Size.y + 2, Vector3.back);
        var posX = (StageData.Size.x + 2) / 2.0f * grid.cellSize.x;
        var posY = (StageData.Size.y + 2) / 2.0f * grid.cellSize.y;
        RepositionGrid();
    }
    
    private void RepositionGrid()
    {
        var posX = - StageData.Size.x / 2.0f * grid.cellSize.x;
        var posY = - StageData.Size.y / 2.0f * grid.cellSize.y;
        grid.transform.localPosition = new Vector3(posX, posY, 0);
    }
}
