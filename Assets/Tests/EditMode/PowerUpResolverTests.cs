using BlockMerge.Core;
using NUnit.Framework;

namespace BlockMerge.Core.Tests
{
    public class PowerUpResolverTests
    {
        private static Piece SingleCell(BlockColor color) =>
            new Piece(new[] { new GridCoord(0, 0) }, color);

        [Test]
        public void Bomb_Clears3x3AreaAroundTarget()
        {
            var board = new Board(8);
            for (int r = 3; r <= 5; r++)
                for (int c = 3; c <= 5; c++)
                    board.Place(SingleCell(BlockColor.Red), new GridCoord(r, c));
            board.Place(SingleCell(BlockColor.Cyan), new GridCoord(2, 2)); // just outside blast

            var affected = PowerUpResolver.Apply(board, PowerUpType.Bomb, new GridCoord(4, 4));

            Assert.AreEqual(9, affected.Count);
            for (int r = 3; r <= 5; r++)
                for (int c = 3; c <= 5; c++)
                    Assert.IsNull(board[r, c]);
            Assert.AreEqual(BlockColor.Cyan, board[2, 2]);
        }

        [Test]
        public void Bomb_ClampsAtBoardEdges()
        {
            var board = new Board(8);
            board.Place(SingleCell(BlockColor.Red), new GridCoord(0, 0));

            var affected = PowerUpResolver.Apply(board, PowerUpType.Bomb, new GridCoord(0, 0));

            Assert.AreEqual(1, affected.Count);
            Assert.IsNull(board[0, 0]);
        }

        [Test]
        public void Line_ClearsEntireTargetRow()
        {
            var board = new Board(8);
            for (int c = 0; c < 8; c++)
                board.Place(SingleCell(BlockColor.Green), new GridCoord(3, c));
            board.Place(SingleCell(BlockColor.Green), new GridCoord(4, 0));

            var affected = PowerUpResolver.Apply(board, PowerUpType.Line, new GridCoord(3, 5));

            Assert.AreEqual(8, affected.Count);
            for (int c = 0; c < 8; c++)
                Assert.IsNull(board[3, c]);
            Assert.AreEqual(BlockColor.Green, board[4, 0]);
        }

        [Test]
        public void ColorWipe_ClearsOnlyMatchingColorAcrossBoard()
        {
            var board = new Board(4);
            board.Place(SingleCell(BlockColor.Purple), new GridCoord(0, 0));
            board.Place(SingleCell(BlockColor.Purple), new GridCoord(3, 3));
            board.Place(SingleCell(BlockColor.Yellow), new GridCoord(1, 1));

            var affected = PowerUpResolver.Apply(board, PowerUpType.ColorWipe, new GridCoord(0, 0));

            Assert.AreEqual(2, affected.Count);
            Assert.IsNull(board[0, 0]);
            Assert.IsNull(board[3, 3]);
            Assert.AreEqual(BlockColor.Yellow, board[1, 1]);
        }

        [Test]
        public void ColorWipe_OnEmptyTarget_ClearsNothing()
        {
            var board = new Board(4);
            board.Place(SingleCell(BlockColor.Purple), new GridCoord(0, 0));

            var affected = PowerUpResolver.Apply(board, PowerUpType.ColorWipe, new GridCoord(2, 2));

            Assert.AreEqual(0, affected.Count);
            Assert.AreEqual(BlockColor.Purple, board[0, 0]);
        }
    }
}
