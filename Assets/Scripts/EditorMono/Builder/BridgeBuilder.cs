﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PathCreation;
using UnityEngine;

public class BridgeBuilder : MonoBehaviour
{
    [SerializeField] private PathCreator pathCreator;
    [SerializeField] private GlobalDisplaySettings displaySettings;
    private const float spacing = 1.1f; //Suitable for tile length of 1 Unity unit (size of a cube)
    
    public void SetBridge(IEnumerable<Vector3> path)
    {
        //Absolute straight line cause PathCreator to glitch, this is to make the bridge not absolutely straight
        var noGlitchPath = path.ToList();
        noGlitchPath[0] = new Vector3(noGlitchPath[0].x + 0.0001f, noGlitchPath[0].y, noGlitchPath[0].z);
        pathCreator.bezierPath = new BezierPath(noGlitchPath, false, PathSpace.xy) {AutoControlLength = 0.31f};
    }

    public int GetMaxTile()
    {
        return (int)Math.Floor(pathCreator.path.length / spacing);
    }

    public void SetBezierColor(Color color)
    {
        if (pathCreator.IsEditingBridge)
            displaySettings.editingBridgePath = color;
        else
            displaySettings.bridgePath = color;
    }

    public void SetEditingBridge(bool state)
    {
        pathCreator.IsEditingBridge = state;
    }
}