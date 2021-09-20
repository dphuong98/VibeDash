using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class MiniStage : MonoBehaviour
{
    public Stage Stage;
    private Grid grid;
    private MeshFilter meshFilter;
    
    private static readonly Dictionary<TileType, Color> ColorMap = new Dictionary<TileType, Color>()
    {
        {TileType.Entrance, new Color(0f, 1f, 1f, 0.5f)},
        {TileType.Exit, new Color(1f, 0f, 0.03f, 0.77f)},
        { TileType.Air, Color.clear },
        { TileType.Road,  new Color(0.24f, 0.26f, 0.42f, 0.5f)},
        { TileType.Wall, new Color(0.94f, 0.62f, 0.79f) },
    };

    public void SetStage(Stage stage)
    {
        Stage = stage;
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
        meshFilter = GetComponentInChildren<MeshFilter>();
    }

    private void DrawSceneGUI(SceneView sceneview)
    {
        if (Stage == null) return;
        
        DrawTileIcons();
        
        HandleClick();
    }
    
    private void DrawTileIcons()
    {
        for (var y = 0; y < Stage.Size.y; y++)
        {
            for (var x = 0; x < Stage.Size.x; x++)
            {
                var tile = Stage[x, y];
                var gridPos = new Vector2Int(x, y);
                
                DrawTileIcon(tile, gridPos);
            }
        }
        
    }
    
    private void DrawTileIcon(TileType tile, Vector2Int gridPos)
    {
        if (ColorMap.TryGetValue(tile, out var color))
        {
            var worldPos = grid.GetCellCenterWorld(new Vector3Int(gridPos.x, gridPos.y, 0));
            Handles.color = color;

            float iconRadius = 0.45f;
            Handles.DrawAAConvexPolygon(new Vector3[]
            {
                worldPos + new Vector3(-iconRadius, -iconRadius),
                worldPos + new Vector3(-iconRadius, iconRadius),
                worldPos + new Vector3(iconRadius, iconRadius),
                worldPos + new Vector3(iconRadius, -iconRadius),
            });
        }
        
    }
    
    private void HandleClick()
    {
        if (Selection.activeGameObject == this.gameObject &&
            Event.current.type == EventType.MouseDown &&
            Event.current.modifiers == EventModifiers.None &&
            TileSelected(out var gridPos))
        {
            if (Event.current.button == 1)
            {
                SceneView.RepaintAll();
                TileMenu().ShowAsContext();
                
                Event.current.Use();
            }
        }
    }
    
    private bool TileSelected(out Vector2Int gridPos)
    {
        var worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        if (Physics.Raycast(worldRay, out var hitInfo))
        {
            if (hitInfo.collider.gameObject == this.gameObject)
            {
                Vector3 point = hitInfo.point;
                var worldPos = hitInfo.collider.gameObject.transform.InverseTransformPoint(point);
                var temp = grid.WorldToCell(worldPos);
                gridPos = new Vector2Int(temp.x, temp.y);

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
        menu.AddItem(new GUIContent("Edit in StageBuilder"), false, OpenInStageBuilder, Stage);
        menu.AddItem(new GUIContent("Remove stage"), false, RemoveSelf);
        
        return menu;
    }

    private void ChangeStage()
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
        var stageInstance = stage as Stage;
        StageBuilderScene.loadStageUponEnable = AssetDatabase.GetAssetPath(stageInstance);
        EditorApplication.ExecuteMenuItem("VibeDash/StageBuilder");
    }
    
    private void RemoveSelf()
    {
        DestroyImmediate(gameObject);
    }

    [ContextMenu("CreateMesh")]
    private void CreateBackgroundMesh()
    {
        //TODO custom pivot
        meshFilter.sharedMesh = MeshGenerator.Quad(Stage.Size.x + 2, Stage.Size.y + 2, Vector3.back);
        var posX = (Stage.Size.x + 2) / 2.0f * grid.cellSize.x;
        var posY = (Stage.Size.y + 2) / 2.0f * grid.cellSize.y;
        meshFilter.transform.localPosition = new Vector3(posX, posY, 0);
        RepositionGrid();
    }
    
    private void RepositionGrid()
    {
        var posX = - Stage.Size.x / 2.0f * grid.cellSize.x;
        var posY = - Stage.Size.y / 2.0f * grid.cellSize.y;
        grid.transform.localPosition = new Vector3(posX, posY, 0);
    }
}
