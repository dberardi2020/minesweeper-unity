---
title: Minesweeper — Assets
date: 2026-04-24
type: design
status: active
---

# Minesweeper — Assets

Every asset required to build the game. For each: what it is, its spec, and how it will be created.

---

## Creation Strategy

Cells use PNG sprites for backgrounds and icon overlays; sprite refs are set in the Inspector on the `CellView` prefab. Numbers 1–3 use icon sprites; 4–8 fall back to colored TextMeshPro text. The header is still TMP-based — tile-based header with digit sprites is the next milestone (see Backlog).

All game logic (`Board.cs`, `Cell.cs`) is sprite-agnostic; swapping visuals requires only `CellView.cs` and the prefab.

---

## Cell Tiles

The grid is 9x9. Each cell has one visual state at a time.

| State | Trigger | Visual (v1) |
|-------|---------|-------------|
| Covered | Default | Mid-gray quad |
| Flagged | Right-clicked by player | Mid-gray quad + `F` |
| Revealed — empty | Revealed, 0 adjacent mines | Dark-gray quad, no content |
| Revealed — number | Revealed, 1–8 adjacent mines | Dark-gray quad + colored number (TextMeshPro) |
| Mine — hit | Player clicked this mine | Red quad + `●` |
| Mine — revealed | All other mines shown on loss | Dark-gray quad + `●` |
| Mine — wrong flag | Flagged cell that wasn't a mine, shown on loss | Dark-gray quad + `F✕` |

> Note: TMP's default font does not support color emoji. Emoji (🚩, 💣) would require a TMP Sprite Asset. Plain Unicode symbols are used for v1. Swap in a Sprite Asset post-v1 if desired.

**Number colors** — applied via TextMeshPro vertex color:

| Number | Color     | Hex     |
| ------ | --------- | ------- |
| 1      | Blue      | #0000FF |
| 2      | Green     | #007B00 |
| 3      | Red       | #FF0000 |
| 4      | Dark blue | #00007B |
| 5      | Dark red  | #7B0000 |
| 6      | Teal      | #007B7B |
| 7      | Black     | #000000 |
| 8      | Gray      | #7B7B7B |

---

## Header Assets

The header sits above the grid and contains three elements.

### Mine Counter and Timer

- Both display as zero-padded 3-digit numbers (e.g. `010`, `003`)
- v1: TextMeshPro with a monospaced font, colored red on dark background to approximate the classic LED display look
- Font: a free 7-segment / LCD-style TrueType font (see note below)

**Digit sprite spec (post-v1).** 11 sprites: digits 0–9 plus a minus sign. Each is 10px wide × 16px tall, imported at 16 PPU (matching all other sprites). Three digits fill the 32px (2-tile) counter area with 1px padding on each side: `1 + 10 + 10 + 10 + 1 = 32`. Because digits are free-positioned on the content layer (see Wiring — Header Two-Layer Rendering), narrower glyphs such as "1" can be right-aligned within their slot without adjusting the background tiles.

### Reset Button

- Displays a face icon that reacts to game state
- v1: TextMeshPro with a large emoji character, centered on a button quad

| Game State | Character |
|------------|-----------|
| Ready / Playing | 🙂 |
| Player holding click | 😮 |
| Won | 😎 |
| Lost | 😵 |

### Header Panel

- A quad behind all header elements, slightly lighter or darker than the window background
- No border art in v1 — plain flat color

---

## Layout and Sizing

Scene is sized in Unity units; `FitCamera()` recalculates orthographic size on every reset.

| Element | Size (Unity units) |
|---------|--------------------|
| Cell | 1 × 1 |
| Grid | cols × rows (difficulty-dependent) |
| Header height | ~1.5 units |
| Total scene height | rows + 1.5 + padding |

Difficulties: Beginner 9×9, Intermediate 16×16, Expert 30×16.

---

## Fonts

| Use | Font | Source |
|-----|------|--------|
| Mine counter / timer | Free 7-segment LCD font (e.g. "DSEG7 Classic") | Free download — dseg.tagimakot.com |
| All other text | Unity default (Arial) | Built-in |

The LCD font is the one asset that may need to be downloaded. Everything else uses Unity built-ins.

---

## Audio

No audio in v1. Out of scope.

---

## Sprite Pass (Post-v1)

Full sprite replacement (Option A) — all TMP characters and colored quad primitives are replaced with PNG sprites. See [Art Brief](Art%20Brief.md) for the complete asset list formatted for designer handoff.

### Layering Approach

Cell sprites use two layers: a **background tile** and an **icon overlay**.

The three background tiles cover distinct cell appearances: covered (raised bevel), revealed (flat), and mine-hit (red flat). All cell content — mine glyph, flag, X, and numbers 1-8 — is handled by icon overlay sprites on a separate layer. This means `icon_mine.png` reuses across both mine-hit and mine-revealed states. The background tiles themselves are content-free.

This avoids a combinatorial explosion of baked-in sprites and keeps the two concerns independent.

### Code Changes

All changes are in the display layer. Game logic (`Board.cs`, `Cell.cs`, `GameState.cs`) is untouched.

**`CellView.cs`**
- Add serialized sprite fields for each background tile and each icon overlay
- In `Refresh()`: replace `Background.color = ...` with `Background.sprite = ...`; set the Icon `SpriteRenderer` sprite (or null) per state
- Remove all TMP `Label` assignments for icons and numbers — the Label component can be dropped from the prefab

**`CellPrefab.prefab`**
- Add an `Icon` child GameObject with its own `SpriteRenderer` for the overlay layer
- Remove the existing TMP `Label` child

**`GameManager.cs`** — No changes required.

---

## v2 — Animated Events

Design guidance for adding animation post-v1 sprite pass. Each animation is a sprite sheet — all frames in one PNG, arranged left-to-right in a single row.

Any player-triggered event is a candidate: mine detonation, flag placement, cell reveal, win state. The art decision is which events get motion and which stay static.

See [Art Brief](Art%20Brief.md) for the v2 animation asset table with frame counts, loop behavior, and file specs.
