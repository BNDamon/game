using System.Collections.Generic;
using System.Linq;
using BlockMerge.Core;
using NUnit.Framework;

namespace BlockMerge.Core.Tests
{
    public class ColorGroupFinderTests
    {
        [Test]
        public void FindGroups_MergesContiguousSameColorCells()
        {
            var board = new Board(4);
            board.Place(new Piece(new[] { new GridCoord(0, 0) }, BlockColor.Red), new GridCoord(0, 0));
            board.Place(new Piece(new[] { new GridCoord(0, 0) }, BlockColor.Red), new GridCoord(0, 1));
            board.Place(new Piece(new[] { new GridCoord(0, 0) }, BlockColor.Red), new GridCoord(0, 2));

            var cellSet = new HashSet<GridCoord>
            {
                new GridCoord(0, 0), new GridCoord(0, 1), new GridCoord(0, 2)
            };

            var groups = ColorGroupFinder.FindGroups(board, cellSet);

            Assert.AreEqual(1, groups.Count);
            Assert.AreEqual(3, groups[0].Count);
        }

        [Test]
        public void FindGroups_SeparatesDifferentColors()
        {
            var board = new Board(4);
            board.Place(new Piece(new[] { new GridCoord(0, 0) }, BlockColor.Red), new GridCoord(0, 0));
            board.Place(new Piece(new[] { new GridCoord(0, 0) }, BlockColor.Cyan), new GridCoord(0, 1));
            board.Place(new Piece(new[] { new GridCoord(0, 0) }, BlockColor.Red), new GridCoord(0, 2));

            var cellSet = new HashSet<GridCoord>
            {
                new GridCoord(0, 0), new GridCoord(0, 1), new GridCoord(0, 2)
            };

            var groups = ColorGroupFinder.FindGroups(board, cellSet);

            Assert.AreEqual(3, groups.Count);
            Assert.IsTrue(groups.All(g => g.Count == 1));
        }

        [Test]
        public void FindGroups_DoesNotConnectDiagonally()
        {
            var board = new Board(4);
            board.Place(new Piece(new[] { new GridCoord(0, 0) }, BlockColor.Red), new GridCoord(0, 0));
            board.Place(new Piece(new[] { new GridCoord(0, 0) }, BlockColor.Red), new GridCoord(1, 1));

            var cellSet = new HashSet<GridCoord> { new GridCoord(0, 0), new GridCoord(1, 1) };

            var groups = ColorGroupFinder.FindGroups(board, cellSet);

            Assert.AreEqual(2, groups.Count);
        }

        [Test]
        public void FindGroups_IgnoresNeighborsOutsideTheCellSet()
        {
            var board = new Board(4);
            // Three red cells in a row on the board, but only the outer two are in the
            // "clearing" set (e.g. only two separate lines cleared, not the middle one).
            board.Place(new Piece(new[] { new GridCoord(0, 0) }, BlockColor.Red), new GridCoord(0, 0));
            board.Place(new Piece(new[] { new GridCoord(0, 0) }, BlockColor.Red), new GridCoord(0, 1));
            board.Place(new Piece(new[] { new GridCoord(0, 0) }, BlockColor.Red), new GridCoord(0, 2));

            var cellSet = new HashSet<GridCoord> { new GridCoord(0, 0), new GridCoord(0, 2) };

            var groups = ColorGroupFinder.FindGroups(board, cellSet);

            Assert.AreEqual(2, groups.Count);
            Assert.IsTrue(groups.All(g => g.Count == 1));
        }
    }
}
