
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using _Main.Game.Interfaces;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum PlayerState
{
    Idle,
    Moving,
    Win,
    Dead,
}

public interface IPlayer: IBasicObject, SimpleStateMachine<PlayerState>
{
    Transform Root { get; }
    IInputController InputController { get; }
    ILevel Level { get; }
    ITileStack TileStack { get; }
    
    UnityEvent OnPlayerWin { get; set; }
    UnityEvent OnPlayerFell { get; set; }
    
    void HandleInput(Vector3Int direction);
}

public class Player : MonoBehaviour, IPlayer
{
    [SerializeField] private InputController inputController;
    [SerializeField] private Level level;
    [SerializeField] private TileStack tileStack;
    [SerializeField] private Transform modelPivot;

    [Space]
    [SerializeField] private AnimatorController modelAnimationController;
    [SerializeField] private Animator playerAnimation;
    [SerializeField] private Animator playerModelAnimation;
    
    public Transform Root => transform;
    public IInputController InputController => inputController;
    public ILevel Level => level;
    public ITileStack TileStack => tileStack;

    public UnityEvent OnPlayerWin { get; set; } = new UnityEvent();
    public UnityEvent OnPlayerFell { get; set; } = new UnityEvent();
    
    private GameObject playerModel;
    private const int Speed = 30;
    private Vector3Int currentGridPos;
    private Vector3Int direction;

    public PlayerState CurrentState { get; private set; }

    public void Setup()
    {
        InputController.OnSwipeDirection.AddListener(HandleInput);
        InputController.Setup();
        
        TileStack.Setup();

        SpawnPlayerModel();
        SetState(PlayerState.Idle);
    }

    public void CleanUp()
    {
        InputController.OnSwipeDirection.RemoveListener(HandleInput);
        TileStack.CleanUp();
        
        InputController.CleanUp();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void SpawnPlayerModel()
    {
        var prefabPath = System.IO.Path.Combine(ResourcePaths.PlayerModelFolder,
            PlayerPrefs.GetString(SaveDataKeys.PlayerModelName));
        var playerPrefab = Resources.Load<GameObject>(prefabPath);
        playerModel = Instantiate(playerPrefab, modelPivot.position, Quaternion.Euler(0, 150, 0), modelPivot);
        playerModelAnimation = playerModel.GetComponent<Animator>();
        playerModelAnimation.runtimeAnimatorController = modelAnimationController;
        playerModelAnimation.applyRootMotion = false;
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
                (!nextTile.IsPassable() || nextTile.TileType == TileType.Bridge && !nextTile.IsTraversed() && TileStack.StackCount == 0))
            {
                SetState(PlayerState.Idle);
                return;
            }
            
            var currentTile = Level.GetTile(currentGridPos);
            if (currentTile.HasRoad() && !currentTile.IsTraversed())
            {
                TileStack.IncreaseStack();
            }
            
            currentTile.OnExit();
        }
        
        //Reposition
        var moveDistance = Speed * Time.deltaTime * (Vector3) direction;
        FloatExt.Swap(ref moveDistance.y, ref moveDistance.z);
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
        var raycastHits = Physics.RaycastAll(Root.position + 3 * Vector3.up, Vector3.down);
        if (raycastHits.Any(hit => hit.transform.CompareTag("FinishLine"))) //TODO raycast all
        {
            SetState(PlayerState.Win);
            OnPlayerWin.Invoke();
            return;
        }
        
        //Check LoseCondition
        if (nextTile == null)
        {
            SetState(PlayerState.Dead);
            OnPlayerFell.Invoke();
            return;
        }
        
        //Tile logic
        switch (nextTile.TileType)
        {
            case TileType.Bridge:
                if (!nextTile.IsTraversed()) TileStack.DecreaseStack();
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
                    SetState(PlayerState.Idle);
                }
                break;
            case TileType.Stop:
                SetState(PlayerState.Idle);
                break;
        }
        
        nextTile.OnEnter();
        currentGridPos = nextGridPos;
    }

    private void SetState(PlayerState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case PlayerState.Moving:
                playerAnimation.SetBool("IsMoving", true);
                playerModelAnimation.SetBool("IsMoving", true);
                break;
            case PlayerState.Idle:
                playerAnimation.SetBool("IsMoving", false);
                playerAnimation.SetFloat("xAxis", direction.x);
                playerAnimation.SetFloat("yAxis", direction.y);
                playerModelAnimation.SetBool("IsMoving", false);
                break;
        }
    }
    
    public void HandleInput(Vector3Int direction)
    {
        //Check movable
        if (CurrentState == PlayerState.Moving || CurrentState == PlayerState.Dead) return;

        currentGridPos = Level.LevelGrid.WorldToCell(Root.position);
        this.direction = direction;
        
        if (Level.GetTile(currentGridPos + direction) == null) return;

        SetState(PlayerState.Moving);
    }
}
