using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PathCreation;
using UnityEngine;

public class BridgeRenderer : MonoBehaviour
{
    [SerializeField] private PathCreator pathCreator;
    private const float spacing = 1.1f; //Suitable for tile length of 1 Unity unit (size of a cube)
    
    public void SetBridge(IEnumerable<Vector3> path)
    {
        pathCreator.bezierPath = new BezierPath(path, false, PathSpace.xy);
        pathCreator.bezierPath.AutoControlLength = 0.31f;
    }

    public int GetMaxTile()
    {
        return 0;
    }
}
