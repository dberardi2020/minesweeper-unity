---
title: Minesweeper — Build Guide
date: 2026-04-24
type: build-guide
status: planning
---

# Minesweeper — Build Guide

Sequenced implementation steps. Each phase produces something runnable — never a long stretch with nothing visible. Complete the design docs before starting here.

References: [UI Design](UI%20Design.md) · [Logic Design](Logic%20Design.md) · [Assets](Assets.md) · [Wiring](Wiring.md)

---

## Phase 1 — Project and Scene Setup

- [ ] Create Unity 6.3 LTS project: `minesweeper-unity`, 2D template
- [ ] Delete default sample assets
- [ ] Create folder structure under `Assets/`:
  ```
  Assets/
  ├── Fonts/
  ├── Prefabs/
  ├── Scenes/
  ├── Scripts/
  │   └── Core/        ← pure C# (Board, Cell, GameState)
  └── Sprites/         ← empty for now; populated in sprite pass post-v1
  ```
- [ ] Rename default scene to `Game`, save to `Assets/Scenes/`
- [ ] Install **TextMeshPro** via Package Manager → import TMP Essentials when prompted
- [ ] Download and import **DSEG7 Classic** font (dseg.tagimakot.com) into `Assets/Fonts/`
- [ ] Set camera: Orthographic, Size 5.75, Position (4, -4, -10), Background color `#1A1A1A`

**Checkpoint:** Scene opens, camera shows a dark rectangle. Nothing else yet.

---

## Phase 2 — Cell Prefab

Goal: a single cell that can display every visual state.

- [ ] Create a 1×1 white quad sprite (Unity built-in `Sprites/Square`) — this is the cell background
- [ ] Create `CellPrefab`:
  - Root GameObject: add `BoxCollider2D` (size 1×1), add `CellView` script (stub for now)
  - Child `Background`: add `SpriteRenderer` with the white square sprite
  - Child `Label`: add `TextMeshPro` (world-space), center-aligned, font size ~0.6, no overflow
- [ ] In `CellView.cs`, add public fields for `Background` (SpriteRenderer) and `Label` (TextMeshPro), wire them in the prefab inspector
- [ ] Implement `Refresh(Cell cell, bool revealAll)` — set background tint and label text/color for all 7 visual states (see Wiring → Cell Refresh Logic)
- [ ] Test: temporarily place one CellPrefab in the scene and call `Refresh` with hardcoded states from a test script. Verify all 7 states look correct. Remove test script when done.

**Checkpoint:** One cell displays all visual states correctly.

---

## Phase 3 — Grid Instantiation

Goal: 81 cells on screen at the right positions.

- [ ] Create `GameManager` empty GameObject in scene
- [ ] Add `GameManager.cs` script with:
  - Inspector field: `GameObject cellPrefab`
  - Inspector field: `Transform gridParent` (the `Grid` object)
  - `CellView[9,9] cellViews` array (private)
  - `BuildGrid()` method: instantiates 81 cells, sets `Row`/`Col` on each `CellView`, stores in array
  - Call `BuildGrid()` in `Awake`
- [ ] Create empty `Grid` GameObject, assign to `GameManager.gridParent`
- [ ] Position formula: `x = col`, `y = -row` (see Wiring → Grid Layout)

**Checkpoint:** 81 covered cells fill the screen in a 9×9 grid.

---

## Phase 4 — Pure C# Model

Goal: the full game model written and verified independent of Unity.

- [ ] Create `GameState.cs` — enum: `Ready`, `Playing`, `Won`, `Lost`
- [ ] Create `Cell.cs` — struct with `isMine`, `isRevealed`, `isFlagged`, `adjacentMines` (see Logic Design)
- [ ] Create `Board.cs`:
  - [ ] Constructor: initializes `Cell[9,9]` in default (covered, no mines) state
  - [ ] `PlaceMines(int safeRow, int safeCol)`: random placement excluding safe cell
  - [ ] `ComputeAdjacentCounts()`: iterates all cells, counts mine neighbors
  - [ ] `Reveal(int row, int col)`: marks revealed; recursive flood fill if `adjacentMines == 0`
  - [ ] `ToggleFlag(int row, int col)`: toggles `isFlagged` on covered cells only
  - [ ] `CheckWin()`: returns true if unrevealed count equals mine count
  - [ ] `FlagCount` property: count of flagged cells

