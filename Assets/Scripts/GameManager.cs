using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject cellPrefab;
    public Transform gridParent;
    public HeaderView headerView;

    [Header("Grid")]
    public Difficulty difficulty = Difficulty.Beginner;

    int _rows, _cols, _mineCount;

    [Header("Testing")]
    public bool testMode;

    Board _board;
    GameState _state;
    CellView[,] _cellViews;
    float _timer;
    Coroutine _bloom;

    void Awake()
    {
        ApplyDifficulty();
        headerView.OnResetClick += ResetGame;
        InitGame();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CellView.UseV2Question = !CellView.UseV2Question;
            RefreshAll(_state == GameState.Lost || _state == GameState.Won);
        }

        if (_state != GameState.Playing) return;
        _timer += Time.deltaTime;
        RefreshHeader();
    }

    void BuildGrid()
    {
        for (int row = 0; row < _rows; row++)
        {
            for (int col = 0; col < _cols; col++)
            {
                GameObject go = Instantiate(cellPrefab, gridParent);
                go.transform.localPosition = new Vector3(col, -row, 0);
                go.transform.localScale = Vector3.one;

                CellView cv = go.GetComponent<CellView>();
                cv.Row = row;
                cv.Col = col;
                cv.Refresh(new Cell(), false);
                cv.OnLeftClick += HandleLeftClick;
                cv.OnRightClick += HandleRightClick;
                _cellViews[row, col] = cv;
            }
        }
    }

    void FitCamera()
    {
        float gridHeight = _rows + 1.5f; // +1.5 for header

        var cam = Camera.main;
        cam.transform.position = new Vector3(
            (_cols - 1) / 2f,
            -(_rows - 1) / 2f + 0.75f,
            -10f
        );

        // Snap PPU to a multiple of 20 so the 0.05-unit tile gap maps to a whole pixel.
        float rawSize    = gridHeight / 2f + 0.5f;
        float rawPpu     = cam.pixelHeight / (rawSize * 2f);
        float snappedPpu = Mathf.Round(rawPpu / 20f) * 20f;
        cam.orthographicSize = snappedPpu > 0
            ? cam.pixelHeight / (snappedPpu * 2f)
            : rawSize;
    }

    void HandleLeftClick(int row, int col)
    {
        if (_state == GameState.Lost || _state == GameState.Won) return;

        if (_bloom != null) { StopCoroutine(_bloom); _bloom = null; RefreshAll(false); }

        if (_state == GameState.Ready)
        {
            if (testMode)
                PlaceTestMines();
            else
                _board.PlaceMines(row, col);
            _board.ComputeAdjacentCounts();
            _state = GameState.Playing;
        }

        bool[,] prevRevealed = Snapshot();

        _board.Reveal(row, col);

        if (_board.Cells[row, col].isRevealed && _board.Cells[row, col].isMine)
        {
            _state = GameState.Lost;
            RefreshAll(revealAll: true);
        }
        else if (_board.CheckWin())
        {
            _state = GameState.Won;
            RefreshAll(revealAll: false);
        }
        else
        {
            RefreshAll(false);
            for (int r = 0; r < _rows; r++)
                for (int c = 0; c < _cols; c++)
                    if (_board.Cells[r, c].isRevealed && !prevRevealed[r, c] && _board.Cells[r, c].adjacentMines == 0)
                        _cellViews[r, c].HideIcon();
            _bloom = StartCoroutine(Bloom(row, col, prevRevealed));
        }

        RefreshHeader();
    }

    void HandleRightClick(int row, int col)
    {
        if (_state != GameState.Playing) return;
        _board.CycleMarker(row, col);
        _cellViews[row, col].Refresh(_board.Cells[row, col], false);
        RefreshHeader();
    }

    void ResetGame()
    {
        if (_bloom != null) { StopCoroutine(_bloom); _bloom = null; }
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);
        InitGame();
    }

    void InitGame()
    {
        _board = new Board(_rows, _cols, _mineCount);
        _state = GameState.Ready;
        _cellViews = new CellView[_rows, _cols];
        _timer = 0f;
        BuildGrid();
        headerView.transform.position = new Vector3((_cols - 1) / 2f, 1f, 0f);
        headerView.Build(_cols);
        FitCamera();
        RefreshHeader();
    }

    IEnumerator Bloom(int originRow, int originCol, bool[,] prevRevealed)
    {
        var cells = new List<(int r, int c, float delay)>();

        for (int r = 0; r < _rows; r++)
        {
            for (int c = 0; c < _cols; c++)
            {
                if (!_board.Cells[r, c].isRevealed || prevRevealed[r, c] || _board.Cells[r, c].adjacentMines != 0) continue;
                float dist = Mathf.Sqrt((r - originRow) * (r - originRow) + (c - originCol) * (c - originCol));
                float delay = Mathf.Pow(dist, 0.6f) * 0.18f + Random.Range(0f, 0.05f);
                cells.Add((r, c, delay));
            }
        }

        cells.Sort((a, b) => a.delay.CompareTo(b.delay));

        float elapsed = 0f;
        foreach (var (r, c, delay) in cells)
        {
            float wait = delay - elapsed;
            if (wait > 0) yield return new WaitForSeconds(wait);
            elapsed = delay;
            _cellViews[r, c].ShowFlower();
        }

        _bloom = null;
    }

    // Fixed-seed mine placement for test mode. Top-right corner (0, _cols-1) is always safe
    // so clicking there produces a predictable bloom. Places exactly _mineCount mines.
    void PlaceTestMines()
    {
        var pool = new List<(int r, int c)>();
        for (int r = 0; r < _rows; r++)
            for (int c = 0; c < _cols; c++)
                if (r != 0 || c != _cols - 1)
                    pool.Add((r, c));

        var rng = new System.Random(42);
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        for (int i = 0; i < _mineCount; i++)
            _board.Cells[pool[i].r, pool[i].c].isMine = true;
    }

    bool[,] Snapshot()
    {
        bool[,] snap = new bool[_rows, _cols];
        for (int r = 0; r < _rows; r++)
            for (int c = 0; c < _cols; c++)
                snap[r, c] = _board.Cells[r, c].isRevealed;
        return snap;
    }

    void RefreshAll(bool revealAll)
    {
        for (int r = 0; r < _rows; r++)
            for (int c = 0; c < _cols; c++)
                _cellViews[r, c].Refresh(_board.Cells[r, c], revealAll);
    }

    void RefreshHeader()
    {
        headerView.Refresh(_state, _mineCount - _board.FlagCount, Mathf.FloorToInt(_timer), SafeProgress());
    }

    float SafeProgress()
    {
        int totalSafe = _rows * _cols - _mineCount;
        if (totalSafe == 0 || _state == GameState.Ready) return 0f;
        int revealed = 0;
        for (int r = 0; r < _rows; r++)
            for (int c = 0; c < _cols; c++)
                if (_board.Cells[r, c].isRevealed && !_board.Cells[r, c].isMine)
                    revealed++;
        return (float)revealed / totalSafe;
    }

    void ApplyDifficulty()
    {
        (_rows, _cols, _mineCount) = DifficultyConfig.Get(difficulty);
    }

    public void SetDifficulty(Difficulty d)
    {
        difficulty = d;
        ApplyDifficulty();
        ResetGame();
    }
}
