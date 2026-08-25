using System.Collections.Generic;
using System.Linq;

namespace GameRpg.Combat
{
    /// <summary>
    /// Computes turn order at the start of a combat encounter (FR-001).
    /// Deterministic given a per-combatant initiative score, so it stays
    /// EditMode-testable without any randomness dependency.
    /// </summary>
    public class InitiativeService
    {
        private readonly System.Func<ICombatant, int> _initiativeScoreProvider;

        /// <param name="initiativeScoreProvider">
        /// Returns a combatant's initiative score; higher acts first. Defaults to
        /// a stable score of 0 for every combatant (order becomes insertion order)
        /// when not supplied, which keeps this class usable before combat-specific
        /// attribute-to-initiative formulas are decided.
        /// </param>
        public InitiativeService(System.Func<ICombatant, int> initiativeScoreProvider = null)
        {
            _initiativeScoreProvider = initiativeScoreProvider ?? (_ => 0);
        }

        public IReadOnlyList<ICombatant> CalculateOrder(IEnumerable<ICombatant> participants)
        {
            return participants
                .Select((combatant, index) => (combatant, score: _initiativeScoreProvider(combatant), index))
                .OrderByDescending(entry => entry.score)
                .ThenBy(entry => entry.index) // stable tie-break: original order
                .Select(entry => entry.combatant)
                .ToList();
        }
    }
}
