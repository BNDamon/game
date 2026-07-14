using System.Collections.Generic;

namespace BlockMerge.Core
{
    public sealed class Piece
    {
        public IReadOnlyList<GridCoord> Cells { get; }
        public BlockColor Color { get; }

        public Piece(IReadOnlyList<GridCoord> cells, BlockColor color)
        {
            Cells = cells;
            Color = color;
        }

        public int CellCount => Cells.Count;
    }
}
