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
public class StageBuilder : Builder<StageData>
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
    private bool pastSolutionMode;
    
    public bool viewSolution = true;
    public bool movingSolution;

    public Vector2Int selectedTile = new Vector2Int(-1, -1);

    public StageData LoadedStageData => LoadedItem;
    public StageData EditingStageData => EditingItem;

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

        base.Init(stageFolder);
    }

    private void DrawSceneGUI(SceneView sceneview)
    {
        if (EditingStageData == null) return;

        if (!pastSolutionMode) CreateSolution();
        pastSolutionMode = viewSolution;

        DrawBuilderFocusButton();
        
        StageRenderer.SetStage(EditingStageData, grid);
        StageRenderer.DrawTileIcons();
        StageRenderer.DrawHighLight(selectedTile);

        if (viewSolution)
        {
            if (movingSolution)
                StageRenderer.DrawMovingSolution(); 
            else StageRenderer.DrawSolution();
        }

        HandleClick();
        HandleKey();
        
        //Other GUI option
    }
    
    private void DrawBuilderFocusButton()
    {
        Handles.BeginGUI();

        if (HandlesExt.DrawButton(10, 400, 100, 50, "StageBuilder"))
        {
            UnityEditorWindowHelper.GetWindow(WindowType.Inspector);
            Selection.activeGameObject = gameObject;
            SceneView.lastActiveSceneView.rotation = Quaternion.Euler(0,0,0);
            SceneView.FrameLastActiveSceneView();
            SceneView.lastActiveSceneView.orthographic = true;
        }

        Handles.EndGUI();
        
    }

    private void HandleClick()
    {
        if (Selection.activeGameObject == this.gameObject &&
            Event.current.type == EventType.MouseDown &&
            Event.current.modifiers == EventModifiers.None &&
            TileSelected(out var gridPos))
        {
            selectedTile = gridPos;

            if (Event.current.button == 1)
            {
                SceneView.RepaintAll();
                TileMenu(selectedTile).ShowAsContext();
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
                var types = ShortCuts.Where(i => i.Value == Event.current.character);
                if (!types.Any()) return;
                
                ExpandBorder(ref gridPos);
                if (types.First().Key == TileType.Exit && !EditingStageData.IsOnBorder(gridPos)) return;
                if (types.First().Key == TileType.PortalOrange && !EditingStageData.PortalPending(out _)) return;
                SetTileData(gridPos.x, gridPos.y, types.First().Key);
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
        if (!EditingStageData.IsValidTile(tilePos))
        {
            foreach (TileType t in Enum.GetValues(typeof(TileType)))
            {
                if (t == TileType.Exit)
                    continue;
                menu.AddItem(new GUIContent(string.Format("[{1}] {0}", t, GetShortcut(t))), false, OnTileSelectMenu, new Tuple<Vector2Int, TileType>(tilePos, t));
            }
        }
        else
        //Inside tile set
        {
            var dot = EditingStageData[tilePos.x, tilePos.y];

            foreach (TileType t in Enum.GetValues(typeof(TileType)))
            {
                if (t == TileType.Exit && !EditingStageData.IsOnBorder(tilePos))
                    continue;
                if (t == TileType.PortalOrange && !EditingStageData.PortalPending(out _))
                    continue;
                
                menu.AddItem(new GUIContent(string.Format("[{1}] {0}", t, GetShortcut(t))), t == dot, OnTileSelectMenu, new Tuple<Vector2Int, TileType>(tilePos, t));
            }

            if (EditingStageData[tilePos.x, tilePos.y] == TileType.Push || 
                EditingStageData[tilePos.x, tilePos.y] == TileType.Corner
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
        
        var currentDirection = EditingStageData.TileDirections[menuData.Item2];

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
        
        EditingStageData.SetTileDirection(menuData.Item2, currentDirection);
        EditorUtility.SetDirty(EditingStageData);
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
        EditingStageData[x, y] = type;
        EditorUtility.SetDirty(EditingStageData);
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

    public void ExpandBottom()
    {
        EditingStageData.ExpandBottom();
        EditorUtility.SetDirty(EditingStageData);
        OnReload();
    }

    public void ExpandLeft()
    {
        EditingStageData.ExpandLeft();
        EditorUtility.SetDirty(EditingStageData);
        OnReload();
    }

    public void ExpandRight()
    {
        EditingStageData.ExpandRight();
        EditorUtility.SetDirty(EditingStageData);
        OnReload();
    }

    public void ExpandTop()
    {
        EditingStageData.ExpandTop();
        EditorUtility.SetDirty(EditingStageData);
        OnReload();
    }

    public void CollapseTop()
    {
        EditingStageData.CollapseTop();
        EditorUtility.SetDirty(EditingStageData);
        OnReload();
    }

    public void CollapseLeft()
    {
        EditingStageData.CollapseLeft();
        EditorUtility.SetDirty(EditingStageData);
        OnReload();
    }

    public void CollapseRight()
    {
        EditingStageData.CollapseRight();
        EditorUtility.SetDirty(EditingStageData);
        OnReload();
    }

    public void CollapseBottom()
    {
        EditingStageData.CollapseBottom();
        EditorUtility.SetDirty(EditingStageData);
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
        if (viewSolution) EditingStageData.GenerateSolution();
    }

    protected override void OnReload()
    {
        CreateBackgroundMesh();
        CreateSolution();
    }
}
