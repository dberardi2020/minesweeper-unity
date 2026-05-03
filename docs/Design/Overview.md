---
title: GardenSweeper
date: 2026-04-24
updated: 2026-05-02
type: overview
status: active
---

# GardenSweeper — Overview

A garden-themed Minesweeper clone built in Unity 6.4 LTS. Second game from Round To It Studio. Product name: **GardenSweeper** (`com.roundtoitstudio.gardensweeper`).

> **Rename in progress:** The repository folder, scene names, and script/class names still say "minesweeper". The productName and bundle identifier have been updated; everything else is backlog.

---

## Concept

Classic Minesweeper with a garden skin: a grid of covered cells, some hiding mines. Left-click to reveal a cell; right-click to cycle through flag, question mark, and back to covered. Numbers indicate how many adjacent cells contain mines. Reveal all safe cells to win; hit a mine to lose.

The "mine counter" header shows a weather icon that progresses from Rain → Dark Cloud → Light Cloud → Sun as you uncover the board. Storm on loss, Rainbow on win.

---

## Learning Goals

- Dynamic GameObject instantiation at runtime (building the grid from a prefab)
- 2D arrays and grid-based data modeling in C#
- Mouse input handling (left-click, right-click)
- Recursive algorithms in a game context (flood fill reveal)
- Separating game logic from Unity UI — pure C# model, Unity only handles display
- More complex game state management than Pong (Ready, Playing, Won, Lost)
- Timer and UI element updates driven by game state

---

## Scope

### Shipped

- Three difficulty presets: Beginner (9×9, 10 mines), Intermediate (16×16, 40 mines), Expert (16×30, 99 mines)
- Runtime difficulty selector (3 buttons, screen-space overlay)
- Left-click to reveal a cell
- Right-click to cycle: covered → flagged → question mark → covered
- Flood fill: revealing an empty cell (0 adjacent mines) recursively reveals neighbors
- First click is always safe — mines are placed after the first click
- Full sprite-based cells: covered tiles (2 variants), revealed tiles (3 variants), mine-hit, flowers
- Number display on revealed cells: sprite icons for 1–4, colored TMP text fallback for 5–8
- Win condition: all non-mine cells revealed
- Lose condition: clicking a mine — all mines revealed, wrong flags marked
- Tile-based header with sprite digit counters (DigitDisplay), weather icon on reset button
- Weather icon system: 6 states (Rain, Dark Cloud, Light Cloud, Sun, Storm, Rainbow) driven by reveal progress and game outcome
- Bloom animation on flood fill reveal (flowers appear in a radiating wave)
- Mac desktop build script (`Build → Build Mac (Desktop)` → `GardenSweeper.app`)

### Out of Scope (current)

- Custom grid size
- Sound and music
- Persistent high scores / best times

---

## Stack

- Unity 6.4 LTS (6000.4.x)
- C# / MonoBehaviour
- Project: `Round To It Studio/Minesweeper/minesweeper-unity/` (folder rename pending)
