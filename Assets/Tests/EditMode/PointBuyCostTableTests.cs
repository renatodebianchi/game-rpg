using GameRpg.Characters;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class PointBuyCostTableTests
    {
        [TestCase(8, 0)]
        [TestCase(9, 1)]
        [TestCase(10, 2)]
        [TestCase(11, 3)]
        [TestCase(12, 4)]
        [TestCase(13, 5)]
        [TestCase(14, 7)]
        [TestCase(15, 9)]
        public void GetCumulativeCost_MatchesDnd5eCurve(int score, int expectedCost)
        {
            Assert.AreEqual(expectedCost, PointBuyCostTable.GetCumulativeCost(score));
        }

        [TestCase(7, false)]
        [TestCase(8, true)]
        [TestCase(15, true)]
        [TestCase(16, false)]
        public void IsValidScore_RespectsRange(int score, bool expected)
        {
            Assert.AreEqual(expected, PointBuyCostTable.IsValidScore(score));
        }
    }
}
