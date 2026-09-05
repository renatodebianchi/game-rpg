using System;
using System.Linq;

namespace GameRpg.Combat
{
    /// <summary>
    /// Continuous enemy behavior for the MVP (FR-007): move in a straight line
    /// toward the nearest living player-side combatant, or attack once within
    /// range. Replaces the turn-based Combat.EnemyAI — decisions happen every
    /// Tick (called once per frame) instead of once per turn, and there is no
    /// pathfinding (research.md: the arena has no obstacles).
    /// </summary>
    public class EnemyCombatAI
    {
        private const float MoveSpeedPerSecond = 2f;

        private readonly CombatArenaEncounter _encounter;
        private readonly RealTimeActionExecutor _executor;
        private readonly BattleArena _arena;
        private readonly RealTimeActionDefinition _basicAttack;

        public EnemyCombatAI(
            CombatArenaEncounter encounter,
            RealTimeActionExecutor executor,
            BattleArena arena,
            RealTimeActionDefinition basicAttack)
        {
            _encounter = encounter ?? throw new ArgumentNullException(nameof(encounter));
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _arena = arena ?? throw new ArgumentNullException(nameof(arena));
            _basicAttack = basicAttack ?? throw new ArgumentNullException(nameof(basicAttack));
        }

        public void Tick(IRealTimeCombatant enemy, TimeSpan delta)
        {
            if (enemy.IsDefeated)
            {
                return;
            }

            var target = SelectTarget();
            if (target == null || enemy.ActionState.HasPendingAction)
            {
                return;
            }

            var distance = Math.Abs(enemy.PositionX - target.PositionX);
            if (distance <= _basicAttack.Range)
            {
                _executor.TryStartAction(enemy, _basicAttack, target);
                return;
            }

            var direction = target.PositionX > enemy.PositionX ? 1f : -1f;
            var proposed = enemy.PositionX + direction * MoveSpeedPerSecond * (float)delta.TotalSeconds;
            enemy.PositionX = _arena.Clamp(proposed);
        }

        private IRealTimeCombatant SelectTarget()
        {
            return _encounter.Participants
                .Where(c => _encounter.IsPlayerSide(c) && !c.IsDefeated)
                .OrderBy(c => c.PositionX) // deterministic tie-break, same spirit as the turn-based EnemyAI
                .FirstOrDefault();
        }
    }
}
