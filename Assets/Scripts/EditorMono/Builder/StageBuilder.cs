using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using TMPro;
using UnityEditor;
using UnityEngine;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
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
        {TileType.Air, 'a'},
        {TileType.Entrance, 'e'},
        {TileType.Corner, 'c'},
        {TileType.Exit, 'x'},
        {TileType.PortalBlue, 'b'},
        {TileType.PortalOrange, 'o'},
        {TileType.Push, 'p'},
        {TileType.Road, 'r'},
        {TileType.Stop, 's'},
        {TileType.Wall, 'w'},
    };

    private static readonly Dictionary<TileType, Color> BackgroundColorMap = new Dictionary<TileType, Color>()
    {
        {TileType.Air, Color.clear},
        {TileType.Corner, new Color(0.63f, 0.63f, 0.63f)},
        {TileType.Entrance, new Color(0.39f, 0.74f, 1f)},
        {TileType.Exit, new Color(1f, 0.26f, 0.19f, 0.77f)},
        {TileType.PortalBlue, new Color(0.63f, 0.63f, 0.63f)},
        {TileType.PortalOrange, new Color(0.63f, 0.63f, 0.63f)},
        {TileType.Push, new Color(0.63f, 0.63f, 0.63f)},
        {TileType.Road,  new Color(0.63f, 0.63f, 0.63f)},
        {TileType.Stop, new Color(0.63f, 0.63f, 0.63f)},
        {TileType.Wall, new Color(0.94f, 0.62f, 0.79f)},
    };

    private static readonly Dictionary<TileType, Texture> IconMap = new Dictionary<TileType, Texture>();
    private static readonly Dictionary<TileType, List<Texture>> DirectionalIconMap = new Dictionary<TileType, List<Texture>>();
    
    //Components
    private Grid grid;
    
    //Members
    private bool pastSolutionMode = false;
    private List<Vector2Int> solution;

    private readonly Color solutionColor = new Color(1f, 0.97f, 0.11f);
    public bool MovingSolution = true;
    public int SolutionSpeed = 4;
    public bool SolutionMode = false;

    public Vector2Int SelectedTile = new Vector2Int(-1, -1);

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
        IconMap[TileType.Entrance] = Resources.Load<Texture>("Icons/Entrance");
        IconMap[TileType.Exit] = Resources.Load<Texture>("Icons/Finish");
        IconMap[TileType.PortalBlue] = Resources.Load<Texture>("Icons/PortalBlue");
        IconMap[TileType.PortalOrange] = Resources.Load<Texture>("Icons/PortalOrange");
        IconMap[TileType.Stop] = Resources.Load<Texture>("Icons/Stop");
        
        //Hint: 0 = Up; 1 = Right; 2 = Down; 3 = Left
        DirectionalIconMap.Add(TileType.Push, new List<Texture>());
        DirectionalIconMap[TileType.Push].AddUnique(Resources.Load<Texture>("Icons/Arrows/U_Arrow"));
        DirectionalIconMap[TileType.Push].AddUnique(Resources.Load<Texture>("Icons/Arrows/R_Arrow"));
        DirectionalIconMap[TileType.Push].AddUnique(Resources.Load<Texture>("Icons/Arrows/D_Arrow"));
        DirectionalIconMap[TileType.Push].AddUnique(Resources.Load<Texture>("Icons/Arrows/L_Arrow"));
        
        DirectionalIconMap.Add(TileType.Corner, new List<Texture>());
        DirectionalIconMap[TileType.Corner].AddUnique(Resources.Load<Texture>("Icons/Corners/U_Corner"));
        DirectionalIconMap[TileType.Corner].AddUnique(Resources.Load<Texture>("Icons/Corners/R_Corner"));
        DirectionalIconMap[TileType.Corner].AddUnique(Resources.Load<Texture>("Icons/Corners/D_Corner"));
        DirectionalIconMap[TileType.Corner].AddUnique(Resources.Load<Texture>("Icons/Corners/L_Corner"));

        base.Init(stageFolder);
    }

    private void DrawSceneGUI(SceneView sceneview)
    {
        if (EditingStage == null) return;
        
        if (!pastSolutionMode) CreateSolution();
        pastSolutionMode = SolutionMode;

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
            HandlesExt.DrawArrow(grid.GetCellCenterWorld(solution[frame]), grid.GetCellCenterWorld(solution[frame+1]), solutionColor);
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

                //Label path order when branch. WARNING: Place before prevent duplication
                var style = new GUIStyle {normal = {textColor = Color.black}};
                if (nextPath != default && Pathfinding.ExistDirectedPath(tracePath, solution[i], solution[i+1]))
                {
                    var nextNodes = Pathfinding.GetNextNodes(tracePath, solution[i], solution[i + 1]);

                    if (nextNodes.Any(s =>
                        grid.GetCellCenterWorld(s) - grid.GetCellCenterWorld(solution[i + 1]) != nextPath))
                    {
                        nextNodes.Add(solution[i + 2]);
                        for (var nodeIndex = 0; nodeIndex < nextNodes.Count; nodeIndex++)
                        {
                            var _nextPath = grid.GetCellCenterWorld(nextNodes[nodeIndex]) - grid.GetCellCenterWorld(solution[i + 1]);
                            var _sideOffset = _nextPath.RotateClockwiseXY().normalized * grid.cellSize.y / 10;
                            var _lengthOffset = _nextPath.normalized * _sideOffset.magnitude * 2;
                            if (_nextPath == Vector3.right)
                            {
                                HandlesExt.DrawText(grid.GetCellCenterWorld(solution[i + 1]) + _sideOffset + _lengthOffset,
                                    (nodeIndex + 1).ToString(), 50);
                                continue;
                            }

                            var _labelOffset = new Vector3();
                            if (_nextPath != Vector3.down) _labelOffset += Vector3.up / 4;
                            if (_nextPath != Vector3.up) _labelOffset += Vector3.left / 5;
                            
                            HandlesExt.DrawText(grid.GetCellCenterWorld(solution[i + 1]) + 2 * _sideOffset + _lengthOffset + _labelOffset,
                                (nodeIndex + 1).ToString(), 50);
                        }
                    }
                }
                
                //Prevent duplication
                if (i + 2 < solution.Count && Pathfinding.ExistDirectedPath(tracePath, solution[i], solution[i+1], solution[i+2]))
                {
                    tracePath.Add(solution[i+1]);
                    continue;
                }

                //Entering cross intersection: draw large overpass over 2 lines
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
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset - lengthOffset, solutionColor);
                    HandlesExt.DrawSimpleArc(grid.GetCellCenterWorld(solution[i + 1]) + sideOffset - lengthOffset,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset, solutionColor);
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
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset - lengthOffset, solutionColor);
                    HandlesExt.DrawSimpleArc(grid.GetCellCenterWorld(solution[i + 1]) + sideOffset - lengthOffset,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset, solutionColor);
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
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset - lengthOffset, solutionColor);
                    HandlesExt.DrawSimpleArc(grid.GetCellCenterWorld(solution[i + 1]) + sideOffset - lengthOffset,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset / 2, solutionColor);
                    HandlesExt.DrawLine(grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset / 2,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset / 2 + gapLength,
                        solutionColor);
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
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset - lengthOffset, solutionColor);
                    HandlesExt.DrawSimpleArc(grid.GetCellCenterWorld(solution[i + 1]) + sideOffset - lengthOffset,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset, solutionColor);
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
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset / 2, solutionColor);
                    HandlesExt.DrawSimpleArc(grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset / 2,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset / 2 + gapLength,
                        solutionColor);
                    tracePath.Add(solution[i+1]);
                    continue;
                }

                //Straight or deadend
                if (currentPath == nextPath || nextPath == default ||
                    (nextPath != default && !solution[i+1].IsAdjacent(solution[i+2])))
                {
                    HandlesExt.DrawPath(grid.GetCellCenterWorld(solution[i]) + sideOffset + lengthOffset,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset, solutionColor);
                    tracePath.Add(solution[i+1]);
                    continue;
                }

                //U-turn
                if (currentPath == -nextPath) //TODO float tolerance
                {
                    var gapLength = currentPath.RotateClockwiseXY() / 5;
                    HandlesExt.DrawLine(grid.GetCellCenterWorld(solution[i + 1]) - sideOffset - lengthOffset,
                        grid.GetCellCenterWorld(solution[i + 1]) - sideOffset - lengthOffset + gapLength,
                        solutionColor);
                    HandlesExt.DrawPath(grid.GetCellCenterWorld(solution[i]) + sideOffset + lengthOffset,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset - lengthOffset, solutionColor);
                    tracePath.Add(solution[i+1]);
                    continue;
                }

                //Left turn
                if (nextPath == currentPath.RotateCounterClockwiseXY())
                {
                    var appendOffset = currentPath.RotateCounterClockwiseXY().normalized * sideOffset.magnitude * 3;
                    HandlesExt.DrawPath(grid.GetCellCenterWorld(solution[i]) + sideOffset + lengthOffset,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset / 2, solutionColor);
                    HandlesExt.DrawLine(grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset / 2,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset / 2 + appendOffset,
                        solutionColor);
                    tracePath.Add(solution[i+1]);
                    continue;
                }

                //Right turn
                if (nextPath == currentPath.RotateClockwiseXY())
                {
                    var appendOffset = currentPath.RotateClockwiseXY().normalized * sideOffset.magnitude;
                    HandlesExt.DrawPath(grid.GetCellCenterWorld(solution[i]) + sideOffset + lengthOffset,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset - lengthOffset / 2, solutionColor);
                    HandlesExt.DrawLine(grid.GetCellCenterWorld(solution[i + 1]) + sideOffset - lengthOffset / 2,
                        grid.GetCellCenterWorld(solution[i + 1]) + sideOffset - lengthOffset / 2 + appendOffset,
                        solutionColor);
                    tracePath.Add(solution[i+1]);
                    continue;
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
                    if (types.First().Key == TileType.PortalOrange && !EditingStage.PortalPending()) return;
                    SetTileData(gridPos.x, gridPos.y, types.First().Key);
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
                menu.AddItem(new GUIContent(string.Format("[{1}] {0}", t, GetShortcut(t))), false, OnTileSelectMenu, new Tuple<int, int, TileType>(tilePos.x, tilePos.y, t));
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
                if (t == TileType.PortalOrange && !EditingStage.PortalPending())
                    continue;
                
                menu.AddItem(new GUIContent(string.Format("[{1}] {0}", t, GetShortcut(t))), t == dot, OnTileSelectMenu, new Tuple<Vector2Int, TileType>(tilePos, t));
            }

            if (EditingStage[tilePos.x, tilePos.y] == TileType.Push || 
                EditingStage[tilePos.x, tilePos.y] == TileType.Corner
                )
            {
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Rotate tile left"), false, OnTileRotateMenu, new Tuple<int, Vector2Int>(0, tilePos));
                menu.AddItem(new GUIContent("Rotate tile right"), false, OnTileRotateMenu, new Tuple<int, Vector2Int>(1, tilePos));
            }
        }
        
        return menu;
    }

    private void OnTileRotateMenu(object userdata)
    {
        if (!(userdata is Tuple<int, Vector2Int> menuData) ||
            (menuData.Item1 != 0 && menuData.Item1 != 1)
        ) return;
        
        var currentDirection = EditingStage.TileDirections[menuData.Item2];

        //Rotate left
        if (menuData.Item1 == 0)
        {
            currentDirection = currentDirection.RotateCounterClockwise();
        }

        //Rotate right
        if (menuData.Item1 == 1)
        {
            currentDirection = currentDirection.RotateClockwise();
        }
        
        EditingStage.SetTileDirection(menuData.Item2, currentDirection);
        EditorUtility.SetDirty(EditingStage);
        CreateSolution();
    }

    private void OnTileSelectMenu(object userdata)
    {
        if (!(userdata is Tuple<Vector2Int, TileType> menuData)) return;
        
        var clickedPos = menuData.Item1;
        ExpandBorder(ref clickedPos);
        SetTileData(clickedPos.x, clickedPos.y, menuData.Item2);
    }

    private void SetTileData(int x, int y, TileType type)
    {
        EditingStage[x, y] = type;
        EditorUtility.SetDirty(EditingStage);
        CreateSolution();
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
                gridPos =  grid.WorldToCell(hitInfo.point).ToVector2Int();
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
        float iconRadius = 0.45f;
        var worldPos = grid.GetCellCenterWorld(new Vector3Int(gridPos.x, gridPos.y, 0));

        if (BackgroundColorMap.TryGetValue(tile, out var color))
        {
            Handles.color = color;
            
            Handles.DrawAAConvexPolygon(new[]
            {
                worldPos + new Vector3(-iconRadius, -iconRadius),
                worldPos + new Vector3(-iconRadius, iconRadius),
                worldPos + new Vector3(iconRadius, iconRadius),
                worldPos + new Vector3(iconRadius, -iconRadius),
            });
        }
        
        //Draw tile icon
        if (IconMap.TryGetValue(tile, out var icon))
        {
            HandlesExt.DrawTexture(worldPos + new Vector3(-iconRadius, iconRadius), icon, 210);
        }

        //Draw directional tile
        if (EditingStage.TileDirections.TryGetValue(gridPos, out var direction))
        {
            if (!DirectionalIconMap.ContainsKey(tile) ||
                DirectionalIconMap[tile].Count != 4
                ) return;
            
            if (direction == Vector2Int.up)
            {
                HandlesExt.DrawTexture(worldPos + new Vector3(-iconRadius, iconRadius), DirectionalIconMap[tile][0], 210);
            }
            if (direction == Vector2Int.right)
            {
                HandlesExt.DrawTexture(worldPos + new Vector3(-iconRadius, iconRadius), DirectionalIconMap[tile][1], 210);
            }
            if (direction == Vector2Int.down)
            {
                HandlesExt.DrawTexture(worldPos + new Vector3(-iconRadius, iconRadius), DirectionalIconMap[tile][2], 210);
            }
            if (direction == Vector2Int.left)
            {
                HandlesExt.DrawTexture(worldPos + new Vector3(-iconRadius, iconRadius), DirectionalIconMap[tile][3], 210);
            }
        }
        
        //Portal numbering
        if (tile == TileType.PortalBlue || tile == TileType.PortalOrange)
        {
            var portal = EditingStage.PortalPairs.Where(s => s.Blue == gridPos || s.Orange == gridPos);
            if (portal.Any())
            {
                var style = new GUIStyle {fontSize = 30};
                Handles.Label(worldPos + new Vector3(-1, 3, 0) * iconRadius * 0.25f, (EditingStage.PortalPairs.IndexOf(portal.First()) + 1).ToString(), style);
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

    [ContextMenu("CreateSolution")]
    private void CreateSolution()
    {
        if (SolutionMode) solution = Pathfinding.GetSolution(EditingStage);
    }

    protected override void OnReload()
    {
        CreateBackgroundMesh();
        CreateSolution();
    }
}
