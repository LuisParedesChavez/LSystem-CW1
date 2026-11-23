# L-System Renderer (Unity, C#)

IS71021E Mathematics for Games and V/AR — Coursework 1, Goldsmiths, University of London.
Luis Antonio Paredes Chavez, 2026.

Recursive L-system generator with a 2D turtle and procedural leaves. Includes the six trees from
*The Algorithmic Beauty of Plants* (Fig. 1.24) and two Koch patterns.

**Video:** https://youtu.be/VIDEO-LINK

## Run
Open with Unity **6000.3.27f1**, open `Assets/Scenes/SampleScene`, press Play.

## Controls
| Key | Action |
|---|---|
| 1–8 | Load preset |
| R | Reload preset |
| Up / Down | Iterations |
| Left / Right | Both angles (Shift: ×5) |
| A / D, J / L | Left / right angle only |
| W / S | Segment length |
| V | Leaves on / off |
| Q / E | Leaf size |
| Z / X | Leaf density |
| G / T | Crown height |
| N | New leaf seed |
| C | Camera auto-fit / lock |

## Presets
Editable text files in `Assets/StreamingAssets/Presets/` (`key = value`, one `rule = X -> ...` per line).