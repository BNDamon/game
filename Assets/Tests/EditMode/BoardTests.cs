using BlockMerge.Core;
using NUnit.Framework;

namespace BlockMerge.Core.Tests
{
    public class BoardTests
    {
        private static Piece SingleCell(BlockColor color) =>
            new Piece(new[] { new GridCoord(0, 0) }, color);

        private static Piece Domino(BlockColor color) =>
            new Piece(new[] { new GridCoord(0, 0), new GridCoord(0, 1) }, color);

        [Test]
        public void CanPlace_ReturnsTrue_OnEmptyBoard()
        {
            var board = new Board(8);
            Assert.IsTrue(board.CanPlace(SingleCell(BlockColor.Red), new GridCoord(3, 3)));
        }

        [Test]
        public void CanPlace_ReturnsFalse_WhenOutOfBounds()
        {
            var board = new Board(8);
            Assert.IsFalse(board.CanPlace(Domino(BlockColor.Red), new GridCoord(0, 7)));
        }

        [Test]
        public void CanPlace_ReturnsFalse_WhenCellOccupied()
        {
            var board = new Board(8);
            board.Place(SingleCell(BlockColor.Red), new GridCoord(2, 2));
            Assert.IsFalse(board.CanPlace(Domino(BlockColor.Cyan), new GridCoord(2, 1)));
        }

        [Test]
        public void Place_StampsPieceColorOntoCells()
        {
            var board = new Board(8);
            var placed = board.Place(Domino(BlockColor.Yellow), new GridCoord(1, 1));

            Assert.AreEqual(BlockColor.Yellow, board[1, 1]);
            Assert.AreEqual(BlockColor.Yellow, board[1, 2]);
            Assert.AreEqual(2, placed.Length);
        }

        [Test]
        public void Place_Throws_WhenPlacementInvalid()
        {
            var board = new Board(8);
            board.Place(SingleCell(BlockColor.Red), new GridCoord(0, 0));

            Assert.Throws<System.InvalidOperationException>(() =>
                board.Place(SingleCell(BlockColor.Cyan), new GridCoord(0, 0)));
        }

        [Test]
        public void HasAnyValidPlacement_ReturnsFalse_WhenNoRoomFits()
        {
            var board = new Board(2);
            board.Place(SingleCell(BlockColor.Red), new GridCoord(0, 0));
            board.Place(SingleCell(BlockColor.Red), new GridCoord(0, 1));
            board.Place(SingleCell(BlockColor.Red), new GridCoord(1, 0));
            board.Place(SingleCell(BlockColor.Red), new GridCoord(1, 1));

            Assert.IsFalse(board.HasAnyValidPlacement(SingleCell(BlockColor.Cyan)));
        }

        [Test]
        public void HasAnyValidPlacement_ReturnsTrue_WhenRoomRemains()
        {
            var board = new Board(2);
            board.Place(SingleCell(BlockColor.Red), new GridCoord(0, 0));

            Assert.IsTrue(board.HasAnyValidPlacement(SingleCell(BlockColor.Cyan)));
        }

        [Test]
        public void ClearAll_EmptiesEveryCell()
        {
            var board = new Board(3);
            board.Place(SingleCell(BlockColor.Red), new GridCoord(1, 1));

            board.ClearAll();

            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    Assert.IsNull(board[r, c]);
        }
    }
}
