---
title: Minesweeper — Art Brief
date: 2026-04-30
type: design
status: draft
---

# Minesweeper — Art Brief

Complete asset list for the sprite pass. Every file, with dimensions and a visual description. No implementation details.

---

## Style

Classic Windows 95 Minesweeper pixel art.

- **Base palette:** Mid-gray (#C0C0C0) cell body, white (#FFFFFF) highlight edges, dark gray (#808080) shadow edges, black (#000000) outlines
- **Raised cells:** 3D button illusion — white/light 1 px edge on top and left, dark 1 px edge on bottom and right
- **Flat cells:** Thin dark border, no bevel, slightly darker gray fill than covered cells
- **All sprites:** PNG, no anti-aliasing
- **Icon overlays:** Transparent backgrounds

---

## Cell Backgrounds

Three background tiles only — no icons drawn on these. All icon content layers on top separately.

| File | Dimensions | Description |
|------|------------|-------------|
| `cell_covered.png` | 16×16 | Raised bevel tile — mid-gray body, white/light top and left edges, dark bottom and right edges, 1 px |
| `cell_revealed.png` | 16×16 | Flat tile — slightly darker gray fill, thin dark border, no bevel |
| `cell_mine_hit.png` | 16×16 | Same shape as revealed but red (#FF0000) fill — mine icon layers on top |

---

## Icon Overlays

Layered on top of cell backgrounds. Transparent backgrounds. Centered on the 16×16 cell.

| File | Dimensions | Description |
|------|------------|-------------|
| `icon_mine.png` | 12×12 | Black circle body with 8 spikes radiating outward, small white highlight spot top-left |
| `icon_flag.png` | 12×12 | Red triangular flag on a black pole, small black base at bottom |
| `icon_x.png` | 10×10 | Red X, 2 px diagonal strokes — overlaid on wrong-flag cells alongside `icon_flag.png` |
| `icon_number_1.png` | 8×12 | Digit "1" in blue (#0000FF) |
| `icon_number_2.png` | 8×12 | Digit "2" in green (#007B00) |
| `icon_number_3.png` | 8×12 | Digit "3" in red (#FF0000) |
| `icon_number_4.png` | 8×12 | Digit "4" in dark blue (#00007B) |
| `icon_number_5.png` | 8×12 | Digit "5" in dark red (#7B0000) |
| `icon_number_6.png` | 8×12 | Digit "6" in teal (#007B7B) |
| `icon_number_7.png` | 8×12 | Digit "7" in black (#000000) |
| `icon_number_8.png` | 8×12 | Digit "8" in gray (#7B7B7B) |

---

## Header

### Reset Button Faces

The button renders as a raised square. The face sprite fills the interior. Four states, one sprite each.

| File | Dimensions | Description |
|------|------------|-------------|
| `face_ready.png` | 24×24 | Simple smiley — circle outline, dot eyes, curved smile, transparent background |
| `face_click.png` | 24×24 | Surprised face — same as ready but with open oval mouth |
| `face_win.png` | 24×24 | Cool face — smiley with pixel-art sunglasses |
| `face_lose.png` | 24×24 | Dead face — X eyes, flat or downturned mouth |

### Display Digits

Used for the mine counter and timer. Red 7-segment LCD style.

| File | Dimensions | Description |
|------|------------|-------------|
| `digit_0.png` – `digit_9.png` | 13×23 | Red (#FF0000) 7-segment digit on black (#000000) background |
| `digit_minus.png` | 13×23 | Center segment only (minus sign) — red on black. Used when the mine counter goes negative (player has placed more flags than there are mines — standard classic behavior). |

### Header Panel

| File | Type | Description |
|------|------|-------------|
| `header_panel.png` | fixed dimensions | Raised outer border matching the cell bevel style. Interior has three sunken insets left-to-right: mine counter, reset button, timer. Draw at the exact header width — no slicing needed. |

---

## Asset Summary (v1)

| Category | Count |
|----------|-------|
| Cell backgrounds | 3 |
| Icon overlays | 11 (mine, flag, X, numbers 1-8) |
| Button faces | 4 |
| Display digits | 11 (0-9, minus) |
| Header panel | 1 |
| **Total** | **30** |

---

## v2 — Animation Sheets

Each animation is a single-row sprite sheet: all frames in one PNG, left-to-right. Frame size matches the cell grid (16×16). Transparent backgrounds.

### Candidate Events

The following events are candidates for animation. Start with the highest-impact ones — mine detonation and win state — and expand from there.

| File | Frames | Loop | Trigger |
|------|--------|------|---------|
| `anim_mine_detonate.png` | 6-8 | No | Player clicks a mine |
| `anim_flag_place.png` | 4-6 | No | Right-click places a flag |
| `anim_flag_remove.png` | 4-6 | No | Right-click removes a flag |
| `anim_cell_reveal.png` | 4-6 | No | Cell reveals (or flood-fill cascade) |
| `anim_win.png` | 6-8 | Yes | Looping on all cells during win state |

### Notes

- Keep frame counts on the low end first — add more if the motion looks choppy
- All frames in a sheet must be the same dimensions
- One-shot animations play once and hold on the final frame
- All sheets are horizontal strips (single row), not grids
