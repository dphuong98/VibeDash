using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine;

public class SceneCameraController : MonoBehaviour
{
    public enum ViewMode
    {
        Isometric,
        Perspective
    }
    
    [SerializeField]
    private StageManager stageManager;
    private ViewMode currentMode;

    public Transform perspectiveTransform;
    public Transform isometricTransform;

    public void SetViewMode(ViewMode mode)
    {
        if (stageManager.GetCurrentStage() is var stage && stage == null)
            return;
        
        var stagePosition = stage.transform.position;

        if (mode == ViewMode.Isometric)
        {
            SceneView.lastActiveSceneView.pivot =  stagePosition + isometricTransform.position;
            SceneView.lastActiveSceneView.rotation =  isometricTransform.rotation;
        }

        if (mode == ViewMode.Perspective)
        {
            SceneView.lastActiveSceneView.pivot = stagePosition + perspectiveTransform.position;
            SceneView.lastActiveSceneView.rotation =  perspectiveTransform.rotation;
        }
        
        currentMode = mode;
    }

    [ContextMenu("ToggleViewMode")]
    public void ToggleViewMode()
    {
        if (currentMode == ViewMode.Isometric)
        {
            SetViewMode(ViewMode.Perspective);
            return;
        }


        if (currentMode == ViewMode.Perspective)
        {
            SetViewMode(ViewMode.Isometric);
            return;
        }
    }

    public ViewMode GetViewMode()
    {
        return currentMode;
    }
}
