using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class StageRenderer
{
    private static readonly Color roadColor = new Color(0.64f, 0.66f, 0.64f);

    private static readonly Dictionary<TileType, Color> BackgroundColorMap = new Dictionary<TileType, Color>()
    {
        {TileType.Air, Color.clear},
        {TileType.Corner, roadColor},
        {TileType.Entrance, new Color(0.39f, 0.74f, 1f)},
        {TileType.Exit, new Color(1f, 0.26f, 0.19f, 0.77f)},
        {TileType.PortalBlue, roadColor},
        {TileType.PortalOrange, roadColor},
        {TileType.Push, roadColor},
        {TileType.Road, roadColor},
        {TileType.Stop, roadColor},
        {TileType.Wall, new Color(0.94f, 0.62f, 0.79f)},
        {TileType.Blank, new Color(0.82f, 0.69f, 0.82f)}
    };

    private static readonly Dictionary<TileType, Texture> IconMap = new Dictionary<TileType, Texture>();

    private static readonly Dictionary<TileType, List<Texture>> DirectionalIconMap =
        new Dictionary<TileType, List<Texture>>();

    public static float solutionSpeed = 4f;
    public static Color solutionColor = new Color(1f, 0.97f, 0.11f);
    private static bool initialized;
    private static Grid grid;
    private static StageData stageData;
    
    private static void Init()
    {
        IconMap[TileType.Entrance] = Resources.Load<Texture>("Icons/Entrance");
        IconMap[TileType.Exit] = Resources.Load<Texture>("Icons/Finish");
        IconMap[TileType.PortalBlue] = Resources.Load<Texture>("Icons/PortalBlue");
        IconMap[TileType.PortalOrange] = Resources.Load<Texture>("Icons/PortalOrange");
        IconMap[TileType.Stop] = Resources.Load<Texture>("Icons/Stop");
        
        //Hint: 0 = Up; 1 = Right; 2 = Down; 3 = Left
        DirectionalIconMap[TileType.Push] = new List<Texture>();
        DirectionalIconMap[TileType.Push].AddUnique(Resources.Load<Texture>("Icons/Arrows/U_Arrow"));
        DirectionalIconMap[TileType.Push].AddUnique(Resources.Load<Texture>("Icons/Arrows/R_Arrow"));
        DirectionalIconMap[TileType.Push].AddUnique(Resources.Load<Texture>("Icons/Arrows/D_Arrow"));
        DirectionalIconMap[TileType.Push].AddUnique(Resources.Load<Texture>("Icons/Arrows/L_Arrow"));
        
        DirectionalIconMap[TileType.Corner] = new List<Texture>();
        DirectionalIconMap[TileType.Corner].AddUnique(Resources.Load<Texture>("Icons/Corners/U_Corner"));
        DirectionalIconMap[TileType.Corner].AddUnique(Resources.Load<Texture>("Icons/Corners/R_Corner"));
        DirectionalIconMap[TileType.Corner].AddUnique(Resources.Load<Texture>("Icons/Corners/D_Corner"));
        DirectionalIconMap[TileType.Corner].AddUnique(Resources.Load<Texture>("Icons/Corners/L_Corner"));

        initialized = true;
    }
    
    public static void SetStage(StageData stageData, Grid grid)
    {
        if (!initialized) Init();
        
        StageRenderer.stageData = stageData;
        StageRenderer.grid = grid;
    }
    
    public static void DrawTileIcons()
    {
        if (stageData == null || grid == null)
        {
            Debug.LogError("Please use StageRenderer.SetStage(...) before using Render methods");
        }
        
        for (var y = 0; y < stageData.Size.y; y++)
        {
            for (var x = 0; x < stageData.Size.x; x++)
            {
                var tile = stageData[x, y];
                var gridPos = new Vector2Int(x, y);

                DrawTileIcon(tile, gridPos);
            }
        }
    }

    private static void DrawTileIcon(TileType tile, Vector2Int gridPos)
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
        if (stageData.TileDirections.TryGetValue(gridPos, out var direction))
        {
            if (!DirectionalIconMap.ContainsKey(tile) ||
                DirectionalIconMap[tile].Count != 4
            ) return;

            if (direction == Vector2Int.up)
            {
                HandlesExt.DrawTexture(worldPos + new Vector3(-iconRadius, iconRadius), DirectionalIconMap[tile][0],
                    210);
            }

            if (direction == Vector2Int.right)
            {
                HandlesExt.DrawTexture(worldPos + new Vector3(-iconRadius, iconRadius), DirectionalIconMap[tile][1],
                    210);
            }

            if (direction == Vector2Int.down)
            {
                HandlesExt.DrawTexture(worldPos + new Vector3(-iconRadius, iconRadius), DirectionalIconMap[tile][2],
                    210);
            }

            if (direction == Vector2Int.left)
            {
                HandlesExt.DrawTexture(worldPos + new Vector3(-iconRadius, iconRadius), DirectionalIconMap[tile][3],
                    210);
            }
        }

        //Portal numbering
        if (tile == TileType.PortalBlue || tile == TileType.PortalOrange)
        {
            var textSize = 100f;
            var portal = stageData.PortalPairs.Where(s => s.Blue == gridPos || s.Orange == gridPos);
            if (portal.Any())
            {
                HandlesExt.DrawText(worldPos + new Vector3(-iconRadius, iconRadius) + new Vector3(1, -1, 0) * iconRadius * 0.4f,
                    (stageData.PortalPairs.IndexOf(portal.First()) + 1).ToString(), textSize);
            }
        }
    }

    public static void DrawHighLight(Vector2Int gridPos)
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

    public static void DrawMovingSolution()
    {
        var solution = stageData.Solution;
        if (solution == null || solution.Count < 2) return;
        
        var frame = (int)Math.Round(Time.realtimeSinceStartup * solutionSpeed) % solution.Count;

        if (frame == solution.Count - 1) return;
        HandlesExt.DrawArrow(grid.GetCellCenterWorld(solution[frame]), grid.GetCellCenterWorld(solution[frame+1]), solutionColor);

    } 
    
    public static void DrawSolution() {
        var solution = stageData.Solution;
        if (solution == null || solution.Count < 2) return;
        
        //TODO refactor this into smaller classes
        var tracePath = new List<Vector2Int> {solution[0]};

        for (var i = 0; i < solution.Count - 1; i++)
        {
            var currentPath = grid.GetCellCenterWorld(solution[i + 1]) - grid.GetCellCenterWorld(solution[i]);
            var nextPath =
                i + 2 == solution.Count
                    ? default
                    : grid.GetCellCenterWorld(solution[i + 2]) - grid.GetCellCenterWorld(solution[i + 1]);
            var sideOffset = currentPath.RotateClockwiseXY().normalized * grid.cellSize.y / 10;
            var lengthOffset = currentPath.normalized * sideOffset.magnitude * 2;

            //Label path order when branch. WARNING: Place before prevent duplication
            if (nextPath != default && Pathfinding.ExistDirectedPath(tracePath, solution[i], solution[i + 1]))
            {
                var nextNodes = Pathfinding.GetNextNodes(tracePath, solution[i], solution[i + 1]);

                if (nextNodes.Any(s =>
                    grid.GetCellCenterWorld(s) - grid.GetCellCenterWorld(solution[i + 1]) != nextPath))
                {
                    nextNodes.Add(solution[i + 2]);
                    for (var nodeIndex = 0; nodeIndex < nextNodes.Count; nodeIndex++)
                    {
                        var _nextPath = grid.GetCellCenterWorld(nextNodes[nodeIndex]) -
                                        grid.GetCellCenterWorld(solution[i + 1]);
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

                        HandlesExt.DrawText(
                            grid.GetCellCenterWorld(solution[i + 1]) + 2 * _sideOffset + _lengthOffset + _labelOffset,
                            (nodeIndex + 1).ToString(), 50);
                    }
                }
            }

            //Prevent duplication
            if (i + 2 < solution.Count &&
                Pathfinding.ExistDirectedPath(tracePath, solution[i], solution[i + 1], solution[i + 2]))
            {
                tracePath.Add(solution[i + 1]);
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
                tracePath.Add(solution[i + 1]);
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
                tracePath.Add(solution[i + 1]);
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
                tracePath.Add(solution[i + 1]);
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
                tracePath.Add(solution[i + 1]);
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
                tracePath.Add(solution[i + 1]);
                continue;
            }

            //Straight or deadend
            if (currentPath == nextPath || nextPath == default ||
                (nextPath != default && !solution[i + 1].IsAdjacent(solution[i + 2])))
            {
                HandlesExt.DrawPath(grid.GetCellCenterWorld(solution[i]) + sideOffset + lengthOffset,
                    grid.GetCellCenterWorld(solution[i + 1]) + sideOffset + lengthOffset, solutionColor);
                tracePath.Add(solution[i + 1]);
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
                tracePath.Add(solution[i + 1]);
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
                tracePath.Add(solution[i + 1]);
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
                tracePath.Add(solution[i + 1]);
                continue;
            }
        }

    }
}
