---
title: Minesweeper — UI Design
date: 2026-04-24
type: design
status: planning
---

# Minesweeper — UI Design

What the player sees and interacts with. No Unity architecture here — that lives in Wiring.

---

## Layout

```
┌─────────────────────────────┐
│  [010]    [ 😊 ]    [000]   │  ← Header
├─────────────────────────────┤
│  . . . . . . . . .          │
│  . . . . . . . . .          │
│  . . . . . . . . .          │
│  . . . . . . . . .          │  ← Grid (9x9)
│  . . . . . . . . .          │
│  . . . . . . . . .          │
│  . . . . . . . . .          │
│  . . . . . . . . .          │
│  . . . . . . . . .          │
└─────────────────────────────┘
```

**Header (left to right):**
- Mine counter — mines remaining (total mines minus flags placed)
- Reset button — restarts the game; face icon reacts to game state
- Timer — seconds elapsed since first click

---

## Cell Visual States

Each cell in the grid has one of the following visual states:

| State | When | Display |
|-------|------|---------|
| Covered | Default — not yet revealed | Raised/blank tile |
| Flagged | Player right-clicked | Flag icon |
| Revealed — empty | Revealed, 0 adjacent mines | Flat, blank |
| Revealed — number | Revealed, 1–8 adjacent mines | Number (color-coded) |
| Mine — hit | The mine the player clicked | Mine with red background |
| Mine — revealed | All other mines shown on loss | Mine icon |
| Mine — wrong flag | Flagged cell that wasn't a mine, shown on loss | Flag with X |

---

## Number Colors

Classic Minesweeper color coding:

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

## Reset Button States

| Game State | Button Display |
|------------|----------------|
| Ready / Playing | 🙂 |
| Player is clicking (mouse held) | 😮 |
| Won | 😎 |
| Lost | 😵 |

---

## Header — Mine Counter & Timer

- Both display as zero-padded 3-digit numbers (e.g. `010`, `003`)
- Mine counter decrements with each flag placed; can go negative if over-flagged
- Timer starts on first click, stops on win or loss
- Timer caps display at 999

---

## Scenes

| Scene | Purpose |
|-------|---------|
| `Game` | The only scene — header + grid, full game loop |
