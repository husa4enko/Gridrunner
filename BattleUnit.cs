
using System;
using Microsoft.Xna.Framework;

namespace Gridrunner;

public abstract class BattleUnit
{
    public Point Tile { get; set; }

    public int Health { get; private set; }

    public int MaximumHealth { get; }

    public int Speed { get; }

    public bool IsAlive => Health > 0;

    protected BattleUnit(Point tile, int maximumHealth, int speed)
    {
        Tile = tile;
        MaximumHealth = maximumHealth;
        Health = maximumHealth;
        Speed = speed;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || !IsAlive)
            return;

        Health = Math.Max(0, Health - damage);
    }

    public void RestoreHealth()
    {
        Health = MaximumHealth;
    }
}
