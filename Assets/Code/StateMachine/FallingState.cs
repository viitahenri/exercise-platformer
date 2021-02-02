using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FallingState : State
{
    private const float GraceTime = .1f;
    private List<ContactPoint2D> _contacts = new List<ContactPoint2D>();
    private ContactFilter2D _filter;
    private float _timer = 0f;

    public FallingState(Player player) : base(player)
    {
        _filter = new ContactFilter2D();
        _filter.SetLayerMask(player.WallMask);
    }

    public override void Enter()
    {
        // Debug.Log($"Falling");
        _timer = 0f;
    }

    public override void Exit()
    {
        _player.Sprite.transform.localScale = Vector3.one;
    }

    public override void Update()
    {
        // Squeeze the sprite a bit when falling
        _timer += Time.deltaTime;
        var squeezeX = Mathf.Lerp(1f, 0.8f, _timer);
        var squeezeY = Mathf.Lerp(1f, 1.2f, _timer);
        _player.Sprite.transform.localScale = new Vector3(squeezeX, squeezeY, 1f);
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

    public override void HandleInput()
    {
        if (!_player.IsGrounded() && _timer <= GraceTime)
        {
            // Debug.Log($"Grace jump when falling!");
            _player.StateMachine.ChangeState(_player.StateMachine.jumpingState);
        }
    }
}