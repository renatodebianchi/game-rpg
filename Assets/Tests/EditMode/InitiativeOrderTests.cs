using System.Collections.Generic;
using GameRpg.Combat;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class InitiativeOrderTests
    {
        [Test]
        public void CalculateOrder_SortsByScoreDescending()
        {
            var slow = new NonPlayerCombatant("slow", 10, 4);
            var fast = new NonPlayerCombatant("fast", 10, 4);
            var medium = new NonPlayerCombatant("medium", 10, 4);

            var scores = new Dictionary<ICombatant, int>
            {
                [slow] = 5,
                [fast] = 20,
                [medium] = 12,
            };

            var service = new InitiativeService(c => scores[c]);
            var order = service.CalculateOrder(new ICombatant[] { slow, fast, medium });

            Assert.AreEqual(new[] { fast, medium, slow }, order);
        }

        [Test]
        public void CalculateOrder_TiedScores_PreservesOriginalOrderAsTieBreak()
        {
            var first = new NonPlayerCombatant("first", 10, 4);
            var second = new NonPlayerCombatant("second", 10, 4);

            var service = new InitiativeService(_ => 10);
            var order = service.CalculateOrder(new ICombatant[] { first, second });

            Assert.AreEqual(new[] { first, second }, order);
        }

        [Test]
        public void CalculateOrder_WithoutScoreProvider_PreservesInsertionOrder()
        {
            var a = new NonPlayerCombatant("a", 10, 4);
            var b = new NonPlayerCombatant("b", 10, 4);

            var service = new InitiativeService();
            var order = service.CalculateOrder(new ICombatant[] { a, b });

            Assert.AreEqual(new[] { a, b }, order);
        }
    }
}
