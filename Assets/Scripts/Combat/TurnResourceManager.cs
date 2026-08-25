using System;

namespace GameRpg.Combat
{
    /// <summary>
    /// Enforces that turn-resource consumption (movement, action, bonus action)
    /// only happens for whoever the CombatEncounter says is the current actor,
    /// and resets those resources at the start of each participant's turn (FR-002).
    /// </summary>
    public class TurnResourceManager
    {
        private readonly CombatEncounter _encounter;

        public TurnResourceManager(CombatEncounter encounter)
        {
            _encounter = encounter ?? throw new ArgumentNullException(nameof(encounter));
        }

        public void ResetForActor(ICombatant actor)
        {
            EnsureCurrentActor(actor);
            actor.TurnResources.ResetForNewTurn();
        }

        public void ConsumeMovement(ICombatant actor, int amount)
        {
            EnsureCurrentActor(actor);
            actor.TurnResources.ConsumeMovement(amount);
        }

        public void ConsumeAction(ICombatant actor)
        {
            EnsureCurrentActor(actor);
            actor.TurnResources.ConsumeAction();
        }

        public void ConsumeBonusAction(ICombatant actor)
        {
            EnsureCurrentActor(actor);
            actor.TurnResources.ConsumeBonusAction();
        }

        private void EnsureCurrentActor(ICombatant actor)
        {
            if (!ReferenceEquals(_encounter.CurrentActor, actor))
            {
                throw new InvalidOperationException(
                    "Cannot spend turn resources for a combatant whose turn it is not.");
            }
        }
    }
}
