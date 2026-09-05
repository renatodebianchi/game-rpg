using System;
using GameRpg.Combat;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class BattleArenaTests
    {
        [Test]
        public void Constructor_WithMaxNotGreaterThanMin_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BattleArena(5f, 5f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BattleArena(5f, 2f));
        }

        [Test]
        public void Clamp_PositionWithinBounds_ReturnsUnchanged()
        {
            var arena = new BattleArena(0f, 10f);
            Assert.AreEqual(4.5f, arena.Clamp(4.5f));
        }

        [Test]
        public void Clamp_PositionBelowMin_ReturnsMin()
        {
            var arena = new BattleArena(0f, 10f);
            Assert.AreEqual(0f, arena.Clamp(-3f));
        }

        [Test]
        public void Clamp_PositionAboveMax_ReturnsMax()
        {
            var arena = new BattleArena(0f, 10f);
            Assert.AreEqual(10f, arena.Clamp(15f));
        }

        [Test]
        public void IsWithinBounds_ReflectsClampBehavior()
        {
            var arena = new BattleArena(0f, 10f);
            Assert.IsTrue(arena.IsWithinBounds(0f));
            Assert.IsTrue(arena.IsWithinBounds(10f));
            Assert.IsFalse(arena.IsWithinBounds(-0.1f));
            Assert.IsFalse(arena.IsWithinBounds(10.1f));
        }
    }
}
