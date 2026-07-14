using System;
using System.Collections.Generic;
using System.Linq;

namespace BlockMerge.Core
{
    /// <summary>Engine-agnostic game-loop orchestrator: owns the board, tray, score,
    /// charge meter, and game-over state, and sequences them the same way the prototype's
    /// onCellClick() -> resolveLines() -> checkPowerUnlock() -> checkGameOver() chain does.
    /// A Unity view/controller wraps this; it has no MonoBehaviour or UnityEngine dependency.</summary>
    public sealed class GameSession
    {
        public const int TraySize = 3;

        private readonly PieceFactory _pieceFactory;
        private readonly LineClearResolver _lineClearResolver = new LineClearResolver();

        public Board Board { get; }
        public ChargeMeter Meter { get; } = new ChargeMeter();
        public ComboTracker Combo { get; } = new ComboTracker();

        /// <summary>Null slots are pieces already used this round. Tray refills when all
        /// three are null — mirrors tray.every(p => p === null) in the prototype. Array
        /// contents are intentionally mutable via the indexer for direct test setup.</summary>
        public Piece[] Tray { get; private set; }

        public int Score { get; private set; }
        public int Best { get; private set; }
        public bool IsGameOver { get; private set; }
        public bool PowerUpAvailable { get; private set; }

        public GameSession(int boardSize = Board.DefaultSize, PieceFactory pieceFactory = null, int startingBest = 0)
        {
            Board = new Board(boardSize);
            _pieceFactory = pieceFactory ?? new PieceFactory();
            Best = startingBest;
            Tray = new Piece[TraySize];
            RefillTray();
        }

        private void RefillTray()
        {
            for (int i = 0; i < TraySize; i++)
                Tray[i] = _pieceFactory.CreateRandom();

            // Guarantee at least one piece in the new tray actually fits the current board,
            // so a refill can never hand the player an instant, unfair game-over.
            if (!Array.Exists(Tray, p => Board.HasAnyValidPlacement(p)))
                Tray[0] = _pieceFactory.CreateSingleCell();
        }

        public bool CanPlace(int trayIndex, GridCoord origin)
        {
            var piece = Tray[trayIndex];
            return piece != null && Board.CanPlace(piece, origin);
        }

        /// <summary>Advances the combo decay clock. Call every frame (or on whatever cadence
        /// the host loop ticks) with real elapsed time — Core has no engine dependency, so it
        /// can't read a clock itself.</summary>
        public void Tick(float deltaTimeSeconds) => Combo.Tick(deltaTimeSeconds);

        public PlacementOutcome PlacePiece(int trayIndex, GridCoord origin)
        {
            if (IsGameOver) return PlacementOutcome.Rejected;

            var piece = Tray[trayIndex];
            if (piece == null || !Board.CanPlace(piece, origin)) return PlacementOutcome.Rejected;

            var placedCells = Board.Place(piece, origin);
            Score += piece.CellCount;
            Tray[trayIndex] = null;

            if (Tray.All(p => p == null)) RefillTray();

            var lineClear = _lineClearResolver.Resolve(Board);
            int comboCount = 0;
            int scoreAwarded = 0;
            if (lineClear.AnyLinesCleared)
            {
                comboCount = Combo.RegisterClear();
                scoreAwarded = (int)Math.Round((lineClear.LineBonus + lineClear.MergeBonus) * Combo.CurrentMultiplier);
                Score += scoreAwarded;
                Meter.Add(lineClear.MergeBonus);
                _lineClearResolver.Apply(Board, lineClear);
            }

            PowerUpAvailable = Meter.IsFull;
            IsGameOver = CheckGameOver();
            if (IsGameOver) Best = Math.Max(Best, Score);

            return new PlacementOutcome(true, placedCells, lineClear, PowerUpAvailable, IsGameOver, comboCount, scoreAwarded);
        }

        public IReadOnlyList<GridCoord> UsePowerUp(PowerUpType type, GridCoord target)
        {
            if (!PowerUpAvailable) return Array.Empty<GridCoord>();

            var affected = PowerUpResolver.Apply(Board, type, target);
            Meter.Reset();
            PowerUpAvailable = false;
            IsGameOver = CheckGameOver();
            if (IsGameOver) Best = Math.Max(Best, Score);
            return affected;
        }

        private bool CheckGameOver()
        {
            foreach (var piece in Tray)
            {
                if (piece != null && Board.HasAnyValidPlacement(piece)) return false;
            }
            return true;
        }

        public void Restart()
        {
            Board.ClearAll();
            Score = 0;
            Meter.Reset();
            Combo.Reset();
            PowerUpAvailable = false;
            IsGameOver = false;
            RefillTray();
        }
    }
}
