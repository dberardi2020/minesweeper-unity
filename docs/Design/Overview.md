---
title: Minesweeper
date: 2026-04-24
type: overview
status: active
---

# Minesweeper — Overview

A Minesweeper clone built in Unity 6.3 LTS. Second game from Round To It Studio.

---

## Concept

Classic Minesweeper: a grid of covered cells, some hiding mines. Left-click to reveal a cell; right-click to flag a suspected mine. Numbers indicate how many adjacent cells contain mines. Reveal all safe cells to win; hit a mine to lose.

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
- Three difficulty presets: Beginner (9×9, 10 mines), Intermediate (16×16, 40 mines), Expert (30×16, 99 mines)
- Left-click to reveal a cell
- Right-click to flag / unflag a cell
- Flood fill: revealing an empty cell (0 adjacent mines) recursively reveals its neighbors
- First click is always safe — mines are placed after the first click
- Number display on revealed cells (1–8) using sprite icons (1–3) and colored TMP text (4–8)
- Win condition: all non-mine cells revealed
- Lose condition: clicking a mine — all mines revealed
- Header UI: mine counter, reset button (face emoji), timer — TMP-based, tile-based header pending
- Bloom animation on flood fill reveal (flowers appear in a wave)
- Sprite-based cell tiles (covered, revealed variants, mine-hit, flowers, flags)
- Runtime difficulty selector (3 buttons, screen-space overlay)

### Out of Scope (current)
- Custom grid size
- Sound and music
- Persistent high scores
- Tile-based header (backlog — awaiting artist assets)

---

## Stack

- Unity 6.3 LTS (6000.3.x)
- C# / MonoBehaviour
- Project: `Round To It Studio/Minesweeper/minesweeper-unity/`
