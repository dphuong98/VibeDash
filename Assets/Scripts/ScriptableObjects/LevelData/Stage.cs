using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Stage : ScriptableObject
{
    [SerializeField, HideInInspector] private Vector2Int size = new Vector2Int(5, 5);
    [SerializeField, HideInInspector] private List<TileType> tiles = new List<TileType>();

    public Vector2Int Size => size;

    private void Init()
    {
        for (var i = 0; i < size.x * size.y; i++)
            tiles.Add(TileType.Wall);
    }

    public static Stage CreateStage()
    {
        var newLevel = ScriptableObject.CreateInstance<Stage>();
        newLevel.Init();
        return newLevel;
    }

    public void ExpandTop()
    {
        for (int r = 0; r < size.x; r++)
        {
            tiles.Insert(0, TileType.Air);
        }
        size.y++;
    }

    public void ExpandLeft()
    {
        for (int r = 0; r < size.y; r++)
        {
            tiles.Insert(r * (size.x + 1), TileType.Air);
        }
        size.x++;
    }

    public void ExpandRight()
    {
        for (int r = 0; r < size.y; r++)
        {
            tiles.Insert(size.x + r * (size.x + 1), TileType.Air);
        }
        size.x++;
    }

    public void ExpandBottom()
    {
        for (int r = 0; r < size.x; r++)
        {
            tiles.Insert(tiles.Count, TileType.Air);
        }
        size.y++;
    }

    public void CollapseTop()
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

    public void CollapseBottom()
    {
        if (size.y == 0) return;
        
        for (int r = 0; r < size.x; r++)
        {
            tiles.RemoveAt(tiles.Count - 1);
        }
        size.y--;
    }

    public TileType this[int c, int r]
    {
        get => tiles[r * size.x + c];
        set => tiles[r * size.x + c] = value;
    }
}

public enum TileType
{
    Air,
    Road,
    Wall,
}
