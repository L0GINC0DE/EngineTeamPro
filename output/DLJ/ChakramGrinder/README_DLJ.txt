ChakramGrinder_DLJ

The supplied silver Chakram.aseprite, spinning like a grinding circular saw.
Original weapon pixels are nearest-neighbor sampled at 2x scale and rotated.
The open center and diagonal grip remain visible. No hue is added to the silver weapon.
Short silver cutting arcs follow the teeth; white metal sparks are flung tangentially,
with a stronger grinding spray on the right-hand edge.
Broad circular glows are removed; contact flashes and sparks remain sharp pixel lines.

Canvas: 112 x 112 pixels. Center pivot: (56,56), normalized (0.5,0.5).
36 frames at approximately 30 fps, 33/34/33 ms repeating. Duration: 1200 ms.
4 clockwise rotations per loop (approximately 200 RPM).
Continuous loop, suitable for a sustained grinding attack or spinning projectile.

Editable layers, back to front:
DLJ_Spin_Trails / DLJ_Original_Chakram_Rotating / DLJ_Grinding_Sparks

ChakramGrinder_DLJ.aseprite: editable layered source.
ChakramGrinder_DLJ_sheet.png: transparent sheet, 6 columns x 6 rows, 112x112 per frame.
ChakramGrinder_DLJ_FX_only.png: matching sheet with arcs and sparks only.
ChakramGrinder_DLJ.json: Aseprite-style frame rectangles and exact timing.
ChakramGrinder_DLJ_preview.gif: 4x nearest-neighbor preview with dark background.

Use Point filtering, no texture compression, and centered sprite pivots.
The weapon art is doubled in pixel size; using twice the original PPU keeps its game-world size.
The existing Unity scene, weapon selection code, and original Chakram.aseprite are unchanged.
