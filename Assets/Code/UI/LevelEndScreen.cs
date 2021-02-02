using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelEndScreen : MonoBehaviour
{
    private const string SCORE_TEXT = "{0} points.";
    private const string ZERO_SCORE_TEXT = "ZERO points!?";
    private const string SOME_SCORE_COMMENT = "Well, at least you got";
    private const string ZERO_SCORE_COMMENT = "Also, you scored";

    [SerializeField] private GameObject _newHighScoreText;
    [Header("Next Level")]
    [SerializeField] private int _levelChangeCountdown = 3;
    [SerializeField] private GameObject _nextLevelParent;
    [SerializeField] private TextMeshProUGUI _nextLevelTimerText;
    [Header("Fail")]
    [SerializeField] private GameObject _failParent;
    [SerializeField] private TextMeshProUGUI _failText;
    [SerializeField] private TextMeshProUGUI _failScoreText;
    [Header("Completed")]
    [SerializeField] private GameObject _doneParent;
    [SerializeField] private TextMeshProUGUI _doneScoreText;

    private bool _hasNextLevel = false;
    private float _levelChangeTimer = 0f;

    void Start()
    {
        _newHighScoreText.SetActive(false);
    }

    public void GotNewHighScore(bool value)
    {
        _newHighScoreText.SetActive(value);
    }

    public void LevelCompleted(bool hasNextLevel = false)
    {
        if (hasNextLevel)
        {
            _nextLevelParent.SetActive(true);
            _failParent.SetActive(false);
            _doneParent.SetActive(false);

            _nextLevelTimerText.text = _levelChangeCountdown.ToString();
            _hasNextLevel = hasNextLevel;
            _levelChangeTimer = _levelChangeCountdown;
        }
        else
        {
            _nextLevelParent.SetActive(false);
            _failParent.SetActive(false);
            _doneParent.SetActive(true);

            _doneScoreText.text = string.Format(SCORE_TEXT, PlayerPrefs.GetInt(LevelManager.HIGH_SCORE_PREFS_NAME));
        }
    }

    void Update()
    {
        if (_hasNextLevel)
        {
            _levelChangeTimer -= Time.deltaTime;
            _nextLevelTimerText.text = Mathf.CeilToInt(_levelChangeTimer).ToString();

            if (_levelChangeTimer <= 0f)
            {
                StartNextLevel();
                _levelChangeTimer = 0f;
            }
        }
    }

    public void LevelFailed()
    {
        _nextLevelParent.SetActive(false);
        _failParent.SetActive(true);
        _doneParent.SetActive(false);

        var score = PlayerPrefs.GetInt(LevelManager.SESSION_COINS_PREFS_NAME);
        if (score == 0)
        {
            _failText.text = ZERO_SCORE_COMMENT;
            _failScoreText.text = ZERO_SCORE_TEXT;
        }
        else
        {
            _failText.text = SOME_SCORE_COMMENT;
            _failScoreText.text = string.Format(SCORE_TEXT, PlayerPrefs.GetInt(LevelManager.HIGH_SCORE_PREFS_NAME));
        }
    }

    public void StartNextLevel()
    {
        LevelManager.Instance.BuildLevel();
        Destroy(gameObject);
    }
}
