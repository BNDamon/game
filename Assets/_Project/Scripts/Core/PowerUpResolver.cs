using System.Collections.Generic;

namespace BlockMerge.Core
{
    /// <summary>Direct port of usePower(): applies a power-up's effect at a target cell
    /// and returns the cells it cleared, for VFX purposes.</summary>
    public static class PowerUpResolver
    {
        public static IReadOnlyList<GridCoord> Apply(Board board, PowerUpType type, GridCoord target)
        {
            var affected = new List<GridCoord>();

            switch (type)
            {
                case PowerUpType.Bomb:
                    for (int dr = -1; dr <= 1; dr++)
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        var coord = new GridCoord(target.Row + dr, target.Col + dc);
                        if (board.InBounds(coord) && board.At(coord) != null)
                        {
                            board.Clear(coord);
                            affected.Add(coord);
                        }
                    }
                    break;

                case PowerUpType.Line:
                    for (int c = 0; c < board.SizeValue; c++)
                    {
                        var coord = new GridCoord(target.Row, c);
                        if (board.At(coord) != null)
                        {
                            board.Clear(coord);
                            affected.Add(coord);
                        }
                    }
                    break;

                case PowerUpType.ColorWipe:
                    var targetColor = board.At(target);
                    if (targetColor != null)
                    {
                        for (int r = 0; r < board.SizeValue; r++)
                        for (int c = 0; c < board.SizeValue; c++)
                        {
                            var coord = new GridCoord(r, c);
                            if (board.At(coord) == targetColor)
                            {
                                board.Clear(coord);
                                affected.Add(coord);
                            }
                        }
                    }
                    break;
            }

            return affected;
        }
    }
}
