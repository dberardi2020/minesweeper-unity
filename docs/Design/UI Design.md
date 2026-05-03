---
title: GardenSweeper — UI Design
date: 2026-04-24
updated: 2026-05-02
type: design
status: active
---

# GardenSweeper — UI Design

What the player sees and interacts with. No Unity architecture here — that lives in Wiring.

---

## Layout

```
┌─────────────────────────────┐
│  [010]    [ 🌦 ]    [000]   │  ← Header
├─────────────────────────────┤
│  . . . . . . . . .          │
│  . . . . . . . . .          │
│  . . . . . . . . .          │
│  . . . . . . . . .          │  ← Grid (9×9 shown; scales to 16×16 and 16×30)
│  . . . . . . . . .          │
│  . . . . . . . . .          │
│  . . . . . . . . .          │
│  . . . . . . . . .          │
│  . . . . . . . . .          │
└─────────────────────────────┘
```

**Header (left to right):**
- Mine counter — mines remaining (total mines minus flags placed)
- Reset button — weather icon that reflects game progress; click to restart
- Timer — seconds elapsed since first click

---

## Cell Visual States

Each cell in the grid has one of the following visual states:

| State | When | Display |
|-------|------|---------|
| Covered | Default — not yet revealed | Raised/blank tile |
| Flagged | Player right-clicked once | Flag icon |
| Question | Player right-clicked twice | Question mark icon |
| Revealed — empty | Revealed, 0 adjacent mines | Flat tile, flower icon (bloom animation) |
| Revealed — number | Revealed, 1–8 adjacent mines | Number (color-coded, sprite for 1–4) |
| Mine — hit | The mine the player clicked | Mine with red background |
| Mine — revealed | All other mines shown on loss | Mine icon on flat tile |
| Mine — wrong flag | Flagged cell that wasn't a mine, shown on loss | Wrong-flag icon |

**Right-click cycle:** covered → flagged → question mark → covered. A cell that has been through the cycle once uses a flat (not raised) covered tile to subtly indicate it's been touched.

---

## Number Colors

Classic Minesweeper color coding. Numbers 1–4 are rendered as sprite icons; 5–8 fall back to colored TMP text.

| Number | Color |
|--------|-------|
| 1 | Blue |
| 2 | Green |
| 3 | Red |
| 4 | Dark blue |
| 5 | Dark red |
| 6 | Teal |
| 7 | Black |
| 8 | Gray |

---

## Weather Icon States

The reset button shows a weather icon instead of a face. It reflects both game state and reveal progress.

| Condition | Icon |
|-----------|------|
| 0–24% revealed | Rain |
| 25–49% revealed | Dark Cloud |
| 50–74% revealed | Light Cloud |
| 75–99% revealed | Sun |
| Lost | Storm |
| Won | Rainbow |

Progress is measured as revealed safe cells ÷ total safe cells (0–1). The icon snaps on each refresh — large flood-fill reveals can jump multiple states in one click.

> **Fallback:** If weather sprites are not wired in the Inspector, the reset button falls back to TMP emoji faces (☺ normal, 😅 held, 😎 won, ☹ lost).

---

## Header — Mine Counter & Timer

- Both display as zero-padded 3-digit numbers (e.g. `010`, `003`)
- Mine counter decrements with each flag placed; can go negative if over-flagged
- Timer starts on first click, stops on win or loss
- Timer caps display at 999

---

## Difficulty Presets

| Preset | Grid | Mines |
|--------|------|-------|
| Beginner | 9×9 | 10 |
| Intermediate | 16×16 | 40 |
| Expert | 16×30 | 99 |

The runtime selector (3 buttons, screen-space overlay) switches difficulty and resets the game. The header and camera resize automatically.

---

## Scenes

| Scene | Purpose |
|-------|---------|
| `Game` | The only scene — header + grid, full game loop |
