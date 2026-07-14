using System.Collections.Generic;

namespace BlockMerge.Core
{
    /// <summary>Direct port of the SHAPES table from the HTML prototype.</summary>
    public static class PieceShapes
    {
        public static readonly IReadOnlyList<GridCoord[]> All = new List<GridCoord[]>
        {
            new[] { new GridCoord(0, 0) },
            new[] { new GridCoord(0, 0), new GridCoord(0, 1) },
            new[] { new GridCoord(0, 0), new GridCoord(1, 0) },
            new[] { new GridCoord(0, 0), new GridCoord(0, 1), new GridCoord(0, 2) },
            new[] { new GridCoord(0, 0), new GridCoord(1, 0), new GridCoord(2, 0) },
            new[] { new GridCoord(0, 0), new GridCoord(0, 1), new GridCoord(1, 0), new GridCoord(1, 1) },
            new[] { new GridCoord(0, 0), new GridCoord(0, 1), new GridCoord(0, 2), new GridCoord(1, 0) },
            new[] { new GridCoord(0, 0), new GridCoord(0, 1), new GridCoord(0, 2), new GridCoord(1, 2) },
            new[] { new GridCoord(0, 0), new GridCoord(1, 0), new GridCoord(1, 1), new GridCoord(1, 2) },
            new[] { new GridCoord(0, 2), new GridCoord(1, 0), new GridCoord(1, 1), new GridCoord(1, 2) },
            new[] { new GridCoord(0, 0), new GridCoord(0, 1), new GridCoord(1, 1), new GridCoord(1, 2) },
            new[] { new GridCoord(0, 1), new GridCoord(0, 2), new GridCoord(1, 0), new GridCoord(1, 1) },
        };
    }
}
