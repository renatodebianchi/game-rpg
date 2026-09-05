using System;
using System.Collections.Generic;

namespace GameRpg.Combat
{
    /// <summary>
    /// Starts and resolves RealTimeActionDefinition uses against a
    /// CombatArenaEncounter (FR-003, FR-004, FR-005, FR-006, FR-008, FR-009).
    /// Replaces the turn-based Combat.ActionResolver — see
    /// specs/004-2d-real-time-combat/contracts/realtime-action-contract.md for
    /// the full contract. Reuses the same IDamageModifier chain the old
    /// ActionResolver exposed, so HungerSystem/SanitySystem/skill-tree
    /// capabilities keep working unchanged (FR-012).
    /// </summary>
    public class RealTimeActionExecutor : IDamageModifierRegistry
    {
        private readonly CombatArenaEncounter _encounter;
        private readonly List<IDamageModifier> _damageModifiers = new List<IDamageModifier>();

        public RealTimeActionExecutor(CombatArenaEncounter encounter)
        {
            _encounter = encounter ?? throw new ArgumentNullException(nameof(encounter));
        }

        public void RegisterDamageModifier(IDamageModifier modifier)
        {
            _damageModifiers.Add(modifier ?? throw new ArgumentNullException(nameof(modifier)));
        }

        /// <summary>
        /// Attempts to start an action (contract preconditions 1-4). Range is
        /// intentionally not checked here — only when the action resolves
        /// (contract rule 5) — so starting a ranged attack/spell while closing
        /// distance is valid.
        /// </summary>
        public bool TryStartAction(
            IRealTimeCombatant actor,
            RealTimeActionDefinition action,
            IRealTimeCombatant target,
            bool hasRequiredCapability = true)
        {
            if (actor == null) throw new ArgumentNullException(nameof(actor));
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (target == null) throw new ArgumentNullException(nameof(target));

            if (actor.ActionState.HasPendingAction)
            {
                return false;
            }

            if (!actor.ActionState.IsOffCooldown(action.ActionId))
            {
                return false;
            }

            if (action.ResourceCost > actor.ActionState.CurrentTechPoints)
            {
                return false;
            }

            if (action.Kind == RealTimeActionKind.Ranged &&
                !string.IsNullOrEmpty(action.RequiredCapabilityId) &&
                !hasRequiredCapability)
            {
                return false;
            }

            actor.ActionState.StartAction(action, target);
            return true;
        }

        /// <summary>
        /// Call once per frame after CombatArenaEncounter.AdvanceTime: resolves
        /// every participant whose pending action's ExecutionTime has elapsed.
        /// </summary>
        public void ResolvePendingActions(IEnumerable<IRealTimeCombatant> participants)
        {
            foreach (var actor in participants)
            {
                if (actor.IsDefeated || !actor.ActionState.HasPendingAction)
                {
                    continue;
                }

                var action = actor.ActionState.PendingAction;
                if (actor.ActionState.PendingActionElapsed < action.ExecutionTime)
                {
                    continue;
                }

                var target = actor.ActionState.PendingActionTarget;
                actor.ActionState.ClearPendingAction();

                if (target == null || target.IsDefeated)
                {
                    continue;
                }

                // Contract rule 5: re-check range at resolution time, not at start time.
                var distance = Math.Abs(actor.PositionX - target.PositionX);
                if (distance > action.Range)
                {
                    continue;
                }

                var damage = action.BaseDamage;
                foreach (var modifier in _damageModifiers)
                {
                    damage = modifier.ModifyOutgoingDamage(actor, damage);
                }

                damage = Math.Max(0, damage);
                _encounter.ApplyDamage(target, damage);
            }
        }
    }
}
