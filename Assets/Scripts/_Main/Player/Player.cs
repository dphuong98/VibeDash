using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Main.Game.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public enum PlayerState
{
    Idle,
    Moving
}

public interface IPlayer: IBasicObject, SimpleStateMachine<PlayerState>
{
    Transform Root { get; }

    void HandleInput(Vector3Int direction);
}

public class Player : MonoBehaviour, IPlayer
{
    [SerializeField] private Vector3Int direction;
    [SerializeField] private InputController inputController;
    [SerializeField] private Level level;

    public Transform Root => transform;
    public PlayerState CurrentState { get; private set; }

    public void Setup()
    {
        inputController.OnSwipeDirection.AddListener(HandleInput);
        CurrentState = PlayerState.Idle;
    }

    public void CleanUp()
    {
        CurrentState = PlayerState.Idle;
    }

    private void FixedUpdate()
    {
        Move();
    }

    public void HandleInput(Vector3Int direction)
    {
        //Check movable
        if (CurrentState == PlayerState.Moving) return;

        this.direction = direction;
        CurrentState = PlayerState.Moving;
    }

    private void Move()
    {
        if (CurrentState != PlayerState.Moving) return;
        
        
    }
}
