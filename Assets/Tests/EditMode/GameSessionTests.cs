using System;
using BlockMerge.Core;
using NUnit.Framework;

namespace BlockMerge.Core.Tests
{
    public class GameSessionTests
    {
        private static Piece SingleCell(BlockColor color) =>
            new Piece(new[] { new GridCoord(0, 0) }, color);

        private static Piece Domino(BlockColor color) =>
            new Piece(new[] { new GridCoord(0, 0), new GridCoord(0, 1) }, color);

        // Deterministic tray: every generated piece is a single red cell, so tests
        // control the board purely through where they place it.
        private static GameSession NewDeterministicSession(int boardSize = 8) =>
            new GameSession(boardSize, new PieceFactory(new DeterministicRandom()));

        private sealed class DeterministicRandom : Random
        {
            public override int Next(int maxValue) => 0; // always picks shapes[0] (1x1) and colors[0] (Red)
        }

        [Test]
        public void PlacePiece_IncreasesScoreByCellCount()
        {
            var session = NewDeterministicSession();

            var outcome = session.PlacePiece(0, new GridCoord(0, 0));

            Assert.IsTrue(outcome.Placed);
            Assert.AreEqual(1, session.Score);
        }

        [Test]
        public void PlacePiece_RejectsOccupiedCell()
        {
            var session = NewDeterministicSession();
            session.PlacePiece(0, new GridCoord(0, 0));

            var outcome = session.PlacePiece(1, new GridCoord(0, 0));

            Assert.IsFalse(outcome.Placed);
            Assert.AreEqual(1, session.Score); // unchanged
        }

        [Test]
        public void PlacePiece_RefillsTray_WhenAllSlotsUsed()
        {
            var session = NewDeterministicSession();

            session.PlacePiece(0, new GridCoord(0, 0));
            session.PlacePiece(1, new GridCoord(1, 0));
            session.PlacePiece(2, new GridCoord(2, 0));

            Assert.IsTrue(Array.TrueForAll(session.Tray, p => p != null));
        }

        [Test]
        public void PlacePiece_ClearingLine_AwardsLineBonusAndMerge()
        {
            var session = NewDeterministicSession(4);

            // Manually stack the tray with single-cell pieces of chosen colors so we can
            // fill row 0 with a 3-red-run (merges) plus a lone cyan (does not merge).
            session.Tray[0] = SingleCell(BlockColor.Red);
            session.PlacePiece(0, new GridCoord(0, 0));
            session.Tray[1] = SingleCell(BlockColor.Red);
            session.PlacePiece(1, new GridCoord(0, 1));
            session.Tray[2] = SingleCell(BlockColor.Red);
            session.PlacePiece(2, new GridCoord(0, 2));

            // Tray refilled after the 3rd placement (deterministic factory -> red 1x1s again).
            session.Tray[0] = SingleCell(BlockColor.Cyan);
            var outcome = session.PlacePiece(0, new GridCoord(0, 3));

            Assert.IsTrue(outcome.LineClear.AnyLinesCleared);
            Assert.AreEqual(1, outcome.LineClear.MergedGroups.Count);
            Assert.AreEqual(3, outcome.LineClear.MergedGroups[0].Count);

            // score = 1+1+1 (placements) + 1 (final placement) + lineBonus(10) + mergeBonus(12)
            Assert.AreEqual(4 + 10 + 12, session.Score);
            Assert.AreEqual(12, session.Meter.Value);
        }

        [Test]
        public void UsePowerUp_IsNoOp_WhenMeterNotFull()
        {
            var session = NewDeterministicSession();

            var affected = session.UsePowerUp(PowerUpType.Bomb, new GridCoord(0, 0));

            Assert.AreEqual(0, affected.Count);
        }

        [Test]
        public void UsePowerUp_ResetsMeter_AfterUse()
        {
            var session = NewDeterministicSession(4);
            // Prime the meter directly (public API), then make any placement — PlacePiece
            // re-evaluates PowerUpAvailable = Meter.IsFull unconditionally at the end of
            // every call, whether or not that placement itself cleared a line.
            session.Meter.Add(ChargeMeter.Max);
            session.PlacePiece(0, new GridCoord(0, 0));
            Assert.IsTrue(session.PowerUpAvailable);

            var affected = session.UsePowerUp(PowerUpType.Bomb, new GridCoord(0, 0));

            Assert.AreEqual(1, affected.Count);
            Assert.AreEqual(0, session.Meter.Value);
            Assert.IsFalse(session.PowerUpAvailable);
        }

        [Test]
        public void GameOver_Detected_WhenRemainingPiecesCannotFitAnywhere()
        {
            var session = NewDeterministicSession(4);

            // Checkerboard-fill the board directly via Board.Place (bypassing PlacePiece,
            // so nothing auto-clears mid-setup): odd (row+col) cells solid, even cells
            // empty. No two empty cells are ever orthogonally adjacent, so once this
            // pattern is in place a 2-cell piece can never land anywhere on the board.
            for (int r = 0; r < 4; r++)
                for (int c = 0; c < 4; c++)
                    if ((r + c) % 2 == 1)
                        session.Board.Place(SingleCell(BlockColor.Red), new GridCoord(r, c));

            session.Tray[0] = SingleCell(BlockColor.Cyan); // the last fittable single-cell gap
            session.Tray[1] = Domino(BlockColor.Red);
            session.Tray[2] = Domino(BlockColor.Red);

            var outcome = session.PlacePiece(0, new GridCoord(0, 0));

            Assert.IsTrue(outcome.Placed);
            Assert.IsFalse(outcome.LineClear.AnyLinesCleared);
            Assert.IsTrue(outcome.GameOver);
            Assert.IsTrue(session.IsGameOver);
        }

        [Test]
        public void Restart_ClearsBoardScoreAndMeter()
        {
            var session = NewDeterministicSession();
            session.PlacePiece(0, new GridCoord(0, 0));
            session.Meter.Add(50);

            session.Restart();

            Assert.AreEqual(0, session.Score);
            Assert.AreEqual(0, session.Meter.Value);
            Assert.IsFalse(session.IsGameOver);
            Assert.IsNull(session.Board[0, 0]);
            Assert.IsTrue(Array.TrueForAll(session.Tray, p => p != null));
        }
    }
}
