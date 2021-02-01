using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static readonly int LEVEL_WIDTH = 7;
    public static readonly int LEVEL_HEIGHT = 4;
    public static readonly string HIGH_SCORE_PREFS_NAME = "HighScore";
    public static readonly string SESSION_COINS_PREFS_NAME = "CoinsInSession";

    private const int LEVEL_TIME_LIMIT_SEC = 100;
    private const string MAIN_MENU_LEVEL_NAME = "MainMenu";

    public static LevelManager Instance;

    [Header("References in scene")]
    [SerializeField] private Grid _grid;
    [SerializeField] private RectTransform _coinsHUDSprite;

    [Header("Prefabs")]
    [SerializeField] private GameObject _wallPrefab;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _coinPrefab;
    [SerializeField] private GameObject _levelEndScreenPrefab;

    [Header("Config")]
    [SerializeField] private List<LevelDataScriptableObject> _levels;
    [SerializeField] private AnimationCurve _coinFlyCurve;
    [SerializeField] private float _coinFlyTime;

    private GameObject _wallParent = null;
    private GameObject _playerGameObject = null;
    private Camera _mainCamera = null;
    private int _currentLevel = 0;
    private float _cellSize = 5f;
    private float _cellHalf = 2.5f;
    private float _levelTimer = 0f;
    private int _coinAmount = 0;
    private int _coinCounter = 0;
    private bool _inLevelEndScreen = false;

    void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }

        Instance = this;

        _mainCamera = Camera.main;

        // Save coins collected in session for high scores
        PlayerPrefs.SetInt(SESSION_COINS_PREFS_NAME, 0);

        _cellSize = _grid.cellSize.x;
        _cellHalf = _grid.cellSize.x / 2f;

        var leftWall = Instantiate(_wallPrefab) as GameObject;
        leftWall.name = "Left Wall";
        leftWall.transform.position = new Vector2(-_cellSize * (LEVEL_WIDTH / 2f), 0f);
        leftWall.transform.localScale = new Vector3(_cellSize, _cellSize * (LEVEL_HEIGHT + 2), 1f);

        var rightWall = Instantiate(_wallPrefab) as GameObject;
        rightWall.name = "Right Wall";
        rightWall.transform.position = new Vector2(_cellSize * (LEVEL_WIDTH / 2f + 1), 0f);
        rightWall.transform.localScale = new Vector3(_cellSize, _cellSize * (LEVEL_HEIGHT + 2), 1f);

        var floor = Instantiate(_wallPrefab) as GameObject;
        floor.name = "Floor";
        floor.transform.position = new Vector2(0f, -_cellSize * (LEVEL_HEIGHT / 2f) - _cellHalf);
        floor.transform.localScale = new Vector3(_cellSize * (LEVEL_WIDTH + 2), _cellSize, 1f);

        var ceiling = Instantiate(_wallPrefab) as GameObject;
        ceiling.name = "Ceiling";
        ceiling.transform.position = new Vector2(0f, _cellSize * (LEVEL_HEIGHT / 2f) + _cellHalf);
        ceiling.transform.localScale = new Vector3(_cellSize * (LEVEL_WIDTH + 2), _cellSize, 1f);

        BuildLevel();
    }

    public void BuildLevel()
    {
        _levelTimer = LEVEL_TIME_LIMIT_SEC;
        _coinCounter = 0;
        HUD.Instance.SetCoinAmount(_coinCounter);
        _coinAmount = 0;
        _inLevelEndScreen = false;

        if (_wallParent != null)
        {
            Destroy(_wallParent);
        }

        if (_playerGameObject == null)
        {
            _playerGameObject = Instantiate(_playerPrefab);
        }

        // Player starts in left bottom corner
        var player = _playerGameObject.GetComponent<Player>();
        player.ResetPosition(_grid.CellToWorld(new Vector3Int(0, -4, 0)) + new Vector3(_cellHalf, _cellHalf, 0f));

        _wallParent = new GameObject("WallParent");
        _wallParent.transform.SetParent(_grid.transform);
        _wallParent.transform.localPosition = Vector3.zero;
        // Level data is left-right, top-down
        var levelData = _levels[_currentLevel].LevelData;
        for(int y = 0; y < LEVEL_HEIGHT; y++)
        {
            for (int x = 0; x < LEVEL_WIDTH; x++)
            {
                if (levelData[y * LEVEL_WIDTH + x])
                {
                    // Walls
                    var wall = Instantiate(_wallPrefab, _wallParent.transform) as GameObject;
                    wall.transform.localPosition = new Vector2(_cellHalf + _cellSize * x, -_cellHalf - _cellSize * y);
                    wall.transform.localScale = Vector2.one * _cellSize;
                }
                else if (!(y == 3 && x == 0)) // Left bottom cell is always for player
                {
                    // Coins
                    var coin = Instantiate(_coinPrefab, _wallParent.transform) as GameObject;
                    coin.name = $"Coin ({x},{y})";
                    coin.transform.localPosition = new Vector2(_cellHalf + _cellSize * x, -_cellHalf - _cellSize * y);

                    _coinAmount++;
                }
            }
        }
    }

    void Update()
    {
        _levelTimer -= Time.deltaTime;
        HUD.Instance.SetTimer(Mathf.CeilToInt(_levelTimer));

        // Debug
        if (Input.GetKeyDown(KeyCode.C))
        {
            _coinCounter = _coinAmount + 1;
        }

        if (!_inLevelEndScreen)
        {
            if (_levelTimer <= 0f)
            {
                // Game over
                EndLevel(false, false);
            }
            else if (_coinCounter >= _coinAmount)
            {
                _currentLevel++;

                if (_currentLevel >= _levels.Count)
                {
                    // Game won
                    EndLevel(true, false);
                }
                else
                {
                    // Next level
                    EndLevel(true, true);
                }
            }
        }
    }

    void EndLevel(bool success, bool nextLevel = false)
    {
        _inLevelEndScreen = true;

        // High scores
        var highScore = PlayerPrefs.GetInt(HIGH_SCORE_PREFS_NAME);
        var currentScore = PlayerPrefs.GetInt(SESSION_COINS_PREFS_NAME);
        var gotNewHighScore = currentScore > highScore;
        if (gotNewHighScore)
        {
            PlayerPrefs.SetInt(HIGH_SCORE_PREFS_NAME, currentScore);
        }

        var screen = Instantiate(_levelEndScreenPrefab).GetComponent<LevelEndScreen>();
        screen.GotNewHighScore(gotNewHighScore);

        if (success)
            screen.LevelCompleted(nextLevel);
        else
            screen.LevelFailed();
    }

    public void CollectCoin(GameObject gameObject)
    {
        _coinCounter++;

        var currentScore = PlayerPrefs.GetInt(SESSION_COINS_PREFS_NAME);
        currentScore++;
        PlayerPrefs.SetInt(SESSION_COINS_PREFS_NAME, currentScore);

        StartCoroutine(FlyToHUDRoutine(gameObject, _coinsHUDSprite));
    }

    IEnumerator FlyToHUDRoutine(GameObject gameObject, RectTransform target)
    {
        var screenRect = RectTransformToScreenSpace(target);
        Vector3 targetWorldPoint = _mainCamera.ScreenToWorldPoint(screenRect.center);

        float timer = 0f;
        float safety = 10f;
        var start = gameObject.transform.position;
        var end = targetWorldPoint;

        while (gameObject != null && timer < _coinFlyTime && timer < safety)
        {
            timer += Time.deltaTime;
            var eval = _coinFlyCurve.Evaluate(timer / _coinFlyTime);
            gameObject.transform.position = Vector3.Lerp(start, end, eval);
            yield return new WaitForEndOfFrame();
        }

        HUD.Instance.SetCoinAmount(_coinCounter);
        Destroy(gameObject);
    }

    // From https://answers.unity.com/questions/1013011/convert-recttransform-rect-to-screen-space.html
    public static Rect RectTransformToScreenSpace(RectTransform transform)
     {
         Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
         return new Rect((Vector2)transform.position - (size * 0.5f), size);
     }
}