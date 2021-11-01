using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private Text stackCountText;
    [SerializeField] private Transform playerModel;
    [SerializeField] private Transform tileStackRoot;
    [SerializeField] private Transform stackPivot;

    public GameObject RoadPrefab { get; set; }
    public Transform PlayerModel => playerModel;
    public Transform TileStackRoot => tileStackRoot;
    public int StackCount { get; private set; }

    private Transform stackCube;
    private float tileStackScale = 2;
    private const string stackLayerName = "Stacks";
    private float stackTileHeight;


    public void Setup()
    {
        stackTileHeight = RoadPrefab.GetComponent<BoxCollider>().size.y;
        stackCube = Instantiate(RoadPrefab, tileStackRoot.position, Quaternion.identity, stackPivot).transform;
        stackCube.gameObject.layer = LayerMask.NameToLayer(stackLayerName);
    }

    public void CleanUp()
    {
        Destroy(stackCube.gameObject);
    }

    public void IncreaseStack()
    {
        StackCount++; stackCountText.text = StackCount.ToString();
        playerModel.position += new Vector3 {y = tileStackScale * stackTileHeight}; 
        stackCube.position = new Vector3(stackCube.position.x, tileStackScale * stackTileHeight * StackCount / 2, stackCube.position.z);
        stackCube.localScale = new Vector3(1, tileStackScale * StackCount, 1); //TODO remove hardcode
    }

    public void DecreaseStack()
    {
        StackCount--; stackCountText.text = StackCount.ToString();
        playerModel.position -= new Vector3 {y = tileStackScale * stackTileHeight};
        stackCube.position = new Vector3(stackCube.position.x, tileStackScale * stackTileHeight * StackCount / 2, stackCube.position.z);
        stackCube.localScale = new Vector3(1, tileStackScale * StackCount, 1);
    }
    
    // public void LookAt(Vector3Int direction)
    // {
    //     var angle = 0;
    //     if (direction == Vector3Int.up) angle = -90;
    //     if (direction == Vector3Int.right) angle = 0;
    //     if (direction == Vector3Int.down) angle = 90;
    //     if (direction == Vector3Int.left) angle = 180;
    //     
    //     tileStackRoot.eulerAngles = new Vector3(0, angle, 0);
    // } TODO scrap this
}
