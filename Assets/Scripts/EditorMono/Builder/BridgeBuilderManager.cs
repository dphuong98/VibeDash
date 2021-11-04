using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class BridgeBuilderManager : MonoBehaviour
{
    public Color bridgeColor = Color.yellow;
    
    private GameObject bridgeBuilderPrefab;

    private void OnEnable()
    {
        bridgeBuilderPrefab = Resources.Load<GameObject>("Prefabs/Editor/BridgeBuilder");
    }

    public void DrawBridges(Grid levelGrid, List<Bridge> bridges)
    {
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }

        foreach (var bridge in bridges)
        {
            var bridgeBuilder = Instantiate(bridgeBuilderPrefab, transform).GetComponent<BridgeBuilder>();
            bridgeBuilder.SetBridge(bridge.BridgeParts.Select(levelGrid.GetCellCenterWorld));
            bridgeBuilder.SetBezierColor(bridgeColor);
        }
    }
}
