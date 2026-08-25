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
    /// State machine for a single combat encounter (FR-001, FR-003).
    /// Owns the participant list and initiative order; turn resource
    /// management lives in TurnResourceManager, action resolution in
    /// ActionResolver.
    /// </summary>
    public class CombatEncounter
    {
        private readonly List<ICombatant> _participants;
        private readonly HashSet<ICombatant> _playerSide;

        public CombatEncounterState State { get; private set; } = CombatEncounterState.NotStarted;
        public IReadOnlyList<ICombatant> Participants => _participants;
        public IReadOnlyList<ICombatant> InitiativeOrder { get; private set; } = Array.Empty<ICombatant>();
        public int CurrentTurnIndex { get; private set; }

        /// <summary>Raised whenever a participant takes damage or is defeated (consumed by World.ForcedCombatReputationBridge, US4).</summary>
        public event Action<ICombatant, int> ParticipantDamaged;
        public event Action<ICombatant> ParticipantDefeated;

        public CombatEncounter(IEnumerable<ICombatant> playerSide, IEnumerable<ICombatant> enemySide)
        {
            _playerSide = new HashSet<ICombatant>(playerSide ?? throw new ArgumentNullException(nameof(playerSide)));
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

        public bool IsPlayerSide(ICombatant combatant) => _playerSide.Contains(combatant);

        /// <summary>Starts the encounter using a pre-computed initiative order (see InitiativeService).</summary>
        public void Start(IReadOnlyList<ICombatant> initiativeOrder)
        {
            if (State != CombatEncounterState.NotStarted)
            {
                throw new InvalidOperationException($"Cannot start a combat encounter in state {State}.");
            }

            InitiativeOrder = initiativeOrder ?? throw new ArgumentNullException(nameof(initiativeOrder));
            CurrentTurnIndex = 0;
            State = CombatEncounterState.InProgress;
        }

        public ICombatant CurrentActor => State == CombatEncounterState.InProgress
            ? InitiativeOrder[CurrentTurnIndex]
            : null;

        public void ApplyDamage(ICombatant target, int amount)
        {
            if (State != CombatEncounterState.InProgress)
            {
                throw new InvalidOperationException("Cannot apply damage outside an in-progress encounter.");
            }

            target.ApplyDamage(amount);
            ParticipantDamaged?.Invoke(target, amount);

            if (target.IsDefeated)
            {
                ParticipantDefeated?.Invoke(target);
                EvaluateOutcome();
            }
        }

        /// <summary>Advances to the next living combatant's turn, wrapping around the initiative order.</summary>
        public void AdvanceTurn()
        {
            if (State != CombatEncounterState.InProgress)
            {
                return;
            }

            for (var i = 0; i < InitiativeOrder.Count; i++)
            {
                CurrentTurnIndex = (CurrentTurnIndex + 1) % InitiativeOrder.Count;
                var next = InitiativeOrder[CurrentTurnIndex];
                if (!next.IsDefeated)
                {
                    return;
                }
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
