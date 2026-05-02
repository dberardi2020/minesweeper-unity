# Asset Pipeline Notes

Practical notes on processing and transforming art assets outside of Unity.

---

## Python (Pillow) for Image Manipulation

Python with the [Pillow](https://pillow.readthedocs.io) library is available in the dev environment and is useful for batch image operations that would otherwise require manual work in Photoshop or Aseprite.

Install once: `pip3 install Pillow`

### Compositing layers and slicing a sprite sheet

**Problem:** Artist exports per-layer PNGs at full canvas size (e.g. 11 digit layers, each 110×16px) instead of cropped individual sprites.

**Solution:** Composite all layers into one sheet, then slice at fixed intervals.

```python
from PIL import Image
import os

src = "path/to/layer/files"
dst = "path/to/output"
os.makedirs(dst, exist_ok=True)

# Composite all layer PNGs into one sheet
sheet = Image.new("RGBA", (110, 16), (0, 0, 0, 0))
for fname in sorted(os.listdir(src)):
    if not fname.endswith(".png"): continue
    layer = Image.open(os.path.join(src, fname)).convert("RGBA")
    sheet = Image.alpha_composite(sheet, layer)

sheet.save(os.path.join(dst, "sheet.png"))

# Slice into individual sprites (10px wide each)
for i in range(11):
    sprite = sheet.crop((i * 10, 0, i * 10 + 10, 16))
    sprite.save(os.path.join(dst, f"sprite_{i}.png"))
```

### Auto-trim transparent padding

```python
img = Image.open("sprite.png").convert("RGBA")
bbox = img.getbbox()  # bounding box of non-transparent pixels
if bbox:
    cropped = img.crop(bbox)
    cropped.save("sprite_trimmed.png")
```

---

## Claude Code context

Claude Code can run Python scripts inline via Bash, making it straightforward to ask it to process a batch of files, rename them, reformat them, or generate derived assets on the fly — without leaving the terminal.
