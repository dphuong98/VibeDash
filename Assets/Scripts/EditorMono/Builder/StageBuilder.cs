using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using UnityEditor;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[ExecuteInEditMode]
public class StageBuilder : Builder<Stage>
{
    //Path
    private const string stageFolder = "Assets/Resources/Data/Stages";
    public static string StageFolder => stageFolder;

    //Data
    private static readonly Dictionary<TileType, char> ShortCuts = new Dictionary<TileType, char>()
    {
        {TileType.Entrance, 'e'},
        {TileType.Exit, 'x'},
        {TileType.Air, 'a'},
        {TileType.Road, 'r'},
        {TileType.Wall, 'w'},
        {TileType.Stop, 's'}
    };

    private static readonly Dictionary<TileType, Color> ColorMap = new Dictionary<TileType, Color>()
    {
        {TileType.Entrance, new Color(0f, 1f, 1f, 0.5f)},
        {TileType.Exit, new Color(1f, 0f, 0.03f, 0.77f)},
        {TileType.Air, Color.clear},
        {TileType.Road,  new Color(0.24f, 0.26f, 0.42f, 0.5f)},
        {TileType.Wall, new Color(0.94f, 0.62f, 0.79f)},
        {TileType.Stop, new Color(1f, 0.62f, 0.27f)}
    };

    //Components
    private Grid grid;
    
    //Members
    private List<Vector2Int> solution;

    public int SolutionSpeed = 4;
    public bool SolutionMode = false;
    public bool MovingSolution = true;
    public Vector2Int SelectedTile = new Vector2Int(-1, -1);
    public Color SolutionColor = Color.yellow;

    public Stage LoadedStage => LoadedItem;
    public Stage EditingStage => EditingItem;

    public int Cols => EditingItem.Size.x;
    public int Rows => EditingItem.Size.y;

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
        
