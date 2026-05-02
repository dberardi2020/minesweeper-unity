---
title: Cell Gap and Pixel Snapping — Plain English
date: 2026-05-02
type: note
---

# Cell Gap and Pixel Snapping — Plain English

## Why the gap looks better

When cells are scaled to 0.95, each one is slightly smaller than the 1×1 slot it sits in. That 0.05-unit sliver of space around every cell gives the grid a **tiled** look — you can see where one cell ends and the next begins, which reads more naturally as a clickable grid.

At scale 1.0, the cells butt up edge-to-edge. It looks fine, but it reads more like a flat image than a set of individual buttons.

---

## What PPU means

**PPU = Pixels Per Unit.** It's the ratio between screen pixels and Unity's world units.

Example: a sprite imported at **16 PPU** renders as 1 world unit tall if it's 16px tall. A cell that's 1×1 world units = 16×16 pixels on screen — if the camera is set correctly.

All sprites in this project are imported at 16 PPU so they all agree on the same scale.

---

## Why the gap needs a special camera setting

The gap is **0.05 world units** wide (= 1 − 0.95).

For that gap to look sharp — one clean row of pixels between cells — 0.05 world units must equal exactly 1 pixel on screen.

That means:

```
0.05 × PPU = 1 pixel
PPU = 1 / 0.05 = 20
```

The PPU must be a **multiple of 20** (20, 40, 60, 80…) for the gap to be a whole number of pixels. Any other PPU value gives a fractional gap — some gaps round to 1px, others to 0px, and the grid looks uneven.

`FitCamera()` in GameManager computes the camera's orthographic size, then nudges it to land on the nearest multiple-of-20 PPU. The adjustment is tiny — usually a fraction of a world unit — so nothing gets clipped.

---

## Why this doesn't affect the header

The gap only exists on **cell prefabs** (they're scaled to 0.95). The header tiles are not scaled — they sit at full 1×1 world units, edge-to-edge, with no gap. The camera snapping is still correct for the header because the header tiles are 16px at 16 PPU, and any multiple of 20 is also a multiple of 16. Both round cleanly.

The visual distinction between gapped cells and flush header tiles is intentional — the header chrome is a continuous surface; the grid is a field of individual, clickable squares.

---

## Summary table

| Setting | Value | Why |
|---|---|---|
| Cell scale | 0.95 | Creates 0.05-unit gap between cells |
| All sprite PPU | 16 | Consistent sizing across all art |
| Camera PPU snapped to | Multiple of 20 | Makes the gap exactly 1px wide |
| Header tile scale | 1.0 | Flush/continuous — no gap |
