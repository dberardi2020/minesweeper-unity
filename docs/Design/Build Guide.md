---
title: Minesweeper — Build Guide
date: 2026-04-24
type: build-guide
status: planning
---

# Minesweeper — Build Guide

Sequenced implementation steps. Each phase produces something runnable — never a long stretch with nothing visible. Complete the design docs before starting here.

References: [UI Design](UI%20Design.md) · [Logic Design](Logic%20Design.md) · [Assets](Assets.md) · [Wiring](Wiring.md)

**Who does what:**
- **[You]** — requires Unity Hub, Unity Editor GUI, or a browser
- **[Claude]** — I execute this: file writes, script authoring, folder setup, manifest edits

---

## Phase 1 — Project and Scene Setup

- [x] **[You]** Create Unity 6.3 LTS project: `minesweeper-unity`, 2D template
- [x] **[Claude]** Delete default sample assets
- [x] **[Claude]** Create folder structure under `Assets/`:
  ```
  Assets/
  ├── Fonts/
  ├── Prefabs/
  ├── Scenes/
  ├── Scripts/
  │   └── Core/        ← pure C# (Board, Cell, GameState)
  └── Sprites/         ← empty for now; populated in sprite pass post-v1
  ```
- [x] **[You]** Rename default scene to `Game`, save to `Assets/Scenes/`
- [x] **[Claude]** Add TextMeshPro to `Packages/manifest.json`
- [x] **[You]** Import TMP Essentials when Unity prompts
- [x] **[Claude]** Download **DSEG7 Classic** font from GitHub (`keshikan/DSEG` releases) and copy into `Assets/Fonts/`
- [x] **[You]** Set Active Input Handling to **Both**: Edit → Project Settings → Player → Other Settings → Active Input Handling → Both. The 2D template defaults to "New Input System Package Only", which breaks `OnMouseDown`/`OnMouseOver`. Setting to Both lets legacy physics messages work alongside the new system.
- [x] **[You]** Set camera: Orthographic, Size 5.75, Position (4, -4, -10), Background color `#1A1A1A`

**Checkpoint:** Scene opens, camera shows a dark rectangle. Nothing else yet.

---

## Phase 2 — Cell Prefab

Goal: a single cell that can display every visual state.

- [x] **[You]** Create a 1×1 white quad sprite: right-click in `Assets/Sprites` → Create → 2D → Sprites → Square, name it `CellBackground`. In the Inspector, set Sprite Mode to **Single** (not Multiple), then click Apply.
- [x] **[Claude]** Create `CellView.cs` stub (empty MonoBehaviour, placeholder fields)
- [x] **[You]** Create `CellPrefab` — hierarchy structure:
  ```
  CellPrefab
  ├── Background   (SpriteRenderer)
  └── Label        (TextMeshPro)
  ```
  - Root `CellPrefab` GameObject: add `BoxCollider2D` (size 1×1), add `CellView` script component
  - Child `Background`: add `SpriteRenderer`, assign `CellBackground` as the sprite
  - Child `Label`: add **TextMeshPro - Text** (not the UI variant — in Unity 6 this is the world-space component), set alignment Center/Middle, font size **6**, no overflow. Note: TMP world-space font size is in TMP internal units, not Unity world units — 1 world unit ≈ 9 TMP units, so size 6 fills a 1×1 cell correctly.
  - Drag the root from the Hierarchy into `Assets/Prefabs/` to create the prefab asset, then delete it from the scene
- [x] **[Claude]** Add public `Background` (SpriteRenderer) and `Label` (TextMeshPro) fields to `CellView.cs`
- [x] **[You]** Wire `Background` and `Label` refs in the prefab inspector
- [x] **[Claude]** Implement `Refresh(Cell cell, bool revealAll)` in `CellView.cs` — all 7 visual states (see Wiring → Cell Refresh Logic)
- [x] **[Claude]** Create `Cell.cs` data stub in `Assets/Scripts/Core/` — `Refresh` references it and won't compile without it. The full Phase 4 work is on `Board.cs`; `Cell.cs` is just a struct with four fields and can be created here to unblock the Phase 2 test.
- [x] **[You]** Test: place one CellPrefab in scene, use a test script to call `Refresh` with hardcoded `Cell` values covering all 7 states, verify visuals. Remove test script when done.

**Checkpoint:** One cell displays all visual states correctly.

---

## Phase 3 — Grid Instantiation

Goal: 81 cells on screen at the right positions.

- [x] **[You]** Create `GameManager` empty GameObject in scene
- [x] **[Claude]** Create `GameManager.cs` with:
  - Inspector field: `GameObject cellPrefab`
  - Inspector field: `Transform gridParent` (the `Grid` object)
  - `CellView[9,9] cellViews` array (private)
  - `BuildGrid()` method: instantiates 81 cells, sets `Row`/`Col` and scale (`0.95×0.95`) on each, calls `Refresh(new Cell(), false)`, stores in array, positions at `x = col`, `y = -row`
  - Call `BuildGrid()` in `Awake`
  - Note: cells scaled to 0.95×0.95 so the dark camera background shows through as grid lines
- [x] **[You]** Add `GameManager.cs` script component to the GameObject
- [x] **[You]** Create empty `Grid` GameObject; assign to `GameManager.gridParent` in inspector
- [x] **[You]** Assign `cellPrefab` in inspector

**Checkpoint:** 81 covered cells fill the screen in a 9×9 grid.

---

## Phase 4 — Pure C# Model

Goal: the full game model written and verified independent of Unity.

