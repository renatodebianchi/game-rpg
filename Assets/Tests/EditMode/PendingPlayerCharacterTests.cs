using GameRpg.Characters;
using GameRpg.Core;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class PendingPlayerCharacterTests
    {
        private static Character CreateCharacter(string id) =>
            new Character(id, maxHitPoints: 20, maxTechPoints: 3, new CharacterAttributes(8, 8, 8, 8));

        [TearDown]
        public void ClearPendingCharacterBetweenTests()
        {
            // Static state (contracts/scene-transition-contract.md) must not leak
            // between tests.
            PendingPlayerCharacter.Consume();
        }

        [Test]
        public void Consume_AfterSet_ReturnsTheSameCharacterAndThenClears()
        {
            var character = CreateCharacter("player");
            PendingPlayerCharacter.Set(character);

            var consumed = PendingPlayerCharacter.Consume();

            Assert.AreSame(character, consumed);
            Assert.IsNull(PendingPlayerCharacter.Character);
        }

        [Test]
        public void Consume_WithoutASetCharacter_ReturnsNull()
        {
            // FR-004 / contract rule 2: a direct scene visit must never reuse a
            // character from an earlier session.
            Assert.IsNull(PendingPlayerCharacter.Consume());
        }

        [Test]
        public void Consume_CalledTwice_OnlyReturnsTheCharacterOnce()
        {
            PendingPlayerCharacter.Set(CreateCharacter("player"));

            PendingPlayerCharacter.Consume();
            var secondConsume = PendingPlayerCharacter.Consume();

            Assert.IsNull(secondConsume);
        }
    }
}
