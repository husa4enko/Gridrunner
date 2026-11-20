
using Microsoft.Xna.Framework;

namespace Gridrunner;

public class Enemy : BattleUnit
{
    public const int MaxHealth = 2;

    public Enemy(Point tile)
        : base(tile, MaxHealth, 5)
    {
    }
}
