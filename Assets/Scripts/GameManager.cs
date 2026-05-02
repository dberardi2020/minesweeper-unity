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
        _board = new Board(_rows, _cols, _mineCount);
        _state = GameState.Ready;
        _cellViews = new CellView[_rows, _cols];
        BuildGrid();
        headerView.transform.position = new Vector3((_cols - 1) / 2f, 1f, 0f);
        FitCamera();
        headerView.OnResetClick += ResetGame;
        RefreshHeader();
    }

    void Update()
    {
        if (_state != GameState.Playing) return;
        _timer += Time.deltaTime;
        headerView.Refresh(_state, _mineCount - _board.FlagCount, Mathf.FloorToInt(_timer));
    }

    void BuildGrid()
    {
        for (int row = 0; row < _rows; row++)
        {
            for (int col = 0; col < _cols; col++)
            {
                GameObject go = Instantiate(cellPrefab, gridParent);
                go.transform.localPosition = new Vector3(col, -row, 0);
                go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

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

    // Centers the camera on the grid + header and sizes it to fit.
    // The tile gap is 0.05 world units (1.0 - 0.95 scale), so PPU must be a multiple
    // of 20 for the gap to be a whole number of pixels.
    void FitCamera()
    {
        float gridWidth  = _cols;
        float gridHeight = _rows + 1.5f; // +1.5 for header

        var cam = Camera.main;
        cam.transform.position = new Vector3(
            (_cols - 1) / 2f,
            -(_rows - 1) / 2f + 0.75f,
            -10f
        );

        float rawSize = gridHeight / 2f + 0.5f;
        float rawPpu  = cam.pixelHeight / (rawSize * 2f);
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
        _board.ToggleFlag(row, col);
        _cellViews[row, col].Refresh(_board.Cells[row, col], false);
        RefreshHeader();
    }

    void ResetGame()
    {
        if (_bloom != null) { StopCoroutine(_bloom); _bloom = null; }

        // Destroy existing cell GameObjects before rebuilding
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        _board = new Board(_rows, _cols, _mineCount);
        _state = GameState.Ready;
        _cellViews = new CellView[_rows, _cols];
        _timer = 0f;
        BuildGrid();
        headerView.transform.position = new Vector3((_cols - 1) / 2f, 1f, 0f);
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

    // Fixed mine layout for test mode — works on any grid size >= 9x9.
    // Mines form a diagonal boundary so clicking top-right always gives a clean bloom.
    void PlaceTestMines()
    {
        (int r, int c)[] positions = { (2,4),(3,3),(4,2),(4,4),(5,1),(5,5),(6,0),(6,6),(7,7),(8,8) };
        foreach (var (r, c) in positions)
            if (r < _rows && c < _cols)
                _board.Cells[r, c].isMine = true;
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
        headerView.Refresh(_state, _mineCount - _board.FlagCount, Mathf.FloorToInt(_timer));
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
