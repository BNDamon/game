using System.Collections.Generic;

namespace BlockMerge.Core
{
    public sealed class LineClearResult
    {
        public static readonly LineClearResult None = new LineClearResult(
            new List<int>(), new List<int>(), new List<GridCoord>(),
            new List<IReadOnlyList<GridCoord>>(), new List<GridCoord>(), 0, 0);

        public IReadOnlyList<int> ClearedRows { get; }
        public IReadOnlyList<int> ClearedCols { get; }

        /// <summary>Every cell being cleared this resolution (rows ∪ cols).</summary>
        public IReadOnlyList<GridCoord> ClearedCells { get; }

        /// <summary>Connected same-color groups within ClearedCells that met the merge threshold.</summary>
        public IReadOnlyList<IReadOnlyList<GridCoord>> MergedGroups { get; }

        /// <summary>Flattened union of MergedGroups, for convenience (e.g. VFX cues).</summary>
        public IReadOnlyList<GridCoord> MergedCells { get; }

        public int LineBonus { get; }
        public int MergeBonus { get; }

        public bool AnyLinesCleared => ClearedRows.Count > 0 || ClearedCols.Count > 0;

        public LineClearResult(
            IReadOnlyList<int> clearedRows,
            IReadOnlyList<int> clearedCols,
            IReadOnlyList<GridCoord> clearedCells,
            IReadOnlyList<IReadOnlyList<GridCoord>> mergedGroups,
            IReadOnlyList<GridCoord> mergedCells,
            int lineBonus,
            int mergeBonus)
        {
            ClearedRows = clearedRows;
            ClearedCols = clearedCols;
            ClearedCells = clearedCells;
            MergedGroups = mergedGroups;
            MergedCells = mergedCells;
            LineBonus = lineBonus;
            MergeBonus = mergeBonus;
        }
    }
}
