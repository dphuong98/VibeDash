
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using _Main.Game.Interfaces;
using PathCreation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum PlayerState
{
    Idle,
    Moving,
    BridgeMoving,
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
    [Header("Game")]
    [SerializeField] private InputController inputController;
    [SerializeField] private Level level;
    [SerializeField] private TileStack tileStack;
    [SerializeField] private Transform modelPivot;

    [Space]
    [Header("Animation")]
    [SerializeField] private AnimatorController modelAnimationController;
    [SerializeField] private Animator playerAnimation;
    [SerializeField] private Animator playerModelAnimation;

    [Space]
    [Header("BridgeMovement")]
    [SerializeField] private PathCreator pathCreator;
    
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

    private const float bridgeSpacing = 1.1f;
    private float bridgeDistance;
    private Bridge currentBridge;

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

        DestroyPlayerModel();
    }
    
    private void SpawnPlayerModel()
    {
        var defaultModelName = "Mousey";
        var modelPath = System.IO.Path.Combine(ResourcePaths.PlayerModelFolder, PlayerPrefs.GetString(SaveDataKeys.PlayerModelName));
        var model = Resources.Load<GameObject>(modelPath);
        if (!model) model = Resources.Load<GameObject>(System.IO.Path.Combine(ResourcePaths.PlayerModelFolder, defaultModelName));
        playerModel = Instantiate(model, modelPivot);
        playerModelAnimation = playerModel.GetComponent<Animator>();
        playerModelAnimation.runtimeAnimatorController = modelAnimationController;
        playerModelAnimation.applyRootMotion = false;
    }
    
    private void DestroyPlayerModel()
    {
        if (playerModel) Destroy(playerModel);
    }

    private void FixedUpdate()
    {
        switch (CurrentState)
        {
            case PlayerState.Moving:
                Move();
                break;
            case PlayerState.BridgeMoving:
                BridgeMove();
                break;
            case PlayerState.Idle:
                ColliderHit();
                break;
        }
    }

    private void Move()
    {
        if (CurrentState != PlayerState.Moving) return;
        
        var levelGrid = Level.LevelGrid;
        var nextGridPos = currentGridPos + direction;
        var currentTile = Level.GetTile(currentGridPos);
        var nextTile = Level.GetTile(nextGridPos);

        //Start from currentGridPos
        if (Root.position == levelGrid.GetCellCenterWorld(currentGridPos))
        {
            if (currentTile == null ||
                nextTile != null && !nextTile.IsPassable())
            {
                SetState(PlayerState.Idle);
                return;
            }
            
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

        //Tile logic
        if (nextTile != null)
        {
            switch (nextTile.TileType)
            {
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
        }

        currentGridPos = nextGridPos;
    }

    private void BridgeMove()
    {
        bridgeDistance += Speed / 100f;
        Root.position = pathCreator.path.GetPointAtDistance(bridgeDistance, EndOfPathInstruction.Stop);;

        if (bridgeDistance > pathCreator.path.length)
        {
            var bridgeParts = currentBridge.BridgeParts;
            currentGridPos = Level.LevelGrid.WorldToCell(Root.position);
            direction = bridgeParts[bridgeParts.Count - 1] - bridgeParts[bridgeParts.Count - 2];
            SetState(PlayerState.Moving);
        }
    }

    private void ColliderHit()
    {
        var raycastHits = Physics.RaycastAll(Root.position + Vector3.up, Vector3.down);
        
        //Check WinCondition, this should be placed here after fully enter a tile
        if (raycastHits.Any(hit => hit.transform.CompareTag("FinishLine")))
        {
            SetState(PlayerState.Win);
            OnPlayerWin.Invoke();
            return;
        }
        
        //Check for bridge entry point
        if (raycastHits.Any(hit => hit.transform.CompareTag("BridgeEntry")))
        {
            var bridge = Level.GetBridge(currentGridPos);
            if (bridge != null)
            {
                var worldPath = bridge.BridgeParts.Select(s => Level.LevelGrid.GetCellCenterWorld(s)).ToList();
                var bezierPath = new BezierPath(worldPath);
                bridgeDistance = bridgeSpacing;
                currentBridge = bridge;
                pathCreator.bezierPath = bezierPath;
                SetState(PlayerState.BridgeMoving);
                return;
            }
        }
        
        //Check LoseCondition
        if (Level.GetTile(currentGridPos) == null)
        {
            SetState(PlayerState.Dead);
            OnPlayerFell.Invoke();
            return;
        }
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

        //TODO bridge start
        currentGridPos = Level.LevelGrid.WorldToCell(Root.position);
        this.direction = direction;
        
        if (Level.GetTile(currentGridPos + direction) == null) return;

        SetState(PlayerState.Moving);
    }
}
