using System;
using System.Collections.Generic;
using GameRpg.Combat.Grid;

namespace GameRpg.Combat
{
    /// <summary>
    /// Grid-based movement pathfinding for combat (FR-002's movement resource).
    /// Uses Dijkstra over orthogonal neighbors so Difficult terrain (movement
    /// cost 2) is respected; Blocked/occupied cells are never traversable.
    /// </summary>
    public class GridPathfinding
    {
        private readonly GridMap _gridMap;

        public GridPathfinding(GridMap gridMap)
        {
            _gridMap = gridMap ?? throw new ArgumentNullException(nameof(gridMap));
        }

        /// <summary>
        /// Finds the lowest-cost path from <paramref name="start"/> to <paramref name="destination"/>.
        /// Returns null if no path exists or the destination is not passable.
        /// </summary>
        public IReadOnlyList<GridCoordinate> FindPath(GridCoordinate start, GridCoordinate destination)
        {
            if (!_gridMap.IsWithinBounds(start) || !_gridMap.IsWithinBounds(destination))
            {
                return null;
            }

            if (!_gridMap.IsPassable(destination) && !start.Equals(destination))
            {
                return null;
            }

            var costSoFar = new Dictionary<GridCoordinate, int> { [start] = 0 };
            var cameFrom = new Dictionary<GridCoordinate, GridCoordinate>();
            var frontier = new List<GridCoordinate> { start };

            while (frontier.Count > 0)
            {
                frontier.Sort((a, b) => costSoFar[a].CompareTo(costSoFar[b]));
                var current = frontier[0];
                frontier.RemoveAt(0);

                if (current.Equals(destination))
                {
                    return ReconstructPath(cameFrom, start, destination);
                }

                foreach (var next in _gridMap.GetOrthogonalNeighbors(current))
                {
                    if (!_gridMap.IsPassable(next) && !next.Equals(destination))
                    {
                        continue;
                    }

                    var stepCost = MovementCost(next);
                    var newCost = costSoFar[current] + stepCost;

                    if (!costSoFar.TryGetValue(next, out var existingCost) || newCost < existingCost)
                    {
                        costSoFar[next] = newCost;
                        cameFrom[next] = current;
                        frontier.Add(next);
                    }
                }
            }

            return null;
        }

        /// <summary>Total movement-point cost of the given path (excludes the starting cell).</summary>
        public int CalculatePathCost(IReadOnlyList<GridCoordinate> path)
        {
            if (path == null || path.Count == 0)
            {
                return 0;
            }

            var cost = 0;
            for (var i = 1; i < path.Count; i++)
            {
                cost += MovementCost(path[i]);
            }

            return cost;
        }

        private int MovementCost(GridCoordinate coordinate) =>
            _gridMap.GetCell(coordinate).Terrain == TerrainType.Difficult ? 2 : 1;

        private static IReadOnlyList<GridCoordinate> ReconstructPath(
            Dictionary<GridCoordinate, GridCoordinate> cameFrom,
            GridCoordinate start,
            GridCoordinate destination)
        {
            var path = new List<GridCoordinate> { destination };
            var current = destination;

            while (!current.Equals(start))
            {
                current = cameFrom[current];
                path.Add(current);
            }

            path.Reverse();
            return path;
        }
    }
}
