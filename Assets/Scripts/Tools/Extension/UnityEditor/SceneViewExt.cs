using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class SceneViewExt
{
    public static Vector3 SceneViewToWorld(this SceneView sceneView)
    {
        var worldPos = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
        worldPos.z = 0;
        return worldPos;
    }
}
