using GameRpg.Characters;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class AttributeAllocationStateTests
    {
        [Test]
        public void NewState_StartsAllAttributesAt8WithFullBudget()
        {
            var state = new AttributeAllocationState();

            Assert.AreEqual(8, state.GetScore(AttributeKind.Strength));
            Assert.AreEqual(8, state.GetScore(AttributeKind.Dexterity));
            Assert.AreEqual(8, state.GetScore(AttributeKind.Intellect));
            Assert.AreEqual(8, state.GetScore(AttributeKind.Willpower));
            Assert.AreEqual(AttributeAllocationState.TotalBudget, state.PointsRemaining);
        }

        [Test]
        public void TryChangeAttribute_ValidIncrease_SpendsCorrectPoints()
        {
            var state = new AttributeAllocationState();

            var accepted = state.TryChangeAttribute(AttributeKind.Strength, 10);

            Assert.IsTrue(accepted);
            Assert.AreEqual(10, state.GetScore(AttributeKind.Strength));
            Assert.AreEqual(AttributeAllocationState.TotalBudget - 2, state.PointsRemaining);
        }

        [TestCase(7)]
        [TestCase(16)]
        public void TryChangeAttribute_OutOfRange_IsRejected(int requestedScore)
        {
            var state = new AttributeAllocationState();

            var accepted = state.TryChangeAttribute(AttributeKind.Strength, requestedScore);

            Assert.IsFalse(accepted);
            Assert.AreEqual(8, state.GetScore(AttributeKind.Strength));
        }

        [Test]
        public void TryChangeAttribute_ExceedingBudget_IsRejected()
        {
            var state = new AttributeAllocationState();
            // Spend most of the budget on Strength (15 costs 9 points), leaving 9.
            state.TryChangeAttribute(AttributeKind.Strength, 15);

            // Raising Dexterity to 15 would cost 9 more, totaling 18 — exactly the budget, should succeed.
            var accepted = state.TryChangeAttribute(AttributeKind.Dexterity, 15);
            Assert.IsTrue(accepted);
            Assert.AreEqual(0, state.PointsRemaining);

            // Any further increase should now be rejected.
            var overBudget = state.TryChangeAttribute(AttributeKind.Intellect, 9);
            Assert.IsFalse(overBudget);
            Assert.AreEqual(8, state.GetScore(AttributeKind.Intellect));
        }

        [Test]
        public void TryChangeAttribute_LoweringScore_RefundsPoints()
        {
            var state = new AttributeAllocationState();
            state.TryChangeAttribute(AttributeKind.Strength, 12); // costs 4
            var remainingAfterRaise = state.PointsRemaining;

            var accepted = state.TryChangeAttribute(AttributeKind.Strength, 8);

            Assert.IsTrue(accepted);
            Assert.AreEqual(8, state.GetScore(AttributeKind.Strength));
            Assert.AreEqual(remainingAfterRaise + 4, state.PointsRemaining);
        }

        [Test]
        public void ToCharacterAttributes_ReflectsCurrentScores()
        {
            var state = new AttributeAllocationState();
            state.TryChangeAttribute(AttributeKind.Strength, 12);
            state.TryChangeAttribute(AttributeKind.Dexterity, 10);

            var attributes = state.ToCharacterAttributes();

            Assert.AreEqual(12, attributes.Strength);
            Assert.AreEqual(10, attributes.Dexterity);
            Assert.AreEqual(8, attributes.Intellect);
            Assert.AreEqual(8, attributes.Willpower);
        }
    }
}
