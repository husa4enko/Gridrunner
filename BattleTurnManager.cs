
using System.Collections.Generic;
using System.Linq;

namespace Gridrunner;

public class BattleTurnManager
{
    private readonly List<BattleUnit> _turnOrder = new();

    private int _currentTurnIndex = -1;

    public int RoundNumber { get; private set; }

    public BattleUnit CurrentUnit { get; private set; }

    public IReadOnlyList<BattleUnit> TurnOrder => _turnOrder;

    public bool IsBattleActive { get; private set; }

    // Prepare the first round.
    public void StartBattle(List<Operative> operatives, List<Enemy> enemies)
    {
        RoundNumber = 1;
        IsBattleActive = true;

        BuildTurnOrder(operatives, enemies);

        _currentTurnIndex = 0;
        UpdateCurrentUnit();
    }

    // Build initiative order based on unit speed.
    private void BuildTurnOrder(
        List<Operative> operatives,
        List<Enemy> enemies)
    {
        _turnOrder.Clear();

        _turnOrder.AddRange(
            operatives.Where(unit => unit.IsAlive));

        _turnOrder.AddRange(
            enemies.Where(unit => unit.IsAlive));

        _turnOrder.Sort((a, b) => b.Speed.CompareTo(a.Speed));
    }

    // Finish the current unit's activation.
    public void EndCurrentUnitTurn()
    {
        if (!IsBattleActive || CurrentUnit == null)
            return;

        _currentTurnIndex++;

        // Everyone has acted: start another round.
        if (_currentTurnIndex >= _turnOrder.Count)
        {
            StartNextRound();
            return;
        }

        UpdateCurrentUnit();
    }

    // Start a new battle round.
    private void StartNextRound()
    {
        RoundNumber++;

        // Recalculate initiative.
        // Dead units are automatically excluded.
        // Operatives and enemies are supplied again by the next method.
        _currentTurnIndex = 0;

        IsBattleActive = false;
    }

    // Rebuild initiative for the next round.
    public void BeginNextRound(
        List<Operative> operatives,
        List<Enemy> enemies)
    {
        if (IsBattleActive)
            return;

        BuildTurnOrder(operatives, enemies);

        if (_turnOrder.Count == 0)
        {
            CurrentUnit = null;
            return;
        }

        _currentTurnIndex = 0;
        IsBattleActive = true;

        UpdateCurrentUnit();
    }

    // Update the active unit.
    private void UpdateCurrentUnit()
    {
        while (_currentTurnIndex < _turnOrder.Count)
        {
            BattleUnit unit = _turnOrder[_currentTurnIndex];

            if (unit.IsAlive)
            {
                CurrentUnit = unit;
                return;
            }

            _currentTurnIndex++;
        }

        CurrentUnit = null;
    }

    // Check whether the active unit belongs to the player.
    public bool IsPlayerTurn()
    {
        return CurrentUnit is Operative;
    }

    // Check whether the active unit belongs to the enemy.
    public bool IsEnemyTurn()
    {
        return CurrentUnit is Enemy;
    }
}
