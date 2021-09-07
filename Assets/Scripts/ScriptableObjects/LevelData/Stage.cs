using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    public bool IsClearable()
    {
        var g = tiles.GroupBy(i => i);

        if (g.Count(i => i.Key == TileType.Entrance) != 1 || g.Count(i => i.Key == TileType.Exit) != 1) return false;
        
        //Pathfinding
        
        
        return true;
    }

    public static Stage CreateStage()
    {
        var newLevel = CreateInstance<Stage>();
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
        set
        {
            if (value == TileType.Entrance || value == TileType.Exit)
            {
                if (tiles.IndexOf(value) is var tilePos && tilePos != -1) tiles[tilePos] = TileType.Wall;
            }
            
            tiles[r * size.x + c] = value;
        }
    }

    public void CopyFrom(Stage other)
    {
        size = other.size;
        tiles = new List<TileType>(other.tiles);
    }

    public List<Vector2Int> ShortestPath()
    {
        //Directed graphs with arbitrary weights without negative cycles
        var shortestPath = new List<Vector2Int>();

        //Create graph using tile logic
        if (tiles.IndexOf(TileType.Entrance) is var tmp && tmp < 0)
        {
            //Cant find entrance
            Debug.Log("Cant find an entrance");
            return null;
        }
        
        var entrancePos = new Vector2Int(tmp % size.x, tmp / size.x);
        var graph = new WeightedGraph<Vector2Int>();
        MapNode(ref graph, entrancePos);
        graph.PrintGraph();

        //Apply Bellman–Ford algorithm to find the 
        
        return shortestPath;
    }

    private void MapNode(ref WeightedGraph<Vector2Int> graph, Vector2Int node)
    {
        var direction = Vector2Int.up;

        if (!this[node.x, node.y].IsWalkable()) return;

        do
        {
            if (this.TryMove(node, direction, out var destination, out var weight))
            {
                if (graph.Exist(node, destination)) return;

                graph.AddDirected(node, destination, weight);
                MapNode(ref graph, destination);
            }
            
            direction.RotateClockwise();
        } while (direction != Vector2Int.up);
    }
}