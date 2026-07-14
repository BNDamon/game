using System.Collections.Generic;

namespace BlockMerge.Core
{
    /// <summary>Direct port of findColorGroupsWithin(): flood-fills 4-connected,
    /// same-color regions within a given cell set (a set of cells about to clear).</summary>
    public static class ColorGroupFinder
    {
        private static readonly GridCoord[] Neighbors4 =
        {
            new GridCoord(-1, 0), new GridCoord(1, 0),
            new GridCoord(0, -1), new GridCoord(0, 1)
        };

        public static List<List<GridCoord>> FindGroups(Board board, HashSet<GridCoord> cellSet)
        {
            var visited = new HashSet<GridCoord>();
            var groups = new List<List<GridCoord>>();

            foreach (var start in cellSet)
            {
                if (visited.Contains(start)) continue;
                var color = board.At(start);
                if (color == null) continue;

                var stack = new Stack<GridCoord>();
                stack.Push(start);
                visited.Add(start);
                var group = new List<GridCoord>();

                while (stack.Count > 0)
                {
                    var current = stack.Pop();
                    group.Add(current);

                    foreach (var delta in Neighbors4)
                    {
                        var neighbor = current.Offset(delta);
                        if (!cellSet.Contains(neighbor) || visited.Contains(neighbor)) continue;
                        if (board.At(neighbor) != color) continue;
                        visited.Add(neighbor);
                        stack.Push(neighbor);
                    }
                }

                groups.Add(group);
            }

            return groups;
        }
    }
}
