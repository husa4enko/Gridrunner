using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Gridrunner;

public class EnemyAI
{
    private readonly Arena _arena;

    private static readonly Point[] Directions =
    {
        new Point(0, -1),
        new Point(0, 1),
        new Point(-1, 0),
        new Point(1, 0)
    };

    public EnemyAI(Arena arena)
    {
        _arena = arena;
    }

    public void TakeEnemyTurn(
        List<Enemy> enemies,
        List<Operative> operatives)
    {
        foreach (Enemy enemy in enemies)
        {
            if (!enemy.IsAlive || operatives.Count == 0)
                continue;

            Operative target = FindAdjacentOperative(enemy, operatives);

            if (target != null)
            {
                target.TakeDamage(1);
                continue;
            }

            MoveTowardsOperative(enemy, enemies, operatives);
        }

        operatives.RemoveAll(o => !o.IsAlive);
    }

    private Operative FindAdjacentOperative(
        Enemy enemy,
        List<Operative> operatives)
    {
        foreach (Operative operative in operatives)
        {
            if (IsAdjacent(enemy.Tile, operative.Tile))
                return operative;
        }

        return null;
    }

    private void MoveTowardsOperative(
        Enemy enemy,
        List<Enemy> enemies,
        List<Operative> operatives)
    {
        Queue<(Point Tile, Point FirstStep)> queue = new();
        HashSet<Point> visited = new();

        queue.Enqueue((enemy.Tile, enemy.Tile));
        visited.Add(enemy.Tile);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (Point direction in Directions)
            {
                Point next = current.Tile + direction;

                if (!IsInsideArena(next) || visited.Contains(next))
                    continue;

                if (_arena.GetTile(next.X, next.Y) != TileType.Floor)
                    continue;

                bool blocked = false;

                foreach (Enemy other in enemies)
                {
                    if (other != enemy && other.Tile == next)
                    {
                        blocked = true;
                        break;
                    }
                }

                if (blocked)
                    continue;

                foreach (Operative operative in operatives)
                {
                    if (operative.Tile == next)
                    {
                        blocked = true;
                        break;
                    }
                }

                if (blocked)
                    continue;

                visited.Add(next);

                Point firstStep = current.Tile == enemy.Tile
                    ? next
                    : current.FirstStep;

                foreach (Operative operative in operatives)
                {
                    if (IsAdjacent(next, operative.Tile))
                    {
                        enemy.Tile = firstStep;
                        return;
                    }
                }

                queue.Enqueue((next, firstStep));
            }
        }
    }

    private static bool IsAdjacent(Point a, Point b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) == 1;
    }

    private static bool IsInsideArena(Point tile)
    {
        return tile.X >= 0 &&
               tile.X < Arena.Columns &&
               tile.Y >= 0 &&
               tile.Y < Arena.Rows;
    }
}
