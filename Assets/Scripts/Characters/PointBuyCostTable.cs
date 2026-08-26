using System;
using System.Collections.Generic;

namespace GameRpg.Characters
{
    /// <summary>
    /// Cumulative Point Buy cost to raise an attribute from the base value of
    /// 8 up to each score between 8 and 15, following the D&D 5e cost curve
    /// (data-model.md, "PointBuyCostTable"). This game's total budget (18
    /// points instead of the official 27) is defined separately by
    /// AttributeAllocationState — this table only defines the per-score cost.
    /// </summary>
    public static class PointBuyCostTable
    {
        public const int MinScore = 8;
        public const int MaxScore = 15;

        private static readonly Dictionary<int, int> CumulativeCostByScore = new Dictionary<int, int>
        {
            [8] = 0,
            [9] = 1,
            [10] = 2,
            [11] = 3,
            [12] = 4,
            [13] = 5,
            [14] = 7,
            [15] = 9,
        };

        /// <summary>Cumulative points spent to reach <paramref name="score"/> from the base value of 8.</summary>
        public static int GetCumulativeCost(int score)
        {
            if (!CumulativeCostByScore.TryGetValue(score, out var cost))
            {
                throw new ArgumentOutOfRangeException(nameof(score), $"Score must be between {MinScore} and {MaxScore}.");
            }

            return cost;
        }

        public static bool IsValidScore(int score) => score >= MinScore && score <= MaxScore;
    }
}
