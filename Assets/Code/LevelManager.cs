using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static readonly int LEVEL_WIDTH = 7;
    public static readonly int LEVEL_HEIGHT = 4;

    [SerializeField] private Grid _grid;
    [SerializeField] private GameObject _wallPrefab;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private List<LevelDataScriptableObject> _levels;

    private int _currentLevel = 0;
    private float _cellSize = 5f;
    private float _cellHalf = 2.5f;
    private GameObject _wallParent = null;
    private GameObject _player = null;

    void Start()
    {
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
        if (_wallParent != null)
        {
            DestroyImmediate(_wallParent);
        }

        if (_player == null)
        {
            _player = Instantiate(_playerPrefab);
        }

        // Player starts in left bottom corner
        _player.transform.position = _grid.CellToWorld(new Vector3Int(0, -4, 0)) + new Vector3(_cellHalf, _cellHalf, 0f);

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
                    var wall = Instantiate(_wallPrefab, _wallParent.transform) as GameObject;
                    wall.transform.localPosition = new Vector2(_cellHalf + _cellSize * x, -_cellHalf - _cellSize * y);
                    wall.transform.localScale = Vector2.one * _cellSize;
                }
            }
        }
    }
}