        base.Init(stageFolder);
    }

    private void DrawSceneGUI(SceneView sceneview)
    {
        if (EditingStage == null) return;

        DrawTileIcons();
        DrawSolution();

        HandleClick();
        HandleKey();

        //Other GUI option
    }

    private void DrawSolution()
    {
        if (!SolutionMode || solution == null || solution.Count < 2) return;

        if (MovingSolution)
        {
            var frame = (int)Math.Round(Time.realtimeSinceStartup * SolutionSpeed) % solution.Count;

            if (frame == solution.Count - 1) return;
            HandlesExt.DrawArrow(grid.GetCellCenterWorld(solution[frame]), grid.GetCellCenterWorld(solution[frame+1]), SolutionColor);
        }
        else
        {
            //TODO refactor this into smaller classes
            var tracePath = new List<Vector2Int> {solution[0]};

            for (var i = 0; i < solution.Count - 1; i++)
            {
                var currentPath = grid.GetCellCenterWorld(solution[i+1]) - grid.GetCellCenterWorld(solution[i]);
                var nextPath =
                    i + 2 == solution.Count ? default : grid.GetCellCenterWorld(solution[i+2]) - grid.GetCellCenterWorld(solution[i+1]);
                var sideOffset = currentPath.RotateClockwiseXY().normalized * grid.cellSize.y / 10;
                var lengthOffset = currentPath.normalized * sideOffset.magnitude * 2;
                
                //Entering cross: draw large overpass over 2 lines
                if (currentPath == nextPath && 
                        (
                        Pathfinding.ExistDirectedPath(tracePath,
                        solution[i + 1] + currentPath.RotateClockwiseXY().ToVector2Int(), solution[i + 1],
                                    solution[i + 1] + currentPath.RotateCounterClockwiseXY().ToVector2Int()) 
                        ||
                        Pathfinding.ExistDirectedPath(tracePath,
                        solution[i + 1] + currentPath.RotateCounterClockwiseXY().ToVector2Int(), solution[i + 1],
                                    solution[i + 1] + currentPath.RotateClockwiseXY().ToVector2Int())
                        )
                )
                {
                    HandlesExt.DrawLine(grid.GetCellCenterWorld(solution[i]) + sideOffset + lengthOffset,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset - lengthOffset, SolutionColor);
                    HandlesExt.DrawSimpleArc(grid.GetCellCenterWorld(solution[i + 1]) + sideOffset - lengthOffset,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset, SolutionColor);
                    tracePath.Add(solution[i+1]);
                    continue;
                }

                //Overpass then turn left
                if (currentPath == nextPath &&
                    Pathfinding.ExistDirectedPath(tracePath,
                        solution[i + 1] + currentPath.RotateClockwiseXY().ToVector2Int(), solution[i + 1], solution[i])
                )
                {
                    HandlesExt.DrawLine(grid.GetCellCenterWorld(solution[i]) + sideOffset + lengthOffset,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset - lengthOffset, SolutionColor);
                    HandlesExt.DrawSimpleArc(grid.GetCellCenterWorld(solution[i + 1]) + sideOffset - lengthOffset,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset, SolutionColor);
                    tracePath.Add(solution[i+1]);
                    continue;
                }

                if (nextPath == currentPath.RotateCounterClockwiseXY() &&
                    Pathfinding.ExistDirectedPath(tracePath, solution[i + 2], solution[i + 1],
                        solution[i + 1] - nextPath.ToVector2Int())
                )
                {
                    var gapLength = currentPath.RotateCounterClockwiseXY() / 3;
                    HandlesExt.DrawLine(grid.GetCellCenterWorld(solution[i]) + sideOffset + lengthOffset,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset - lengthOffset, SolutionColor);
                    HandlesExt.DrawSimpleArc(grid.GetCellCenterWorld(solution[i + 1]) + sideOffset - lengthOffset,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset / 2, SolutionColor);
                    HandlesExt.DrawLine(grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset / 2,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset / 2 + gapLength,
                        SolutionColor);
                    tracePath.Add(solution[i+1]);
                    continue;
                }

                //Left turn then overpass
                if (currentPath == nextPath &&
                    Pathfinding.ExistDirectedPath(tracePath, solution[i + 2], solution[i + 1],
                        solution[i + 1] + currentPath.RotateClockwiseXY().ToVector2Int())
                )
                {
                    HandlesExt.DrawLine(grid.GetCellCenterWorld(solution[i]) + sideOffset + lengthOffset,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset - lengthOffset, SolutionColor);
                    HandlesExt.DrawSimpleArc(grid.GetCellCenterWorld(solution[i + 1]) + sideOffset - lengthOffset,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset, SolutionColor);
                    tracePath.Add(solution[i+1]);
                    continue;
                }

                if (nextPath == currentPath.RotateCounterClockwiseXY() &&
                    Pathfinding.ExistDirectedPath(tracePath, solution[i + 1] + currentPath.ToVector2Int(),
                        solution[i + 1], solution[i])
                )
                {
                    var gapLength = currentPath.RotateCounterClockwiseXY() / 3.3f;
                    HandlesExt.DrawLine(grid.GetCellCenterWorld(solution[i]) + sideOffset + lengthOffset,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset / 2, SolutionColor);
                    HandlesExt.DrawSimpleArc(grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset / 2,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset / 2 + gapLength,
                        SolutionColor);
                    tracePath.Add(solution[i+1]);
                    continue;
                }

                //Straight or deadend
                if (currentPath == nextPath || nextPath == default)
                {
                    HandlesExt.DrawPath(grid.GetCellCenterWorld(solution[i]) + sideOffset + lengthOffset,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset, SolutionColor);
                    tracePath.Add(solution[i+1]);
                    continue;
                }

                //U-turn
                if (currentPath == -nextPath) //TODO float tolerance
                {
                    var gapLength = currentPath.RotateClockwiseXY() / 5;
                    HandlesExt.DrawLine(grid.GetCellCenterWorld(solution[i + 1]) - sideOffset - lengthOffset,
                        grid.GetCellCenterWorld(solution[i + 1]) - sideOffset - lengthOffset + gapLength,
                        SolutionColor);
                    HandlesExt.DrawPath(grid.GetCellCenterWorld(solution[i]) + sideOffset + lengthOffset,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset - lengthOffset, SolutionColor);
                    tracePath.Add(solution[i+1]);
                    continue;
                }

                //Left turn
                if (nextPath == currentPath.RotateCounterClockwiseXY())
                {
                    var appendOffset = currentPath.RotateCounterClockwiseXY().normalized * sideOffset.magnitude * 3;
                    HandlesExt.DrawPath(grid.GetCellCenterWorld(solution[i]) + sideOffset + lengthOffset,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset / 2, SolutionColor);
                    HandlesExt.DrawLine(grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset / 2,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset / 2 + appendOffset,
                        SolutionColor);
                    tracePath.Add(solution[i+1]);
                    continue;
                }

                //Right turn
                if (nextPath == currentPath.RotateClockwiseXY())
                {
                    var appendOffset = currentPath.RotateClockwiseXY().normalized * sideOffset.magnitude;
                    HandlesExt.DrawPath(grid.GetCellCenterWorld(solution[i]) + sideOffset + lengthOffset,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset - lengthOffset / 2, SolutionColor);
                    HandlesExt.DrawLine(grid.GetCellCenterWorld(solution[i + 1]) + sideOffset - lengthOffset / 2,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset - lengthOffset / 2 + appendOffset,
                        SolutionColor);
                    tracePath.Add(solution[i+1]);
                    continue;
                }
                
                //Number path order
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
        }
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
                    if (types.First().Key == TileType.Exit && !EditingStage.IsOnBorder(gridPos)) return;
                    
                    EditingStage[gridPos.x, gridPos.y] = types.First().Key;
                    CreateSolution();
                }
            }
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
            var dot = EditingStage[tilePos.x, tilePos.y];

            foreach (TileType t in Enum.GetValues(typeof(TileType)))
            {
                if (t == TileType.Exit && !EditingStage.IsOnBorder(tilePos))
                    continue;
                menu.AddItem(new GUIContent(string.Format("[{1}] {0}", t, GetShortcut(t))), t == dot, OnTileMenuClicked, new Tuple<int, int, TileType>(tilePos.x, tilePos.y, t));
            }
        }
        
        return menu;
    }

    private void OnTileMenuClicked(object userdata)
    {
        if (userdata is Tuple<int, int, TileType> menuData)
        {
            var clickedPos = new Vector2Int(menuData.Item1, menuData.Item2);
            ExpandBorder(ref clickedPos);
            EditingStage[clickedPos.x, clickedPos.y] = menuData.Item3;
            EditorUtility.SetDirty(EditingStage);
            CreateSolution();
        }
    }

    private static char GetShortcut(TileType tile)
    {
        if (ShortCuts.TryGetValue(tile, out var shortcut))
        {
            return shortcut;
        }

        return ' ';
    }

    //TODO refactor
    private bool TileSelected(out Vector2Int gridPos)
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
    
    private void DrawTileIcons()
    {
        for (var y = 0; y < EditingStage.Size.y; y++)
        {
            for (var x = 0; x < EditingStage.Size.x; x++)
            {
                var tile = EditingStage[x, y];
                var gridPos = new Vector2Int(x, y);
                
                DrawTileIcon(tile, gridPos);
                if (SelectedTile == gridPos)
                    DrawHighLight(gridPos);
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

    public void ExpandBottom()
    {
        EditingStage.ExpandBottom();
        EditorUtility.SetDirty(EditingStage);
        OnReload();
    }

    public void ExpandLeft()
    {
        EditingStage.ExpandLeft();
        EditorUtility.SetDirty(EditingStage);
        OnReload();
    }

    public void ExpandRight()
    {
        EditingStage.ExpandRight();
        EditorUtility.SetDirty(EditingStage);
        OnReload();
    }

    public void ExpandTop()
    {
        EditingStage.ExpandTop();
        EditorUtility.SetDirty(EditingStage);
        OnReload();
    }

    public void CollapseTop()
    {
        EditingStage.CollapseTop();
        EditorUtility.SetDirty(EditingStage);
        OnReload();
    }

    public void CollapseLeft()
    {
        EditingStage.CollapseLeft();
        EditorUtility.SetDirty(EditingStage);
        OnReload();
    }

    public void CollapseRight()
    {
        EditingStage.CollapseRight();
        EditorUtility.SetDirty(EditingStage);
        OnReload();
    }

    public void CollapseBottom()
    {
        EditingStage.CollapseBottom();
        EditorUtility.SetDirty(EditingStage);
        OnReload();
    }

    private void CreateBackgroundMesh()
    {
        GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().sharedMesh = MeshGenerator.Quad(Cols + 2, Rows + 2, Vector3.back);
        RepositionGrid();
    }
    
    private void RepositionGrid()
    {
        var posX = - Cols / 2.0f * grid.cellSize.x;
        var posY = - Rows / 2.0f * grid.cellSize.y;
        grid.transform.localPosition = new Vector3(posX, posY, 0);
    }

    private void CreateSolution()
    {
        solution = Pathfinding.GetSolution(EditingStage);
    }

    protected override void OnReload()
    {
        CreateBackgroundMesh();
        CreateSolution();
    }
}
