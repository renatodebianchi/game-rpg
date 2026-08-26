using System;
using System.IO;
using GameRpg.Characters;
using GameRpg.Core;
using GameRpg.Skills;
using GameRpg.World;
using NUnit.Framework;

namespace GameRpg.Tests.PlayMode
{
    public class SaveLoadRoundTripTests
    {
        private string _tempSaveDirectory;

        [SetUp]
        public void SetUp()
        {
            _tempSaveDirectory = Path.Combine(Path.GetTempPath(), "GameRpgSaveTests_" + Guid.NewGuid());
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempSaveDirectory))
            {
                Directory.Delete(_tempSaveDirectory, recursive: true);
            }
        }

        [Test]
        public void SaveThenLoad_ProducesObservationallyIdenticalState()
        {
            var saveSystem = new SaveSystem(_tempSaveDirectory);

            var character = new Character("player", maxHitPoints: 20, maxMovementPoints: 4,
                new CharacterAttributes(strength: 5, dexterity: 4, intellect: 3, willpower: 2));
            character.ApplyDamage(7);
            character.GrantSkillPoints(5);
            var node = SkillNodeDefinition.CreateForTesting("root", SkillTrack.Combatant, cost: 2);
            var skillTreeService = new SkillTreeService(new[] { node });
            skillTreeService.AcquireNode(character, node);
            character.Hunger = 42f;
            character.Sanity = 63f;
            character.Inventory.Add("food", 3);
            character.Visuals = new VisualCharacteristics
            {
                BodyType = BodyType.Sturdy,
                SkinTone = SkinTone.Dark,
                HairStyle = HairStyle.Bald,
                HairColor = new UnityEngine.Color(0.9f, 0.1f, 0.1f),
            };

            var worldClock = new WorldClock();
            worldClock.Advance(TimeSpan.FromHours(11));

            var villageA = new Community("village_a", new[] { "npc1", "npc2" });
            villageA.AddResourceStock("food", 25f);
            var log = new ImpactfulChoiceLog();
            var reputationService = new ReputationService(new[] { villageA }, log);
            log.Record(ImpactfulChoiceType.SaveNpc, "village_a", worldClock.ElapsedSimulatedTime, relatedNpcId: "npc1");

            // Save.
            var saveData = saveSystem.CaptureGameState(character, new[] { villageA }, worldClock);
            saveSystem.SaveToDisk(saveData);

            // Fresh, blank objects representing a newly loaded session.
            var loadedCharacter = new Character("player", maxHitPoints: 20, maxMovementPoints: 4, new CharacterAttributes());
            var loadedWorldClock = new WorldClock();
            var loadedVillageA = new Community("village_a", Array.Empty<string>());

            Assert.IsTrue(saveSystem.TryLoadFromDisk(out var loadedData));
            saveSystem.ApplyGameState(
                loadedData, loadedCharacter, new System.Collections.Generic.Dictionary<string, Community> { ["village_a"] = loadedVillageA }, loadedWorldClock);

            // Character state matches.
            Assert.AreEqual(character.CurrentHitPoints, loadedCharacter.CurrentHitPoints);
            Assert.AreEqual(character.AvailableSkillPoints, loadedCharacter.AvailableSkillPoints);
            CollectionAssert.AreEquivalent(character.AcquiredSkillNodeIds, loadedCharacter.AcquiredSkillNodeIds);
            Assert.AreEqual(character.Hunger, loadedCharacter.Hunger, 0.001f);
            Assert.AreEqual(character.Sanity, loadedCharacter.Sanity, 0.001f);
            Assert.AreEqual(character.Inventory.GetQuantity("food"), loadedCharacter.Inventory.GetQuantity("food"));
            Assert.AreEqual(character.Visuals.BodyType, loadedCharacter.Visuals.BodyType);
            Assert.AreEqual(character.Visuals.SkinTone, loadedCharacter.Visuals.SkinTone);
            Assert.AreEqual(character.Visuals.HairStyle, loadedCharacter.Visuals.HairStyle);
            Assert.AreEqual(character.Visuals.HairColor, loadedCharacter.Visuals.HairColor);

            // World clock matches.
            Assert.AreEqual(worldClock.ElapsedSimulatedTime, loadedWorldClock.ElapsedSimulatedTime);

            // Community state matches.
            Assert.AreEqual(villageA.ReputationWithPlayer, loadedVillageA.ReputationWithPlayer);
            Assert.AreEqual(villageA.GetResourceStock("food"), loadedVillageA.GetResourceStock("food"), 0.001f);
            CollectionAssert.AreEquivalent(villageA.PopulationNpcIds, loadedVillageA.PopulationNpcIds);
            Assert.AreEqual(villageA.IsPermanentlyInactive, loadedVillageA.IsPermanentlyInactive);
        }
    }
}
