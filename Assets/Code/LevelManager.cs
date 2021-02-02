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
    [SerializeField] private GameObject _hintText;

    [Header("Prefabs")]
    [SerializeField] private GameObject _wallPrefab;
    [SerializeField] private GameObject _edgePrefab;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _coinPrefab;
    [SerializeField] private GameObject _levelEndScreenPrefab;

    [Header("Config")]
    [SerializeField] private List<LevelDataScriptableObject> _levels;
    [SerializeField] private AnimationCurve _coinFlyCurve;
    [SerializeField] private float _coinFlyTime;
    [Range(0, 100)]
    [SerializeField] private int _extraCoinLevelChance = 33;
    [SerializeField] private int _extraCoinTimeLimitSeconds = 30;

    private GameObject _wallParent = null;
    private GameObject _playerGameObject = null;
    private Camera _mainCamera = null;
    private int _currentLevel = 0;
    private float _cellSize = 5f;
    private float _cellHalf = 2.5f;
    private float _levelTimer = 0f;
    private bool _inLevelEndScreen = false;
    private List<Coin> _coins = new List<Coin>();

    void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }

        PlayerPrefs.SetInt(HIGH_SCORE_PREFS_NAME, 0);

        Instance = this;

        _mainCamera = Camera.main;

        // Save coins collected in session for high scores
        PlayerPrefs.SetInt(SESSION_COINS_PREFS_NAME, 0);

        _cellSize = _grid.cellSize.x;
        _cellHalf = _grid.cellSize.x / 2f;

        var leftWall = Instantiate(_edgePrefab) as GameObject;
        leftWall.name = "Left Wall";
        leftWall.transform.position = new Vector2(-_cellSize * (LEVEL_WIDTH / 2f), 0f);
        leftWall.transform.localScale = new Vector3(_cellSize, _cellSize * (LEVEL_HEIGHT + 2), 1f);

        var rightWall = Instantiate(_edgePrefab) as GameObject;
        rightWall.name = "Right Wall";
        rightWall.transform.position = new Vector2(_cellSize * (LEVEL_WIDTH / 2f + 1), 0f);
        rightWall.transform.localScale = new Vector3(_cellSize, _cellSize * (LEVEL_HEIGHT + 2), 1f);

        var floor = Instantiate(_edgePrefab) as GameObject;
        floor.name = "Floor";
        floor.transform.position = new Vector2(0f, -_cellSize * (LEVEL_HEIGHT / 2f) - _cellHalf);
        floor.transform.localScale = new Vector3(_cellSize * (LEVEL_WIDTH + 2), _cellSize, 1f);
        var sprite = floor.GetComponent<SpriteRenderer>().sprite;

        var ceiling = Instantiate(_edgePrefab) as GameObject;
        ceiling.name = "Ceiling";
        ceiling.transform.position = new Vector2(0f, _cellSize * (LEVEL_HEIGHT / 2f) + _cellHalf);
        ceiling.transform.localScale = new Vector3(_cellSize * (LEVEL_WIDTH + 2), _cellSize, 1f);

        BuildLevel();
    }

    public void BuildLevel()
    {
        _levelTimer = LEVEL_TIME_LIMIT_SEC;
        HUD.Instance.SetCoinAmount(0);
        _inLevelEndScreen = false;
        _coins.Clear();

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
                    var coinObj = Instantiate(_coinPrefab, _wallParent.transform) as GameObject;
                    coinObj.name = $"Coin ({x},{y})";
                    coinObj.transform.localPosition = new Vector2(_cellHalf + _cellSize * x, -_cellHalf - _cellSize * y);
                    _coins.Add(coinObj.GetComponent<Coin>());
                }
            }
        }

        bool hasExtra = Random.Range(0, 100) <= _extraCoinLevelChance;
        if (hasExtra)
        {
            var idx = Random.Range(0, _coins.Count);
            _coins[idx].SetExtra(true, _extraCoinTimeLimitSeconds);
        }
    }

    void Update()
    {
        _levelTimer -= Time.deltaTime;
        HUD.Instance.SetTimer(Mathf.CeilToInt(_levelTimer));

        if (Input.GetKeyDown(KeyCode.Space) || (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
        {
            _hintText.SetActive(false);
        }

        if (!_inLevelEndScreen)
        {
            if (_levelTimer <= 0f)
            {
                // Game over
                EndLevel(false, false);
            }
            else if (_coins.TrueForAll(c => c.CurrentState != Coin.State.Collectable))
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

    public void CollectCoin(Coin coin)
    {
        var currentScore = PlayerPrefs.GetInt(SESSION_COINS_PREFS_NAME);
        int add = coin.IsExtra ? currentScore / 10 : 1;
        currentScore += add == 0 ? 5 : add; // Diamond is 10% or 5 if player has < 10 points
        PlayerPrefs.SetInt(SESSION_COINS_PREFS_NAME, currentScore);

        StartCoroutine(FlyToHUDRoutine(coin, _coinsHUDSprite));
    }

    IEnumerator FlyToHUDRoutine(Coin coin, RectTransform target)
    {
        var screenRect = RectTransformToScreenSpace(target);
        Vector3 targetWorldPoint = _mainCamera.ScreenToWorldPoint(screenRect.center);

        float timer = 0f;
        float safety = 10f;
        var start = coin.transform.position;
        var end = targetWorldPoint;

        while (coin.gameObject != null && timer < _coinFlyTime && timer < safety)
        {
            timer += Time.deltaTime;
            var eval = _coinFlyCurve.Evaluate(timer / _coinFlyTime);
            coin.transform.position = Vector3.Lerp(start, end, eval);
            yield return new WaitForEndOfFrame();
        }

        HUD.Instance.SetCoinAmount(PlayerPrefs.GetInt(SESSION_COINS_PREFS_NAME));
        coin.Collect();
    }

    // From https://answers.unity.com/questions/1013011/convert-recttransform-rect-to-screen-space.html
    public static Rect RectTransformToScreenSpace(RectTransform transform)
     {
         Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
         return new Rect((Vector2)transform.position - (size * 0.5f), size);
     }
}