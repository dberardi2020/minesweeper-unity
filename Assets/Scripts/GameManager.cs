using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject cellPrefab;
    public Transform gridParent;

    Board _board;
    GameState _state;
    CellView[,] _cellViews = new CellView[9, 9];

    void Awake()
    {
        _board = new Board();
        _state = GameState.Ready;
        BuildGrid();
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

        if (_state == GameState.Ready)
        {
            _board.PlaceMines(row, col);
            _board.ComputeAdjacentCounts();
            _state = GameState.Playing;
        }

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
            RefreshAll(revealAll: false);
        }
    }

    void HandleRightClick(int row, int col)
    {
        if (_state != GameState.Playing) return;
        _board.ToggleFlag(row, col);
        _cellViews[row, col].Refresh(_board.Cells[row, col], false);
    }

    void RefreshAll(bool revealAll)
    {
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                _cellViews[r, c].Refresh(_board.Cells[r, c], revealAll);
    }
}
