using System;
using GameRpg.Characters;
using GameRpg.Skills;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class SkillRespecTests
    {
        private static Character CreateCharacter(int skillPoints = 10)
        {
            var character = new Character("player", maxHitPoints: 10, maxMovementPoints: 4, new CharacterAttributes());
            character.GrantSkillPoints(skillPoints);
            return character;
        }

        [Test]
        public void Respec_RefundsPointsAndAllowsReinvestment()
        {
            var character = CreateCharacter(skillPoints: 5);
            var node = SkillNodeDefinition.CreateForTesting("root", SkillTrack.Combatant, cost: 3);
            var service = new SkillTreeService(new[] { node });

            service.AcquireNode(character, node);
            service.Respec(character, node);

            Assert.AreEqual(5, character.AvailableSkillPoints);
            CollectionAssert.DoesNotContain(character.AcquiredSkillNodeIds, "root");

            // Free to reinvest afterwards (FR-018: "livre e sem custo, a qualquer momento").
            Assert.DoesNotThrow(() => service.AcquireNode(character, node));
        }

        [Test]
        public void Respec_OfNodeNotAcquired_Throws()
        {
            var character = CreateCharacter();
            var node = SkillNodeDefinition.CreateForTesting("root", SkillTrack.Combatant);
            var service = new SkillTreeService(new[] { node });

            Assert.Throws<InvalidOperationException>(() => service.Respec(character, node));
        }

        [Test]
        public void Respec_OfPrerequisite_CascadesToRemoveDependentNode()
        {
            var character = CreateCharacter(skillPoints: 10);
            var root = SkillNodeDefinition.CreateForTesting("root", SkillTrack.Combatant, cost: 2);
            var child = SkillNodeDefinition.CreateForTesting("child", SkillTrack.Combatant, new[] { root }, cost: 3);
            var service = new SkillTreeService(new[] { root, child });

            service.AcquireNode(character, root);
            service.AcquireNode(character, child);

            service.Respec(character, root);

            CollectionAssert.DoesNotContain(character.AcquiredSkillNodeIds, "root");
            CollectionAssert.DoesNotContain(character.AcquiredSkillNodeIds, "child");
            Assert.AreEqual(10, character.AvailableSkillPoints);
        }

        [Test]
        public void Respec_OfPrerequisite_CascadesThroughMultipleLevels()
        {
            var character = CreateCharacter(skillPoints: 10);
            var root = SkillNodeDefinition.CreateForTesting("root", SkillTrack.Combatant, cost: 1);
            var middle = SkillNodeDefinition.CreateForTesting("middle", SkillTrack.Combatant, new[] { root }, cost: 1);
            var leaf = SkillNodeDefinition.CreateForTesting("leaf", SkillTrack.Combatant, new[] { middle }, cost: 1);
            var service = new SkillTreeService(new[] { root, middle, leaf });

            service.AcquireNode(character, root);
            service.AcquireNode(character, middle);
            service.AcquireNode(character, leaf);

            service.Respec(character, root);

            CollectionAssert.IsEmpty(character.AcquiredSkillNodeIds);
            Assert.AreEqual(10, character.AvailableSkillPoints);
        }
    }
}
