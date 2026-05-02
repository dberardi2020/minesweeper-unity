using System.Collections.Generic;

public class Board
{
    public readonly int Rows;
    public readonly int Cols;
    public readonly int MineCount;

    public Cell[,] Cells;

    public Board(int rows, int cols, int mineCount)
    {
        Rows = rows;
        Cols = cols;
        MineCount = mineCount;
        Cells = new Cell[rows, cols];
    }

    public int FlagCount
    {
        get
        {
            int count = 0;
            foreach (Cell c in Cells)
                if (c.isFlagged) count++;
            return count;
        }
    }

    public void PlaceMines(int safeRow, int safeCol)
    {
        List<(int, int)> candidates = new List<(int, int)>();
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                if (r != safeRow || c != safeCol)
                    candidates.Add((r, c));

        // Fisher-Yates shuffle
        System.Random rng = new System.Random();
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }

        for (int i = 0; i < MineCount; i++)
        {
            var (r, c) = candidates[i];
            Cells[r, c].isMine = true;
        }
    }

    public void ComputeAdjacentCounts()
    {
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                if (Cells[r, c].isMine) continue;
                int count = 0;
                for (int dr = -1; dr <= 1; dr++)
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        int nr = r + dr, nc = c + dc;
                        if (nr >= 0 && nr < Rows && nc >= 0 && nc < Cols && Cells[nr, nc].isMine)
                            count++;
                    }
                Cells[r, c].adjacentMines = count;
            }
        }
    }

    public void Reveal(int row, int col)
    {
        if (row < 0 || row >= Rows || col < 0 || col >= Cols) return;
        if (Cells[row, col].isRevealed || Cells[row, col].isFlagged) return;

        Cells[row, col].isRevealed = true;

        if (Cells[row, col].adjacentMines == 0 && !Cells[row, col].isMine)
        {
            for (int dr = -1; dr <= 1; dr++)
                for (int dc = -1; dc <= 1; dc++)
                    if (dr != 0 || dc != 0)
                        Reveal(row + dr, col + dc);
        }
    }

    public void ToggleFlag(int row, int col)
    {
        if (Cells[row, col].isRevealed) return;
        Cells[row, col].isFlagged = !Cells[row, col].isFlagged;
    }

    public bool CheckWin()
    {
        int unrevealed = 0;
        foreach (Cell c in Cells)
            if (!c.isRevealed) unrevealed++;
        return unrevealed == MineCount;
    }
}
