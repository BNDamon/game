using System;
using System.Collections.Generic;

namespace BlockMerge.Core
{
    /// <summary>Result of a single GameSession.PlacePiece() call — everything a view layer
    /// needs to animate/react to (what got placed, what cleared/merged, meter/game-over/combo
    /// state).</summary>
    public sealed class PlacementOutcome
    {
        public static readonly PlacementOutcome Rejected =
            new PlacementOutcome(false, Array.Empty<GridCoord>(), LineClearResult.None, false, false, 0, 0);

        public bool Placed { get; }
        public IReadOnlyList<GridCoord> PlacedCells { get; }
        public LineClearResult LineClear { get; }
        public bool PowerUpAvailable { get; }
        public bool GameOver { get; }

        /// <summary>0 if this placement didn't clear a line. 1 for a clear that started a
        /// fresh streak, 2+ for a chained combo (see ComboTracker).</summary>
        public int ComboCount { get; }

        /// <summary>The actual score awarded for the clear this placement caused (line bonus
        /// + merge bonus, after the combo multiplier) — 0 if nothing cleared.</summary>
        public int ScoreAwarded { get; }

        public PlacementOutcome(
            bool placed,
            IReadOnlyList<GridCoord> placedCells,
            LineClearResult lineClear,
            bool powerUpAvailable,
            bool gameOver,
            int comboCount = 0,
            int scoreAwarded = 0)
        {
            Placed = placed;
            PlacedCells = placedCells;
            LineClear = lineClear;
            PowerUpAvailable = powerUpAvailable;
            GameOver = gameOver;
            ComboCount = comboCount;
            ScoreAwarded = scoreAwarded;
        }
    }
}
