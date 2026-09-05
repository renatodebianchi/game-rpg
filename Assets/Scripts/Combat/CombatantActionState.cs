using System;
using System.Collections.Generic;

namespace GameRpg.Combat
{
    /// <summary>
    /// Real-time combat bookkeeping for a single IRealTimeCombatant: the
    /// limited resource spent on skills/spells (FR-008, "Pontos de Técnica"),
    /// per-action cooldowns, any action currently executing/casting (FR-005,
    /// interruptible per FR-009), and the flee channel (FR-013). Replaces the
    /// turn-based Combat.TurnResources.
    /// </summary>
    public class CombatantActionState
    {
        private const float TechPointRegenPerSecond = 2f;

        private readonly Dictionary<string, float> _cooldownRemainingByActionId = new Dictionary<string, float>();

        public float MaxTechPoints { get; }
        public float CurrentTechPoints { get; private set; }

        public RealTimeActionDefinition PendingAction { get; private set; }
        public IRealTimeCombatant PendingActionTarget { get; private set; }
        public float PendingActionElapsed { get; private set; }
        public bool HasPendingAction => PendingAction != null;

        public bool IsChannelingFlee { get; private set; }
        public float FleeChannelElapsed { get; private set; }

        public CombatantActionState(float maxTechPoints)
        {
            if (maxTechPoints < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxTechPoints));
            }

            MaxTechPoints = maxTechPoints;
            CurrentTechPoints = maxTechPoints;
        }

        public float GetCooldownRemaining(string actionId) =>
            _cooldownRemainingByActionId.TryGetValue(actionId, out var remaining) ? remaining : 0f;

        public bool IsOffCooldown(string actionId) => GetCooldownRemaining(actionId) <= 0f;

        /// <summary>Advances TP regen and every action's cooldown by delta (called once per frame).</summary>
        public void AdvanceTime(TimeSpan delta)
        {
            var seconds = (float)delta.TotalSeconds;

            CurrentTechPoints = Math.Min(MaxTechPoints, CurrentTechPoints + TechPointRegenPerSecond * seconds);

            var actionIds = new List<string>(_cooldownRemainingByActionId.Keys);
            foreach (var actionId in actionIds)
            {
                _cooldownRemainingByActionId[actionId] = Math.Max(0f, _cooldownRemainingByActionId[actionId] - seconds);
            }

            if (HasPendingAction)
            {
                PendingActionElapsed += seconds;
            }
        }

        /// <summary>Contract rule (realtime-action-contract.md): spends the resource, starts the
        /// pending action, and applies the cooldown immediately — before it resolves.</summary>
        public void StartAction(RealTimeActionDefinition action, IRealTimeCombatant target)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (HasPendingAction)
            {
                throw new InvalidOperationException("Cannot start a new action while one is already pending.");
            }

            if (action.ResourceCost > CurrentTechPoints)
            {
                throw new InvalidOperationException("Not enough Tech Points to start this action.");
            }

            CurrentTechPoints -= action.ResourceCost;
            _cooldownRemainingByActionId[action.ActionId] = action.Cooldown;
            PendingAction = action;
            PendingActionTarget = target;
            PendingActionElapsed = 0f;
        }

        /// <summary>The pending action concluded (successfully or not) — clears it either way.</summary>
        public void ClearPendingAction()
        {
            PendingAction = null;
            PendingActionTarget = null;
            PendingActionElapsed = 0f;
        }

        /// <summary>FR-009: being hit while an action is pending interrupts it without effect.</summary>
        public void InterruptPendingAction()
        {
            ClearPendingAction();
        }

        public void AdvanceFleeChannel(TimeSpan delta)
        {
            IsChannelingFlee = true;
            FleeChannelElapsed += (float)delta.TotalSeconds;
        }

        public void ResetFleeChannel()
        {
            IsChannelingFlee = false;
            FleeChannelElapsed = 0f;
        }
    }
}
