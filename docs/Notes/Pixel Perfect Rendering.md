---
title: Pixel Perfect Rendering
date: 2026-05-01
type: note
---

# Pixel Perfect Rendering

## The Problem

On high-DPI displays (Mac Retina, Mac Neo, etc.) the grid tiles appear unevenly spaced — some rows or columns look 1 pixel thicker than others. The cause is **sub-pixel rendering**: the 0.05 world-unit gap between tiles maps to a fractional number of physical pixels, so some gaps round to 1px and others to 0px.

This doesn't appear on all machines — it depends on whether the screen resolution and camera orthographic size happen to produce a clean integer pixels-per-world-unit ratio.

---

## Current Fix (interim)

`GameManager.SnapCameraToPixelGrid()` runs in `Awake` and nudges the camera's orthographic size so that world units land on exact pixel boundaries:

```csharp
void SnapCameraToPixelGrid()
{
    var cam = Camera.main;
    float ppu = Mathf.Round(Screen.height / (cam.orthographicSize * 2f));
    if (ppu > 0) cam.orthographicSize = Screen.height / (ppu * 2f);
}
```

It reads the actual screen height at runtime, computes the nearest integer pixels-per-unit (PPU), then adjusts ortho size by a tiny fraction to match. The adjustment is < 0.5 world units, so nothing gets clipped.

**Why this is "interim":** it snaps to the *nearest* integer PPU, which may not be a clean multiple of a sprite's native pixel size. Fine for colored quads; bad for pixel art (see below).

---

## The Right Long-Term Fix: Pixel Perfect Camera

Unity's `Pixel Perfect Camera` component (built into URP 17+, no separate package needed in Unity 6) is the correct solution once real sprite assets exist.

### How it works

1. You tell it your sprite's **Assets Per Unit** (PPU in your sprite import settings)
2. You tell it the **Reference Resolution** — the game's "native" pixel canvas
3. At runtime it picks the largest integer zoom factor (1×, 2×, 3×…) that fits the screen, then sets ortho size accordingly

This guarantees tiles always render at an exact integer multiple of their native pixel size — no blurring, no sub-pixel gaps, on any screen.

### How to configure it

1. Select **Main Camera** → Add Component → **Pixel Perfect Camera**
2. Set **Assets Per Unit** to match the sprite import PPU (check any sprite in the Inspector under "Pixels Per Unit")
3. Set **Reference Resolution** using this formula:

```
ref_width  = horizontal_world_units × PPU
ref_height = vertical_world_units × PPU
```

For this game (9 units wide, ~11.5 units tall at ortho size 5.75):

| Sprite PPU | Reference Resolution |
|---|---|
| 16 | 144 × 184 |
| 32 | 288 × 368 |
| 64 | 576 × 736 |

4. Set **Crop Frame** to `None`
5. Remove or disable `SnapCameraToPixelGrid()` in `GameManager` — the two approaches conflict

### Mixed assets

If you mix pixel art sprites and plain colored quads, import everything at the **same PPU**. The Pixel Perfect Camera applies one zoom factor to the whole scene; assets at different PPUs will be rendered at inconsistent sizes.

---

## Decision Point

| Situation | Use |
|---|---|
| Colored quad sprites only | `SnapCameraToPixelGrid()` |
| Pixel art sprites (uniform PPU) | Pixel Perfect Camera |
| Mixing art styles | Standardize on one PPU, use Pixel Perfect Camera |

When pixel art assets are ready, remove `SnapCameraToPixelGrid()`, add Pixel Perfect Camera, and set Reference Resolution per the table above.
