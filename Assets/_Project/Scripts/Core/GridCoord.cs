using System;

namespace BlockMerge.Core
{
    /// <summary>Row/column coordinate. Kept engine-agnostic (no UnityEngine.Vector2Int)
    /// so Core has no Unity dependency and can be unit tested in isolation.</summary>
    public readonly struct GridCoord : IEquatable<GridCoord>
    {
        public readonly int Row;
        public readonly int Col;

        public GridCoord(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public GridCoord Offset(GridCoord delta) => new GridCoord(Row + delta.Row, Col + delta.Col);

        public bool Equals(GridCoord other) => Row == other.Row && Col == other.Col;
        public override bool Equals(object obj) => obj is GridCoord other && Equals(other);
        public override int GetHashCode() => (Row * 397) ^ Col;
        public override string ToString() => $"({Row},{Col})";

        public static bool operator ==(GridCoord a, GridCoord b) => a.Equals(b);
        public static bool operator !=(GridCoord a, GridCoord b) => !a.Equals(b);
    }
}
