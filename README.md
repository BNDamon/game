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
`Core` state as UGUI panels — no art assets, everything (the rounded-rect
shape, "particle" bursts, ambient embers) is generated from code — and
forwards input into `GameSession`. It's built entirely from code in
`GameController.Awake()` — Canvas, HUD, grid, tray, power-up bar, game-over
panel — rather than a hand-authored `.unity` scene file, which is risky to
author correctly by hand outside the Editor.

| Gameplay file | Responsibility |
|---|---|
| `UiFactory` | code-only helpers: solid + rounded-rect sprites (a generated `Texture2D`, 9-sliced, with a subtle brightness gradient baked into the RGB channels), panels, text, buttons, canvas, EventSystem |
| `BlockColorPalette` | `BlockColor` enum → actual RGB (the prototype's `COLORS` array) |
| `CellView` / `GridView` | one board cell (rounded, animated, idle breathing glow while filled); the 8×8 (or `Board.SizeValue`) grid of them |
| `PieceTraySlotView` / `PieceTrayView` | one tray slot (drag source, dims in place once used, shakes red on an invalid drop); the 3-slot tray |
| `HudView` | compact score/best chips (animated count-up) + the charge meter as a "power core" — glow that breathes with charge level, a flash pulse on reaching full |
| `PowerUpBar` | bomb/line/color-wipe buttons (charged gold styling, gentle breathing pulse while available) |
| `GameOverPanel` | full-screen overlay that fades/scales in + restart |
| `BurstEffect` | small burst of fading/expanding squares where a merge happens (fake "particles" — a real ParticleSystem would render behind our Screen Space - Overlay canvas) |
| `ScorePopupEffect` / `ComboPopupEffect` | floating "+N" and "COMBO x3" callouts where a scoring clear lands |
| `ScreenFlashEffect` | brief full-screen color flash, intensity scaling with combo heat |
| `AmbientEmbers` | continuously drifting background embers so the board never reads as a static screenshot |
| `SfxPlayer` | procedurally synthesized placeholder SFX (place/clear/merge/power-up/game-over), pitch-shifts up with combo heat |
| `BestScoreStore` | persists best score across launches via `PlayerPrefs` |
| `GameController` | builds the screen, owns the `GameSession`, drives drag input + all of the above |

| Core file | Responsibility | Prototype equivalent |
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
| `ComboTracker` | chains clears within a time window into an escalating score multiplier | — (new, not in the prototype) |
| `GameSession` | orchestrates the above: score, tray, refill, game-over, combo | the rest of the `<script>` |

Constants (board size 8, merge threshold 3, meter max 100, line bonus 10/line,
merge bonus 4/cell, combo window 4s, combo multiplier +0.25x per chain up to
3x) are carried over unchanged from the prototype except the combo system,
which is new — see below.

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

### Why a combo system

Feedback on the previous pass was that the game felt generic — like any
other block-placement clone — and slow/stagnant. Visual polish alone
doesn't fix that; it needed something that changes how the game actually
*plays*. So this pass adds a **combo/chain system**: clearing lines quickly,
back-to-back, builds a streak that multiplies your score (up to 3x),
decaying if you slow down. It's the thing that turns "place pieces
carefully" into "how fast can you chain" — a genuinely different feel from
the deliberate, unhurried pace of most games in this genre, and it's a
direct extension of the merge-to-charge hook rather than a bolted-on timer.
Combined with escalating feedback (a bigger callout, a screen flash, the
SFX pitching up) as the chain grows, this is the game's actual answer to
"make it feel fast and unique," not just a visual reskin.

This pass also makes the board feel alive even when you're not touching
it — the biggest single note from the last screenshot ("stagnant board").

- **Combo/chain scoring** (`ComboTracker`, Core): clear lines within a 4s
  window of each other and each one raises a multiplier (+0.25x per chain,
  capped at 3x). Waiting too long resets it back to 1x. `GameSession.Tick()`
  advances the decay clock — `GameController` calls it every frame.
- **Escalating combo feedback**: a big "COMBO x3" callout on top of the
  regular "+N" score popup, a screen-wide color flash that gets stronger
  the longer the chain, and the clear/merge SFX pitching up with it.
- **Ambient board life**: small embers drift upward across the whole
  screen continuously, and filled cells have a slow, subtle breathing
  glow — the board should never look like a static screenshot again, even
  before you place anything.
- **Snappier pace**: clear/merge/score/meter animation durations trimmed
  ~25–35% across the board.
- **Fairness**: a tray refill can never leave you with zero legal moves —
  if every freshly-dealt piece would be unplaceable, one slot is swapped
  for a single cell (which always fits, since a fully-packed board would
  have already cleared itself).
- **Unmissable feedback**: an invalid drop now flashes red and shakes back
  into its tray slot instead of silently snapping back, and the game-over
  screen fades/scales in instead of just appearing.
- **Visual fixes from the last round**: the glossy top-highlight overlay
  read as a dated skeuomorphic bevel and was replaced with a subtle
  brightness gradient baked directly into the block texture; a "used" tray
  slot's color was nearly transparent against the dark background (2 of 3
  tray slots were effectively invisible) and is opaque now; the HUD's
  score/best boxes were oversized and are now compact chips.

> All of this is new and hasn't been visually verified — there's no Unity
> Editor in the environment it was written in. The underlying scoring
> logic (combo windows, multiplier math, decay) is Core code exercised by
> EditMode tests, so that part should be solid; the things most worth
> judging on your own are whether the combo window (4 seconds) and
> multiplier curve actually *feel* fast and rewarding rather than
> arbitrary, and whether the ambient embers read as "alive" or as visual
> noise — both are just numbers I picked and easy to retune once you've
> played with them.

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
