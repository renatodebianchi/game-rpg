using GameRpg.Characters;
using GameRpg.World;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class VisualCharacteristicsTests
    {
        private static Character CreateCharacter() =>
            new Character("player", maxHitPoints: 20, maxTechPoints: 3, new CharacterAttributes());

        private static EquipmentKitDefinition[] CreateKits()
        {
            var sword = ResourceDefinition.CreateForTesting("sword_basic", "Espada básica", isEssential: false);
            return new[] { EquipmentKitDefinition.CreateForTesting(CharacterOrientation.Combatant, new[] { (sword, 1) }) };
        }

        private static CharacterCreationProfile CreateFinalizableProfile()
        {
            var profile = new CharacterCreationProfile { Orientation = CharacterOrientation.Combatant };
            profile.AttributeAllocation.TryChangeAttribute(AttributeKind.Strength, 15);
            profile.AttributeAllocation.TryChangeAttribute(AttributeKind.Dexterity, 15);
            return profile;
        }

        [Test]
        public void Finalize_AppliesExplicitlyChosenVisualCharacteristics()
        {
            var profile = CreateFinalizableProfile();
            profile.VisualCharacteristics = new VisualCharacteristics
            {
                BodyType = BodyType.Sturdy,
                SkinTone = SkinTone.Dark,
                HairStyle = HairStyle.Bald,
            };
            var character = CreateCharacter();

            profile.Finalize(character, CreateKits());

            Assert.AreEqual(BodyType.Sturdy, character.Visuals.BodyType);
            Assert.AreEqual(SkinTone.Dark, character.Visuals.SkinTone);
            Assert.AreEqual(HairStyle.Bald, character.Visuals.HairStyle);
        }

        [Test]
        public void Finalize_WithoutExplicitChoice_AppliesDocumentedDefaults()
        {
            // Per FR-007: an unmodified profile still finalizes successfully,
            // using VisualCharacteristics.Default rather than blocking.
            var profile = CreateFinalizableProfile();
            var character = CreateCharacter();

            profile.Finalize(character, CreateKits());

            var defaults = VisualCharacteristics.Default;
            Assert.AreEqual(defaults.BodyType, character.Visuals.BodyType);
            Assert.AreEqual(defaults.SkinTone, character.Visuals.SkinTone);
            Assert.AreEqual(defaults.HairStyle, character.Visuals.HairStyle);
        }
    }
}
