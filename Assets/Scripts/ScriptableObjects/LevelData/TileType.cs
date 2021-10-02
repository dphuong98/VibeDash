using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Entrance,
    Exit,
    Air,
    Road,
    Wall,
    Stop,
    PortalBlue,
    PortalOrange
}

public static class TileTypeExt {
    public static bool IsWalkable(this TileType type)
    {
        return type == TileType.Entrance || type == TileType.Road || type == TileType.Exit;
    }
}