using GameRpg.Characters;
using GameRpg.Skills;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class SkillNodePrerequisiteTests
    {
        private static Character CreateCharacter(int skillPoints = 10)
        {
            var character = new Character("player", maxHitPoints: 10, maxMovementPoints: 4, new CharacterAttributes());
            character.GrantSkillPoints(skillPoints);
            return character;
        }

        [Test]
        public void NodeWithoutPrerequisites_IsAvailableImmediately()
        {
            var character = CreateCharacter();
            var node = SkillNodeDefinition.CreateForTesting("root", SkillTrack.Combatant);
            var service = new SkillTreeService(new[] { node });

            Assert.IsTrue(service.IsAvailableForInvestment(character, node));
        }

        [Test]
        public void NodeWithUnmetPrerequisite_IsNotAvailable()
        {
            var character = CreateCharacter();
            var root = SkillNodeDefinition.CreateForTesting("root", SkillTrack.Combatant);
            var child = SkillNodeDefinition.CreateForTesting("child", SkillTrack.Combatant, new[] { root });
            var service = new SkillTreeService(new[] { root, child });

            Assert.IsFalse(service.IsAvailableForInvestment(character, child));
        }

        [Test]
        public void NodeWithMetPrerequisite_IsAvailable()
        {
            var character = CreateCharacter();
            var root = SkillNodeDefinition.CreateForTesting("root", SkillTrack.Combatant);
            var child = SkillNodeDefinition.CreateForTesting("child", SkillTrack.Combatant, new[] { root });
            var service = new SkillTreeService(new[] { root, child });

            service.AcquireNode(character, root);

            Assert.IsTrue(service.IsAvailableForInvestment(character, child));
        }

        [Test]
        public void HybridNode_RequiringBothTracks_IsUnavailableWithOnlyOneTrackSatisfied()
        {
            var character = CreateCharacter();
            var combatantRoot = SkillNodeDefinition.CreateForTesting("combatant-root", SkillTrack.Combatant);
            var arcanistRoot = SkillNodeDefinition.CreateForTesting("arcanist-root", SkillTrack.Arcanist);
            var hybrid = SkillNodeDefinition.CreateForTesting(
                "hybrid", SkillTrack.Hybrid, new[] { combatantRoot, arcanistRoot });
            var service = new SkillTreeService(new[] { combatantRoot, arcanistRoot, hybrid });

            service.AcquireNode(character, combatantRoot);

            Assert.IsFalse(service.IsAvailableForInvestment(character, hybrid));
        }

        [Test]
        public void HybridNode_RequiringBothTracks_IsAvailableOnceBothSatisfied()
        {
            var character = CreateCharacter();
            var combatantRoot = SkillNodeDefinition.CreateForTesting("combatant-root", SkillTrack.Combatant);
            var arcanistRoot = SkillNodeDefinition.CreateForTesting("arcanist-root", SkillTrack.Arcanist);
            var hybrid = SkillNodeDefinition.CreateForTesting(
                "hybrid", SkillTrack.Hybrid, new[] { combatantRoot, arcanistRoot });
            var service = new SkillTreeService(new[] { combatantRoot, arcanistRoot, hybrid });

            service.AcquireNode(character, combatantRoot);
            service.AcquireNode(character, arcanistRoot);

            Assert.IsTrue(service.IsAvailableForInvestment(character, hybrid));
        }
    }
}
