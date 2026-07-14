using System;
using System.Collections.Generic;

namespace BlockMerge.Core
{
    /// <summary>Result of a single GameSession.PlacePiece() call — everything a view layer
    /// needs to animate/react to (what got placed, what cleared/merged, meter/game-over state).</summary>
    public sealed class PlacementOutcome
    {
        public static readonly PlacementOutcome Rejected =
            new PlacementOutcome(false, Array.Empty<GridCoord>(), LineClearResult.None, false, false);

        public bool Placed { get; }
        public IReadOnlyList<GridCoord> PlacedCells { get; }
        public LineClearResult LineClear { get; }
        public bool PowerUpAvailable { get; }
        public bool GameOver { get; }

        public PlacementOutcome(
            bool placed,
            IReadOnlyList<GridCoord> placedCells,
            LineClearResult lineClear,
            bool powerUpAvailable,
            bool gameOver)
        {
            Placed = placed;
            PlacedCells = placedCells;
            LineClear = lineClear;
            PowerUpAvailable = powerUpAvailable;
            GameOver = gameOver;
        }
    }
}
