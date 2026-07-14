using System.Collections.Generic;
using System.Linq;

namespace BlockMerge.Core
{
    /// <summary>Direct port of resolveLines(): finds full rows/cols, splits the cleared
    /// cells into same-color connected groups, and decides which groups "merge into charge"
    /// (>= MergeThreshold cells) vs. pop normally. Resolve() is a pure query — call Apply()
    /// separately once any clear/merge VFX has had a chance to read the result.</summary>
    public sealed class LineClearResolver
    {
        public const int MergeThreshold = 3;
        private const int LineBonusPerLine = 10;
        private const int MergeBonusPerCell = 4;

        public LineClearResult Resolve(Board board)
        {
            var clearedRows = new List<int>();
            for (int r = 0; r < board.SizeValue; r++)
            {
                bool full = true;
                for (int c = 0; c < board.SizeValue; c++)
                {
                    if (board[r, c] == null) { full = false; break; }
                }
                if (full) clearedRows.Add(r);
            }

            var clearedCols = new List<int>();
            for (int c = 0; c < board.SizeValue; c++)
            {
                bool full = true;
                for (int r = 0; r < board.SizeValue; r++)
                {
                    if (board[r, c] == null) { full = false; break; }
                }
                if (full) clearedCols.Add(c);
            }

            if (clearedRows.Count == 0 && clearedCols.Count == 0)
                return LineClearResult.None;

            var clearedCells = new HashSet<GridCoord>();
            foreach (var r in clearedRows)
                for (int c = 0; c < board.SizeValue; c++)
                    clearedCells.Add(new GridCoord(r, c));
            foreach (var c in clearedCols)
                for (int r = 0; r < board.SizeValue; r++)
                    clearedCells.Add(new GridCoord(r, c));

            var groups = ColorGroupFinder.FindGroups(board, clearedCells);

            var mergedGroups = new List<IReadOnlyList<GridCoord>>();
            var mergedCells = new HashSet<GridCoord>();
            int mergeBonus = 0;
            foreach (var group in groups)
            {
                if (group.Count < MergeThreshold) continue;
                mergeBonus += group.Count * MergeBonusPerCell;
                mergedGroups.Add(group);
                foreach (var cell in group) mergedCells.Add(cell);
            }

            int lineBonus = (clearedRows.Count + clearedCols.Count) * LineBonusPerLine;

            return new LineClearResult(
                clearedRows,
                clearedCols,
                clearedCells.ToList(),
                mergedGroups,
                mergedCells.ToList(),
                lineBonus,
                mergeBonus);
        }

        /// <summary>Empties every cleared cell on the board. Merged cells clear too —
        /// merging converts them into meter charge, not a surviving block.</summary>
        public void Apply(Board board, LineClearResult result)
        {
            foreach (var cell in result.ClearedCells)
                board.Clear(cell);
        }
    }
}
