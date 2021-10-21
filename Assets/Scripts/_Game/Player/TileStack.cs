using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileStack : MonoBehaviour
{
    [SerializeField]
    private Player player;
    private int currentStackCount;
    private float stackFloorHeight;
    private float stackTileHeight;
    private int stackLayerMask;

    private void Start()
    {
        stackLayerMask = LayerMask.NameToLayer("Stacks");
        stackFloorHeight = player.Level.LevelGrid.GetCellCenterWorld(Vector2Int.one).y;
        var stackTileMesh = LevelLoader.PrefabPack.RoadPrefab.GetComponentInChildren<MeshFilter>();
        stackTileHeight = stackTileMesh.sharedMesh.bounds.size.y * stackTileMesh.transform.localScale.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //TODO stack change by 2+ tile
        var newStackCount = player.StackPoints;
        if (currentStackCount < newStackCount)
            IncreaseStack();
        if (currentStackCount > newStackCount)
            DecreaseStack();
    }
    
    private void IncreaseStack()
    {
        player.transform.position += new Vector3(0, stackTileHeight, 0);
        var tilePlacementPos = GetPlayerPosition();
        tilePlacementPos.y = stackFloorHeight;
        LevelLoader.PlaceTile(tilePlacementPos, TileType.Road, transform).layer = stackLayerMask;
        currentStackCount++;
    }

    private void DecreaseStack()
    {
        player.transform.position -= new Vector3(0, stackTileHeight, 0);
        Destroy(transform.GetChild(transform.childCount - 1).gameObject);
        currentStackCount--;
    }

    private Vector3 GetPlayerPosition()
    {
        return player.transform.position;
    }
}
