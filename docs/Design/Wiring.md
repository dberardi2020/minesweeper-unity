---
title: GardenSweeper — Wiring
date: 2026-04-24
updated: 2026-05-02
type: design
status: active
---

# GardenSweeper — Wiring

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
├── EventSystem           ← required for EventSystem.current checks in input handlers
├── DifficultySelector    ← screen-space Canvas overlay with 3 preset buttons
├── GameManager           ← empty GameObject; owns the whole game loop
├── Grid                  ← empty at edit-time; children instantiated at runtime by GameManager
└── Header                ← has HeaderView script; Build() creates tile bg and digit displays at runtime
    ├── MineCounter        ← TextMeshPro fallback (deactivated if tile header sprites wired)
    ├── Timer              ← TextMeshPro fallback (deactivated if tile header sprites wired)
    └── ResetButton        ← BoxCollider2D + Clickable; SpriteRenderer added at runtime if weather sprites wired
        └── ResetLabel     ← TextMeshPro fallback face (hidden if weather sprites wired)
```

`HeaderView.Build(cols)` runs at game init and on every reset. It destroys any previously created runtime objects, then creates:
- `Header_Bg` — child containing one tile SpriteRenderer per column
- `MineDisplay` and `TimerDisplay` — DigitDisplay components positioned at counter areas

---

## Camera

- **Projection:** Orthographic
- **Size:** calculated at runtime by `FitCamera()` — snapped to a multiple-of-20 PPU so the 0.05-unit cell gap is always exactly 1 pixel (see Cell Gap and Pixel Snapping note)
- **Position:** centered on the grid + header stack, recalculated on each reset
- **Background:** dark color (matches the window background)

---

## Prefabs

### CellPrefab

One prefab represents a single grid cell. Instantiated at runtime (`_rows × _cols` times).

```
CellPrefab (GameObject)
├── BoxCollider2D          ← 1×1, detects mouse clicks (OnMouseDown / OnMouseOver)
├── CellView               ← script (see below)
└── Children:
    ├── Background         ← SpriteRenderer (sorting order 0) — background tile
    ├── Icon               ← SpriteRenderer (sorting order 1) — flag, mine, number sprite, flower
    └── Label              ← TextMeshPro world-space (sorting order 2) — number fallback for 5–8
