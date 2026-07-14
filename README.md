# Block Merge

A grid-based block-placement puzzle (Unity, iOS + Android). Drag pieces onto
an 8×8 board to fill rows/columns like Block Blast — but when a full line
clears, any same-colored blocks that were touching *within that line* fuse
into charge instead of popping, filling a meter that unlocks a power-up
(bomb / line-clear / color-wipe).

The mechanic was validated in an HTML/JS prototype first; this project is a
straight port of that prototype's game logic into a Unity project, starting
with the data model before any UI/art/monetization work.

## Project layout

```
Assets/
  _Project/
    Scripts/
      Core/       Engine-agnostic C# game logic (this is what's implemented so far)
      Gameplay/   MonoBehaviours: views, input, controller (not started yet)
    Scenes/       (empty — no scene wired up yet)
    Prefabs/      (empty)
    Art/          (empty — flat colored squares planned, no sprites yet)
  Tests/
    EditMode/     NUnit tests for Scripts/Core
```

## Architecture

`Scripts/Core` has **no UnityEngine dependency** (`noEngineReferences: true`
in its asmdef) — it's plain C#, so it's fast to test and portable if the
engine ever changes. `Scripts/Gameplay` (not started) will be the
MonoBehaviour layer that renders `Core` state and forwards input into it.

| File | Responsibility | Prototype equivalent |
|---|---|---|
| `GridCoord` | row/col struct (no `Vector2Int`, to keep Core Unity-free) | — |
| `BlockColor` | 5-color enum | `COLORS` |
| `PieceShapes` | the 12 piece shapes | `SHAPES` |
| `Piece` / `PieceFactory` | a shape + color; random generator | `randomPiece()` |
| `Board` | 2D grid, bounds/occupancy checks, placement | `board`, `canPlace()` |
| `ColorGroupFinder` | flood-fill same-color groups within a cell set | `findColorGroupsWithin()` |
| `LineClearResolver` | full-line detection, line bonus, merge-vs-pop decision | `resolveLines()` |
| `ChargeMeter` | clamped 0–100 meter | `meter` |
| `PowerUpType` / `PowerUpResolver` | bomb / line / color-wipe effects | `usePower()` |
| `GameSession` | orchestrates the above: score, tray, refill, game-over | the rest of the `<script>` |

Constants (board size 8, merge threshold 3, meter max 100, line bonus 10/line,
merge bonus 4/cell) are carried over unchanged from the prototype.

## Running the tests

Open the project in Unity 2022.3 LTS (or update `ProjectSettings/ProjectVersion.txt`
to whatever LTS you have installed — the project has no version-specific
assets, so this is safe), then **Window → General → Test Runner → EditMode → Run All**.

Tests cover: placement/bounds checks, color-group flood fill, full-line
detection, merge-threshold behavior, line/merge scoring, charge meter
clamping, all three power-ups, and full `GameSession` playthroughs (score,
tray refill, game-over detection, restart).

> Note: these tests were written and hand-traced against the ported logic
> but not run through Unity's Test Runner in this environment (no Unity
> Editor available here) — run them locally before relying on them.

## What's next

1. `Scripts/Gameplay`: a `GridView` that renders `Board` state as flat
   colored squares (no sprites), a piece tray view, and drag/tap input
   wired to `GameSession`.
2. A `Scenes/Main.unity` scene wiring it together.
3. Once the mechanic is playable end-to-end in Unity: juice (clear/merge
   VFX, meter fill animation), then monetization (rewarded video,
   interstitial, IAP) and art pass.
