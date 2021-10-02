using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct Portal
{
    [FormerlySerializedAs("Entrance")] public Vector2Int Blue;
    [FormerlySerializedAs("Exit")] public Vector2Int Orange;

    public Portal(Vector2Int portalBlue, Vector2Int portalOrange)
    {
        this.Blue = portalBlue;
        this.Orange = portalOrange;
    }
}

[Serializable]
public class Stage : ScriptableObject, IInit, ICopiable<Stage>
{
    //TODO purge vector2int in this class
    [SerializeField, HideInInspector] private Vector2Int size = new Vector2Int(5, 5);
    [SerializeField, HideInInspector] private List<TileType> tiles = new List<TileType>();
    [SerializeField, HideInInspector] private List<Portal> portalPairs = new List<Portal>();

    public Vector2Int Size => size;
    public List<Portal> PortalPairs => new List<Portal>(portalPairs);

    public void Init()
    {
        for (var i = 0; i < size.x * size.y; i++)
            tiles.Add(TileType.Wall);
    }

    public bool IsOnBorder(Vector2Int tilePos)
    {
        return 0 == tilePos.x || tilePos.x == size.x - 1 || 0 == tilePos.y || tilePos.y == size.y - 1;
    }

    public TileType this[int c, int r]
    {
        get => tiles[r * size.x + c];
        set
        {
            // //Destroy portals on tile position
            foreach (var portal in portalPairs.Where(portal => portal.Blue == new Vector2Int(c, r) || portal.Orange == new Vector2Int(c, r)))
            {
                var portal_ = portal;
                portalPairs.Remove(portal);
                tiles[portal_.Blue.y * size.x + portal_.Blue.x] = TileType.Wall;
                if (portal_.Orange != -Vector2Int.one)
                    tiles[portal_.Orange.y * size.x + portal_.Orange.x] = TileType.Wall;
                break;
            }
            
            //Ensure that their could be only one entrance or exit
            if (value == TileType.Entrance || value == TileType.Exit)
            {
                if (tiles.IndexOf(value) is var tilePos && tilePos != -1) tiles[tilePos] = TileType.Wall;
            }

            if (value == TileType.PortalBlue)
            {
                OpenPortal(new Vector2Int(c, r));
            }
            
            if (value == TileType.PortalOrange)
            {
                if (PortalPending())
                    ClosePortal(new Vector2Int(c, r));
                else return;
            }
            
            tiles[r * size.x + c] = value;
        }
    }

    public void CopyFrom(Stage other)
    {
        size = other.size;
        tiles = new List<TileType>(other.tiles);
        portalPairs = new List<Portal>(other.portalPairs);
    }

    public void OpenPortal(Vector2Int portalEntrance)
    {
        CancelPortal();
        portalPairs.Add(new Portal(portalEntrance, -Vector2Int.one));
    }

    public void CancelPortal()
    {
        if (PortalPending())
        {
            var item1 = portalPairs.Last().Blue;
            tiles[item1.y * size.x + item1.x]  = TileType.Wall;
            portalPairs.RemoveAt(portalPairs.Count - 1);
        }
    }
    
    public void ClosePortal(Vector2Int portalExit)
    {
        if (PortalPending())
        {
            var item1 = portalPairs.Last().Blue;
            portalPairs.Add(new Portal(item1, portalExit));
            portalPairs.RemoveAt(portalPairs.Count - 2);
        }
    }

    public bool PortalPending()
    {
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
    }

    public void CollapseLeft()
    {
        if (size.x == 0) return;
        
        for (int r = size.y - 1; r >= 0; r--)
        {
            tiles.RemoveAt(r * size.x);
        }
        size.x--;
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
}