using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelEndScreen : MonoBehaviour
{
    private const string TIMER_TEXT = "{0} seconds";
    private const string SCORE_TEXT = "{0} points.";

    [SerializeField] private GameObject _newHighScoreText;
    [Header("Next Level")]
    [SerializeField] private int _levelChangeCountdown = 3;
    [SerializeField] private GameObject _nextLevelParent;
    [SerializeField] private TextMeshProUGUI _nextLevelTimerText;
    [Header("Fail")]
    [SerializeField] private GameObject _failParent;
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
        _nextLevelParent.SetActive(true);
        _failParent.SetActive(false);
        _doneParent.SetActive(false);

        _failScoreText.text = string.Format(SCORE_TEXT, PlayerPrefs.GetInt(LevelManager.HIGH_SCORE_PREFS_NAME));
    }

    public void StartNextLevel()
    {
        LevelManager.Instance.BuildLevel();
        Destroy(gameObject);
    }
}