```

No header prefab — Header elements exist statically in the scene and are referenced by HeaderView.

---

## Scripts

### Board.cs — Pure C#, no MonoBehaviour

Owns the data model and all game logic. No Unity imports.

| Responsibility | Details |
|----------------|---------|
| Holds `Cell[rows, cols]` | Grid state for current difficulty |
| `PlaceMines(int safeRow, int safeCol)` | Called on first click — excludes clicked cell |
| `ComputeAdjacentCounts()` | Fills `adjacentMines` for all non-mine cells |
| `Reveal(int row, int col)` | Marks cell revealed; recursive flood fill if count == 0 |
| `CycleMarker(int row, int col)` | Cycles covered cell: none → flagged → question → none |
| `CheckWin()` | Returns true if unrevealed non-mine count == 0 |
| `FlagCount` property | Count of flagged cells (used by mine counter) |

### GameManager.cs — MonoBehaviour

The central controller. Owns the Board, drives the game loop, and tells the display objects when to update.

| Responsibility | Details |
|----------------|---------|
| `ApplyDifficulty()` | Reads `DifficultyConfig.Get(difficulty)` → sets `_rows`, `_cols`, `_mineCount` |
| `InitGame()` | Creates Board, positions Header, calls `headerView.Build(_cols)`, builds grid, fits camera |
| `BuildGrid()` | Instantiates `_rows × _cols` CellPrefabs; assigns Row/Col, subscribes to events |
| `FitCamera()` | Computes ortho size snapped to a multiple-of-20 PPU for pixel-perfect cell gaps |
| `HandleLeftClick(row, col)` | On first click: places mines and starts Playing; then reveals, checks win/loss |
| `HandleRightClick(row, col)` | Calls `Board.CycleMarker`; refreshes that cell |
| `RefreshAll(bool revealAll)` | Calls `CellView.Refresh` on every cell |
| `RefreshHeader()` | Single callsite: calls `headerView.Refresh(_state, minesRemaining, seconds, SafeProgress())` |
| `SafeProgress()` | Revealed safe cells ÷ total safe cells (0–1); returns 0 in Ready state |
| `Bloom(originRow, originCol, prevRevealed)` | Coroutine: animates flowers appearing in a radiating wave after flood fill |
| `ResetGame()` | Destroys all grid children, calls `InitGame()` |
| `SetDifficulty(Difficulty d)` | Changes preset and calls `ResetGame()` |

`Update()` advances the timer while Playing and routes through `RefreshHeader()`. A Q key press toggles `CellView.UseV2Question` (alternate question mark sprite) and refreshes all cells.

### CellView.cs — MonoBehaviour

Lives on each CellPrefab instance. Knows its position in the grid and how to display any cell state. Does not know about Board or game rules.

| Responsibility | Details |
|----------------|---------|
| `int Row, Col` | Set by GameManager at instantiation |
| `Action<int,int> OnLeftClick` | Raised on `OnMouseDown` (left button) |
| `Action<int,int> OnRightClick` | Raised on `OnMouseOver` + right button pressed this frame |
| `Refresh(Cell cell, bool revealAll)` | Updates Background sprite, Icon sprite, and Label text/color |
| `HideIcon()` | Hides the Icon renderer (used to prep cells before the bloom animation) |
| `ShowFlower()` | Sets a random flower sprite on the Icon renderer (used by the bloom coroutine) |

**Sprite fields:**

- Background tiles: `SpriteCovered`, `SpriteCoveredV2` (alternate raised tile), `SpriteCoveredBlank` (flat — used after a cell has been marked), `SpriteRevealed1/2/3` (three revealed variants), `SpriteMineHit`
- Icon overlays: `SpriteFlag`, `SpriteFlagWrong`, `SpriteQuestion1`, `SpriteQuestion2`, mine sprite
- `SpriteNumbers[]` — size 9; index 0 unused, indices 1–8 map to `adjacentMines` value; null slot falls back to colored TMP text
- `SpriteFlowers[]` — randomly selected per cell position; shown on empty-revealed cells

**Cell Refresh Logic** (evaluated in order):

```
if revealAll and cell.isMine and !cell.isRevealed   → mine-revealed: RevealedVariant() bg + mine icon
if revealAll and cell.isFlagged and !cell.isMine    → wrong-flag: covered bg + SpriteFlagWrong
else if cell.isRevealed and cell.isMine             → mine-hit: SpriteMineHit bg + mine icon
else if cell.isRevealed and adjacentMines > 0       → SetNumber(n): sprite if SpriteNumbers[n] != null, else TMP
else if cell.isRevealed                             → empty: RevealedVariant() bg + FlowerVariant() icon
else if cell.isFlagged                              → flagged: covered bg + SpriteFlag icon
else if cell.isQuestion                             → question: covered bg + question sprite
else                                                → covered: SpriteCoveredBlank (if hasBeenMarked) or SpriteCovered[V2]
```

`RevealedVariant()` cycles through 3 revealed tile sprites based on `(row * 9 + col) % 3` — breaks up visual monotony.
`FlowerVariant()` picks from `SpriteFlowers[]` using `(row * 7 + col * 13) % count` — same cell always gets the same flower.

### DigitDisplay.cs — MonoBehaviour

Created at runtime by `HeaderView.Build()`. Owns three digit SpriteRenderers.

| Responsibility | Details |
|----------------|---------|
| `Sprite[] Sprites` | Set by HeaderView; indices 0–9 = digits, 10 = minus sign |
| `Awake()` | Creates three child GameObjects with SpriteRenderers, positioned at 1px padding + 10px per digit |
| `Show(int value)` | Clamps to -99..999; maps hundreds/tens/ones to sprite indices |

### HeaderView.cs — MonoBehaviour

Assigned to the `Header` GameObject. Manages the tile-based header background, digit displays, and weather icon.

| Responsibility | Details |
|----------------|---------|
| Inspector refs — TMP fallback | `MineCounter`, `Timer`, `ResetLabel` (TextMeshPro); `ResetButton` (Clickable) |
| Inspector refs — tile sprites | `HeaderCapLeft/Right`, `HeaderMiddle/Middle2`, `HeaderCounterLeft/Center/Right`, `DigitSprites[]` (11 sprites) |
| Inspector refs — weather sprites | `WeatherRain`, `WeatherDarkCloud`, `WeatherLightCloud`, `WeatherSun`, `WeatherStorm`, `WeatherRainbow` |
| `Build(int cols)` | Destroys previous runtime objects; deactivates TMP fallbacks; creates tile bg + DigitDisplay instances; adds SpriteRenderer to ResetButton if weather sprites wired |
| `Refresh(GameState, int minesRemaining, int seconds, float progress = 0f)` | Updates digit displays (or TMP fallback); sets weather icon via `WeatherForProgress()` |
| `Action OnResetClick` | Event raised when reset button is clicked |

**Tile layout** for a grid of `cols` columns:

```
[CapLeft][CounterL][CounterC][CounterR][Middle×(cols-8)][CounterL][CounterC][CounterR][CapRight]
```

Middle tiles alternate between `HeaderMiddle` and `HeaderMiddle2` by column parity.

**Weather icon logic:**

```
Lost            → WeatherStorm
Won             → WeatherRainbow
progress ≥ 0.75 → WeatherSun
progress ≥ 0.50 → WeatherLightCloud
progress ≥ 0.25 → WeatherDarkCloud
default         → WeatherRain
```

### Clickable.cs — MonoBehaviour

Minimal script on `ResetButton`. `OnMouseDown` raises `Action OnClick`. GameManager and HeaderView subscribe to this.

### DifficultySelector.cs — MonoBehaviour

Screen-space Canvas with three buttons. Each button calls `GameManager.SetDifficulty(Difficulty.X)`.

---

## Data Flow

### Left-click on a cell

```
CellView.OnMouseDown()
  → raises OnLeftClick(row, col)
    → GameManager.HandleLeftClick(row, col)
      → if Ready: Board.PlaceMines(row, col), ComputeAdjacentCounts, state = Playing
      → Board.Reveal(row, col)
      → if hit mine: state = Lost, RefreshAll(revealAll: true)
      → else if CheckWin: state = Won, RefreshAll
      → else: RefreshAll, start Bloom coroutine
      → RefreshHeader()
```

### Right-click on a cell

```
CellView.OnMouseOver() + right button
  → raises OnRightClick(row, col)
    → GameManager.HandleRightClick(row, col)
      → Board.CycleMarker(row, col)
      → CellView[row,col].Refresh(...)
      → RefreshHeader()
```

### Reset

```
ResetButton Clickable.OnMouseDown()
  → raises OnClick
    → HeaderView forwards → GameManager.ResetGame()
      → destroy all Grid children
      → InitGame() → new Board, state = Ready, BuildGrid(), headerView.Build(_cols), FitCamera()
      → RefreshHeader()
```

### Difficulty change

```
DifficultySelector button click
  → GameManager.SetDifficulty(d)
    → ApplyDifficulty() → updates _rows, _cols, _mineCount
    → ResetGame()
```

---

## Grid Layout

Cells are positioned in world space. Row 0 is the top of the grid, col 0 is the left.

```
position.x = col          (left to right)
position.y = -(row)       (0 at top, -(_rows-1) at bottom)
```

The grid parent is offset so the header sits at `y ≈ 1`. Camera is centered on the midpoint of the full scene (grid + header).
