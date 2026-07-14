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
      Core/       Engine-agnostic C# game logic
      Gameplay/   MonoBehaviours: views, input, controller — builds the whole
                  screen procedurally at runtime, no scene file needed
    Scenes/       (empty — see "Playing it" below for the one-GameObject setup)
    Prefabs/      (empty)
    Art/          (empty — flat colored squares only, no sprites yet)
  Tests/
    EditMode/     NUnit tests for Scripts/Core
```

## Architecture

`Scripts/Core` has **no UnityEngine dependency** (`noEngineReferences: true`
in its asmdef) — it's plain C#, so it's fast to test and portable if the
engine ever changes. `Scripts/Gameplay` is the MonoBehaviour layer: it renders
`Core` state as flat colored UGUI panels (no art assets — a solid white 4×4
texture tinted per-cell) and forwards clicks into `GameSession`. It's built
entirely from code in `GameController.Awake()` — Canvas, HUD, grid, tray,
power-up bar, game-over panel — rather than a hand-authored `.unity` scene
file, which is risky to author correctly by hand outside the Editor.

| Gameplay file | Responsibility |
|---|---|
| `UiFactory` | code-only helpers: solid-color sprite, panels, text, buttons, canvas, EventSystem |
| `BlockColorPalette` | `BlockColor` enum → actual RGB (the prototype's `COLORS` array) |
| `CellView` / `GridView` | one board cell; the 8×8 (or `Board.SizeValue`) grid of them |
| `PieceTraySlotView` / `PieceTrayView` | one tray slot rendering a piece's shape; the 3-slot tray |
| `HudView` | score/best boxes + charge meter bar |
| `PowerUpBar` | bomb/line/color-wipe buttons, shown only when the meter is full |
| `GameOverPanel` | full-screen overlay + restart |
| `GameController` | builds the screen, owns the `GameSession`, wires clicks to it |

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

## Playing it

There's no `.unity` scene file checked in yet, but the setup is one step:

1. **File → New Scene** (or use the default empty scene Unity opens with).
2. In the Hierarchy, **right-click → Create Empty**, name it anything
   (e.g. `Game`).
3. Add the **Game Controller** component to it (`Assets/_Project/Scripts/Gameplay/GameController.cs`).
4. Press **Play**. `GameController.Awake()` builds the Canvas, HUD, 8×8
   grid, piece tray, power-up bar, and game-over overlay at runtime and
   starts a `GameSession`.

Controls mirror the prototype: tap a tray piece to select it (highlights
yellow), tap a board cell to place it. When the charge meter fills, three
power-up buttons appear — tap one, then tap a board cell to apply it.

> This view/input layer was written and reasoned through carefully but not
> visually verified — there's no Unity Editor in the environment it was
> written in, so treat the first Play session as a check for layout/sizing
> issues (e.g. the grid or tray not filling the screen as expected on your
> aspect ratio) rather than an already-polished screen. The interaction
> logic (placement, clears, merges, power-ups, game over) is the same code
> the EditMode tests already exercise, so that part should be solid; it's
> purely the handwritten UGUI layout code that's unverified.

## What's next

1. Save a `Scenes/Main.unity` scene once the one-GameObject setup above is
   confirmed working, so it's not manual every time.
2. Juice: clear/merge VFX (the prototype's `.clearing` / `.merging` CSS
   animations — a scale+fade tween and a scale+brighten tween are the
   direct equivalents), meter fill animation, drag-to-place instead of
   tap-to-select if that feels better once playable.
3. Monetization (rewarded video, interstitial, IAP) and an actual art pass,
   once the mechanic itself feels right in Unity.
