using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using UnityEditor;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

[ExecuteInEditMode]
public class StageBuilder : MonoBehaviour
{
    //Path
    private string levelFolder = "Assets/Resources/Levels/";

    //Data
    private static Dictionary<TileType, char> ShortCuts = new Dictionary<TileType, char>()
    {
        {TileType.Entrance, 'e'},
        {TileType.Exit, 'x'},
        {TileType.Air, 'a'},
        {TileType.Road, 'r'},
        {TileType.Wall, 'w'},
    };

    private static Dictionary<TileType, Color> ColorMap = new Dictionary<TileType, Color>()
    {
        {TileType.Entrance, new Color(0f, 1f, 1f, 0.5f)},
        {TileType.Exit, new Color(1f, 0f, 0.03f, 0.77f)},
        { TileType.Air, Color.clear },
        { TileType.Road,  new Color(0.24f, 0.26f, 0.42f, 0.5f)},
        { TileType.Wall, new Color(0.96f, 0.38f, 0.83f) },
    };

    //Components
    private Grid grid;
    
    //Members
    private List<Vector2Int> solution;
    
    public bool SolutionMode = false;
    public Vector2Int SelectedTile = new Vector2Int(-1, -1);
    
    private Stage loadedStage;
    public Stage LoadedStage
    {
        get => loadedStage;
    }

    private Stage editingStage;
    public Stage EditingStage => editingStage;

    public int Cols => editingStage.Size.x;
    public int Rows => editingStage.Size.y;

    private void OnEnable()
    {
        SceneView.duringSceneGui += DrawSceneGUI;
        grid = GetComponentInChildren<Grid>();
        
        if (editingStage == null)
            NewStage();
        CreateVisualization();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DrawSceneGUI;
    }

    private void DrawSceneGUI(SceneView sceneview)
    {
        if (editingStage == null)
        {
            return;
        }
        
        DrawTileIcons();
        DrawSolution();

        HandleClick();
        HandleKey();

        //Other GUI option

        //DrawSolution
    }

    private void DrawSolution()
    {
        if (!SolutionMode) return;
        
        if (solution == null)
            CreateSolution();
        
        //Draw

    }

    private void HandleKey()
    {
        if (Selection.activeGameObject == gameObject &&
            Event.current.type == EventType.KeyDown
        )
        {
            if (TileSelected(out var gridPos))
            {
                if (gridPos.x == -1 || gridPos.x == Cols || gridPos.y == -1 || gridPos.y == Rows)
                {
                    return;
                }

                var types = ShortCuts.Where(i => i.Value == Event.current.character);

                if (types.Any())
                {
                    editingStage[gridPos.x, gridPos.y] = types.First().Key;
                    CreateSolution();
                }
            }
        }
    }

    private void HandleClick()
    {
        if (Selection.activeGameObject == this.gameObject &&
            Event.current.type == EventType.MouseDown &&
            Event.current.modifiers == EventModifiers.None &&
            TileSelected(out var gridPos))
        {
            SelectedTile = gridPos;

            if (Event.current.button == 1)
            {
                
                SceneView.RepaintAll();
                TileMenu(SelectedTile).ShowAsContext();
            }
            
            Event.current.Use();
        }
    }

    private void ExpandBorder(ref Vector2Int newTilePos)
    {
        var offsetVector = new Vector2Int();

        if (newTilePos.x == -1)
        {
            ExpandLeft();
            offsetVector.x = -1;
        }
        
        if (newTilePos.x == Cols)
        {
            ExpandRight();
        }

        if (newTilePos.y == -1)
        {
            ExpandBottom();
            offsetVector.y = -1;
        }
        
        if (newTilePos.y == Rows)
        {
            ExpandTop();
        }

        newTilePos -= offsetVector;
    }

    private GenericMenu TileMenu(Vector2Int tilePos)
    {
        GenericMenu menu = new GenericMenu();
        
        //Border expand set
        if (tilePos.x == -1 || tilePos.x == Cols || tilePos.y == -1 || tilePos.y == Rows)
        {
            foreach (TileType t in Enum.GetValues(typeof(TileType)))
            {
                if (t == TileType.Exit)
                    continue;
                menu.AddItem(new GUIContent(string.Format("[{1}] {0}", t, GetShortcut(t))), false, OnTileMenuClicked, new Tuple<int, int, TileType>(tilePos.x, tilePos.y, t));
            }
        }
        else
        //Inside tile set
        {
            var dot = editingStage[tilePos.x, tilePos.y];

            foreach (TileType t in Enum.GetValues(typeof(TileType)))
            {
                if (t == TileType.Exit && !IsOnBorder(tilePos))
                    continue;
                menu.AddItem(new GUIContent(string.Format("[{1}] {0}", t, GetShortcut(t))), t == dot, OnTileMenuClicked, new Tuple<int, int, TileType>(tilePos.x, tilePos.y, t));
            }
        }
        
        return menu;
    }

