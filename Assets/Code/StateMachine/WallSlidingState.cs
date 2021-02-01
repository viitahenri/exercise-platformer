using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WallSlidingState : State
{
    private List<ContactPoint2D> _contacts = new List<ContactPoint2D>();
    private float _timer = 0f;

    public WallSlidingState(Player player) : base(player) { }

    public override void Enter()
    {
        // Debug.Log("Wallslide");
        _timer = 0f;
        _player.Rigidbody.GetContacts(_contacts);
        _player.Animation.Play(Player.WALLSLIDE_ANIMATION_NAME);
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
        else if (!_player.IsTouchingWall())
        {
            _player.Rigidbody.AddForce(-_contacts.First().normal * 2f, ForceMode2D.Impulse);
            _player.StateMachine.ChangeState(_player.StateMachine.fallingState);
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