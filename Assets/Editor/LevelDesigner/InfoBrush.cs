using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;

namespace UnityEditor.Tilemaps
{
    [CustomGridBrush(false, true, false, "Info Brush")]
    public class InfoBrush : BasePrefabBrush
    {
        public override void Paint(GridLayout grid, GameObject brushTarget, Vector3Int position)
        {
            var objectsInCell = GetObjectsInCell(grid, brushTarget.transform, position);

            var text = "";
            foreach (var item in objectsInCell)
                text += item.name + "; ";

            Debug.Log(text);
        }
    }
}
