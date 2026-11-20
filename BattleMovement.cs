
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Gridrunner;

public class BattleMovement
{
    private readonly Arena _arena;

    private static readonly Point[] Directions =
    {
        new Point(0, -1), // Up
        new Point(0, 1),  // Down
        new Point(-1, 0), // Left
        new Point(1, 0)   // Right
    };

    public BattleMovement(Arena arena)
    {
        _arena = arena;
    }

    // Find every tile a unit can reach within its movement range.
    public List<Point> GetReachableTiles(
        BattleUnit unit,
        int movementRange,
        IEnumerable<BattleUnit> allUnits)
    {
        List<Point> reachableTiles = new();

        if (movementRange <= 0 || !unit.IsAlive)
            return reachableTiles;

        Queue<(Point Tile, int Distance)> queue = new();
        HashSet<Point> visited = new();

        queue.Enqueue((unit.Tile, 0));
        visited.Add(unit.Tile);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current.Distance >= movementRange)
                continue;

            foreach (Point direction in Directions)
            {
                Point next = current.Tile + direction;

                if (!IsInsideArena(next))
                    continue;

                if (visited.Contains(next))
                    continue;

                if (!IsWalkable(next))
                    continue;

                if (IsOccupied(next, unit, allUnits))
                    continue;

                visited.Add(next);

                int nextDistance = current.Distance + 1;

                reachableTiles.Add(next);

                queue.Enqueue((next, nextDistance));
            }
        }

        return reachableTiles;
    }

    // Find the shortest path to a destination.
    // The returned path excludes the starting tile.
    public List<Point> FindPath(
        BattleUnit unit,
        Point destination,
        IEnumerable<BattleUnit> allUnits)
    {
        List<Point> path = new();

        if (!unit.IsAlive ||
            !IsInsideArena(destination) ||
            !IsWalkable(destination))
        {
            return path;
        }

        if (destination == unit.Tile)
            return path;

        if (IsOccupied(destination, unit, allUnits))
            return path;

        Queue<Point> queue = new();
        HashSet<Point> visited = new();
        Dictionary<Point, Point> previous = new();

        queue.Enqueue(unit.Tile);
        visited.Add(unit.Tile);

        bool found = false;

        while (queue.Count > 0)
        {
            Point current = queue.Dequeue();

            foreach (Point direction in Directions)
            {
                Point next = current + direction;

                if (!IsInsideArena(next))
                    continue;

                if (visited.Contains(next))
                    continue;

                if (!IsWalkable(next))
                    continue;

                if (IsOccupied(next, unit, allUnits))
                    continue;

                visited.Add(next);
                previous[next] = current;

                if (next == destination)
                {
                    found = true;
                    break;
                }

                queue.Enqueue(next);
            }

            if (found)
                break;
        }

        if (!found)
            return path;

        // Reconstruct the path from destination to starting tile.
        Point step = destination;

        while (step != unit.Tile)
        {
            path.Add(step);
            step = previous[step];
        }

        path.Reverse();

        return path;
    }

    // Check whether a tile contains another living unit.
    private static bool IsOccupied(
        Point tile,
        BattleUnit movingUnit,
        IEnumerable<BattleUnit> allUnits)
    {
        foreach (BattleUnit unit in allUnits)
        {
            if (unit != movingUnit &&
                unit.IsAlive &&
                unit.Tile == tile)
            {
                return true;
            }
        }

        return false;
    }

    // Check whether a tile is floor.
    private bool IsWalkable(Point tile)
    {
        return _arena.GetTile(tile.X, tile.Y) == TileType.Floor;
    }

    // Check battlefield boundaries.
    private static bool IsInsideArena(Point tile)
    {
        return tile.X >= 0 &&
               tile.X < Arena.Columns &&
               tile.Y >= 0 &&
               tile.Y < Arena.Rows;
    }
}
