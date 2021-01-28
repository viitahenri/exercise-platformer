using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FallingState : State
{
    private List<ContactPoint2D> _contacts = new List<ContactPoint2D>();
    private ContactFilter2D _filter;
    private const float GracePeriod = .2f;
    private float _graceTimer = 0f;

    public FallingState(Player player) : base(player)
    {
        _filter = new ContactFilter2D();
        _filter.SetLayerMask(player.WallMask);
    }

    public override void Enter()
    {
        Debug.Log($"Falling");
    }
    
    public override void FixedUpdate()
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
    }
}