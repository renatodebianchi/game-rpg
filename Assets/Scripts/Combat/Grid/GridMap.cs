using System;
using System.Collections.Generic;

namespace GameRpg.Combat.Grid
{
    public enum TerrainType
    {
        Normal,
        Difficult,
        Blocked
    }

    [Serializable]
    public struct GridCoordinate : IEquatable<GridCoordinate>
    {
        public int X;
        public int Y;

        public GridCoordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(GridCoordinate other) => X == other.X && Y == other.Y;

        public override bool Equals(object obj) => obj is GridCoordinate other && Equals(other);

        public override int GetHashCode() => (X, Y).GetHashCode();

        public override string ToString() => $"({X}, {Y})";

        public static int ManhattanDistance(GridCoordinate a, GridCoordinate b) =>
            Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }

    public class GridCell
    {
        public GridCoordinate Coordinate { get; }
        public TerrainType Terrain { get; set; }
        public string OccupantId { get; private set; }

        public bool IsOccupied => OccupantId != null;

        public GridCell(GridCoordinate coordinate, TerrainType terrain = TerrainType.Normal)
        {
            Coordinate = coordinate;
            Terrain = terrain;
        }

        public void SetOccupant(string occupantId)
        {
            if (occupantId == null)
            {
                throw new ArgumentNullException(nameof(occupantId));
            }

            if (IsOccupied)
            {
                throw new InvalidOperationException(
                    $"Grid cell {Coordinate} is already occupied by '{OccupantId}'.");
            }

            OccupantId = occupantId;
        }

        public void ClearOccupant()
        {
            OccupantId = null;
        }
    }

    /// <summary>
    /// Logical combat grid, deliberately decoupled from any Unity scene/Tilemap
    /// representation (see research.md, "Decision: Movimento e combate em grade").
    /// </summary>
    public class GridMap
    {
        private readonly Dictionary<GridCoordinate, GridCell> _cells = new Dictionary<GridCoordinate, GridCell>();

        public int Width { get; }
        public int Height { get; }

        public GridMap(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Grid dimensions must be positive.");
            }

            Width = width;
            Height = height;

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var coordinate = new GridCoordinate(x, y);
                    _cells[coordinate] = new GridCell(coordinate);
                }
            }
        }

        public bool IsWithinBounds(GridCoordinate coordinate) =>
            coordinate.X >= 0 && coordinate.X < Width && coordinate.Y >= 0 && coordinate.Y < Height;

        public GridCell GetCell(GridCoordinate coordinate)
        {
            if (!_cells.TryGetValue(coordinate, out var cell))
            {
                throw new ArgumentOutOfRangeException(nameof(coordinate), $"Coordinate {coordinate} is outside the grid.");
            }

            return cell;
        }

        public bool IsOccupied(GridCoordinate coordinate) => GetCell(coordinate).IsOccupied;

        public bool IsPassable(GridCoordinate coordinate) =>
            IsWithinBounds(coordinate) &&
            GetCell(coordinate).Terrain != TerrainType.Blocked &&
            !GetCell(coordinate).IsOccupied;

        public void PlaceOccupant(GridCoordinate coordinate, string occupantId)
        {
            GetCell(coordinate).SetOccupant(occupantId);
        }

        public void RemoveOccupant(GridCoordinate coordinate)
        {
            GetCell(coordinate).ClearOccupant();
        }

        public IEnumerable<GridCoordinate> GetOrthogonalNeighbors(GridCoordinate coordinate)
        {
            var candidates = new[]
            {
                new GridCoordinate(coordinate.X + 1, coordinate.Y),
                new GridCoordinate(coordinate.X - 1, coordinate.Y),
                new GridCoordinate(coordinate.X, coordinate.Y + 1),
                new GridCoordinate(coordinate.X, coordinate.Y - 1),
            };

            foreach (var candidate in candidates)
            {
                if (IsWithinBounds(candidate))
                {
                    yield return candidate;
                }
            }
        }
    }
}
