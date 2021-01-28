using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JumpingState : State
{
    private List<ContactPoint2D> _contacts = new List<ContactPoint2D>();
    private ContactFilter2D _filter;
    private const float GracePeriod = .2f;
    private float _graceTimer = 0f;

    public JumpingState(Player player) : base(player)
    {
    }

    public override void Enter()
    {
        // stop falling to make jumps equally powerful even if sliding down a wall
        var velocity = _player.Rigidbody.velocity;
        _player.Rigidbody.velocity = new Vector2(velocity.x, 0f);

        if (_player.IsWallSliding())
        {
            Debug.Log("Wall jump");
            _player.Rigidbody.GetContacts(_filter, _contacts);
            _player.WallJump(_contacts.First().normal);
        }
        else
        {
            _player.Jump();
            Debug.Log("Jump");
        }

        _graceTimer = 0f;
    }
    
    public override void FixedUpdate()
    {
        _graceTimer += Time.fixedDeltaTime;

        if (_graceTimer > GracePeriod)
        {
            if (_player.IsTouchingWall())
            {
                _player.StateMachine.ChangeState(_player.StateMachine.wallSlidingState);
            }
            else if (_player.IsGrounded())
            {
                _player.StateMachine.ChangeState(_player.StateMachine.runningState);
            }
        }
    }

    public override void Exit()
    {
        
    }
}