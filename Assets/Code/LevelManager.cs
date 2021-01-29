using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static readonly int LEVEL_WIDTH = 7;
    public static readonly int LEVEL_HEIGHT = 4;

    private const string MAIN_MENU_LEVEL_NAME = "MainMenu";

    public static LevelManager Instance;

    [Header("References in scene")]
    [SerializeField] private Grid _grid;

    [Header("Prefabs")]
    [SerializeField] private GameObject _wallPrefab;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _coinPrefab;

    [Header("Levels")]
    [SerializeField] private List<LevelDataScriptableObject> _levels;

    private int _currentLevel = 0;
    private float _cellSize = 5f;
    private float _cellHalf = 2.5f;
    private GameObject _wallParent = null;
    private GameObject _playerGameObject = null;
    private float _levelTimer = 0f;
    private int _coinAmount = 0;
    private int _coinCounter = 0;

    void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }

        Instance = this;

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

    void BuildLevel()
    {
        _levelTimer = 100f;
        _coinCounter = 0;
        HUD.Instance.SetCoinAmount(_coinCounter);
        _coinAmount = 0;

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

        if (_levelTimer <= 0f)
        {
            // Game over
            UnityEngine.SceneManagement.SceneManager.LoadScene(MAIN_MENU_LEVEL_NAME);
        }

        if (_coinCounter >= _coinAmount)
        {
            _currentLevel++;

            if (_currentLevel >= _levels.Count)
            {
                // Game won
                UnityEngine.SceneManagement.SceneManager.LoadScene(MAIN_MENU_LEVEL_NAME);
            }
            else
            {
                // Next level
                BuildLevel();
            }
        }

        // Debug
        if (Input.GetKeyDown(KeyCode.C))
        {
            _coinCounter = _coinAmount + 1;
        }
    }

    public void CollectCoin(GameObject gameObject)
    {
        Destroy(gameObject);
        _coinCounter++;
        HUD.Instance.SetCoinAmount(_coinCounter);
    }
}