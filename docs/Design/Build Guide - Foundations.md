---
title: Minesweeper — Build Guide
date: 2026-04-24
type: build-guide
status: planning
---

# GardenSweeper — Build Guide

> **Status (2026-05-02):** Phases 1–6 complete. Phase 7 (final pass) complete — `GardenSweeper.app` built and verified. This guide is now a historical reference for how the game was assembled.

Sequenced implementation steps. Each phase produces something runnable — never a long stretch with nothing visible. Complete the design docs before starting here.

References: [UI Design](UI%20Design.md) · [Logic Design](Logic%20Design.md) · [Assets](Assets.md) · [Wiring](Wiring.md)

**Who does what:**
- **[You]** — requires Unity Hub, Unity Editor GUI, or a browser
- **[Claude]** — I execute this: file writes, script authoring, folder setup, manifest edits

---

## Phase 1 — Project and Scene Setup

- [x] **[You]** Create Unity 6.3 LTS project:
  1. Open Unity Hub → click **New project**
  2. Select the **2D (Built-In Render Pipeline)** template
  3. Set name to `minesweeper-unity`, choose your preferred save location
  4. Click **Create project**

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

- [x] **[You]** Rename default scene to `Game`, save to `Assets/Scenes/`:
  1. In the **Project** window, locate `Assets/Scenes/SampleScene`
  2. Right-click it → **Rename** → type `Game` → press Enter
  3. Press **Cmd+S** (Mac) or **Ctrl+S** (Windows) to save

- [x] **[Claude]** Add TextMeshPro to `Packages/manifest.json`

- [x] **[You]** Import TMP Essentials when Unity prompts:
  1. When the TMP importer window appears, click **Import TMP Essentials**
  2. Close the window when the import bar completes

- [x] **[Claude]** Download **DSEG7 Classic** font from GitHub (`keshikan/DSEG` releases) and copy into `Assets/Fonts/`

- [x] **[You]** Set Active Input Handling to **Both**:
  1. Open **Edit → Project Settings → Player**
  2. Expand the **Other Settings** foldout
  3. Find **Active Input Handling** — change it from "Input System Package (New)" to **Both**
  4. Click **Apply** when prompted to restart the Editor
  > Required for `OnMouseDown` / `OnMouseOver` physics messages to fire alongside the new Input System.

- [x] **[You]** Set up the camera:
  1. Select **Main Camera** in the Hierarchy
  2. In the Inspector under **Camera**:
     - Set **Projection** to `Orthographic`
     - Set **Size** to `5.75`
  3. In the Inspector under **Transform**:
     - Set **Position** to X = `4`, Y = `−4`, Z = `−10`
  4. Click the **Background** color swatch and enter hex `1A1A1A`

**Checkpoint:** Scene opens, camera shows a dark rectangle. Nothing else yet.

---

## Phase 2 — Cell Prefab

Goal: a single cell that can display every visual state.

- [x] **[You]** Create a 1×1 white quad sprite named `CellBackground`:
  1. In the **Project** window, right-click inside `Assets/Sprites`
  2. Choose **Create → 2D → Sprites → Square**
  3. Rename the new asset to `CellBackground`
  4. Select `CellBackground` in the Project window
  5. In the Inspector, confirm **Sprite Mode** is set to **Single** — click **Apply**

- [x] **[Claude]** Create `CellView.cs` stub (empty MonoBehaviour, placeholder fields)

- [x] **[You]** Build the `CellPrefab` hierarchy:
  1. In the **Hierarchy**, right-click in empty space → **Create Empty** → rename to `CellPrefab`
  2. With `CellPrefab` selected, click **Add Component** in the Inspector → search `Box Collider 2D` → add it
     - Set **Size** to X = `1`, Y = `1`
  3. Click **Add Component** again → search `Cell View` → add the CellView script
  4. Right-click `CellPrefab` in the Hierarchy → **Create Empty** → rename the new child to `Background`
  5. Select `Background`, click **Add Component** → `Sprite Renderer`
     - Drag `CellBackground` from `Assets/Sprites/` into the **Sprite** field
  6. Right-click `CellPrefab` in the Hierarchy → **Create Empty** → rename the new child to `Label`
  7. Select `Label`, click **Add Component** → search `TextMeshPro` → select **TextMeshPro - Text** (the world-space version — *not* the UI variant labelled "(UI)")
     - Set **Alignment** to Center / Middle
     - Set **Font Size** to `6`
     > TMP world-space font size is in TMP units, not Unity units. Size 6 fills a 1×1 cell correctly (1 world unit ≈ 9 TMP units).
  8. Drag the root `CellPrefab` from the Hierarchy into `Assets/Prefabs/` to create the prefab asset
  9. Delete `CellPrefab` from the scene (right-click → Delete)

