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
        
        [CustomEditor(typeof(InfoBrush))]
        public class InfoBrushEditor : BasePrefabBrushEditor
        {
            /// <summary>
            /// Callback for painting the inspector GUI for the PrefabBrush in the Tile Palette.
            /// The PrefabBrush Editor overrides this to have a custom inspector for this Brush.
            /// </summary>
            public override void OnPaintInspectorGUI()
            {
                const string eraseAnyObjectsTooltip =
                    "If true, erases any GameObjects that are in a given position " +
                    "within the selected layers with Erasing. " +
                    "Otherwise, erases only GameObjects that are created " +
                    "from owned Prefab in a given position within the selected layers with Erasing.";

                base.OnPaintInspectorGUI();

                m_SerializedObject.UpdateIfRequiredOrScript();

                m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }
        
        [MenuItem("Assets/Create/2D/Brushes/Info Brush")]
        static void CreatePrefabBrush()
        {
            ProjectWindowUtil.CreateAsset(ScriptableObject.CreateInstance<InfoBrush>(), "New Info Brush.asset");
        }
    }
}
