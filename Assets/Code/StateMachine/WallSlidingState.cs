using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WallSlidingState : State
{
    private float _timer = 0f;

    public WallSlidingState(Player player) : base(player) { }

    public override void Enter()
    {
        Debug.Log("Wallslide");
        _timer = 0f;
    }

    public override void Update()
    {
        _timer += Time.deltaTime;
    }

    public override void FixedUpdate()
    {
        if (_player.IsGrounded())
        {
            if (_player.IsTouchingWall())
                _player.StateMachine.ChangeState(_player.StateMachine.wallHuggingState);
            else
                _player.StateMachine.ChangeState(_player.StateMachine.runningState);
        }
    }

    public override void HandleInput()
    {
        if (_player.IsWallSliding())
        {
            _player.StateMachine.ChangeState(_player.StateMachine.jumpingState);
        }
    }
}