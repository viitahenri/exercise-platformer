using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Coin : MonoBehaviour
{
    public enum State
    {
        Collectable,
        Collected,
        Expired
    }

    public bool IsExtra { get { return _isExtra; } }
    public State CurrentState
    { 
        get { return _currentState; }
    }

    [SerializeField] private GameObject _normalSprite;
    [SerializeField] private GameObject _extraSprite;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Image _extraTimerImage;
    
    private State _currentState = State.Collectable;
    private bool _isExtra = false;
    private int _timeLimitSeconds;
    private float _timer = 0f;

    void Awake()
    {
        // Starts as normal
        _normalSprite.SetActive(!_isExtra);
        _extraSprite.SetActive(_isExtra);
        ToggleCanvas(false);
    }

    public void SetExtra(bool isExtra, int timeLimitSeconds = 0)
    {
        _timer = _timeLimitSeconds = timeLimitSeconds;
        _isExtra = isExtra;
        _normalSprite.SetActive(!isExtra);
        _extraSprite.SetActive(isExtra);
        ToggleCanvas(true);
    }

    public void Collect()
    {
        _currentState = State.Collected;
        gameObject.SetActive(false);
    }

    public void ToggleCanvas(bool enabled)
    {
        _canvas.gameObject.SetActive(enabled);
        _extraTimerImage.enabled = enabled;
    }

    void Update()
    {
        if (IsExtra && _currentState == State.Collectable)
        {
            _timer -= Time.deltaTime;
            _extraTimerImage.fillAmount = _timer / _timeLimitSeconds;

            if (_timer <= 0f)
            {
                _currentState = State.Expired;
                gameObject.SetActive(false);
            }
        }
    }
}