# Block Merge

A grid-based block-placement puzzle (Unity, iOS + Android). Drag pieces onto
an 8×8 board to fill rows/columns like Block Blast — but when a full line
clears, any same-colored blocks that were touching *within that line* fuse
into charge instead of popping, filling a meter that unlocks a power-up
(bomb / line-clear / color-wipe).

Visual identity leans into that mechanic: an "energy/charge" theme where
blocks are rounded and glossy, merges burst into particles, the charge meter
reads as a glowing power core, and the whole screen's background subtly
warms up as charge builds.

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
`Core` state as UGUI panels — no art assets, everything (rounded-rect shapes,
the glossy top-highlight, "particle" bursts) is generated from code — and
forwards input into `GameSession`. It's built entirely from code in
`GameController.Awake()` — Canvas, HUD, grid, tray, power-up bar, game-over
panel — rather than a hand-authored `.unity` scene file, which is risky to
author correctly by hand outside the Editor.

| Gameplay file | Responsibility |
|---|---|
| `UiFactory` | code-only helpers: solid + rounded-rect sprites (generated `Texture2D`s, 9-sliced), glossy highlight overlay, panels, text, buttons, canvas, EventSystem |
| `BlockColorPalette` | `BlockColor` enum → actual RGB (the prototype's `COLORS` array) |
| `CellView` / `GridView` | one board cell (rounded, glossy, animated); the 8×8 (or `Board.SizeValue`) grid of them |
| `PieceTraySlotView` / `PieceTrayView` | one tray slot (drag source, dims in place once used, shakes red on an invalid drop); the 3-slot tray |
| `HudView` | score/best boxes (animated count-up) + the charge meter as a "power core" — glow that breathes with charge level, a flash pulse on reaching full |
| `PowerUpBar` | bomb/line/color-wipe buttons (charged gold styling, gentle breathing pulse while available) |
| `GameOverPanel` | full-screen overlay that fades/scales in + restart |
| `BurstEffect` | small burst of fading/expanding squares where a merge happens (fake "particles" — a real ParticleSystem would render behind our Screen Space - Overlay canvas) |
| `SfxPlayer` | procedurally synthesized placeholder SFX (place/clear/merge/power-up/game-over) |
| `BestScoreStore` | persists best score across launches via `PlayerPrefs` |
| `GameController` | builds the screen, owns the `GameSession`, drives drag input + all of the above |

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
clamping, all three power-ups, solvable-tray-refill guarantee, and full
`GameSession` playthroughs (score, tray refill, game-over detection, restart).

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

Controls: **drag** a tray piece onto the board to place it — a floating
ghost follows your finger (lifted above it so it's not hidden), and the
board previews the drop as valid (light) or invalid (red) while you drag.
When the charge meter fills, three power-up buttons appear — tap one, then
**tap** a board cell to target it (power-ups are a single-cell pick, not a
placement, so that stays tap-based).

Since the last playtest round, this pass adds:
- **Fairness**: a tray refill can never leave you with zero legal moves —
  if every freshly-dealt piece would be unplaceable, one slot is swapped
  for a single cell (which always fits, since a fully-packed board would
  have already cleared itself).
- **Unmissable feedback**: an invalid drop now flashes red and shakes back
  into its tray slot instead of silently snapping back, and the game-over
  screen fades/scales in instead of just appearing — the actual complaint
  that prompted this ("it doesn't tell you, it just ends") should be fixed
  either way, whichever path was actually causing it.
- **Visual identity**: rounded, glossy blocks (a generated rounded-rect
  texture + soft top-highlight, not flat squares); the charge meter as a
  glowing "power core" that breathes brighter as it fills and flashes when
  full; a small burst of "particles" where blocks merge; the background
  subtly warming from its base dark-blue toward an ember tone as the meter
  builds, so the whole screen's mood tracks the mechanic.
- **Feel**: placed cells pop in, cleared cells pop-and-fade, merged cells
  glow brighter and longer before converting to charge, the meter bar eases
  toward its new value instead of snapping, and the score counts up instead
  of jumping.
- **Sound**: short synthesized tones on place/clear/merge/power-up/game-over
  — placeholders standing in for real SFX, so there's audio feedback before
  any sound design exists.
- **Persistence**: best score survives closing the app (`PlayerPrefs`).

> All of this is new since the last confirmed-working playtest and hasn't
> been visually verified — there's no Unity Editor in the environment it
> was written in. The underlying game logic it's built on (placement,
> clears, merges, power-ups, solvability) is the same Core code the
> EditMode tests exercise, so that part should be solid; treat the drag
> feel, animation timing, and the new rounded-corner rendering as the
> things to sanity-check first — a 9-sliced sprite with a fixed pixel
> corner radius is a well-worn Unity technique, but it's the one part of
> this pass most likely to look slightly off on a first look (e.g. corners
> a little too round or too sharp) and want a quick numeric tweak.

## What's next

1. Save a `Scenes/Main.unity` scene once the one-GameObject setup above is
   confirmed working, so it's not manual every time.
2. Real audio and an actual art pass (the flat-color-square constraint was
   deliberate for nailing the mechanic first — this is where that ends).
3. **Monetization (rewarded video, interstitial, IAP)** — this is the next
   big piece, and it needs your input specifically: which ad network(s)
   (Unity Ads, AdMob, ironSource, etc.), whether you already have
   developer accounts set up for iOS/Android, and what the IAP catalog
   should look like (remove-ads, currency, cosmetics). Flagging this now
   rather than guessing, since it's the one area that depends on accounts
   only you have access to.
