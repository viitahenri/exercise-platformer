using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallHuggingState : State
{
    public WallHuggingState(Player player) : base(player)
    {
    }

    public override void Enter()
    {
        Debug.Log("Wallhug");

    }
    
    public override void FixedUpdate()
    {

    }

    public override void HandleInput()
    {
        if (_player.IsGrounded())
        {
            _player.StateMachine.ChangeState(_player.StateMachine.jumpingState);
        }
    }
}