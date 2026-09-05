using System;
using System.Collections.Generic;
using System.Linq;

namespace GameRpg.Combat
{
    public enum CombatEncounterState
    {
        NotStarted,
        InProgress,
        WonByPlayer,
        PlayerFled,
        PlayerDefeated
    }

    /// <summary>
    /// State of a single real-time combat encounter in a BattleArena (FR-001,
    /// FR-006, FR-007). Replaces the turn-based Combat.CombatEncounter — there
    /// is no initiative order/current-turn-index; every participant advances
    /// simultaneously via AdvanceTime, called once per frame by whatever
    /// MonoBehaviour drives the encounter (same pattern as Core.WorldClock).
    /// </summary>
    public class CombatArenaEncounter
    {
        private readonly List<IRealTimeCombatant> _participants;
        private readonly HashSet<IRealTimeCombatant> _playerSide;

        public CombatEncounterState State { get; private set; } = CombatEncounterState.NotStarted;
        public IReadOnlyList<IRealTimeCombatant> Participants => _participants;

        /// <summary>Raised whenever a participant takes damage or is defeated (consumed by World.ForcedCombatReputationBridge).</summary>
        public event Action<IRealTimeCombatant, int> ParticipantDamaged;
        public event Action<IRealTimeCombatant> ParticipantDefeated;

        public CombatArenaEncounter(IEnumerable<IRealTimeCombatant> playerSide, IEnumerable<IRealTimeCombatant> enemySide)
        {
            _playerSide = new HashSet<IRealTimeCombatant>(playerSide ?? throw new ArgumentNullException(nameof(playerSide)));
            var enemies = (enemySide ?? throw new ArgumentNullException(nameof(enemySide))).ToList();

            if (_playerSide.Count == 0)
            {
                throw new ArgumentException("A combat encounter needs at least one player-side combatant.", nameof(playerSide));
            }

            if (enemies.Count == 0)
            {
                throw new ArgumentException("A combat encounter needs at least one enemy.", nameof(enemySide));
            }

            _participants = _playerSide.Concat(enemies).ToList();
        }

        public bool IsPlayerSide(IRealTimeCombatant combatant) => _playerSide.Contains(combatant);

        public void Start()
        {
            if (State != CombatEncounterState.NotStarted)
            {
                throw new InvalidOperationException($"Cannot start a combat encounter in state {State}.");
            }

            State = CombatEncounterState.InProgress;
        }

        /// <summary>Advances every participant's real-time bookkeeping (cooldowns, pending
        /// actions, flee channel). Enemy AI / player input drive their own decisions
        /// separately; this only advances the shared clock-like state.</summary>
        public void AdvanceTime(TimeSpan delta)
        {
            if (State != CombatEncounterState.InProgress)
            {
                return;
            }

            foreach (var participant in _participants)
            {
                if (!participant.IsDefeated)
                {
                    participant.ActionState.AdvanceTime(delta);
                }
            }
        }

        public void ApplyDamage(IRealTimeCombatant target, int amount)
        {
            if (State != CombatEncounterState.InProgress)
            {
                throw new InvalidOperationException("Cannot apply damage outside an in-progress encounter.");
            }

            // FR-009: being hit interrupts whatever action the target was casting/executing.
            if (target.ActionState.HasPendingAction)
            {
                target.ActionState.InterruptPendingAction();
            }

            target.ApplyDamage(amount);
            ParticipantDamaged?.Invoke(target, amount);

            if (target.IsDefeated)
            {
                ParticipantDefeated?.Invoke(target);
                EvaluateOutcome();
            }
        }

        public void MarkPlayerFled()
        {
            if (State != CombatEncounterState.InProgress)
            {
                throw new InvalidOperationException("Cannot flee outside an in-progress encounter.");
            }

            State = CombatEncounterState.PlayerFled;
        }

        private void EvaluateOutcome()
        {
            if (_playerSide.All(c => c.IsDefeated))
            {
                State = CombatEncounterState.PlayerDefeated;
                return;
            }

            if (_participants.Except(_playerSide).All(c => c.IsDefeated))
            {
                State = CombatEncounterState.WonByPlayer;
            }
        }
    }
}
