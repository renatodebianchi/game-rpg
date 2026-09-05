using System;
using GameRpg.Characters;
using GameRpg.Core;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class HungerSystemTests
    {
        private static Character CreateCharacter() =>
            new Character("player", maxHitPoints: 10, maxTechPoints: 4, new CharacterAttributes());

        [Test]
        public void AdvancingTime_IncreasesHunger()
        {
            var character = CreateCharacter();
            var config = BalancingConfig.CreateForTesting(hungerIncreasePerSimulatedHour: 2f);
            var hunger = new HungerSystem(character, config, worldClock: null);

            hunger.AdvanceByElapsedTime(TimeSpan.FromHours(3));

            Assert.AreEqual(6f, character.Hunger, 0.001f);
        }

        [Test]
        public void CrossingCriticalThreshold_AppliesDamagePenalty()
        {
            var character = CreateCharacter();
            var config = BalancingConfig.CreateForTesting(
                hungerAlertThreshold: 60f, hungerCriticalThreshold: 85f, hungerIncreasePerSimulatedHour: 100f);
            var hunger = new HungerSystem(character, config, worldClock: null);

            hunger.AdvanceByElapsedTime(TimeSpan.FromHours(1)); // hunger -> 100, well past critical

            Assert.AreEqual(SurvivalThresholdLevel.Critical, hunger.CurrentLevel);
            Assert.AreEqual(5, hunger.ModifyOutgoingDamage(character, 10));
        }

        [Test]
        public void Feeding_RestoresHungerAndRemovesPenalty()
        {
            var character = CreateCharacter();
            var config = BalancingConfig.CreateForTesting(hungerCriticalThreshold: 85f, hungerIncreasePerSimulatedHour: 100f);
            var hunger = new HungerSystem(character, config, worldClock: null);
            hunger.AdvanceByElapsedTime(TimeSpan.FromHours(1));
            Assert.AreEqual(SurvivalThresholdLevel.Critical, hunger.CurrentLevel);

            hunger.Feed(100f);

            Assert.AreEqual(0f, character.Hunger, 0.001f);
            Assert.AreEqual(SurvivalThresholdLevel.Normal, hunger.CurrentLevel);
            Assert.AreEqual(10, hunger.ModifyOutgoingDamage(character, 10));
        }

        [Test]
        public void ModifyOutgoingDamage_DoesNotAffectOtherCombatants()
        {
            var character = CreateCharacter();
            var otherCharacter = CreateCharacter();
            var config = BalancingConfig.CreateForTesting(hungerCriticalThreshold: 85f, hungerIncreasePerSimulatedHour: 100f);
            var hunger = new HungerSystem(character, config, worldClock: null);
            hunger.AdvanceByElapsedTime(TimeSpan.FromHours(1));

            Assert.AreEqual(10, hunger.ModifyOutgoingDamage(otherCharacter, 10));
        }
    }
}
