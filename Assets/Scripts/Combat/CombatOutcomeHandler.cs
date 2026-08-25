using System;

namespace GameRpg.Combat
{
    public readonly struct CombatantHarmedEvent
    {
        public readonly ICombatant Combatant;
        public readonly string LinkedNpcId;
        public readonly int DamageAmount;
        public readonly bool WasDefeated;

        public CombatantHarmedEvent(ICombatant combatant, string linkedNpcId, int damageAmount, bool wasDefeated)
        {
            Combatant = combatant;
            LinkedNpcId = linkedNpcId;
            DamageAmount = damageAmount;
            WasDefeated = wasDefeated;
        }
    }

    /// <summary>
    /// Reacts to a CombatEncounter reaching a terminal state (FR-003): grants
    /// rewards on victory, marks a checkpoint on defeat, and simply lets a
    /// successful flee exit back to exploration. Also re-exposes per-combatant
    /// damage/defeat events enriched with LinkedNpcId, which World.ForcedCombatReputationBridge
    /// (US4, T057) subscribes to in order to apply FR-022.
    /// </summary>
    public class CombatOutcomeHandler
    {
        private readonly CombatEncounter _encounter;

        public event Action<int> VictoryRewardsGranted;
        public event Action DefeatCheckpointRestored;
        public event Action<CombatantHarmedEvent> CombatantHarmed;

        public CombatOutcomeHandler(CombatEncounter encounter)
        {
            _encounter = encounter ?? throw new ArgumentNullException(nameof(encounter));
            _encounter.ParticipantDamaged += OnParticipantDamaged;
            _encounter.ParticipantDefeated += OnParticipantDefeated;
        }

        /// <summary>Call once per frame/turn-end (or from a state-change hook) to react to a just-reached terminal state.</summary>
        public void HandleStateIfTerminal(int experienceRewardOnVictory)
        {
            switch (_encounter.State)
            {
                case CombatEncounterState.WonByPlayer:
                    VictoryRewardsGranted?.Invoke(experienceRewardOnVictory);
                    break;
                case CombatEncounterState.PlayerDefeated:
                    DefeatCheckpointRestored?.Invoke();
                    break;
                case CombatEncounterState.PlayerFled:
                    // No explicit reward/penalty: exploration resumes as-is.
                    break;
            }
        }

        private void OnParticipantDamaged(ICombatant combatant, int amount)
        {
            CombatantHarmed?.Invoke(new CombatantHarmedEvent(
                combatant,
                (combatant as NonPlayerCombatant)?.LinkedNpcId,
                amount,
                wasDefeated: combatant.IsDefeated));
        }

        private void OnParticipantDefeated(ICombatant combatant)
        {
            // Already covered by OnParticipantDamaged's wasDefeated flag for the
            // triggering hit; this handler exists so CombatEncounter's two events
            // stay independently subscribable without forcing consumers to infer
            // defeat from damage payloads alone.
        }
    }
}
