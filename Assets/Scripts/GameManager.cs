using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject cellPrefab;
    public Transform gridParent;
    public HeaderView headerView;

    [Header("Testing")]
    public bool testMode;

    // Fixed layout: mines form a diagonal boundary through the middle,
    // leaving the top-right open so clicking (0,8) always gives a clean bloom.
    static readonly (int r, int c)[] TestMines =
    {
        (2, 4), (3, 3), (4, 2), (4, 4),
        (5, 1), (5, 5), (6, 0), (6, 6),
        (7, 7), (8, 8),
    };

    Board _board;
    GameState _state;
    CellView[,] _cellViews = new CellView[9, 9];
    float _timer;
    Coroutine _bloom;

    void Awake()
    {
        SnapCameraToPixelGrid();
        _board = new Board();
        _state = GameState.Ready;
        BuildGrid();
        headerView.OnResetClick += ResetGame;
        RefreshHeader();
    }

    void Update()
    {
        if (_state != GameState.Playing) return;
        _timer += Time.deltaTime;
        headerView.Refresh(_state, 10 - _board.FlagCount, Mathf.FloorToInt(_timer));
    }

    // The tile gap is 0.05 world units (1.0 - 0.95 scale), so PPU must be a multiple
    // of 20 for the gap to be a whole number of pixels. cam.pixelHeight gives the
    // physical render target height, which is correct on Retina/HiDPI displays.
    void SnapCameraToPixelGrid()
    {
        var cam = Camera.main;
        float rawPpu = cam.pixelHeight / (cam.orthographicSize * 2f);
        float snappedPpu = Mathf.Round(rawPpu / 20f) * 20f;
        if (snappedPpu > 0) cam.orthographicSize = cam.pixelHeight / (snappedPpu * 2f);
    }

    void BuildGrid()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
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

    void HandleLeftClick(int row, int col)
    {
        if (_state == GameState.Lost || _state == GameState.Won) return;

        // Snap any in-progress bloom before processing the new click
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

        // Snapshot revealed state before this reveal so we know which cells are new
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
            // Reveal tiles and numbers immediately
            RefreshAll(false);
            // Hide flowers on newly-revealed empty cells so they can bloom in
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
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
        _board = new Board();
        _state = GameState.Ready;
        _timer = 0f;
        RefreshAll(revealAll: false);
        RefreshHeader();
    }

    // Animates newly-revealed cells in a wave outward from the clicked cell.
    // Each cell gets a delay = distance * speed + small random jitter.
    IEnumerator Bloom(int originRow, int originCol, bool[,] prevRevealed)
    {
        var cells = new List<(int r, int c, float delay)>();

        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                // Only animate flowers on newly-revealed empty cells
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

    void PlaceTestMines()
    {
        foreach (var (r, c) in TestMines)
            _board.Cells[r, c].isMine = true;
    }

    bool[,] Snapshot()
    {
        bool[,] snap = new bool[9, 9];
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                snap[r, c] = _board.Cells[r, c].isRevealed;
        return snap;
    }

    void RefreshAll(bool revealAll)
    {
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                _cellViews[r, c].Refresh(_board.Cells[r, c], revealAll);
    }

    void RefreshHeader()
    {
        headerView.Refresh(_state, 10 - _board.FlagCount, Mathf.FloorToInt(_timer));
    }
}