- [x] **[Claude]** Create `GameState.cs` — enum: `Ready`, `Playing`, `Won`, `Lost`
- [x] **[Claude]** Create `Cell.cs` — struct with `isMine`, `isRevealed`, `isFlagged`, `adjacentMines` (see Logic Design)
- [x] **[Claude]** Create `Board.cs`:
  - [x] Constructor: initializes `Cell[9,9]` in default (covered, no mines) state
  - [x] `PlaceMines(int safeRow, int safeCol)`: random placement excluding safe cell
  - [x] `ComputeAdjacentCounts()`: iterates all cells, counts mine neighbors
  - [x] `Reveal(int row, int col)`: marks revealed; recursive flood fill if `adjacentMines == 0`
  - [x] `ToggleFlag(int row, int col)`: toggles `isFlagged` on covered cells only
  - [x] `CheckWin()`: returns true if unrevealed count equals mine count
  - [x] `FlagCount` property: count of flagged cells

**No Unity testing needed here — the logic is pure C#. Trace through each algorithm on paper or in your head using the Logic Design doc as the reference.**

**Checkpoint:** Board.cs compiles with no errors. All methods match the Logic Design spec.

---

## Phase 5 — Input and Game Loop

Goal: clicking cells triggers real game logic and updates the display.

- [x] **[You]** Set Active Input Handling to **Both**: Edit → Project Settings → Player → Other Settings → Active Input Handling → Both. Restart when prompted. Required for `OnMouseDown`/`OnMouseOver` physics messages to fire alongside the new Input System.
- [x] **[Claude]** Add `Action<int,int> OnLeftClick` and `Action<int,int> OnRightClick` events to `CellView`
- [x] **[Claude]** Implement input in `CellView`: `OnMouseDown` raises `OnLeftClick`; `OnMouseOver` + `Mouse.current.rightButton.wasPressedThisFrame` raises `OnRightClick`
- [x] **[Claude]** Expand `GameManager.cs`:
  - [x] Create `Board` in `Awake` (or `ResetGame`)
  - [x] Add `GameState _state` field
  - [x] After building grid, subscribe to each `CellView`'s events
  - [x] `HandleLeftClick(row, col)`:
    - If `Ready`: call `Board.PlaceMines`, `Board.ComputeAdjacentCounts`, set state to `Playing`
    - Call `Board.Reveal(row, col)`
    - If revealed cell was a mine: set state to `Lost`, call `RefreshAll(revealAll: true)`
    - Else if `Board.CheckWin()`: set state to `Won`, call `RefreshAll`
    - Else: call `RefreshAll` (flood fill may have changed many cells)
  - [x] `HandleRightClick(row, col)`:
    - Ignore if state is not `Playing`
    - Call `Board.ToggleFlag(row, col)`, refresh that cell
  - [x] `RefreshAll(bool revealAll)`: loop over all cells, call `cellViews[r,c].Refresh(board[r,c], revealAll)`

**Checkpoint:** Clicking cells reveals them. Flood fill opens empty regions. Right-click flags cells. Clicking a mine reveals all mines. Uncovering the board triggers the win state (verify by placing a breakpoint or Debug.Log).

---

## Phase 6 — Header

Goal: mine counter, timer, and reset button are live.

- [ ] **[You]** Create `Header` GameObject above the grid (y ≈ 1.25)
- [ ] **[You]** Add child objects: `Panel` (SpriteRenderer quad), `MineCounter` (TMP), `Timer` (TMP), `ResetButton` (SpriteRenderer + TMP + BoxCollider2D)
- [ ] **[Claude]** Create `HeaderView.cs`:
  - Inspector refs to all four child objects
  - `Action OnResetClick` event
  - `Refresh(GameState state, int minesRemaining, int seconds)`: updates counter (zero-padded 3-digit), timer (zero-padded, capped at 999), and button face emoji
  - `OnMouseDown` on reset button raises `OnResetClick`
- [ ] **[You]** Add `HeaderView.cs` component to `Header` GameObject; wire all inspector refs
- [ ] **[You]** Set `MineCounter` and `Timer` to DSEG7 font, red text on dark panel
- [ ] **[Claude]** Expand `GameManager.cs`:
  - Inspector ref to `HeaderView`
  - Subscribe to `HeaderView.OnResetClick`
  - `ResetGame()`: new Board, reset state to Ready, timer to 0, rebuild or refresh all cells, refresh header
  - Call `HeaderView.Refresh(...)` after every state change
  - Timer: in `Update`, if state is `Playing`, increment `_timer` by `Time.deltaTime`, call `HeaderView.Refresh` each second
- [ ] **[You]** Assign `HeaderView` ref in `GameManager` inspector

**Checkpoint:** Mine counter decrements on flag. Timer counts up from first click and stops on win/loss. Reset button restarts the game. Face emoji changes with game state.

---

## Phase 7 — Final Pass

- [ ] **[You]** Verify all 7 cell visual states appear correctly in a real game session
- [ ] **[You]** Verify mine counter can go negative when over-flagged
- [ ] **[You]** Verify timer caps display at 999
- [ ] **[You]** Verify first click is always safe (play 10+ times, never lose on first click)
- [ ] **[You]** Verify flood fill never reveals mines or flagged cells
- [ ] **[You]** Verify win is detected correctly — try a game where you methodically uncover every safe cell
- [ ] **[You]** Verify wrong-flag state shows on loss for flagged non-mine cells
- [ ] **[You]** Read through all five design docs. Confirm every decision documented in them is reflected in the built game.

**Checkpoint:** All verification items pass. Docs reviewed. Ship.
