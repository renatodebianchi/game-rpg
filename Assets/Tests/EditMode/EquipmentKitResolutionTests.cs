using GameRpg.Characters;
using GameRpg.Core;
using GameRpg.World;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class EquipmentKitResolutionTests
    {
        private static Character CreateCharacter() =>
            new Character("player", maxHitPoints: 20, maxTechPoints: 3, new CharacterAttributes());

        private static CharacterCreationProfile CreateFinalizableProfile(CharacterOrientation orientation)
        {
            var profile = new CharacterCreationProfile { Orientation = orientation };
            profile.AttributeAllocation.TryChangeAttribute(AttributeKind.Strength, 15);
            profile.AttributeAllocation.TryChangeAttribute(AttributeKind.Dexterity, 15);
            return profile;
        }

        [Test]
        public void Finalize_AddsCombatantKitItems_WhenCombatantOrientationChosen()
        {
            var sword = ResourceDefinition.CreateForTesting("sword_basic", "Espada básica", isEssential: false);
            var staff = ResourceDefinition.CreateForTesting("staff_basic", "Cajado básico", isEssential: false);
            var kits = new[]
            {
                EquipmentKitDefinition.CreateForTesting(CharacterOrientation.Combatant, new[] { (sword, 1) }),
                EquipmentKitDefinition.CreateForTesting(CharacterOrientation.Arcanist, new[] { (staff, 1) }),
            };

            var character = CreateCharacter();
            var profile = CreateFinalizableProfile(CharacterOrientation.Combatant);

            profile.Finalize(character, kits);

            Assert.AreEqual(1, character.Inventory.GetQuantity("sword_basic"));
            Assert.AreEqual(0, character.Inventory.GetQuantity("staff_basic"));
        }

        [Test]
        public void Finalize_AddsArcanistKitItems_WhenArcanistOrientationChosen()
        {
            var sword = ResourceDefinition.CreateForTesting("sword_basic", "Espada básica", isEssential: false);
            var staff = ResourceDefinition.CreateForTesting("staff_basic", "Cajado básico", isEssential: false);
            var kits = new[]
            {
                EquipmentKitDefinition.CreateForTesting(CharacterOrientation.Combatant, new[] { (sword, 1) }),
                EquipmentKitDefinition.CreateForTesting(CharacterOrientation.Arcanist, new[] { (staff, 1) }),
            };

            var character = CreateCharacter();
            var profile = CreateFinalizableProfile(CharacterOrientation.Arcanist);

            profile.Finalize(character, kits);

            Assert.AreEqual(1, character.Inventory.GetQuantity("staff_basic"));
            Assert.AreEqual(0, character.Inventory.GetQuantity("sword_basic"));
        }

        [Test]
        public void Finalize_WithNoMatchingKit_ThrowsContentValidationException()
        {
            var sword = ResourceDefinition.CreateForTesting("sword_basic", "Espada básica", isEssential: false);
            var kits = new[]
            {
                EquipmentKitDefinition.CreateForTesting(CharacterOrientation.Combatant, new[] { (sword, 1) }),
            };

            var character = CreateCharacter();
            var profile = CreateFinalizableProfile(CharacterOrientation.Arcanist);

            Assert.Throws<ContentValidationException>(() => profile.Finalize(character, kits));
        }
    }
}