    private bool IsOnBorder(Vector2Int tilePos)
    {
        return 0 == tilePos.x || tilePos.x == Cols - 1 || 0 == tilePos.y || tilePos.y == Rows - 1;
    }

    private void OnTileMenuClicked(object userdata)
    {
        if (userdata is Tuple<int, int, TileType> menuData)
        {
            var clickedPos = new Vector2Int(menuData.Item1, menuData.Item2);
            ExpandBorder(ref clickedPos);
            editingStage[clickedPos.x, clickedPos.y] = menuData.Item3;
            EditorUtility.SetDirty(editingStage);
            CreateSolution();
        }
    }

    private char GetShortcut(TileType tile)
    {
        if (ShortCuts.TryGetValue(tile, out var shortcut))
        {
            return shortcut;
        }

        return ' ';
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

                Event.current.Use();

                return true;
            }
        }

        gridPos = Vector2Int.zero;
        return false;
    }

    /// <summary>
    /// Draw from (0,0) of grid
    /// </summary>
    private void DrawTileIcons()
    {
        for (var y = 0; y < editingStage.Size.y; y++)
        {
            for (var x = 0; x < editingStage.Size.x; x++)
            {
                var tile = editingStage[x, y];
                var gridPos = new Vector2Int(x, y);
                
                DrawTileIcon(tile, gridPos);
                if (SelectedTile == gridPos)
                    DrawHighLight(gridPos);
            }
        }
        
    }

    private void DrawHighLight(Vector2Int gridPos)
    {
        Handles.color = Color.yellow;
        var worldPos = grid.GetCellCenterWorld(new Vector3Int(gridPos.x, gridPos.y, 0));

        var Radius = 0.5f;
        var rr = Radius / 2 * Mathf.Sqrt(2) * 1.4f;
        Handles.DrawAAPolyLine(7, new Vector3[]
        {
            worldPos + new Vector3(-rr, -rr),
            worldPos + new Vector3(-rr, rr),
            worldPos + new Vector3(rr, rr),
            worldPos + new Vector3(rr, -rr),

            worldPos + new Vector3(-rr, -rr),
        });
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

    public void NewStage()
    {
        editingStage = Stage.CreateStage();
        var tmp = Path.Combine(levelFolder, "_tmp_.asset");
        AssetDatabase.CreateAsset(editingStage, Path.Combine(levelFolder, "_tmp_.asset"));
        AssetDatabase.SaveAssets();
        CreateVisualization();
    }
    
    public void ExpandBottom()
    {
        editingStage.ExpandBottom();
        EditorUtility.SetDirty(editingStage);
        CreateVisualization();
    }

    public void ExpandLeft()
    {
        editingStage.ExpandLeft();
        EditorUtility.SetDirty(editingStage);
        CreateVisualization();
    }

    public void ExpandRight()
    {
        editingStage.ExpandRight();
        EditorUtility.SetDirty(editingStage);
        CreateVisualization();
    }

    public void ExpandTop()
    {
        editingStage.ExpandTop();
        EditorUtility.SetDirty(editingStage);
        CreateVisualization();
    }

    public void CollapseTop()
    {
        editingStage.CollapseTop();
        EditorUtility.SetDirty(editingStage);
        CreateVisualization();
    }

    public void CollapseLeft()
    {
        editingStage.CollapseLeft();
        EditorUtility.SetDirty(editingStage);
        CreateVisualization();
    }

    public void CollapseRight()
    {
        editingStage.CollapseRight();
        EditorUtility.SetDirty(editingStage);
        CreateVisualization();
    }

    public void CollapseBottom()
    {
        editingStage.CollapseBottom();
        EditorUtility.SetDirty(editingStage);
        CreateVisualization();
    }
    
    private void CreateBackgroundMesh()
    {
        GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().sharedMesh = MeshGenerator.Quad(Cols + 2, Rows + 2, Vector3.back);
        RepositionGrid();
    }

    private void CreateVisualization()
    {
        CreateBackgroundMesh();
        CreateSolution();
    }

    [ContextMenu("CreateSolution")]
    private void CreateSolution()
    {
        if (editingStage.GetEntrance() == -Vector2Int.one || editingStage.GetExit() == -Vector2Int.one)
            return;
        //Brute force
        //Find all path from entrance to exit
        if (MapNode(new Graph<Vector2Int>(), editingStage.GetEntrance(), out var allExitPaths))
        {
            var fullPath = allExitPaths.GroupBy(s => s.Distinct().Count()).Aggregate((i1,i2) => i1.Key > i2.Key ? i1 : i2);
            var shortestFullPath = fullPath.GroupBy(s => s.Count).Aggregate((i1,i2) => i1.Key < i2.Key ? i1 : i2);
            solution = shortestFullPath.First();
        }
    }

    //TODO refactor this
    private bool MapNode(Graph<Vector2Int> traceGraph, Vector2Int currentNode, out List<List<Vector2Int>> exitPaths)
    {
        exitPaths = new List<List<Vector2Int>>();
        
        if (editingStage[currentNode.x, currentNode.y] == TileType.Exit)
            return true;

        var direction = Vector2Int.up;

        do
        {
            if (traceGraph.ExistDirectedPath(currentNode, currentNode + direction))
            {
                direction.RotateClockwise(); continue;
            }
            
            //Exit path detected
            if (editingStage.TryMove(currentNode, direction, out var scoutPath))
            {
                if (scoutPath.Count == 0)
                {
                    direction.RotateClockwise(); continue;
                }
                
                //Trace Stacks
                traceGraph.AddDirected(currentNode, scoutPath.First());
                for (int i = 0; i < scoutPath.Count - 1; i++)
                {
                    traceGraph.AddDirected(scoutPath[i], scoutPath[i+1]);
                }

                //Recursion ends when DFS meet an exit. Only return false when exit doesnt exist
                if (MapNode(traceGraph, scoutPath.Last(), out var scoutPath2))
                {
                    if (scoutPath2.Count == 0)
                    {
                        exitPaths.Add(scoutPath);
                    }
                    
                    foreach (var i in scoutPath2)
                    {
                        exitPaths.Add(scoutPath.Concat(i).ToList());
                    }
                }

                //Remove Trace Stacks
                traceGraph.RemoveDirected(currentNode, scoutPath.First());
                for (int i = 0; i < scoutPath.Count - 1; i++)
                {
                    traceGraph.RemoveDirected(scoutPath[i], scoutPath[i+1]);
                }
            }
            
            direction.RotateClockwise();
        } while (direction != Vector2Int.up);

        return exitPaths.Count > 0;
    }

    private void RepositionGrid()
    {
        var posX = - Cols / 2.0f * grid.cellSize.x;
        var posY = - Rows / 2.0f * grid.cellSize.y;
        grid.transform.position = new Vector3(posX, posY, 0);
    }

    public void SaveAs(string path)
    {
        try
        {
            var asset = ScriptableObject.CreateInstance<Stage>();
            asset.CopyFrom(editingStage);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            loadedStage = asset;

            gameObject.name = Path.GetFileNameWithoutExtension(path);
            Debug.LogFormat("Saved level to {0}", path);
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("{0} {1}", ex.Message, ex.StackTrace);
        }
    }

    public bool Open(string path)
    {
        try
        {
            var asset = AssetDatabase.LoadAssetAtPath<Stage>(path);
            if (asset == null)
            {
                Debug.LogErrorFormat("Cannot load level asset at {0}", path);
                return false;
            }
            loadedStage = asset;
            editingStage.CopyFrom(asset);
            CreateVisualization();
            
            gameObject.name = Path.GetFileNameWithoutExtension(path);
            Debug.LogFormat("Opened level from {0}", path);
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("Exception when open asset {0} {1} {2}", path, ex.Message, ex.StackTrace);
        }

        return true;
    }

    public void Reload()
    {
        if (loadedStage != null)
        {
            editingStage.CopyFrom(LoadedStage);
            EditorUtility.SetDirty(editingStage);
            CreateBackgroundMesh();
            Debug.Log("Reloaded level");
        }
    }

    public void Save()
    {
        if (LoadedStage == null)
        {
            Debug.LogError("Loaded Stage is NULL");
            return;
        }

        LoadedStage.CopyFrom(editingStage);

        EditorUtility.SetDirty(LoadedStage);
        EditorUtility.SetDirty(editingStage);
        AssetDatabase.SaveAssets();
        
        Debug.LogFormat("Saved level to {0}", LoadedStage);
    }
}
