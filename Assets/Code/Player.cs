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
    public static readonly string WALLSLIDE_ANIMATION_NAME = "WallSlide";
    public static readonly string JUMP_ANIMATION_NAME = "Jump";
    public static readonly string LAND_ANIMATION_NAME = "Land";

    private const string COIN_LAYER_NAME = "Coin";

    [Header("Gameplay")]
    public LayerMask WallMask;
    [Range(0, 20f)]
    public float MoveSpeed = 10f;
    [Range(0, 30f)]
    public float JumpForce = 15f;
    [Range(10, 80f)]
    public float WallJumpAngle = 45f;
    [Range(0, 50f)]
    public float WallJumpForce = 30f;

    [Header("Sounds")]
    [SerializeField] private List<AudioClip> _jumpClips = new List<AudioClip>();

    public Rigidbody2D Rigidbody { get { return _rigidbody; } }
    public StateMachine StateMachine { get { return _stateMachine; } }
    public SpriteRenderer Sprite{ get { return _sprite; } }
    public Direction CurrentDirection
    {
        get { return _direction; }
        set { _direction = value; }
    }
    public Animation Animation { get { return _animation; } }

    private Rigidbody2D _rigidbody;
    private Vector2 _velocity = Vector2.zero;
    private StateMachine _stateMachine;
    private Direction _direction = Direction.Nil;
    private List<ContactPoint2D> _contacts = new List<ContactPoint2D>();
    private ContactFilter2D _filter;
    private Animation _animation;
    private AudioSource _audioSource;
    private SpriteRenderer _sprite;

    void Awake()
    {
        _animation = GetComponent<Animation>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>();
        _sprite = GetComponentInChildren<SpriteRenderer>();

        _stateMachine = new StateMachine(this);

        _filter = new ContactFilter2D();
        _filter.SetLayerMask(WallMask);
    }

    public void ResetPosition(Vector3 position)
    {
        transform.position = position;
        StateMachine.ChangeState(StateMachine.stationaryState);
        _rigidbody.velocity = Vector3.zero;
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

        _stateMachine.Update();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer(COIN_LAYER_NAME))
        {
            // Debug.Log($"Collected {col.name}");
            LevelManager.Instance.CollectCoin(col.gameObject);
        }
    }

    public bool IsGrounded()
    {
        if (_rigidbody.IsTouchingLayers(WallMask))
        {
            _rigidbody.GetContacts(_filter, _contacts);
            if (_contacts.Any(c => Vector2.Dot(c.normal, Vector2.up) == 1))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsTouchingWall()
    {
        if (_rigidbody.IsTouchingLayers(WallMask))
        {
            _rigidbody.GetContacts(_filter, _contacts);
            if (_contacts.Any(c => Mathf.Abs(Vector2.Dot(c.normal, Vector2.right)) == 1))
            {
                return true;
            }
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
        // Debug.Log($"Player Wall Jump to Dir {CurrentDirection}");
    }

    public void JumpEffect()
    {
        _animation.Play(JUMP_ANIMATION_NAME, PlayMode.StopSameLayer);

        if (_jumpClips.Count > 0)
        {
            //_audioSource.PlayOneShot(_jumpClips[Random.Range(0, _jumpClips.Count)]);
        }
    }
}
