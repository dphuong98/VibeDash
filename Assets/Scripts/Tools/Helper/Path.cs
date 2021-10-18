
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    //TODO: Primitive obsession, convert List<Vector3Int> to Path object
    public static Vector3 LerpPath(Level level, List<Vector3Int> path, float lerpValue)
    {
        lerpValue = Math.Min(Math.Max(lerpValue, 0), 1);
        var nodePosition = lerpValue * (path.Count - 1);

        var start = level.LevelGrid.GetCellCenterWorld(path[(int)Math.Floor(nodePosition)]);
        var end = level.LevelGrid.GetCellCenterWorld(path[(int) Math.Ceiling(nodePosition)]);
        var truePosition = Vector3.Lerp(start, end, (float)(nodePosition - Math.Floor(nodePosition)));

        return truePosition;
    }
}
