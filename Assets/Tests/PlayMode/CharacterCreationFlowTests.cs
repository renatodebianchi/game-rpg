using GameRpg.Characters;
using GameRpg.World;
using NUnit.Framework;

namespace GameRpg.Tests.PlayMode
{
    public class CharacterCreationFlowTests
    {
        [Test]
        public void FullCreationFlow_AllocateOrientAppearanceFinalize_ProducesExpectedCharacter()
        {
            var sword = ResourceDefinition.CreateForTesting("sword_basic", "Espada básica", isEssential: false);
            var potion = ResourceDefinition.CreateForTesting("healing_potion", "Poção de cura", isEssential: false);
            var kits = new[]
            {
                EquipmentKitDefinition.CreateForTesting(
                    CharacterOrientation.Combatant, new[] { (sword, 1), (potion, 2) }),
                EquipmentKitDefinition.CreateForTesting(
                    CharacterOrientation.Arcanist, new[] { (potion, 3) }),
            };

            // Step 1: allocate the full 18-point Point Buy budget.
            // Cost: 15->9, 15->9, 8->0, 8->0 = 18 total.
            var profile = new CharacterCreationProfile();
            profile.AttributeAllocation.TryChangeAttribute(AttributeKind.Strength, 15);
            profile.AttributeAllocation.TryChangeAttribute(AttributeKind.Dexterity, 15);
            Assert.AreEqual(0, profile.AttributeAllocation.PointsRemaining);

            // Step 2: choose orientation.
            profile.Orientation = CharacterOrientation.Combatant;

            // Step 3: choose appearance.
            profile.VisualCharacteristics = new VisualCharacteristics
            {
                BodyType = BodyType.Sturdy,
                SkinTone = SkinTone.Light,
                HairStyle = HairStyle.Long,
            };

            // Finalize.
            var character = new Character("player", maxHitPoints: 20, maxMovementPoints: 3, new CharacterAttributes());
            profile.Finalize(character, kits);

            Assert.AreEqual(15, character.Attributes.Strength);
            Assert.AreEqual(15, character.Attributes.Dexterity);
            Assert.AreEqual(8, character.Attributes.Intellect);
            Assert.AreEqual(8, character.Attributes.Willpower);

            Assert.AreEqual(1, character.Inventory.GetQuantity("sword_basic"));
            Assert.AreEqual(2, character.Inventory.GetQuantity("healing_potion"));

            Assert.AreEqual(BodyType.Sturdy, character.Visuals.BodyType);
            Assert.AreEqual(SkinTone.Light, character.Visuals.SkinTone);
            Assert.AreEqual(HairStyle.Long, character.Visuals.HairStyle);
        }
    }
}
