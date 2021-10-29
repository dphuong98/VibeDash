using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITileStack: IBasicObject
{
    GameObject RoadPrefab { get; set; }
    Transform PlayerModel { get; }
    Transform TileStackRoot { get; }
    int StackCount { get; }

    void IncreaseStack();
    void DecreaseStack();
}

public class TileStack : MonoBehaviour, ITileStack
{
    [SerializeField] private const string stackLayerName = "Stacks";
    [SerializeField] private Transform playerModel;
    [SerializeField] private Transform tileStackRoot;

    public GameObject RoadPrefab { get; set; }
    public Transform PlayerModel => playerModel;
    public Transform TileStackRoot => tileStackRoot;
    public int StackCount { get; private set; }
    
    private float stackFloorHeight;
    private float stackTileHeight;
    
    
    public void Setup()
    {
        stackFloorHeight = tileStackRoot.position.y;
        stackTileHeight = RoadPrefab.GetComponent<BoxCollider>().size.y;
    }

    public void CleanUp()
    {
        
    }

    public void IncreaseStack()
    {
        StackCount++;
        playerModel.position += new Vector3 {y = stackTileHeight};
        var tilePlacementPos = tileStackRoot.position;
        tilePlacementPos.y = stackFloorHeight + StackCount * stackTileHeight;
        Instantiate(RoadPrefab, tilePlacementPos, Quaternion.identity, tileStackRoot).layer = LayerMask.NameToLayer(stackLayerName);
    }

    public void DecreaseStack()
    {
        StackCount--;
        playerModel.position -= new Vector3 {y = stackTileHeight};
        Destroy(TileStackRoot.GetChild(TileStackRoot.childCount - 1).gameObject);
    }
}
