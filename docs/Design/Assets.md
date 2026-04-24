---
title: Minesweeper — Assets
date: 2026-04-24
type: design
status: planning
---

# Minesweeper — Assets

Every asset required to build the game. For each: what it is, its spec, and how it will be created.

---

## Creation Strategy

**v1 uses Unity primitives + TextMeshPro. No external art files.**

Cells are colored quads. Numbers are TextMeshPro text. Icons (flag, mine, face) are TextMeshPro Unicode characters styled to fit. This keeps the focus on Unity mechanics rather than art pipeline, and allows the full game to be built and tested before any sprites exist.

Sprites can be swapped in later without changing any game logic, since visual representation is isolated in the display layer (see Wiring).

---

## Cell Tiles

The grid is 9x9. Each cell has one visual state at a time.

| State | Trigger | Visual (v1) |
|-------|---------|-------------|
| Covered | Default | Mid-gray quad |
| Flagged | Right-clicked by player | Mid-gray quad + flag character (🚩) |
| Revealed — empty | Revealed, 0 adjacent mines | Dark-gray quad, no content |
| Revealed — number | Revealed, 1–8 adjacent mines | Dark-gray quad + colored number (TextMeshPro) |
| Mine — hit | Player clicked this mine | Red quad + mine character (💣) |
| Mine — revealed | All other mines shown on loss | Dark-gray quad + mine character (💣) |
| Mine — wrong flag | Flagged cell that wasn't a mine, shown on loss | Dark-gray quad + flag character with X overlay (🚩✕) |

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

No fixed pixel sizes in v1 — the scene is sized in Unity units and scaled to fill the camera view.

| Element | Size (Unity units) |
|---------|--------------------|
| Cell | 1 × 1 |
| Grid | 9 × 9 |
| Header height | ~2 units |
| Total scene | ~9 × 11 |

Camera is set to orthographic with size calculated to frame the full scene with minimal padding.

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

## Future Sprite Pass (Post-v1)

When replacing v1 primitives with real sprites, each cell state becomes a separate sprite. Recommended spec if/when that happens:

- **Format:** PNG, power-of-two dimensions
- **Cell sprite size:** 16×16 px
- **Style:** Classic Windows 95 Minesweeper pixel art — raised tile with highlight/shadow bevel for covered state, flat tile for revealed
- **Source options:** Draw in Aseprite, or source a CC0 sprite sheet from itch.io

This is a drop-in swap — game logic is unaffected.
