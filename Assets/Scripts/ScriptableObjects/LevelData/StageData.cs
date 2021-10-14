using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.WSA;

[Serializable]
public struct Portal
{
    public Vector2Int Blue;
    public Vector2Int Orange;

    public Portal(Vector2Int portalBlue, Vector2Int portalOrange)
    {
        this.Blue = portalBlue;
        this.Orange = portalOrange;
    }
}

[Serializable]
public class TileDirection : SerializableDictionary<Vector2Int, Vector2Int>
{
    public TileDirection() : base() {}
    public TileDirection(TileDirection otherTileDirections) : base(otherTileDirections) { }
    public TileDirection(IDictionary<Vector2Int, Vector2Int> dictionary) : base(dictionary) {}
}

[Serializable]
public class StageData : ScriptableObject, IInit, ICopiable<StageData>
{
    [SerializeField, HideInInspector] private Vector2Int size = new Vector2Int(5, 5);
    [SerializeField, HideInInspector] private List<TileType> tiles = new List<TileType>();
    [SerializeField, HideInInspector] private List<Vector2Int> solution = new List<Vector2Int>();
    [SerializeField, HideInInspector] private List<Portal> portalPairs = new List<Portal>();
    [SerializeField, HideInInspector] private TileDirection tileDirections = new TileDirection();

    public Vector2Int Size => size;
    public List<Vector2Int> Solution => new List<Vector2Int>(solution);
    public List<Portal> PortalPairs => new List<Portal>(portalPairs);
    public Dictionary<Vector2Int, Vector2Int> TileDirections => new Dictionary<Vector2Int, Vector2Int>(tileDirections);

    public void Init()
    {
        for (var i = 0; i < size.x * size.y; i++)
            tiles.Add(TileType.Wall);
    }

    public bool IsOnBorder(Vector2Int tilePos)
    {
        return 0 == tilePos.x || tilePos.x == size.x - 1 || 0 == tilePos.y || tilePos.y == size.y - 1;
    }

    public bool IsValidTile(Vector2Int tilePos)
    {
        return 0 <= tilePos.x && tilePos.x < size.x &&
               0 <= tilePos.y && tilePos.y < size.y;
    }

    public TileType this[int c, int r]
    {
        get => tiles[r * size.x + c];
        set
        {
           Unset(c,r);
           Set(c, r, value);
           tiles[r * size.x + c] = value;
        }
    }

    private void Unset(int c, int r)
    {
        //Destroy portals on tile position
        foreach (var portal in portalPairs.Where(portal => portal.Blue == new Vector2Int(c, r) || portal.Orange == new Vector2Int(c, r)))
        {
            if (IsValidTile(portal.Blue))
                tiles[portal.Blue.y * size.x + portal.Blue.x] = TileType.Wall;
            if (IsValidTile(portal.Orange))
                tiles[portal.Orange.y * size.x + portal.Orange.x] = TileType.Wall;
            portalPairs.Remove(portal);
            break;
        }
        
        //Destroy tile direction
        tileDirections.Remove(new Vector2Int(c, r));
    }

    private void Set(int c, int r, TileType value)
    {
        switch (value)
        {
            case TileType.Entrance: case TileType.Exit:
                //Ensure that their could be only one entrance or exit
                if (tiles.IndexOf(value) is var tilePos && tilePos != -1) tiles[tilePos] = TileType.Wall;
                break;
            case TileType.PortalBlue:
                OpenPortal(new Vector2Int(c, r));
                break;
            case TileType.PortalOrange:
                ClosePortal(new Vector2Int(c, r));
                break;
            case TileType.Push: case TileType.Corner:
                tileDirections.Add(new Vector2Int(c, r), Vector2Int.up);
                break;
        }
    }

    public void SetTileDirection(Vector2Int position, Vector2Int direction)
    {
        if (tileDirections.TryGetValue(position, out _))
        {
            tileDirections[position] = direction;
        }
    }

    public void CopyFrom(StageData other)
    {
        size = other.size;
        tiles = new List<TileType>(other.tiles);
        solution = new List<Vector2Int>(other.solution);
        portalPairs = new List<Portal>(other.portalPairs);
        tileDirections = new TileDirection(other.tileDirections);
    }

    public void OpenPortal(Vector2Int portalEntrance)
    {
        CancelPortal();
        portalPairs.Add(new Portal(portalEntrance, -Vector2Int.one));
    }

    public void CancelPortal()
    {
        if (!PortalPending(out var portal)) return;
        var bluePortal = portal.Blue;
        tiles[bluePortal.y * size.x + bluePortal.x]  = TileType.Wall;
        portalPairs.RemoveAt(portalPairs.Count - 1);
    }
    
