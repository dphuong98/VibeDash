using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MovementManager))]
public class MovementManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var movementManager = target as MovementManager;
        
        base.OnInspectorGUI();

        if (GUILayout.Button("Up"))
        {
            movementManager.Move(Vector3Int.up);
        }
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Left"))
        {
            movementManager.Move(Vector3Int.left);
        }
        if (GUILayout.Button("Right"))
        {
            movementManager.Move(Vector3Int.right);
        }
        GUILayout.EndHorizontal();
        
        if (GUILayout.Button("Down"))
        {
            movementManager.Move(Vector3Int.down);
        }
    }
}
