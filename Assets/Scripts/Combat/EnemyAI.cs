using System.Linq;
using GameRpg.Combat.Grid;

namespace GameRpg.Combat
{
    /// <summary>
    /// Minimal enemy turn behavior for the MVP: move toward the nearest living
    /// player-side combatant and attack once adjacent.
    /// </summary>
    public class EnemyAI
    {
        private const int AttackRange = 1;

        private readonly CombatEncounter _encounter;
        private readonly TurnResourceManager _turnResourceManager;
        private readonly ActionResolver _actionResolver;
        private readonly GridPathfinding _pathfinding;

        public EnemyAI(
            CombatEncounter encounter,
            TurnResourceManager turnResourceManager,
            ActionResolver actionResolver,
            GridPathfinding pathfinding)
        {
            _encounter = encounter;
            _turnResourceManager = turnResourceManager;
            _actionResolver = actionResolver;
            _pathfinding = pathfinding;
        }

        public void TakeTurn(ICombatant enemy, int baseAttackDamage)
        {
            var target = SelectTarget();
            if (target == null)
            {
                return;
            }

            if (GridCoordinate.ManhattanDistance(enemy.Position, target.Position) <= AttackRange)
            {
                _actionResolver.ResolveBasicAttack(enemy, target, baseAttackDamage);
                return;
            }

            MoveTowards(enemy, target.Position);
        }

        private ICombatant SelectTarget()
        {
            return _encounter.Participants
                .Where(c => _encounter.IsPlayerSide(c) && !c.IsDefeated)
                .OrderBy(c => c.Position.X) // deterministic tie-break; real distance handled in MoveTowards
                .FirstOrDefault();
        }

        private void MoveTowards(ICombatant enemy, GridCoordinate destination)
        {
            var path = _pathfinding.FindPath(enemy.Position, destination);
            if (path == null || path.Count <= 1)
            {
                return;
            }

            var movementRemaining = enemy.TurnResources.MovementPointsRemaining;
            var reachableIndex = 0;
            var costSoFar = 0;

            for (var i = 1; i < path.Count; i++)
            {
                var stepCost = _pathfinding.CalculatePathCost(new[] { path[i - 1], path[i] });
                if (costSoFar + stepCost > movementRemaining)
                {
                    break;
                }

                costSoFar += stepCost;
                reachableIndex = i;
            }

            if (reachableIndex == 0)
            {
                return;
            }

            _turnResourceManager.ConsumeMovement(enemy, costSoFar);
            enemy.Position = path[reachableIndex];
        }
    }
}
