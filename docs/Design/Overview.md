---
title: Minesweeper
date: 2026-04-24
type: overview
status: planning
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

### v1 — In Scope
- Fixed beginner grid: 9x9, 10 mines
- Left-click to reveal a cell
- Right-click to flag / unflag a cell
- Flood fill: revealing an empty cell (0 adjacent mines) recursively reveals its neighbors
- First click is always safe — mines are placed after the first click
- Number display on revealed cells (1–8)
- Win condition: all non-mine cells revealed
- Lose condition: clicking a mine — all mines revealed
- Header UI: mine counter, reset button, timer

### Out of Scope (v1)
- Difficulty selection (beginner / intermediate / expert)
- Custom grid size
- Sound and music
- Animations or visual polish
- Persistent high scores

---

## Stack

- Unity 6.3 LTS (6000.3.x)
- C# / MonoBehaviour
- Project: `Round To It Studio/Minesweeper/minesweeper-unity/`
