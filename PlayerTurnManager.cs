
using System.Collections.Generic;

namespace Gridrunner;

public class PlayerTurnManager
{
    private readonly ActionPointSystem _actionPoints;

    public bool IsPlayerTurn { get; private set; }

    public PlayerTurnManager(ActionPointSystem actionPoints)
    {
        _actionPoints = actionPoints;
    }

    public void BeginTurn(List<Operative> operatives)
    {
        _actionPoints.BeginTurn(operatives);
        IsPlayerTurn = true;
    }

    public bool CanPerformAction(Operative operative, int cost = 1)
    {
        return IsPlayerTurn &&
               operative.IsAlive &&
               _actionPoints.CanSpend(operative, cost);
    }

    public bool SpendAction(Operative operative, int cost = 1)
    {
        if (!CanPerformAction(operative, cost))
            return false;

        return _actionPoints.TrySpend(operative, cost);
    }

    public int GetRemainingAP(Operative operative)
    {
        return _actionPoints.GetRemaining(operative);
    }

    public bool AreAllActionsUsed(List<Operative> operatives)
    {
        foreach (Operative operative in operatives)
        {
            if (operative.IsAlive &&
                _actionPoints.HasActionsRemaining(operative))
            {
                return false;
            }
        }

        return true;
    }

    public void EndTurn()
    {
        IsPlayerTurn = false;
    }
}
