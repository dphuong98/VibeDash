using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using UnityEditor;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

[ExecuteInEditMode]
public class MiniStage : MonoBehaviour
{
    public int maxPoints { get; private set; }
    public StageData StageData { get; private set; }
    private MeshFilter meshFilter;

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
        meshFilter = GetComponent<MeshFilter>();
    }

    private void DrawSceneGUI(SceneView sceneview)
    {
        if (StageData == null) return;

        DrawMaxPoints();
        HandleClick();
    }

    private void DrawMaxPoints()
    {
        var topLeft = transform.position + new Vector3(0, meshFilter.sharedMesh.bounds.size.y + 0.7f, 0);
        HandlesExt.DrawText(topLeft, "MaxPoints: " + maxPoints, 150, Color.white);
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
        var path = EditorUtility.OpenFilePanel("Open", StageBuilder.StageFolder, "asset");
        if (string.IsNullOrEmpty(path)) return;
        
        try
        {
            var stage = AssetDatabase.LoadAssetAtPath<StageData>(FileUtil.GetProjectRelativePath(path));
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
        GetComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh = MeshGenerator.Quad(StageData.Size.x,
            StageData.Size.y, Vector3.back, new Vector2Int(-1, -1));
    }
}