**No Unity testing needed here — the logic is pure C#. Trace through each algorithm on paper or in your head using the Logic Design doc as the reference.**

**Checkpoint:** Board.cs compiles with no errors. All methods match the Logic Design spec.

---

## Phase 5 — Input and Game Loop

Goal: clicking cells triggers real game logic and updates the display.

- [ ] Add `Action<int,int> OnLeftClick` and `Action<int,int> OnRightClick` events to `CellView`
- [ ] Implement `OnMouseDown` in `CellView`: raise the appropriate event based on `Input.GetMouseButtonDown(0/1)`
- [ ] In `GameManager`:
  - [ ] Create `Board` in `Awake` (or `ResetGame`)
  - [ ] Add `GameState _state` field
  - [ ] After building grid, subscribe to each `CellView`'s events
  - [ ] `HandleLeftClick(row, col)`:
    - If `Ready`: call `Board.PlaceMines`, `Board.ComputeAdjacentCounts`, set state to `Playing`
    - Call `Board.Reveal(row, col)`
    - If revealed cell was a mine: set state to `Lost`, call `RefreshAll(revealAll: true)`
    - Else if `Board.CheckWin()`: set state to `Won`, call `RefreshAll`
    - Else: call `RefreshAll` (flood fill may have changed many cells)
  - [ ] `HandleRightClick(row, col)`:
    - Ignore if state is not `Playing`
    - Call `Board.ToggleFlag(row, col)`, refresh that cell
  - [ ] `RefreshAll(bool revealAll)`: loop over all cells, call `cellViews[r,c].Refresh(board[r,c], revealAll)`

**Checkpoint:** Clicking cells reveals them. Flood fill opens empty regions. Right-click flags cells. Clicking a mine reveals all mines. Uncovering the board triggers the win state (verify by placing a breakpoint or Debug.Log).

---

## Phase 6 — Header

Goal: mine counter, timer, and reset button are live.

- [ ] Create `Header` GameObject above the grid (y ≈ 1.25), add `HeaderView.cs`
- [ ] Add child objects: `Panel` (SpriteRenderer quad), `MineCounter` (TMP), `Timer` (TMP), `ResetButton` (SpriteRenderer + TMP + BoxCollider2D)
- [ ] In `HeaderView.cs`:
  - Inspector refs to all four child objects
  - `Action OnResetClick` event
  - `Refresh(GameState state, int minesRemaining, int seconds)`: updates counter (zero-padded 3-digit), timer (zero-padded, capped at 999), and button face emoji
  - `OnMouseDown` on reset button raises `OnResetClick`
- [ ] Set `MineCounter` and `Timer` to use DSEG7 font, red text on dark panel
- [ ] In `GameManager`:
  - Inspector ref to `HeaderView`
  - Subscribe to `HeaderView.OnResetClick`
  - `ResetGame()`: new Board, reset state to Ready, timer to 0, rebuild or refresh all cells, refresh header
  - Call `HeaderView.Refresh(...)` after every state change
  - Timer: in `Update`, if state is `Playing`, increment `_timer` by `Time.deltaTime`, call `HeaderView.Refresh` each second

**Checkpoint:** Mine counter decrements on flag. Timer counts up from first click and stops on win/loss. Reset button restarts the game. Face emoji changes with game state.

---

## Phase 7 — Final Pass

- [ ] Verify all 7 cell visual states appear correctly in a real game session
- [ ] Verify mine counter can go negative when over-flagged
- [ ] Verify timer caps display at 999
- [ ] Verify first click is always safe (play 10+ times, never lose on first click)
- [ ] Verify flood fill never reveals mines or flagged cells
- [ ] Verify win is detected correctly — try a game where you methodically uncover every safe cell
- [ ] Verify wrong-flag state shows on loss for flagged non-mine cells
- [ ] Read through all five design docs. Confirm every decision documented in them is reflected in the built game.

**Checkpoint:** All verification items pass. Docs reviewed. Ship.
