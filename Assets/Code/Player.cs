﻿using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    Nil,
    Left,
    Right
}

public class Player : MonoBehaviour
{
    public LayerMask WallMask;
    [Range(5, 20f)]
    public float MoveSpeed = 10f;
    [Range(5, 30f)]
    public float JumpForce = 15f;
    [Range(10, 80f)]
    public float WallJumpAngle = 45f;
    [Range(5, 50f)]
    public float WallJumpForce = 30f;

    public Rigidbody2D Rigidbody { get { return _rigidbody; } }
    public StateMachine StateMachine { get { return _stateMachine; } }
    public Direction CurrentDirection
    {
        get { return _direction; }
        set { _direction = value; }
    }

    private Rigidbody2D _rigidbody;
    private Vector2 _velocity = Vector2.zero;
    private StateMachine _stateMachine;
    private Direction _direction = Direction.Nil;
    private List<ContactPoint2D> _contacts = new List<ContactPoint2D>();
    private ContactFilter2D _filter;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();

        _stateMachine = new StateMachine(this);

        _filter = new ContactFilter2D();
        _filter.SetLayerMask(WallMask);
    }
    void FixedUpdate()
    {
        _stateMachine.FixedUpdate();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _stateMachine.HandleInput();
        }
    }

    public bool IsGrounded()
    {
        if (_rigidbody.IsTouchingLayers(WallMask))
        {
            _rigidbody.GetContacts(_filter, _contacts);
            var normal = _contacts.First().normal;
            if (Vector2.Dot(normal, Vector2.up) == 1)
                return true;
        }
        return false;
    }

    public bool IsTouchingWall()
    {
        if (_rigidbody.IsTouchingLayers(WallMask))
        {
            _rigidbody.GetContacts(_filter, _contacts);
            var normal = _contacts.First().normal;
            if (Mathf.Abs(Vector2.Dot(normal, Vector2.right)) == 1)
                return true;
        }
        return false;
    }

    public bool IsWallSliding()
    {
        return IsTouchingWall() && !IsGrounded();
    }

    public void Jump()
    {
        _rigidbody.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
    }

    public void WallJump(Vector2 normal)
    {
        float dot = Vector2.Dot(normal, Vector2.right);

        // Wall jump vector based on the angle
        float angleRad = WallJumpAngle * Mathf.Deg2Rad;
        float x = Mathf.Cos(angleRad) * dot;
        float y = Mathf.Sin(angleRad);

        _rigidbody.AddForce(new Vector2(x, y) * WallJumpForce, ForceMode2D.Impulse);

        CurrentDirection = CurrentDirection == Direction.Left ? Direction.Right : Direction.Left;
        Debug.Log($"Player Wall Jump to Dir {CurrentDirection}");
    }
}