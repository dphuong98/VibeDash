﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Tilemaps;
using UnityEngine;
using System.Linq;
using UnityEngine.WSA;

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
        GUILayout.BeginHorizontal();
        foreach (var brush in brushes)
        {
            var so = new SerializedObject(brush);
            var prefab = so.FindProperty("m_Prefab").objectReferenceValue;

            var buttonContent = new GUIContent(AssetPreview.GetAssetPreview(prefab), brush.name);
            if (GUILayout.Button(buttonContent, GUILayout.Width(buttonSize.x), GUILayout.Height(buttonSize.y)))
            {
                GetTilePaletteWindow();
                EditorTools.SetActiveTool(typeof(PaintTool));
                GridPaintingState.gridBrush = brush;
            }
        }
        GUILayout.EndHorizontal();
    }

    private void GetTilePaletteWindow()
    {
        var type = Type.GetType("UnityEditor.Tilemaps.GridPaintPaletteWindow, Unity.2D.Tilemap.Editor.dll");
        var getWindowInfo = typeof(EditorWindow)
            .GetMethods().First(m => m.Name == "GetWindow"
                                     && m.IsGenericMethod
                                     && m.GetParameters().Length > 0
                                     && m.GetParameters()[0].ParameterType == typeof(Type[]));
        var genericMethod = getWindowInfo.MakeGenericMethod(type);
        genericMethod.Invoke(null, new object[] { new Type[] { typeof(TileBrush) }});
    }
}
