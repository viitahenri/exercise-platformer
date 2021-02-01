using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunningState : State
{

    public RunningState(Player player) : base(player)
    {
        
    }

    public override void Enter()
    {
        // Debug.Log($"Running to {_player.CurrentDirection}");
        _player.Animation.Play(Player.LAND_ANIMATION_NAME);
    }
    
    public override void FixedUpdate()
    {
        var dir = _player.CurrentDirection == Direction.Left ? Vector2.left : Vector2.right;
        _player.Rigidbody.velocity = dir * _player.MoveSpeed;

        if (_player.IsTouchingWall())
        {
            _player.StateMachine.ChangeState(_player.StateMachine.wallHuggingState);
        }

        if (!_player.IsGrounded())
        {
            _player.StateMachine.ChangeState(_player.StateMachine.fallingState);
        }
    }

    public override void Exit()
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