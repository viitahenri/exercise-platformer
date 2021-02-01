using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationaryState : State
{
    public StationaryState(Player player) : base(player)
    {
    }

    public override void Enter()
    {
        _player.Animation.Play(Player.LAND_ANIMATION_NAME);
    }
    
    public override void FixedUpdate()
    {
        
    }

    public override void Exit()
    {

    }

    public override void HandleInput()
    {
        if (_player.IsGrounded())
        {
            _player.CurrentDirection = Direction.Right;
            _player.StateMachine.ChangeState(_player.StateMachine.runningState);
        }
    }
}