using System;
using GameRpg.Characters;
using GameRpg.World;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class CharacterCreationFinalizationTests
    {
        private static Character CreateCharacter() =>
            new Character("player", maxHitPoints: 20, maxMovementPoints: 3, new CharacterAttributes());

        private static EquipmentKitDefinition[] CreateKits()
        {
            var sword = ResourceDefinition.CreateForTesting("sword_basic", "Espada básica", isEssential: false);
            var staff = ResourceDefinition.CreateForTesting("staff_basic", "Cajado básico", isEssential: false);

            return new[]
            {
                EquipmentKitDefinition.CreateForTesting(
                    CharacterOrientation.Combatant, new[] { (sword, 1) }),
                EquipmentKitDefinition.CreateForTesting(
                    CharacterOrientation.Arcanist, new[] { (staff, 1) }),
            };
        }

        [Test]
        public void Finalize_WithUnspentPoints_Throws()
        {
            var profile = new CharacterCreationProfile { Orientation = CharacterOrientation.Combatant };
            var character = CreateCharacter();

            Assert.Throws<InvalidOperationException>(() => profile.Finalize(character, CreateKits()));
        }

        [Test]
        public void Finalize_WithoutOrientation_Throws()
        {
            var profile = new CharacterCreationProfile();
            SpendFullBudget(profile);
            var character = CreateCharacter();

            Assert.Throws<InvalidOperationException>(() => profile.Finalize(character, CreateKits()));
        }

        [Test]
        public void Finalize_WithFullBudgetSpentAndOrientationChosen_AppliesAttributesToCharacter()
        {
            var profile = new CharacterCreationProfile { Orientation = CharacterOrientation.Combatant };
            profile.AttributeAllocation.TryChangeAttribute(AttributeKind.Strength, 15);
            profile.AttributeAllocation.TryChangeAttribute(AttributeKind.Dexterity, 15);
            var character = CreateCharacter();

            profile.Finalize(character, CreateKits());

            Assert.AreEqual(15, character.Attributes.Strength);
            Assert.AreEqual(15, character.Attributes.Dexterity);
        }

        private static void SpendFullBudget(CharacterCreationProfile profile)
        {
            profile.AttributeAllocation.TryChangeAttribute(AttributeKind.Strength, 15);
            profile.AttributeAllocation.TryChangeAttribute(AttributeKind.Dexterity, 15);
        }
    }
}
