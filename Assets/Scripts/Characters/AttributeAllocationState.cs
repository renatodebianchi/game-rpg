using System.Collections.Generic;
using System.Linq;

namespace GameRpg.Characters
{
    /// <summary>Which of the four base attributes a Point Buy change applies to.</summary>
    public enum AttributeKind
    {
        Strength,
        Dexterity,
        Intellect,
        Willpower
    }

    /// <summary>
    /// Mutable Point Buy state held during character creation (FR-002, FR-003;
    /// see contracts/point-buy-contract.md). Rebalanced budget for this game's
    /// 4-attribute model: 18 points instead of D&D 5e's official 27 for 6
    /// attributes (27 * 4/6 = 18 — see spec.md Assumptions), using the same
    /// per-score cost curve (PointBuyCostTable).
    /// </summary>
    public class AttributeAllocationState
    {
        public const int TotalBudget = 18;

        private readonly Dictionary<AttributeKind, int> _scores = new Dictionary<AttributeKind, int>
        {
            [AttributeKind.Strength] = PointBuyCostTable.MinScore,
            [AttributeKind.Dexterity] = PointBuyCostTable.MinScore,
            [AttributeKind.Intellect] = PointBuyCostTable.MinScore,
            [AttributeKind.Willpower] = PointBuyCostTable.MinScore,
        };

        public int GetScore(AttributeKind attribute) => _scores[attribute];

        public int PointsSpent => _scores.Values.Sum(PointBuyCostTable.GetCumulativeCost);

        public int PointsRemaining => TotalBudget - PointsSpent;

        /// <summary>
        /// Attempts to change one attribute's score. Rejects (returns false,
        /// state unchanged) if the requested score is outside 8-15, or if it
        /// would spend more than the total budget. Lowering a score is always
        /// allowed when it doesn't itself go out of range, letting the player
        /// reclaim points before finalizing (contracts/point-buy-contract.md,
        /// rules 1-3).
        /// </summary>
        public bool TryChangeAttribute(AttributeKind attribute, int requestedScore)
        {
            if (!PointBuyCostTable.IsValidScore(requestedScore))
            {
                return false;
            }

            var previousScore = _scores[attribute];
            var costDelta = PointBuyCostTable.GetCumulativeCost(requestedScore) - PointBuyCostTable.GetCumulativeCost(previousScore);

            if (PointsSpent + costDelta > TotalBudget)
            {
                return false;
            }

            _scores[attribute] = requestedScore;
            return true;
        }

        public CharacterAttributes ToCharacterAttributes() => new CharacterAttributes(
            _scores[AttributeKind.Strength],
            _scores[AttributeKind.Dexterity],
            _scores[AttributeKind.Intellect],
            _scores[AttributeKind.Willpower]);
    }
}
