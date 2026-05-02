# Backlog

Future ideas and features to revisit — not currently scheduled for implementation. This is a living reference, not a spec.

---

## Emoji Reactions

**Description:** Progressive face happiness that scales with win percentage.

**Notes:** Neutral/nervous face at low win %, happy at mid, excited at high. Hover should trigger a funny/random reaction. Click reaction should be distinct from the current surprised face.

---

## Flower Picking Mechanic

**Description:** Clicking a revealed flower cell "picks" it and increments a session counter.

**Notes:** Half-baked idea — needs design work before implementation. Counter is non-persistent (session only, at least initially).

---

## Reskinning System

**Description:** ScriptableObject-based skin system to reskin the game without touching code.

**Notes:** Would enable a "deskinned" classic mode alongside themed skins. Keeps art and logic cleanly separated.

---

## Complex Menu

**Description:** Full difficulty/settings menu scene.

**Notes:** Distinct from the simple in-game preset selector already planned. A proper menu scene with more configuration options.

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

---

## Question Mark State

**Description:** A third cell state between covered and flagged (covered → flagged → ? → covered cycle).

**Notes:** Classic optional behavior. Useful for marking uncertain cells without committing a flag.
