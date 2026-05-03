# Backlog

Future ideas and features to revisit — not currently scheduled for implementation. This is a living reference, not a spec.

---

## GardenSweeper Rename

**Description:** Everything that still says "Minesweeper" needs updating.

**Notes:** productName and bundle identifier are already set. Remaining: repository name, project folder (`minesweeper-unity/`), script and class names, scene names, any in-game text. Low risk but tedious — save for a quiet session.

---

## testMode / PlaceTestMines() Fix

**Description:** `PlaceTestMines()` in GameManager has hardcoded row/col positions designed for a 9×9 grid. It quietly breaks on Intermediate or Expert.

**Notes:** Either clamp the positions to board bounds, regenerate a valid test layout per difficulty, or gate the test mode to Beginner only with a runtime warning.

---

## Weather Cascade Skip

**Description:** A large flood-fill reveal can uncover enough cells in one click to jump two or three weather states.

**Notes:** Design question: is the smooth Weather arc a meaningful part of the experience, or just a nice detail? If meaningful, consider advancing one state per click rather than snapping to progress. Deferred until the overall game feel is more established.

---

## TMP "Can't Generate Mesh" Warning on ResetLabel

**Description:** `ResetLabel` (the TMP fallback on the reset button) generates a harmless but noisy console warning.

**Notes:** Reproduces when weather sprites are wired (label is hidden but TMP still tries to generate a mesh). Suppress by disabling the TMP component entirely when `useWeatherSprites` is true, or just leave it — it's a warning, not an error.

---

## Emoji Reactions

**Description:** Progressive face happiness that scales with win percentage.

**Notes:** Superseded in part by the weather icon system. Revisit when deciding on the reset button's final look — whether it stays as a weather icon or gets a face layer on top.

---

## Flower Picking Mechanic

**Description:** Clicking a revealed flower cell "picks" it and increments a session counter.

**Notes:** Half-baked idea — needs design work before implementation. Counter is non-persistent (session only, at least initially).

---

## Reskinning System

**Description:** ScriptableObject-based skin system to reskin the game without touching code.

**Notes:** Would enable a "deskinned" classic mode alongside the garden theme. Keeps art and logic cleanly separated.

---

## Complex Menu

**Description:** Full difficulty/settings menu scene.

**Notes:** Distinct from the simple in-game preset selector already shipped. A proper menu scene with more configuration options.

---

## Persistence

**Description:** Save and restore player data across sessions.

**Notes:** Covers high scores, best times, and the flower-pick counter (if that mechanic ships).

---

## Sound

**Description:** Audio feedback for key game events.

**Notes:** Cell reveal, mine hit, flag placement, and win condition at minimum.

---

## Chording

**Description:** Auto-reveal neighbors when the correct number of flags are adjacent to a revealed cell.

**Notes:** Classic Minesweeper behavior, typically triggered by middle-click or double-click.
