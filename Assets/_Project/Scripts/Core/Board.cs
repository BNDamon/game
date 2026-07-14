using System;

namespace BlockMerge.Core
{
    /// <summary>The grid data model. Equivalent to `board` (the 2D array of cell colors)
    /// plus canPlace()/placement mutation from the prototype.</summary>
    public sealed class Board
    {
        public const int DefaultSize = 8;

        private readonly BlockColor?[,] _cells;

        public Board(int size = DefaultSize)
        {
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
            SizeValue = size;
            _cells = new BlockColor?[size, size];
        }

        public int SizeValue { get; }

        public BlockColor? this[int row, int col] => _cells[row, col];

        public BlockColor? At(GridCoord coord) => _cells[coord.Row, coord.Col];

        public bool InBounds(GridCoord coord) =>
            coord.Row >= 0 && coord.Row < SizeValue && coord.Col >= 0 && coord.Col < SizeValue;

        public bool IsEmpty(GridCoord coord) => InBounds(coord) && _cells[coord.Row, coord.Col] == null;

        public bool CanPlace(Piece piece, GridCoord origin)
        {
            foreach (var offset in piece.Cells)
            {
                var coord = origin.Offset(offset);
                if (!InBounds(coord)) return false;
                if (_cells[coord.Row, coord.Col] != null) return false;
            }
            return true;
        }

        /// <summary>Stamps the piece's color onto the board. Caller must have checked CanPlace.</summary>
        public GridCoord[] Place(Piece piece, GridCoord origin)
        {
            if (!CanPlace(piece, origin))
                throw new InvalidOperationException($"Piece cannot be placed at {origin}.");

            var placed = new GridCoord[piece.Cells.Count];
            for (int i = 0; i < piece.Cells.Count; i++)
            {
                var coord = origin.Offset(piece.Cells[i]);
                _cells[coord.Row, coord.Col] = piece.Color;
                placed[i] = coord;
            }
            return placed;
        }

        public void Clear(GridCoord coord) => _cells[coord.Row, coord.Col] = null;

        public void ClearAll()
        {
            for (int r = 0; r < SizeValue; r++)
                for (int c = 0; c < SizeValue; c++)
                    _cells[r, c] = null;
        }

        /// <summary>Mirrors checkGameOver()'s per-piece fit check: does this piece fit anywhere?</summary>
        public bool HasAnyValidPlacement(Piece piece)
        {
            for (int r = 0; r < SizeValue; r++)
                for (int c = 0; c < SizeValue; c++)
                    if (CanPlace(piece, new GridCoord(r, c)))
                        return true;
            return false;
        }
    }
}
