---
title: GardenSweeper — Assets
date: 2026-04-24
updated: 2026-05-02
type: design
status: active
---

# GardenSweeper — Assets

Every asset required to build the game. For each: what it is, its spec, and how it will be created.

---

## Creation Strategy

All cell backgrounds and icon overlays are PNG sprites. Sprite refs are set in the Inspector on the `CellView` prefab. Numbers 1–4 use icon sprites; 5–8 fall back to colored TMP text (sprite slots exist but are null). The header is tile-based with sprite digits (DigitDisplay). The reset button shows a weather icon sprite that reflects game progress.

All game logic (`Board.cs`, `Cell.cs`) is sprite-agnostic; swapping visuals requires only `CellView.cs` and the prefab.

---

## Sprite Import Standard

All pixel art sprites in this project:

| Setting | Value |
|---------|-------|
| Filter Mode | Point (no filter) |
| Pixels Per Unit | 16 |
| Texture Compression | None (0) |
| sRGB | Yes |
| Sprite Mode | Single |

Deviating from these settings causes blurring or anti-aliasing on pixel art.

---

## Cell Backgrounds

Three background tiles for distinct cell appearances. All content (numbers, mine, flag, etc.) is handled by a separate icon overlay layer — backgrounds are content-free.

| File | Dimensions | Description |
|------|------------|-------------|
| `cell_covered.png` | 16×16 | Raised bevel tile — mid-gray body, white/light top and left edges, dark bottom and right edges |
| `cell_covered_v2.png` | 16×16 | Alternate raised tile variant |
| `cell_covered_blank.png` | 16×16 | Flat covered tile — used after a cell has been flagged/questioned (subtle "touched" indicator) |
| `cell_revealed_1.png` | 16×16 | Flat revealed tile variant 1 — thin dark border, no bevel |
| `cell_revealed_2.png` | 16×16 | Flat revealed tile variant 2 |
| `cell_revealed_3.png` | 16×16 | Flat revealed tile variant 3 |
| `cell_mine_hit.png` | 16×16 | Red (#FF0000) fill — mine icon layers on top |

Revealed variants are assigned by `(row * 9 + col) % 3` so the grid has visual texture without repeating.

---

## Icon Overlays

Layered on top of cell backgrounds. Transparent backgrounds. Centered on the 16×16 cell.

### Cell Icons

| File | Dimensions | Description |
|------|------------|-------------|
| `icon_mine.png` | 12×12 | Mine sprite (garden theme: rabbit) — reused across mine-hit and mine-revealed states |
| `icon_flag.png` | 12×12 | Flag sprite |
| `icon_mine_wrong.png` | 16×16 | Wrong-flag indicator — shown on flagged cells that weren't mines at game over |
| `icon_question_1.png` | 12×12 | Question mark sprite — default |
| `icon_question_2.png` | 12×12 | Question mark sprite — alternate (toggled by Q key during gameplay) |

### Number Sprites

Sprite icons for adjacentMines values 1–8. Null slot falls back to colored TMP text. Currently: 1–4 are wired; 5–8 are null.

| File | Dimensions | Color |
|------|------------|-------|
| `icon_number_1.png` | 16×16 | Blue (#0000FF) |
| `icon_number_2.png` | 16×16 | Green (#007B00) |
| `icon_number_3.png` | 16×16 | Red (#FF0000) |
| `icon_number_4.png` | 16×16 | Dark blue (#00007B) |
| `icon_number_5.png` | 16×16 | Dark red (#7B0000) — not yet wired |
| `icon_number_6.png` | 16×16 | Teal (#007B7B) — not yet wired |
| `icon_number_7.png` | 16×16 | Black (#000000) — not yet wired |
| `icon_number_8.png` | 16×16 | Gray (#7B7B7B) — not yet wired |

### Flower Sprites

Used on empty-revealed cells. Multiple variants — each cell always gets the same flower (deterministic by row/col). Stored in `SpriteFlowers[]` array on CellView.

---

## Header Assets

### Tile Background

The tile-based header uses 7 sprite types. Layout for a grid of `cols` columns:

```
[CapLeft][CounterL][CounterC][CounterR][Middle×(cols-8)][CounterL][CounterC][CounterR][CapRight]
```

| File | Description |
|------|-------------|
| `header_cap_left.png` | Left endcap tile |
| `header_cap_right.png` | Right endcap tile |
| `header_counter_left.png` | Left side of counter background area |
| `header_counter_center.png` | Center of counter background area |
| `header_counter_right.png` | Right side of counter background area |
| `header_middle.png` | Middle fill tile (even columns) |
| `header_middle_2.png` | Middle fill tile (odd columns) — alternated for visual texture |

All header tiles: 16×16px.

### Display Digits

11 sprites for the DigitDisplay component (mine counter and timer). Indices 0–9 = digits, 10 = minus sign.

| File | Dimensions | Description |
|------|------------|-------------|
| `digit_0.png` – `digit_9.png` | 13×23 | Red (#FF0000) 7-segment digit on black (#000000) background |
| `digit_minus.png` | 13×23 | Center segment only — used when mine counter goes negative |

### Weather Icons (Reset Button)

Six 16×16 sprites driving the weather icon on the reset button. Progress thresholds and game-state mappings are in Wiring → HeaderView.

| File | Condition |
|------|-----------|
| `weather_rain.png` | Default / 0–24% revealed |
| `weather_dark_cloud.png` | 25–49% revealed |
| `weather_light_cloud.png` | 50–74% revealed |
| `weather_sun.png` | 75–99% revealed |
| `weather_storm.png` | Lost |
| `weather_rainbow.png` | Won |

Location: `Assets/Sprites/header/header_weather_icons/`

---

## Layout and Sizing

Scene is sized in Unity units; `FitCamera()` recalculates orthographic size on every reset.

| Element | Size (Unity units) |
|---------|--------------------|
| Cell | 1×1 (rendered at 0.95×0.95 to create 1px gaps) |
| Grid | `cols × rows` (difficulty-dependent) |
| Header height | 1 tile = 1 unit |
| Total scene height | rows + 1 (header) + 1 (padding) |

---

## Audio

No audio. Out of scope.

---

## v2 — Animated Events

Design guidance for adding animation. Each animation is a sprite sheet — all frames in one PNG, left-to-right in a single row.

| File | Frames | Loop | Trigger |
|------|--------|------|---------|
| `anim_mine_detonate.png` | 6–8 | No | Player clicks a mine |
| `anim_flag_place.png` | 4–6 | No | Right-click places a flag |
| `anim_flag_remove.png` | 4–6 | No | Right-click removes a flag |
| `anim_cell_reveal.png` | 4–6 | No | Cell reveals |
| `anim_win.png` | 6–8 | Yes | Looping on all cells during win state |

All sheets: horizontal strips (single row), same dimensions per frame, transparent backgrounds.
