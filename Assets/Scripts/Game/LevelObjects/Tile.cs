
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITile
{
    TileType TileType { get; }

    bool IsPassable();
    bool HasRoad();
    bool IsTraversed();

    void OnEnter();
    void OnExit();
}

public class Tile : MonoBehaviour, ITile
{
    [SerializeField] private TileType tileType;
    [SerializeField] private GameObject RoadGameObject;
    [SerializeField] private GameObject BlankGameObject;
        
    public TileType TileType => tileType;

    private bool traversed;

    public bool IsPassable()
    {
        return TileType != TileType.Air && TileType != TileType.Wall;
    }

    public bool HasRoad()
    {
        return TileType == TileType.Corner || TileType == TileType.Road;
    }

    public bool IsTraversed()
    {
        return traversed;
    }

    public void OnEnter()
    {
        if (TileType == TileType.Blank && !IsTraversed())
        {
            BlankGameObject.SetActive(true);
            traversed = true;
        }
    }

    public void OnExit()
    {
        if (HasRoad() && !IsTraversed())
        {
            BlankGameObject.SetActive(true);
            RoadGameObject.SetActive(false);
        }
        traversed = true;
    }
}