- [x] **[Claude]** Add public `Background` (SpriteRenderer) and `Label` (TextMeshPro) fields to `CellView.cs`

- [x] **[You]** Wire `Background` and `Label` refs inside the prefab:
  1. In the **Project** window, double-click `Assets/Prefabs/CellPrefab` to open it in prefab isolation mode
  2. Select the root `CellPrefab` object
  3. In the Inspector under **Cell View (Script)**:
     - Drag the `Background` child (from the hierarchy panel on the left) into the **Background** field
     - Drag the `Label` child into the **Label** field
  4. Press **Cmd+S** / **Ctrl+S** to save, then click the **←** arrow at the top-left to exit prefab mode

- [x] **[Claude]** Implement `Refresh(Cell cell, bool revealAll)` in `CellView.cs` — all 7 visual states (see Wiring → Cell Refresh Logic)

- [x] **[Claude]** Create `Cell.cs` data stub in `Assets/Scripts/Core/` — needed so `CellView.cs` compiles. The full Phase 4 definition replaces this.

- [x] **[You]** Test all 7 visual states:
  1. Drag `Assets/Prefabs/CellPrefab` into the scene
  2. Create a temporary test script that calls `Refresh` with hardcoded `Cell` values for each of the 7 states (covered, flagged, empty, number, mine-hit, mine-revealed, wrong-flag)
  3. Enter Play mode and confirm each state renders correctly
  4. Exit Play mode, delete the test script, delete the cell from the scene

**Checkpoint:** One cell displays all visual states correctly.

---

## Phase 3 — Grid Instantiation

Goal: 81 cells on screen at the right positions.

- [x] **[You]** Create the `GameManager` empty GameObject:
  1. In the Hierarchy, right-click in empty space → **Create Empty**
  2. Rename it `GameManager`
  3. Confirm **Transform Position** is (0, 0, 0)

- [x] **[Claude]** Create `GameManager.cs` with:
  - Inspector field: `GameObject cellPrefab`
  - Inspector field: `Transform gridParent` (the `Grid` object)
  - `CellView[9,9] cellViews` private array
  - `BuildGrid()`: instantiates 81 cells, assigns `Row`/`Col`, scales each to `0.95×0.95` (so the dark background shows through as grid lines), calls `Refresh(new Cell(), false)`, positions at `x = col`, `y = -row`
  - Calls `BuildGrid()` in `Awake`

- [x] **[You]** Add the `GameManager` script component:
  1. Select `GameManager` in the Hierarchy
  2. Click **Add Component** → search `Game Manager` → add it

- [x] **[You]** Create the `Grid` parent object and assign it:
  1. In the Hierarchy, right-click in empty space → **Create Empty** → rename to `Grid`
  2. Confirm **Transform Position** is (0, 0, 0)
  3. Select `GameManager` in the Hierarchy
  4. In the Inspector under **Game Manager (Script)**, drag `Grid` from the Hierarchy into the **Grid Parent** field

- [x] **[You]** Assign `CellPrefab` in the inspector:
  1. Select `GameManager` in the Hierarchy
  2. Drag `Assets/Prefabs/CellPrefab` from the Project window into the **Cell Prefab** field

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

**No Unity testing needed here — the logic is pure C#. Trace through each algorithm against the Logic Design doc.**

**Checkpoint:** Board.cs compiles with no errors. All methods match the Logic Design spec.

---

## Phase 5 — Input and Game Loop

Goal: clicking cells triggers real game logic and updates the display.

- [x] **[You]** Confirm Active Input Handling is set to **Both** — done in Phase 1; no action needed

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

