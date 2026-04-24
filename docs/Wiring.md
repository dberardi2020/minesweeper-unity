---
title: Minesweeper — Wiring
date: 2026-04-24
type: design
status: planning
---

# Minesweeper — Wiring

How the game is assembled in Unity — scene hierarchy, prefabs, scripts, and the flow of data between them. This doc assumes the UI Design, Logic Design, and Assets docs are understood.

---

## Core Principle

The pure C# model (Board, Cell, GameState) knows nothing about Unity. MonoBehaviours handle only display and input — they read from the model and update visuals, but never contain game rules. This separation means the game logic can be read, tested, and reasoned about without opening Unity.

---

## Scene Hierarchy

Everything lives in a single `Game` scene. All objects are in world space — no Canvas or UI system.

```
Game (scene)
├── Main Camera
├── GameManager          ← empty GameObject; owns the whole game
├── Grid                 ← empty GameObject; parent of all instantiated cells
└── Header
    ├── Panel            ← background quad
    ├── MineCounter      ← TextMeshPro
    ├── ResetButton      ← quad + TextMeshPro + BoxCollider2D
    └── Timer            ← TextMeshPro
```

The `Grid` object is empty at edit time. Its children (81 cell GameObjects) are instantiated by `GameManager` at runtime.

---

## Camera

- **Projection:** Orthographic
- **Size:** 5.75 (frames the full ~9×11 scene with slight padding — see Assets for sizing)
- **Position:** (4, -4, -10) — centered on the grid + header stack
- **Background:** dark color (matches the window background)

---

## Prefabs

### CellPrefab

One prefab represents a single grid cell. Instantiated 81 times at runtime.

```
CellPrefab (GameObject)
├── BoxCollider2D       ← 1×1, detects mouse clicks
├── CellView            ← script (see below)
└── Children:
    ├── Background      ← SpriteRenderer, 1×1 white quad sprite, tinted by code
    └── Label           ← TextMeshPro (world-space), centered, used for numbers/icons
```

No header prefab — the header elements are placed manually in the scene and referenced by `HeaderView`.

---

## Scripts

### Board.cs — Pure C#, no MonoBehaviour

Owns the data model and all game logic. No Unity imports.

| Responsibility | Details |
|----------------|---------|
| Holds `Cell[9,9]` | The grid state |
| `PlaceMines(int safeRow, int safeCol)` | Called on first click — excludes clicked cell |
| `ComputeAdjacentCounts()` | Fills `adjacentMines` for all non-mine cells |
| `Reveal(int row, int col)` | Marks cell revealed; recursive flood fill if count == 0 |
| `ToggleFlag(int row, int col)` | Toggles `isFlagged` on covered cells |
| `CheckWin()` | Returns true if unrevealed count == mine count |
| `FlagCount` property | Count of flagged cells (used by mine counter) |

### GameManager.cs — MonoBehaviour

The central controller. Owns the Board, drives the game loop, and tells the display objects when to update.

| Responsibility | Details |
|----------------|---------|
| Creates the Board | On Awake or Reset |
| Instantiates the grid | 81 CellPrefabs at positions derived from row/col |
| Holds `CellView[9,9]` | Reference array for refresh calls |
| Holds `GameState` | Current state: Ready, Playing, Won, Lost |
| Handles cell left-click | Delegates to Board.Reveal; calls RefreshAll or relevant cells |
| Handles cell right-click | Delegates to Board.ToggleFlag; refreshes that cell + header |
| Handles reset | Re-creates Board, resets all CellViews, resets timer |
| Runs the timer | In Update — increments counter while state is Playing |
| Refreshes HeaderView | After any state change |

`GameManager` subscribes to events raised by each `CellView` and the reset button.

### CellView.cs — MonoBehaviour

Lives on each CellPrefab instance. Knows its position in the grid and how to display any cell state. Does not know about Board or game rules.

| Responsibility | Details |
|----------------|---------|
| `int Row, Col` | Set by GameManager at instantiation |
| `Action<int,int> OnLeftClick` | Event raised on left mouse button down |
| `Action<int,int> OnRightClick` | Event raised on right mouse button down |
| `Action<int,int> OnMouseHeld` | Event raised while left button is held (for 😮 face) |
| `Refresh(Cell cell, bool revealAll)` | Updates background color and label to match cell state |

`revealAll` is true when the game is lost — triggers mine-revealed and wrong-flag states for cells that aren't yet revealed.

**Why events instead of direct calls?**
CellView shouldn't need to know GameManager exists. Events keep the dependency one-way: GameManager knows about CellViews, but CellViews are unaware of GameManager. This matches the display-only responsibility of CellView.

### HeaderView.cs — MonoBehaviour

Manages the three header elements. Assigned to the `Header` GameObject.

| Responsibility | Details |
|----------------|---------|
| Inspector refs | MineCounter (TMP), Timer (TMP), ResetButton (TMP) |
| `Refresh(GameState, int minesRemaining, int seconds)` | Updates all three elements |
| `Action OnResetClick` | Event raised when reset button is clicked |

The reset button click uses `OnMouseDown` on its `BoxCollider2D`, same pattern as cells.

---

## Data Flow

### Left-click on a cell

```
CellView.OnMouseDown()
  → raises OnLeftClick(row, col)
    → GameManager.HandleLeftClick(row, col)
      → if Ready: Board.PlaceMines(row, col), transition to Playing
      → Board.Reveal(row, col)
      → if Board hit mine: transition to Lost, RefreshAll(revealAll: true)
      → if Board.CheckWin(): transition to Won, RefreshAll
      → else: RefreshAll (flood fill may have revealed many cells)
      → HeaderView.Refresh(...)
```

### Right-click on a cell

```
CellView.OnMouseDown() (right button)
  → raises OnRightClick(row, col)
    → GameManager.HandleRightClick(row, col)
      → Board.ToggleFlag(row, col)
      → CellView[row,col].Refresh(...)
      → HeaderView.Refresh(...)
```

### Reset

```
HeaderView button OnMouseDown()
  → raises OnResetClick
    → GameManager.ResetGame()
      → new Board()
      → GameState = Ready
      → RefreshAll(revealAll: false)
      → HeaderView.Refresh(Ready, totalMines, 0)
      → timer = 0
```

---

## Grid Layout

Cells are positioned in world space. Row 0 is the top of the grid, col 0 is the left.

```
position.x = col          (0–8, left to right)
position.y = -(row)       (0 at top, -8 at bottom)
```

The grid occupies x: [0, 8], y: [-8, 0]. The header sits above at y ≈ [1, 2]. Camera is centered on the midpoint.

---

## Cell Refresh Logic

`CellView.Refresh` maps Cell data to visual state. Evaluated in this order:

```
if revealAll and cell.isMine and !cell.isRevealed → mine-revealed
if revealAll and cell.isFlagged and !cell.isMine  → wrong-flag
else if cell.isRevealed and cell.isMine           → mine-hit (should not occur in normal flow)
else if cell.isRevealed and adjacentMines > 0     → number (colored)
else if cell.isRevealed                           → empty (flat, no label)
else if cell.isFlagged                            → flagged
else                                              → covered
```

Background color and label text/color are set via the `Background` SpriteRenderer tint and `Label` TextMeshPro properties.
