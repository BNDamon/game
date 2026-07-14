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
    }
}
