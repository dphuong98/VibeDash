using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        {TileType.Entrance, 'e'},
        {TileType.Exit, 'x'},
        {TileType.Air, 'a'},
        {TileType.Road, 'r'},
        {TileType.Wall, 'w'},
        {TileType.Bridge, 'b'},
    };

    private static Dictionary<TileType, Color> ColorMap = new Dictionary<TileType, Color>()
    {
        {TileType.Entrance, new Color(0f, 1f, 1f, 0.5f)},
        {TileType.Exit, new Color(1f, 0f, 0.03f, 0.77f)},
        { TileType.Air, Color.clear },
        { TileType.Road,  new Color(0.24f, 0.26f, 0.42f, 0.5f)},
        { TileType.Wall, new Color(0.96f, 0.38f, 0.83f) },
        { TileType.Bridge, new Color(0.63f, 0.34f, 0.02f) },
    };

    private Grid grid;
    private Level loadedLevel;

    public Vector2Int SelectedTile = new Vector2Int(-1, -1);
    
    public Level LoadedLevel
    {
        get => loadedLevel;
    }

    private Level editingLevel;
    public Level EditingLevel
    {
        get
        {
            if (editingLevel == null)
            {
                Open(TempFile);
                
                if (editingLevel == null)
                {
                    NewLevel();
                }
            }

            return editingLevel;
        }
    }
    
    public int Cols => EditingLevel.Size.x;
    public int Rows => EditingLevel.Size.y;

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

    [ContextMenu("Test")]
    public void Test()
    {
        var tmp = EditingLevel.ShortestPath();
    }
    
    private void DrawSceneGUI(SceneView sceneview)
    {
        if (EditingLevel == null)
        {
            return;
        }

        DrawTileIcons();
        HandleClick();
        HandleKey();

        //Other GUI option

        //DrawSolution
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
                    EditingLevel[gridPos.x, gridPos.y] = types.First().Key;
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

        if (tilePos.x == -1 || tilePos.x == Cols || tilePos.y == -1 || tilePos.y == Rows)
        {
            foreach (TileType t in Enum.GetValues(typeof(TileType)))
            {
                menu.AddItem(new GUIContent(string.Format("[{1}] {0}", t, GetShortcut(t))), false, OnTileMenuClicked, new Tuple<int, int, TileType>(tilePos.x, tilePos.y, t));
            }
        }
        else
        {
            var dot = EditingLevel[tilePos.x, tilePos.y];

            foreach (TileType t in Enum.GetValues(typeof(TileType)))
            {
                menu.AddItem(new GUIContent(string.Format("[{1}] {0}", t, GetShortcut(t))), t == dot, OnTileMenuClicked, new Tuple<int, int, TileType>(tilePos.x, tilePos.y, t));
            }
        }
        
        return menu;
    }

    private void OnTileMenuClicked(object userdata)
    {
        if (userdata is Tuple<int, int, TileType> menuData)
        {
            var tilePos = new Vector2Int(menuData.Item1, menuData.Item2);
            ExpandBorder(ref tilePos);
            EditingLevel[tilePos.x, tilePos.y] = menuData.Item3;
            EditorUtility.SetDirty(editingLevel);
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
        for (var y = 0; y < EditingLevel.Size.y; y++)
        {
            for (var x = 0; x < EditingLevel.Size.x; x++)
            {
                var tile = EditingLevel[x, y];
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
    

    public void NewLevel()
    {
        editingLevel = Level.CreateLevel();
        AssetDatabase.CreateAsset(editingLevel, TempFile);
        AssetDatabase.SaveAssets();
        
        CreateBackgroundMesh();
    }

    //TODO add enum direction to prevent repetition
    public void ExpandBottom()
    {
        EditingLevel.ExpandTop();
        EditorUtility.SetDirty(EditingLevel);
        CreateBackgroundMesh();
    }

    public void ExpandLeft()
    {
        EditingLevel.ExpandLeft();
        EditorUtility.SetDirty(EditingLevel);
        CreateBackgroundMesh();
    }

    public void ExpandRight()
    {
        EditingLevel.ExpandRight();
        EditorUtility.SetDirty(EditingLevel);
        CreateBackgroundMesh();
    }

    public void ExpandTop()
    {
        EditingLevel.ExpandBottom();
        EditorUtility.SetDirty(EditingLevel);
        CreateBackgroundMesh();
    }

    public void CollapseTop()
    {
        EditingLevel.CollapseTop();
        EditorUtility.SetDirty(EditingLevel);
        CreateBackgroundMesh();
    }

    public void CollapseLeft()
    {
        EditingLevel.CollapseLeft();
        EditorUtility.SetDirty(EditingLevel);
        CreateBackgroundMesh();
    }

    public void CollapseRight()
    {
        EditingLevel.CollapseRight();
        EditorUtility.SetDirty(EditingLevel);
        CreateBackgroundMesh();
    }

    public void CollapseBottom()
    {
        EditingLevel.CollapseBottom();
        EditorUtility.SetDirty(EditingLevel);
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

    public void SaveAs(string path)
    {
        try
        {
            var asset = ScriptableObject.CreateInstance<Level>();
            asset.CopyFrom(editingLevel);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            loadedLevel = asset;

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
            var asset = AssetDatabase.LoadAssetAtPath<Level>(path);
            if (asset == null)
            {
                Debug.LogErrorFormat("Cannot load level asset at {0}", path);
                return false;
            }
            loadedLevel = asset;
            editingLevel.CopyFrom(asset);
            CreateBackgroundMesh();
            
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
        if (loadedLevel != null)
        {
            EditingLevel.CopyFrom(LoadedLevel);
            EditorUtility.SetDirty(EditingLevel);
            CreateBackgroundMesh();
            Debug.Log("Reloaded level");
        }
    }

    public void Save()
    {
        if (LoadedLevel == null)
        {
            Debug.LogError("Loaded Level is NULL");
            return;
        }

        LoadedLevel.CopyFrom(EditingLevel);

        EditorUtility.SetDirty(LoadedLevel);
        EditorUtility.SetDirty(EditingLevel);
        AssetDatabase.SaveAssets();
        
        Debug.LogFormat("Saved level to {0}", LoadedLevel);
    }
}
