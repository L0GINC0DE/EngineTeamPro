DLJ - head particles baked into the ORIGINAL player Aseprites

The existing seven files in Assets/02.Sprites/DLJ/Player were edited directly.
Their DLJ_Head_Particles layer contains the pixelfx particles in every frame.
The four original body layers and all their chunks are preserved byte for byte.
Canvas: unchanged 64 x 77. Animation: unchanged 8 frames x 160 ms (1.28 s).

Attack: red; Defend: blue; Act: orange; Heal: green;
HealthBar: yellow; Setting: silver; TextBar: white.

The former overlay PNG resources, extra SpriteRenderer component and
DLJ_HeadUIParticles script have been removed from the project.
PlayerTest now plays the original body frames through PlayerAppearance,
using the same single SpriteRenderer as the body. No particle runtime exists.
The weapon idle playback is preserved.

Rebuild (idempotent; replaces only the named particle layer):
node output/DLJ/HeadUIParticles/build.mjs <path-to-pixelfx>

Validate with Unity:
./Tests/DLJHeadParticles/Run-Validation.ps1

preview.png is a review-only contact sheet of all eight original frames.
It is outside Assets and is never loaded by the game.
imported-preview.png compares actual Unity textures: originals above, particles below.
If the particle extents change, refresh imported frame geometry with:
./Tests/DLJHeadParticles/Run-Validation.ps1 -Mode RepairMetadata
The resulting metadata-repair files preserve the original sprite IDs.
LICENSE_pixelfx.txt contains the MIT license for pixelfx.
