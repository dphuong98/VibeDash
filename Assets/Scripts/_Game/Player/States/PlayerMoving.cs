﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using UnityEngine;

public class PlayerMoving : StateMachineBehaviour
{
    private float elapsedTime = 0f;
    private const float speed = 12f; //tile/s
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private List<Vector3Int> path;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        elapsedTime = 0f;
        InputManager.DisableInput();
        path = animator.GetComponent<Player>().Path;
        // playerMover = new PlayerMover(animator.GetComponent<PathGenerator>().Path,
        //     );
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        elapsedTime += Time.deltaTime;
        var totalTime = path.Count / speed;
        var lerpValue = elapsedTime / totalTime;
        animator.transform.position = PathLerp.LerpPath(animator.GetComponent<Player>().Level, path, lerpValue);

        if (lerpValue >= 1)
        {
            animator.GetComponent<Player>().ResetPath();
            animator.SetBool(IsMoving, false);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}