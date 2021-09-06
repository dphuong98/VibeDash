using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PrefabGrid 
{
    public static List<GameObject> GetObjectsInCell(GridLayout grid, Transform parent, Vector3Int position)
    {
        //TODO Each brush have anchor, this is default, if anchor were to change, write this code to find and use m_anchor
        var m_Anchor = new Vector3(0.5f, 0.5f, 0.5f);
        
        var results = new List<GameObject>();
        var childCount = parent.childCount;
        var anchorCellOffset = Vector3Int.FloorToInt(m_Anchor);
        var cellSize = grid.cellSize;
        anchorCellOffset.x = cellSize.x == 0 ? 0 : anchorCellOffset.x;
        anchorCellOffset.y = cellSize.y == 0 ? 0 : anchorCellOffset.y;
        anchorCellOffset.z = cellSize.z == 0 ? 0 : anchorCellOffset.z;
        for (var i = 0; i < childCount; i++)
        {
            var child = parent.GetChild(i);
            if (position == grid.WorldToCell(child.position) - anchorCellOffset)
            {
                results.Add(child.gameObject);
            }
        }
        return results;
    }
    
    public static GameObject GetObjectInCell(GridLayout grid, Transform parent, Vector3Int position)
    {
        var objects = GetObjectsInCell(grid, parent, position);
        return objects.Count == 0 ? null : objects.First();
    }
}
