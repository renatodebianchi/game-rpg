using System;
using System.Collections.Generic;
using System.Linq;

namespace GameRpg.Combat
{
    /// <summary>
    /// Continuous "hold toward the edge" flee channel (FR-013). Replaces the
    /// turn-based Combat.FleeAction, which spent a menu-turn action instantly;
    /// see specs/004-2d-real-time-combat/contracts/flee-channel-contract.md.
    /// Reuses the same success-chance formula (distance to nearest living
    /// hostile + Dexterity), adapted from GridCoordinate.ManhattanDistance to
    /// a float horizontal distance.
    /// </summary>
    public class RealTimeFleeAction
    {
        private const float MinChannelDurationSeconds = 2f;
        private const double MinSuccessChance = 0.1;
        private const double MaxSuccessChance = 0.9;
        private const float DistanceForMaxChance = 5f;

        private readonly CombatArenaEncounter _encounter;
        private readonly Func<double> _randomRollProvider;

        public RealTimeFleeAction(CombatArenaEncounter encounter, Func<double> randomRollProvider = null)
        {
            _encounter = encounter ?? throw new ArgumentNullException(nameof(encounter));
            _randomRollProvider = randomRollProvider ?? (() => new Random().NextDouble());
        }

        public double CalculateSuccessChance(IRealTimeCombatant fleeingCombatant, IEnumerable<IRealTimeCombatant> hostiles, int dexterity)
        {
            var livingHostiles = hostiles.Where(h => !h.IsDefeated).ToList();
            if (livingHostiles.Count == 0)
            {
                return MaxSuccessChance;
            }

            var nearestDistance = livingHostiles.Min(h => Math.Abs(fleeingCombatant.PositionX - h.PositionX));

            var distanceFactor = Math.Min(1.0, nearestDistance / DistanceForMaxChance);
            var dexterityFactor = Math.Min(1.0, dexterity / 20.0);

            var chance = MinSuccessChance + (MaxSuccessChance - MinSuccessChance) * (0.7 * distanceFactor + 0.3 * dexterityFactor);
            return Math.Clamp(chance, MinSuccessChance, MaxSuccessChance);
        }

        /// <summary>
        /// Advances the flee channel by delta when <paramref name="isAttemptingToFlee"/>
        /// is true (holding the flee command near an arena edge); resets it
        /// otherwise (contract rule 2). Once the channel reaches the minimum
        /// duration, resolves a single flee attempt and resets the channel
        /// regardless of outcome (contract rule 3). Returns true if an attempt
        /// was resolved this call — check the encounter's State for the result.
        /// </summary>
        public bool AdvanceChannel(
            IRealTimeCombatant fleeingCombatant,
            IEnumerable<IRealTimeCombatant> hostiles,
            int dexterity,
            TimeSpan delta,
            bool isAttemptingToFlee)
        {
            var state = fleeingCombatant.ActionState;

            if (!isAttemptingToFlee)
            {
                state.ResetFleeChannel();
                return false;
            }

            state.AdvanceFleeChannel(delta);
            if (state.FleeChannelElapsed < MinChannelDurationSeconds)
            {
                return false;
            }

            var chance = CalculateSuccessChance(fleeingCombatant, hostiles, dexterity);
            var succeeded = _randomRollProvider() < chance;
            state.ResetFleeChannel();

            if (succeeded && _encounter.IsPlayerSide(fleeingCombatant))
            {
                _encounter.MarkPlayerFled();
            }

            return true;
        }
    }
}