    public void ClosePortal(Vector2Int portalExit)
    {
        if (!PortalPending(out var portal)) return;
        portal.Orange = portalExit;
        portalPairs.RemoveAt(portalPairs.Count - 1);
        portalPairs.Add(portal);
    }

    public bool PortalPending(out Portal portal)
    {
        portal = portalPairs.Any() ? portalPairs.Last() : default;
        return portalPairs.Any() &&
               portalPairs.Last().Orange == -Vector2Int.one;
    }

    public void ExpandBottom()
    {
        for (int r = size.x - 1; r >= 0; r--)
        {
            tiles.Insert(0, TileType.Wall);
        }
        size.y++;
        
        for (int r = size.x - 1; r >= 0; r--)
        {
            if (this[r, 1] == TileType.Exit)
            {
                this[r, 0] = TileType.Exit;
                break;
            }
        }
        ShiftSpecialTiles(Vector2Int.up);
    }

    public void ExpandLeft()
    {
        for (int r = 0; r < size.y; r++)
        {
            tiles.Insert(r * (size.x + 1), TileType.Wall);
        }
        size.x++;
        
        for (int r = 0; r < size.y; r++)
        {
            if (this[1, r] == TileType.Exit)
            {
                this[0, r] = TileType.Exit;
                break;
            }
        }
        ShiftSpecialTiles(Vector2Int.right);
    }

    public void ExpandRight()
    {
        for (int r = 0; r < size.y; r++)
        {
            tiles.Insert(size.x + r * (size.x + 1), TileType.Wall);
        }
        size.x++;

        for (int r = 0; r < size.y; r++)
        {
            if (this[size.x - 2, r] == TileType.Exit)
            {
                this[size.x - 1, r] = TileType.Exit;
                break;
            }
        }
    }

    public void ExpandTop()
    {
        for (int r = 0; r < size.x; r++)
        {
            tiles.Insert(tiles.Count, TileType.Wall);
        }
        size.y++;

        for (int r = 0; r < size.x; r++)
        {
            if (this[r, size.y - 2] == TileType.Exit)
            {
                this[r, size.y - 1] = TileType.Exit;
            }
        }
    }

    public void CollapseBottom()
    {
        if (size.y == 0) return;
        
        for (int r = 0; r < size.x; r++)
        {
            tiles.RemoveAt(0);
        }
        size.y--;
        ShiftSpecialTiles(Vector2Int.down);
    }

    public void CollapseLeft()
    {
        if (size.x == 0) return;
        
        for (int r = size.y - 1; r >= 0; r--)
        {
            tiles.RemoveAt(r * size.x);
        }
        size.x--;
        ShiftSpecialTiles(Vector2Int.left);
    }

    public void CollapseRight()
    {
        if (size.x == 0) return;
        
        for (int r = size.y - 1; r >= 0; r--)
        {
            tiles.RemoveAt(r * size.x + size.x - 1);
        }
        size.x--;
    }

    public void CollapseTop()
    {
        if (size.y == 0) return;
        
        for (int r = 0; r < size.x; r++)
        {
            tiles.RemoveAt(tiles.Count - 1);
        }
        size.y--;
    }

    private void ShiftSpecialTiles(Vector2Int direction)
    {
        //Shift in direction
        for (var i = 0; i < portalPairs.Count; i++)
        {
            var shiftedPortal = new Portal(portalPairs[i].Blue + direction, portalPairs[i].Orange + direction);
            portalPairs[i] = shiftedPortal;
        }
        
        foreach (var key in tileDirections.Keys.ToList())
        {
            tileDirections.Add(key + direction, tileDirections[key]);
            tileDirections.Remove(key);
        }
        
        //Purge invalid tiles
        portalPairs.RemoveAll(s => !IsValidTile(s.Blue) || !IsValidTile(s.Orange));
        foreach (var key in tileDirections.Keys.Where(s => !IsValidTile(s)))
        {
            tileDirections.Remove(key);
        }
    }
    
    public Vector2Int GetEntrance()
    {
        if (tiles.IndexOf(TileType.Entrance) is var tilePos && tilePos != -1)
        {
            return new Vector2Int(tilePos % size.x, tilePos / size.x);
        }

        return -Vector2Int.one;
    }
    
    public Vector2Int GetExit()
    {
        if (tiles.IndexOf(TileType.Exit) is var tilePos && tilePos != -1)
        {
            return new Vector2Int(tilePos % size.x, tilePos / size.x);
        }

        return -Vector2Int.one;
    }

    public void GenerateSolution()
    {
        solution = Pathfinding.GetSolution(this);
    }

    public bool IsEntranceStage()
    {
        return !IsOnBorder(GetEntrance());
    }
}