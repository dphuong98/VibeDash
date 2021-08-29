using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class StageBuilder : MonoBehaviour
{
    //Paths
    private const string TempFile = "Assets/Editor/LevelBuilderTmp/_tmp_.asset"; //OG named these caches but they dont behave like caches

    private Grid grid;

    private Stage _currentStage;
    public Stage CurrentStage
    {
        get
        {
            if (_currentStage == null)
            {
                Debug.Log("Creating a new level...");
                _currentStage = AssetDatabase.LoadAssetAtPath<Stage>(TempFile);
                
                if (_currentStage == null)
                {
                    NewStage();
                }
            }

            return _currentStage;
        }
    }
    
    public int Cols => CurrentStage.Size.x;
    public int Rows => CurrentStage.Size.y;

    private void OnEnable()
    {
        SceneView.duringSceneGui += DrawSceneGUI;
        grid = GetComponentInChildren<Grid>();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DrawSceneGUI;
    }

    public void Init()
    {
        
    }
    
    private void DrawSceneGUI(SceneView sceneview)
    {
        if (CurrentStage == null)
        {
            return;
        }

        DrawTileIcons();
        //HandleClick();

        //Other GUI option
        
        //Stage info

        //DrawSolution
    }

    private void DrawTileIcons()
    {
        for (var y = 0; y < CurrentStage.Size.y; y++)
        {
            for (var x = 0; x < CurrentStage.Size.x; x++)
            {
                var tile = CurrentStage[x, y];
                DrawTileIcon(tile, new Vector3Int(x, y, 0));
            }
        }
        
    }

    private void DrawTileIcon(TileType tile, Vector3Int gridPos)
    {
        if (tile == TileType.Wall)
        {
            var worldPos = grid.GetCellCenterWorld(gridPos);
            Handles.color = Color.green;

            float ee = 0.45f;
            Handles.DrawAAConvexPolygon(new Vector3[]
            {
                worldPos + new Vector3(-ee, -ee),
                worldPos + new Vector3(-ee, ee),
                worldPos + new Vector3(ee, ee),
                worldPos + new Vector3(ee, -ee),
            });
        }
    }
    

    public void NewStage()
    {
        _currentStage = Stage.CreateStage();
        AssetDatabase.CreateAsset(_currentStage, TempFile);
        AssetDatabase.SaveAssets();
        
        CreateBackgroundMesh();
    }

    //TODO add enum direction to prevent repetition
    public void ExpandTop()
    {
        CurrentStage.ExpandTop();
        EditorUtility.SetDirty(CurrentStage);
        CreateBackgroundMesh();
    }

    public void ExpandLeft()
    {
        CurrentStage.ExpandLeft();
        EditorUtility.SetDirty(CurrentStage);
        CreateBackgroundMesh();
    }

    public void ExpandRight()
    {
        CurrentStage.ExpandRight();
        EditorUtility.SetDirty(CurrentStage);
        CreateBackgroundMesh();
    }

    public void ExpandBottom()
    {
        CurrentStage.ExpandBottom();
        EditorUtility.SetDirty(CurrentStage);
        CreateBackgroundMesh();
    }

    public void CollapseTop()
    {
        CurrentStage.CollapseTop();
        EditorUtility.SetDirty(CurrentStage);
        CreateBackgroundMesh();
    }

    public void CollapseLeft()
    {
        CurrentStage.CollapseLeft();
        EditorUtility.SetDirty(CurrentStage);
        CreateBackgroundMesh();
    }

    public void CollapseRight()
    {
        CurrentStage.CollapseRight();
        EditorUtility.SetDirty(CurrentStage);
        CreateBackgroundMesh();
    }

    public void CollapseBottom()
    {
        CurrentStage.CollapseBottom();
        EditorUtility.SetDirty(CurrentStage);
        CreateBackgroundMesh();
    }
    
    private void CreateBackgroundMesh()
    {
        GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().sharedMesh = MeshGenerator.Quad(Cols + 2, Rows + 2, Vector3.back);
        RepositionGrid();
    }

    private void RepositionGrid()
    {
        var posX = - Cols / 2.0f * grid.cellSize.x;
        var posY = Rows / 2.0f * grid.cellSize.y;
        grid.transform.position = new Vector3(posX, posY, 0);
    }
}
