using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Tilemaps;
using UnityEngine;

public class TileBrush : EditorWindow
{
    private readonly Vector2 buttonSize = new Vector2(50,50);

    private GameObject tilemapPalette;
    private PrefabBrush[] brushes;

    [MenuItem("Extra/TileBrush")]
    static void Init()
    {
        // TODO Make floating window
        GetWindow(typeof(TileBrush));
    }

    private void OnEnable()
    {
        brushes = Resources.LoadAll<PrefabBrush>("TileBrushes");
    }

    private void OnGUI()
    {
        GUIContent buttonContent;
        GUILayout.BeginHorizontal();
        foreach (var brush in brushes)
        {
            var so = new SerializedObject(brush);
            var prefab = so.FindProperty("m_Prefab").objectReferenceValue;

            buttonContent = new GUIContent(AssetPreview.GetAssetPreview(prefab), brush.name);
            if (GUILayout.Button(buttonContent, GUILayout.Width(buttonSize.x), GUILayout.Height(buttonSize.y)))
            {
                //TODO Even if you switch draw tool, Unity wont let you draw unless you have Tile Palette window opened
                EditorTools.SetActiveTool(typeof(PaintTool));
                GridPaintingState.gridBrush = brush;
            }
        }
        GUILayout.EndHorizontal();
    }
}
