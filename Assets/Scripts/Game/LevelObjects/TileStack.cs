using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public interface ITileStack: IBasicObject
{
    GameObject RoadPrefab { get; set; }
    Transform PlayerModel { get; }
    Transform Root { get; }
    int StackCount { get; }

    void IncreaseStack();
    void DecreaseStack();
}

public class TileStack : MonoBehaviour, ITileStack
{
    [SerializeField] private Text stackCountText;
    [SerializeField] private Transform playerModel;
    [SerializeField] private Transform stackPivot;

    public GameObject RoadPrefab { get; set; }
    public Transform PlayerModel => playerModel;
    public Transform Root => transform;
    public int StackCount { get; private set; }

    private Transform stackCube;
    private const float tileStackScale = 1.5f;
    private const string stackLayerName = "Stacks";
    private float stackTileHeight;


    public void Setup()
    {
        stackTileHeight = RoadPrefab.GetComponent<BoxCollider>().size.y;
        stackCube = Instantiate(RoadPrefab, Root.position, Quaternion.identity, stackPivot).transform;
        stackCube.gameObject.layer = LayerMask.NameToLayer(stackLayerName);

        StackCount = 0;  stackCountText.text = StackCount.ToString();
    }

    public void CleanUp()
    {
        Destroy(stackCube.gameObject);

        playerModel.position = new Vector3(playerModel.position.x, 0, playerModel.position.z);
    }

    public void IncreaseStack()
    {
        StackCount++; stackCountText.text = StackCount.ToString();
        UpdateTransforms();
    }

    public void DecreaseStack()
    {
        StackCount--; stackCountText.text = StackCount.ToString();
        UpdateTransforms();
    }

    private void UpdateTransforms()
    {
        var stackCubeScale = StackCount == 0 ? tileStackScale : tileStackScale * StackCount;
        var stackCubeHeight = stackCubeScale * stackTileHeight;
        playerModel.position = new Vector3(playerModel.position.x, stackCubeHeight, playerModel.position.z);
        stackCube.position = new Vector3(stackCube.position.x, stackCubeHeight / 2, stackCube.position.z);
        stackCube.localScale = new Vector3(1, stackCubeScale, 1);
    }
}
