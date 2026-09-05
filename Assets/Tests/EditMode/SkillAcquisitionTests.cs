using System;
using GameRpg.Characters;
using GameRpg.Skills;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class SkillAcquisitionTests
    {
        private static Character CreateCharacter(int skillPoints = 10)
        {
            var character = new Character("player", maxHitPoints: 10, maxTechPoints: 4, new CharacterAttributes());
            character.GrantSkillPoints(skillPoints);
            return character;
        }

        [Test]
        public void AcquireNode_SpendsPointsAndMarksNodeAsAcquired()
        {
            var character = CreateCharacter(skillPoints: 5);
            var node = SkillNodeDefinition.CreateForTesting("root", SkillTrack.Combatant, cost: 2);
            var service = new SkillTreeService(new[] { node });

            service.AcquireNode(character, node);

            Assert.AreEqual(3, character.AvailableSkillPoints);
            CollectionAssert.Contains(character.AcquiredSkillNodeIds, "root");
        }

        [Test]
        public void AcquireNode_AlreadyAcquired_Throws()
        {
            var character = CreateCharacter();
            var node = SkillNodeDefinition.CreateForTesting("root", SkillTrack.Combatant);
            var service = new SkillTreeService(new[] { node });

            service.AcquireNode(character, node);

            Assert.Throws<InvalidOperationException>(() => service.AcquireNode(character, node));
        }

        [Test]
        public void AcquireNode_NotEnoughSkillPoints_Throws()
        {
            var character = CreateCharacter(skillPoints: 1);
            var node = SkillNodeDefinition.CreateForTesting("root", SkillTrack.Combatant, cost: 5);
            var service = new SkillTreeService(new[] { node });

            Assert.Throws<InvalidOperationException>(() => service.AcquireNode(character, node));
        }
    }
}
