using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using _Main.Game.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum PlayerState
{
    Idle,
    Moving,
    Dead
}

public interface IPlayer: IBasicObject, SimpleStateMachine<PlayerState>
{
    Transform Root { get; }
    IInputController InputController { get; }
    ILevel Level { get; }
    
    UnityEvent OnWin { get; }
    UnityEvent OnLose { get; }
    
    void HandleInput(Vector3Int direction);
}

public class Player : MonoBehaviour, IPlayer
{
    [SerializeField] private InputController inputController;
    [SerializeField] private Level level;

    public Transform Root => transform;
    public IInputController InputController => inputController;
    public ILevel Level => level;

    public UnityEvent OnWin { get; }
    public UnityEvent OnLose { get; }

    private int stackCount = 0;
    private const int speed = 6;
    private Vector3Int currentGridPos;
    private Vector3Int direction;
    
    public PlayerState CurrentState { get; private set; }

    public void Setup()
    {
        InputController.Setup();

        InputController.OnSwipeDirection.AddListener(HandleInput);
        CurrentState = PlayerState.Idle;
    }

    public void CleanUp()
    {
        InputController.CleanUp();
    }

    private void Update()
    {
        DebugUI.Instance.AddText("StackCount: " + stackCount);
    }

    private void FixedUpdate()
    {
        Move();
    }

    public void HandleInput(Vector3Int direction)
    {
        //Check movable
        if (CurrentState == PlayerState.Moving || CurrentState == PlayerState.Dead) return;

        currentGridPos = Level.LevelGrid.WorldToCell(Root.position);
        this.direction = direction;
        if (Level.GetTile(currentGridPos + direction) == null) return;

        CurrentState = PlayerState.Moving;
    }

    private void Move()
    {
        if (CurrentState != PlayerState.Moving) return;
        
        var levelGrid = Level.LevelGrid;
        var nextGridPos = currentGridPos + direction;
        var nextTile = Level.GetTile(nextGridPos);

        //First step of moving from tile to tile
        if (Root.position == levelGrid.GetCellCenterWorld(currentGridPos))
        {
            if (nextTile != null && 
                (!nextTile.IsPassable() || nextTile.TileType == TileType.Bridge && !nextTile.IsTraversed() && stackCount == 0))
            {
                CurrentState = PlayerState.Idle;
                return;
            }
            
            var currentTile = Level.GetTile(currentGridPos);
            if (currentTile.HasRoad() && !currentTile.IsTraversed())
            {
                stackCount++;
            }
            
            currentTile.OnExit();
        }
        
        //Reposition
        var moveDistance = speed * Time.deltaTime * (Vector3) direction;
        if (Vector3Ext.IsCBetweenAB(levelGrid.GetCellCenterWorld(currentGridPos),
            levelGrid.GetCellCenterWorld(nextGridPos),
            Root.position + moveDistance))
        {
            //Is between start and end gridPos
            Root.position += moveDistance;
            return;
        }

        //Arrived at endGridPos
        Root.position = levelGrid.GetCellCenterWorld(nextGridPos);

        //Check WinCondition
        
        //Check LoseCondition
        if (nextTile == null)
        {
            CurrentState = PlayerState.Dead;
            //OnLose.Invoke();
            return;
        }
        
        //Tile logic
        switch (nextTile.TileType)
        {
            case TileType.Bridge:
                if (!nextTile.IsTraversed()) stackCount--;
                var bridge = Level.GetBridge(nextGridPos, direction);
                direction = bridge.bridgeParts[1] - bridge.bridgeParts[0];
                break;
            case TileType.Push:
                direction = Level.GetTileDirection(nextGridPos);
                break;
            case TileType.Corner:
                var upVector = Level.GetTileDirection(nextGridPos);
                var rightVector = upVector.RotateClockwise();
                if (direction == -upVector) direction = rightVector;
                if (direction == -rightVector) direction = upVector;
                break;
            case TileType.PortalBlue: case TileType.PortalOrange:
                var otherPortal = Level.GetOtherPortal(nextGridPos);
                if (otherPortal != null)
                {
                    Root.position = levelGrid.GetCellCenterWorld((Vector3Int) otherPortal);
                    CurrentState = PlayerState.Idle;
                }
                break;
            case TileType.Stop:
                CurrentState = PlayerState.Idle;
                break;
        }
        
        nextTile.OnEnter();
        currentGridPos = nextGridPos;
    }
}
