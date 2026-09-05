using System.Collections;
using GameRpg.Characters;
using GameRpg.Core;
using GameRpg.Demo;
using GameRpg.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameRpg.Tests.PlayMode
{
    public class CharacterCreationToExplorationFlowTests
    {
        [TearDown]
        public void ClearPendingCharacterBetweenTests()
        {
            PendingPlayerCharacter.Consume();
        }

        [UnityTest]
        public IEnumerator ExplorationCharacterController_WithPendingCharacter_UsesItAndClearsThePending()
        {
            // Step 1-3 of contracts/scene-transition-contract.md: a fully finalized
            // Character (feature 002) is handed to the Exploration scene.
            var sword = ResourceDefinition.CreateForTesting("sword_basic", "Espada básica", isEssential: false);
            var kits = new[] { EquipmentKitDefinition.CreateForTesting(CharacterOrientation.Combatant, new[] { (sword, 1) }) };

            var profile = new CharacterCreationProfile { Orientation = CharacterOrientation.Combatant };
            profile.AttributeAllocation.TryChangeAttribute(AttributeKind.Strength, 15);
            profile.AttributeAllocation.TryChangeAttribute(AttributeKind.Dexterity, 15);
            profile.VisualCharacteristics = new VisualCharacteristics { BodyType = BodyType.Sturdy, SkinTone = SkinTone.Dark };

            var finalizedCharacter = new Character("player", maxHitPoints: 20, maxTechPoints: 3, new CharacterAttributes());
            profile.Finalize(finalizedCharacter, kits);

            PendingPlayerCharacter.Set(finalizedCharacter);

            var controllerGameObject = new GameObject("ExplorationCharacterController");
            var controller = controllerGameObject.AddComponent<ExplorationCharacterController>();

            yield return null; // let Start() run.

            Assert.AreSame(finalizedCharacter, controller.Character);
            Assert.AreEqual(15, controller.Character.Attributes.Strength);
            Assert.AreEqual(1, controller.Character.Inventory.GetQuantity("sword_basic"));
            Assert.AreEqual(BodyType.Sturdy, controller.Character.Visuals.BodyType);
            Assert.IsNull(PendingPlayerCharacter.Character, "PendingPlayerCharacter must be cleared once consumed.");

            Object.Destroy(controllerGameObject);
        }

        [UnityTest]
        public IEnumerator ExplorationCharacterController_WithoutAPendingCharacter_CreatesADefaultCharacter()
        {
            // FR-004: the scene opened directly (no prior creation flow) still
            // produces a usable character with the documented visual defaults.
            var controllerGameObject = new GameObject("ExplorationCharacterController");
            var controller = controllerGameObject.AddComponent<ExplorationCharacterController>();

            yield return null;

            Assert.IsNotNull(controller.Character);
            Assert.AreEqual(VisualCharacteristics.Default.BodyType, controller.Character.Visuals.BodyType);

            Object.Destroy(controllerGameObject);
        }
    }
}
