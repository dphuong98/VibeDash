using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GridExt
{
    public static Vector3 GetCellCenterWorld(this Grid grid, Vector2Int gridPos)
    {
        var gridPos3 = new Vector3Int(gridPos.x, gridPos.y, 0);
        return grid.GetCellCenterWorld(gridPos3);
    }
}
