
using System.Collections.Generic;

namespace Gridrunner;

public class ActionPointSystem
{
    public const int MaxActionPoints = 2;

    private readonly Dictionary<Operative, int> _actionPoints = new();

    // Reset all operatives at the beginning of the player's turn.
    public void BeginTurn(IEnumerable<Operative> operatives)
    {
        _actionPoints.Clear();

        foreach (Operative operative in operatives)
        {
            if (operative.IsAlive)
            {
                _actionPoints[operative] = MaxActionPoints;
            }
        }
    }

    // Check whether an operative has enough AP.
    public bool CanSpend(Operative operative, int amount = 1)
    {
        return amount > 0 &&
               _actionPoints.TryGetValue(operative, out int points) &&
               points >= amount;
    }

    // Spend AP after a successful action.
    public bool TrySpend(Operative operative, int amount = 1)
    {
        if (!CanSpend(operative, amount))
            return false;

        _actionPoints[operative] -= amount;
        return true;
    }

    // Get remaining AP.
    public int GetRemaining(Operative operative)
    {
        return _actionPoints.TryGetValue(operative, out int points)
            ? points
            : 0;
    }

    // Check whether an operative has finished their actions.
    public bool HasActionsRemaining(Operative operative)
    {
        return GetRemaining(operative) > 0;
    }
}
