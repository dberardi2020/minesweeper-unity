---
title: Minesweeper — Logic Design
date: 2026-04-24
type: design
status: planning
---

# Minesweeper — Logic Design

Pure game logic — no Unity, no MonoBehaviours. This is the data model and algorithms only.

---

## Data Model

### Cell

```csharp
struct Cell {
    bool isMine;
    bool isRevealed;
    bool isFlagged;
    int  adjacentMines;  // 0–8; only meaningful if !isMine
}
```

### Board

A 2D array of Cells: `Cell[rows, cols]` — for v1, 9x9.

---

## Game States

```
Ready → Playing → Won
                → Lost
```

| State | Description |
|-------|-------------|
| `Ready` | Board initialized, no click yet. Mines not placed yet. |
| `Playing` | First click made, mines placed, timer running. |
| `Won` | All non-mine cells revealed. |
| `Lost` | Player revealed a mine. |

---

## Algorithms

### Mine Placement

Runs on first click, after the target cell is known — guarantees the first click is always safe.

1. Collect all cell positions except the clicked cell (and optionally its neighbors)
2. Shuffle and take the first N as mine positions
3. Set `isMine = true` for each
4. Compute `adjacentMines` for every non-mine cell

### Adjacent Mine Count

For each non-mine cell, count mines in the 8 neighbors (clamped to board bounds):

```
(row-1, col-1)  (row-1, col)  (row-1, col+1)
(row,   col-1)  [  cell  ]    (row,   col+1)
(row+1, col-1)  (row+1, col)  (row+1, col+1)
```

### Reveal (Flood Fill)

Triggered when the player left-clicks a covered, unflagged cell.

1. If cell is a mine → game over (Lost)
2. Mark cell as revealed
3. If `adjacentMines == 0` → recursively reveal all 8 neighbors that are covered and unflagged

Recursion terminates naturally at numbered cells and board edges.

### Win Detection

After each reveal: if the count of unrevealed cells equals the total mine count, the game is Won.

### Flag Toggle

Right-click on a covered cell: toggles `isFlagged`. Has no effect on revealed cells. Does not check if the cell is actually a mine.

---

## Constants (v1)

| Constant | Value |
|----------|-------|
| Rows | 9 |
| Cols | 9 |
| Mines | 10 |
