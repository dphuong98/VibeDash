
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITile
{
    TileType TileType { get; }
}

public class Tile : MonoBehaviour, ITile
{
    public TileType TileType { get; }
}
