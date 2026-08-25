using System;
using GameRpg.Characters;
using GameRpg.Core;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class SanitySystemTests
    {
        private static Character CreateCharacter() =>
            new Character("player", maxHitPoints: 10, maxMovementPoints: 4, new CharacterAttributes());

        [Test]
        public void DisturbingEvent_ReducesSanity()
        {
            var character = CreateCharacter();
            var config = BalancingConfig.CreateForTesting();
            var sanity = new SanitySystem(character, config);

            sanity.ApplyDisturbingEvent(30f);

            Assert.AreEqual(70f, character.Sanity, 0.001f);
        }

        [Test]
        public void CrossingCriticalThreshold_AppliesDamagePenalty()
        {
            var character = CreateCharacter();
            var config = BalancingConfig.CreateForTesting(sanityAlertThreshold: 40f, sanityCriticalThreshold: 15f);
            var sanity = new SanitySystem(character, config);

            sanity.ApplyDisturbingEvent(90f); // 100 -> 10, below critical threshold of 15

            Assert.AreEqual(SurvivalThresholdLevel.Critical, sanity.CurrentLevel);
            Assert.AreEqual(5, sanity.ModifyOutgoingDamage(character, 10));
        }

        [Test]
        public void Recovering_RestoresSanityAndRemovesPenalty()
        {
            var character = CreateCharacter();
            var config = BalancingConfig.CreateForTesting(sanityCriticalThreshold: 15f);
            var sanity = new SanitySystem(character, config);
            sanity.ApplyDisturbingEvent(90f);
            Assert.AreEqual(SurvivalThresholdLevel.Critical, sanity.CurrentLevel);

            sanity.Recover(90f);

            Assert.AreEqual(SurvivalThresholdLevel.Normal, sanity.CurrentLevel);
            Assert.AreEqual(10, sanity.ModifyOutgoingDamage(character, 10));
        }

        [Test]
        public void HungerAndSanityBothCritical_PenaltiesStackCumulatively()
        {
            // FR-021: penalties from both systems apply together, without a
            // combined cap or either one taking priority over the other.
            var character = CreateCharacter();
            var hungerConfig = BalancingConfig.CreateForTesting(hungerCriticalThreshold: 85f, hungerIncreasePerSimulatedHour: 100f);
            var sanityConfig = BalancingConfig.CreateForTesting(sanityCriticalThreshold: 15f);

            var hunger = new HungerSystem(character, hungerConfig, worldClock: null);
            var sanity = new SanitySystem(character, sanityConfig);

            hunger.AdvanceByElapsedTime(TimeSpan.FromHours(1));
            sanity.ApplyDisturbingEvent(90f);

            Assert.AreEqual(SurvivalThresholdLevel.Critical, hunger.CurrentLevel);
            Assert.AreEqual(SurvivalThresholdLevel.Critical, sanity.CurrentLevel);

            // Simulate ActionResolver chaining both modifiers in sequence:
            // 10 -> 5 (hunger, x0.5) -> 2 or 3 (sanity, x0.5), i.e. both are applied.
            var afterHunger = hunger.ModifyOutgoingDamage(character, 10);
            var afterBoth = sanity.ModifyOutgoingDamage(character, afterHunger);

            Assert.AreEqual(5, afterHunger);
            Assert.Less(afterBoth, afterHunger);
        }
    }
}
