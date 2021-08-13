using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerController))]
public class PlayerControllerEditor : Editor
{
    private Vector3Int destination;
    
    public override void OnInspectorGUI()
    {
        var playerController = target as PlayerController;
        
        base.OnInspectorGUI();
        GUILayout.BeginHorizontal();
        destination.x = EditorGUILayout.IntField(destination.x);
        destination.y = EditorGUILayout.IntField(destination.y);
        destination.z = EditorGUILayout.IntField(destination.z);
        GUILayout.EndHorizontal();
        if (GUILayout.Button("MoveTo"))
        {
            playerController.DirectionalMove(destination);
        }
    }
}