**Checkpoint:** Clicking cells reveals them. Flood fill opens empty regions. Right-click flags cells. Clicking a mine reveals all mines. Uncovering the board triggers the win state (verify by adding a `Debug.Log` in the win branch).

---

## Phase 6 — Header

Goal: mine counter, timer, and reset button are live.

### Scene hierarchy

```
Header                    ← empty GameObject at (4, 1.25, 0); has HeaderView script
├── Panel                 ← SpriteRenderer, CellBackground sprite, dark tint, scale (9,1,1), order 0
├── MineCounter           ← TextMeshPro, local pos (-2.5, 0, 0), DSEG7 font, red, order 1
├── Timer                 ← TextMeshPro, local pos (2.5, 0, 0), DSEG7 font, red, order 1
└── ResetButton           ← empty root; BoxCollider2D (1×1) + Clickable script
    ├── Background        ← SpriteRenderer, CellBackground sprite, grey tint, scale (0.95, 0.95, 1), order 0
    └── Label             ← TextMeshPro, face emoji, font size 5, Center/Middle, order 1
```

> **Note:** SpriteRenderer and TMP cannot share a GameObject (MeshRenderer conflict). ResetButton must be split into a root + two children, same pattern as CellPrefab.

### Steps

- [x] **[You]** Create `Header` empty GameObject:
  1. Right-click in the Hierarchy → **Create Empty** → rename to `Header`
  2. Set **Transform Position** to X = `4`, Y = `1.25`, Z = `0`

- [x] **[You]** Add `Panel` child:
  1. Right-click `Header` in the Hierarchy → **Create Empty** → rename to `Panel`
  2. Select `Panel`, click **Add Component** → **Sprite Renderer**
  3. Drag `CellBackground` into the **Sprite** field
  4. Click the **Color** swatch → enter hex `2A2A2A`
  5. Set **Transform Scale** to X = `9`, Y = `1`, Z = `1`
  6. In the Sprite Renderer component, set **Sorting Order** to `0`

- [x] **[You]** Add `MineCounter` child:
  1. Right-click `Header` → **Create Empty** → rename to `MineCounter`
  2. Select `MineCounter`, click **Add Component** → search `TextMeshPro` → select **TextMeshPro - Text** (world-space, not UI)
  3. Set the text content to `010`
  4. Set **Transform Local Position** to X = `−2.5`, Y = `0`, Z = `0`
  5. Scroll to **Extra Settings** in the TMP component → set **Sorting Order** to `1`

- [x] **[You]** Add `Timer` child:
  1. Right-click `Header` → **Create Empty** → rename to `Timer`
  2. Select `Timer`, click **Add Component** → **TextMeshPro - Text** (world-space)
  3. Set text to `000`
  4. Set **Transform Local Position** to X = `2.5`, Y = `0`, Z = `0`
  5. In **Extra Settings**, set **Sorting Order** to `1`

- [x] **[You]** Add `ResetButton` child:
  1. Right-click `Header` → **Create Empty** → rename to `ResetButton`
  2. Select `ResetButton`, click **Add Component** → **Box Collider 2D** → set Size to X = `1`, Y = `1`
     > The collider must be on `ResetButton` itself — not on `Background` or `Label`. `OnMouseDown` only fires when the collider and the script are on the same GameObject.
  3. Click **Add Component** → search `Clickable` → add the Clickable script
  4. Leave **Transform Local Position** at (0, 0, 0)

- [x] **[You]** Add `Background` child under `ResetButton`:
  1. Right-click `ResetButton` → **Create Empty** → rename to `Background`
  2. Select `Background`, click **Add Component** → **Sprite Renderer**
  3. Drag `CellBackground` into the **Sprite** field
  4. Click the **Color** swatch → enter hex `555555`
  5. Set **Transform Scale** to X = `0.95`, Y = `0.95`, Z = `1`
  6. Set **Sorting Order** to `0`

- [x] **[You]** Add `Label` child under `ResetButton`:
  1. Right-click `ResetButton` → **Create Empty** → rename to `Label`
  2. Select `Label`, click **Add Component** → **TextMeshPro - Text** (world-space)
  3. Set text to `🙂` (placeholder — HeaderView.cs replaces this with a sprite tag at runtime)
  4. Set **Font Size** to `5`
  5. Set **Alignment** to Center / Middle
  6. In **Extra Settings**, set **Sorting Order** to `1`

