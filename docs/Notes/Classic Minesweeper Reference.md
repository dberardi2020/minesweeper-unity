---
title: Classic Minesweeper — Reference
date: 2026-04-30
type: reference
---

# Classic Minesweeper — Reference

How the original game works, end-to-end. This is the canonical behavior our game is built against.

---

## Grid

| Difficulty | Grid | Mines |
|------------|------|-------|
| Beginner | 9×9 | 10 |
| Intermediate | 16×16 | 40 |
| Expert | 30×16 | 99 |

All three difficulty presets are implemented.

---

## Setup

The grid starts fully covered. The player has no information — mine positions are hidden and not placed yet (see First Click below).

---

## First Click Safety

Mines are placed **after** the player's first left click, and the clicked cell is excluded from mine placement. This guarantees the player can never lose on the first move.

---

## Cell States

Every cell is in exactly one state at a time.

| State | Visible When |
|-------|-------------|
| Covered | Default — not yet revealed or flagged |
| Flagged | Player has right-clicked to mark it |
| Revealed — empty | Revealed with 0 adjacent mines |
| Revealed — number | Revealed with 1-8 adjacent mines |
| Mine — hit | The mine the player clicked (loss trigger) |
| Mine — revealed | All other un-flagged mines shown on loss |
| Wrong flag | A flagged cell that was not a mine, shown on loss |

---

## Left Click — Reveal

Clicking a covered, unflagged cell reveals it.

- **Mine:** triggers loss. All mines reveal. Wrong flags show an X.
- **Empty (0 adjacent mines):** triggers flood fill — all connected empty cells reveal automatically, along with the numbered cells bordering them.
- **Number (1-8):** just that cell reveals.

Clicking an already-revealed or flagged cell does nothing.

---

## Right Click — Flag

Right-clicking a covered cell toggles a flag on it.

- Flagged cells cannot be accidentally revealed by left click.
- Right-clicking a flagged cell removes the flag (returns to covered).
- The classic game does not use a "?" intermediate state (we are not implementing that either).

---

## Mine Counter

Displays: total mines minus flags placed.

- Starts at 10 (total mines).
- Decrements by 1 each time a flag is placed.
- Increments by 1 each time a flag is removed.
- **Can go negative** — if the player places more flags than there are mines, the counter goes below zero. Classic behavior allows down to -99. This is intentional and expected.
- Display range: -99 to 999, zero-padded to 3 digits (e.g. `010`, `-03`).

---

## Timer

- Starts counting on the player's **first left click**.
- Stops on win or loss.
- Displays elapsed seconds, zero-padded to 3 digits.
- Caps at 999.

---

## Win Condition

All non-mine cells are revealed. The player does **not** need to flag every mine to win — revealing all safe cells is sufficient.

On win: timer stops, reset button shows the win face. Classic Minesweeper auto-flags any remaining unflagged mines (optional behavior — our v1 does not implement this).

---

## Loss Condition

The player reveals a mine.

On loss:
- The clicked mine shows a red background (mine-hit state).
- All other un-flagged mines reveal.
- Any cell the player flagged that was not a mine shows an X over the flag (wrong-flag state).
- Correctly flagged mines remain as flags (they are not un-flagged on loss).
- Timer stops. Reset button shows the loss face.

---

## Reset Button

Starts a new game at any point during play. Mine positions reset, timer resets, all cells return to covered.

Face states:

| State | Trigger |
|-------|---------|
| Smiley | Ready or playing |
| Surprised (O-mouth) | Player is holding left click down |
| Sunglasses | Won |
| Dead (X eyes) | Lost |

---

## What We Are Not Implementing

- **Chording:** In the original game, left+right clicking a revealed numbered cell auto-reveals all adjacent unflagged cells if the correct number of adjacent flags are already placed. Widely used by experienced players. Not implemented.
- **Auto-flag on win:** Remaining unflagged mines auto-flag when the player wins. Not implemented.
- **Best times:** Classic leaderboard per difficulty. Not implemented.

## Deviations from Classic Behavior

- **Question mark state:** Implemented. Right-click cycles covered → flagged → ? → covered. Question-marked cells can still be revealed by left-click (unlike flags).
- **Multiple difficulty modes:** Implemented. Beginner (9×9, 10), Intermediate (16×16, 40), Expert (16×30, 99).
