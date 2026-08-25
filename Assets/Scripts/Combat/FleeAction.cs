using System;
using System.Collections.Generic;
using System.Linq;
using GameRpg.Combat.Grid;

namespace GameRpg.Combat
{
    /// <summary>
    /// Implements the "flee" way of ending a combat encounter (FR-003).
    /// Resolves G1 from /speckit-analyze: previously only the resulting
    /// PlayerFled state existed, with no task implementing the action itself.
    /// </summary>
    public class FleeAction
    {
        private const double MinSuccessChance = 0.1;
        private const double MaxSuccessChance = 0.9;
        private const int DistanceForMaxChance = 5;

        private readonly CombatEncounter _encounter;
        private readonly TurnResourceManager _turnResourceManager;
        private readonly Func<double> _randomRollProvider;

        public FleeAction(
            CombatEncounter encounter,
            TurnResourceManager turnResourceManager,
            Func<double> randomRollProvider = null)
        {
            _encounter = encounter ?? throw new ArgumentNullException(nameof(encounter));
            _turnResourceManager = turnResourceManager ?? throw new ArgumentNullException(nameof(turnResourceManager));
            _randomRollProvider = randomRollProvider ?? (() => new Random().NextDouble());
        }

        /// <summary>
        /// Chance of success grows with distance from the nearest living hostile
        /// and with the fleeing combatant's Dexterity, clamped to a sane range so
        /// fleeing is never guaranteed nor impossible.
        /// </summary>
        public double CalculateSuccessChance(ICombatant fleeingCombatant, IEnumerable<ICombatant> hostiles, int dexterity)
        {
            var livingHostiles = hostiles.Where(h => !h.IsDefeated).ToList();
            if (livingHostiles.Count == 0)
            {
                return MaxSuccessChance;
            }

            var nearestDistance = livingHostiles
                .Min(h => GridCoordinate.ManhattanDistance(fleeingCombatant.Position, h.Position));

            var distanceFactor = Math.Min(1.0, (double)nearestDistance / DistanceForMaxChance);
            var dexterityFactor = Math.Min(1.0, dexterity / 20.0);

            var chance = MinSuccessChance + (MaxSuccessChance - MinSuccessChance) * (0.7 * distanceFactor + 0.3 * dexterityFactor);
            return Math.Clamp(chance, MinSuccessChance, MaxSuccessChance);
        }

        /// <summary>
        /// Spends the fleeing combatant's action; on success (roll &lt; successChance)
        /// transitions a player-side CombatEncounter to PlayerFled. Returns whether
        /// the attempt succeeded.
        /// </summary>
        public bool TryFlee(ICombatant fleeingCombatant, double successChance)
        {
            _turnResourceManager.ConsumeAction(fleeingCombatant);

            var succeeded = _randomRollProvider() < successChance;

            if (succeeded && _encounter.IsPlayerSide(fleeingCombatant))
            {
                _encounter.MarkPlayerFled();
            }

            return succeeded;
        }
    }
}
