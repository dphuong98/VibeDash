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

    public GameObject RoadPrefab { get; set; }
    public Transform PlayerModel => playerModel;
    public Transform TileStackRoot => tileStackRoot;
    public int StackCount { get; private set; }
    
    private const string stackLayerName = "Stacks";
    private float stackTileHeight;
    private Transform stackCube;
    
    
    public void Setup()
    {
        stackTileHeight = RoadPrefab.GetComponent<BoxCollider>().size.y;
        stackCube = Instantiate(RoadPrefab, tileStackRoot.position, Quaternion.identity, tileStackRoot).transform;
        stackCube.gameObject.layer = LayerMask.NameToLayer(stackLayerName);
    }

    public void CleanUp()
    {
        Destroy(stackCube.gameObject);
    }

    public void IncreaseStack()
    {
        StackCount++; stackCountText.text = StackCount.ToString();
        playerModel.position += new Vector3 {y = stackTileHeight};
        stackCube.position = new Vector3(stackCube.position.x, stackTileHeight * StackCount / 2, stackCube.position.z);
        stackCube.localScale = new Vector3(1, StackCount, 1);
    }

    public void DecreaseStack()
    {
        StackCount--; stackCountText.text = StackCount.ToString();
        playerModel.position -= new Vector3 {y = stackTileHeight};
        stackCube.position = new Vector3(stackCube.position.x, stackTileHeight * StackCount / 2, stackCube.position.z);
        stackCube.localScale = new Vector3(1, StackCount, 1);
    }
}
