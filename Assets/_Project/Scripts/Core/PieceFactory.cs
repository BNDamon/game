using System;

namespace BlockMerge.Core
{
    /// <summary>Random piece generator, mirroring randomPiece() in the prototype.
    /// Takes an injectable System.Random so game logic tests can be deterministic.</summary>
    public sealed class PieceFactory
    {
        private static readonly int ColorCount = Enum.GetValues(typeof(BlockColor)).Length;

        private readonly Random _random;

        public PieceFactory(Random random = null)
        {
            _random = random ?? new Random();
        }

        public Piece CreateRandom()
        {
            var shape = PieceShapes.All[_random.Next(PieceShapes.All.Count)];
            var color = (BlockColor)_random.Next(ColorCount);
            return new Piece(shape, color);
        }

        /// <summary>The 1x1 shape with a random color. Used to guarantee a solvable tray:
        /// a single cell always fits as long as the board isn't completely empty-cell-free,
        /// which can't happen since a fully-filled board clears itself.</summary>
        public Piece CreateSingleCell()
        {
            var color = (BlockColor)_random.Next(ColorCount);
            return new Piece(PieceShapes.All[0], color);
        }
    }
}
