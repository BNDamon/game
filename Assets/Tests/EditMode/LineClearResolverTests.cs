using BlockMerge.Core;
using NUnit.Framework;

namespace BlockMerge.Core.Tests
{
    public class LineClearResolverTests
    {
        private static void FillRow(Board board, int row, params BlockColor[] colors)
        {
            for (int c = 0; c < colors.Length; c++)
                board.Place(new Piece(new[] { new GridCoord(0, 0) }, colors[c]), new GridCoord(row, c));
        }

        private static void FillCol(Board board, int col, params BlockColor[] colors)
        {
            FillCol(board, col, 0, colors);
        }

        private static void FillCol(Board board, int col, int startRow, params BlockColor[] colors)
        {
            for (int i = 0; i < colors.Length; i++)
                board.Place(new Piece(new[] { new GridCoord(0, 0) }, colors[i]), new GridCoord(startRow + i, col));
        }

        [Test]
        public void Resolve_ReturnsNoClears_WhenNoLineIsFull()
        {
            var board = new Board(4);
            board.Place(new Piece(new[] { new GridCoord(0, 0) }, BlockColor.Red), new GridCoord(0, 0));

            var result = new LineClearResolver().Resolve(board);

            Assert.IsFalse(result.AnyLinesCleared);
            Assert.AreEqual(0, result.ClearedCells.Count);
        }

        [Test]
        public void Resolve_DetectsFullRow_WithNoMerge_WhenColorsAlternate()
        {
            var board = new Board(4);
            FillRow(board, 0, BlockColor.Red, BlockColor.Cyan, BlockColor.Red, BlockColor.Cyan);

            var result = new LineClearResolver().Resolve(board);

            Assert.IsTrue(result.AnyLinesCleared);
            CollectionAssert.AreEqual(new[] { 0 }, result.ClearedRows);
            Assert.AreEqual(4, result.ClearedCells.Count);
            Assert.AreEqual(0, result.MergedGroups.Count);
            Assert.AreEqual(10, result.LineBonus);
            Assert.AreEqual(0, result.MergeBonus);
        }

        [Test]
        public void Resolve_DetectsFullColumn()
        {
            var board = new Board(4);
            FillCol(board, 2, BlockColor.Green, BlockColor.Purple, BlockColor.Green, BlockColor.Purple);

            var result = new LineClearResolver().Resolve(board);

            CollectionAssert.AreEqual(new[] { 2 }, result.ClearedCols);
            Assert.AreEqual(10, result.LineBonus);
        }

        [Test]
        public void Resolve_DoublesLineBonus_WhenRowAndColumnBothClear()
        {
            var board = new Board(4);
            // Fill row 0 and column 0 entirely; corner (0,0) is shared so the column fill
            // starts at row 1. Distinct colors keep any run under the merge threshold so
            // this test isolates the line bonus.
            FillRow(board, 0, BlockColor.Red, BlockColor.Cyan, BlockColor.Red, BlockColor.Cyan);
            FillCol(board, 0, 1, BlockColor.Purple, BlockColor.Green, BlockColor.Purple);

            var result = new LineClearResolver().Resolve(board);

            Assert.AreEqual(1, result.ClearedRows.Count);
            Assert.AreEqual(1, result.ClearedCols.Count);
            Assert.AreEqual(20, result.LineBonus);
        }

        [Test]
        public void Resolve_MergesGroupAtOrAboveThreshold()
        {
            var board = new Board(8);
            // Row of 8 cells: 3 contiguous reds (meets threshold), then distinct colors.
            FillRow(board, 0,
                BlockColor.Red, BlockColor.Red, BlockColor.Red,
                BlockColor.Cyan, BlockColor.Yellow, BlockColor.Purple, BlockColor.Green, BlockColor.Cyan);

            var result = new LineClearResolver().Resolve(board);

            Assert.AreEqual(1, result.MergedGroups.Count);
            Assert.AreEqual(3, result.MergedGroups[0].Count);
            Assert.AreEqual(3 * 4, result.MergeBonus);
            // Merged cells still clear off the board, they just also award charge.
            Assert.AreEqual(8, result.ClearedCells.Count);
        }

        [Test]
        public void Resolve_DoesNotMerge_GroupBelowThreshold()
        {
            var board = new Board(8);
            // Only 2 contiguous reds - below MergeThreshold (3).
            FillRow(board, 0,
                BlockColor.Red, BlockColor.Red,
                BlockColor.Cyan, BlockColor.Yellow, BlockColor.Purple, BlockColor.Green, BlockColor.Cyan, BlockColor.Yellow);

            var result = new LineClearResolver().Resolve(board);

            Assert.AreEqual(0, result.MergedGroups.Count);
            Assert.AreEqual(0, result.MergeBonus);
        }

        [Test]
        public void Apply_ClearsAllClearedCells_IncludingMergedOnes()
        {
            var board = new Board(8);
            FillRow(board, 0,
                BlockColor.Red, BlockColor.Red, BlockColor.Red,
                BlockColor.Cyan, BlockColor.Yellow, BlockColor.Purple, BlockColor.Green, BlockColor.Cyan);

            var resolver = new LineClearResolver();
            var result = resolver.Resolve(board);
            resolver.Apply(board, result);

            for (int c = 0; c < 8; c++)
                Assert.IsNull(board[0, c]);
        }
    }
}
