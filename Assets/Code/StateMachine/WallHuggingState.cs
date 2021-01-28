using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallHuggingState : State
{
    private float _timer = 0f;

    public WallHuggingState(Player player) : base(player)
    {
    }

    public override void Enter()
    {
        Debug.Log("Wallhug");
        _timer = 0f;
    }
    
    public override void Update()
    {
        _timer += Time.deltaTime;
    }

    public override void HandleInput()
    {
        if (_player.IsGrounded())
        {
            _player.StateMachine.ChangeState(_player.StateMachine.jumpingState);
        }
    }
}