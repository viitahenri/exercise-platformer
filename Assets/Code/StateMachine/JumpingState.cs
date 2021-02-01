using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JumpingState : State
{
    private List<ContactPoint2D> _contacts = new List<ContactPoint2D>();
    private ContactFilter2D _filter;
    private const float Hysteresis = .1f;
    private float _timer = 0f;

    public JumpingState(Player player) : base(player)
    {
        _filter = new ContactFilter2D();
        _filter.SetLayerMask(player.WallMask);
    }

    public override void Enter()
    {
        // stop falling to make jumps equally powerful even if sliding down a wall
        var velocity = _player.Rigidbody.velocity;
        _player.Rigidbody.velocity = new Vector2(velocity.x, 0f);

        if (_player.IsWallSliding())
        {
            // Debug.Log("Wall jump");
            _player.Rigidbody.GetContacts(_filter, _contacts);
            _player.WallJump(_contacts.First().normal);
        }
        else
        {
            _player.Jump();
            // Debug.Log("Jump");
        }

        _player.JumpEffect();

        _timer = 0f;
    }
    
    public override void FixedUpdate()
    {
        _timer += Time.fixedDeltaTime;

        if (_timer > Hysteresis)
        {
            if (_player.IsGrounded())
            {
                _player.StateMachine.ChangeState(_player.StateMachine.runningState);
            }
            else if (_player.IsTouchingWall())
            {
                _player.Rigidbody.GetContacts(_filter, _contacts);
                var normal = _contacts.First().normal;
                var dot = Vector2.Dot(_contacts.First().normal, Vector2.down);
                if (dot < 1) // Don't wallslide on ceiling
                    _player.StateMachine.ChangeState(_player.StateMachine.wallSlidingState);
            }
            else
            {
                _player.StateMachine.ChangeState(_player.StateMachine.fallingState);
            }            
        }
    }

    public override void Exit()
    {
        
    }
}