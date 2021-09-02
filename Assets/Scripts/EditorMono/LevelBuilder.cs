using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class LevelBuilder : MonoBehaviour
{
    //Paths
    private const string TempFile = "Assets/Editor/LevelBuilderTmp/_tmp_.asset"; //OG named these caches but they dont behave like caches.
    
    //Data
    private static Dictionary<TileType, char> ShortCuts = new Dictionary<TileType, char>()
    {
        {TileType.Air, 'a'},
        {TileType.Road, 'r'},
        {TileType.Wall, 'w'},
    };

    private static Dictionary<TileType, Color> ColorMap = new Dictionary<TileType, Color>()
    {
        { TileType.Air, Color.clear },
        { TileType.Road, new Color(0f, 1f, 1f, 0.5f) },
        { TileType.Wall, new Color(0.96f, .25f, .82f, 1) },
    };

    private Grid grid;
    private Vector2Int selectedTile = new Vector2Int(-1, -1);
    
    

    private Level _currentLevel;
    public Level CurrentLevel
    {
        get
        {
            if (_currentLevel == null)
            {
                _currentLevel = AssetDatabase.LoadAssetAtPath<Level>(TempFile);
                
                if (_currentLevel == null)
                {
                    NewStage();
                }
            }

            return _currentLevel;
        }
    }
    
    public int Cols => CurrentLevel.Size.x;
    public int Rows => CurrentLevel.Size.y;

    private void OnEnable()
    {
        SceneView.duringSceneGui += DrawSceneGUI;
        grid = GetComponentInChildren<Grid>();
        CreateBackgroundMesh();
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
        if (CurrentLevel == null)
        {
            return;
        }

        DrawTileIcons();
        HandleClick();
        HandleKey();

        //Other GUI option

        //Stage info

        //DrawSolution
    }

    private void HandleKey()
    {
        /*if (Selection.activeGameObject == gameObject &&
            Event.current.type == EventType.KeyDown
            )
        {

            Level.Dot dot = null;

            if (selection.r >= 0 && selection.r < editingLevel.Dimension.r && selection.c >= 0 && selection.c < editingLevel.Dimension.c)
            {
                dot = editingLevel[selection.r, selection.c];
            }
            else
            {
                return;
            }

            var types = ShortCuts.Where(i => i.Value == Event.current.character).Select(k => k.Key);
            if (types.Count() > 0)
            {
                dot.type = types.First();
                Event.current.Use();
                EditorUtility.SetDirty(editingLevel);
            }

            if (Event.current.character == 'h')
            {
                if (dot.type == Level.DotType.Tile || dot.type == Level.DotType.Block || dot.type == Level.DotType.Empty || dot.type == Level.DotType.Redirection)
                {
                    dot.Index = selection;
                    if (!EditingLevel.IsHint(dot.Index))
                        EditingLevel.AddHint(selection);
                    else
                        EditingLevel.RemoveHint(selection);
                    Event.current.Use();
                    EditorUtility.SetDirty(editingLevel);
                }
            }

            if (Event.current.character >= '6' && Event.current.character <= '9')
            {
                dot.lamp[Event.current.character - '6'] = !dot.lamp[Event.current.character - '6'];
                Event.current.Use();
                EditorUtility.SetDirty(editingLevel);
            }

        }*/
    }

    private void HandleClick()
    {
        if (Selection.activeGameObject == this.gameObject &&
            Event.current.type == EventType.MouseDown &&
            Event.current.modifiers == EventModifiers.None &&
            Event.current.button == 0)
        {
            if (TileSelected(out var gridPos))
            {
                selectedTile = gridPos;
            }
        }

        if (Selection.activeGameObject == gameObject &&
            Event.current.type == EventType.MouseDown &&
            Event.current.modifiers == EventModifiers.None &&
            Event.current.button == 1)
        {
            if (TileSelected(out var gridPos))
            {
                selectedTile = gridPos;
                SceneView.RepaintAll();
                TileMenu(selectedTile).ShowAsContext();
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

    private GenericMenu TileMenu(Vector2Int vector2Int)
    {
        GenericMenu menu = new GenericMenu();

        var dot = CurrentLevel[vector2Int.x, vector2Int.y];

        foreach (TileType t in Enum.GetValues(typeof(TileType)))
        {
            menu.AddItem(new GUIContent(string.Format("[{1}] {0}", t, GetShortcut(t))), t == dot, OnTileMenuClicked, new Tuple<int, int, TileType>(vector2Int.x, vector2Int.y, t));
        }

        //menu.AddSeparator("");
        /*menu.AddItem(new GUIContent("Hint [h]"), EditingLevel.IsHint(dot.Index),
            OnContextHintClicked, new MenuItemData(dot, dot.type));

        menu.AddSeparator("");
        for (int i = 0; i < dot.lamp.Length; i++)
        {
            menu.AddItem(new GUIContent(string.Format("Lamp: {0} [{1}]", SideToString[i], i + 6)), dot.lamp[i],
                OnContextLampClicked, new MenuItemData(dot, dot.type, i));
        }

        menu.AddSeparator("");
        for (int i = 0; i < dot.source.Length; i++)
        {
            foreach (Level.LightDirection st in Enum.GetValues(typeof(Level.LightDirection)))
            {
                menu.AddItem(new GUIContent("Source " + SideToString[i] + "/" + st.ToString()), st == dot.source[i],
                    OnContextSourceClicked, new MenuItemData(dot, st, i));
            }
        }*/

        return menu;
    }

    private void OnTileMenuClicked(object userdata)
    {
        if (userdata is Tuple<int, int, TileType> menuData)
        {
            var tilePos = new Vector2Int(menuData.Item1, menuData.Item2);
            ExpandBorder(ref tilePos);
            CurrentLevel[tilePos.x, tilePos.y] = menuData.Item3;
            EditorUtility.SetDirty(_currentLevel);
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
        for (var y = 0; y < CurrentLevel.Size.y; y++)
        {
            for (var x = 0; x < CurrentLevel.Size.x; x++)
            {
                var tile = CurrentLevel[x, y];
                var gridPos = new Vector2Int(x, y);
                
                DrawTileIcon(tile, gridPos);
                if (selectedTile == gridPos)
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
        _currentLevel = Level.CreateLevel();
        AssetDatabase.CreateAsset(_currentLevel, TempFile);
        AssetDatabase.SaveAssets();
        
        CreateBackgroundMesh();
    }

    //TODO add enum direction to prevent repetition
    public void ExpandBottom()
    {
        CurrentLevel.ExpandTop();
        EditorUtility.SetDirty(CurrentLevel);
        CreateBackgroundMesh();
    }

    public void ExpandLeft()
    {
        CurrentLevel.ExpandLeft();
        EditorUtility.SetDirty(CurrentLevel);
        CreateBackgroundMesh();
    }

    public void ExpandRight()
    {
        CurrentLevel.ExpandRight();
        EditorUtility.SetDirty(CurrentLevel);
        CreateBackgroundMesh();
    }

    public void ExpandTop()
    {
        CurrentLevel.ExpandBottom();
        EditorUtility.SetDirty(CurrentLevel);
        CreateBackgroundMesh();
    }

    public void CollapseTop()
    {
        CurrentLevel.CollapseTop();
        EditorUtility.SetDirty(CurrentLevel);
        CreateBackgroundMesh();
    }

    public void CollapseLeft()
    {
        CurrentLevel.CollapseLeft();
        EditorUtility.SetDirty(CurrentLevel);
        CreateBackgroundMesh();
    }

    public void CollapseRight()
    {
        CurrentLevel.CollapseRight();
        EditorUtility.SetDirty(CurrentLevel);
        CreateBackgroundMesh();
    }

    public void CollapseBottom()
    {
        CurrentLevel.CollapseBottom();
        EditorUtility.SetDirty(CurrentLevel);
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
        var posY = - Rows / 2.0f * grid.cellSize.y;
        grid.transform.position = new Vector3(posX, posY, 0);
    }
}