- [x] **[Claude]** Create `Clickable.cs` — `OnMouseDown` raises `Action OnClick`
- [x] **[Claude]** Create `HeaderView.cs` — manages counter, timer, and face; `Refresh(GameState, int, int)`; subscribes to `ResetButton.OnClick`
- [x] **[Claude]** Expand `GameManager.cs` — adds `HeaderView` ref, timer in `Update`, `ResetGame()`, `RefreshHeader()` calls after every state change

- [x] **[You]** Convert DSEG7 font to a TMP Font Asset:
  1. Open **Window → TextMeshPro → Font Asset Creator**
  2. In the **Source Font File** field, click the circle icon → select `DSEG7Classic-Regular.ttf` from `Assets/Fonts/`
  3. Leave all other settings at their defaults (SDF mode, Atlas Resolution 512×512)
  4. Click **Generate Font Atlas** — wait for the progress bar to finish
  5. Click **Save** → save the asset into `Assets/Fonts/` as `DSEG7Classic-Regular SDF`
  6. Dismiss any "missing characters" warnings — DSEG7 only contains digits and basic punctuation, so missing letters are expected

- [x] **[You]** Apply DSEG7 font and red color to `MineCounter` and `Timer`:
  1. Select `MineCounter` in the Hierarchy
  2. In the Inspector under **TextMeshPro**, click the **Font Asset** circle icon → select `DSEG7Classic-Regular SDF`
  3. Click the **Color** swatch → enter hex `FF0000`
  4. Select `Timer` in the Hierarchy and repeat steps 2–3

- [x] **[You]** Set the Sprite Asset on `Label` (enables emoji rendering):
  1. Select `Label` (child of `ResetButton`) in the Hierarchy
  2. In the Inspector under **TextMeshPro**, scroll to **Extra Settings**
  3. Click the **Sprite Asset** circle icon → select `EmojiOne` from `Assets/TextMesh Pro/Resources/Sprite Assets/`

- [x] **[You]** Add `HeaderView` script to the `Header` GameObject:
  1. Select `Header` in the Hierarchy
  2. Click **Add Component** → search `Header View` → add it

- [x] **[You]** Wire `HeaderView` inspector refs:
  1. Select `Header` in the Hierarchy
  2. In the Inspector under **Header View (Script)**:
     - **Mine Counter** → drag the `MineCounter` GameObject here (Unity extracts its TMP component automatically)
     - **Reset Label** → drag the `Label` GameObject (child of `ResetButton`) here
     - **Timer** → drag the `Timer` GameObject here
     - **Reset Button** → drag the `ResetButton` GameObject here (Unity extracts its Clickable component)

- [x] **[You]** Wire the `GameManager` header ref:
  1. Select `GameManager` in the Hierarchy
  2. In the Inspector under **Game Manager (Script)**, drag the `Header` GameObject into the **Header View** field (Unity extracts its HeaderView component)

> **Emoji note:** The TMP EmojiOne atlas only ships with 16 faces. `HeaderView.cs` uses TMP sprite tags (`<sprite name="263a">` etc.) instead of raw emoji characters. The faces used are ☺/😅/😎/☹ as substitutes for the original spec's 🙂/😮/😎/😵. Swap them out during the art pass.

**Checkpoint:** Mine counter shows `010`, timer shows `000`, face shows ☺. First click starts timer. Flagging decrements counter. Win → 😎, lose → ☹. Reset button restarts everything.

---

## Phase 7 — Final Pass

- [x] **[You]** Verify all cell visual states (covered, flagged, question, revealed empty, revealed number, mine hit, mine revealed, wrong flag)
- [x] **[You]** Verify mine counter goes negative when over-flagged
- [x] **[You]** Verify first click is always safe
- [x] **[You]** Verify flood fill behavior
- [x] **[You]** Verify win detection
- [x] **[You]** Verify wrong-flag display on loss
- [x] **[You]** Verify weather icon cycles through all 6 states across a full game
- [x] **[You]** `GardenSweeper.app` built and verified on Mac desktop

**Checkpoint:** All verification items pass. `GardenSweeper.app` shipped.
